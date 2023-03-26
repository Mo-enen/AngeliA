using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eYayaPlayer : Player {


		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();

			if (
				Selecting != this ||
				Game.Current.State != GameState.Play ||
				FrameTask.HasTask(Const.TASK_ROUTE)
			) return;

			switch (CharacterState) {


				case CharacterState.GamePlay:
					ControlHintUI.AddHint(Gamekey.Left, Gamekey.Right, Language.Get(WORD.HINT_MOVE));
					if (!LockingInput) {
						ControlHintUI.AddHint(Gamekey.Jump, Language.Get(WORD.HINT_JUMP));
					}
					if (CurrentActionTarget != null) {
						ControlHintUI.AddHint(Gamekey.Action, Language.Get(YayaUtil.GetHintLanguageCode(CurrentActionTarget.TypeID)));
					} else if (!IsSafe) {
						ControlHintUI.AddHint(Gamekey.Action, Language.Get(WORD.HINT_ATTACK));
					}
					break;


				case CharacterState.Passout:
					if (IsFullPassout) {
						ControlHintUI.DrawGlobalHint(X - Const.HALF, Y + Const.CEL * 3 / 2, Gamekey.Action, Language.Get(WORD.UI_CONTINUE), true);
					}
					break;


				case CharacterState.Sleep:
					ControlHintUI.DrawGlobalHint(X - Const.HALF, Y + Const.CEL * 3 / 2, Gamekey.Action, Language.Get(WORD.HINT_WAKE), true);
					ControlHintUI.AddHint(Gamekey.Action, Language.Get(WORD.HINT_WAKE));
					break;
			}

		}


	}
}