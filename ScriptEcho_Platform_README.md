# ScriptEcho AR剧本杀平台 - AR赛车游戏集成方案

## 项目概述

ScriptEcho是一款将AR沉浸式互动玩法与票房分成模式深度融合的剧本杀平台。本项目成功将原有的AR赛车游戏集成为平台中的小游戏模块，实现了完整的平台化解决方案。

## 核心功能

### 🎭 平台功能
- **智能角色匹配**: 基于玩家偏好和游戏风格进行自动匹配
- **游戏会话管理**: 完整的剧本杀会话生命周期管理
- **票房分成系统**: 支持多种分账模式的收益分配
- **社交组局**: 好友系统和组局功能
- **AR内容集成**: 将AR小游戏无缝融入剧本杀体验

### 🏎️ AR赛车游戏集成
- **场景管理**: 支持叠加加载和场景切换两种模式
- **进度同步**: 游戏进度与平台账户实时同步
- **奖励系统**: 完成任务获得经验值和平台货币
- **暂停/恢复**: 完整的游戏状态管理
- **数据统计**: 详细的游戏时长和表现数据

## 技术架构

### 📁 项目结构
```
Assets/Scripts/ScriptEcho/
├── Platform/                    # 平台核心
│   ├── ScriptEchoPlatformManager.cs    # 平台主管理器
│   ├── PlayerProfile.cs               # 玩家配置文件
│   ├── GameSession.cs                 # 游戏会话管理
│   └── PlatformGameManager.cs         # 平台游戏管理器
├── Integration/                 # 游戏集成
│   ├── ARGameIntegration.cs           # AR游戏集成系统
│   └── ARGameBridge.cs                # 游戏桥接器
├── UI/                         # 用户界面
│   └── ScriptEchoMainUI.cs            # 主界面控制器
└── ScriptEchoBootstrap.cs      # 平台启动器
```

### 🏗️ 核心组件

#### 1. ScriptEchoPlatformManager (平台管理器)
```csharp
// 功能：管理平台状态、用户会话、AR游戏启动
public class ScriptEchoPlatformManager : Singleton<ScriptEchoPlatformManager>
{
    // 核心方法
    public void LaunchARRacingGame()        // 启动AR赛车游戏
    public void StartRoleMatching()         // 开始角色匹配
    public void StartGameSession()          // 开始游戏会话
    public void OpenRevenueShare()          // 打开分账界面
}
```

#### 2. ARGameIntegration (AR游戏集成)
```csharp
// 功能：管理AR游戏的加载、卸载、事件处理
public class ARGameIntegration : Singleton<ARGameIntegration>
{
    // 核心方法
    public void LaunchARGame()              // 启动AR游戏
    public void ExitARGame()                // 退出AR游戏
    public void PauseARGame()               // 暂停游戏
    public void ResumeARGame()              // 恢复游戏
}
```

#### 3. PlayerProfile (玩家配置)
```csharp
// 功能：存储玩家数据、经验、收益、成就
public class PlayerProfile
{
    public string PlayerName;               // 玩家姓名
    public int Level;                       // 等级
    public decimal TotalEarnings;           // 总收益
    public List<RoleType> PreferredRoles;   // 偏好角色
    public List<string> Achievements;       // 成就列表
}
```

## 🎮 集成流程

### 1. 启动流程
```
ScriptEchoBootstrap → ScriptEchoPlatformManager → ScriptEchoMainUI
        ↓
    创建默认玩家 → 初始化平台组件 → 显示主界面
```

### 2. AR游戏启动流程
```
用户点击AR游戏按钮 → ScriptEchoPlatformManager.LaunchARRacingGame()
        ↓
ARGameIntegration.LaunchARGame() → 加载AR游戏场景
        ↓
ARGameBridge初始化 → 绑定游戏事件 → 开始游戏
```

