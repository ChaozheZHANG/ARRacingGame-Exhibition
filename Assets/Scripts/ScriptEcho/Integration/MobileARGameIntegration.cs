using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using ScriptEcho.Platform;
using DilmerGames.Core.Singletons;
using System.Collections;
using UnityEngine.Events;
using System.Collections.Generic;

namespace ScriptEcho.Integration
{
    /// <summary>
    /// 移动端AR游戏集成系统 - 专为iPhone等移动设备优化的AR赛车游戏集成
    /// </summary>
    public class MobileARGameIntegration : Singleton<MobileARGameIntegration>
    {
        [Header("移动端AR配置")]
        [SerializeField] private ARSessionOrigin arSessionOrigin;
        [SerializeField] private ARCamera arCamera;
        [SerializeField] private ARPlaneManager planeManager;
        [SerializeField] private ARRaycastManager raycastManager;
        [SerializeField] private ARPointCloudManager pointCloudManager;
        
        [Header("游戏配置")]
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private GameObject flagPrefab;
        [SerializeField] private LayerMask placementMask = 1;
        [SerializeField] private float carPlacementHeight = 0.1f;
        
        [Header("平台集成")]
        [SerializeField] private ScriptEchoPlatformManager platformManager;
        [SerializeField] private bool enablePlatformFeatures = true;
        [SerializeField] private GameObject platformUI;
        
        [Header("游戏状态")]
        [SerializeField] private bool isGameActive = false;
        [SerializeField] private bool carPlaced = false;
        [SerializeField] private int targetsCollected = 0;
        [SerializeField] private int totalTargets = 3;
        
        [Header("移动优化")]
        [SerializeField] private int maxFrameRate = 60;
        [SerializeField] private bool optimizeForBattery = true;
        [SerializeField] private bool enableHapticFeedback = true;
        
        [Header("事件")]
        public UnityEvent OnARSessionStarted = new UnityEvent();
        public UnityEvent OnCarPlaced = new UnityEvent();
        public UnityEvent OnGameStarted = new UnityEvent();
        public UnityEvent OnGameCompleted = new UnityEvent();
        public UnityEvent<int> OnTargetCollected = new UnityEvent<int>();

        private Camera mainCamera;
        private CarController carController;
        private PlayerMissionManager missionManager;
        private bool arSessionReady = false;
        private Pose lastCarPose;

        public bool IsGameActive => isGameActive;
        public bool CarPlaced => carPlaced;
        public bool ARSessionReady => arSessionReady;
        public int TargetsCollected => targetsCollected;
        public int TotalTargets => totalTargets;

        protected override void Awake()
        {
            base.Awake();
            InitializeMobileOptimization();
            SetupARComponents();
        }

        void Start()
        {
            StartCoroutine(WaitForARSession());
            if (enablePlatformFeatures && platformManager == null)
            {
                platformManager = ScriptEchoPlatformManager.Instance;
            }
        }

        void Update()
        {
            if (arSessionReady && !carPlaced)
            {
                HandleCarPlacement();
            }
            
            if (isGameActive && carPlaced)
            {
                UpdateGameState();
            }
        }

        /// <summary>
        /// 初始化移动设备优化
        /// </summary>
        private void InitializeMobileOptimization()
        {
            // 设置目标帧率
            Application.targetFrameRate = maxFrameRate;
            
            // 优化电池使用
            if (optimizeForBattery)
            {
                QualitySettings.vSyncCount = 0;
                Screen.sleepTimeout = SleepTimeout.NeverSleep;
            }
            
            // 设置屏幕方向
            Screen.autorotateToPortrait = false;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = true;
            Screen.autorotateToLandscapeRight = true;
            Screen.orientation = ScreenOrientation.AutoRotation;
        }

