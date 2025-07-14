using UnityEngine;
using UnityEngine.InputSystem;
using DilmerGames.Core.Singletons;
using TMPro;

namespace ScriptEcho.Integration
{
    /// <summary>
    /// 移动端车辆控制器 - 专为iPhone等移动设备优化的车辆控制
    /// 支持触摸虚拟按钮、设备倾斜控制等
    /// </summary>
    public class MobileCarController : Singleton<MobileCarController>
    {
        [Header("车辆参数")]
        [SerializeField] private float speed = 1.0f;
        [SerializeField] private float torque = 1.0f;
        [SerializeField] private float minSpeedBeforeTorque = 0.3f;
        [SerializeField] private float minSpeedBeforeIdle = 0.2f;
        [SerializeField] private Rigidbody carRigidBody = null;

        [Header("移动端控制")]
        [SerializeField] private bool enableTouchControl = true;
        [SerializeField] private bool enableGyroscopeControl = true;
        [SerializeField] private bool enableVirtualButtons = true;
        [SerializeField] private float gyroSensitivity = 2.0f;
        [SerializeField] private float touchSensitivity = 1.5f;

        [Header("虚拟按钮")]
        [SerializeField] private RectTransform leftButton;
        [SerializeField] private RectTransform rightButton;
        [SerializeField] private RectTransform accelerateButton;
        [SerializeField] private RectTransform brakeButton;

        [Header("统计显示")]
        [SerializeField] private bool showStats = true;
        [SerializeField] private TextMeshProUGUI speedText = null;
        [SerializeField] private TextMeshProUGUI controlModeText = null;

        private CarWheel[] wheels;
        private int targetsCollected = 0;
        private bool isActive = false;
        
        // 控制状态
        private bool accelerate = false;
        private bool reverse = false;
        private bool turnLeft = false;
        private bool turnRight = false;
        
        // 移动端输入
        private Vector2 currentTouchInput = Vector2.zero;
        private Vector3 initialGyroAttitude;
        private bool gyroInitialized = false;
        
        // 触摸控制区域
        private RectTransform canvasRect;
        private Camera uiCamera;

        public enum ControlMode
        {
            VirtualButtons,
            TouchDrag,
            Gyroscope,
            Hybrid
        }
        
        [SerializeField] private ControlMode currentControlMode = ControlMode.Hybrid;

        public bool IsActive
        {
            get => isActive;
            set => isActive = value;
        }

        protected override void Awake()
        {
            base.Awake();
            wheels = GetComponentsInChildren<CarWheel>();
            
            // 获取Canvas组件
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                canvasRect = canvas.GetComponent<RectTransform>();
                uiCamera = canvas.worldCamera ?? Camera.main;
            }
            
            InitializeGyroscope();
        }

        void Start()
        {
            // 查找并绑定到玩家输入控制器（如果存在）
            var inputController = FindObjectOfType<PlayerInputController>();
            if (inputController != null)
            {
                inputController.Bind(this);
            }
            
            UpdateControlModeUI();
        }

        void Update()
        {
            if (!isActive) return;

            // 处理移动端输入
            HandleMobileInput();
            
            // 处理车辆移动
            HandleCarMovement();
            
            // 更新统计显示
            UpdateStats();
        }

        /// <summary>
        /// 初始化陀螺仪
        /// </summary>
        private void InitializeGyroscope()
        {
            if (enableGyroscopeControl && SystemInfo.supportsGyroscope)
            {
                Input.gyro.enabled = true;
                gyroInitialized = false; // 将在第一次使用时初始化
                Debug.Log("陀螺仪已启用");
            }
        }

        /// <summary>
        /// 处理移动端输入
        /// </summary>
        private void HandleMobileInput()
        {
            switch (currentControlMode)
            {
                case ControlMode.VirtualButtons:
                    HandleVirtualButtons();
                    break;
                    
                case ControlMode.TouchDrag:
                    HandleTouchDrag();
                    break;
                    
                case ControlMode.Gyroscope:
                    HandleGyroscope();
                    break;
                    
                case ControlMode.Hybrid:
                    HandleVirtualButtons();
                    HandleGyroscope();
                    break;
            }
        }

