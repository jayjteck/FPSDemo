using UnityEngine;
using FPSDemo.Core;
using FPSDemo.Core.Event;
using FPSDemo.Player;
using FPSDemo.Systems;
using Random = UnityEngine.Random;

namespace FPSDemo.Weapon
{
    /// <summary>
    /// 单把枪的运行逻辑：开火、换弹、扩散、后坐力、命中判定。
    /// 挂在一把枪的物体上，数据由 WeaponManager 在装备时通过 Initialize 注入。
    /// </summary>
    public class Weapon : MonoBehaviour
    {
        #region 私有字段

        private WeaponData _data;        // 这把枪的数据（运行时注入）
        private int _currentMag;         // 当前弹匣剩余
        private int _reserveAmmo;        // 当前备弹
        private float _reloadTimer;      // 换弹剩余时间
        private float _currentSpread;    // 当前扩散值
        private float _nextFireTime;     // 下一次可开火的时间戳（冷却）
        private WeaponState _state = WeaponState.Idle;

        [Header("表现层引用")]
        [SerializeField] private Transform _muzzlePoint;     // 枪口位置（特效起点）
        [SerializeField] private CameraShake _cameraShake;   // 开火后坐力震动（Player 上的组件）

        #endregion

        #region 公共属性

        /// <summary>当前数据，供 WeaponManager 判断全自动/半自动</summary>
        public WeaponData Data => _data;

        #endregion

        #region 生命周期

        private void Update()
        {
            // 扩散每帧恢复；换弹用计时器推进（不用协程，避免物体被禁用后状态卡死）
            UpdateSpread(Time.deltaTime);

            if (_state == WeaponState.Reloading)
            {
                _reloadTimer -= Time.deltaTime;
                if (_reloadTimer <= 0f)
                    FinishReload();
            }
        }

        #endregion

        #region 公共方法

        /// <summary>装备时初始化弹量与状态（换枪 = 换一份数据，逻辑不动）</summary>
        public void Initialize(WeaponData data)
        {
            _data = data;
            if (data == null)
                return;

            _currentMag = data.magazineSize;
            _reserveAmmo = data.reserveAmmo;
            _currentSpread = data.spreadBase;
            _state = WeaponState.Idle;
            NotifyAmmoChanged();
        }

        /// <summary>对外开火入口：满足条件才真正开火，否则忽略</summary>
        public void TryFire(Camera cam)
        {
            if (!CanFire())
                return;

            _nextFireTime = Time.time + _data.fireInterval; // 先扣冷却，防止同一帧连发
            Fire(cam);
        }

        /// <summary>能否开火：空闲状态、冷却结束、还有子弹</summary>
        public bool CanFire()
        {
            return _data != null
                && _state == WeaponState.Idle
                && Time.time >= _nextFireTime
                && _currentMag > 0;
        }

        /// <summary>进入换弹状态（弹匣已满或没备弹则忽略）</summary>
        public void StartReload()
        {
            if (_data == null
                || _state == WeaponState.Reloading
                || _currentMag >= _data.magazineSize
                || _reserveAmmo <= 0)
                return;

            _state = WeaponState.Reloading;
            _reloadTimer = _data.reloadTime;
            PlaySound(_data.reloadSound);
        }

        /// <summary>装备时调用（切枪切入）</summary>
        public void Equip()
        {
            _state = WeaponState.Idle;
            _currentSpread = _data != null ? _data.spreadBase : 0f;
        }

        /// <summary>卸载时调用（切枪切出，打断换弹）</summary>
        public void Unequip()
        {
            _state = WeaponState.Idle;
        }

        /// <summary>主动重发一次弹药事件（切枪后让 UI 立即显示新枪弹药）</summary>
        public void RefreshAmmo() => NotifyAmmoChanged();

        #endregion

        #region 私有方法

        /// <summary>真正开火：扣弹、加扩散、后坐力、特效、射线判定</summary>
        private void Fire(Camera cam)
        {
            _currentMag--;
            NotifyAmmoChanged();

            // 扩散随开火增大，但不能超过上限
            _currentSpread = Mathf.Min(_currentSpread + _data.spreadPerShot, _data.spreadMax);

            // 镜头后坐力（走 CameraShake 的 Impulse 系统）
            if (_cameraShake != null)
                _cameraShake.Shake(_data.recoilKick);

            // 枪声 + 枪口火焰
            PlaySound(_data.shootSound);
            // 枪口火焰：挂到枪口下，跟随枪（相机）移动，避免"固定在原地"的感觉
            SpawnEffect(_data.muzzleFlashPrefab, MuzzleTransform, Vector3.zero, Quaternion.identity);

            // 射线命中判定（hitscan）：从相机中心发射，保证准星即命中点
            PerformRaycast(cam);
        }

