using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ScriptEcho.Platform;
using System.Collections.Generic;

namespace ScriptEcho.UI
{
    /// <summary>
    /// ScriptEcho主界面UI控制器
    /// </summary>
    public class ScriptEchoMainUI : MonoBehaviour
    {
        [Header("顶部信息栏")]
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image avatarImage;
        [SerializeField] private TextMeshProUGUI earningsText;

        [Header("主要功能按钮")]
        [SerializeField] private Button startRoleMatchingButton;
        [SerializeField] private Button createSessionButton;
        [SerializeField] private Button joinSessionButton;
        [SerializeField] private Button arRacingGameButton;
        [SerializeField] private Button revenueShareButton;

        [Header("快捷菜单")]
        [SerializeField] private Button friendsButton;
        [SerializeField] private Button achievementsButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button profileButton;

        [Header("游戏会话列表")]
        [SerializeField] private Transform sessionListParent;
        [SerializeField] private GameObject sessionItemPrefab;
        [SerializeField] private ScrollRect sessionScrollRect;

        [Header("AR游戏预览")]
        [SerializeField] private Image arGamePreviewImage;
        [SerializeField] private TextMeshProUGUI arGameDescriptionText;
        [SerializeField] private Button quickPlayARButton;

        [Header("通知和状态")]
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private TextMeshProUGUI notificationText;
        [SerializeField] private TextMeshProUGUI onlineStatusText;

        private ScriptEchoPlatformManager platformManager;
        private List<GameObject> sessionListItems = new List<GameObject>();

        void Start()
        {
            InitializeUI();
            BindEvents();
            UpdatePlayerInfo();
        }

        void OnEnable()
        {
            if (platformManager != null)
            {
                platformManager.OnStateChanged.AddListener(OnPlatformStateChanged);
            }
        }

        void OnDisable()
        {
            if (platformManager != null)
            {
                platformManager.OnStateChanged.RemoveListener(OnPlatformStateChanged);
            }
        }

        private void InitializeUI()
        {
            platformManager = ScriptEchoPlatformManager.Instance;
            
            // 设置AR游戏描述
            arGameDescriptionText.text = "体验沉浸式AR赛车竞技，在现实世界中放置赛道和车辆，完成刺激的竞速挑战！";
            
            // 设置在线状态
            onlineStatusText.text = "在线";
            onlineStatusText.color = Color.green;

            // 隐藏通知面板
            if (notificationPanel != null)
                notificationPanel.SetActive(false);
        }

        private void BindEvents()
        {
            // 绑定主要功能按钮事件
            startRoleMatchingButton.onClick.AddListener(OnStartRoleMatching);
            createSessionButton.onClick.AddListener(OnCreateSession);
            joinSessionButton.onClick.AddListener(OnJoinSession);
            arRacingGameButton.onClick.AddListener(OnLaunchARRacingGame);
            revenueShareButton.onClick.AddListener(OnOpenRevenueShare);
            quickPlayARButton.onClick.AddListener(OnQuickPlayAR);

            // 绑定快捷菜单事件
            friendsButton.onClick.AddListener(OnOpenFriends);
            achievementsButton.onClick.AddListener(OnOpenAchievements);
            settingsButton.onClick.AddListener(OnOpenSettings);
            profileButton.onClick.AddListener(OnOpenProfile);
        }

        private void UpdatePlayerInfo()
        {
            if (platformManager?.CurrentPlayer != null)
            {
                var player = platformManager.CurrentPlayer;
                playerNameText.text = player.PlayerName;
                levelText.text = $"Lv.{player.Level}";
                earningsText.text = $"¥{player.TotalEarnings:F2}";

                // 如果有头像URL，可以在这里加载头像
                // LoadAvatarFromUrl(player.AvatarUrl);
            }
            else
            {
                // 创建默认玩家配置
                var defaultPlayer = new PlayerProfile("默认玩家");
                platformManager?.SetCurrentPlayer(defaultPlayer);
                UpdatePlayerInfo();
            }
        }

        private void OnPlatformStateChanged(PlatformState newState)
        {
            // 根据平台状态更新UI显示
            switch (newState)
            {
                case PlatformState.MainMenu:
                    UpdateButtonStates(true);
                    break;
                case PlatformState.RoleMatching:
                case PlatformState.GameSession:
                case PlatformState.ARGame:
                    UpdateButtonStates(false);
                    break;
            }
        }