        /// <summary>
        /// 设置AR组件
        /// </summary>
        private void SetupARComponents()
        {
            // 如果没有找到AR组件，尝试自动查找
            if (arSessionOrigin == null)
                arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
            
            if (arCamera == null)
                arCamera = FindObjectOfType<ARCamera>();
                
            if (planeManager == null)
                planeManager = FindObjectOfType<ARPlaneManager>();
                
            if (raycastManager == null)
                raycastManager = FindObjectOfType<ARRaycastManager>();
                
            if (pointCloudManager == null)
                pointCloudManager = FindObjectOfType<ARPointCloudManager>();

            mainCamera = Camera.main;
        }

        /// <summary>
        /// 等待AR会话启动
        /// </summary>
        private IEnumerator WaitForARSession()
        {
            yield return new WaitUntil(() => ARSession.state == ARSessionState.SessionTracking);
            
            arSessionReady = true;
            OnARSessionStarted?.Invoke();
            
            Debug.Log("AR会话已启动，可以开始放置车辆");
            ShowPlacementInstructions();
        }

        /// <summary>
        /// 处理车辆放置
        /// </summary>
        private void HandleCarPlacement()
        {
            // 检测触摸输入
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    TryPlaceCar(touch.position);
                }
            }
            
            // 编辑器中使用鼠标点击
            if (Application.isEditor && Input.GetMouseButtonDown(0))
            {
                TryPlaceCar(Input.mousePosition);
            }
        }

        /// <summary>
        /// 尝试放置车辆
        /// </summary>
        private void TryPlaceCar(Vector2 screenPosition)
        {
            if (raycastManager == null) return;

            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            
            // 进行AR射线检测
            if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                Pose hitPose = hits[0].pose;
                PlaceCarAtPosition(hitPose);
            }
        }

        /// <summary>
        /// 在指定位置放置车辆
        /// </summary>
        private void PlaceCarAtPosition(Pose pose)
        {
            if (carPlaced) return;

            // 调整车辆位置，让它稍微悬浮在平面上
            Vector3 carPosition = pose.position + Vector3.up * carPlacementHeight;
            
            // 实例化车辆
            GameObject carInstance = Instantiate(carPrefab, carPosition, pose.rotation);
            carController = carInstance.GetComponent<CarController>();
            
            // 记录位置
            lastCarPose = new Pose(carPosition, pose.rotation);
            carPlaced = true;
            
            // 触发事件
            OnCarPlaced?.Invoke();
            
            // 播放触觉反馈
            if (enableHapticFeedback)
            {
                PlayHapticFeedback();
            }
            
            // 开始生成目标
            StartCoroutine(SpawnTargetsAroundCar());
            
            Debug.Log("车辆已放置，开始游戏！");
        }

        /// <summary>
        /// 在车辆周围生成目标
        /// </summary>
        private IEnumerator SpawnTargetsAroundCar()
        {
            yield return new WaitForSeconds(1f);
            
            for (int i = 0; i < totalTargets; i++)
            {
                yield return new WaitForSeconds(0.5f);
                SpawnRandomTarget();
            }
            
            // 开始游戏
            StartGame();
        }

        /// <summary>
        /// 生成随机目标
        /// </summary>
        private void SpawnRandomTarget()
        {
            // 在车辆周围随机位置生成目标
            Vector3 randomOffset = new Vector3(
                Random.Range(-3f, 3f),
                0,
                Random.Range(-3f, 3f)
            );
            
            Vector3 targetPosition = lastCarPose.position + randomOffset;
            
            // 尝试在平面上放置
            if (TryFindPlanePosition(targetPosition, out Vector3 planePosition))
            {
                GameObject flag = Instantiate(flagPrefab, planePosition + Vector3.up * 0.1f, Quaternion.identity);
                
                // 添加目标组件
                var targetItem = flag.AddComponent<PlacedObjectItem>();
                targetItem.PlayerItem = new PlayerItem
                {
                    ItemType = ItemType.Flag,
                    PlacementState = PlacementState.Placed
                };
                
                flag.layer = LayerMask.NameToLayer("Target");
            }
        }

        /// <summary>
        /// 尝试在平面上找到位置
        /// </summary>
        private bool TryFindPlanePosition(Vector3 worldPosition, out Vector3 planePosition)
        {
            planePosition = worldPosition;
            
            // 将世界坐标转换为屏幕坐标
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPosition);
            
            List<ARRaycastHit> hits = new List<ARRaycastHit>();
            if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            {
                planePosition = hits[0].pose.position;
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        private void StartGame()
        {
            isGameActive = true;
            OnGameStarted?.Invoke();
            
            // 更新平台状态
            if (enablePlatformFeatures && platformManager != null)
            {
                platformManager.SwitchToState(PlatformState.ARGame);
            }
            
            Debug.Log("AR赛车游戏开始！");
        }

        /// <summary>
        /// 更新游戏状态
        /// </summary>
        private void UpdateGameState()
        {
            // 检查是否完成所有目标
            if (targetsCollected >= totalTargets)
            {
                CompleteGame();
            }
        }

        /// <summary>
        /// 收集目标
        /// </summary>
        public void CollectTarget()
        {
            targetsCollected++;
            OnTargetCollected?.Invoke(targetsCollected);
            
            // 播放触觉反馈
            if (enableHapticFeedback)
            {
                PlayHapticFeedback();
            }
            
            Debug.Log($"目标已收集！进度：{targetsCollected}/{totalTargets}");
        }

        /// <summary>
        /// 完成游戏
        /// </summary>
        private void CompleteGame()
        {
            isGameActive = false;
            OnGameCompleted?.Invoke();
            
            // 更新平台数据
            if (enablePlatformFeatures && platformManager != null)
            {
                var player = platformManager.CurrentPlayer;
                if (player != null)
                {
                    player.TotalARGamesPlayed++;
                    player.CompletedMissions++;
                    player.AddExperience(100);
                }
                
                platformManager.SwitchToState(PlatformState.MainMenu);
            }
            
            Debug.Log("恭喜！AR赛车游戏完成！");
            ShowGameCompletedUI();
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public void ResetGame()
        {
            // 清理现有对象
            CleanupGameObjects();
            
            // 重置状态
            carPlaced = false;
            isGameActive = false;
            targetsCollected = 0;
            
            Debug.Log("游戏已重置");
        }

        /// <summary>
        /// 清理游戏对象
        /// </summary>
        private void CleanupGameObjects()
        {
            // 清理车辆
            if (carController != null)
            {
                Destroy(carController.gameObject);
                carController = null;
            }
            
            // 清理目标
            GameObject[] targets = GameObject.FindGameObjectsWithTag("Target");
            foreach (GameObject target in targets)
            {
                Destroy(target);
            }
        }

        /// <summary>
        /// 播放触觉反馈
        /// </summary>
        private void PlayHapticFeedback()
        {
            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                Handheld.Vibrate();
            }
        }

        /// <summary>
        /// 显示放置说明
        /// </summary>
        private void ShowPlacementInstructions()
        {
            // 这里可以显示UI指导用户如何放置车辆
            Debug.Log("点击屏幕上的平面来放置你的赛车！");
        }

        /// <summary>
        /// 显示游戏完成UI
        /// </summary>
        private void ShowGameCompletedUI()
        {
            // 这里可以显示游戏完成的UI
            Debug.Log("游戏完成！点击重新开始来再玩一次。");
        }

        /// <summary>
        /// 获取游戏统计
        /// </summary>
        public string GetGameStats()
        {
            return $"目标收集: {targetsCollected}/{totalTargets}\n" +
                   $"游戏状态: {(isGameActive ? "进行中" : "未开始")}\n" +
                   $"车辆已放置: {(carPlaced ? "是" : "否")}";
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // 处理应用暂停/恢复
            if (pauseStatus && isGameActive)
            {
                Debug.Log("游戏暂停");
            }
            else if (!pauseStatus && isGameActive)
            {
                Debug.Log("游戏恢复");
            }
        }

        void OnDestroy()
        {
            CleanupGameObjects();
        }
    }
} 