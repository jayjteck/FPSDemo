using UnityEngine;
using FPSDemo.Core.Event;
using FPSDemo.Systems;

namespace FPSDemo.Player
{
    /// <summary>
    /// 玩家血量，实现 IDamageable：被敌人攻击后统一走 TakeDamage。
    /// </summary>
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        #region 私有字段

        [SerializeField] private int _maxHealth = 100;
        private int _currentHealth;

        #endregion

        #region 公共属性

        public bool IsAlive => _currentHealth > 0;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;

        #endregion

        #region 生命周期

        private void Awake()
        {
            _currentHealth = _maxHealth;
        }

        #endregion

        #region 公共方法

        /// <summary>受击（实现 IDamageable），死亡则触发事件</summary>
        public void TakeDamage(int amount, GameObject source)
        {
            if (!IsAlive)
                return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            EventCenter.Instance.EventTrigger(E_EventType.OnHealthChanged,
                new HealthInfo { current = _currentHealth, max = _maxHealth });

            if (_currentHealth <= 0)
                Die();
        }

        #endregion

        #region 私有方法

        private void Die()
        {
            EventCenter.Instance.EventTrigger(E_EventType.OnPlayerDied);

            // MVP 简单处理：解锁鼠标、禁用移动（死亡画面/重开后续由 GameManager 做）
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var controller = GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = false;
        }

        #endregion
    }
}
