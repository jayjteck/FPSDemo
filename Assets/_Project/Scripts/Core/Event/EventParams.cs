namespace FPSDemo.Core.Event
{
    /// <summary>
    /// 弹药变化事件参数（事件中心只支持单泛型参数，两个 int 打包成一个结构体）
    /// </summary>
    public struct AmmoInfo
    {
        public int currentMag; // 当前弹匣
        public int reserve;    // 备弹
    }

    /// <summary>
    /// 血量变化事件参数
    /// </summary>
    public struct HealthInfo
    {
        public int current; // 当前血量
        public int max;     // 最大血量
    }
}
