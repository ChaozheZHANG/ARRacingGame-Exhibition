using UnityEngine;
using UnityEngine.SceneManagement;
using ScriptEcho.Platform;
using ScriptEcho.Integration;

namespace ScriptEcho
{
    /// <summary>
    /// ScriptEcho平台启动器 - 负责初始化整个平台系统
    /// </summary>
    public class ScriptEchoBootstrap : MonoBehaviour
    {
        [Header("启动配置")]
        [SerializeField] private bool autoStartPlatform = true;
        [SerializeField] private string platformSceneName = "ScriptEchoPlatform";
        [SerializeField] private bool enableDebugMode = true;

        [Header("默认玩家配置")]
        [SerializeField] private string defaultPlayerName = "新玩家";
        [SerializeField] private int defaultLevel = 1;
        [SerializeField] private decimal defaultEarnings = 0m;

        [Header("平台设置")]
        [SerializeField] private bool enableARIntegration = true;
        [SerializeField] private bool enableAutoSave = true;
        [SerializeField] private float splashScreenDuration = 2f;

        [Header("UI组件")]
        [SerializeField] private GameObject splashScreen;
        [SerializeField] private GameObject loadingScreen;

        private bool isInitialized = false;

        void Awake()
        {
            // 确保启动器不被销毁
            DontDestroyOnLoad(gameObject);
            
            if (autoStartPlatform)
            {
                StartPlatform();
            }
        }

        /// <summary>
        /// 启动ScriptEcho平台
        /// </summary>
        public void StartPlatform()
        {
            if (isInitialized)
            {
                Debug.LogWarning("平台已经初始化");
                return;
            }

            StartCoroutine(InitializePlatformCoroutine());
        }

        private System.Collections.IEnumerator InitializePlatformCoroutine()
        {
            Debug.Log("开始初始化ScriptEcho平台...");

            // 显示启动画面
            if (splashScreen != null)
            {
                splashScreen.SetActive(true);
            }

            yield return new WaitForSeconds(splashScreenDuration);

            // 隐藏启动画面，显示加载画面
            if (splashScreen != null)
            {
                splashScreen.SetActive(false);
            }

            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }

            // 初始化平台组件
            yield return StartCoroutine(InitializePlatformComponents());

            // 创建默认玩家
            yield return StartCoroutine(SetupDefaultPlayer());

            // 加载平台主场景
            yield return StartCoroutine(LoadPlatformScene());

            // 隐藏加载画面
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }

