using System;

namespace LinkCallBack2
{
    public interface I_LCBDebug
    {
        void LogError(string msg, params object[] objs);
        void LogException(Exception ex);
    }
}