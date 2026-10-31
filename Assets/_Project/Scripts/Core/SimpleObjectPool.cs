using System.Collections.Generic;
using UnityEngine;

namespace FPSDemo.Core
{
    /// <summary>
    /// 对象池：管理高频生成/销毁的特效（枪口火焰、命中火花等）。
    /// 原则：运行时尽量别 Instantiate/Destroy 高频物体，否则触发 GC 造成卡顿（面试必考）。
    /// 用法：场景里放一个空物体挂本组件；特效 prefab 上挂 PooledEffect 实现自动归还。
    /// </summary>
    public class SimpleObjectPool : MonoBehaviour
    {
        public static SimpleObjectPool Instance { get; private set; }

        private readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
        private Transform _poolRoot;

        private void Awake()
        {
            Instance = this;

            // 所有池化物体统一挂到一个根节点下，保持层级干净
            _poolRoot = new GameObject("[ObjectPool]").transform;
            _poolRoot.SetParent(transform);
        }

        /// <summary>取一个实例：池子里有就复用，没有才实例化</summary>
        public GameObject Get(GameObject prefab)
        {
            if (prefab == null)
                return null;

            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }

            GameObject obj = null;
            // 从队列取出一个可用的（跳过可能已被外部 Destroy 的 null）
            while (queue.Count > 0 && obj == null)
                obj = queue.Dequeue();

            if (obj == null)
                obj = Instantiate(prefab, _poolRoot);

            // 把 prefab 引用记录到 PooledEffect 上，方便它到点后自行归还
            var effect = obj.GetComponent<PooledEffect>();
            if (effect != null)
                effect.SetPrefab(prefab);

            obj.name = prefab.name; // 去掉 "(Clone)" 后缀，方便调试
            obj.SetActive(true);
            return obj;
        }

        /// <summary>归还实例：SetActive(false) 挂起，等待下次复用</summary>
        public void Release(GameObject instance, GameObject prefab)
        {
            if (instance == null || prefab == null)
                return;

            instance.SetActive(false);
            instance.transform.SetParent(_poolRoot);

            if (!_pools.TryGetValue(prefab, out var queue))
            {
                queue = new Queue<GameObject>();
                _pools[prefab] = queue;
            }
            queue.Enqueue(instance);
        }

        /// <summary>预热：开局就实例化好 count 个，避免首次开枪卡顿</summary>
        public void Prewarm(GameObject prefab, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = Instantiate(prefab, _poolRoot);
                obj.SetActive(false);

                if (!_pools.TryGetValue(prefab, out var queue))
                {
                    queue = new Queue<GameObject>();
                    _pools[prefab] = queue;
                }
                queue.Enqueue(obj);
            }
        }
    }
}
