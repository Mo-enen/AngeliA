using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class DialogueUI : EntityUI {




		#region --- VAR ---


		// Data
		private static DialogueUI Main;
		

		#endregion




		#region --- MSG ---


		public DialogueUI () => Main = this;


		public override void UpdateUI () {
			base.UpdateUI();
			if (FrameTask.GetCurrentTask() is not DialogueTask) {
				Active = false;
				return;
			}
			DrawDialogue();
		}


		private void DrawDialogue () {





		}


		#endregion




		#region --- API ---


		public static void Enable () {
			if (Main != null && !Main.Active) {
				Stage.SpawnEntity(Main.TypeID, 0, 0);
			}
		}


		#endregion




		#region --- LGC ---



		#endregion




	}
}