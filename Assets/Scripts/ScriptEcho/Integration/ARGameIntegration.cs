using UnityEngine;
using UnityEngine.SceneManagement;
using ScriptEcho.Platform;
using DilmerGames.Core.Singletons;
using System.Collections;
using UnityEngine.Events;

namespace ScriptEcho.Integration
{
    /// <summary>
    /// AR游戏集成系统 - 管理AR赛车游戏与ScriptEcho平台的集成
    /// </summary>
    public class ARGameIntegration : Singleton<ARGameIntegration>
    {
        [Header("AR游戏配置")]
        [SerializeField] private string arGameSceneName = "MainGameAR_ARDK";
        [SerializeField] private string platformSceneName = "ScriptEchoPlatform";
        [SerializeField] private bool useAdditiveLoading = true;
        
        [Header("游戏状态")]
        [SerializeField] private bool isARGameActive = false;
        [SerializeField] private float gameSessionTimeout = 1800f; // 30分钟超时
        
        [Header("奖励设置")]
        [SerializeField] private int completionExperience = 100;
        [SerializeField] private decimal completionReward = 10m;
        [SerializeField] private int missionCompletionExp = 50;

        [Header("事件")]
        public UnityEvent OnARGameStarted = new UnityEvent();
        public UnityEvent OnARGameCompleted = new UnityEvent();
        public UnityEvent OnARGameExited = new UnityEvent();
        public UnityEvent<int> OnExperienceEarned = new UnityEvent<int>();

        private ScriptEchoPlatformManager platformManager;
        private PlayerMissionManager originalMissionManager;
        private CarController originalCarController;
        private GameManager originalGameManager;
        
        private Coroutine gameTimeoutCoroutine;
        private float gameStartTime;
        private bool gamePaused = false;

        public bool IsARGameActive => isARGameActive;
        public float GamePlayTime => isARGameActive ? Time.time - gameStartTime : 0f;

        void Awake()
        {
            platformManager = ScriptEchoPlatformManager.Instance;
            
            // 订阅平台事件
            if (platformManager != null)
            {
                platformManager.OnARGameLaunched.AddListener(LaunchARGame);
                platformManager.OnARGameExited.AddListener(ExitARGame);
            }
        }

        void Start()
        {
            // 查找原有的游戏组件
            FindOriginalGameComponents();
        }

        void OnDestroy()
        {
            // 取消订阅事件
            if (platformManager != null)
            {
                platformManager.OnARGameLaunched.RemoveListener(LaunchARGame);
                platformManager.OnARGameExited.RemoveListener(ExitARGame);
            }
        }

        /// <summary>
        /// 启动AR赛车游戏
        /// </summary>
        public void LaunchARGame(string sceneName = null)
        {
            if (isARGameActive)
            {
                Debug.LogWarning("AR游戏已经在运行中");
                return;
            }

            StartCoroutine(LaunchARGameCoroutine(sceneName ?? arGameSceneName));
        }

        /// <summary>
        /// 退出AR游戏
        /// </summary>
        public void ExitARGame()
        {
            if (!isARGameActive)
            {
                Debug.LogWarning("AR游戏未在运行");
                return;
            }

            StartCoroutine(ExitARGameCoroutine());
        }

        /// <summary>
        /// 暂停AR游戏
        /// </summary>
        public void PauseARGame()
        {
            if (isARGameActive && !gamePaused)
            {
                Time.timeScale = 0f;
                gamePaused = true;
                Debug.Log("AR游戏已暂停");
            }
        }

        /// <summary>
        /// 恢复AR游戏
        /// </summary>
        public void ResumeARGame()
        {
            if (isARGameActive && gamePaused)
            {
                Time.timeScale = 1f;
                gamePaused = false;
                Debug.Log("AR游戏已恢复");
            }
        }

