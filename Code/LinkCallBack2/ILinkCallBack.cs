using System;

namespace LinkCallBack2
{
    public interface ILinkCallBack
    {
        LinkCallBack<T> SetCB_NonGenric<T>(Func<object, LinkCallBack<T>> cb);
        void Trigger_NonGenric(object obj = default(object));
        LinkCallBack<T> Trigger_NonGenric_internal<T>(object obj = default(object));
    }
}