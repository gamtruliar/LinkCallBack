/*
 * 
 */
using LinkCallBack2;


/**
 * Race
 * OrLinkCallBack is friend of AndLinkCallBack used to  wait until one branch finish
 *   trigger by your own/ other ways
 *     L  L L.....
 *     \   |   /    para of Final L ( Object of one L result)
 *     final L
 *
 *    Final will be trigger  if and only if one L are Triggered
 *     **** if addCB after FinishAddCB, all CB WILL BE ignored and log an error
 *
 *     usage :
 *     trigger with same para
 * *
 *     non trigger only wait:
 *     new OrLinkCallBack("wa").AddCB(a).AddCB(b).AddCB(c).FinishAddCB().SetCB(xxxx)
 */
namespace LinkCallBack2
{
	public class OrLinkCallBack
	{
		LinkCallBack<object> finalCB;

		private bool someTriggered = false;

		//-------------------------wait part
		//Lock CBIDLOCK =new ReentrantLock();
		int CBID = 0;


		bool canTriggerGroupWait = false;
		object Callbacks_ret_Para = null;

		int finishedCallCnt = 0;

		//------------------------------------
		public OrLinkCallBack()
		{
			Init("");
		}

		public OrLinkCallBack(string name)
		{
			Init(name);
		}

		void Init(string name)
		{
			finalCB = new LinkCallBack<object>();
		}

		public LinkCallBack<object> callbackRespond(ILinkCallBack orgLcb, object obj, int id)
		{
			if (finalCB == null)
				return null;
			//Debug.LogError ("OrLinkCallBack: cb back"+StackTraceUtility.ExtractStackTrace() );
			Callbacks_ret_Para = obj;
			someTriggered = true;
			//Debug.Log ("callbackRespond:" );
			//orgLcb.printSetCBPos ();
			FinalTrigger();
			return null;
		}

		//warning all CB  attached in para LinkCallback cb will be removed
		public OrLinkCallBack AddCB(ILinkCallBack cb)
		{
			if (canTriggerGroupWait)
			{
				LCBCommon.Debug?.LogError("GroupedLinkCallback add callback after FinishAddCB is not permitted");
				return this;
			}

			//canTriggerGroupWait = false;
			//CBIDLOCK.lock();
			int nowID = CBID;
			CBID++;

			cb.SetCB_NonGenric(x => callbackRespond(cb, x, nowID));
			return this;
		}

		public LinkCallBack<object> FinishAddCB()
		{
			canTriggerGroupWait = true;
			var retcb = finalCB;
			FinalTrigger();
			return retcb;
		}

		//LCB OBJ:any one x of AddCB(x)
		void FinalTrigger()
		{
			if (!canTriggerGroupWait) return;
			if (finalCB != null && someTriggered)
			{
				finishedCallCnt++;
				if (finishedCallCnt > 1)
				{
					LCBCommon.Debug?.LogError("OLCB: over trigger:");
				}

				var tmpcb = finalCB;
				finalCB = null;
				tmpcb.Trigger(Callbacks_ret_Para);
			}
		}
	}
}