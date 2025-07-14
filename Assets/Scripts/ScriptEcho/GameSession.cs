using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptEcho.Platform
{
    /// <summary>
    /// 游戏会话 - 管理一次完整的剧本杀游戏流程
    /// </summary>
    [Serializable]
    public class GameSession
    {
        [Header("会话基本信息")]
        public string SessionId;
        public string SessionName;
        public string ScriptName;
        public DateTime CreatedTime;
        public DateTime StartTime;
        public DateTime EndTime;
        public SessionStatus Status;

        [Header("参与者信息")]
        public string HostPlayerId;
        public List<SessionPlayer> Players;
        public int MaxPlayers;
        public int MinPlayers;

        [Header("游戏设置")]
        public DifficultyLevel Difficulty;
        public bool ARModeEnabled;
        public List<string> EnabledMiniGames;
        public TimeSpan EstimatedDuration;

        [Header("分账设置")]
        public RevenueShareModel RevenueModel;
        public decimal SessionPrice;
        public decimal HostSharePercentage;
        public decimal PlatformSharePercentage;

        [Header("游戏进度")]
        public int CurrentChapter;
        public List<string> CompletedObjectives;
        public Dictionary<string, object> GameData;

        public GameSession()
        {
            SessionId = Guid.NewGuid().ToString();
            CreatedTime = DateTime.Now;
            Status = SessionStatus.WaitingForPlayers;
            Players = new List<SessionPlayer>();
            EnabledMiniGames = new List<string>();
            CompletedObjectives = new List<string>();
            GameData = new Dictionary<string, object>();
        }

        public GameSession(string sessionName, string scriptName, string hostPlayerId) : this()
        {
            SessionName = sessionName;
            ScriptName = scriptName;
            HostPlayerId = hostPlayerId;
        }

        /// <summary>
        /// 添加玩家到会话
        /// </summary>
        public bool AddPlayer(PlayerProfile player, RoleType assignedRole)
        {
            if (Players.Count >= MaxPlayers)
            {
                Debug.LogWarning("会话已满，无法添加更多玩家");
                return false;
            }

            var sessionPlayer = new SessionPlayer
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                AssignedRole = assignedRole,
                JoinTime = DateTime.Now,
                IsReady = false
            };

            Players.Add(sessionPlayer);
            Debug.Log($"玩家 {player.PlayerName} 加入会话 {SessionName}，角色: {assignedRole}");
            
            CheckSessionReady();
            return true;
        }

        /// <summary>
        /// 移除玩家
        /// </summary>
        public bool RemovePlayer(string playerId)
        {
            var player = Players.Find(p => p.PlayerId == playerId);
            if (player != null)
            {
                Players.Remove(player);
                Debug.Log($"玩家 {player.PlayerName} 离开会话 {SessionName}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 开始游戏会话
        /// </summary>
        public bool StartSession()
        {
            if (Players.Count < MinPlayers)
            {
                Debug.LogWarning("玩家数量不足，无法开始游戏");
                return false;
            }

            if (!Players.TrueForAll(p => p.IsReady))
            {
                Debug.LogWarning("还有玩家未准备就绪");
                return false;
            }

            StartTime = DateTime.Now;
            Status = SessionStatus.InProgress;
            CurrentChapter = 1;
            
            Debug.Log($"游戏会话 {SessionName} 正式开始!");
            return true;
        }

        /// <summary>
        /// 结束游戏会话
        /// </summary>
        public void EndSession()
        {
            EndTime = DateTime.Now;
            Status = SessionStatus.Completed;
            
            // 计算分账
            CalculateRevenueShare();
            
            Debug.Log($"游戏会话 {SessionName} 结束");
        }

        /// <summary>
        /// 检查会话是否准备就绪
        /// </summary>
        private void CheckSessionReady()
        {
            if (Players.Count >= MinPlayers && Players.TrueForAll(p => p.IsReady))
            {
                Status = SessionStatus.Ready;
            }
        }

        /// <summary>
        /// 计算分账
        /// </summary>
        private void CalculateRevenueShare()
        {
            decimal totalRevenue = SessionPrice * Players.Count;
            decimal hostShare = totalRevenue * (HostSharePercentage / 100m);
            decimal platformShare = totalRevenue * (PlatformSharePercentage / 100m);
            decimal playersShare = totalRevenue - hostShare - platformShare;

            Debug.Log($"会话 {SessionName} 分账 - 总收入: {totalRevenue}, 主持人: {hostShare}, 平台: {platformShare}, 玩家: {playersShare}");
        }

        /// <summary>
        /// 启用AR赛车小游戏
        /// </summary>
        public void EnableARRacingGame()
        {
            if (!EnabledMiniGames.Contains("ARRacing"))
            {
                EnabledMiniGames.Add("ARRacing");
                ARModeEnabled = true;
                Debug.Log("已启用AR赛车小游戏");
            }
        }
    }

    /// <summary>
    /// 会话中的玩家信息
    /// </summary>
    [Serializable]
    public class SessionPlayer
    {
        public string PlayerId;
        public string PlayerName;
        public RoleType AssignedRole;
        public DateTime JoinTime;
        public bool IsReady;
        public int Score;
        public bool IsConnected;

        public void SetReady(bool ready)
        {
            IsReady = ready;
            Debug.Log($"玩家 {PlayerName} 准备状态: {ready}");
        }
    }

    /// <summary>
    /// 会话状态枚举
    /// </summary>
    public enum SessionStatus
    {
        WaitingForPlayers,  // 等待玩家
        Ready,              // 准备就绪
        InProgress,         // 进行中
        Paused,             // 暂停
        Completed,          // 已完成
        Cancelled           // 已取消
    }

    /// <summary>
    /// 难度等级
    /// </summary>
    public enum DifficultyLevel
    {
        Beginner,   // 初级
        Normal,     // 普通
        Hard,       // 困难
        Expert      // 专家
    }

    /// <summary>
    /// 分账模式
    /// </summary>
    public enum RevenueShareModel
    {
        FixedRate,      // 固定比例
        Performance,    // 表现分成
        Auction,        // 竞拍模式
        Subscription    // 订阅模式
    }
} 