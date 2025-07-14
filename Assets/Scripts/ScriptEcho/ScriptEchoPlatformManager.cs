using System;
using System.Collections.Generic;
using DilmerGames.Core.Singletons;
using UnityEngine;
using UnityEngine.Events;

namespace ScriptEcho.Platform
{
    /// <summary>
    /// ScriptEcho平台管理器 - 管理AR剧本杀平台的核心功能
    /// 包括角色匹配、组局、分账、AR小游戏等功能
    /// </summary>
    public class ScriptEchoPlatformManager : Singleton<ScriptEchoPlatformManager>
    {
        [Header("平台状态")]
        [SerializeField]
        private PlatformState currentState = PlatformState.MainMenu;
        
        [Header("平台模块")]
        [SerializeField]
        private GameObject mainMenuUI;
        [SerializeField]
        private GameObject roleMatchingUI;
        [SerializeField]
        private GameObject gameSessionUI;
        [SerializeField]
        private GameObject arGameUI;
        [SerializeField]
        private GameObject revenueShareUI;

        [Header("AR游戏管理")]
        [SerializeField]
        private string arRacingGameScene = "MainGameAR_ARDK";
        
        [Header("事件")]
        public UnityEvent<PlatformState> OnStateChanged = new UnityEvent<PlatformState>();
        public UnityEvent<string> OnARGameLaunched = new UnityEvent<string>();
        public UnityEvent OnARGameExited = new UnityEvent();

        private Dictionary<PlatformState, GameObject> uiPanels;
        private PlayerProfile currentPlayer;
        private GameSession currentSession;

        public PlatformState CurrentState => currentState;
        public PlayerProfile CurrentPlayer => currentPlayer;
        public GameSession CurrentSession => currentSession;

        void Awake()
        {
            InitializeUIPanels();
            InitializePlatform();
        }

        void Start()
        {
            SwitchToState(PlatformState.MainMenu);
        }

        private void InitializeUIPanels()
        {
            uiPanels = new Dictionary<PlatformState, GameObject>
            {
                { PlatformState.MainMenu, mainMenuUI },
                { PlatformState.RoleMatching, roleMatchingUI },
                { PlatformState.GameSession, gameSessionUI },
                { PlatformState.ARGame, arGameUI },
                { PlatformState.RevenueShare, revenueShareUI }
            };
        }

        private void InitializePlatform()
        {
            // 初始化平台数据
            Debug.Log("ScriptEcho平台初始化完成");
        }

        /// <summary>
        /// 切换平台状态
        /// </summary>
        public void SwitchToState(PlatformState newState)
        {
            if (currentState == newState) return;

            // 隐藏当前UI
            if (uiPanels.ContainsKey(currentState) && uiPanels[currentState] != null)
            {
                uiPanels[currentState].SetActive(false);
            }

            // 更新状态
            PlatformState oldState = currentState;
            currentState = newState;

            // 显示新UI
            if (uiPanels.ContainsKey(newState) && uiPanels[newState] != null)
            {
                uiPanels[newState].SetActive(true);
            }

            // 触发状态变化事件
            OnStateChanged?.Invoke(newState);
            
            Debug.Log($"平台状态从 {oldState} 切换到 {newState}");
        }

        /// <summary>
        /// 启动AR赛车游戏
        /// </summary>
        public void LaunchARRacingGame()
        {
            Debug.Log("启动AR赛车游戏");
            SwitchToState(PlatformState.ARGame);
            OnARGameLaunched?.Invoke(arRacingGameScene);
            
            // 这里可以加载AR赛车游戏场景
            LoadARGameScene(arRacingGameScene);
        }

        /// <summary>
        /// 退出AR游戏
        /// </summary>
        public void ExitARGame()
        {
            Debug.Log("退出AR游戏");
            OnARGameExited?.Invoke();
            SwitchToState(PlatformState.MainMenu);
        }

        /// <summary>
        /// 开始角色匹配
        /// </summary>
        public void StartRoleMatching()
        {
            Debug.Log("开始角色匹配");
            SwitchToState(PlatformState.RoleMatching);
        }

        /// <summary>
        /// 开始游戏会话
        /// </summary>
        public void StartGameSession(GameSession session)
        {
            currentSession = session;
            SwitchToState(PlatformState.GameSession);
            Debug.Log($"开始游戏会话: {session.SessionName}");
        }

        /// <summary>
        /// 打开分账界面
        /// </summary>
        public void OpenRevenueShare()
        {
            SwitchToState(PlatformState.RevenueShare);
        }

        /// <summary>
        /// 设置当前玩家
        /// </summary>
        public void SetCurrentPlayer(PlayerProfile player)
        {
            currentPlayer = player;
            Debug.Log($"当前玩家设置为: {player.PlayerName}");
        }

        private void LoadARGameScene(string sceneName)
        {
            // 这里可以使用Unity的场景加载来加载AR游戏
            // UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            Debug.Log($"加载AR游戏场景: {sceneName}");
        }
    }

    /// <summary>
    /// 平台状态枚举
    /// </summary>
    public enum PlatformState
    {
        MainMenu,        // 主菜单
        RoleMatching,    // 角色匹配
        GameSession,     // 游戏会话
        ARGame,          // AR游戏
        RevenueShare     // 分账
    }
} 