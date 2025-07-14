using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScriptEcho.Integration;
using ScriptEcho.Platform;
using DilmerGames.Core.Singletons;

namespace ScriptEcho.UI
{
    /// <summary>
    /// 移动端AR游戏UI管理器 - 专为iPhone等移动设备优化的UI界面
    /// </summary>
    public class MobileARGameUI : Singleton<MobileARGameUI>
    {
        [Header("主要UI面板")]
        [SerializeField] private GameObject welcomePanel;
        [SerializeField] private GameObject placementPanel;
        [SerializeField] private GameObject gameplayPanel;
        [SerializeField] private GameObject completionPanel;
        [SerializeField] private GameObject settingsPanel;
        
        [Header("游戏状态显示")]
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI instructionText;
        [SerializeField] private Slider progressSlider;
        
        [Header("控制按钮")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button resetGameButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button backToPlatformButton;
        [SerializeField] private Button helpButton;
        
        [Header("AR相关UI")]
        [SerializeField] private GameObject placementReticle;
        [SerializeField] private TextMeshProUGUI arStatusText;
        [SerializeField] private Button calibrateARButton;
        
        [Header("平台集成UI")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI experienceText;
        [SerializeField] private Button leaderboardButton;
        
        [Header("移动端优化")]
        [SerializeField] private bool enableHapticFeedback = true;
        [SerializeField] private bool adaptToSafeArea = true;
        [SerializeField] private float buttonSizeMultiplier = 1.2f;
        
        private MobileARGameIntegration gameIntegration;
        private ScriptEchoPlatformManager platformManager;
        private RectTransform canvasRect;
        
        // UI状态
        private UIState currentState = UIState.Welcome;
        
        public enum UIState
        {
            Welcome,
            Placement,
            Gameplay,
            Completion,
            Settings
        }

        protected override void Awake()
        {
            base.Awake();
            canvasRect = GetComponent<RectTransform>();
            SetupMobileUI();
        }

        void Start()
        {
            gameIntegration = MobileARGameIntegration.Instance;
            platformManager = ScriptEchoPlatformManager.Instance;
            
            BindEvents();
            InitializeUI();
            AdaptToSafeArea();
        }

        /// <summary>
        /// 设置移动端UI优化
        /// </summary>
        private void SetupMobileUI()
        {
            // 调整按钮大小适应移动设备
            Button[] buttons = GetComponentsInChildren<Button>();
            foreach (Button button in buttons)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    Vector2 sizeDelta = buttonRect.sizeDelta;
                    buttonRect.sizeDelta = sizeDelta * buttonSizeMultiplier;
                }
            }
            
            // 确保文本大小适中
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.fontSize < 14)
                    text.fontSize = 14;
            }
        }

        /// <summary>
        /// 适应安全区域
        /// </summary>
        private void AdaptToSafeArea()
        {
            if (!adaptToSafeArea) return;
            
            // 获取安全区域
            Rect safeArea = Screen.safeArea;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);
            
            // 计算安全区域的相对位置
            Vector2 anchorMin = safeArea.position / screenSize;
            Vector2 anchorMax = (safeArea.position + safeArea.size) / screenSize;
            
            // 应用到Canvas
            if (canvasRect != null)
            {
                canvasRect.anchorMin = anchorMin;
                canvasRect.anchorMax = anchorMax;
                canvasRect.offsetMin = Vector2.zero;
                canvasRect.offsetMax = Vector2.zero;
            }
        }

        /// <summary>
        /// 绑定事件
        /// </summary>
        private void BindEvents()
        {
            // 游戏集成事件
            if (gameIntegration != null)
            {
                gameIntegration.OnARSessionStarted.AddListener(OnARSessionStarted);
                gameIntegration.OnCarPlaced.AddListener(OnCarPlaced);
                gameIntegration.OnGameStarted.AddListener(OnGameStarted);
                gameIntegration.OnGameCompleted.AddListener(OnGameCompleted);
                gameIntegration.OnTargetCollected.AddListener(OnTargetCollected);
            }
            
            // 按钮事件
            if (startGameButton != null)
                startGameButton.onClick.AddListener(OnStartGameClicked);
                
            if (resetGameButton != null)
                resetGameButton.onClick.AddListener(OnResetGameClicked);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
                
            if (backToPlatformButton != null)
                backToPlatformButton.onClick.AddListener(OnBackToPlatformClicked);
                
            if (helpButton != null)
                helpButton.onClick.AddListener(OnHelpClicked);
                
            if (calibrateARButton != null)
                calibrateARButton.onClick.AddListener(OnCalibrateARClicked);
                
            if (leaderboardButton != null)
                leaderboardButton.onClick.AddListener(OnLeaderboardClicked);
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            SwitchToState(UIState.Welcome);
            UpdatePlayerInfo();
            
            if (instructionText != null)
                instructionText.text = "欢迎来到AR赛车游戏！";
                
            if (statusText != null)
                statusText.text = "准备开始";
        }

        /// <summary>
        /// 切换UI状态
        /// </summary>
        public void SwitchToState(UIState newState)
        {
            currentState = newState;
            
            // 隐藏所有面板
            SetPanelActive(welcomePanel, false);
            SetPanelActive(placementPanel, false);
            SetPanelActive(gameplayPanel, false);
            SetPanelActive(completionPanel, false);
            SetPanelActive(settingsPanel, false);
            
            // 显示对应面板
            switch (newState)
            {
                case UIState.Welcome:
                    SetPanelActive(welcomePanel, true);
                    UpdateWelcomeUI();
                    break;
                    
                case UIState.Placement:
                    SetPanelActive(placementPanel, true);
                    UpdatePlacementUI();
                    break;
                    
                case UIState.Gameplay:
                    SetPanelActive(gameplayPanel, true);
                    UpdateGameplayUI();
                    break;
                    
                case UIState.Completion:
                    SetPanelActive(completionPanel, true);
                    UpdateCompletionUI();
                    break;
                    
                case UIState.Settings:
                    SetPanelActive(settingsPanel, true);
                    UpdateSettingsUI();
                    break;
            }
            
            PlayHapticFeedback();
        }

        /// <summary>
        /// 设置面板激活状态
        /// </summary>
        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null)
                panel.SetActive(active);
        }

        /// <summary>
        /// 更新欢迎界面
        /// </summary>
        private void UpdateWelcomeUI()
        {
            if (instructionText != null)
                instructionText.text = "点击开始游戏来体验AR赛车！";
                
            if (statusText != null)
                statusText.text = "就绪";
        }

        /// <summary>
        /// 更新放置界面
        /// </summary>
        private void UpdatePlacementUI()
        {
            if (instructionText != null)
                instructionText.text = "移动设备找到平面，然后点击屏幕放置赛车";
                
            if (statusText != null)
                statusText.text = "寻找平面中...";
                
            if (placementReticle != null)
                placementReticle.SetActive(true);
        }

        /// <summary>
        /// 更新游戏界面
        /// </summary>
        private void UpdateGameplayUI()
        {
            if (instructionText != null)
                instructionText.text = "使用虚拟按钮或设备倾斜来控制赛车！";
                
            if (placementReticle != null)
                placementReticle.SetActive(false);
                
            UpdateScore();
        }

        /// <summary>
        /// 更新完成界面
        /// </summary>
        private void UpdateCompletionUI()
        {
            if (instructionText != null)
                instructionText.text = "恭喜完成游戏！";
                
            if (statusText != null)
                statusText.text = "游戏完成";
                
            // 显示最终分数和统计
            if (gameIntegration != null)
            {
                UpdateFinalStats();
            }
        }

        /// <summary>
        /// 更新设置界面
        /// </summary>
        private void UpdateSettingsUI()
        {
            // 这里可以添加设置选项
        }

        /// <summary>
        /// 更新玩家信息
        /// </summary>
        private void UpdatePlayerInfo()
        {
            if (platformManager?.CurrentPlayer != null)
            {
                var player = platformManager.CurrentPlayer;
                
                if (playerNameText != null)
                    playerNameText.text = player.PlayerName;
                    
                if (levelText != null)
                    levelText.text = $"Lv.{player.Level}";
                    
                if (experienceText != null)
                    experienceText.text = $"EXP: {player.ExperiencePoints}";
            }
        }

        /// <summary>
        /// 更新分数显示
        /// </summary>
        private void UpdateScore()
        {
            if (gameIntegration != null)
            {
                if (scoreText != null)
                    scoreText.text = $"{gameIntegration.TargetsCollected}/{gameIntegration.TotalTargets}";
                    
                if (progressSlider != null)
                {
                    progressSlider.maxValue = gameIntegration.TotalTargets;
                    progressSlider.value = gameIntegration.TargetsCollected;
                }
            }
        }

        /// <summary>
        /// 更新最终统计
        /// </summary>
        private void UpdateFinalStats()
        {
            if (gameIntegration != null && statusText != null)
            {
                statusText.text = gameIntegration.GetGameStats();
            }
        }

        /// <summary>
        /// AR会话开始事件
        /// </summary>
        private void OnARSessionStarted()
        {
            if (arStatusText != null)
                arStatusText.text = "AR已就绪";
                
            SwitchToState(UIState.Placement);
        }

        /// <summary>
        /// 车辆放置事件
        /// </summary>
        private void OnCarPlaced()
        {
            if (statusText != null)
                statusText.text = "车辆已放置";
                
            PlayHapticFeedback();
        }

        /// <summary>
        /// 游戏开始事件
        /// </summary>
        private void OnGameStarted()
        {
            SwitchToState(UIState.Gameplay);
        }

        /// <summary>
        /// 游戏完成事件
        /// </summary>
        private void OnGameCompleted()
        {
            SwitchToState(UIState.Completion);
            PlayHapticFeedback();
        }

        /// <summary>
        /// 目标收集事件
        /// </summary>
        private void OnTargetCollected(int collected)
        {
            UpdateScore();
            PlayHapticFeedback();
            
            if (statusText != null)
                statusText.text = $"目标已收集! ({collected})";
        }

        // 按钮事件处理
        private void OnStartGameClicked()
        {
            SwitchToState(UIState.Placement);
            PlayHapticFeedback();
        }

        private void OnResetGameClicked()
        {
            gameIntegration?.ResetGame();
            SwitchToState(UIState.Placement);
            PlayHapticFeedback();
        }

        private void OnSettingsClicked()
        {
            SwitchToState(UIState.Settings);
            PlayHapticFeedback();
        }

        private void OnBackToPlatformClicked()
        {
            platformManager?.SwitchToState(PlatformState.MainMenu);
            PlayHapticFeedback();
        }

        private void OnHelpClicked()
        {
            // 显示帮助信息
            if (instructionText != null)
                instructionText.text = "移动设备寻找平面，点击放置赛车，然后收集所有旗帜！";
            PlayHapticFeedback();
        }

        private void OnCalibrateARClicked()
        {
            // 重新校准AR
            if (arStatusText != null)
                arStatusText.text = "正在校准AR...";
            PlayHapticFeedback();
        }

        private void OnLeaderboardClicked()
        {
            // 显示排行榜
            Debug.Log("显示排行榜");
            PlayHapticFeedback();
        }

        /// <summary>
        /// 播放触觉反馈
        /// </summary>
        private void PlayHapticFeedback()
        {
            if (enableHapticFeedback && SystemInfo.deviceType == DeviceType.Handheld)
            {
                Handheld.Vibrate();
            }
        }

        /// <summary>
        /// 设置AR状态文本
        /// </summary>
        public void SetARStatusText(string status)
        {
            if (arStatusText != null)
                arStatusText.text = status;
        }

        /// <summary>
        /// 显示临时消息
        /// </summary>
        public void ShowTemporaryMessage(string message, float duration = 3f)
        {
            if (statusText != null)
            {
                statusText.text = message;
                Invoke(nameof(ClearStatusText), duration);
            }
        }

        private void ClearStatusText()
        {
            if (statusText != null)
                statusText.text = "";
        }

        void OnDestroy()
        {
            // 清理事件绑定
            if (gameIntegration != null)
            {
                gameIntegration.OnARSessionStarted.RemoveListener(OnARSessionStarted);
                gameIntegration.OnCarPlaced.RemoveListener(OnCarPlaced);
                gameIntegration.OnGameStarted.RemoveListener(OnGameStarted);
                gameIntegration.OnGameCompleted.RemoveListener(OnGameCompleted);
                gameIntegration.OnTargetCollected.RemoveListener(OnTargetCollected);
            }
        }
    }
} 