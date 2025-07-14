using System;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptEcho.Platform
{
    /// <summary>
    /// 玩家配置文件 - 存储玩家的基本信息和游戏数据
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        [Header("基本信息")]
        public string PlayerId;
        public string PlayerName;
        public string AvatarUrl;
        public int Level;
        public int ExperiencePoints;

        [Header("角色偏好")]
        public List<RoleType> PreferredRoles;
        public PlayStyle PlayStylePreference;

        [Header("游戏统计")]
        public int TotalGamesPlayed;
        public int TotalARGamesPlayed;
        public float TotalPlayTime;
        public int CompletedMissions;

        [Header("分账信息")]
        public decimal TotalEarnings;
        public PaymentMethod PreferredPaymentMethod;
        public string PaymentAccount;

        [Header("社交")]
        public List<string> FriendsList;
        public int ReputationScore;
        public List<string> Achievements;

        public PlayerProfile()
        {
            PlayerId = Guid.NewGuid().ToString();
            PreferredRoles = new List<RoleType>();
            FriendsList = new List<string>();
            Achievements = new List<string>();
            Level = 1;
            ReputationScore = 100;
        }

        public PlayerProfile(string playerName) : this()
        {
            PlayerName = playerName;
        }

        /// <summary>
        /// 添加经验值
        /// </summary>
        public void AddExperience(int exp)
        {
            ExperiencePoints += exp;
            CheckLevelUp();
        }

        /// <summary>
        /// 检查是否升级
        /// </summary>
        private void CheckLevelUp()
        {
            int requiredExp = Level * 100; // 简单的升级公式
            if (ExperiencePoints >= requiredExp)
            {
                Level++;
                Debug.Log($"玩家 {PlayerName} 升级到 {Level} 级!");
            }
        }

        /// <summary>
        /// 添加收益
        /// </summary>
        public void AddEarnings(decimal amount)
        {
            TotalEarnings += amount;
        }

        /// <summary>
        /// 完成任务
        /// </summary>
        public void CompleteMission(string missionName, int expReward)
        {
            CompletedMissions++;
            AddExperience(expReward);
            Debug.Log($"玩家 {PlayerName} 完成任务: {missionName}");
        }
    }

    /// <summary>
    /// 角色类型枚举
    /// </summary>
    public enum RoleType
    {
        Detective,      // 侦探
        Suspect,        // 嫌疑人
        Witness,        // 证人
        Victim,         // 受害者
        Support,        // 支援角色
        Neutral         // 中立角色
    }

    /// <summary>
    /// 游戏风格枚举
    /// </summary>
    public enum PlayStyle
    {
        Casual,         // 休闲
        Competitive,    // 竞技
        Social,         // 社交
        Immersive       // 沉浸式
    }

    /// <summary>
    /// 支付方式枚举
    /// </summary>
    public enum PaymentMethod
    {
        WeChat,         // 微信
        Alipay,         // 支付宝
        BankCard,       // 银行卡
        Digital         // 数字货币
    }
} 