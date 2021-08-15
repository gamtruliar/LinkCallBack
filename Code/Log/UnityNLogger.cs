using System;
using LinkCallBack2;
using UnityEngine;

namespace IPGameServer.CommonLib.Log
{
    public class UnityNLogger:I_LCBDebug
    {
        public void LogError(string msg)
        {
            Debug.LogError(msg);
        }

        public void LogException(Exception ex)
        {
            Debug.LogException(ex);
        }
    }
}