using System;
using LinkCallBack2;


/*
 * LoopLinkCallBack: loop support of LinkCallBack
 * Usage:
 * setLoop(gameobject,Handle Loop Content)
 * new LoopLinkCallBack()
 * .setLoop<T>(go,(LoopLCB,obj,loop Count){
 * 		//do some things
* 		if(end of loop){
			LoopLCB.Loop_End(para); <--Should Call if you need to continue at loop end action,the para will pass to LoopLinkCallBack.setCB.
			return null;<--return null to stop the loop
		}
* 		//do some things
		if(continue to loop){
			//if need to go to next iteration now return DirectTriggered LCB
			return The LinkCallBack which will be triggered //ps the loop will continues when the LinkCallBack trigger , and the para will transter to obj of this
		}
 *
 * }).setCB_End((obj)=>{
 * 		//loop finish
 * });
 * 
*   stream flow:
*   xxx->LoopLCB->Handle Loop Content->[continues?] --No--> CB
 * 	             ^<---------------------<-NO-v
*action description:
* when LoopLinkCallBack has been multi-triggered, there will be n loop in the same time but ending in collapsing at one CB
* e.g:  
		   yyy
			|	/->Handle Loop Content(yyy)->[continues?] --No-|
			v	/  ^<----------loop 2----------<-NO-v          v
	xxx->LoopLCB->Handle Loop Content(xxx)->[continues?] --No--> CB
*                ^<----------loop 1----------<-NO-v
 * */
namespace LinkCallBack2
{
	public class LoopLinkCallBack<RETTYPE> : LinkCallBack<RETTYPE>
	{
		LinkCallBack<RETTYPE> midLinkCallBack;
		Func<LoopLinkCallBack<RETTYPE>, RETTYPE, int, LinkCallBack<RETTYPE>> m_loopContent;
		private bool m_isSafe;

		private ILinkCallBackTriggerExecutor m_executor;

		public LoopLinkCallBack() : base()
		{
			midLinkCallBack = new LinkCallBack<RETTYPE>();
		}

		public static LoopLinkCallBack<RETTYPE> DirectTrigger_Loop(RETTYPE para = default(RETTYPE))
		{
			var ret = new LoopLinkCallBack<RETTYPE>();
			ret.Trigger(para);
			return ret;
		}

		public LoopLinkCallBack<RETTYPE> setLoop(
			Func<LoopLinkCallBack<RETTYPE>, RETTYPE, int, LinkCallBack<RETTYPE>> loopContent,
			ILinkCallBackTriggerExecutor executor = null)
		{

			return setLoop_i(loopContent, executor);
		}




		LinkCallBack<RETTYPE> loopFnST(RETTYPE x)
		{
			return loopFn(x, 0);
		}

		LinkCallBack<RETTYPE> loopFn(RETTYPE x, int cnt)
		{
			if (m_loopContent == null) return null;
			// ////Profiler.BeginSample("LoopLinkCallBack:loopContent");
			var retcb = m_loopContent(this, x, cnt);
			// ////Profiler.EndSample();
			//////Profiler.EndSample();
			if (retcb == null)
				return null;

			retcb.setExecutor(m_executor).SetCB((y) => loopFn(y, cnt + 1));
			return null;
		}

		LoopLinkCallBack<RETTYPE> setLoop_i(
			Func<LoopLinkCallBack<RETTYPE>, RETTYPE, int, LinkCallBack<RETTYPE>> loopContent,
			ILinkCallBackTriggerExecutor executor = null)
		{

			m_loopContent = loopContent;

			m_executor = executor;


			midLinkCallBack.SetCB(loopFnST);

			return this;
		}

		public void LoopEnd(RETTYPE obj = default(RETTYPE))
		{
			m_loopContent = null;
			m_isSafe = false;
			m_executor = null;
			base.Trigger(obj);
		}

		public override void Trigger(RETTYPE obj = default(RETTYPE))
		{
			midLinkCallBack.Trigger(obj);
		}

	}


}