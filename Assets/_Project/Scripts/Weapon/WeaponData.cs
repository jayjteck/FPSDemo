using UnityEngine;

namespace FPSDemo.Weapon
{
    /// <summary>
    /// 一把枪的纯数据（ScriptableObject），运行时不改。
    /// 数据驱动的核心：换枪 = 换一份资产，逻辑代码完全不动。
    /// 右键 Create > FPSDemo > WeaponData 创建资产。
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponData", menuName = "FPSDemo/WeaponData")]
    public class WeaponData : ScriptableObject
    {
        [Header("标识")]
        public string id = "";
        public string displayName = "";

        [Header("伤害与射程")]
        public int damage = 10;          // 单发伤害
        public float range = 100f;       // 射程（射线最大距离，米）

        [Header("射速与弹药")]
        public float fireInterval = 0.1f; // 两发之间的间隔（秒），0.1 = 每秒 10 发
        public int magazineSize = 30;     // 弹匣容量
        public int reserveAmmo = 120;     // 备弹
        public float reloadTime = 2f;     // 换弹时长（秒）
        public bool isAuto = true;        // 全自动（按住连发）/ 半自动（每按一次一发）

        [Header("扩散")]
        public float spreadBase = 0.5f;   // 基础扩散（下限，永远存在的误差）
        public float spreadPerShot = 1f;  // 每发增加的扩散
        public float spreadMax = 8f;      // 扩散上限（连续射击封顶）
        public float spreadRecovery = 6f; // 扩散每秒恢复量

        [Header("后坐力")]
        public float recoilKick = 1f;     // 镜头后坐力强度（传给 CameraShake）

        [Header("表现层引用")]
        public GameObject muzzleFlashPrefab; // 枪口火焰（Prefab，需挂 PooledEffect）
        public GameObject impactPrefab;      // 命中火花（Prefab，需挂 PooledEffect）
        public GameObject tracerPrefab;      // 弹道拖尾（可选，MVP 暂未实现）
        public AudioClip shootSound;         // 枪声
        public AudioClip reloadSound;        // 换弹声
    }
}
