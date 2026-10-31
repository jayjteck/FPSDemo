using UnityEngine;

namespace FPSDemo.Core
{
    /// <summary>
    /// 挂在特效 prefab（枪口火焰、命中火花）上：播放一小段时间后自动归还对象池
    /// 这样开火代码只管"取"，不用自己操心"还"
    /// </summary>
    public class PooledEffect : MonoBehaviour
    {
        [SerializeField] private float _lifeTime = 0.5f; // 存活时长（秒），到点归还

        private GameObject _prefab; // 自己对应的 prefab，由 SimpleObjectPool.Get 注入
        private float _timer;

        /// <summary>由对象池注入 prefab 引用（归还时作 key）</summary>
        internal void SetPrefab(GameObject prefab) => _prefab = prefab;

        private void OnEnable()
        {
            _timer = 0f; // 每次被复用（重新激活）都从零开始计时
        }

        private void Update()
        {
            _timer += Time.deltaTime;
            if (_timer >= _lifeTime && _prefab != null && SimpleObjectPool.Instance != null)
                SimpleObjectPool.Instance.Release(gameObject, _prefab);
        }
    }
}
