using System;
using System.Reflection;
using UnityEngine;

namespace FPSDemo.Systems
{
    /// <summary>
    /// 不继承MonoBehaviour的单例模式基类
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonBase<T> where T:class
    {
        private static T instance;
    
        //用于加锁的对象
        protected static readonly object lockObj = new object();

        public static T Instance
        {
            get
            {
                //为了提高访问效率，在加锁前先判断单例对象是否等于null，只要不等于空就直接返回，提高访问效率
                if(instance == null)
                {
                    //为实例化对象的代码逻辑加锁
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            //利用反射，得到继承单例模式基类的类中的无参私有的构造函数，用于对象的实例化
                            //先得到继承单例模式基类的类的Type
                            Type type = typeof(T);
                        
                            //利用Type类中的 GetConstructor方法，获取继承单例模式基类的类中的私有无参构造函数
                            ConstructorInfo info = type.GetConstructor(
                                BindingFlags.Instance | BindingFlags.NonPublic,
                                null,
                                Type.EmptyTypes,
                                null
                            );
                        
                            if (info != null)
                                //利用获取的无参构造函数，去实例化继承单例模式基类的类对象
                                instance = info.Invoke(null) as T;
                            else
                                Debug.LogError("没有得到对应的无参构造函数");
                        }
                    }
                }
                return instance;
            }
        }
    }
}
