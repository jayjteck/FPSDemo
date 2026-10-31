using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSDemo.Weapon
{
    /// <summary>
    /// 管理多把枪的持有与切换，是玩家输入和武器之间的桥梁。
    /// 挂在 Player 上，把 Fire/Reload 输入转发给当前武器。
    /// </summary>
    public class WeaponManager : MonoBehaviour
    {
        #region 私有字段

        [SerializeField] private InputActionAsset _inputActions; // 拖入 PlayerControls.inputactions
        [SerializeField] private Camera _playerCamera;           // 主相机（射线起点）
        [SerializeField] private Weapon[] _weapons;              // 场景里挂好的每把枪（顺序）
        [SerializeField] private WeaponData[] _weaponDatas;      // 对应每把枪的数据（顺序一致）

        private InputAction _fireAction;
        private InputAction _reloadAction;
        private int _currentIndex;

        #endregion

        #region 生命周期

        private void Awake()
        {
            InputActionMap map = _inputActions.FindActionMap("Player");
            _fireAction = map.FindAction("Fire");
            _reloadAction = map.FindAction("Reload");

            // 换弹用事件驱动：只在"按下"那一帧触发（不像开火需要持续按住）
            _reloadAction.performed += _ => GetCurrentWeapon()?.StartReload();

            // 装备时注入数据：代码只依赖 WeaponData，不依赖具体哪把枪
            for (int i = 0; i < _weapons.Length; i++)
            {
                if (_weapons[i] != null && i < _weaponDatas.Length && _weaponDatas[i] != null)
                    _weapons[i].Initialize(_weaponDatas[i]);
            }

            // 初始只激活第一把枪，其余关闭
            for (int i = 0; i < _weapons.Length; i++)
            {
                if (_weapons[i] == null)
                    continue;

                bool isCurrent = (i == 0);
                _weapons[i].gameObject.SetActive(isCurrent);
                if (isCurrent)
                {
                    _weapons[i].Equip();
                    _weapons[i].RefreshAmmo(); // 让 UI 立即显示初始弹药
                }
            }
            _currentIndex = 0;
        }

        private void OnEnable()
        {
            // 只单独启用这两个动作，不重复 Enable 整个资产（整体已由 PlayerController 启用）
            _fireAction?.Enable();
            _reloadAction?.Enable();
        }

        private void OnDisable()
        {
            _fireAction?.Disable();
            _reloadAction?.Disable();
        }

        private void Update()
        {
            HandleSwitchInput();

            var weapon = GetCurrentWeapon();
            if (weapon == null || weapon.Data == null)
                return;

            // 全自动：按住连发；半自动：每次按下打一发
            bool wantFire = weapon.Data.isAuto
                ? _fireAction.IsPressed()
                : _fireAction.WasPressedThisFrame();

            if (wantFire)
                weapon.TryFire(_playerCamera);
        }

        #endregion

        #region 公共方法

        /// <summary>当前手持的武器</summary>
        public Weapon GetCurrentWeapon()
        {
            if (_weapons == null || _weapons.Length == 0)
                return null;
            return _weapons[_currentIndex];
        }

        /// <summary>切换到指定索引的武器</summary>
        public void SwitchWeapon(int index)
        {
            if (_weapons == null || _weapons.Length == 0)
                return;

            index = Mathf.Clamp(index, 0, _weapons.Length - 1);
            if (index == _currentIndex)
                return;

            // 切出旧枪：打断换弹、关闭物体
            if (_weapons[_currentIndex] != null)
            {
                _weapons[_currentIndex].Unequip();
                _weapons[_currentIndex].gameObject.SetActive(false);
            }

            _currentIndex = index;

            // 切入新枪：激活物体、立即刷新弹药显示
            var weapon = _weapons[_currentIndex];
            if (weapon != null)
            {
                weapon.gameObject.SetActive(true);
                weapon.Equip();
                weapon.RefreshAmmo(); // 让 UI 立即切换到新枪的弹药
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 切枪输入：滚轮循环切 + 数字键 1~9 直达。
        /// 用低级设备 API 图省事；正式项目可把这些也收进 .inputactions。
        /// </summary>
        private void HandleSwitchInput()
        {
            float scroll = Mouse.current != null ? Mouse.current.scroll.ReadValue().y : 0f;
            if (scroll > 0.1f)
                SwitchWeapon(_currentIndex + 1);
            else if (scroll < -0.1f)
                SwitchWeapon(_currentIndex - 1);

            for (int i = 0; i < _weapons.Length && i < 9; i++)
            {
                if (Keyboard.current != null && Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
                    SwitchWeapon(i);
            }
        }

        #endregion
    }
}
