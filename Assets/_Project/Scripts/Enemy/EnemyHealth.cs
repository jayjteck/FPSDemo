using UnityEngine;
using FPSDemo.Core.Event;
using FPSDemo.Systems;

namespace FPSDemo.Enemy
{
    /// <summary>
    /// 敌人血量，实现 IDamageable：被武器命中后统一走 TakeDamage。
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        #region 私有字段

        private int _maxHealth;
        private int _currentHealth;

        #endregion

        #region 公共属性

        public bool IsAlive => _currentHealth > 0;
        public int CurrentHealth => _currentHealth;
        public int MaxHealth => _maxHealth;

        #endregion

        #region 公共方法

        /// <summary>初始化血量（由 EnemyAI 从 EnemyData 注入，数据驱动）</summary>
        public void Init(int maxHealth)
        {
            _maxHealth = maxHealth;
            _currentHealth = _maxHealth;
        }

        /// <summary>受击（实现 IDamageable），死亡则触发事件</summary>
        public void TakeDamage(int amount, GameObject source)
        {
            if (!IsAlive)
                return;

            _currentHealth = Mathf.Max(0, _currentHealth - amount);
            if (_currentHealth <= 0)
                Die();
        }

        #endregion

        #region 私有方法

        private void Die()
        {
            // 通过事件中心广播死亡，参数是自己（供 EnemyAI 判断是否自己、供计分统计）
            EventCenter.Instance.EventTrigger(E_EventType.OnEnemyDied, gameObject);
        }

        #endregion
    }
}