        private void UpdateButtonStates(bool mainMenuActive)
        {
            startRoleMatchingButton.interactable = mainMenuActive;
            createSessionButton.interactable = mainMenuActive;
            joinSessionButton.interactable = mainMenuActive;
            arRacingGameButton.interactable = mainMenuActive;
            quickPlayARButton.interactable = mainMenuActive;
        }

        // 事件处理方法
        private void OnStartRoleMatching()
        {
            ShowNotification("开始智能角色匹配...");
            platformManager.StartRoleMatching();
        }

        private void OnCreateSession()
        {
            ShowNotification("创建新的游戏会话...");
            // 这里可以打开创建会话的详细界面
            CreateNewGameSession();
        }

        private void OnJoinSession()
        {
            ShowNotification("寻找可加入的游戏会话...");
            RefreshSessionList();
        }

        private void OnLaunchARRacingGame()
        {
            ShowNotification("启动AR赛车游戏...");
            platformManager.LaunchARRacingGame();
        }

        private void OnQuickPlayAR()
        {
            ShowNotification("快速开始AR游戏...");
            // 直接启动单人AR游戏模式
            platformManager.LaunchARRacingGame();
        }

        private void OnOpenRevenueShare()
        {
            platformManager.OpenRevenueShare();
        }

        private void OnOpenFriends()
        {
            ShowNotification("好友系统开发中...");
        }

        private void OnOpenAchievements()
        {
            ShowNotification("成就系统开发中...");
        }

        private void OnOpenSettings()
        {
            ShowNotification("设置界面开发中...");
        }

        private void OnOpenProfile()
        {
            ShowNotification("个人资料开发中...");
        }

        private void CreateNewGameSession()
        {
            if (platformManager?.CurrentPlayer != null)
            {
                var session = new GameSession("新游戏会话", "经典剧本", platformManager.CurrentPlayer.PlayerId);
                session.MaxPlayers = 6;
                session.MinPlayers = 3;
                session.SessionPrice = 50m;
                session.HostSharePercentage = 20m;
                session.PlatformSharePercentage = 10m;
                session.EnableARRacingGame(); // 启用AR赛车小游戏

                platformManager.StartGameSession(session);
            }
        }

        private void RefreshSessionList()
        {
            // 清除现有列表项
            foreach (var item in sessionListItems)
            {
                if (item != null) Destroy(item);
            }
            sessionListItems.Clear();

            // 这里应该从服务器获取会话列表
            // 目前创建一些示例会话用于演示
            CreateSampleSessionItems();
        }

        private void CreateSampleSessionItems()
        {
            string[] sampleSessions = {
                "经典推理 - 密室逃脱",
                "现代悬疑 - 公司阴谋", 
                "古代奇案 - 皇宫疑云",
                "科幻冒险 - 太空站危机"
            };

            foreach (var sessionName in sampleSessions)
            {
                if (sessionItemPrefab != null)
                {
                    var item = Instantiate(sessionItemPrefab, sessionListParent);
                    var itemText = item.GetComponentInChildren<TextMeshProUGUI>();
                    if (itemText != null)
                    {
                        itemText.text = sessionName + " (AR模式)";
                    }

                    var itemButton = item.GetComponent<Button>();
                    if (itemButton != null)
                    {
                        itemButton.onClick.AddListener(() => JoinSpecificSession(sessionName));
                    }

                    sessionListItems.Add(item);
                }
            }
        }

        private void JoinSpecificSession(string sessionName)
        {
            ShowNotification($"加入会话: {sessionName}");
            // 这里实现加入特定会话的逻辑
        }

        private void ShowNotification(string message)
        {
            if (notificationPanel != null && notificationText != null)
            {
                notificationText.text = message;
                notificationPanel.SetActive(true);
                
                // 3秒后自动隐藏通知
                Invoke(nameof(HideNotification), 3f);
            }
            
            Debug.Log($"通知: {message}");
        }

        private void HideNotification()
        {
            if (notificationPanel != null)
            {
                notificationPanel.SetActive(false);
            }
        }

        /// <summary>
        /// 外部调用，更新收益显示
        /// </summary>
        public void UpdateEarningsDisplay()
        {
            if (platformManager?.CurrentPlayer != null)
            {
                earningsText.text = $"¥{platformManager.CurrentPlayer.TotalEarnings:F2}";
            }
        }

        /// <summary>
        /// 外部调用，添加经验值
        /// </summary>
        public void AddExperience(int exp)
        {
            if (platformManager?.CurrentPlayer != null)
            {
                platformManager.CurrentPlayer.AddExperience(exp);
                levelText.text = $"Lv.{platformManager.CurrentPlayer.Level}";
                ShowNotification($"获得 {exp} 经验值!");
            }
        }
    }
} 