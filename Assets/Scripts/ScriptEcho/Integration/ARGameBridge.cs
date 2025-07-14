using UnityEngine;
using ScriptEcho.Platform;
using ScriptEcho.Integration;
using UnityEngine.UI;
using TMPro;

namespace ScriptEcho.Integration
{
    /// <summary>
    /// AR游戏桥接器 - 在AR游戏场景中提供平台功能接口
    /// </summary>
    public class ARGameBridge : MonoBehaviour
    {
        [Header("平台UI组件")]
        [SerializeField] private GameObject platformOverlayUI;
        [SerializeField] private Button exitGameButton;
        [SerializeField] private Button pauseGameButton;
        [SerializeField] private TextMeshProUGUI gameTimeText;
        [SerializeField] private TextMeshProUGUI playerInfoText;
        [SerializeField] private TextMeshProUGUI missionStatusText;

        [Header("游戏状态显示")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("奖励提示")]
        [SerializeField] private GameObject rewardNotificationPrefab;
        [SerializeField] private Transform notificationParent;

        private ARGameIntegration gameIntegration;
        private ScriptEchoPlatformManager platformManager;
        private PlayerMissionManager missionManager;
        private bool isInitialized = false;

        void Start()
        {
            InitializeBridge();
        }

        void Update()
        {
            if (isInitialized)
            {
                UpdateGameUI();
            }
        }

        private void InitializeBridge()
        {
            // 获取平台组件
            gameIntegration = ARGameIntegration.Instance;
            platformManager = ScriptEchoPlatformManager.Instance;
            missionManager = FindObjectOfType<PlayerMissionManager>();

            // 设置UI
            SetupPlatformUI();
            
            // 绑定事件
            BindEvents();

            isInitialized = true;
            
            Debug.Log("AR游戏桥接器初始化完成");
        }

        private void SetupPlatformUI()
        {
            // 确保平台覆盖UI激活
            if (platformOverlayUI != null)
            {
                platformOverlayUI.SetActive(true);
            }

            // 设置暂停面板初始状态
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }

            // 绑定按钮事件
            if (exitGameButton != null)
                exitGameButton.onClick.AddListener(OnExitGameClicked);
                
            if (pauseGameButton != null)
                pauseGameButton.onClick.AddListener(OnPauseGameClicked);
                
            if (resumeButton != null)
                resumeButton.onClick.AddListener(OnResumeGameClicked);
                
            if (settingsButton != null)
                settingsButton.onClick.AddListener(OnSettingsClicked);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void BindEvents()
        {
            // 绑定游戏集成事件
            if (gameIntegration != null)
            {
                gameIntegration.OnExperienceEarned.AddListener(OnExperienceEarned);
            }

            // 绑定任务管理器事件
            if (missionManager != null)
            {
                missionManager.OnMissionCompleted.AddListener(OnMissionCompleted);
            }
        }

        private void UpdateGameUI()
        {
            // 更新游戏时间显示
            if (gameTimeText != null && gameIntegration != null)
            {
                float gameTime = gameIntegration.GamePlayTime;
                int minutes = Mathf.FloorToInt(gameTime / 60f);
                int seconds = Mathf.FloorToInt(gameTime % 60f);
                gameTimeText.text = $"{minutes:00}:{seconds:00}";
            }

            // 更新玩家信息
            if (playerInfoText != null && platformManager?.CurrentPlayer != null)
            {
                var player = platformManager.CurrentPlayer;
                playerInfoText.text = $"{player.PlayerName} Lv.{player.Level}";
            }

            // 更新任务状态
            if (missionStatusText != null && missionManager != null)
            {
                if (missionManager.CarWasPlaced)
                {
                    int targetCount = missionManager.MissionTargetCount;
                    missionStatusText.text = $"剩余目标: {targetCount}";
                }
                else
                {
                    missionStatusText.text = "请放置汽车";
                }
            }
        }

        #region 按钮事件处理
        private void OnExitGameClicked()
        {
            Debug.Log("玩家请求退出AR游戏");
            
            // 显示确认对话框
            ShowExitConfirmation();
        }

        private void OnPauseGameClicked()
        {
            Debug.Log("暂停AR游戏");
            
            if (gameIntegration != null)
            {
                gameIntegration.PauseARGame();
            }
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        private void OnResumeGameClicked()
        {
            Debug.Log("恢复AR游戏");
            
            if (gameIntegration != null)
            {
                gameIntegration.ResumeARGame();
            }
            
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void OnSettingsClicked()
        {
            Debug.Log("打开游戏设置");
            // 这里可以打开游戏设置界面
            ShowNotification("游戏设置功能开发中...");
        }

        private void OnQuitClicked()
        {
            Debug.Log("退出到主菜单");
            ExitGame();
        }
        #endregion

        #region 游戏事件处理
        private void OnExperienceEarned(int experience)
        {
            ShowRewardNotification($"+{experience} EXP", Color.green);
        }

        private void OnMissionCompleted()
        {
            ShowRewardNotification("任务完成!", Color.yellow);
            
            // 播放完成音效（如果有的话）
            // AudioSource.PlayClipAtPoint(missionCompleteSound, transform.position);
        }
        #endregion

        #region UI辅助方法
        private void ShowExitConfirmation()
        {
            // 简单的确认逻辑，实际项目中应该使用更好的对话框
            if (gameIntegration != null)
            {
                var stats = gameIntegration.GetCurrentGameStats();
                Debug.Log($"确认退出？游戏时长: {stats.PlayTime:F1}秒");
                
                // 这里可以显示一个确认对话框
                // 现在直接退出
                ExitGame();
            }
        }

        private void ExitGame()
        {
            if (gameIntegration != null)
            {
                gameIntegration.ExitARGame();
            }
        }

        private void ShowRewardNotification(string message, Color color)
        {
            if (rewardNotificationPrefab != null && notificationParent != null)
            {
                var notification = Instantiate(rewardNotificationPrefab, notificationParent);
                var notificationText = notification.GetComponentInChildren<TextMeshProUGUI>();
                
                if (notificationText != null)
                {
                    notificationText.text = message;
                    notificationText.color = color;
                }

                // 自动销毁通知
                Destroy(notification, 3f);
            }
            
            Debug.Log($"奖励通知: {message}");
        }

        private void ShowNotification(string message)
        {
            ShowRewardNotification(message, Color.white);
        }
        #endregion

        #region 公共接口
        /// <summary>
        /// 设置UI可见性
        /// </summary>
        public void SetUIVisible(bool visible)
        {
            if (platformOverlayUI != null)
            {
                platformOverlayUI.SetActive(visible);
            }
        }

        /// <summary>
        /// 更新任务提示
        /// </summary>
        public void UpdateMissionHint(string hint)
        {
            if (missionStatusText != null)
            {
                missionStatusText.text = hint;
            }
        }

        /// <summary>
        /// 显示成就通知
        /// </summary>
        public void ShowAchievementUnlocked(string achievementName)
        {
            ShowRewardNotification($"成就解锁: {achievementName}", Color.gold);
        }
        #endregion

        void OnDestroy()
        {
            // 清理事件绑定
            if (gameIntegration != null)
            {
                gameIntegration.OnExperienceEarned.RemoveListener(OnExperienceEarned);
            }

            if (missionManager != null)
            {
                missionManager.OnMissionCompleted.RemoveListener(OnMissionCompleted);
            }
        }
    }
} 