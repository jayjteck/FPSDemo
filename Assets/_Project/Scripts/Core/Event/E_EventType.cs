namespace FPSDemo.Core.Event
{
    /// <summary>
    /// 事件类型枚举：所有全局事件都登记在这里
    /// </summary>
    public enum E_EventType
    {
        /// <summary>
        /// 弹药变化（AmmoInfo：当前弹匣/备弹）
        /// </summary>
        OnAmmoChanged,    
        /// <summary>
        /// 玩家血量变化（HealthInfo：当前/最大）
        /// </summary>
        OnHealthChanged, 
        /// <summary>
        /// 玩家死亡（无参）
        /// </summary>
        OnPlayerDied,     
        /// <summary>
        /// 敌人死亡（GameObject：敌人自己）
        /// </summary>
        OnEnemyDied,      
    }
}
