using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Runtime.InteropServices;

namespace ScriptEcho.WebGL
{
    /// <summary>
    /// WebGL AR游戏管理器 - 展览专用版本
    /// 支持WebXR和浏览器优化
    /// </summary>
    public class WebGLARGameManager : MonoBehaviour
    {
        [Header("展览配置")]
        [SerializeField] private bool exhibitionMode = true;
        [SerializeField] private float autoResetTime = 300f; // 5分钟自动重置
        [SerializeField] private int maxUsers = 10; // 最大同时用户数
        
        [Header("WebGL优化")]
        [SerializeField] private bool enableWebXR = true;
        [SerializeField] private bool fallbackToScreenSpace = true;
        [SerializeField] private int targetFrameRate = 60;
        
        [Header("游戏元素")]
        [SerializeField] private GameObject carPrefab;
        [SerializeField] private GameObject flagPrefab;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Canvas uiCanvas;
        
        [Header("UI元素")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Text statusText;
        [SerializeField] private Text instructionText;
        [SerializeField] private Slider progressSlider;
        
        // WebGL特定功能
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void RequestCameraPermission();
        
        [DllImport("__Internal")]
        private static extern bool IsCameraPermissionGranted();
        
        [DllImport("__Internal")]
        private static extern void ShowFullscreenButton();
        
        [DllImport("__Internal")]
        private static extern void EnableVibration();
#endif

        private bool gameActive = false;
        private bool carPlaced = false;
        private int targetsCollected = 0;
        private int totalTargets = 3;
        private float lastActivityTime;
        private Vector3 lastTouchPosition;
        private GameObject currentCar;
        private GameObject[] currentFlags;

        public bool IsGameActive => gameActive;
        public bool CarPlaced => carPlaced;

        void Start()
        {
            InitializeWebGLGame();
            SetupUI();
            StartCoroutine(CheckPermissions());
        }

        void Update()
        {
            // 展览模式自动重置
            if (exhibitionMode && gameActive)
            {
                if (Input.anyKeyDown || Input.touchCount > 0)
                {
                    lastActivityTime = Time.time;
                }
                
                if (Time.time - lastActivityTime > autoResetTime)
                {
                    ResetGame();
                }
            }
            
            // 处理触摸输入
            HandleInput();
        }

        /// <summary>
        /// 初始化WebGL游戏
        /// </summary>
        private void InitializeWebGLGame()
        {
            // 设置帧率
            Application.targetFrameRate = targetFrameRate;
            
            // WebGL优化设置
            QualitySettings.vSyncCount = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            // 展览模式设置
            if (exhibitionMode)
            {
                // 禁用右键菜单
#if UNITY_WEBGL && !UNITY_EDITOR
                Application.ExternalEval(@"
                    document.addEventListener('contextmenu', function(e) {
                        e.preventDefault();
                    });
                ");
#endif
            }
            
            lastActivityTime = Time.time;
        }

        /// <summary>
        /// 设置UI
        /// </summary>
        private void SetupUI()
        {
            if (startButton != null)
                startButton.onClick.AddListener(StartGame);
                
            if (resetButton != null)
                resetButton.onClick.AddListener(ResetGame);
            
            UpdateUI();
        }

        /// <summary>
        /// 检查权限
        /// </summary>
        private IEnumerator CheckPermissions()
        {
            if (statusText != null)
                statusText.text = "正在请求相机权限...";

#if UNITY_WEBGL && !UNITY_EDITOR
            RequestCameraPermission();
            
            // 等待权限授予
            float timeout = 10f;
            float elapsed = 0f;
            
            while (elapsed < timeout)
            {
                if (IsCameraPermissionGranted())
                {
                    if (statusText != null)
                        statusText.text = "✅ 相机权限已获得";
                    yield return new WaitForSeconds(1f);
                    break;
                }
                
                elapsed += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
            
            if (elapsed >= timeout)
            {
                if (statusText != null)
                    statusText.text = "❌ 需要相机权限才能体验AR";
                yield break;
            }
#endif

            // 初始化AR或回退模式
            if (enableWebXR)
            {
                InitializeWebXR();
            }
            else if (fallbackToScreenSpace)
            {
                InitializeFallbackMode();
            }
        }

        /// <summary>
        /// 初始化WebXR
        /// </summary>
        private void InitializeWebXR()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebXR初始化代码
            Application.ExternalEval(@"
                if (navigator.xr) {
                    navigator.xr.isSessionSupported('immersive-ar').then((supported) => {
                        if (supported) {
                            console.log('WebXR AR支持已启用');
                        } else {
                            console.log('WebXR AR不支持，使用回退模式');
                        }
                    });
                } else {
                    console.log('WebXR不可用，使用回退模式');
                }
            ");
#endif
            
            if (statusText != null)
                statusText.text = "🎯 WebXR AR已就绪";
        }

        /// <summary>
        /// 初始化回退模式（屏幕空间AR模拟）
        /// </summary>
        private void InitializeFallbackMode()
        {
            if (statusText != null)
                statusText.text = "📱 触摸屏幕开始游戏";
                
            if (instructionText != null)
                instructionText.text = "点击屏幕放置赛车，然后使用触摸控制驾驶";
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        private void HandleInput()
        {
            // 触摸输入
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    HandleTouch(touch.position);
                }
            }
            
            // 鼠标输入（开发测试用）
            if (Application.isEditor && Input.GetMouseButtonDown(0))
            {
                HandleTouch(Input.mousePosition);
            }
        }

        /// <summary>
        /// 处理触摸
        /// </summary>
        private void HandleTouch(Vector2 screenPosition)
        {
            if (!gameActive) return;
            
            if (!carPlaced)
            {
                PlaceCarAtScreenPosition(screenPosition);
            }
            else
            {
                // 控制车辆（简化的触摸控制）
                ControlCarWithTouch(screenPosition);
            }
        }

        /// <summary>
        /// 在屏幕位置放置车辆
        /// </summary>
        private void PlaceCarAtScreenPosition(Vector2 screenPosition)
        {
            // 将屏幕坐标转换为世界坐标
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(
                screenPosition.x, 
                screenPosition.y, 
                5f // 距离相机5米
            ));
            
            // 放置车辆
            currentCar = Instantiate(carPrefab, worldPosition, Quaternion.identity);
            carPlaced = true;
            
            // 播放反馈
            PlayFeedback();
            
            // 生成目标
            StartCoroutine(SpawnTargets());
            
            if (statusText != null)
                statusText.text = "🏎️ 赛车已放置！";
                
            if (instructionText != null)
                instructionText.text = "触摸屏幕控制赛车收集旗帜";
        }

        /// <summary>
        /// 触摸控制车辆
        /// </summary>
        private void ControlCarWithTouch(Vector2 screenPosition)
        {
            if (currentCar == null) return;
            
            // 简化的触摸控制逻辑
            Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
            Vector2 touchDirection = (screenPosition - screenCenter).normalized;
            
            // 获取车辆控制组件
            var carController = currentCar.GetComponent<Rigidbody>();
            if (carController != null)
            {
                // 基于触摸方向移动车辆
                Vector3 movement = new Vector3(touchDirection.x, 0, touchDirection.y) * 5f * Time.deltaTime;
                carController.AddForce(movement, ForceMode.VelocityChange);
            }
        }

        /// <summary>
        /// 生成目标
        /// </summary>
        private IEnumerator SpawnTargets()
        {
            currentFlags = new GameObject[totalTargets];
            
            for (int i = 0; i < totalTargets; i++)
            {
                yield return new WaitForSeconds(0.5f);
                
                // 在车辆周围随机位置生成旗帜
                Vector3 randomOffset = new Vector3(
                    Random.Range(-3f, 3f),
                    0.5f,
                    Random.Range(-3f, 3f)
                );
                
                Vector3 flagPosition = currentCar.transform.position + randomOffset;
                currentFlags[i] = Instantiate(flagPrefab, flagPosition, Quaternion.identity);
                
                // 添加碰撞检测
                var collider = currentFlags[i].GetComponent<Collider>();
                if (collider != null)
                {
                    collider.isTrigger = true;
                    var triggerHandler = currentFlags[i].AddComponent<TriggerHandler>();
                    triggerHandler.OnTriggerAction = () => CollectTarget(i);
                }
            }
        }

        /// <summary>
        /// 收集目标
        /// </summary>
        public void CollectTarget(int flagIndex)
        {
            if (flagIndex < 0 || flagIndex >= currentFlags.Length) return;
            if (currentFlags[flagIndex] == null) return;
            
            targetsCollected++;
            Destroy(currentFlags[flagIndex]);
            currentFlags[flagIndex] = null;
            
            // 播放反馈
            PlayFeedback();
            
            // 更新UI
            UpdateProgress();
            
            if (statusText != null)
                statusText.text = $"🎯 已收集 {targetsCollected}/{totalTargets}";
            
            // 检查游戏完成
            if (targetsCollected >= totalTargets)
            {
                CompleteGame();
            }
        }

        /// <summary>
        /// 开始游戏
        /// </summary>
        public void StartGame()
        {
            gameActive = true;
            lastActivityTime = Time.time;
            
            if (statusText != null)
                statusText.text = "🎮 点击屏幕放置赛车";
                
            if (startButton != null)
                startButton.gameObject.SetActive(false);
                
            if (resetButton != null)
                resetButton.gameObject.SetActive(true);
        }

        /// <summary>
        /// 完成游戏
        /// </summary>
        private void CompleteGame()
        {
            gameActive = false;
            
            if (statusText != null)
                statusText.text = "🏆 恭喜完成！";
                
            if (instructionText != null)
                instructionText.text = "点击重置开始新游戏";
            
            // 展览模式自动重置
            if (exhibitionMode)
            {
                StartCoroutine(AutoResetAfterCompletion());
            }
        }

        /// <summary>
        /// 重置游戏
        /// </summary>
        public void ResetGame()
        {
            // 清理游戏对象
            if (currentCar != null)
            {
                Destroy(currentCar);
                currentCar = null;
            }
            
            if (currentFlags != null)
            {
                for (int i = 0; i < currentFlags.Length; i++)
                {
                    if (currentFlags[i] != null)
                    {
                        Destroy(currentFlags[i]);
                    }
                }
                currentFlags = null;
            }
            
            // 重置状态
            gameActive = false;
            carPlaced = false;
            targetsCollected = 0;
            lastActivityTime = Time.time;
            
            // 更新UI
            UpdateUI();
        }

        /// <summary>
        /// 更新UI
        /// </summary>
        private void UpdateUI()
        {
            if (startButton != null)
                startButton.gameObject.SetActive(!gameActive);
                
            if (resetButton != null)
                resetButton.gameObject.SetActive(gameActive);
                
            if (statusText != null)
                statusText.text = gameActive ? "🎮 游戏进行中" : "🎯 准备开始";
                
            if (instructionText != null)
                instructionText.text = "点击开始按钮体验AR赛车";
                
            UpdateProgress();
        }

        /// <summary>
        /// 更新进度
        /// </summary>
        private void UpdateProgress()
        {
            if (progressSlider != null)
            {
                progressSlider.maxValue = totalTargets;
                progressSlider.value = targetsCollected;
            }
        }

        /// <summary>
        /// 播放反馈
        /// </summary>
        private void PlayFeedback()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            // WebGL触觉反馈
            EnableVibration();
#endif
        }

        /// <summary>
        /// 完成后自动重置
        /// </summary>
        private IEnumerator AutoResetAfterCompletion()
        {
            yield return new WaitForSeconds(10f); // 10秒后自动重置
            ResetGame();
        }
    }

    /// <summary>
    /// 触发器处理器
    /// </summary>
    public class TriggerHandler : MonoBehaviour
    {
        public System.Action OnTriggerAction;
        
        void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") || other.name.Contains("Car"))
            {
                OnTriggerAction?.Invoke();
            }
        }
    }
} 