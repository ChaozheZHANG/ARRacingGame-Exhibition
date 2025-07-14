using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using ScriptEcho.Platform;
using ScriptEcho.Integration;
using DilmerGames.Core.Singletons;

namespace ScriptEcho.Platform
{
    /// <summary>
    /// 平台游戏管理器 - 扩展原有GameManager以支持ScriptEcho平台功能
    /// </summary>
    public class PlatformGameManager : Singleton<PlatformGameManager>
    {
        [Header("平台集成")]
        [SerializeField] private bool enablePlatformIntegration = true;
        [SerializeField] private GameObject platformUIPrefab;
        
        [Header("游戏设置")]
        [SerializeField] private GlobalGameSettings globalGameSettings;
        [SerializeField] private bool autoStartMissions = true;
        
        [Header("平台数据")]
        [SerializeField] private bool saveProgressToPlatform = true;
        [SerializeField] private float autoSaveInterval = 60f; // 每分钟自动保存

        private GameManager originalGameManager;
        private ARGameIntegration gameIntegration;
        private ARGameBridge gameBridge;
        private float lastAutoSaveTime;

        public GlobalGameSettings GlobalGameSettings => globalGameSettings;
        public bool IsPlatformMode => enablePlatformIntegration;

        protected override void Awake()
        {
            base.Awake();
            
            // 查找原有的GameManager
            originalGameManager = FindObjectOfType<GameManager>();
            
            InitializePlatformIntegration();
        }

        void Start()
        {
            // 启用增强触摸支持
            EnhancedTouchSupport.Enable();
            
            // 设置平台UI
            SetupPlatformUI();
            
            // 开始自动保存
            if (saveProgressToPlatform)
            {
                lastAutoSaveTime = Time.time;
            }
        }

        void Update()
        {
            // 自动保存进度
            if (saveProgressToPlatform && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                SaveProgressToPlatform();
                lastAutoSaveTime = Time.time;
            }
        }

        private void InitializePlatformIntegration()
        {
            if (!enablePlatformIntegration) return;

            // 获取游戏集成组件
            gameIntegration = ARGameIntegration.Instance;
            
            // 如果在AR游戏场景中，设置桥接器
            if (IsInARGameScene())
            {
                SetupGameBridge();
            }

            Debug.Log("平台游戏管理器初始化完成");
        }

        private bool IsInARGameScene()
        {
            // 检查当前场景是否为AR游戏场景
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            return currentScene.Contains("MainGameAR") || currentScene.Contains("AR");
        }

        private void SetupGameBridge()
        {
            // 查找或创建游戏桥接器
            gameBridge = FindObjectOfType<ARGameBridge>();
            
            if (gameBridge == null && gameIntegration != null)
            {
                // 创建桥接器游戏对象
                GameObject bridgeObject = new GameObject("ARGameBridge");
                gameBridge = bridgeObject.AddComponent<ARGameBridge>();
                
                Debug.Log("自动创建AR游戏桥接器");
            }
        }

        private void SetupPlatformUI()
        {
            if (!enablePlatformIntegration || platformUIPrefab == null) return;

            // 在AR游戏场景中实例化平台UI
            if (IsInARGameScene())
            {
                var platformUI = Instantiate(platformUIPrefab);
                platformUI.name = "PlatformUI";
                
                // 确保UI在最顶层
                Canvas canvas = platformUI.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingOrder = 1000;
                    canvas.overrideSorting = true;
                }
                
                Debug.Log("平台UI已加载到AR游戏场景");
            }
        }

        private void SaveProgressToPlatform()
        {
            if (!enablePlatformIntegration) return;

            var platformManager = ScriptEchoPlatformManager.Instance;
            if (platformManager?.CurrentPlayer == null) return;

            // 获取当前游戏状态
            var missionManager = FindObjectOfType<PlayerMissionManager>();
            if (missionManager != null)
            {
                // 保存任务进度
                int completedMissions = missionManager.CompletedMissions;
                platformManager.CurrentPlayer.CompletedMissions = completedMissions;
            }

            // 保存游戏时长
            if (gameIntegration != null)
            {
                float currentPlayTime = gameIntegration.GamePlayTime;
                platformManager.CurrentPlayer.TotalPlayTime += currentPlayTime;
            }

            Debug.Log("游戏进度已保存到平台");
        }

        #region 公共接口

        /// <summary>
        /// 获取原有的GameManager引用
        /// </summary>
        public GameManager GetOriginalGameManager()
        {
            return originalGameManager;
        }

        /// <summary>
        /// 强制保存进度
        /// </summary>
        public void ForceSaveProgress()
        {
            if (saveProgressToPlatform)
            {
                SaveProgressToPlatform();
            }
        }

        /// <summary>
        /// 设置平台集成状态
        /// </summary>
        public void SetPlatformIntegration(bool enabled)
        {
            enablePlatformIntegration = enabled;
            
            if (gameBridge != null)
            {
                gameBridge.SetUIVisible(enabled);
            }
        }

        /// <summary>
        /// 获取当前游戏统计
        /// </summary>
        public PlatformGameStats GetGameStats()
        {
            var stats = new PlatformGameStats();
            
            if (gameIntegration != null)
            {
                var arStats = gameIntegration.GetCurrentGameStats();
                stats.PlayTime = arStats.PlayTime;
                stats.IsActive = arStats.IsActive;
                stats.CompletedMissions = arStats.CompletedMissions;
            }

            var missionManager = FindObjectOfType<PlayerMissionManager>();
            if (missionManager != null)
            {
                stats.TargetsRemaining = missionManager.MissionTargetCount;
                stats.CarPlaced = missionManager.CarWasPlaced;
            }

            return stats;
        }

        /// <summary>
        /// 重置游戏状态
        /// </summary>
        public void ResetGameState()
        {
            var missionManager = FindObjectOfType<PlayerMissionManager>();
            if (missionManager != null)
            {
                missionManager.StartMission();
            }

            Debug.Log("游戏状态已重置");
        }

        /// <summary>
        /// 显示任务提示
        /// </summary>
        public void ShowMissionHint(string hint)
        {
            if (gameBridge != null)
            {
                gameBridge.UpdateMissionHint(hint);
            }
            
            Debug.Log($"任务提示: {hint}");
        }

        /// <summary>
        /// 触发成就解锁
        /// </summary>
        public void UnlockAchievement(string achievementName)
        {
            var platformManager = ScriptEchoPlatformManager.Instance;
            if (platformManager?.CurrentPlayer != null)
            {
                if (!platformManager.CurrentPlayer.Achievements.Contains(achievementName))
                {
                    platformManager.CurrentPlayer.Achievements.Add(achievementName);
                    
                    if (gameBridge != null)
                    {
                        gameBridge.ShowAchievementUnlocked(achievementName);
                    }
                    
                    Debug.Log($"成就解锁: {achievementName}");
                }
            }
        }

        #endregion

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && saveProgressToPlatform)
            {
                // 应用暂停时保存进度
                SaveProgressToPlatform();
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && saveProgressToPlatform)
            {
                // 应用失去焦点时保存进度
                SaveProgressToPlatform();
            }
        }

        void OnDestroy()
        {
            // 应用退出时保存进度
            if (saveProgressToPlatform)
            {
                SaveProgressToPlatform();
            }
        }
    }

    /// <summary>
    /// 平台游戏统计信息
    /// </summary>
    [System.Serializable]
    public struct PlatformGameStats
    {
        public float PlayTime;
        public bool IsActive;
        public int CompletedMissions;
        public int TargetsRemaining;
        public bool CarPlaced;
    }
} 