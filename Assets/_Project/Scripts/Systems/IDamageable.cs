using UnityEngine;

namespace FPSDemo.Systems
{
    /// <summary>
    /// 伤害接口：玩家和敌人统一实现它。
    /// 射击命中后只调 TakeDamage，不关心打中的具体是谁，避免写两套伤害逻辑。
    /// </summary>
    public interface IDamageable
    {
        /// <summary>受到伤害</summary>
        /// <param name="amount">伤害数值</param>
        /// <param name="source">伤害来源物体（例如开枪的那把枪）</param>
        void TakeDamage(int amount, GameObject source);

        /// <summary>是否还活着</summary>
        bool IsAlive { get; }
    }
}
