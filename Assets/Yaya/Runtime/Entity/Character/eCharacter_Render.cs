using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract partial class eCharacter {




		#region --- SUB ---


		private class AniSheet {

			public int[] Faces = null;
			public int FaceBlink = 0;
			public int Sleep = 0;
			public int Damaging = 0;
			public int Passout = 0;
			public int DoorFront = 0;
			public int DoorBack = 0;

			// Movement
			public int Idle = 0;
			public int Walk = 0;
			public int Run = 0;
			public int JumpU = 0;
			public int JumpD = 0;
			public int Dash = 0;
			public int SquatIdle = 0;
			public int SquatMove = 0;
			public int SwimIdle = 0;
			public int SwimMove = 0;
			public int SwimDash = 0;
			public int Pound = 0;
			public int Climb = 0;
			public int Roll = 0;
			public int Fly = 0;
			public int Slide = 0;
			public int GrabTop = 0;
			public int GrabSide = 0;
			public int GrabFlip = 0;

			// Attack
			public int[] Attacks = null;
			public int[] Attacks_Move = null;
			public int[] Attacks_Air = null;
			public int[] Attacks_Water = null;
			public int[] Attacks_Charge = null;

			// API
			public static int LoadAniCode (string name, int failback = 0) => LoadAniCode(
				name.AngeHash(), failback
			);
			public static int LoadAniCode (int code, int failback = 0) {
				if (CellRenderer.TryGetSpriteChain(code, out _) || CellRenderer.TryGetSprite(code, out _)) {
					return code;
				} else {
					return failback;
				}
			}


			public static int[] GetAniArray (string keyName) {
				var result = new List<int>();
				int code = keyName.AngeHash();
				if (CellRenderer.TryGetSprite(code, out _, 0)) {
					result.Add(code);
				}
				for (char c = 'A'; c <= 'Z'; c++) {
					code = $"{keyName}{c}".AngeHash();
					if (CellRenderer.TryGetSprite(code, out _, 0)) {
						result.Add(code);
					} else break;
				}
				return result.ToArray();
			}


			public static int[] GetGroupCode (string name) {
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
				return codes.ToArray();
			}


		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int SLEEP_PARTICLE_CODE = typeof(eDefaultParticle).AngeHash();
		private static readonly int SLIDE_PARTICLE_CODE = typeof(eSlideDust).AngeHash();
		private static readonly int[] BOUNCE_AMOUNTS = new int[] { 500, 200, 100, 50, 25, 50, 100, 200, 500, };
		private static readonly int[] BOUNCE_AMOUNTS_BIG = new int[] { 0, -600, -900, -1200, -1400, -1200, -900, -600, 0, };
		private const int BOUNCY = 150;
		private const int POUNDING_BOUNCE = 1500;
		private const int SWIM_ROTATION_LERP = 100;
		private const int DAMAGE_BLINK_RATE = 8;
		private const int EYE_BLINK_RATE = 360;

		// Api
		public int FaceIndex { get; set; } = 0;

		// Data
		private readonly AniSheet AnimationSheet = new();
		private float TargetRotation = 0f;
		private int CurrentAni = 0;
		private int CurrentAniFrame = 0;
		private int CurrentCode = 0;
		private int CurrentBounce = 1000;
		private int LastCellHeight = Const.CEL;
		private int LastRequireBounceFrame = int.MinValue;
		private int BlinkingTime = int.MinValue;
		private int DamagingTime = int.MinValue;
		private int PrevSleepAmount = 0;
		private int EnterDoorEndFrame = 0;


		#endregion




		#region --- MSG ---


		public void OnInitialize_Render () {

			string name = GetType().Name;
			if (name.StartsWith('e')) name = name[1..];

			AnimationSheet.Idle = AniSheet.LoadAniCode($"{name}.Idle");
			if (AnimationSheet.Idle == 0) AnimationSheet.Idle = AniSheet.LoadAniCode($"{name}");
			AnimationSheet.Walk = AniSheet.LoadAniCode($"{name}.Walk", AnimationSheet.Idle);
			AnimationSheet.Run = AniSheet.LoadAniCode($"{name}.Run", AnimationSheet.Walk);
			int jump = AniSheet.LoadAniCode($"{name}.Jump", AnimationSheet.Idle);
			AnimationSheet.JumpU = AniSheet.LoadAniCode($"{name}.JumpU", jump);
			AnimationSheet.JumpD = AniSheet.LoadAniCode($"{name}.JumpD", jump);
			AnimationSheet.Roll = AniSheet.LoadAniCode($"{name}.Roll", AnimationSheet.Run);
			AnimationSheet.Dash = AniSheet.LoadAniCode($"{name}.Dash", AnimationSheet.Roll);
			int squat = AniSheet.LoadAniCode($"{name}.Squat", AnimationSheet.Idle);
			AnimationSheet.SquatIdle = AniSheet.LoadAniCode($"{name}.SquatIdle", squat);
			AnimationSheet.SquatMove = AniSheet.LoadAniCode($"{name}.SquatMove", squat);
			int swim = AniSheet.LoadAniCode($"{name}.Swim", AnimationSheet.Run);
			AnimationSheet.SwimIdle = AniSheet.LoadAniCode($"{name}.SwimIdle", swim);
			AnimationSheet.SwimMove = AniSheet.LoadAniCode($"{name}.SwimMove", swim);
			AnimationSheet.SwimDash = AniSheet.LoadAniCode($"{name}.SwimDash", swim);
			AnimationSheet.Pound = AniSheet.LoadAniCode($"{name}.Pound", AnimationSheet.Idle);
			AnimationSheet.Climb = AniSheet.LoadAniCode($"{name}.Climb", AnimationSheet.Idle);
			AnimationSheet.Fly = AniSheet.LoadAniCode($"{name}.Fly", AnimationSheet.Run);
			AnimationSheet.Slide = AniSheet.LoadAniCode($"{name}.Slide", AnimationSheet.JumpD);
			AnimationSheet.GrabTop = AniSheet.LoadAniCode($"{name}.GrabTop", AnimationSheet.JumpD);
			AnimationSheet.GrabSide = AniSheet.LoadAniCode($"{name}.GrabSide", AnimationSheet.Slide);
			AnimationSheet.GrabFlip = AniSheet.LoadAniCode($"{name}.GrabSide", AnimationSheet.Roll);

			AnimationSheet.Sleep = AniSheet.LoadAniCode($"{name}.Sleep", AnimationSheet.Idle);
			AnimationSheet.Damaging = AniSheet.LoadAniCode($"{name}.Damage", AnimationSheet.Idle);
			AnimationSheet.Passout = AniSheet.LoadAniCode($"{name}.Passout");
			AnimationSheet.Faces = AniSheet.GetGroupCode($"{name}.Face");
			AnimationSheet.FaceBlink = AniSheet.LoadAniCode($"{name}.Face.Blink");

			AnimationSheet.DoorFront = AniSheet.LoadAniCode($"{name}.DoorFront", AnimationSheet.Idle);
			AnimationSheet.DoorBack = AniSheet.LoadAniCode($"{name}.DoorBack", AnimationSheet.Idle);

			AnimationSheet.Attacks = AniSheet.GetAniArray($"{name}.Attack");
			AnimationSheet.Attacks_Move = AniSheet.GetAniArray($"{name}.AttackMove");
			AnimationSheet.Attacks_Air = AniSheet.GetAniArray($"{name}.AttackAir");
			AnimationSheet.Attacks_Water = AniSheet.GetAniArray($"{name}.AttackWater");
			AnimationSheet.Attacks_Charge = AniSheet.GetAniArray($"{name}.Charge");

		}


		public void FrameUpdate_Render () {

			int frame = Game.GlobalFrame;

			// Blink
			if (frame < BlinkingTime && (BlinkingTime - frame) % DAMAGE_BLINK_RATE < DAMAGE_BLINK_RATE / 2) return;

			// Damage
			if (frame < DamagingTime) {
				CellRenderer.Draw_Animation(
					AnimationSheet.Damaging,
					X, Y, 500, 0, 0,
					FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame
				);
				return;
			}

			// Door
			if (frame < EnterDoorEndFrame.Abs()) {
				CellRenderer.Draw_Animation(
					EnterDoorEndFrame > 0 ? AnimationSheet.DoorFront : AnimationSheet.DoorBack,
					X, Y, 500, 0, 0,
					FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame
				);
				return;
			}

			// Draw
			switch (CharacterState) {
				case CharacterState.GamePlay:
					DrawBody();
					DrawFace();
					if (IsSliding && Game.GlobalFrame % 24 == 0) {
						var rect = Rect;
						Game.Current.AddEntity(
							SLIDE_PARTICLE_CODE, FacingRight ? rect.xMax : rect.xMin, rect.yMin + rect.height * 3 / 4
						);
					}
					break;
				case CharacterState.Sleep:
					DrawSleep();
					break;
				case CharacterState.Passout:
					CellRenderer.Draw_Animation(
						AnimationSheet.Passout,
						X, Y,
						500, 0, 0,
						FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame
					);
					break;
			}
		}


		private void DrawBody () {

			var ani = AnimationSheet.Idle;
			int frame = Game.GlobalFrame;

			// Get Ani
			if (IsAttacking) {
				// Attack
				var attacks = AnimationSheet.Attacks;
				switch (MoveState) {
					case MovementState.SwimDash:
					case MovementState.SwimIdle:
					case MovementState.SwimMove:
						attacks = AnimationSheet.Attacks_Water ?? attacks;
						break;
					case MovementState.JumpDown:
					case MovementState.JumpUp:
					case MovementState.Roll:
						if (!IsGrounded && !InWater) {
							attacks = AnimationSheet.Attacks_Air ?? attacks;
						}
						break;
					case MovementState.Walk:
					case MovementState.Run:
						attacks = AnimationSheet.Attacks_Move ?? attacks;
						break;
				}
				if (attacks.Length > 0) {
					ani = attacks[AttackCombo.Clamp(0, attacks.Length - 1)];
					if (frame <= LastAttackFrame) CurrentAniFrame = 0;
				}
			} else {
				// Movement
				ani = MoveState switch {
					MovementState.Walk => AnimationSheet.Walk,
					MovementState.Run => AnimationSheet.Run,
					MovementState.JumpUp => AnimationSheet.JumpU,
					MovementState.JumpDown => AnimationSheet.JumpD,
					MovementState.SwimIdle => AnimationSheet.SwimIdle,
					MovementState.SwimMove => AnimationSheet.SwimMove,
					MovementState.SwimDash => AnimationSheet.SwimDash,
					MovementState.SquatIdle => AnimationSheet.SquatIdle,
					MovementState.SquatMove => AnimationSheet.SquatMove,
					MovementState.Dash => AnimationSheet.Dash,
					MovementState.Roll => AnimationSheet.Roll,
					MovementState.Pound => AnimationSheet.Pound,
					MovementState.Climb => AnimationSheet.Climb,
					MovementState.Fly => AnimationSheet.Fly,
					MovementState.Slide => AnimationSheet.Slide,
					MovementState.GrabTop => AnimationSheet.GrabTop,
					MovementState.GrabSide => AnimationSheet.GrabSide,
					MovementState.GrabFlip => AnimationSheet.GrabFlip,
					_ => AnimationSheet.Idle,
				};
			}

			// Rotation
			int pivotY = 0;
			int offsetY = 0;
			if (UseFreeStyleSwim && InWater && !IsGrounded) {
				TargetRotation = Quaternion.LerpUnclamped(
					Quaternion.Euler(0, 0, TargetRotation),
					Quaternion.FromToRotation(
						Vector3.up, new(-LastMoveDirection.x, LastMoveDirection.y)
					),
					SWIM_ROTATION_LERP / 1000f
				).eulerAngles.z;
				pivotY = 500;
				offsetY = LastCellHeight / 2;
			} else {
				TargetRotation = 0f;
			}

			// Reset Frame when Switch Ani
			if (CurrentAni != ani) {
				CurrentAniFrame = 0;
				CurrentAni = ani;
			}

			// Draw
			bool isPounding = MoveState == MovementState.Pound;
			bool isClimbing = MoveState == MovementState.Climb;
			bool isSquating = MoveState == MovementState.SquatIdle || MoveState == MovementState.SquatMove;
			var cell = CellRenderer.Draw_Animation(
				CurrentAni,
				X, Y + offsetY, 500, pivotY, (int)TargetRotation,
				FacingRight || isPounding || isClimbing ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
				Const.ORIGINAL_SIZE,
				CurrentAniFrame
			);
			CurrentCode = CellRenderer.LastDrawnID;
			LastCellHeight = cell.Height;

			// Bouncy
			if (BOUNCY > 0) {
				int bounce = 1000;
				int duration = BOUNCE_AMOUNTS.Length;
				bool reverse = false;
				if (frame < LastRequireBounceFrame + duration) {
					bounce = BOUNCE_AMOUNTS[frame - LastRequireBounceFrame];
				} else if (isPounding) {
					bounce = POUNDING_BOUNCE;
				} else if (!isPounding && IsGrounded && frame.InRangeExculde(LastPoundingFrame, LastPoundingFrame + duration)) {
					bounce = BOUNCE_AMOUNTS_BIG[frame - LastPoundingFrame];
				} else if (isSquating && frame.InRangeExculde(LastSquatFrame, LastSquatFrame + duration)) {
					bounce = BOUNCE_AMOUNTS[frame - LastSquatFrame];
				} else if (IsGrounded && frame.InRangeExculde(LastGroundFrame, LastGroundFrame + duration)) {
					bounce = BOUNCE_AMOUNTS[frame - LastGroundFrame];
				} else if (!isSquating && frame.InRangeExculde(LastSquatingFrame, LastSquatingFrame + duration)) {
					bounce = BOUNCE_AMOUNTS[frame - LastSquatingFrame];
					reverse = true;
				}
				if (bounce != 1000) {
					bounce = (int)Util.RemapUnclamped(0, 1000, 1000 - BOUNCY, 1000, bounce);
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
			if (ani != AnimationSheet.Climb) {
				// Normal
				CurrentAniFrame++;
			} else {
				// Climb
				int climbVelocity = IntendedY != 0 ? IntendedY : IntendedX;
				if (climbVelocity > 0) {
					CurrentAniFrame++;
				} else if (climbVelocity < 0) {
					CurrentAniFrame--;
				}
			}

		}


		private void DrawFace () {
			if (
				AnimationSheet.Faces.Length <= 0 ||
				!CellRenderer.TryGetSprite(CurrentCode, out var sprite) ||
				sprite.GlobalBorder.IsZero
			) return;
			int bounce = Mathf.Abs(CurrentBounce);
			int offsetY = sprite.GlobalHeight - sprite.GlobalBorder.Up;
			if (CurrentBounce > 0) {
				offsetY = offsetY * bounce / 1000;
			} else {
				offsetY += offsetY * (1000 - bounce) / 1000;
			}
			var faceID = AnimationSheet.Faces[FaceIndex.UMod(AnimationSheet.Faces.Length)];
			CellRenderer.Draw_9Slice(
				Game.GlobalFrame % EYE_BLINK_RATE > 8 ? faceID : AnimationSheet.FaceBlink,
				X - sprite.GlobalWidth / 2 + (FacingRight ? sprite.GlobalBorder.Left : sprite.GlobalBorder.Right),
				Y + offsetY,
				0, 1000, 0,
				sprite.GlobalWidth - sprite.GlobalBorder.Left - sprite.GlobalBorder.Right,
				Const.ORIGINAL_SIZE
			);
		}


		private void DrawSleep () {
			var backCell = CellRenderer.Draw_Animation(
				AnimationSheet.Sleep,
				X, Y,
				500, 0, 0,
				Const.ORIGINAL_SIZE,
				Const.ORIGINAL_SIZE,
				Game.GlobalFrame
			);
			var cell = CellRenderer.Draw_Animation(
				AnimationSheet.Sleep,
				X, Y,
				500, 0, 0,
				Const.ORIGINAL_SIZE,
				Const.ORIGINAL_SIZE,
				Game.GlobalFrame
			);
			if (CellRenderer.TryGetSprite(AnimationSheet.Sleep, out var sprite) && sprite.GlobalBorder.Down != 0) {
				cell.Y -= sprite.GlobalBorder.Down;
				backCell.Y -= sprite.GlobalBorder.Down;
			}
			backCell.Color.r = 128;
			backCell.Color.g = 128;
			backCell.Color.b = 128;
			backCell.Color.a = 255;
			// Fill
			if (SleepAmount < 1000) {
				cell.Shift.Up = Util.Remap(90, 0, 0, 1000, SleepFrame);
			} else if (SleepAmount >= 1000 && PrevSleepAmount < 1000) {
				// Spawn Particle
				if (Game.Current.TryAddEntity(
					SLEEP_PARTICLE_CODE,
					cell.X - (int)(cell.PivotX * cell.Width) + cell.Width / 2,
					cell.Y - (int)(cell.PivotY * cell.Height) + cell.Height / 2,
					out var particle
				)) {
					particle.Width = Const.CEL * 2;
					particle.Height = Const.CEL * 2;
				}
			}
			PrevSleepAmount = SleepAmount;
			// ZZZ
			if (Game.GlobalFrame % 42 == 0) {
				Game.Current.TryAddEntity(eSleepParticle.TYPE_ID, X, Y + Height / 2, out _);
			}
		}


		#endregion




		#region --- API ---


		public void RenderBounce () => LastRequireBounceFrame = Game.GlobalFrame;


		public void RenderBlink (int duration) => BlinkingTime = Game.GlobalFrame + duration;


		public void RenderDamage (int duration) => DamagingTime = Game.GlobalFrame + duration;


		public void RenderEnterDoor (int duration, bool front) => EnterDoorEndFrame = (Game.GlobalFrame + duration) * (front ? 1 : -1);


		#endregion




	}
}