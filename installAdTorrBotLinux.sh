#!/bin/bash
clear

echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo " 🔷 Добро пожаловать в установочный скрипт AdTorrBot!"
echo " Этот бот предназначен для управления TorrServer."
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Функция выбора действия
choose_action() {
    echo "Что вы хотите сделать?"
    echo "1 Установить AdTorrBot"
    echo "2 Переустановить AdTorrBot(удалить + установить)"
    echo "3 Удалить AdTorrBot"
    echo "4 Обновить AdTorrBot"
    echo "5 Проверить окружение (AdTorrBot/TorrServer)"
    echo "0 Выйти"
    echo ""

    read -p "Введите номер действия: " action
    case "$action" in
        1) install_bot ;;
        2) reinstall_bot ;;
        3) uninstall_bot ;;
        4) update_bot ;;
        5) check_environment ;;
        0) echo "👋 Выход..."; exit 0 ;;
        *) echo "❌ Неверный ввод! Попробуйте снова."; choose_action ;;
    esac
}

# Проверка TorrServer
check_torrserver() {
    if ! systemctl is-active --quiet torrserver.service; then
        echo "❌ TorrServer не запущен! Установите и запустите TorrServer перед выполнением скрипта."
        exit 1
    fi
    echo "✅ TorrServer работает."
}

create_user() {
    if ! id "adtorrbot" &>/dev/null; then
        echo "🔍 Создаем пользователя 'adtorrbot'..."
        sudo useradd -r -s /bin/bash adtorrbot
        echo "✅ Пользователь 'adtorrbot' создан."
    else
        echo "✅ Пользователь 'adtorrbot' уже существует."
    fi

    # ⏩ Сразу настраиваем совместный доступ
    configure_shared_access
}


# Настройка sudo привилегий
configure_sudo() {
    local cmds=(
        "/usr/bin/systemctl start torrserver.service"
        "/usr/bin/systemctl stop torrserver.service"
        "/sbin/reboot"
        "/bin/echo"
        "/usr/bin/tee"
        "/sbin/sysctl"
    )

    echo "🔍 Настраиваем sudo для adtorrbot..."
    for cmd in "${cmds[@]}"; do
        if ! sudo grep -q "adtorrbot ALL=(ALL) NOPASSWD: $cmd" /etc/sudoers; then
            echo "adtorrbot ALL=(ALL) NOPASSWD: $cmd" | sudo tee -a /etc/sudoers > /dev/null
        fi
    done
    echo "✅ Настройки sudo обновлены!"
}
configure_shared_access() {
    echo "🔧 Настраиваем совместный доступ к /opt/torrserver..."

    # Создаем группу, если её нет
    if ! getent group torrgroup >/dev/null; then
        sudo groupadd torrgroup
        echo "✅ Группа 'torrgroup' создана."
    else
        echo "✅ Группа 'torrgroup' уже существует."
    fi

    # Добавляем torrserver и adtorrbot в общую группу
    sudo usermod -aG torrgroup torrserver
    sudo usermod -aG torrgroup adtorrbot

    # Назначаем группу владельцем папки и выставляем права
    if [ -d /opt/torrserver ]; then
        sudo chown -R torrserver:torrgroup /opt/torrserver
        sudo chmod -R 775 /opt/torrserver
        echo "✅ Права на /opt/torrserver обновлены для группы."
    else
        echo "⚠️ Папка /opt/torrserver не найдена. Совместный доступ будет настроен после установки TorrServer."
    fi
}

