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
    echo "0 Выйти"
    echo ""

    read -p "Введите номер действия: " action
    case "$action" in
        1) install_bot ;;
        2) reinstall_bot ;;
        3) uninstall_bot ;;
        4) update_bot ;;
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

# Создание пользователя
create_user() {
    if ! id "adtorrbot" &>/dev/null; then
        echo "🔍 Создаем пользователя 'adtorrbot'..."
        sudo useradd -r -s /bin/false adtorrbot
        echo "✅ Пользователь 'adtorrbot' создан."
    else
        echo "✅ Пользователь 'adtorrbot' уже существует."
    fi
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

# Настройка polkit
configure_polkit() {
    echo "🔍 Настраиваем polkit..."
    sudo tee /etc/polkit-1/localauthority/50-local.d/adtorrbot.pkla > /dev/null <<EOF
[Allow AdTorrBot Full Systemctl Control]
Identity=unix-user:adtorrbot
Action=org.freedesktop.systemd1.manage-units
ResultActive=yes
ResultInactive=yes
ResultAny=yes
EOF
    echo "✅ Polkit настроен!"
}

# Установка зависимостей
install_dependencies() {
    echo "🔍 Проверяем и устанавливаем необходимые пакеты..."
    for package in wget unrar jq dotnet-sdk-8.0; do
        if ! dpkg -l | grep -q "$package"; then
            echo "➕ Устанавливаем $package..."
            sudo apt install -y $package >/dev/null 2>&1
        else
            echo "✅ $package уже установлен."
        fi
    done
    echo "✅ Установка пакетов завершена!"
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

update_bot() {
    echo "🔍 Проверяем наличие новой версии AdTorrBot..."

    # Получаем последнюю версию с GitHub API
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
    BOT_ARCHIVE="/tmp/AdTorrBot-${LATEST_VERSION}-Linux64.rar"
    BOT_DOWNLOAD_URL="https://github.com/IGNATOV93/AdTorrBot/releases/download/$LATEST_VERSION/AdTorrBot-${LATEST_VERSION}-Linux64.rar"

    sudo systemctl stop adtorrbot.service

    echo "🚀 Скачивание последней версии..."
    wget -q --show-progress -O "$BOT_ARCHIVE" "$BOT_DOWNLOAD_URL" || { echo "❌ Ошибка скачивания."; exit 1; }

    if [[ ! -f "$BOT_ARCHIVE" ]]; then
        echo "❌ Ошибка: файл обновления отсутствует!"
        exit 1
    fi

    # Проверяем, установлен ли `unrar`
    if ! command -v unrar &> /dev/null; then
        echo "❌ Ошибка: пакет `unrar` не установлен! Устанавливаем..."
        sudo apt update && sudo apt install unrar -y
    fi

    # Новая команда распаковки
    echo "📂 Распаковка архива..."
    if unrar x -o+ "$BOT_ARCHIVE" "$BOT_DIR/" > /dev/null 2>&1; then
        rm "$BOT_ARCHIVE"

        # ✅ Обновляем права
        sudo chown -R adtorrbot:adtorrbot "$BOT_DIR"
        sudo chmod -R 750 "$BOT_DIR"
        sudo chmod +x "$BOT_DIR/AdTorrBot"

        echo "$LATEST_VERSION" | sudo tee /opt/AdTorrBot/version.txt > /dev/null

        sudo systemctl start adtorrbot.service
        echo "✅ Обновление завершено!"
    else
        echo "❌ Ошибка распаковки архива."
        exit 1
    fi
}




# Настройка systemd
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

[Install]
WantedBy=multi-user.target
EOF

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
# Запуск меню выбора действия
choose_action
