using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class CharacterRenderer {




		#region --- SUB ---


		private class AniCode {
			public int Code = 0;
			public int Width = 0;
			public int Height = 0;
			public AniCode (string name, params string[] failbacks) {
				int code = name.AngeHash();
				if (CellRenderer.TryGetSprite(code, out var sprite, 0)) {
					Width = sprite.GlobalWidth;
					Height = sprite.GlobalHeight;
				} else {
					foreach (var failback in failbacks) {
						code = failback.AngeHash();
						if (CellRenderer.TryGetSprite(code, out sprite, 0)) {
							Code = code;
							Width = sprite.GlobalWidth;
							Height = sprite.GlobalHeight;
							break;
						}
					}
				}
			}
		}


		#endregion




		#region --- VAR ---


		// Api
		protected eCharacter Character { get; init; } = null;

		// Data
		private readonly AniCode Ani_Idle = null;
		private readonly AniCode Ani_Walk = null;
		private readonly AniCode Ani_Run = null;
		private readonly AniCode Ani_Jump = null;
		private readonly AniCode Ani_Dash = null;
		private readonly AniCode Ani_SquatIdle = null;
		private readonly AniCode Ani_SquatRun = null;
		private readonly AniCode Ani_Swim = null;
		private readonly AniCode Ani_SwimDash = null;
		private readonly AniCode Ani_Pound = null;


		#endregion




		#region --- MSG ---


		public CharacterRenderer (eCharacter ch) {
			Character = ch;
			string name = GetType().Name;
			if (name.StartsWith('e')) name = name[1..];
			Ani_Idle = new($"_a{name}.Idle");
			Ani_Walk = new($"_a{name}.Walk", $"_a{name}.Run", $"_a{name}.Idle");
			Ani_Run = new($"_a{name}.Run", $"_a{name}.Walk", $"_a{name}.Idle");
			Ani_Jump = new($"_a{name}.Jump", $"_a{name}.Idle");
			Ani_Dash = new($"_a{name}.Dash", $"_a{name}.Run");
			Ani_SquatIdle = new($"_a{name}.SquatIdle", $"_a{name}.Idle");
			Ani_SquatRun = new($"_a{name}.SquatRun", $"_a{name}.Run");
			Ani_Swim = new($"_a{name}.Swim", $"_a{name}.Run");
			Ani_SwimDash = new($"_a{name}.SwimDash", $"_a{name}.Run");
			Ani_Pound = new($"_a{name}.Pound", $"_a{name}.Idle");
		}


		public virtual void FrameUpdate (int frame) {






		}


		#endregion




		#region --- API ---



		#endregion




		#region --- LGC ---




		#endregion




	}
}