        private IEnumerator LaunchARGameCoroutine(string sceneName)
        {
            Debug.Log($"正在启动AR游戏场景: {sceneName}");
            
            // 设置游戏状态
            isARGameActive = true;
            gameStartTime = Time.time;
            
            // 触发游戏开始事件
            OnARGameStarted?.Invoke();

            if (useAdditiveLoading)
            {
                // 使用叠加加载模式
                AsyncOperation loadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                
                while (!loadOperation.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                // 使用场景切换模式
                SceneManager.LoadScene(sceneName);
            }

            // 等待一帧确保场景完全加载
            yield return null;

            // 重新查找游戏组件
            FindOriginalGameComponents();
            
            // 绑定游戏事件
            BindGameEvents();
            
            // 开始游戏超时计时
            if (gameSessionTimeout > 0)
            {
                gameTimeoutCoroutine = StartCoroutine(GameTimeoutCoroutine());
            }

            Debug.Log("AR游戏启动完成");
        }

        private IEnumerator ExitARGameCoroutine()
        {
            Debug.Log("正在退出AR游戏");

            // 停止超时计时
            if (gameTimeoutCoroutine != null)
            {
                StopCoroutine(gameTimeoutCoroutine);
                gameTimeoutCoroutine = null;
            }

            // 恢复时间缩放
            Time.timeScale = 1f;
            gamePaused = false;

            // 取消绑定游戏事件
            UnbindGameEvents();

            // 计算游戏时长和奖励
            float playTime = GamePlayTime;
            CalculateAndAwardRewards(playTime);

            if (useAdditiveLoading)
            {
                // 卸载AR游戏场景
                AsyncOperation unloadOperation = SceneManager.UnloadSceneAsync(arGameSceneName);
                
                while (!unloadOperation.isDone)
                {
                    yield return null;
                }
            }
            else
            {
                // 切换回平台场景
                SceneManager.LoadScene(platformSceneName);
            }

            // 设置游戏状态
            isARGameActive = false;
            
            // 触发游戏退出事件
            OnARGameExited?.Invoke();
            
            // 切换平台状态回主菜单
            if (platformManager != null)
            {
                platformManager.SwitchToState(PlatformState.MainMenu);
            }

            Debug.Log($"AR游戏退出完成，游戏时长: {playTime:F1}秒");
        }

        private IEnumerator GameTimeoutCoroutine()
        {
            yield return new WaitForSeconds(gameSessionTimeout);
            
            Debug.Log("AR游戏会话超时，自动退出");
            ExitARGame();
        }

        private void FindOriginalGameComponents()
        {
            // 查找原有的游戏管理组件
            originalMissionManager = FindObjectOfType<PlayerMissionManager>();
            originalCarController = FindObjectOfType<CarController>();
            originalGameManager = FindObjectOfType<GameManager>();

            Debug.Log($"找到游戏组件 - Mission: {originalMissionManager != null}, Car: {originalCarController != null}, Game: {originalGameManager != null}");
        }

        private void BindGameEvents()
        {
            // 绑定任务管理器事件
            if (originalMissionManager != null)
            {
                originalMissionManager.OnMissionCompleted.AddListener(OnMissionCompleted);
            }

            Debug.Log("AR游戏事件绑定完成");
        }

        private void UnbindGameEvents()
        {
            // 取消绑定任务管理器事件
            if (originalMissionManager != null)
            {
                originalMissionManager.OnMissionCompleted.RemoveListener(OnMissionCompleted);
            }

            Debug.Log("AR游戏事件解绑完成");
        }

        private void OnMissionCompleted()
        {
            Debug.Log("AR赛车任务完成!");
            
            // 给玩家奖励
            if (platformManager?.CurrentPlayer != null)
            {
                platformManager.CurrentPlayer.CompleteMission("AR赛车任务", missionCompletionExp);
                platformManager.CurrentPlayer.AddEarnings(completionReward);
                
                OnExperienceEarned?.Invoke(missionCompletionExp);
            }

            // 检查是否所有任务都完成
            if (originalMissionManager != null && IsAllMissionsCompleted())
            {
                OnARGameCompleted?.Invoke();
                
                // 延迟退出游戏
                StartCoroutine(DelayedGameExit(3f));
            }
        }

        private bool IsAllMissionsCompleted()
        {
            // 检查所有目标是否达成
            if (originalMissionManager != null)
            {
                return originalMissionManager.MissionTargetCount == 0;
            }
            return false;
        }

        private IEnumerator DelayedGameExit(float delay)
        {
            yield return new WaitForSeconds(delay);
            ExitARGame();
        }

        private void CalculateAndAwardRewards(float playTime)
        {
            if (platformManager?.CurrentPlayer == null) return;

            int timeBonus = Mathf.RoundToInt(playTime / 60f) * 10; // 每分钟10经验
            int totalExp = completionExperience + timeBonus;
            
            platformManager.CurrentPlayer.AddExperience(totalExp);
            platformManager.CurrentPlayer.AddEarnings(completionReward);
            platformManager.CurrentPlayer.TotalPlayTime += playTime;
            platformManager.CurrentPlayer.TotalARGamesPlayed++;

            OnExperienceEarned?.Invoke(totalExp);

            Debug.Log($"AR游戏奖励 - 经验: {totalExp}, 金币: {completionReward}");
        }

        /// <summary>
        /// 获取当前游戏统计信息
        /// </summary>
        public ARGameStats GetCurrentGameStats()
        {
            return new ARGameStats
            {
                PlayTime = GamePlayTime,
                IsActive = isARGameActive,
                IsPaused = gamePaused,
                CompletedMissions = originalMissionManager?.CompletedMissions ?? 0
            };
        }

        /// <summary>
        /// 强制结束游戏（用于紧急情况）
        /// </summary>
        public void ForceExitGame()
        {
            if (isARGameActive)
            {
                StopAllCoroutines();
                Time.timeScale = 1f;
                isARGameActive = false;
                
                if (useAdditiveLoading)
                {
                    SceneManager.UnloadSceneAsync(arGameSceneName);
                }
                
                Debug.Log("强制退出AR游戏");
            }
        }
    }

    /// <summary>
    /// AR游戏统计信息
    /// </summary>
    [System.Serializable]
    public struct ARGameStats
    {
        public float PlayTime;
        public bool IsActive;
        public bool IsPaused;
        public int CompletedMissions;
    }
} 