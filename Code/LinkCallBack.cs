using System;
using System.Threading;
using System.Threading.Tasks;
using Script.CommonLib.Caching;

/*
 *Simple async control 
 *usage:
 *
 * pass callback by return value 
 * 
 * caller:
 *  // do Callee=>Callee=>finish
 * 
 * Callee().SetCB(x=>{
 *		task finish x is ret
 *		return Callee();
 * }) 
 * .SetCB_End(x=>{
 *      task finish again x is ret
 * });
 *
 * callee:
 * LinkCallBack<Return Type> Callee(){
 *		var retLCB=new LinkCallBack<Return Type>();
 *      //traditional CallBack
 *		asyncOP((ret)=>{
 *			retLCB.Trigger(ret);
 *		});
 *		return retLCB;
 * }
 *
 * 
 */
namespace LinkCallBack2
{
    public class LinkCallBack<RETTYPE>:ILinkCallBack
    {
	    
        private bool isTriggered;
        private RETTYPE triggerObj;
        private Func<RETTYPE,ILinkCallBack> orgCB;
        private Func<object,ILinkCallBack> orgCB_G;
        private Func<RETTYPE,ILinkCallBack> GenCB;
        public ILinkCallBack nextLinkCB;
        private bool cbSetted;
        Func<bool> safePredict;
        private bool NonVaildContinue;
        private bool isReusable;
        private Mutex ExecCheckLock=new Mutex();
        private bool neverBeingTrigger;
        ILinkCallBackTriggerExecutor in_executor;

        public static DirtyCheckGetValue<LinkCallBack<RETTYPE>> DirectExecNull=new DirtyCheckGetValue<LinkCallBack<RETTYPE>>(
            () => {
                var lcb = new LinkCallBack<RETTYPE> ();
                lcb.isReusable = true;
                lcb.Trigger (default(RETTYPE));
                return lcb;
            });
	
	
        public static LinkCallBack<RETTYPE> DirectExec(RETTYPE obj){
            var lcb = new LinkCallBack<RETTYPE> ();
            lcb.Trigger (obj);
            return lcb;
        }
        
        ILinkCallBack commonGenCB<T>(RETTYPE x){
            GenCB = null;
            if (safePredict != null)
            {
                if (!safePredict())
                {
                    if (NonVaildContinue)
                        return LinkCallBack<T>.DirectExec(default(T));
                    return null;
                }
            }
            if(orgCB!=null) return orgCB?.Invoke(x);
            return orgCB_G?.Invoke(x);
        }
        public virtual LinkCallBack<T> SetCB<T>( Func<RETTYPE,LinkCallBack<T>> cb)
        {
	        ExecCheckLock.WaitOne();
	        orgCB = cb;
	        GenCB = commonGenCB<T>;
	        var ret= SubSetCB<T> ();
            return ret;
        } 
        public virtual LinkCallBack<T> SetCB_NonGenric<T>(Func<object, LinkCallBack<T>> cb){
	        ExecCheckLock.WaitOne();
	        orgCB_G = cb;
	        GenCB = commonGenCB<T>;
	        return SubSetCB<T> ();
        }
        protected LinkCallBack<T> SubSetCB<T>(){
			
			if (neverBeingTrigger){
				GenCB = null;
				return new LinkCallBack<T>();
			}
			if (cbSetted && !isReusable) {
				LCBCommon.Debug?.LogError ("link call back, SetCB has beed called 2+ times, non time related attr break|");
			}
			if (nextLinkCB == null ){
				
				var lnextLinkCB = new LinkCallBack<T>();
				nextLinkCB = lnextLinkCB;
				if (isReusable) lnextLinkCB.isReusable = true;
				
			}else if (nextLinkCB.GetType() != typeof(LinkCallBack<T>)){
				var oldLCB = nextLinkCB;
				var lnextLinkCB = new LinkCallBack<T>();
				if (isReusable) lnextLinkCB.isReusable = true;
				oldLCB.SetCB_NonGenric<object>(x => {
					oldLCB.Trigger_NonGenric(x);
					return null;
				});
			}
			cbSetted = true;
			//Debug.Log ("LCB NK:"+tID+"->"+nextLinkCB.tID);
			if (isTriggered) {
				ExecCheckLock.ReleaseMutex();
				SimpleExec(in_executor,triggerObj,GenCB,nextLinkCB);
			} else{
				ExecCheckLock.ReleaseMutex();
			}

			//isInstanceCB = instant;
			

			#if UNITY_EDITOR
			// if (needStackTrace){
			// 	//this will cause Editor Mode being lag when over 20000 LCB setCB in the Same Time 
			// 	mSetCBUse.cbSetter = StackTraceUtility.ExtractStackTrace();
			// }	
			#endif
			var ret= (nextLinkCB) as LinkCallBack<T>;

			if (ret == null && nextLinkCB != null){
				LCBCommon.Debug?.LogError("mSetCBUse?.nextLinkCB cast fail:"+nextLinkCB.GetType()+"|"+typeof(LinkCallBack<T>));	
			}
			//////Profiler.EndSample();
			return ret;
		}
        
