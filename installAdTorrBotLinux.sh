#!/bin/bash
# ./install_adTorrBot.sh
clear

# Проверяем, запущен ли TorrServer
if ! systemctl is-active --quiet torrserver.service; then
    echo "❌ TorrServer не запущен! Установите и запустите TorrServer перед выполнением скрипта."
    exit 1
fi
echo "✅ TorrServer работает."

# Проверяем, существует ли пользователь adtorrbot
if ! id "adtorrbot" &>/dev/null; then
    echo "🔍 Создаем пользователя 'adtorrbot'..."
    sudo useradd -r -s /bin/false adtorrbot
    echo "✅ Пользователь 'adtorrbot' создан."
else
    echo "✅ Пользователь 'adtorrbot' уже существует."
fi

# Даем `adtorrbot` права на systemctl без пароля
echo "🔍 Настраиваем sudo для управления TorrServer..."

if ! sudo grep -q "adtorrbot ALL=(ALL) NOPASSWD: /usr/bin/systemctl start torrserver.service" /etc/sudoers; then
    sudo bash -c 'echo "adtorrbot ALL=(ALL) NOPASSWD: /usr/bin/systemctl start torrserver.service" >> /etc/sudoers'
fi

if ! sudo grep -q "adtorrbot ALL=(ALL) NOPASSWD: /usr/bin/systemctl stop torrserver.service" /etc/sudoers; then
    sudo bash -c 'echo "adtorrbot ALL=(ALL) NOPASSWD: /usr/bin/systemctl stop torrserver.service" >> /etc/sudoers'
fi

if ! sudo grep -q "adtorrbot ALL=(ALL) NOPASSWD: /sbin/reboot" /etc/sudoers; then
    sudo bash -c 'echo "adtorrbot ALL=(ALL) NOPASSWD: /sbin/reboot" >> /etc/sudoers'
fi

if ! sudo grep -q "adtorrbot ALL=(ALL) NOPASSWD: /bin/echo" /etc/sudoers; then
    sudo bash -c 'echo "adtorrbot ALL=(ALL) NOPASSWD: /bin/echo" >> /etc/sudoers'
fi

if ! sudo grep -q "adtorrbot ALL=(ALL) NOPASSWD: /usr/bin/tee" /etc/sudoers; then
    sudo bash -c 'echo "adtorrbot ALL=(ALL) NOPASSWD: /usr/bin/tee" >> /etc/sudoers'
fi

if ! sudo grep -q "adtorrbot ALL=(ALL) NOPASSWD: /sbin/sysctl" /etc/sudoers; then
    sudo bash -c 'echo "adtorrbot ALL=(ALL) NOPASSWD: /sbin/sysctl" >> /etc/sudoers'
fi


echo "✅ `adtorrbot` теперь может управлять `torrserver.service` и перезапускать VPS!"

# Настраиваем polkit для adtorrbot
echo "🔍 Настраиваем polkit для управления TorrServer..."
sudo tee /etc/polkit-1/localauthority/50-local.d/adtorrbot.pkla > /dev/null <<EOF
[Allow AdTorrBot Full Systemctl Control]
Identity=unix-user:adtorrbot
Action=org.freedesktop.systemd1.manage-units
ResultActive=yes
ResultInactive=yes
ResultAny=yes
EOF

echo "✅ `polkit` теперь позволяет `adtorrbot` управлять `systemctl` без аутентификации!"

# Даем `adtorrbot` права на изменение /etc/sysctl.conf
echo "🔍 Даем `adtorrbot` права на изменение /etc/sysctl.conf..."
sudo chown adtorrbot:root /etc/sysctl.conf
sudo chmod 644 /etc/sysctl.conf
echo "✅ Права на /etc/sysctl.conf обновлены!"

# Перезапускаем polkit
sudo systemctl restart polkit

# **Обновляем права на TorrServer (771), чтобы он запускался корректно**
echo "🔍 Устанавливаем правильные права на TorrServer..."
sudo chmod -R 771 /opt/torrserver/
sudo chown -R adtorrbot:torrserver /opt/torrserver/
echo "✅ Права на TorrServer обновлены!"

# **Перезапускаем TorrServer для применения прав**
echo "🔍 Перезапускаем TorrServer..."
sudo systemctl restart torrserver.service
echo "✅ TorrServer запущен!"

# Устанавливаем .NET 8.0, если его нет
DOTNET_VERSION=$(dotnet --version 2>/dev/null)
if [[ -z "$DOTNET_VERSION" || "$DOTNET_VERSION" != 8.* ]]; then
    echo "🔍 .NET 8 не найден. Устанавливаем .NET 8..."
    sudo apt install -y dotnet-sdk-8.0 || {
        echo "❌ Ошибка установки .NET 8."
        exit 1
    }
    echo "✅ .NET 8 установлен."
else
    echo "✅ .NET 8 уже установлен. Версия: $DOTNET_VERSION"
fi

# Устанавливаем wget, unrar и jq
for package in wget unrar jq; do
    if ! command -v $package &>/dev/null; then
        echo "🔍 Устанавливаем $package..."
        sudo apt install -y $package
    fi
done

# Скачиваем и распаковываем бота
BOT_RELEASE_URL="https://github.com/IGNATOV93/AdTorrBot/releases/download/v1.0/AdTorrBot-v1.0-Linux64.rar"
BOT_DIR="/opt/AdTorrBot"
BOT_ARCHIVE="$BOT_DIR/AdTorrBot-v1.0-Linux64.rar"

mkdir -p "$BOT_DIR" || { echo "❌ Ошибка создания папки $BOT_DIR"; exit 1; }
wget -q -O "$BOT_ARCHIVE" "$BOT_RELEASE_URL" || { echo "❌ Ошибка скачивания архива."; exit 1; }
unrar x "$BOT_ARCHIVE" "/opt/" || { echo "❌ Ошибка распаковки архива"; exit 1; }
rm "$BOT_ARCHIVE"

echo "✅ Бот загружен!"

# Настроить права на /opt/AdTorrBot/
sudo chown -R adtorrbot:adtorrbot "$BOT_DIR"
sudo chmod -R 750 "$BOT_DIR"

# Запрашиваем Telegram API Token и Chat ID
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
echo "🔍 Сохраняем настройки в settings.json..."
sudo tee /opt/AdTorrBot/settings.json > /dev/null <<EOF
{
    "YourBotTelegramToken": "$TELEGRAM_TOKEN",
    "AdminChatId": "$TELEGRAM_CHAT_ID",
    "FilePathTorrserver": "/opt/torrserver/"
}
EOF
echo "✅ Настройки сохранены!"
sudo chown adtorrbot:adtorrbot /opt/AdTorrBot/settings.json
sudo chmod 644 /opt/AdTorrBot/settings.json
sudo chown adtorrbot:adtorrbot /opt/AdTorrBot/*.db
sudo chmod 644 /opt/AdTorrBot/*.db


# Создаем systemd-сервис для бота
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

# Перезапускаем и включаем сервис
sudo systemctl daemon-reload
sudo systemctl enable adtorrbot.service
sudo systemctl start adtorrbot.service

echo "✅ Бот удачно запустился как служба! Проверяем статус..."
sudo systemctl status adtorrbot.service --no-pager
