#!/bin/bash

# 🚀 AR赛车展览版部署脚本
echo "🏎️ ScriptEcho AR赛车 - 展览版部署"
echo "================================"

# 配置变量
SERVER_IP="119.28.50.43"
DOMAIN="scriptecho.games"
DEPLOY_PATH="/var/www/html/ar-racing"
LOCAL_FILE="exhibition-ar-racing.html"

echo ""
echo "📋 部署信息："
echo "   🌐 服务器: $SERVER_IP"
echo "   🔗 域名: $DOMAIN" 
echo "   📁 部署路径: $DEPLOY_PATH"
echo "   📄 本地文件: $LOCAL_FILE"
echo ""

# 检查本地文件是否存在
if [ ! -f "$LOCAL_FILE" ]; then
    echo "❌ 错误：找不到本地文件 $LOCAL_FILE"
    exit 1
fi

echo "✅ 本地文件检查通过"

# 获取服务器用户名
read -p "请输入服务器用户名: " USERNAME

if [ -z "$USERNAME" ]; then
    echo "❌ 错误：用户名不能为空"
    exit 1
fi

echo ""
echo "🚀 开始部署..."

# 1. 创建远程目录
echo "📁 创建远程目录..."
ssh "$USERNAME@$SERVER_IP" "sudo mkdir -p $DEPLOY_PATH && sudo chown -R $USERNAME:$USERNAME $DEPLOY_PATH"

if [ $? -ne 0 ]; then
    echo "❌ 创建远程目录失败"
    exit 1
fi

# 2. 上传文件
echo "📤 上传游戏文件..."
scp "$LOCAL_FILE" "$USERNAME@$SERVER_IP:$DEPLOY_PATH/index.html"

if [ $? -ne 0 ]; then
    echo "❌ 文件上传失败"
    exit 1
fi

# 3. 设置权限
echo "🔒 设置文件权限..."
ssh "$USERNAME@$SERVER_IP" "sudo chown -R www-data:www-data $DEPLOY_PATH && sudo chmod -R 644 $DEPLOY_PATH/*"

# 4. 配置Nginx（如果需要）
echo ""
read -p "是否需要配置Nginx虚拟主机？(y/n): " SETUP_NGINX

if [ "$SETUP_NGINX" = "y" ] || [ "$SETUP_NGINX" = "Y" ]; then
    echo "⚙️ 配置Nginx..."
    
    # 创建Nginx配置
    cat > /tmp/ar-racing.conf << EOF
server {
    listen 80;
    server_name $DOMAIN www.$DOMAIN;
    root $DEPLOY_PATH;
    index index.html;
    
    # 基本配置
    location / {
        try_files \$uri \$uri/ /index.html;
        
        # 添加安全头
        add_header X-Frame-Options "SAMEORIGIN" always;
        add_header X-Content-Type-Options "nosniff" always;
        add_header X-XSS-Protection "1; mode=block" always;
        
        # 移动端优化
        add_header Cache-Control "no-cache, no-store, must-revalidate";
        add_header Pragma "no-cache";
        add_header Expires "0";
    }
    
    # 静态资源缓存
    location ~* \.(js|css|png|jpg|jpeg|gif|ico|svg|woff|woff2|ttf|eot)$ {
        expires 1h;
        add_header Cache-Control "public, immutable";
    }
    
    # 压缩
    gzip on;
    gzip_vary on;
    gzip_min_length 10240;
    gzip_proxied expired no-cache no-store private must-revalidate;
    gzip_types
        text/plain
        text/css
        text/xml
        text/javascript
        application/x-javascript
        application/xml+rss
        application/javascript;
    
    # 错误页面
    error_page 404 /index.html;
    
    # 日志
    access_log /var/log/nginx/ar-racing.access.log;
    error_log /var/log/nginx/ar-racing.error.log;
}
EOF
    
    # 上传配置文件
    scp /tmp/ar-racing.conf "$USERNAME@$SERVER_IP:/tmp/"
    
    # 安装配置并重启Nginx
    ssh "$USERNAME@$SERVER_IP" "
        sudo mv /tmp/ar-racing.conf /etc/nginx/sites-available/ar-racing &&
        sudo ln -sf /etc/nginx/sites-available/ar-racing /etc/nginx/sites-enabled/ &&
        sudo nginx -t && 
        sudo systemctl reload nginx
    "
    
    if [ $? -eq 0 ]; then
        echo "✅ Nginx配置成功"
    else
        echo "⚠️ Nginx配置可能有问题，请手动检查"
    fi
    
    # 清理临时文件
    rm /tmp/ar-racing.conf
fi

echo ""
echo "🎉 部署完成！"
echo ""
echo "📱 访问地址："
echo "   🌐 http://$SERVER_IP"
if [ "$SETUP_NGINX" = "y" ] || [ "$SETUP_NGINX" = "Y" ]; then
echo "   🔗 http://$DOMAIN"
fi
echo ""
echo "🎮 游戏特性："
echo "   ✅ 3D赛车体验"
echo "   ✅ 触摸屏控制"
echo "   ✅ 展览模式（5分钟自动重置）"
echo "   ✅ 移动端优化"
echo "   ✅ 实时统计显示"
echo ""
echo "🎪 展览准备就绪！"

# 可选：打开浏览器测试
read -p "是否在本地浏览器中打开测试？(y/n): " OPEN_BROWSER

if [ "$OPEN_BROWSER" = "y" ] || [ "$OPEN_BROWSER" = "Y" ]; then
    if command -v open >/dev/null; then
        open "http://$SERVER_IP"
    elif command -v xdg-open >/dev/null; then
        xdg-open "http://$SERVER_IP"
    else
        echo "请手动在浏览器中访问: http://$SERVER_IP"
    fi
fi 