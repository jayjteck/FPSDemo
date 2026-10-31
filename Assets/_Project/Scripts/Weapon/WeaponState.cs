namespace FPSDemo.Weapon
{
    /// <summary>
    /// 单把枪的运行状态。MVP 阶段只用到 Idle / Reloading，
    /// Equipping 预留做切枪动画（切枪瞬间禁止开火）。
    /// </summary>
    public enum WeaponState
    {
        /// <summary>
        /// 空闲，可以开火
        /// </summary>
        Idle,
        /// <summary>
        /// 换弹中，禁止开火
        /// </summary>
        Reloading,
        /// <summary>
        /// 切枪中（预留）
        /// </summary>
        Equipping
    }
}