            isInitialized = true;
            Debug.Log("ScriptEcho平台初始化完成!");
        }

        private System.Collections.IEnumerator InitializePlatformComponents()
        {
            Debug.Log("初始化平台组件...");

            // 确保平台管理器存在
            var platformManager = ScriptEchoPlatformManager.Instance;
            if (platformManager == null)
            {
                Debug.LogError("无法找到ScriptEchoPlatformManager!");
                yield break;
            }

            // 初始化AR游戏集成
            if (enableARIntegration)
            {
                var arIntegration = ARGameIntegration.Instance;
                if (arIntegration == null)
                {
                    Debug.LogWarning("无法找到ARGameIntegration组件");
                }
                else
                {
                    Debug.Log("AR游戏集成组件已初始化");
                }
            }

            // 设置调试模式
            if (enableDebugMode)
            {
                Debug.unityLogger.logEnabled = true;
                Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            }

            yield return null;
        }

        private System.Collections.IEnumerator SetupDefaultPlayer()
        {
            Debug.Log("设置默认玩家...");

            var platformManager = ScriptEchoPlatformManager.Instance;
            if (platformManager != null)
            {
                // 检查是否已有玩家数据（实际项目中应该从存储中加载）
                if (platformManager.CurrentPlayer == null)
                {
                    var defaultPlayer = new PlayerProfile(defaultPlayerName)
                    {
                        Level = defaultLevel,
                        TotalEarnings = defaultEarnings,
                        PlayStylePreference = PlayStyle.Casual
                    };

                    // 添加一些默认的偏好角色
                    defaultPlayer.PreferredRoles.Add(RoleType.Detective);
                    defaultPlayer.PreferredRoles.Add(RoleType.Witness);

                    platformManager.SetCurrentPlayer(defaultPlayer);
                    Debug.Log($"默认玩家创建成功: {defaultPlayerName}");
                }
            }

            yield return null;
        }

        private System.Collections.IEnumerator LoadPlatformScene()
        {
            Debug.Log($"加载平台场景: {platformSceneName}");

            // 检查当前场景是否已经是平台场景
            if (SceneManager.GetActiveScene().name == platformSceneName)
            {
                Debug.Log("已在平台场景中");
                yield break;
            }

            // 异步加载平台场景
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(platformSceneName);
            
            while (!loadOperation.isDone)
            {
                // 这里可以更新加载进度
                float progress = loadOperation.progress;
                Debug.Log($"场景加载进度: {progress * 100:F1}%");
                yield return null;
            }

            Debug.Log("平台场景加载完成");
        }

        /// <summary>
        /// 重启平台
        /// </summary>
        public void RestartPlatform()
        {
            Debug.Log("重启ScriptEcho平台...");
            
            isInitialized = false;
            
            // 重新加载当前场景
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 关闭平台
        /// </summary>
        public void ShutdownPlatform()
        {
            Debug.Log("关闭ScriptEcho平台...");
            
            // 保存玩家数据
            var platformManager = ScriptEchoPlatformManager.Instance;
            if (platformManager?.CurrentPlayer != null && enableAutoSave)
            {
                SavePlayerData(platformManager.CurrentPlayer);
            }

            // 清理资源
            CleanupResources();

            // 退出应用
            Application.Quit();
        }

        private void SavePlayerData(PlayerProfile player)
        {
            // 这里应该实现实际的数据保存逻辑
            // 例如保存到本地文件、云端服务器等
            Debug.Log($"保存玩家数据: {player.PlayerName}");
            
            // 示例：保存到PlayerPrefs（实际项目中应使用更可靠的存储方案）
            PlayerPrefs.SetString("PlayerName", player.PlayerName);
            PlayerPrefs.SetInt("PlayerLevel", player.Level);
            PlayerPrefs.SetString("PlayerEarnings", player.TotalEarnings.ToString());
            PlayerPrefs.SetInt("CompletedMissions", player.CompletedMissions);
            PlayerPrefs.SetFloat("TotalPlayTime", player.TotalPlayTime);
            PlayerPrefs.Save();
        }

        private void CleanupResources()
        {
            // 清理资源和取消事件订阅
            Debug.Log("清理平台资源...");
            
            // 这里可以添加具体的清理逻辑
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && enableAutoSave && isInitialized)
            {
                var platformManager = ScriptEchoPlatformManager.Instance;
                if (platformManager?.CurrentPlayer != null)
                {
                    SavePlayerData(platformManager.CurrentPlayer);
                }
            }
        }

        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && enableAutoSave && isInitialized)
            {
                var platformManager = ScriptEchoPlatformManager.Instance;
                if (platformManager?.CurrentPlayer != null)
                {
                    SavePlayerData(platformManager.CurrentPlayer);
                }
            }
        }

        /// <summary>
        /// 获取平台状态信息
        /// </summary>
        public PlatformStatus GetPlatformStatus()
        {
            return new PlatformStatus
            {
                IsInitialized = isInitialized,
                ARIntegrationEnabled = enableARIntegration,
                AutoSaveEnabled = enableAutoSave,
                DebugModeEnabled = enableDebugMode,
                CurrentSceneName = SceneManager.GetActiveScene().name
            };
        }
    }

    /// <summary>
    /// 平台状态信息
    /// </summary>
    [System.Serializable]
    public struct PlatformStatus
    {
        public bool IsInitialized;
        public bool ARIntegrationEnabled;
        public bool AutoSaveEnabled;
        public bool DebugModeEnabled;
        public string CurrentSceneName;
    }
} 