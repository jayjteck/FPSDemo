using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;

namespace FPSDemo.Player
{
    /// <summary>
    /// 把新输入系统的 Look 动作喂给 Cinemachine 2.x 的 POV 组件。
    /// 用法：挂到 vcam 上，并把 POV 两个轴的 Input Axis Name 清空（None），Cinemachine 会自动通过本组件的 GetAxisValue 读取输入
    /// </summary>
    public class CinemachineLookInput : MonoBehaviour, AxisState.IInputAxisProvider
    {
        #region 私有字段

        [SerializeField]
        private InputActionAsset _inputActions; // 拖入 PlayerControls.inputactions
        private InputAction _lookAction;

        [SerializeField, Range(0.001f, 1f)]
        private float sensitivity = 0.05f; // 鼠标灵敏度：缩放原始像素位移，越小越稳

        #endregion

        #region 生命周期

        private void Awake()
        {
            _lookAction = _inputActions.FindActionMap("Player").FindAction("Look");
        }

        private void OnEnable() => _lookAction?.Enable();
        private void OnDisable() => _lookAction?.Disable();

        #endregion
        
        #region 公共方法
        
        /// <summary>
        /// 获取指定轴的输入值
        /// </summary>
        /// <param name="axis"></param>
        /// <returns>axis: 0 = X(左右), 1 = Y(上下), 2 = Z</returns>
        public float GetAxisValue(int axis)
        {
            if (_lookAction == null) 
                return 0f;
            
            Vector2 look = _lookAction.ReadValue<Vector2>();

            // 关键：delta 是"像素"，数值可达几十上百，直接喂给 POV 会过灵敏，
            // 乘一个很小的 sensitivity 把它压到 POV 期望的量级
            return (axis == 0 ? look.x : look.y) * sensitivity;
        }
        
        #endregion
        
    }
}
