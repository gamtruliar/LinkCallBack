using System.Collections.Generic;
using System.Linq;
using LinkCallBack2;

/**
 *    trigger many Callback at the same time
 *        T
 *      /  |  \
*     L  L L.....
*  *     usage :
*     new LinkCallbackDispacther("wa").AddCB(a).AddCB(b).AddCB(c).Trigger(obj)
*	 the base LCB Will be fixed and only can be affected by traditional LCB OP
*/
namespace LinkCallBack2
{
	public class LinkCallbackDispacther<RETTYPE> : LinkCallBack<RETTYPE>
	{

		List<LinkCallBack<RETTYPE>> m_LinkCallbacks = null;
		List<LinkCallBack<RETTYPE>> m_rm_LinkCallbacks = null;
		bool triggered = false;
		RETTYPE triggerObj;
		private bool glcbt_triggerOnce = false;

		public static LinkCallbackDispacther<RETTYPE> refreshIfTriggered(LinkCallbackDispacther<RETTYPE> glcbt)
		{
			if (glcbt.isTriggered())
				return new LinkCallbackDispacther<RETTYPE>();
			return glcbt;
		}

		public LinkCallbackDispacther() : base()
		{

		}

		public LinkCallbackDispacther(bool branchOnly, bool triggerOnce) : base()
		{
			if (branchOnly)
			{
				NoMoreLink();
			}

			glcbt_triggerOnce = triggerOnce;
		}

		public long Count()
		{
			return m_LinkCallbacks?.Count ?? 0 + m_rm_LinkCallbacks?.Count ?? 0;
		}

		//tail LCB:
		//make a GroupedLinkCallbackTrigger as a spreader lcb
		// lcb-> GLCB
		//	   /
		//lcb->-
		//	   \
		public LinkCallbackDispacther(LinkCallBack<RETTYPE> lcb, bool needReaction = false) : base()
		{
			if (lcb == null)
			{

				return;
			}

			lcb.SetCB((x) => { return GTrigger(x, false, needReaction); });
		}

		public bool isTriggered()
		{
			return triggered;
		}

		public bool RemoveCB(LinkCallBack<RETTYPE> cb)
		{
			bool removed = false;
			if (m_LinkCallbacks != null)
			{
				lock (m_LinkCallbacks)
				{
					removed = m_LinkCallbacks.Remove(cb);
				}
			}

			if (!removed)
			{
				if (m_rm_LinkCallbacks != null)
				{
					lock (m_rm_LinkCallbacks)
					{
						removed = m_rm_LinkCallbacks.Remove(cb);
					}
				}
			}

			return removed;
		}

		public LinkCallBack<RETTYPE> GetCB(bool onceOnly)
		{
			LinkCallBack<RETTYPE> retLCB = new LinkCallBack<RETTYPE>();
			AddCB(retLCB, onceOnly);
			return retLCB;
		}

		public LinkCallbackDispacther<RETTYPE> AddCB(LinkCallBack<RETTYPE> cb, bool onceOnly = false)
		{
			//Debug.Log("GroupedLinkCallbackTrigger : AddCB|"+StackTraceUtility.ExtractStackTrace());
			if (triggered)
			{
				//Debug.Log("GroupedLinkCallbackTrigger : Direct triggered|"+StackTraceUtility.ExtractStackTrace());
				cb.Trigger(triggerObj);
				//return this;
			}

			if (!triggered)
			{

				if (onceOnly)
				{
					if (m_rm_LinkCallbacks == null)
					{
						m_rm_LinkCallbacks = new List<LinkCallBack<RETTYPE>>();
					}

					lock (m_rm_LinkCallbacks)
					{
						m_rm_LinkCallbacks.Add(cb);
					}
				}
				else
				{
					if (m_LinkCallbacks == null)
					{
						m_LinkCallbacks = new List<LinkCallBack<RETTYPE>>();
					}

					lock (m_LinkCallbacks)
					{
						m_LinkCallbacks.Add(cb);
					}
				}
			}

			return this;
		}

		//should not call by interal as it will loop
		public LinkCallbackDispacther<List<bool>> GGTrigger(RETTYPE obj = default(RETTYPE), bool DoNotWaitBase = false)
		{
			return new LinkCallbackDispacther<List<bool>>(GTrigger(obj, DoNotWaitBase, false));
		}

		//Hard Problem:
		//to do:
		//support time irrelated TraceBack CB for AddCB
		//now:
		//GTrigger-wait will not support Trigger first and AddCB() later, as the m_LinkCallbacks is empty before AddCB()
		public LinkCallBack<List<bool>> GTrigger(RETTYPE obj, bool DoNotWaitBase, bool WithReaction)
		{
			//Debug.Log("GroupedLinkCallbackTrigger : GTrigger|"+StackTraceUtility.ExtractStackTrace());
			triggerObj = obj;
			triggered = true;

			List<LinkCallBack<RETTYPE>> copyList = null;
			if (m_LinkCallbacks != null)
			{
				lock (m_LinkCallbacks)
				{

					copyList = new List<LinkCallBack<RETTYPE>>(m_LinkCallbacks);
				}
			}

			if (m_rm_LinkCallbacks != null)
			{
				lock (m_rm_LinkCallbacks)
				{
					if (copyList == null)
						copyList = new List<LinkCallBack<RETTYPE>>(m_rm_LinkCallbacks);
					else
						copyList.AddRange(m_rm_LinkCallbacks);
				}
			}

			AndLinkCallBack GLCB = null;

			base.Trigger(obj);
			if (copyList != null)
			{
				foreach (var e in copyList)
				{
					e.Trigger(obj);
				}
			}


			if (m_rm_LinkCallbacks != null)
			{
				lock (m_rm_LinkCallbacks)
				{
					m_rm_LinkCallbacks.Clear();
				}
			}


			//Debug.Log("GTrigger :"+GLCB.Get_nonCalledBack_Callbacks_Count+"|"+StackTraceUtility.ExtractStackTrace());
			if (WithReaction)
			{
				return GLCB.FinishAddCB().SetCB<List<bool>>((x) =>
				{
					return LinkCallBack<List<bool>>.DirectExec(x.Select(y => (bool) y).ToList());
				});
			}
			else
			{
				return null;
			}
		}

		public override void Trigger(RETTYPE obj = default(RETTYPE))
		{
			GTrigger(obj, false, false);
		}

	}

}