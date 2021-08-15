using System;
using System.Collections;
using Script.CommonLib.Scheduling;
using UnityEngine;
using UnityEngine.Events;

namespace LinkCallBack2.Extends.Unity
{
    public static class LCB_UnityExtends
    {
        //use it safely,corotine will not stop even gameobject destroyed
        public static LinkCallBack<object> fromYield(YieldInstruction yn,ICorouteStarter starter)
        {
            var retLCB = new LinkCallBack<object>();
            starter.StartCoroutine(fromYieldCo(yn, retLCB));
            return retLCB;
        }
        //use it safely,corotine will not stop even gameobject destroyed
        public static LinkCallBack<object> fromYield(IEnumerator yn,ICorouteStarter starter){
            var retLCB = new LinkCallBack<object>();
            starter.StartCoroutine(fromYieldCo(yn, retLCB));
            return retLCB;
        }
        static IEnumerator fromYieldCo(YieldInstruction yn,LinkCallBack<object> lcb)
        {
            yield return yn;
            lcb.Trigger();
        }
        static IEnumerator fromYieldCo(IEnumerator yn,LinkCallBack<object> lcb)
        {
            yield return yn;
            lcb.Trigger();
        }
        public static Func<bool> Yield_safeFunc(GameObject go){
            return ()=>{return go!=null;};
        }


        public static IEnumerator toYield<T>(this LinkCallBack<T> lcb){
            bool tmpTriggered = false;
            lcb.SetCB_End ((x) => {
                tmpTriggered=true;
            });
            while (tmpTriggered==false)
                yield return null;
        }
        
        
        public static LinkCallBack<object> fromUnityEvent(UnityEvent evt){
            var retLCB = new LinkCallBack<object>();
            UnityAction handleFN=()=>{};
            handleFN = () => {
                evt.RemoveListener(handleFN);
                retLCB.Trigger();
            };
            evt.AddListener(handleFN);
            return retLCB;
        }
    }
}