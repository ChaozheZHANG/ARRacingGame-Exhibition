# 🎪 ScriptEcho AR赛车 - 展览部署指南

## 🚀 云端部署概述

由于本地内存限制，我们采用**GitHub Actions云端构建**+**GitHub Pages部署**的方案，实现**零本地资源占用**的展览级AR游戏部署。

## 📋 部署清单

### ✅ 已完成配置
- [x] WebGL专用游戏管理器
- [x] 展览优化JavaScript插件
- [x] GitHub Actions自动构建
- [x] 移动端触摸控制
- [x] 自动重置和性能优化
- [x] 专业展览UI界面

### 🎯 部署步骤

## 第1步：创建GitHub仓库

```bash
# 在当前目录初始化Git仓库
git init

# 添加所有文件
git add .

# 提交更改
git commit -m "🎪 ScriptEcho AR赛车展览版 - 初始提交"

# 连接到GitHub仓库（替换为你的用户名和仓库名）
git remote add origin https://github.com/你的用户名/ARRacingGame-Exhibition.git

# 推送到GitHub
git push -u origin main
```

## 第2步：配置Unity License

在GitHub仓库中设置Secrets：

1. 访问你的GitHub仓库
2. 进入 `Settings` → `Secrets and variables` → `Actions`
3. 添加以下Secrets：

```
UNITY_LICENSE: 你的Unity License内容
UNITY_EMAIL: 你的Unity账号邮箱
UNITY_PASSWORD: 你的Unity账号密码
```

### 获取Unity License方法：

#### 方法1：Unity Personal License（免费）
```bash
# 在本地运行Unity编辑器，然后执行：
Unity -batchmode -quit -logFile -serial SB-XXXX-XXXX-XXXX-XXXX-XXXX -username "email@example.com" -password "YourPassword"
```

#### 方法2：使用GitHub上的Unity License获取工具
访问：https://github.com/game-ci/unity-license-requests

## 第3步：启用GitHub Pages

1. 在GitHub仓库中，进入 `Settings` → `Pages`
2. Source选择 `GitHub Actions`
3. 保存设置

## 第4步：触发构建

```bash
# 推送任何更改都会触发自动构建
git add .
git commit -m "🚀 触发展览版构建"
git push
```

或者手动触发：
1. 进入仓库的 `Actions` 标签
2. 选择 `Unity WebGL Build for Exhibition`
3. 点击 `Run workflow`

## 📱 展览访问方式

构建完成后，你的AR赛车游戏将在以下地址可访问：

```
https://你的用户名.github.io/ARRacingGame-Exhibition
```

## 🎯 展览现场使用指南

### 访客体验流程：
1. **扫码访问** - 提供二维码让访客快速访问
2. **允许权限** - 浏览器请求相机权限
3. **开始游戏** - 点击开始按钮进入AR体验
4. **放置赛车** - 触摸屏幕在现实环境中放置虚拟赛车
5. **驾驶体验** - 使用触摸控制收集旗帜
6. **自动重置** - 游戏完成或超时自动重置

### 展览优化特性：
- ⏰ **5分钟自动重置** - 避免长时间占用
- 📱 **多设备兼容** - iPhone、Android、iPad全支持
- 🔄 **触觉反馈** - 增强体验真实感
- 🎮 **简化控制** - 一键即玩，无需复杂操作
- 📊 **性能自适应** - 根据设备自动调整画质

## 🛠️ 技术架构

```
展览访客手机
    ↓
浏览器访问GitHub Pages
    ↓
WebGL AR游戏加载
    ↓
相机权限获取
    ↓
WebXR AR体验
    ↓
触摸控制交互
    ↓
ScriptEcho平台数据
```

## 🎪 展览现场准备

### 1. 硬件准备
- [ ] 展示屏幕/投影（显示二维码和说明）
- [ ] 稳定的WiFi网络
- [ ] 充电宝/充电设备（供访客使用）
- [ ] 指导牌（操作说明）

### 2. 软件准备
- [ ] 生成访问二维码
- [ ] 准备Demo设备（预装浏览器书签）
- [ ] 测试网络连接速度
- [ ] 备用域名（防止GitHub Pages故障）

### 3. 现场部署清单
```bash
# 生成二维码（可以使用在线工具）
https://你的用户名.github.io/ARRacingGame-Exhibition

# 测试访问链接
curl -I https://你的用户名.github.io/ARRacingGame-Exhibition

# 检查构建状态
# 访问 https://github.com/你的用户名/ARRacingGame-Exhibition/actions
```

## 📊 实时监控

### 构建状态监控
- GitHub Actions页面显示构建进度
- 构建失败会收到邮件通知
- 可以实时查看构建日志

### 访问统计
使用GitHub Pages内置的流量统计，或集成第三方分析：

```javascript
// 可在index.html中添加Google Analytics
gtag('config', 'GA_MEASUREMENT_ID');
```

## 🔧 故障排除

### 常见问题及解决方案：

**Q: 构建失败 "Unity License Error"**
```bash
# 重新生成Unity License
# 确保UNITY_LICENSE、UNITY_EMAIL、UNITY_PASSWORD正确设置
```

**Q: 相机权限被拒绝**
```bash
# 确保访问HTTPS链接
# 在浏览器地址栏点击锁图标重新授权
```

**Q: 游戏卡在加载界面**
```bash
# 检查网络连接
# 刷新页面重试
# 确认WebGL文件完整
```

**Q: 触摸控制不响应**
```bash
# 确认设备支持触摸
# 检查浏览器兼容性
# 尝试全屏模式
```

## 🎯 展览效果优化建议

### 1. 网络优化
- 使用CDN加速（可配置CloudFlare）
- 压缩WebGL构建文件
- 预加载关键资源

### 2. 用户体验优化
- 添加音效反馈
- 优化加载动画
- 提供多语言支持

### 3. 展览管理
- 设置访客使用时长限制
- 收集用户反馈数据
- 实时监控系统状态

## 📞 技术支持

部署过程中如有问题，请检查：

1. **GitHub Actions日志** - 查看详细错误信息
2. **浏览器控制台** - 检查前端错误
3. **网络连接** - 确保稳定的互联网连接
4. **设备兼容性** - 测试不同品牌手机

---

## 🎉 完成！

按照以上步骤，你将拥有一个**专业级展览AR赛车游戏**：

- 🌐 **零安装体验** - 扫码即玩
- 🎮 **沉浸式AR** - 真实环境中的虚拟赛车
- 📱 **全设备支持** - iPhone、Android无缝体验
- 🔄 **自动化部署** - 云端构建，无需本地Unity
- 🎪 **展览级稳定性** - 自动重置，持续运行

**立即开始部署，让访客体验未来科技的魅力！** 🚀 