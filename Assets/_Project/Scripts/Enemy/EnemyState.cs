namespace FPSDemo.Enemy
{
    /// <summary>敌人 AI 状态</summary>
    public enum EnemyState
    {
        Idle,    // 原地待机
        Patrol,  // 巡逻
        Chase,   // 追击
        Attack,  // 攻击
        Dead     // 死亡
    }
}
