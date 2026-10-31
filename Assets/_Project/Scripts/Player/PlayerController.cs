using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSDemo.Player
{
    public class PlayerController : MonoBehaviour
    {
        #region 私有字段

        [SerializeField] private InputActionAsset _inputActions; // 在 Inspector 拖入 PlayerControls.inputactions

        private CharacterController _controller; // 角色控制器
        private Camera _mainCamera;              // 场景主相机（由 CinemachineBrain 驱动）
        private float _verticalVelocity;         // 垂直速度（重力 + 跳跃）

        // 从动作资产里解析出来的动作，代码只依赖"语义"，不依赖具体按键
        private InputAction _moveAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;

        #endregion

        #region 公共字段

        [Header("移动")]
        public float moveSpeed = 5f;       // 走路速度
        public float sprintSpeed = 8f;     // 奔跑速度
        [Header("跳跃与重力")]
        public float jumpHeight = 1.5f;    // 跳跃高度（米）
        public float gravity = 20f;        // 重力加速度

        #endregion

        #region 生命周期

        private void Awake()
        {
            _controller = GetComponent<CharacterController>();
            _mainCamera = Camera.main; // 需保证主相机带 "MainCamera" 标签（默认就有）

            // 视角现在由 Cinemachine POV 处理，这里不再读 Look 动作
            InputActionMap map = _inputActions.FindActionMap("Player");
            _moveAction = map.FindAction("Move");
            _jumpAction = map.FindAction("Jump");
            _sprintAction = map.FindAction("Sprint");

            // 跳跃用事件驱动：只在"按下"那一帧回调，不轮询
            _jumpAction.performed += _ => Jump();

            // 锁定鼠标到屏幕中央并隐藏光标（FPS 标准做法）
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OnEnable() => _inputActions.Enable();   // 启用整个动作资产
        private void OnDisable() => _inputActions.Disable(); // 禁用，防止残留输入

        private void Update()
        {
            SyncBodyWithCamera(); // 身体跟随相机水平朝向，视角本身交给 Cinemachine
            Move();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 让身体朝向跟随相机的水平方向，保证按 W 朝你看的方向走（忽略俯仰）
        /// </summary>
        private void SyncBodyWithCamera()
        {
            if (_mainCamera == null) 
                return;

            Vector3 forward = _mainCamera.transform.forward;
            forward.y = 0f; // 只取水平分量
            if (forward.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.LookRotation(forward);
        }

        /// <summary>
        /// 移动：读动作值 → 计算方向 → 处理重力 → 施加位移
        /// </summary>
        private void Move()
        {
            // 从 Move 动作读二维值（键盘 WASD 或手柄摇杆，对代码透明）
            Vector2 input = _moveAction.ReadValue<Vector2>();

            // 以角色自身朝向为基准计算移动方向
            Vector3 moveDir = transform.right * input.x + transform.forward * input.y;
            moveDir = Vector3.ClampMagnitude(moveDir, 1f); // 防止斜向移动更快

            // 读 Sprint 动作（Button 类型，按下为 1，松开为 0）
            bool sprint = _sprintAction.ReadValue<float>() > 0.5f;
            float speed = sprint ? sprintSpeed : moveSpeed;

            // 落地时把垂直速度压到一个小负值，保证 isGrounded 稳定不抖动
            if (_controller.isGrounded && _verticalVelocity < 0f)
                _verticalVelocity = -2f;

            // 每帧累加重力
            _verticalVelocity -= gravity * Time.deltaTime;

            // 合成最终位移并移动（CharacterController.Move 接收的是位移，不是速度）
            Vector3 velocity = moveDir * speed;
            velocity.y = _verticalVelocity;
            _controller.Move(velocity * Time.deltaTime);
        }

        /// <summary>
        /// 跳跃：仅落地时触发，用期望高度反推起跳速度
        /// </summary>
        private void Jump()
        {
            // isGrounded 保证只有站在地面才能跳，防止无限连跳
            if (_controller.isGrounded)
            {
                // 物理公式：v = √(2gh)，由期望跳跃高度算出需要的初速度
                _verticalVelocity = Mathf.Sqrt(jumpHeight * 2f * gravity);
            }
        }

        #endregion
    }
}
