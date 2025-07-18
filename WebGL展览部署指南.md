# 🚀 AR赛车游戏 WebGL展览部署指南

## 🎯 部署概述

您的Unity AR赛车项目已经具备完整的WebGL构建和部署系统，可以在**不打开Unity**的情况下部署到网页！

## 🌐 方案一：GitHub Pages自动部署（推荐）

### ✅ 优势
- 🆓 完全免费
- 🤖 自动构建和部署
- 🔄 支持自动更新
- 📱 移动端优化

### 📋 部署步骤

1. **查看构建状态**
   ```
   访问: https://github.com/ChaozheZHANG/ARRacingGame-Exhibition/actions
   ```

2. **如果构建成功**
   ```
   游戏地址: https://chaozhezhang.github.io/ARRacingGame-Exhibition/
   ```

3. **如果需要Unity许可证配置**
   - GitHub Actions需要Unity许可证才能构建
   - 可联系Unity获取个人许可证
   - 或使用方案二本地构建

## 🖥️ 方案二：本地轻量构建

如果GitHub Actions需要额外配置，可以使用Unity Hub快速构建：

### 步骤
1. 下载Unity Hub（免费）
2. 安装Unity 2021.2.15f1（项目指定版本）
3. 添加WebGL模块
4. 打开项目，构建WebGL版本

### 构建配置
```
平台: WebGL
场景: Assets/Scenes/MainGameAR_Standalone.unity
输出: Build/WebGL/
```

## 🌟 方案三：服务器部署

### 部署到您的服务器 (119.28.50.43)

1. **获取构建文件**（从方案一或二）

2. **上传到服务器**
   ```bash
   scp -r Build/WebGL/ user@119.28.50.43:/var/www/ar-racing/
   ```

3. **Nginx配置**
   ```nginx
   server {
       listen 80;
       server_name scriptecho.games;
       root /var/www/ar-racing;
       
       location / {
           try_files $uri $uri/ /index.html;
           
           # WebGL特殊MIME类型
           location ~* \\.wasm$ {
               add_header Content-Type application/wasm;
           }
           
           location ~* \\.gz$ {
               add_header Content-Encoding gzip;
           }
       }
   }
   ```

## 📱 展览优化功能

您的项目已包含以下展览专用功能：

### 🎮 交互特性
- ✅ 触摸控制优化
- ✅ 自动权限请求
- ✅ 全屏模式支持
- ✅ 防止意外操作

### 🔄 展览管理
- ✅ 5分钟自动重置
- ✅ 用户引导界面
- ✅ 实时进度显示
- ✅ 触觉反馈支持

### 📺 界面优化
- ✅ 专业展览UI
- ✅ 移动端适配
- ✅ 加载进度显示
- ✅ 操作说明overlay

## 🎯 游戏体验流程

1. **用户访问网页**
2. **自动请求相机权限**
3. **显示欢迎界面和操作说明**
4. **点击屏幕放置虚拟赛车**
5. **触摸控制驾驶**
6. **收集旗帜完成游戏**
7. **自动重置等待下一位用户**

## 🔍 测试建议

### 本地测试
```bash
# 在项目目录运行
python3 -m http.server 8000
# 访问: http://localhost:8000
```

### 移动端测试
- 📱 iPhone Safari
- 🤖 Android Chrome
- 🌐 微信内置浏览器

## ⚡ 故障排除

### 常见问题
1. **相机权限被拒绝** → 刷新页面重新授权
2. **加载缓慢** → 检查网络连接
3. **触摸无响应** → 确保JavaScript已启用
4. **WebGL不支持** → 使用现代浏览器

### 浏览器兼容性
- ✅ Chrome 80+
- ✅ Safari 13+
- ✅ Firefox 75+
- ✅ Edge 80+

## 📊 部署状态监控

- **GitHub Actions**: [构建日志](https://github.com/ChaozheZHANG/ARRacingGame-Exhibition/actions)
- **展览网址**: https://chaozhezhang.github.io/ARRacingGame-Exhibition/
- **备用服务器**: http://scriptecho.games/

---

🎪 **准备就绪！您的AR赛车游戏现在可以在任何设备的浏览器中运行，无需下载安装！** 