configure_polkit() {
    echo "🔍 Определяем версию Ubuntu..."

    UBUNTU_VERSION=$(lsb_release -rs | cut -d '.' -f1)

    if [ "$UBUNTU_VERSION" -ge 24 ]; then
        echo "🛡️ Ubuntu $UBUNTU_VERSION обнаружена — используем .rules (JS)..."

        RULE_FILE="/etc/polkit-1/rules.d/99-torrserver-control.rules"

        if [ -f "$RULE_FILE" ]; then
            echo "✅ Правило уже существует: $RULE_FILE — пропускаем."
        else
            sudo tee "$RULE_FILE" > /dev/null <<EOF
polkit.addRule(function(action, subject) {
    if (action.id == "org.freedesktop.systemd1.manage-units" &&
        action.lookup("unit") == "torrserver.service" &&
        subject.user == "adtorrbot") {
        return polkit.Result.YES;
    }
});
EOF
            sudo systemctl restart polkit
            echo "✅ Polkit настроен для Ubuntu 24+ (.rules)"
        fi

    else
        echo "🛡️ Ubuntu $UBUNTU_VERSION обнаружена — используем .pkla..."

        PKLA_FILE="/etc/polkit-1/localauthority/50-local.d/adtorrbot.pkla"

        if [ -f "$PKLA_FILE" ]; then
            echo "✅ Правило уже существует: $PKLA_FILE — пропускаем."
        else
            sudo tee "$PKLA_FILE" > /dev/null <<EOF
[Allow AdTorrBot to control torrserver]
Identity=unix-user:adtorrbot
Action=org.freedesktop.systemd1.manage-units
ResultActive=yes
ResultInactive=yes
ResultAny=yes
EOF
            echo "✅ Polkit настроен для Ubuntu 22 (.pkla)"
        fi
    fi
}


install_dependencies() {
    echo "🔍 Проверяем и устанавливаем необходимые пакеты..."

    sudo apt update >/dev/null 2>&1

    for package in wget unrar jq; do
        if ! dpkg -l | grep -q "$package"; then
            echo "➕ Устанавливаем $package..."
            sudo apt install -y $package >/dev/null 2>&1
        else
            echo "✅ $package уже установлен."
        fi
    done

    echo "📦 Устанавливаем .NET SDK 8.0 и Runtime..."

    # Скачиваем и устанавливаем SDK и Runtime в /opt/dotnet
    wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh >/dev/null 2>&1
    chmod +x dotnet-install.sh
    sudo mkdir -p /opt/dotnet
    ./dotnet-install.sh --version 8.0.100 --install-dir /opt/dotnet >/dev/null 2>&1
    ./dotnet-install.sh --runtime dotnet --version 8.0.15 --install-dir /opt/dotnet >/dev/null 2>&1

    # Создаём ссылку для глобального доступа
    sudo ln -sf /opt/dotnet/dotnet /usr/bin/dotnet

    echo "⚙️ Прописываем окружение для пользователя adtorrbot..."
    if [ -d /home/adtorrbot ]; then
        sudo tee /home/adtorrbot/.profile >/dev/null <<EOF
export DOTNET_ROOT=/opt/dotnet
export PATH=\$PATH:/opt/dotnet
EOF
        sudo chown adtorrbot:adtorrbot /home/adtorrbot/.profile
        echo "✅ Окружение .NET для adtorrbot настроено."
    else
        echo "⚠️ Папка /home/adtorrbot не найдена. Переменные окружения не добавлены."
    fi

    echo "🔍 Проверяем наличие dotnet и его версию..."

    if command -v dotnet >/dev/null 2>&1; then
        DOTNET_VERSION=$(dotnet --version)
        DOTNET_MAJOR=$(echo "$DOTNET_VERSION" | cut -d '.' -f1)

        if [ "$DOTNET_MAJOR" -ge 8 ]; then
            echo "🎉 .NET SDK и Runtime установлены: версия $DOTNET_VERSION"
        else
            echo "❌ Обнаружена устаревшая версия .NET SDK ($DOTNET_VERSION). Требуется 8.0 или выше."
            exit 1
        fi
    else
        echo "❌ dotnet не установлен или не найден в PATH."
        exit 1
    fi

    echo "✅ Установка зависимостей завершена!"
}


