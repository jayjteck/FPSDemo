using UnityEngine;
using UnityEngine.AI;
using FPSDemo.Core.Event;
using FPSDemo.Systems;

namespace FPSDemo.Enemy
{
    /// <summary>
    /// 敌人 AI：显式枚举 + switch 的简单状态机（Idle/Patrol/Chase/Attack/Dead）。
    /// 状态切换和攻击逻辑自己实现，导航只交给 NavMeshAgent 的 SetDestination。
    /// </summary>
    public class EnemyAI : MonoBehaviour
    {
        #region 私有字段

        [SerializeField] private EnemyData _data;
        [SerializeField] private Transform _player;          // 玩家（Inspector 拖入，也可改成按 tag 查找）
        [SerializeField] private Transform[] _patrolPoints;  // 巡逻点（可空，空则原地待机）

        private NavMeshAgent _agent;
        private EnemyHealth _health;
        private IDamageable _playerHealth;
        private EnemyState _state;
        private float _attackTimer;
        private int _patrolIndex;

        #endregion

        #region 生命周期

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
            _health = GetComponent<EnemyHealth>();

            // 把玩家的受伤接口缓存起来，攻击时直接调 TakeDamage
            if (_player != null)
                _playerHealth = _player.GetComponent<IDamageable>();
        }

        private void Start()
        {
            // 血量数值从数据资产注入（数据驱动）
            _health.Init(_data.maxHealth);

            // 订阅全局敌人死亡事件，死亡的是自己时才进入 Dead 状态
            EventCenter.Instance.AddEventListener<GameObject>(E_EventType.OnEnemyDied, HandleEnemyDied);

            // 有巡逻点就巡逻，否则原地待机
            ChangeState(_patrolPoints.Length > 0 ? EnemyState.Patrol : EnemyState.Idle);
        }

        private void Update()
        {
            UpdateState();
        }

        private void OnDestroy()
        {
            // 敌人销毁前取消订阅，避免全局事件中心持有已销毁对象的引用（内存泄漏）
            EventCenter.Instance.RemoveEventListener<GameObject>(E_EventType.OnEnemyDied, HandleEnemyDied);
        }

        #endregion

        #region 状态机

        /// <summary>按当前状态分派行为</summary>
        private void UpdateState()
        {
            switch (_state)
            {
                case EnemyState.Idle: UpdateIdle(); break;
                case EnemyState.Patrol: UpdatePatrol(); break;
                case EnemyState.Chase: UpdateChase(); break;
                case EnemyState.Attack: UpdateAttack(); break;
                case EnemyState.Dead: break; // 死亡后什么都不做
            }
        }

        /// <summary>切换状态：统一处理进入新状态时的初始化</summary>
        private void ChangeState(EnemyState newState)
        {
            if (_state == newState)
                return;

            _state = newState;

            switch (newState)
            {
                case EnemyState.Patrol:
                    _agent.speed = _data.patrolSpeed;
                    GoToNextPatrolPoint();
                    break;
                case EnemyState.Chase:
                    _agent.speed = _data.moveSpeed;
                    break;
                case EnemyState.Attack:
                    if (_agent.enabled && _agent.isOnNavMesh)
                        _agent.ResetPath();   // 停下，攻击时别乱走（未烘焙时跳过）
                    _attackTimer = 0f;        // 进入攻击立刻可以打一下
                    break;
                case EnemyState.Dead:
                    _agent.enabled = false;   // 直接禁用（不先 isStopped，避免未烘焙 NavMesh 时报错）
                    var col = GetComponent<Collider>();
                    if (col != null) col.enabled = false; // 死了不挡路、不再被射线命中
                    Destroy(gameObject, 2f);              // 2 秒后移除
                    break;
            }
        }

        #endregion

        #region 各状态逻辑

        private void UpdateIdle()
        {
            if (CanSeePlayer())
                ChangeState(EnemyState.Chase);
        }

        private void UpdatePatrol()
        {
            if (CanSeePlayer())
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            // 到达当前巡逻点就去下一个
            if (_agent.remainingDistance <= 0.5f && !_agent.pathPending)
                GoToNextPatrolPoint();
        }

        private void UpdateChase()
        {
            if (CanSeePlayer())
            {
                SetAgentDestination(_player.position);

                if (Vector3.Distance(transform.position, _player.position) <= _data.attackRange)
                    ChangeState(EnemyState.Attack);
            }
            else
            {
                // 丢失目标：回巡逻或原地待机
                ChangeState(_patrolPoints.Length > 0 ? EnemyState.Patrol : EnemyState.Idle);
            }
        }

        private void UpdateAttack()
        {
            float distance = Vector3.Distance(transform.position, _player.position);

            // 玩家跑出攻击范围 → 继续追
            if (distance > _data.attackRange)
            {
                ChangeState(EnemyState.Chase);
                return;
            }

            FacePlayer();

            _attackTimer -= Time.deltaTime;
            if (_attackTimer <= 0f)
            {
                Attack();
                _attackTimer = _data.attackCooldown;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>能否看到玩家：距离 + 视野角 + 射线遮挡 三层判断</summary>
        private bool CanSeePlayer()
        {
            if (_player == null)
                return false;

            Vector3 toPlayer = _player.position - transform.position;
            float distance = toPlayer.magnitude;

            // 1. 距离检测
            if (distance > _data.detectionRange)
                return false;

            // 2. 视野角检测：目标方向与自身 forward 的夹角 < fovAngle/2
            float angle = Vector3.Angle(transform.forward, toPlayer.normalized);
            if (angle > _data.fovAngle * 0.5f)
                return false;

            // 3. 射线遮挡检测：从眼睛高度发一条线，中间有墙就看不到
            Vector3 eyePos = transform.position + Vector3.up * 1.5f;
            Vector3 targetPos = _player.position + Vector3.up * 1.5f;
            Vector3 dir = (targetPos - eyePos).normalized;

            if (Physics.Raycast(eyePos, dir, out RaycastHit hit, distance))
            {
                // 打到的不是玩家（或其子物体）→ 被遮挡
                if (!hit.transform.IsChildOf(_player))
                    return false;
            }

            return true;
        }

        /// <summary>攻击：对玩家调统一伤害接口 TakeDamage</summary>
        private void Attack()
        {
            if (_playerHealth != null && _playerHealth.IsAlive)
                _playerHealth.TakeDamage(_data.attackDamage, gameObject);
        }

        /// <summary>面向玩家（只转水平方向）</summary>
        private void FacePlayer()
        {
            Vector3 dir = _player.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 10f);
        }

        /// <summary>安全设置导航目标：agent 不在 NavMesh 上时（未烘焙/未放置）跳过，避免报错刷屏</summary>
        private void SetAgentDestination(Vector3 pos)
        {
            if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
                _agent.SetDestination(pos);
        }

        /// <summary>去下一个巡逻点（循环）</summary>
        private void GoToNextPatrolPoint()
        {
            if (_patrolPoints.Length == 0)
                return;
            SetAgentDestination(_patrolPoints[_patrolIndex].position);
            _patrolIndex = (_patrolIndex + 1) % _patrolPoints.Length;
        }

        /// <summary>全局敌人死亡事件回调：只有自己死亡才切换状态</summary>
        private void HandleEnemyDied(GameObject enemy)
        {
            if (enemy == gameObject)
                ChangeState(EnemyState.Dead);
        }

        #endregion
    }
}
