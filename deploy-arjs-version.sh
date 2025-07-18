#!/bin/bash

# 部署AR.js成熟库版本 - 真正解决问题
echo "🚀 部署ScriptEcho AR赛车游戏 - AR.js成熟库版本..."

# 服务器信息
SERVER="119.28.50.43"
USER="ubuntu"
PASSWORD="zhe18611922329."

echo "📁 上传AR.js版本..."

# 上传到临时目录
sshpass -p "$PASSWORD" ssh $USER@$SERVER << 'EOF'
mkdir -p /tmp/ar-games
EOF

# 上传文件
sshpass -p "$PASSWORD" scp ar-racing-with-arjs.html $USER@$SERVER:/tmp/ar-games/

echo "🔧 配置服务器..."
sshpass -p "$PASSWORD" ssh $USER@$SERVER << 'EOF'
# 备份现有版本
sudo cp /var/www/html/index.html /var/www/html/index.html.backup3

# 部署AR.js版本
sudo cp /tmp/ar-games/ar-racing-with-arjs.html /var/www/html/index.html
sudo cp /tmp/ar-games/ar-racing-with-arjs.html /var/www/html/ar-js.html

# 设置权限
sudo chown www-data:www-data /var/www/html/index.html
sudo chown www-data:www-data /var/www/html/ar-js.html
sudo chmod 644 /var/www/html/index.html
sudo chmod 644 /var/www/html/ar-js.html

# 清理
rm -rf /tmp/ar-games

# 重启nginx
sudo systemctl restart nginx

echo "✅ AR.js版本部署完成！"
echo "🌐 主页访问: https://scriptecho.games"
echo "🎮 AR.js版本: https://scriptecho.games/ar-js.html"
EOF

echo "🎯 AR.js成熟库版本部署成功！"
echo ""
echo "🔧 使用AR.js成熟库解决的问题："
echo "   ✅ 真正的地面锚定 - 使用HIRO标记锚定到现实世界"
echo "   ✅ 完整的碰撞检测 - 使用之前工作的代码"
echo "   ✅ 灵敏的控制响应 - 提高速度和响应性"
echo "   ✅ 明显的反馈 - 分数弹窗、音效、console日志"
echo ""
echo "🎮 使用方法："
echo "   1. 打印HIRO标记: https://jeromeetienne.github.io/AR.js/data/images/HIRO.jpg"
echo "   2. 或在另一个屏幕上显示HIRO标记"
echo "   3. 将相机对准标记"
echo "   4. 游戏内容会锚定到标记上"
echo "   5. 移动相机观看不同角度，内容保持固定"
echo ""
echo "📱 访问地址："
echo "   https://scriptecho.games (主页)"
echo "   https://scriptecho.games/ar-js.html (AR.js版本)"
echo ""
echo "🎯 现在使用成熟的AR.js库，真正解决地面锚定问题！" 