# Функция запроса Telegram API Token и Chat ID
request_telegram_credentials() {
    echo "🔍 Введите Telegram API Token:"
    read -r TELEGRAM_TOKEN
    echo "🔍 Введите Telegram Chat ID:"
    read -r TELEGRAM_CHAT_ID

    # Проверяем валидность Telegram API Token
    API_URL="https://api.telegram.org/bot$TELEGRAM_TOKEN/getChat?chat_id=$TELEGRAM_CHAT_ID"
    RESPONSE=$(curl -s "$API_URL")

    if echo "$RESPONSE" | grep -q '"ok":true'; then
        echo "✅ Токен и Chat ID валидны!"
    else
        echo "❌ Ошибка! Токен или Chat ID неверны!"
        exit 1
    fi

    # Создаем папку, если ее нет
    sudo mkdir -p /opt/AdTorrBot

    echo "🔍 Сохраняем настройки в settings.json..."
    sudo tee /opt/AdTorrBot/settings.json > /dev/null <<EOF
{
    "YourBotTelegramToken": "$TELEGRAM_TOKEN",
    "AdminChatId": "$TELEGRAM_CHAT_ID",
    "FilePathTorrserver": "/opt/torrserver/"
}
EOF

    sudo chown adtorrbot:adtorrbot /opt/AdTorrBot/settings.json
    sudo chmod 644 /opt/AdTorrBot/settings.json
    echo "✅ Настройки сохранены!"
}


