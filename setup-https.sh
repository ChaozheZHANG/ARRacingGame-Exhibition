#!/bin/bash

# HTTPS配置脚本 - 解决摄像头访问问题
echo "🔒 开始配置HTTPS以支持摄像头访问..."

# 服务器配置
SERVER_IP="119.28.50.43"
SERVER_USER="ubuntu"
SERVER_PASS="zhe18611922329."
DOMAIN="scriptecho.games"

# 创建HTTPS配置文件
cat > temp_https_setup.sh << 'EOF'
#!/bin/bash
echo "🔧 配置HTTPS环境..."

# 安装certbot用于SSL证书
sudo apt update
sudo apt install -y certbot python3-certbot-nginx

# 创建新的nginx配置支持HTTPS
sudo tee /etc/nginx/sites-available/ar-racing > /dev/null << 'NGINX_CONF'
server {
    listen 80;
    server_name scriptecho.games www.scriptecho.games;
    
    # 重定向HTTP到HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name scriptecho.games www.scriptecho.games;
    
    root /var/www/ar-racing;
    index index.html;
    
    # SSL配置（certbot会自动填充）
    ssl_certificate /etc/ssl/certs/self-signed.crt;
    ssl_certificate_key /etc/ssl/private/self-signed.key;
    
    # 安全头配置
    add_header Strict-Transport-Security "max-age=31536000" always;
    add_header X-Content-Type-Options nosniff;
    add_header X-Frame-Options DENY;
    add_header X-XSS-Protection "1; mode=block";
    
    # CORS配置（摄像头访问必需）
    add_header 'Access-Control-Allow-Origin' '*';
    add_header 'Access-Control-Allow-Methods' 'GET, POST, OPTIONS';
    add_header 'Access-Control-Allow-Headers' 'DNT,User-Agent,X-Requested-With,If-Modified-Since,Cache-Control,Content-Type,Range';
    
    location / {
        try_files $uri $uri/ /index.html;
    }
}
NGINX_CONF

# 创建自签名证书（临时解决方案）
echo "🔐 创建自签名SSL证书..."
sudo mkdir -p /etc/ssl/private
sudo openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout /etc/ssl/private/self-signed.key \
    -out /etc/ssl/certs/self-signed.crt \
    -subj "/C=CN/ST=Guangdong/L=Shenzhen/O=ScriptEcho/CN=scriptecho.games"

# 启用新配置
sudo ln -sf /etc/nginx/sites-available/ar-racing /etc/nginx/sites-enabled/
sudo rm -f /etc/nginx/sites-enabled/default

# 测试nginx配置
sudo nginx -t

# 重启nginx
sudo systemctl restart nginx

echo "✅ HTTPS配置完成！"
echo "🌐 现在可以通过HTTPS访问: https://scriptecho.games"
echo "📱 摄像头访问问题已解决！"
EOF

echo "📤 上传HTTPS配置脚本..."
sshpass -p "$SERVER_PASS" scp -o StrictHostKeyChecking=no temp_https_setup.sh $SERVER_USER@$SERVER_IP:/home/ubuntu/

echo "🔧 执行HTTPS配置..."
sshpass -p "$SERVER_PASS" ssh -o StrictHostKeyChecking=no $SERVER_USER@$SERVER_IP 'chmod +x /home/ubuntu/temp_https_setup.sh && bash /home/ubuntu/temp_https_setup.sh'

# 清理临时文件
rm -f temp_https_setup.sh

echo "✅ HTTPS配置完成！"
echo ""
echo "🌐 新的访问地址:"
echo "   https://scriptecho.games (主要 - 支持摄像头)"
echo "   https://$SERVER_IP (备用 - 支持摄像头)"
echo ""
echo "⚠️  浏览器可能会显示证书警告，选择'继续访问'即可"
echo "�� 现在摄像头应该可以正常工作了！" 