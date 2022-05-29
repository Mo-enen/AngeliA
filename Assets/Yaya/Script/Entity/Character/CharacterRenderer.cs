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

		// Ser
		[SerializeField] int[] BounceAmounts = new int[] { 500, 200, 100, 50, 25, 50, 100, 200, 500, };
		[SerializeField] int[] BounceAmountsBig = new int[] { 0, -600, -900, -1200, -1400, -1200, -900, -600, 0, };
		[SerializeField] int Bouncy = 150;
		[SerializeField] int PoundingBounce = 1500;
		[SerializeField] int SwimRotationLerp = 100;

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
		private AniCode Ani_Roll = null;
		private AniCode CurrentAni = null;
		private GroupCode Face = null;
		private int CurrentAniFrame = 0;
		private int CurrentCode = 0;
		private int CurrentBounce = 1000;
		private float TargetSwimRotation = 0f;
		private int LastCellHeight = Const.CELL_SIZE;


		#endregion




		#region --- MSG ---


		public void Init (eCharacter ch) {
			Character = ch;
			string name = ch.GetType().Name;
			if (name.StartsWith('e')) name = name[1..];
			Ani_Idle = new($"_a{name}.Idle", $"_a{name}");
			Ani_Walk = new($"_a{name}.Walk", $"_a{name}.Run", $"_a{name}.Idle");
			Ani_Run = new($"_a{name}.Run", $"_a{name}.Walk", $"_a{name}.Idle");
			Ani_JumpU = new($"_a{name}.JumpU", $"_a{name}.Idle");
			Ani_JumpD = new($"_a{name}.JumpD", $"_a{name}.Idle");
			Ani_Roll = new($"_a{name}.Roll", $"_a{name}.Run");
			Ani_Dash = new($"_a{name}.Dash", $"_a{name}.Roll");
			Ani_SquatIdle = new($"_a{name}.SquatIdle", $"_a{name}.Idle");
			Ani_SquatMove = new($"_a{name}.SquatMove", $"_a{name}.Walk");
			Ani_SwimIdle = new($"_a{name}.SwimIdle", $"_a{name}.Swim", $"_a{name}.Idle");
			Ani_SwimMove = new($"_a{name}.SwimMove", $"_a{name}.SwimIdle", $"_a{name}.Swim", $"_a{name}.Run");
			Ani_SwimDash = new($"_a{name}.SwimDash", $"_a{name}.SwimMove", $"_a{name}.Swim", $"_a{name}.Dash");
			Ani_Pound = new($"_a{name}.Pound", $"_a{name}.Idle");
			Ani_Climb = new($"_a{name}.Climb", $"_a{name}.Walk");
			Face = new($"{name}.Face");
		}


		public void FrameUpdate () {
			DrawCharacter();
			DrawFace();
		}


		private void DrawCharacter () {

			int frame = Game.GlobalFrame;
			var movement = Character.Movement;
			AniCode ani;
			if (movement.IsClimbing) {
				ani = Ani_Climb;
			} else if (movement.IsPounding) {
				ani = Ani_Pound;
			} else if (movement.IsRolling) {
				ani = Ani_Roll;
			} else if (movement.IsDashing) {
				ani = !movement.IsGrounded && movement.InWater ? Ani_SwimDash : Ani_Dash;
			} else if (movement.IsSquating) {
				ani = movement.IsMoving ? Ani_SquatMove : Ani_SquatIdle;
			} else if (movement.InWater && !movement.IsGrounded) {
				ani = movement.IsMoving ? Ani_SwimMove : Ani_SwimIdle;
			} else if (movement.IsInAir) {
				ani = movement.FinalVelocityY > 0 ? Ani_JumpU : Ani_JumpD;
			} else {
				ani = movement.IsRunning ? Ani_Run : movement.IsMoving ? Ani_Walk : Ani_Idle;
			}

			// Reset Frame when Switch Ani
			if (CurrentAni != ani) {
				CurrentAniFrame = 0;
				CurrentAni = ani;
			}

			// Swim Rotation
			int pivotY = 0;
			int offsetY = 0;
			if (movement.UseFreeStyleSwim && movement.InWater && !movement.IsGrounded) {
				TargetSwimRotation = Quaternion.LerpUnclamped(
					Quaternion.Euler(0, 0, TargetSwimRotation),
					Quaternion.FromToRotation(
						Vector3.up, new(-movement.LastMoveDirection.x, movement.LastMoveDirection.y)
					),
					SwimRotationLerp / 1000f
				).eulerAngles.z;
				pivotY = 500;
				offsetY = LastCellHeight / 2;
			} else {
				TargetSwimRotation = 0f;
			}

			// Draw
			ref var cell = ref CellRenderer.Draw_Animation(
				CurrentAni.Code,
				Character.X, Character.Y + offsetY, 500, pivotY, (int)TargetSwimRotation,
				movement.FacingRight || movement.IsPounding || movement.IsClimbing ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
				Const.ORIGINAL_SIZE,
				CurrentAniFrame,
				ani.LoopStart
			);
			CurrentCode = CellRenderer.LastDrawnID;

			// Bouncy
			if (Bouncy > 0) {
				int bounce = 1000;
				int duration = BounceAmounts.Length;
				bool reverse = false;
				if (movement.IsSquating && frame.InRangeExculde(movement.LastSquatFrame, movement.LastSquatFrame + duration)) {
					bounce = BounceAmounts[frame - movement.LastSquatFrame];
				}
				if (movement.IsGrounded && frame.InRangeExculde(movement.LastGroundFrame, movement.LastGroundFrame + duration)) {
					bounce = BounceAmounts[frame - movement.LastGroundFrame];
				}
				if (!movement.IsSquating && frame.InRangeExculde(movement.LastSquatingFrame, movement.LastSquatingFrame + duration)) {
					bounce = BounceAmounts[frame - movement.LastSquatingFrame];
					reverse = true;
				}
				if (!movement.IsPounding && movement.IsGrounded && frame.InRangeExculde(movement.LastPoundingFrame, movement.LastPoundingFrame + duration)) {
					bounce = BounceAmountsBig[frame - movement.LastPoundingFrame];
				}
				if (movement.IsPounding) {
					bounce = PoundingBounce;
				}
				if (bounce != 1000) {
					bounce = (int)Util.RemapUnclamped(0, 1000, 1000 - Bouncy, 1000, bounce);
					if (reverse) {
						cell.Width = cell.Width * bounce / 1000;
						cell.Height += cell.Height * (1000 - bounce) / 1000;
					} else {
						cell.Width += cell.Width * (1000 - bounce) / 1000;
						cell.Height = cell.Height * bounce / 1000;
					}
				}
				CurrentBounce = reverse ? -bounce : bounce;
			}

			// Grow Ani Frame
			if (ani == Ani_Climb) {
				if (movement.FinalVelocityY > 0) {
					CurrentAniFrame++;
				} else if (movement.FinalVelocityY < 0) {
					CurrentAniFrame--;
				}
			} else {
				CurrentAniFrame++;
			}
			LastCellHeight = cell.Height;

		}


		private void DrawFace () {
			if (
				Face.Count <= 0 ||
				!CellRenderer.TryGetCharacterMeta(CurrentCode, out var cMeta) ||
				!cMeta.Head.IsVailed ||
				!cMeta.Head.Front
			) return;
			var movement = Character.Movement;
			int bounce = Mathf.Abs(CurrentBounce);
			int offsetY;
			if (CurrentBounce > 0) {
				offsetY = (cMeta.Head.Y + cMeta.Head.Height) * bounce / 1000;
			} else {
				offsetY = cMeta.Head.Y + cMeta.Head.Height;
				offsetY += offsetY * (1000 - bounce) / 1000;
			}
			CellRenderer.Draw_9Slice(
				Face[FaceIndex],
				Character.X - cMeta.SpriteWidth / 2 +
					(movement.FacingRight ?
						cMeta.Head.X :
						cMeta.SpriteWidth - (cMeta.Head.X + cMeta.Head.Width)
					),
				Character.Y + offsetY,
				0, 1000, 0,
				cMeta.Head.Width,
				Const.ORIGINAL_SIZE
			);
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}