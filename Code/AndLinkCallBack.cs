using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/*
 * Wait All
 * AndLinkCallBack is used to  wait all branch finish
 *   trigger by your own/ other ways
 *     L  L L.....
 *     \   |   /    para of Final L ( Object of one L result)
 *     final L
 *
 *    Final will be trigger  if and only if all L are Triggered
 *     **** if addCB after FinishAddCB, all CB WILL BE ignored and log an error
 *
 *     usage :
 *     trigger with same para
 * *
 *     non trigger only wait:
 *     new AndLinkCallBack("wa").AddCB(a).AddCB(b).AddCB(c).FinishAddCB().SetCB(xxxx)
 */
namespace LinkCallBack2
{
    public class AndLinkCallBack
    {
        LinkCallBack<List<object>> finalCB;

        //-------------------------wait part
        //Lock CBIDLOCK =new ReentrantLock();
        int CBID=0;
        int nonCalledBack_Callbacks_Count=0;
        public int Get_nonCalledBack_Callbacks_Count{
            get{return nonCalledBack_Callbacks_Count; }
        }
        bool canTriggerGroupWait=false;
        List<object> Callbacks_ret_Para=new List<object>();
        int finishedCallCnt=0;
        //------------------------------------
        public AndLinkCallBack(){
            Init("");
        }
        public AndLinkCallBack(String name){
            Init(name);
        }
        void Init(String name){
            finalCB=new LinkCallBack<List<object>>();
        }

        public LinkCallBack<object> callbackRespond(ILinkCallBack orgLcb,object obj,int id){
            if (Callbacks_ret_Para [id] != null) {
                LCBCommon.Debug?.LogError ("Don't use many trigger LCB in GroupedLinkCallback"+StackTraceUtility.ExtractStackTrace());
                return null;
            }

            nonCalledBack_Callbacks_Count--;

            Callbacks_ret_Para[id]=obj;
            //Debug.Log ("callbackRespond:" );
            //orgLcb.printSetCBPos ();
            FinalTrigger();
            return null;
        }

        //warning all CB  attached in para LinkCallback cb will be removed
        public AndLinkCallBack AddCB(ILinkCallBack cb){
            if(canTriggerGroupWait){
                LCBCommon.Debug?.LogError("GroupedLinkCallback add callback after FinishAddCB is not permitted");
                return this;
            }
            int nowID=CBID;
            CBID++;
            nonCalledBack_Callbacks_Count++;
            Callbacks_ret_Para.Add(null);
            cb.SetCB_NonGenric(x=>callbackRespond(cb,x,nowID));

            return this;
        }

        public LinkCallBack<List<T>> FinishAddCB_Cast<T>(){
            return FinishAddCB().SetCB(x => LinkCallBack<List<T>>.DirectExec(x.Select(y => (T)y ).ToList()));
        }

        public LinkCallBack<List<object>> FinishAddCB(){
            canTriggerGroupWait=true;
            FinalTrigger();
            return finalCB;
        }
	

        void FinalTrigger(){
            if(!canTriggerGroupWait)return;
            if (nonCalledBack_Callbacks_Count == 0) {
                finishedCallCnt++;
                if (finishedCallCnt > 1) {
                    LCBCommon.Debug?.LogError ("GLCB: over trigger:" + StackTraceUtility.ExtractStackTrace ());
                }
                finalCB.Trigger (Callbacks_ret_Para);
            }
        }
    }
}