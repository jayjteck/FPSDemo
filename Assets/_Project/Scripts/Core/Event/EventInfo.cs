using UnityEngine.Events;

namespace FPSDemo.Core.Event
{
    /// <summary>
    /// 该类用于包裹有参数的事件
    /// </summary>
    /// <typeparam name="T">要传的参数类型</typeparam>
    public class EventInfo<T>:EventInfoBase
    {
        //被这个类包裹的委托变量，所有监听者都是监听这个事件（委托）
        public UnityAction<T> actions;

        //通过构造函数，为该事件添加监听者
        public EventInfo(UnityAction<T> action)
        {
            actions += action;
        }
    }

    /// <summary>
    /// 该类用于包裹无参数的事件
    /// </summary>
    public class EventInfo: EventInfoBase
    {
        //被这个类包裹的委托变量，所有监听者都是监听这个事件（委托）
        public UnityAction actions;
    
        //通过构造函数，为该事件添加监听者
        public EventInfo(UnityAction action)
        {
            actions += action;
        }
    }
}

