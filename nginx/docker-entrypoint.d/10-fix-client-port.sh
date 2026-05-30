#!/bin/sh
sed -i "s/__CLIENT_PORT__/${CLIENT_PORT:-5173}/g" /etc/nginx/conf.d/default.conf
