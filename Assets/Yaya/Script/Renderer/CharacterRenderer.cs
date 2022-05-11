using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[System.Serializable]
	public class CharacterRenderer {




		#region --- SUB ---


		private class AniCode {
			public int Code = 0;
			public int Width = 0;
			public int Height = 0;
			public int LoopStart = 0;
			public AniCode (string name, params string[] failbacks) {
				int code = name.AngeHash();
				if (CellRenderer.TryGetSprite(code, out var sprite, 0)) {
					Code = code;
					Width = sprite.GlobalWidth;
					Height = sprite.GlobalHeight;
					if (CellRenderer.TryGetSpriteChain(code, out var chain)) {
						LoopStart = chain.LoopStart;
					}
				} else {
					foreach (var failback in failbacks) {
						code = failback.AngeHash();
						if (CellRenderer.TryGetSprite(code, out sprite, 0)) {
							Code = code;
							Width = sprite.GlobalWidth;
							Height = sprite.GlobalHeight;
							if (CellRenderer.TryGetSpriteChain(code, out var chain)) {
								LoopStart = chain.LoopStart;
							}
							break;
						}
					}
				}
			}
		}


		#endregion




		#region --- VAR ---


		// Api
		protected eCharacter Character { get; private set; } = null;

		// Ser


		// Data
		private AniCode Ani_Idle = null;
		private AniCode Ani_Walk = null;
		private AniCode Ani_Run = null;
		private AniCode Ani_JumpU = null;
		private AniCode Ani_JumpD = null;
		private AniCode Ani_Dash = null;
		private AniCode Ani_SquatIdle = null;
		private AniCode Ani_SquatMove = null;
		private AniCode Ani_SwimIdle = null;
		private AniCode Ani_SwimMove = null;
		private AniCode Ani_SwimDash = null;
		private AniCode Ani_Pound = null;
		private AniCode Ani_Climb = null;
		private AniCode CurrentAni = null;
		private int CurrentAniFrame = 0;


		#endregion




		#region --- MSG ---


		public void Init (eCharacter ch) {
			Character = ch;
			string name = ch.GetType().Name;
			if (name.StartsWith('e')) name = name[1..];
			Ani_Idle = new($"_a{name}.Idle");
			Ani_Walk = new($"_a{name}.Walk", $"_a{name}.Run", $"_a{name}.Idle");
			Ani_Run = new($"_a{name}.Run", $"_a{name}.Walk", $"_a{name}.Idle");
			Ani_JumpU = new($"_a{name}.JumpU", $"_a{name}.Idle");
			Ani_JumpD = new($"_a{name}.JumpD", $"_a{name}.Idle");
			Ani_Dash = new($"_a{name}.Dash", $"_a{name}.Run");
			Ani_SquatIdle = new($"_a{name}.SquatIdle", $"_a{name}.Idle");
			Ani_SquatMove = new($"_a{name}.SquatMove", $"_a{name}.Walk");
			Ani_SwimIdle = new($"_a{name}.SwimIdle", $"_a{name}.Idle");
			Ani_SwimMove = new($"_a{name}.SwimMove", $"_a{name}.Run");
			Ani_SwimDash = new($"_a{name}.SwimDash", $"_a{name}.Run");
			Ani_Pound = new($"_a{name}.Pound", $"_a{name}.Idle");
			Ani_Climb = new($"_a{name}.Climb", $"_a{name}.Idle");
		}


		public virtual void FrameUpdate (int frame) {
			var movement = Character.Movement;
			AniCode ani;
			if (movement.IsClimbing) {
				ani = Ani_Climb;
			} else if (movement.IsPounding) {
				ani = Ani_Pound;
			} else if (movement.IsDashing) {
				ani = movement.InWater ? Ani_SwimDash : Ani_Dash;
			} else if (movement.IsSquating) {
				ani = movement.IsMoving ? Ani_SquatMove : Ani_SquatIdle;
			} else if (movement.InWater) {
				ani = movement.IsMoving ? Ani_SwimMove : Ani_SwimIdle;
			} else if (movement.IsInAir) {
				ani = movement.FinalVelocityY > 0 ? Ani_JumpU : Ani_JumpD;
			} else {
				ani = movement.IsRunning ? Ani_Run : movement.IsMoving ? Ani_Walk : Ani_Idle;
			}
			if (CurrentAni != ani) {
				CurrentAniFrame = 0;
				CurrentAni = ani;
			}
			CellRenderer.Draw_Animation(
				CurrentAni.Code,
				Character.X, Character.Y, 500, 0, 0,
				movement.FacingRight ? CurrentAni.Width : -CurrentAni.Width,
				CurrentAni.Height,
				CurrentAniFrame,
				ani.LoopStart
			);
			CurrentAniFrame++;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}