        void ExecCB_instant(RETTYPE obj,Func<RETTYPE,ILinkCallBack> p_GenCB,ILinkCallBack p_nextLinkCB){
	        ILinkCallBack retcb =null;
	        try{
			
		        retcb=p_GenCB (obj);
	        }catch(Exception ex){
		        LCBCommon.Debug?.LogException(ex);
// 		        if (cbSetted) {
// #if UNITY_EDITOR
// 				Debug.LogError ("cb seted:" + mSetCBUse?.cbSetter);
// #endif
// 		        }
// 		        if (isTriggered) {
// #if UNITY_EDITOR
// 				Debug.LogError ("cb triggered:" + mTriggerCBUse?.cbTrigger);
// #endif
// 		        }
	        }

	        if (retcb != null
	        ) {
		      retcb.SetCB_NonGenric<object> (p_nextLinkCB.Trigger_NonGenric_internal<object>);
	        }
        }
        void SimpleExec(ILinkCallBackTriggerExecutor p_in_executor,RETTYPE obj,Func<RETTYPE,ILinkCallBack> p_GenCB,ILinkCallBack p_nextLinkCB)
        {
	        if (p_in_executor == null)
	        {
		        ExecCB_instant(obj,p_GenCB,p_nextLinkCB);
	        }else{
		        p_in_executor.run(() =>
		        {
			        ExecCB_instant(obj,p_GenCB,p_nextLinkCB);
		        });
	        }
        }
        
        public virtual void Trigger_NonGenric(object obj = default(object)){
	        Trigger((RETTYPE)obj);
        }
        public virtual LinkCallBack<T> Trigger_NonGenric_internal<T>(object obj = default(object))
        {
	        Trigger_NonGenric(obj);
	        return null;
        }

        public virtual void Trigger(RETTYPE obj = default(RETTYPE))
        {
	        in_trigger(obj);
        }

        void in_trigger(RETTYPE obj = default(RETTYPE))
        {
	        ExecCheckLock.WaitOne();
			if (GenCB != null)
			{
				isTriggered = true;
				triggerObj = obj;
				ExecCheckLock.ReleaseMutex();
				SimpleExec(in_executor,triggerObj,GenCB,nextLinkCB);
			} else {
				if (!isReusable && isTriggered){
					LCBCommon.Debug?.LogError("triggerOnceOnly cb triggered 2 times");
				}
				triggerObj = obj;
				isTriggered = true;
				ExecCheckLock.ReleaseMutex();
			}
        }
        public virtual void Trigger_LCBOnly(RETTYPE obj=default(RETTYPE)){
	        in_trigger ( obj);
        }
        //-------------------------base extended feature----------------------------------------
        public LinkCallBack<RETTYPE> setExecutor(ILinkCallBackTriggerExecutor executor){
	        ExecCheckLock.WaitOne();
	        in_executor = executor;
	        ExecCheckLock.ReleaseMutex();
	        return this;
        }
        
        public virtual LinkCallBack<RETTYPE> NoMoreLink(){
	        ExecCheckLock.WaitOne();
	        orgCB = null;
	        orgCB_G = null;
	        GenCB = null;
	        isTriggered = true;
	        triggerObj = default;
	        in_executor = null;
	        ExecCheckLock.ReleaseMutex();
	        return this;
        }
        public virtual  void SetCB_End( Action<RETTYPE> cb){
	        SetCB<object> ((x)=>{cb(x);return null;});
        }
        public virtual LinkCallBack<RETTYPE2> CastTo<RETTYPE2>() where RETTYPE2 : class{
	        return SetCB(x => LinkCallBack<RETTYPE2>.DirectExec(x as RETTYPE2));
        }
        public virtual LinkCallBack<RETTYPE2> Select<RETTYPE2>(Func<RETTYPE,RETTYPE2> selecter) {
	        return SetCB(x => LinkCallBack<RETTYPE2>.DirectExec(selecter(x)));
        }
        public void ToSync()
        {
	        bool tmpTriggered = false;
	        SetCB_End ((x) => {
		        tmpTriggered=true;
	        });
	        while (tmpTriggered==false)
	        {
		        Thread.Sleep(1);
			
	        }
        }
        //------------------CallBack
        public Action<RETTYPE> toCallback(){
	        Action<RETTYPE> retFn = (x) => {
		        Trigger(x);
	        };
	        return retFn;
        }
        
        public Action toCallback_NoParam(){
	        Action retFn = () => {
		        Trigger(default);
	        };
	        return retFn;
        }
        
        //---------------Event
        public static LinkCallBack<object> fromEvent_0(Action<Action> attachFN,Action<Action> removeFn){
	        var retLCB = new LinkCallBack<object>();
	        Action handleFN=()=>{};
	        handleFN = () => {
		        removeFn(handleFN);
		        retLCB.Trigger();
	        };
	        attachFN(handleFN);
	        return retLCB;
        }
        public static LinkCallBack<RETTYPE> fromEvent_1(Action<Action<RETTYPE>> attachFN,Action<Action<RETTYPE>> removeFn){
	        var retLCB = new LinkCallBack<RETTYPE>();
	        Action<RETTYPE> handleFN=(_)=>{};
	        handleFN = (x) => {
		        removeFn(handleFN);
		        retLCB.Trigger(x);
	        };
	        attachFN(handleFN);
	        return retLCB;
        }
        
        //------------------Task c#
        public static LinkCallBack<T> FromTask<T>(Task<T> ao){
	        var retLCB = new LinkCallBack<T>();
	        ao.ContinueWith(q => {
		        retLCB.Trigger(q.Result);
	        });
	        return retLCB;
        }
        public static LinkCallBack<object> FromTask(Task ao){
	        var retLCB = new LinkCallBack<object>();
	        ao.ContinueWith(q => {
		        retLCB.Trigger(null);
	        });
	        return retLCB;
        }

        public Task<RETTYPE> ToTask()
        {
	        var task=new TaskCompletionSource<RETTYPE>();
	        SetCB_End ((x) => {
		        task.SetResult(x);
	        });
	        return task.Task;
        }

    }
}