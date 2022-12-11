using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract partial class eCharacter {




		#region --- SUB ---


		private class AniSheet {

			public GroupCode Face = null;
			public AniCode FaceBlink = null;
			public AniCode Sleep = null;
			public AniCode Damaging = null;
			public AniCode Passout = null;
			public AniCode DoorFront = null;
			public AniCode DoorBack = null;

			// Movement
			public AniCode Idle = null;
			public AniCode Walk = null;
			public AniCode Run = null;
			public AniCode JumpU = null;
			public AniCode JumpD = null;
			public AniCode Dash = null;
			public AniCode SquatIdle = null;
			public AniCode SquatMove = null;
			public AniCode SwimIdle = null;
			public AniCode SwimMove = null;
			public AniCode SwimDash = null;
			public AniCode Pound = null;
			public AniCode Climb = null;
			public AniCode Roll = null;
			public AniCode Fly = null;
			public AniCode Slide = null;

			// Attack
			public AniCode[] Attacks = null;
			public AniCode[] Attacks_Move = null;
			public AniCode[] Attacks_Air = null;
			public AniCode[] Attacks_Water = null;
			public AniCode[] Attacks_Charge = null;

		}


		private class AniCode {


			public int Code = 0;
			public int LoopStart = 0;


			public AniCode (string name, params AniCode[] failbacks) : this(name.AngeHash(), failbacks) { }
			public AniCode (int code, params AniCode[] failbacks) {
				Load(code);
				if (Code == 0) {
					foreach (var failback in failbacks) {
						if (failback.Code != 0) {
							Code = failback.Code;
							LoopStart = failback.LoopStart;
							break;
						}
					}
				}
			}

			public void Load (int nameCode) {
				if (CellRenderer.TryGetSpriteChain(nameCode, out var chain)) {
					Code = nameCode;
					LoopStart = chain.LoopStart;
				} else if (CellRenderer.TryGetSprite(nameCode, out _)) {
					Code = nameCode;
					LoopStart = 0;
				} else {
					Code = 0;
					LoopStart = 0;
				}
			}


			public static AniCode[] GetAnimationArray (string keyName, int defaultLoopStart) {
				var result = new List<AniCode>();
				int code = keyName.AngeHash();
				if (CellRenderer.TryGetSprite(code, out _, 0)) {
					result.Add(new AniCode(code) { LoopStart = defaultLoopStart, });
				}
				for (char c = 'A'; c <= 'Z'; c++) {
					code = $"{keyName}{c}".AngeHash();
					if (CellRenderer.TryGetSprite(code, out _, 0)) {
						result.Add(new AniCode(code) { LoopStart = defaultLoopStart, });
					} else break;
				}
				return result.ToArray();
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


		// Const
		private static readonly int SLEEP_PARTICLE_CODE = eDefaultParticle.TYPE_ID;
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
		private AniCode CurrentAni = null;
		private float TargetRotation = 0f;
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

			AnimationSheet.Idle = new($"{name}.Idle");
			if (AnimationSheet.Idle.Code == 0) AnimationSheet.Idle = new($"{name}");
			AnimationSheet.Walk = new($"{name}.Walk", AnimationSheet.Idle);
			AnimationSheet.Run = new($"{name}.Run", AnimationSheet.Walk);
			var jump = new AniCode($"{name}.Jump", AnimationSheet.Idle);
			AnimationSheet.JumpU = new($"{name}.JumpU", jump);
			AnimationSheet.JumpD = new($"{name}.JumpD", jump);
			AnimationSheet.Roll = new($"{name}.Roll", AnimationSheet.Run, AnimationSheet.Idle);
			AnimationSheet.Dash = new($"{name}.Dash", AnimationSheet.Roll);
			var squat = new AniCode($"{name}.Squat", AnimationSheet.Idle);
			AnimationSheet.SquatIdle = new($"{name}.SquatIdle", squat);
			AnimationSheet.SquatMove = new($"{name}.SquatMove", squat);
			var swim = new AniCode($"{name}.Swim", AnimationSheet.Run);
			AnimationSheet.SwimIdle = new($"{name}.SwimIdle", swim);
			AnimationSheet.SwimMove = new($"{name}.SwimMove", swim);
			AnimationSheet.SwimDash = new($"{name}.SwimDash", swim);
			AnimationSheet.Pound = new($"{name}.Pound", AnimationSheet.Idle);
			AnimationSheet.Climb = new($"{name}.Climb", AnimationSheet.Idle);
			AnimationSheet.Fly = new($"{name}.Fly", AnimationSheet.Run);
			AnimationSheet.Slide = new($"{name}.Slide", AnimationSheet.JumpD);

			AnimationSheet.Sleep = new($"{name}.Sleep", AnimationSheet.Idle);
			AnimationSheet.Damaging = new($"{name}.Damage", AnimationSheet.Idle);
			AnimationSheet.Passout = new($"{name}.Passout");
			AnimationSheet.Face = new($"{name}.Face");
			AnimationSheet.FaceBlink = new($"{name}.Face.Blink");

			AnimationSheet.DoorFront = new($"{name}.DoorFront", AnimationSheet.Idle);
			AnimationSheet.DoorBack = new($"{name}.DoorBack", AnimationSheet.Idle);

			AnimationSheet.Attacks = AniCode.GetAnimationArray($"{name}.Attack", -1);
			AnimationSheet.Attacks_Move = AniCode.GetAnimationArray($"{name}.AttackMove", -1);
			AnimationSheet.Attacks_Air = AniCode.GetAnimationArray($"{name}.AttackAir", -1);
			AnimationSheet.Attacks_Water = AniCode.GetAnimationArray($"{name}.AttackWater", -1);
			AnimationSheet.Attacks_Charge = AniCode.GetAnimationArray($"{name}.Charge", -1);

		}


		public void FrameUpdate_Render () {

			int frame = Game.GlobalFrame;

			// Blink
			if (frame < BlinkingTime && (BlinkingTime - frame) % DAMAGE_BLINK_RATE < DAMAGE_BLINK_RATE / 2) return;

			// Damage
			if (frame < DamagingTime) {
				CellRenderer.Draw_Animation(
					AnimationSheet.Damaging.Code,
					X, Y, 500, 0, 0,
					FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame,
					AnimationSheet.Damaging.LoopStart
				);
				return;
			}

			// Door
			if (frame < EnterDoorEndFrame.Abs()) {
				CellRenderer.Draw_Animation(
					EnterDoorEndFrame > 0 ? AnimationSheet.DoorFront.Code : AnimationSheet.DoorBack.Code,
					X, Y, 500, 0, 0,
					FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame,
					AnimationSheet.Damaging.LoopStart
				);
				return;
			}

			// Draw
			switch (CharacterState) {
				case CharacterState.GamePlay:
					DrawBody();
					DrawFace();
					break;
				case CharacterState.Sleep:
					DrawSleep();
					break;
				case CharacterState.Passout:
					CellRenderer.Draw_Animation(
						AnimationSheet.Passout.Code,
						X, Y,
						500, 0, 0,
						FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame,
						AnimationSheet.Passout.LoopStart
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
				CurrentAni.Code,
				X, Y + offsetY, 500, pivotY, (int)TargetRotation,
				FacingRight || isPounding || isClimbing ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
				Const.ORIGINAL_SIZE,
				CurrentAniFrame,
				ani.LoopStart
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
				AnimationSheet.Face.Count <= 0 ||
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
			var faceID = AnimationSheet.Face[FaceIndex.UMod(AnimationSheet.Face.Count)];
			CellRenderer.Draw_9Slice(
				Game.GlobalFrame % EYE_BLINK_RATE > 8 ? faceID : AnimationSheet.FaceBlink.Code,
				X - sprite.GlobalWidth / 2 + (FacingRight ? sprite.GlobalBorder.Left : sprite.GlobalBorder.Right),
				Y + offsetY,
				0, 1000, 0,
				sprite.GlobalWidth - sprite.GlobalBorder.Left - sprite.GlobalBorder.Right,
				Const.ORIGINAL_SIZE
			);
		}


		private void DrawSleep () {
			var backCell = CellRenderer.Draw_Animation(
				AnimationSheet.Sleep.Code,
				X, Y,
				500, 0, 0,
				Const.ORIGINAL_SIZE,
				Const.ORIGINAL_SIZE,
				Game.GlobalFrame,
				AnimationSheet.Sleep.LoopStart
			);
			var cell = CellRenderer.Draw_Animation(
				AnimationSheet.Sleep.Code,
				X, Y,
				500, 0, 0,
				Const.ORIGINAL_SIZE,
				Const.ORIGINAL_SIZE,
				Game.GlobalFrame,
				AnimationSheet.Sleep.LoopStart
			);
			if (CellRenderer.TryGetSprite(AnimationSheet.Sleep.Code, out var sprite) && sprite.GlobalBorder.Down != 0) {
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