### 3. 游戏退出流程
```
玩家退出游戏 → ARGameBridge.ExitGame()
        ↓
ARGameIntegration.ExitARGame() → 计算奖励 → 保存进度
        ↓
卸载AR场景 → 返回平台主界面
```

## 🏆 奖励系统

### 经验值获取
- **任务完成**: 50 EXP
- **游戏完成**: 100 EXP  
- **时长奖励**: 每分钟 10 EXP
- **成就解锁**: 可变 EXP

### 收益系统
- **任务奖励**: 10 虚拟货币
- **会话分成**: 基于表现的动态分配
- **成就奖励**: 特殊成就额外奖励

## 📱 用户界面

### 主界面功能
- **玩家信息栏**: 显示姓名、等级、收益
- **功能按钮区**: 角色匹配、创建会话、AR游戏等
- **会话列表**: 可加入的游戏会话
- **AR游戏预览**: 游戏介绍和快速启动
- **通知系统**: 实时消息和状态提示

### AR游戏界面
- **游戏时间显示**: 实时游戏时长
- **任务状态**: 当前任务进度和提示
- **暂停菜单**: 游戏控制选项
- **奖励通知**: 经验值和成就提示

## 🔧 配置说明

### 平台配置
```csharp
[Header("AR游戏配置")]
public string arGameSceneName = "MainGameAR_ARDK";    // AR游戏场景名
public bool useAdditiveLoading = true;               // 使用叠加加载
public float gameSessionTimeout = 1800f;             // 游戏会话超时(秒)

[Header("奖励设置")]
public int completionExperience = 100;               // 完成游戏经验奖励
public decimal completionReward = 10m;               // 完成游戏货币奖励
```

### 集成模式
1. **叠加加载模式** (推荐)
   - AR游戏场景叠加到平台场景上
   - 保持平台状态，快速切换
   - 适合频繁进出游戏的场景

2. **场景切换模式**
   - 完全切换到AR游戏场景
   - 独立的游戏环境
   - 适合长时间游戏会话

## 🎯 使用指南

### 开发者设置
1. 确保项目包含所有ScriptEcho脚本
2. 创建ScriptEchoPlatform场景并添加平台管理器
3. 配置AR游戏场景名称和加载模式
4. 设置UI组件引用和预制体

### 运行时流程
1. 启动应用自动初始化平台
2. 创建或登录玩家账户
3. 选择剧本杀会话或直接体验AR游戏
4. 在AR游戏中完成任务获得奖励
5. 返回平台查看进度和收益

### 扩展开发
- **添加新小游戏**: 继承ARGameIntegration模式
- **自定义奖励规则**: 修改CalculateAndAwardRewards方法
- **扩展玩家数据**: 在PlayerProfile中添加新字段
- **增加平台功能**: 在ScriptEchoPlatformManager中添加新状态

## 🐛 故障排除

### 常见问题
1. **AR游戏无法启动**
   - 检查场景名称配置是否正确
   - 确认AR游戏场景已添加到Build Settings

2. **数据不同步**
   - 检查ARGameIntegration的事件绑定
   - 确认PlatformGameManager正确初始化

3. **UI显示异常**
   - 检查ScriptEchoMainUI的组件引用
   - 确认Canvas设置正确

### 调试建议
- 启用Debug模式查看详细日志
- 使用ScriptEchoBootstrap的状态检查功能
- 监控ARGameIntegration的游戏统计数据

## 🚀 未来扩展

### 计划功能
- [ ] 多人AR游戏支持
- [ ] 云端数据同步
- [ ] 更多小游戏集成
- [ ] 高级分账算法
- [ ] 社交功能完善
- [ ] 成就系统扩展

### 技术优化
- [ ] 场景加载优化
- [ ] 内存管理改进
- [ ] 网络同步机制
- [ ] UI动画效果
- [ ] 数据缓存策略

## 📞 技术支持

如需技术支持或有任何问题，请联系开发团队。

---

**ScriptEcho - 让AR与剧本杀完美融合，创造全新的沉浸式娱乐体验！** 🎭✨ 