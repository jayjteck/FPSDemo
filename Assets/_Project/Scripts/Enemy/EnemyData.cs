using UnityEngine;

namespace FPSDemo.Enemy
{
    /// <summary>
    /// 一个敌人的纯数据（ScriptableObject）。数值都进资产，不在代码里硬编码。
    /// 右键 Create > FPSDemo > EnemyData 创建资产。
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "FPSDemo/EnemyData")]
    public class EnemyData : ScriptableObject
    {
        [Header("生命")]
        public int maxHealth = 100;

        [Header("移动")]
        public float patrolSpeed = 2f;   // 巡逻速度
        public float moveSpeed = 3.5f;   // 追击速度

        [Header("攻击")]
        public int attackDamage = 10;     // 每次攻击伤害
        public float attackRange = 2f;    // 攻击距离
        public float attackCooldown = 1.5f; // 攻击间隔（秒）

        [Header("感知")]
        public float detectionRange = 15f; // 发现玩家的距离
        public float fovAngle = 120f;      // 视野角（度）
    }
}
