using UnityEngine;
using UnityEngine.UI;
using TMPro;
using FPSDemo.Core.Event;
using FPSDemo.Player;
using FPSDemo.Weapon;

namespace FPSDemo.UI
{
    /// <summary>
    /// HUD：订阅事件中心更新显示（血量、弹药、击杀数、死亡提示）。
    /// 原则：UI 只订阅事件、只读一次初始值，不被业务逻辑直接调用。
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [Header("业务引用（仅用于读初始值）")]
        [SerializeField] private PlayerHealth _playerHealth;    // 拖 Player 上的 PlayerHealth
        [SerializeField] private WeaponManager _weaponManager;  // 拖 Player 上的 WeaponManager

        [Header("UI 元素")]
        [SerializeField] private Image _healthBar;     // 血条（Image Type 设 Filled + Horizontal）
        [SerializeField] private TMP_Text _healthText; // 血量数字
        [SerializeField] private TMP_Text _ammoText;   // 弹药
        [SerializeField] private TMP_Text _killText;   // 击杀数

        private int _kills;

        private void Start()
        {
            // 订阅事件中心，之后所有更新都走事件驱动
            EventCenter.Instance.AddEventListener<HealthInfo>(E_EventType.OnHealthChanged, OnHealthChanged);
            EventCenter.Instance.AddEventListener<AmmoInfo>(E_EventType.OnAmmoChanged, OnAmmoChanged);
            EventCenter.Instance.AddEventListener<GameObject>(E_EventType.OnEnemyDied, OnEnemyDied);
            EventCenter.Instance.AddEventListener(E_EventType.OnPlayerDied, OnPlayerDied);

            // 初始值：血量直接读，弹药借 RefreshAmmo 补发一次事件
            RefreshHealth(_playerHealth.CurrentHealth, _playerHealth.MaxHealth);
            _weaponManager.GetCurrentWeapon()?.RefreshAmmo();
            RefreshKills();
        }

        private void OnDestroy()
        {
            // HUD 常驻不销毁，但为规范起见仍取消订阅
            EventCenter.Instance.RemoveEventListener<HealthInfo>(E_EventType.OnHealthChanged, OnHealthChanged);
            EventCenter.Instance.RemoveEventListener<AmmoInfo>(E_EventType.OnAmmoChanged, OnAmmoChanged);
            EventCenter.Instance.RemoveEventListener<GameObject>(E_EventType.OnEnemyDied, OnEnemyDied);
            EventCenter.Instance.RemoveEventListener(E_EventType.OnPlayerDied, OnPlayerDied);
        }

        #region 事件回调

        private void OnHealthChanged(HealthInfo info) => RefreshHealth(info.current, info.max);

        private void OnAmmoChanged(AmmoInfo info)
        {
            if (_ammoText != null)
                _ammoText.text = $"{info.currentMag} / {info.reserve}";
        }

        private void OnEnemyDied(GameObject enemy)
        {
            _kills++;
            RefreshKills();
        }

        private void OnPlayerDied()
        {
            if (_healthText != null)
                _healthText.text = "你死了";
        }

        #endregion

        #region 刷新显示

        private void RefreshHealth(int current, int max)
        {
            if (_healthBar != null)
                _healthBar.fillAmount = max > 0 ? (float)current / max : 0f;
            if (_healthText != null)
                _healthText.text = $"{current} / {max}";
        }

        private void RefreshKills()
        {
            if (_killText != null)
                _killText.text = $"击杀：{_kills}";
        }

        #endregion
    }
}
