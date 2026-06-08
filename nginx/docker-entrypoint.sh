#!/bin/sh
set -e

DOMAIN="${DOMAIN:-localhost}"
CLIENT_PORT="${CLIENT_PORT:-5173}"

sed -i "s/__CLIENT_PORT__/${CLIENT_PORT}/g" /etc/nginx/conf.d/default.conf
sed -i "s/__DOMAIN__/${DOMAIN}/g" /etc/nginx/conf.d/default.conf

CERT_DIR="/etc/letsencrypt/live/${DOMAIN}"
mkdir -p "$CERT_DIR"

if [ ! -f "$CERT_DIR/fullchain.pem" ]; then
    echo "Generating self-signed certificate for ${DOMAIN}..."
    openssl req -x509 -nodes -newkey rsa:2048 -days 365 \
        -keyout "$CERT_DIR/privkey.pem" \
        -out "$CERT_DIR/fullchain.pem" \
        -subj "/CN=${DOMAIN}" 2>/dev/null
    cp "$CERT_DIR/fullchain.pem" "$CERT_DIR/chain.pem"
    echo "Self-signed certificate generated."
fi

exec nginx -g "daemon off;"
