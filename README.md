# LinkCallBack

Async Control Wrapper For C#

LinkCallBack:

	Simple async control
		-pass callback by return value
		-Simple Convert from/to callback, event, Task

	usage:

		caller:
		// do Callee=>Callee=>finish

		Callee().SetCB(x=>{
			task finish x is ret
			return Callee();
		})
		.SetCB_End(x=>{
			task finish again x is ret
		});

		callee:
			LinkCallBack<Return Type> Callee(){
				var retLCB=new LinkCallBack<Return Type>();
				//traditional CallBack
				asyncOP((ret)=>{
					retLCB.Trigger(ret);
				});
				return retLCB;
			}

AndLinkCallBack:
	-Wait All
	- AndLinkCallBack is used to  wait all branch finish
	
	trigger by your own/ other ways
	L  L L.....
	\	|	/	 para of Final L ( Object of one L result)
	final L

	Final will be trigger  if and only if all L are Triggered
	* if addCB after FinishAddCB, all CB WILL BE ignored and log an error

	usage :
		trigger with same para
		non trigger only wait:
			new AndLinkCallBack("wa").AddCB(a).AddCB(b).AddCB(c).FinishAddCB().SetCB(xxxx)

OrLinkCallBack:
	-Race
	-OrLinkCallBack is friend of AndLinkCallBack used to  wait until one branch finish
	
	trigger by your own/ other ways
	L  L L.....
	\	|	/	 para of Final L ( Object of one L result)
	final L
	
	Final will be trigger  if and only if one L are Triggered
	** if addCB after FinishAddCB, all CB WILL BE ignored and log an error

	usage :
		trigger with same para
		non trigger only wait:
			new OrLinkCallBack("wa").AddCB(a).AddCB(b).AddCB(c).FinishAddCB().SetCB(xxxx)
	
LoopLinkCallBack:
	-loop support of LinkCallBack

	Usage:
		setLoop(gameobject,Handle Loop Content)
			new LoopLinkCallBack()
			.setLoop<T>(go,(LoopLCB,obj,loop Count){
				//do some things
				if(end of loop){
					LoopLCB.Loop_End(para); <--Should Call if you need to continue at loop end action,the para will pass to LoopLinkCallBack.setCB.
					return null;<--return null to stop the loop
				}
				//do some things
				if(continue to loop){
					//if need to go to next iteration now return DirectTriggered LCB
					return The LinkCallBack which will be triggered //ps the loop will continues when the LinkCallBack trigger , and the para will transter to obj of this
				}
			}).setCB_End((obj)=>{
				//loop finish
			});
	stream flow:
	xxx->LoopLCB->Handle Loop Content->[continues?]	--No--> CB
 				^<---------------------<-NO-v
	action description:
		when LoopLinkCallBack has been multi-triggered, there will be n loop in the same time but ending in collapsing at one CB
 