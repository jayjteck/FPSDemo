using UnityEngine;
using Cinemachine;

namespace FPSDemo.Player
{
    /// <summary>
    /// 相机震动：用 Cinemachine 的 Impulse 系统做开枪/受击反馈。
    /// 挂在 Player（或任意物体）上，把 Impulse Source 拖进槽里
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        #region 私有方法

        [SerializeField] 
        private CinemachineImpulseSource _impulseSource;

        #endregion

        #region 公共方法

        /// <summary>
        /// 触发相机震动，intensity 越大震得越强
        /// </summary>
        public void Shake(float intensity = 1f)
        {
            if (_impulseSource != null)
                _impulseSource.GenerateImpulse(intensity * Vector3.up);
        }

        #endregion
    }
}
