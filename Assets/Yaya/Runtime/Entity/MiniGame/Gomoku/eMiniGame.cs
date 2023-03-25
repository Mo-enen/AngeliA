using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class MiniGameTask : TaskItem {
		public eMiniGame MiniGame = null;
		public override TaskResult FrameUpdate () => MiniGame != null && MiniGame.Active ? TaskResult.Continue : TaskResult.End;
	}
	[EntityAttribute.Capacity(1, 0)]
	[EntityAttribute.DontDestroyOutOfRange]
	public abstract class eMiniGame : EntityUI {
		public override void OnActived () {
			base.OnActived();
			if (FrameTask.AddToLast(
				typeof(MiniGameTask).AngeHash(),
				Const.TASK_ROUTE
			) is MiniGameTask task) {
				task.MiniGame = this;
			}
		}

	}
}