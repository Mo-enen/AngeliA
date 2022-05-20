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
			public int LoopStart = 0;
			public AniCode (string name, params string[] failbacks) {
				int code = name.AngeHash();
				if (CellRenderer.TryGetSprite(code, out _, 0)) {
					Code = code;
					if (CellRenderer.TryGetSpriteChain(code, out var chain)) {
						LoopStart = chain.LoopStart;
					}
				} else {
					foreach (var failback in failbacks) {
						code = failback.AngeHash();
						if (CellRenderer.TryGetSprite(code, out _, 0)) {
							Code = code;
							if (CellRenderer.TryGetSpriteChain(code, out var chain)) {
								LoopStart = chain.LoopStart;
							}
							break;
						}
					}
				}
			}
		}


		private class GroupCode {

			public int this[int i] => Codes.Length > 0 ? Codes[i.Clamp(0, Codes.Length - 1)] : 0;
			public int Count => Codes.Length;

			private readonly int[] Codes = new int[0];
			public GroupCode (string name) {
				var codes = new List<int>();
				int code = name.AngeHash();
				if (CellRenderer.TryGetSprite(code, out _, 0)) {
					codes.Add(code);
				}
				for (int i = 0; i < 1024; i++) {
					code = $"{name} {i}".AngeHash();
					if (CellRenderer.TryGetSprite(code, out _, 0)) {
						codes.Add(code);
					} else break;
				}
				Codes = codes.ToArray();
			}

		}


		#endregion




		#region --- VAR ---


		// Api
		protected eCharacter Character { get; private set; } = null;
		public int FaceIndex { get; set; } = 0;

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
		private GroupCode Face = null;
		private int CurrentAniFrame = 0;
		private int CurrentCode = 0;


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
			Face = new($"{name}.Face");
		}


		public void FrameUpdate () {
			DrawCharacter();
			DrawFace();
		}


		private void DrawCharacter () {
			var movement = Character.Movement;
			AniCode ani;
			if (movement.IsClimbing) {
				ani = Ani_Climb;
			} else if (movement.IsPounding) {
				ani = Ani_Pound;
			} else if (movement.IsDashing) {
				ani = !movement.IsGrounded && movement.InWater ? Ani_SwimDash : Ani_Dash;
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
				movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
				Const.ORIGINAL_SIZE,
				CurrentAniFrame,
				ani.LoopStart
			);
			CurrentCode = CellRenderer.LastDrawnID;
			CurrentAniFrame++;
		}


		private void DrawFace () {
			if (Face.Count > 0 && CellRenderer.TryGetCharacterMeta(CurrentCode, out var cMeta) && cMeta.Head.IsVailed) {
				var movement = Character.Movement;
				CellRenderer.Draw_9Slice(
					Face[FaceIndex],
					Character.X - cMeta.SpriteWidth / 2 +
						(movement.FacingRight ?
							cMeta.Head.X :
							cMeta.SpriteWidth - (cMeta.Head.X + cMeta.Head.Width)
						),
					Character.Y + cMeta.Head.Y + cMeta.Head.Height,
					0, 1000, 0,
					cMeta.Head.Width,
					Const.ORIGINAL_SIZE
				);
			}
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}