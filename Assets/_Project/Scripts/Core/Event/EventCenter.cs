using System.Collections.Generic;
using FPSDemo.Systems;
using UnityEngine.Events;

namespace FPSDemo.Core.Event
{
    /// <summary>
    /// 事件中心模块 
    /// </summary>
    public class EventCenter: SingletonBase<EventCenter>
    {
        //用字典记录所有的事件
        //字典的键是各个事件名字，字典的值是EventInfoBase（利用里氏替换原则，通过EventInfoBase去装所有的子类事件）
        private Dictionary<E_EventType, EventInfoBase> eventDic = new Dictionary<E_EventType, EventInfoBase>();

        private EventCenter() { }

        /// <summary>
        /// 触发有参数的事件
        /// </summary>
        /// <param name="eventName">事件类型枚举，通过枚举传入指定事件</param>
        /// <param name="info">要传递的参数</param>
        /// <typeparam name="T">要传递的参数类型</typeparam>
        public void EventTrigger<T>(E_EventType eventName, T info)
        {
            //如果字典中存在该事件，才去触发这个事件
            if(eventDic.ContainsKey(eventName))
            {
                //通过字典触发其中对应的事件，并将参数传过去（将EventInfoBase转换为对应的事件类）
                (eventDic[eventName] as EventInfo<T>).actions?.Invoke(info);
            }
        }

        /// <summary>
        /// 触发无参数的事件
        /// </summary>
        /// <param name="eventName">事件类型枚举，通过枚举传入指定事件</param>
        public void EventTrigger(E_EventType eventName)
        {
            //如果字典中存在该事件，才去触发这个事件
            if (eventDic.ContainsKey(eventName))
            {
                //通过字典触发其中对应的事件，并将参数传过去（将EventInfoBase转换为对应的事件类）
                (eventDic[eventName] as EventInfo).actions?.Invoke();
            }
        }


        /// <summary>
        /// 为有参数事件添加监听者
        /// </summary>
        /// <param name="eventName">事件类型枚举，通过枚举传入指定事件</param>
        /// <param name="func">监听这个事件的函数</param>
        /// <typeparam name="T">监听这个事件的函数要传的参数的类型</typeparam>
        public void AddEventListener<T>(E_EventType eventName, UnityAction<T> func)
        {
            //如果字典中存在对应的事件，就直接让传进来的函数监听这个事件
            if (eventDic.ContainsKey(eventName))
            {
                //让传进来的函数监听这个事件
                (eventDic[eventName] as EventInfo<T>).actions += func;
            }
            //如果字典中不存在对应的事件，就往字典中添加这个事件
            else
            {
                //往字典中添加这个事件，并通过构造函数将监听这个事件的函数传进去，就能让其监听这个事件了
                eventDic.Add(eventName, new EventInfo<T>(func));
            }
        }

        /// <summary>
        /// 为无参数事件添加监听者
        /// </summary>
        /// <param name="eventName">事件类型枚举，通过枚举传入指定事件</param>
        /// <param name="func">监听这个事件的函数</param>
        public void AddEventListener(E_EventType eventName, UnityAction func)
        {
            //如果字典中存在对应的事件，就直接让传进来的函数监听这个事件
            if (eventDic.ContainsKey(eventName))
            {
                //让传进来的函数监听这个事件
                (eventDic[eventName] as EventInfo).actions += func;
            }
            //如果字典中不存在对应的事件，就往字典中添加这个事件
            else
            {
                //往字典中添加这个事件，并通过构造函数将监听这个事件的函数传进去，就能让其监听这个事件了
                eventDic.Add(eventName, new EventInfo(func));
            }
        }

        /// <summary>
        /// 移除指定的有参数事件的指定监听者
        /// </summary>
        /// <param name="eventName">事件类型枚举，通过枚举传入指定事件</param>
        /// <param name="func">要从这个事件中移除的函数</param>
        public void RemoveEventListener<T>(E_EventType eventName, UnityAction<T> func)
        {
            //只有当字典中存在这个事件，才会去移除对应的函数
            if (eventDic.ContainsKey(eventName))
                (eventDic[eventName] as EventInfo<T>).actions -= func;
        }

        /// <summary>
        /// 移除指定的无参数事件的指定监听者
        /// </summary>
        /// <param name="eventName">事件类型枚举，通过枚举传入指定事件</param>
        /// <param name="func">要从这个事件中移除的函数</param>
        public void RemoveEventListener(E_EventType eventName, UnityAction func)
        {
            //只有当字典中存在这个事件，才会去移除对应的函数
            if (eventDic.ContainsKey(eventName))
                (eventDic[eventName] as EventInfo).actions -= func;
        }

        /// <summary>
        /// 清空所有事件的监听者
        /// </summary>
        public void Clear()
        {
            //将存储所有事件的字典清空
            eventDic.Clear();
        }

        /// <summary>
        /// 清除某一个指定事件的所有监听者
        /// </summary>
        /// <param name="eventName">事件类型枚举，通过枚举传入指定事件</param>
        public void Clear(E_EventType eventName)
        {
            //只当字典中存在这个事件，才会去将该事件移除
            if (eventDic.ContainsKey(eventName))
            {
                //直接将字典中对应的事件移除，就相当于清除这个事件的所有监听者了
                eventDic.Remove(eventName);
            } 
        }
    }
}

