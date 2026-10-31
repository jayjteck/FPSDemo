using UnityEngine;
using UnityEngine.UI;

namespace FPSDemo.Enemy
{
    /// <summary>
    /// 敌人头顶血条：挂在敌人头顶的 World Space Canvas 上，每帧读 EnemyHealth 更新。
    /// 属于「局部一对一 UI」，直接引用同物体的 EnemyHealth（不需要走全局事件中心）。
    /// </summary>
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private EnemyHealth _health;  // 敌人身上的 EnemyHealth
        [SerializeField] private Image _fillImage;     // 血条填充（Image Type = Filled + Horizontal）

        private void LateUpdate()
        {
            if (_health == null || _fillImage == null)
                return;

            // 更新血条比例
            _fillImage.fillAmount = _health.MaxHealth > 0
                ? (float)_health.CurrentHealth / _health.MaxHealth
                : 0f;

            // 死亡后隐藏血条
            if (!_health.IsAlive)
            {
                gameObject.SetActive(false);
                return;
            }

            // 让血条始终正对相机（billboard），抵消敌人的旋转
            var cam = Camera.main;
            if (cam != null)
                transform.rotation = cam.transform.rotation;
        }
    }
}
