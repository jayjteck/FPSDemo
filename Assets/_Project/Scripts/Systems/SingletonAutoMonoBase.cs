using UnityEngine;

namespace FPSDemo.Systems
{
    /// <summary>
    /// 自动挂载式的继承MonoBehaviour的单例模式基类
    /// 推荐使用 
    /// 无需手动挂载，无需动态添加，无需关心切场景带来的问题
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SingletonAutoMonoBase<T> : MonoBehaviour where T:MonoBehaviour
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if(instance == null)
                {
                    //在场景上创建空物体
                    GameObject obj = new GameObject();
                
                    //得到T脚本的类名，为对象改名，这样在编辑器中可以明确的看到该单例模式脚本对象依附的GameObject
                    obj.name = typeof(T).ToString();
                
                    //动态挂载对应的单例模式脚本
                    instance = obj.AddComponent<T>();
                
                    //过场景时不移除对象，保证它在整个游戏生命周期中都存在
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }
    }
}
