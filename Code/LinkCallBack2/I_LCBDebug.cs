using System;

namespace LinkCallBack2
{
    public interface I_LCBDebug
    {
        void LogError(string msg);
        void LogException(Exception ex);
    }
}