        /// <summary>从相机中心打一条射线，处理命中特效与伤害</summary>
        private void PerformRaycast(Camera cam)
        {
            Vector3 origin = cam.transform.position;
            Vector3 dir = ComputeSpreadDirection(cam);

            if (Physics.Raycast(origin, dir, out RaycastHit hit, _data.range))
            {
                var damageable = hit.collider.GetComponent<IDamageable>();
                // 临时调试：看打中了什么、有没有伤害接口
                Debug.Log($"[射击] 命中 {hit.collider.name}，有IDamageable={damageable != null}，距离={hit.distance:F1}");

                // 命中火花：世界坐标，固定在命中点（本就不该跟随）
                SpawnEffect(_data.impactPrefab, null, hit.point, Quaternion.LookRotation(hit.normal));

                // 统一走 IDamageable 接口，不关心打中的是敌人还是玩家
                if (damageable != null)
                    damageable.TakeDamage(_data.damage, gameObject);
            }
            else
            {
                Debug.Log("[射击] 未命中任何物体（打空或超出射程）");
            }
        }

        /// <summary>
        /// 扩散核心：命中方向 = 相机正前方 + 以 currentSpread 为半径的圆内随机偏移。
        /// insideUnitCircle 让散布在圆内均匀，比逐轴 Random 更接近真实弹道。
        /// </summary>
        private Vector3 ComputeSpreadDirection(Camera cam)
        {
            Vector2 offset = Random.insideUnitCircle * _currentSpread;
            Vector3 dir = cam.transform.forward
                        + cam.transform.right * offset.x
                        + cam.transform.up * offset.y;
            return dir.normalized;
        }

        /// <summary>扩散随时间线性恢复到基础值（不低过 spreadBase）</summary>
        private void UpdateSpread(float dt)
        {
            if (_data == null)
                return;
            _currentSpread = Mathf.Max(_currentSpread - _data.spreadRecovery * dt, _data.spreadBase);
        }

        /// <summary>换弹完成：从备弹补满弹匣</summary>
        private void FinishReload()
        {
            int need = _data.magazineSize - _currentMag;
            int give = Mathf.Min(need, _reserveAmmo);
            _currentMag += give;
            _reserveAmmo -= give;
            _state = WeaponState.Idle;
            NotifyAmmoChanged();
        }

        /// <summary>
        /// 从对象池取一个特效并放置（见 SimpleObjectPool）。
        /// parent 非空：挂到 parent 下，pos/rot 是相对 parent 的局部值，跟随父物体移动（枪口火焰）；
        /// parent 为空：放到世界坐标，固定不动（命中火花）。
        /// </summary>
        private void SpawnEffect(GameObject prefab, Transform parent, Vector3 pos, Quaternion rot)
        {
            if (prefab == null || SimpleObjectPool.Instance == null)
                return;

            var obj = SimpleObjectPool.Instance.Get(prefab);
            if (obj == null)
                return;

            if (parent != null)
            {
                obj.transform.SetParent(parent, false);
                obj.transform.SetLocalPositionAndRotation(pos, rot);
            }
            else
            {
                obj.transform.SetPositionAndRotation(pos, rot);
            }
        }

        /// <summary>音效：就地播放，简单够用（正式项目可走 AudioManager）</summary>
        private void PlaySound(AudioClip clip)
        {
            if (clip != null)
                AudioSource.PlayClipAtPoint(clip, MuzzlePosition);
        }

        private Vector3 MuzzlePosition => _muzzlePoint != null ? _muzzlePoint.position : transform.position;
        private Transform MuzzleTransform => _muzzlePoint != null ? _muzzlePoint : transform;

        /// <summary>通过事件中心广播弹药变化（UI 订阅事件中心即可，无需直接引用 Weapon）</summary>
        private void NotifyAmmoChanged() =>
            EventCenter.Instance.EventTrigger(E_EventType.OnAmmoChanged,
                new AmmoInfo { currentMag = _currentMag, reserve = _reserveAmmo });

        #endregion
    }
}