# Скачивание и установка бота
download_bot() {
    BOT_DIR="/opt/AdTorrBot"
    BOT_ARCHIVE="/tmp/AdTorrBot-v1.0-Linux64.rar"
    LATEST_VERSION=$(curl -sL https://api.github.com/repos/IGNATOV93/AdTorrBot/releases/latest | jq -r '.tag_name')
    BOT_ARCHIVE="/tmp/AdTorrBot-${LATEST_VERSION}-Linux64.rar"
    BOT_RELEASE_URL="https://github.com/IGNATOV93/AdTorrBot/releases/download/$LATEST_VERSION/AdTorrBot-${LATEST_VERSION}-Linux64.rar"


    sudo mkdir -p "$BOT_DIR"

    echo "🚀 Скачивание AdTorrBot..."
    wget -q --show-progress -O "$BOT_ARCHIVE" "$BOT_RELEASE_URL" || { echo "❌ Ошибка скачивания."; exit 1; }

    # Распаковываем архив без лишней вложенности
    if unrar e -o- "$BOT_ARCHIVE" "$BOT_DIR" > /dev/null; then
        rm "$BOT_ARCHIVE"

        # ✅ Назначаем права после распаковки
        sudo chown -R adtorrbot:adtorrbot "$BOT_DIR"
        sudo chmod -R 750 "$BOT_DIR"

        # ✅ Устанавливаем правильные права на settings.json и базу данных
        sudo chown adtorrbot:adtorrbot "$BOT_DIR/settings.json"
        sudo chmod 644 "$BOT_DIR/settings.json"
  
        # ✅ Даем права на исполнение боту
        sudo chmod +x "$BOT_DIR/AdTorrBot"

        echo "✅ Бот установлен в $BOT_DIR и готов к запуску!"
    else
        echo "❌ Архив поврежден!"
        exit 1
    fi
}
# Обновление бота
update_bot() {
    echo "🔍 Проверяем наличие новой версии AdTorrBot..."

    LATEST_VERSION=$(curl -sL https://api.github.com/repos/IGNATOV93/AdTorrBot/releases/latest | jq -r '.tag_name')
    LOCAL_VERSION=$(cat /opt/AdTorrBot/version.txt 2>/dev/null || echo "unknown")

    if [[ -z "$LATEST_VERSION" || "$LATEST_VERSION" == "null" ]]; then
        echo "❌ Ошибка: невозможно получить версию AdTorrBot с GitHub!"
        exit 1
    fi

    if [[ "$LOCAL_VERSION" == "$LATEST_VERSION" ]]; then
        echo "✅ У вас уже установлена последняя версия ($LOCAL_VERSION)."
        exit 0
    fi

    echo "🔄 Доступна новая версия: $LATEST_VERSION (у вас $LOCAL_VERSION)"
    read -p "Обновить AdTorrBot до $LATEST_VERSION? [Y/n] " response
    if [[ "$response" != "Y" && "$response" != "y" && -n "$response" ]]; then
        echo "❌ Обновление отменено."
        exit 0
    fi

    echo "🔄 Начинаем обновление..."

    BOT_DIR="/opt/AdTorrBot"
    TEMP_DIR="/tmp/AdTorrBot_Update"
    EXTRACT_DIR="$TEMP_DIR/extracted"
    BOT_ARCHIVE="$TEMP_DIR/AdTorrBot-${LATEST_VERSION}-Linux64.rar"
    BOT_DOWNLOAD_URL="https://github.com/IGNATOV93/AdTorrBot/releases/download/$LATEST_VERSION/AdTorrBot-${LATEST_VERSION}-Linux64.rar"

    sudo systemctl stop adtorrbot.service
    sudo systemctl disable adtorrbot.service
    sudo pkill -f "$BOT_DIR/AdTorrBot"

    echo "🚀 Скачивание последней версии..."
    mkdir -p "$TEMP_DIR"
    wget -q -O "$BOT_ARCHIVE" "$BOT_DOWNLOAD_URL" || { echo "❌ Ошибка скачивания."; exit 1; }

    if [[ ! -f "$BOT_ARCHIVE" ]]; then
        echo "❌ Ошибка: файл обновления отсутствует!"
        exit 1
    fi

    if ! command -v unrar &> /dev/null; then
        echo "📦 Устанавливаем unrar..."
        sudo apt update && sudo apt install unrar -y
    fi

    echo "📂 Распаковка архива во временную папку..."
    mkdir -p "$EXTRACT_DIR"
    if unrar x -o+ "$BOT_ARCHIVE" "$EXTRACT_DIR/" > /dev/null; then
        rm -f "$BOT_ARCHIVE"

        if [[ -d "$EXTRACT_DIR/AdTorrBot" ]]; then
            [[ -d "$EXTRACT_DIR/AdTorrBot/AdTorrBot" ]] && rm -rf "$EXTRACT_DIR/AdTorrBot/AdTorrBot"
            find "$EXTRACT_DIR/AdTorrBot" -maxdepth 1 -type f ! -name "settings.json" -exec cp -t "$TEMP_DIR/" {} +
            rm -rf "$EXTRACT_DIR/AdTorrBot"
        fi

        if [[ -d "$BOT_DIR/AdTorrBot" ]]; then
            echo "🧨 Удаляем '$BOT_DIR/AdTorrBot' — остаток старой установки"
            sudo rm -rf "$BOT_DIR/AdTorrBot"
        fi

        echo "🔄 Обновляем файлы без удаления settings.json..."
        rsync -a --exclude="settings.json" "$TEMP_DIR/" "$BOT_DIR/"

        echo "$LATEST_VERSION" | sudo tee "$BOT_DIR/version.txt" > /dev/null

        echo "⚙️ Настраиваем права доступа..."
        sudo chown -R adtorrbot:adtorrbot "$BOT_DIR"
        sudo chmod -R 750 "$BOT_DIR"
        [[ -f "$BOT_DIR/settings.json" ]] && sudo chmod 644 "$BOT_DIR/settings.json"
        [[ -f "$BOT_DIR/app.db" ]] && sudo chmod 644 "$BOT_DIR/app.db"
        [[ -f "$BOT_DIR/AdTorrBot" ]] && sudo chmod +x "$BOT_DIR/AdTorrBot"

        rm -rf "$TEMP_DIR"
        sudo systemctl enable adtorrbot.service
        sudo systemctl start adtorrbot.service
        echo "✅ Обновление до $LATEST_VERSION завершено!"
    else
        echo "❌ Ошибка распаковки архива."
        exit 1
    fi
}
setup_systemd() {
    echo "🔍 Создаем службу AdTorrBot..."
    sudo tee /etc/systemd/system/adtorrbot.service > /dev/null <<EOF
[Unit]
Description=AdTorrBot Service
After=network.target

[Service]
User=adtorrbot
WorkingDirectory=/opt/AdTorrBot/
ExecStart=/opt/AdTorrBot/AdTorrBot
StandardOutput=journal
StandardError=journal
Restart=on-failure
Type=simple
KillSignal=SIGINT

# ✅ Указываем путь к .NET для systemd
Environment=DOTNET_ROOT=/opt/dotnet
Environment=PATH=/opt/dotnet:/usr/bin:/bin

[Install]
WantedBy=multi-user.target
EOF

    sudo systemctl daemon-reexec
    sudo systemctl daemon-reload
    sudo systemctl enable adtorrbot.service
    sudo systemctl start adtorrbot.service

    echo "✅ Бот удачно запустился как служба!"
}


# Установка бота
install_bot() {
    echo "🚀 Начинаем установку AdTorrBot..."
    check_torrserver
    create_user
    configure_sudo
    configure_polkit
    install_dependencies
    request_telegram_credentials
    download_bot
    setup_systemd
    echo "✅ Установка завершена!"
}

# Функция переустановки бота
reinstall_bot() {
    echo "🔄 Переустановка AdTorrBot..."
    uninstall_bot
    install_bot
    echo "✅ Переустановка завершена!"
}

# Функция удаления бота
uninstall_bot() {
    echo "🗑 Удаляем AdTorrBot..."

    # Проверяем, существует ли служба перед отключением
    if systemctl list-units --type=service | grep -q "adtorrbot.service"; then
        sudo systemctl stop adtorrbot.service
        sudo systemctl disable adtorrbot.service
        sudo rm -f /etc/systemd/system/adtorrbot.service
        sudo systemctl daemon-reload
        sudo systemctl reset-failed
        echo "✅ Служба AdTorrBot отключена."
    else
        echo "⚠️ Служба AdTorrBot уже удалена или не существует."
    fi

    # Удаляем файлы бота
    sudo rm -rf /opt/AdTorrBot
    echo "✅ AdTorrBot успешно удален!"
}
# функция проверки окружения бота и torrserver
check_environment() {
  echo "======================================"
  echo "   Проверка окружения AdTorrBot/TorrServer"
  echo "======================================"
  echo ""

  errors=() warnings=()

  # Сервисы
  echo "[Сервисы]"
  systemctl is-active --quiet adtorrbot && echo "AdTorrBot: ✅ работает" || { echo "AdTorrBot: ❌ не запущен"; errors+=("Бот не запущен"); }
  systemctl is-active --quiet torrserver && echo "TorrServer: ✅ работает" || { echo "TorrServer: ❌ не запущен"; errors+=("TorrServer не запущен"); }
  echo ""

  # Владельцы и группы
  echo "[Владельцы и группы]"
  if [ -d /opt/torrserver ]; then
    cat_owner=$(stat -c "%U" /opt/torrserver)
    cat_group=$(stat -c "%G" /opt/torrserver)
    cat_perms=$(stat -c "%A" /opt/torrserver)
    echo "📂 /opt/torrserver | владелец=$cat_owner | группа=$cat_group | права=$cat_perms"
  else
    echo "❌ Каталог /opt/torrserver отсутствует"
    errors+=("Нет каталога /opt/torrserver")
  fi

  if [ -f /opt/torrserver/accs.db ]; then
    db_owner=$(stat -c "%U" /opt/torrserver/accs.db)
    db_group=$(stat -c "%G" /opt/torrserver/accs.db)
    db_perms=$(stat -c "%A" /opt/torrserver/accs.db)
    echo "📄 accs.db | владелец=$db_owner | группа=$db_group | права=$db_perms"
  else
    echo "⚠️ Файл accs.db отсутствует (создастся при первом запуске TorrServer)"
    warnings+=("accs.db отсутствует (создастся при первом запуске)")
  fi
  echo ""

  # Пользователи
  echo "[Пользователи]"
  ad_groups=$(id -nG adtorrbot 2>/dev/null)
  ts_groups=$(id -nG torrserver 2>/dev/null)
  echo "👤 adtorrbot | группы: ${ad_groups:-нет пользователя/групп}"
  echo "👤 torrserver | группы: ${ts_groups:-нет пользователя/групп}"
  echo ""

  # Итог по правам для работы бота
  echo "[Права для работы бота]"
  rights_ok=true

  if [ -d /opt/torrserver ]; then
    sudo -u adtorrbot test -w /opt/torrserver || { rights_ok=false; errors+=("adtorrbot не может писать в /opt/torrserver"); }
  fi

  if [ -f /opt/torrserver/accs.db ]; then
    sudo -u adtorrbot test -w /opt/torrserver/accs.db || { rights_ok=false; errors+=("adtorrbot не может изменять accs.db"); }
  else
    [ -d /opt/torrserver ] && sudo -u adtorrbot test -w /opt/torrserver || { rights_ok=false; errors+=("adtorrbot не может создать accs.db в /opt/torrserver"); }
  fi

  if $rights_ok; then
    echo "✅ Права настроены корректно."
  else
    echo "❌ Права настроены некорректно — у бота нет достаточных прав."
  fi
  echo ""

  # Сеть: TCP congestion control
  echo "[Сеть: TCP congestion control]"
  algo=$(sysctl -n net.ipv4.tcp_congestion_control 2>/dev/null)
  if [ -n "$algo" ]; then
    echo "📡 Алгоритм: $algo"
    if [ "$algo" = "bbr" ]; then
      echo "✅ BBR включён"
    else
      echo "⚠️ Используется $algo (не критично; для лучшей отдачи можно включить BBR)"
      warnings+=("Используется $algo — не критично, но BBR обычно быстрее")
    fi
  else
    echo "📡 Алгоритм: не удалось определить"
    warnings+=("Не удалось определить алгоритм TCP congestion control")
  fi
  echo ""

  # Доступ к Telegram API (c таймаутами)
  echo "[Доступ к Telegram API]"
  code4=$(curl -4 --max-time 5 -s -o /dev/null -w "%{http_code}" https://api.telegram.org || echo "000")
  if [ "$code4" = "200" ] || [ "$code4" = "302" ]; then
    echo "IPv4: ✅ доступен"
  else
    echo "IPv4: ❌ недоступен"
    errors+=("Нет доступа к Telegram по IPv4")
  fi

  if ip -6 addr show | grep -q "inet6"; then
    echo "🌐 IPv6 включён на сервере"
    code6=$(curl -6 --max-time 5 -s -o /dev/null -w "%{http_code}" https://api.telegram.org || echo "000")
    if [ "$code6" = "200" ] || [ "$code6" = "302" ]; then
      echo "IPv6: ✅ доступен"
    else
      echo "IPv6: ⚠️ включён, но Telegram недоступен"
      warnings+=("IPv6 включён, но Telegram недоступен — бот может падать; принудите IPv4 или отключите IPv6")
    fi
  else
    echo "IPv6: ℹ️ не включён"
  fi
  echo ""

  # AppArmor/SELinux
  if command -v aa-status >/dev/null; then
    echo "[AppArmor]"
    if aa-status | grep -q "profiles are in enforce mode"; then
      echo "⚠️ AppArmor активен"
      warnings+=("AppArmor активен — может ограничивать доступ к /opt")
    else
      echo "✅ AppArmor не активен"
    fi
    echo ""
  fi

  if command -v getenforce >/dev/null; then
    echo "[SELinux]"
    sel=$(getenforce)
    echo "SELinux: $sel"
    [ "$sel" = "Enforcing" ] && warnings+=("SELinux Enforcing — возможны ограничения доступа")
    echo ""
  fi

  # TorrServer процесс
  echo "[Процесс TorrServer]"
  systemctl is-active --quiet torrserver && echo "✅ TorrServer запущен через systemd" || { echo "❌ TorrServer не найден"; errors+=("Сервис torrserver не активен"); }
  echo ""

  # Сводка причин
  echo "[Сводка]"
  if [ ${#errors[@]} -eq 0 ]; then
    echo "✅ Критических ошибок не найдено"
  else
    echo "❌ Критические проблемы:"
    for e in "${errors[@]}"; do echo "   • $e"; done
  fi

  if [ ${#warnings[@]} -gt 0 ]; then
    echo "⚠️ Предупреждения:"
    for w in "${warnings[@]}"; do echo "   • $w"; done
  fi

  echo ""
  echo "======================================"
  echo "   Проверка завершена ✅"
  echo "======================================"
}

# Запуск меню выбора действия
choose_action
