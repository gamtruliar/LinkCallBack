
using System;

namespace LinkCallBack2
{
	public interface ILinkCallBackTriggerExecutor
	{
		void run(Action act);
	}
}