        /// <summary>
        /// 处理虚拟按钮输入
        /// </summary>
        private void HandleVirtualButtons()
        {
            if (!enableVirtualButtons) return;

            // 检测触摸输入
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);
                Vector2 touchPos = touch.position;
                
                // 转换为Canvas坐标
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, touchPos, uiCamera, out Vector2 localPoint);

                bool touching = touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary;
                
                // 检查每个按钮区域
                CheckButtonPress(leftButton, localPoint, touching, ref turnLeft);
                CheckButtonPress(rightButton, localPoint, touching, ref turnRight);
                CheckButtonPress(accelerateButton, localPoint, touching, ref accelerate);
                CheckButtonPress(brakeButton, localPoint, touching, ref reverse);
            }

            // 编辑器中使用鼠标测试
            if (Application.isEditor)
            {
                HandleMouseInput();
            }
        }

        /// <summary>
        /// 检查按钮按压
        /// </summary>
        private void CheckButtonPress(RectTransform button, Vector2 localPoint, bool touching, ref bool buttonState)
        {
            if (button == null) return;

            bool wasInside = RectTransformUtility.RectangleContainsScreenPoint(button, localPoint);
            buttonState = touching && wasInside;
        }

        /// <summary>
        /// 处理鼠标输入（编辑器测试用）
        /// </summary>
        private void HandleMouseInput()
        {
            Vector2 mousePos = Input.mousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect, mousePos, uiCamera, out Vector2 localPoint);

            bool clicking = Input.GetMouseButton(0);
            
            CheckButtonPress(leftButton, localPoint, clicking, ref turnLeft);
            CheckButtonPress(rightButton, localPoint, clicking, ref turnRight);
            CheckButtonPress(accelerateButton, localPoint, clicking, ref accelerate);
            CheckButtonPress(brakeButton, localPoint, clicking, ref reverse);
        }

        /// <summary>
        /// 处理触摸拖拽控制
        /// </summary>
        private void HandleTouchDrag()
        {
            if (!enableTouchControl) return;

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                
                if (touch.phase == TouchPhase.Began)
                {
                    currentTouchInput = Vector2.zero;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 deltaPosition = touch.deltaPosition * touchSensitivity;
                    currentTouchInput += deltaPosition;
                    
                    // 根据拖拽方向控制车辆
                    accelerate = currentTouchInput.y > 50f;
                    reverse = currentTouchInput.y < -50f;
                    turnLeft = currentTouchInput.x < -30f;
                    turnRight = currentTouchInput.x > 30f;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    // 重置控制状态
                    accelerate = reverse = turnLeft = turnRight = false;
                    currentTouchInput = Vector2.zero;
                }
            }
        }

        /// <summary>
        /// 处理陀螺仪控制
        /// </summary>
        private void HandleGyroscope()
        {
            if (!enableGyroscopeControl || !SystemInfo.supportsGyroscope) return;

            if (!gyroInitialized)
            {
                initialGyroAttitude = Input.gyro.attitude.eulerAngles;
                gyroInitialized = true;
                return;
            }

            Vector3 currentAttitude = Input.gyro.attitude.eulerAngles;
            Vector3 deltaAttitude = currentAttitude - initialGyroAttitude;
            
            // 处理角度环绕
            if (deltaAttitude.z > 180f) deltaAttitude.z -= 360f;
            if (deltaAttitude.z < -180f) deltaAttitude.z += 360f;

            // 根据设备倾斜角度控制转向
            float tiltThreshold = 15f;
            turnLeft = deltaAttitude.z > tiltThreshold;
            turnRight = deltaAttitude.z < -tiltThreshold;
        }

        /// <summary>
        /// 处理车辆移动
        /// </summary>
        private void HandleCarMovement()
        {
            if (carRigidBody.velocity.magnitude <= minSpeedBeforeIdle)
            {
                AddWheelsSpeed(0);
            }

            if (accelerate)
            {
                Accelerate();
            }
            if (reverse)
            {
                Reverse();
            }
            if (turnLeft)
            {
                TurnLeft();
            }
            if (turnRight)
            {
                TurnRight();
            }
        }

        /// <summary>
        /// 加速
        /// </summary>
        public void Accelerate()
        {
            carRigidBody.AddForce(transform.forward * speed, ForceMode.Acceleration);
            AddWheelsSpeed(speed);
        }

        /// <summary>
        /// 倒车
        /// </summary>
        public void Reverse()
        {
            carRigidBody.AddForce(-transform.forward * speed, ForceMode.Acceleration);
            AddWheelsSpeed(-speed);
        }

        /// <summary>
        /// 左转
        /// </summary>
        public void TurnLeft()
        {
            if (CanApplyTorque())
                carRigidBody.AddTorque(transform.up * -torque);
        }

        /// <summary>
        /// 右转
        /// </summary>
        public void TurnRight()
        {
            if (CanApplyTorque())
                carRigidBody.AddTorque(transform.up * torque);
        }

        /// <summary>
        /// 添加车轮速度
        /// </summary>
        private void AddWheelsSpeed(float speed)
        {
            foreach (var wheel in wheels)
            {
                wheel.WheelSpeed = speed;
            }
        }

        /// <summary>
        /// 检查是否可以施加扭矩
        /// </summary>
        private bool CanApplyTorque()
        {
            Vector3 velocity = carRigidBody.velocity;
            return Mathf.Abs(velocity.x) >= minSpeedBeforeTorque || Mathf.Abs(velocity.z) >= minSpeedBeforeTorque;
        }

        /// <summary>
        /// 更新统计显示
        /// </summary>
        private void UpdateStats()
        {
            if (!showStats) return;

            if (speedText != null)
            {
                // 将m/s转换为mph
                float mph = carRigidBody.velocity.magnitude * 2.236936f;
                speedText.text = $"{mph:F1} Mph";
            }
        }

        /// <summary>
        /// 更新控制模式UI
        /// </summary>
        private void UpdateControlModeUI()
        {
            if (controlModeText != null)
            {
                controlModeText.text = $"控制: {GetControlModeText()}";
            }
        }

        /// <summary>
        /// 获取控制模式文本
        /// </summary>
        private string GetControlModeText()
        {
            switch (currentControlMode)
            {
                case ControlMode.VirtualButtons: return "虚拟按钮";
                case ControlMode.TouchDrag: return "触摸拖拽";
                case ControlMode.Gyroscope: return "陀螺仪";
                case ControlMode.Hybrid: return "混合模式";
                default: return "未知";
            }
        }

        /// <summary>
        /// 切换控制模式
        /// </summary>
        public void SwitchControlMode()
        {
            int currentIndex = (int)currentControlMode;
            currentIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(ControlMode)).Length;
            currentControlMode = (ControlMode)currentIndex;
            
            UpdateControlModeUI();
            Debug.Log($"控制模式切换为: {GetControlModeText()}");
        }

        /// <summary>
        /// 设置控制模式
        /// </summary>
        public void SetControlMode(ControlMode mode)
        {
            currentControlMode = mode;
            UpdateControlModeUI();
        }

        /// <summary>
        /// 重置车辆状态
        /// </summary>
        public void ResetVehicle()
        {
            accelerate = reverse = turnLeft = turnRight = false;
            currentTouchInput = Vector2.zero;
            gyroInitialized = false;
            
            if (carRigidBody != null)
            {
                carRigidBody.velocity = Vector3.zero;
                carRigidBody.angularVelocity = Vector3.zero;
            }
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        void OnCollisionEnter(Collision other)
        {
            var placedObjectItem = other.gameObject.GetComponentInParent<PlacedObjectItem>();
            
            if (placedObjectItem != null && other.gameObject.layer == LayerMask.NameToLayer("Target"))
            {
                placedObjectItem.PlayerItem.TargetReached = true;
                other.gameObject.SetActive(false);
                
                // 通知目标收集
                var integration = MobileARGameIntegration.Instance;
                if (integration != null)
                {
                    integration.CollectTarget();
                }
                
                // 播放触觉反馈
                if (SystemInfo.deviceType == DeviceType.Handheld)
                {
                    Handheld.Vibrate();
                }
            }
        }

        /// <summary>
        /// 启用/禁用车辆控制
        /// </summary>
        public void SetControlEnabled(bool enabled)
        {
            isActive = enabled;
            if (!enabled)
            {
                ResetVehicle();
            }
        }
    }
} 