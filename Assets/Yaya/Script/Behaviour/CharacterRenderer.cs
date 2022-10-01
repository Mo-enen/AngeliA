using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using UnityEngine.TextCore.Text;


namespace Yaya {
	[System.Serializable]
	public class CharacterRenderer {




		#region --- SUB ---


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


			public static AniCode[] GetAnimationArray (string keyName, int defaultLoopStart, AniCode[] failbacks = null) {
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
				// Failback
				if (failbacks != null && failbacks.Length > 0 && result.Count == 0) {
					result.AddRange(failbacks);
				}
				return result.ToArray();
			}


		}


		public class GroupCode {

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
		private static readonly int SLEEP_PARTICLE_CODE = typeof(eDefaultParticle).AngeHash();

		// Api
		public eCharacter Character { get; private set; } = null;
		public int FaceIndex { get; set; } = 0;

		// Ser
		[SerializeField] int[] BounceAmounts = new int[] { 500, 200, 100, 50, 25, 50, 100, 200, 500, };
		[SerializeField] int[] BounceAmountsBig = new int[] { 0, -600, -900, -1200, -1400, -1200, -900, -600, 0, };
		[SerializeField] int Bouncy = 150;
		[SerializeField] int PoundingBounce = 1500;
		[SerializeField] int SwimRotationLerp = 100;
		[SerializeField] int DamageBlinkRate = 8;
		[SerializeField] int EyeBlinkRate = 360;

		// Config
		private AniCode Sleep = null;
		private AniCode Damaging = null;
		private AniCode Passout = null;
		private GroupCode Face = null;
		private AniCode FaceBlink = null;
		private AniCode Idle = null;
		private AniCode Walk = null;
		private AniCode Run = null;
		private AniCode JumpU = null;
		private AniCode JumpD = null;
		private AniCode Dash = null;
		private AniCode SquatIdle = null;
		private AniCode SquatMove = null;
		private AniCode SwimIdle = null;
		private AniCode SwimMove = null;
		private AniCode SwimDash = null;
		private AniCode Pound = null;
		private AniCode Climb = null;
		private AniCode Roll = null;
		private AniCode Fly = null;
		private AniCode[] Attacks = null;
		private AniCode[] Attacks_Move = null;
		private AniCode[] Attacks_Air = null;
		private AniCode[] Attacks_Water = null;

		// Data
		private AniCode CurrentAni = null;
		private float TargetRotation = 0f;
		private int CurrentAniFrame = 0;
		private int CurrentCode = 0;
		private int CurrentBounce = 1000;
		private int LastCellHeight = Const.CELL_SIZE;
		private int LastRequireBounceFrame = int.MinValue;
		private int BlinkingTime = int.MinValue;
		private int DamagingTime = int.MinValue;
		private int AnimationCode = 0;
		private int AnimationFrame = 0;
		private int AnimationLoopStart = 0;
		private int PrevSleepAmount = 0;
		private bool AnimationFlipX = false;
		private bool AnimationFlipY = false;


		#endregion




		#region --- MSG ---


		public void OnActived (eCharacter source) {
			Character = source;
			string name = Character.GetType().Name;
			if (name.StartsWith('e')) name = name[1..];

			Idle = new($"_a{name}.Idle");
			if (Idle.Code == 0) Idle = new($"_a{name}");
			Walk = new($"_a{name}.Walk", Idle);
			Run = new($"_a{name}.Run", Walk);
			var jump = new AniCode($"_a{name}.Jump", Idle);
			JumpU = new($"_a{name}.JumpU", jump);
			JumpD = new($"_a{name}.JumpD", jump);
			Roll = new($"_a{name}.Roll", Run, Idle);
			Dash = new($"_a{name}.Dash", Roll);
			var squat = new AniCode($"_a{name}.Squat", Idle);
			SquatIdle = new($"_a{name}.SquatIdle", squat);
			SquatMove = new($"_a{name}.SquatMove", squat);
			var swim = new AniCode($"_a{name}.Swim", Run);
			SwimIdle = new($"_a{name}.SwimIdle", swim);
			SwimMove = new($"_a{name}.SwimMove", swim);
			SwimDash = new($"_a{name}.SwimDash", swim);
			Pound = new($"_a{name}.Pound", Idle);
			Climb = new($"_a{name}.Climb", Idle);
			Fly = new($"_a{name}.Fly", Run);

			Sleep = new($"_a{name}.Sleep", Idle);
			Damaging = new($"_a{name}.Damage", Idle);
			Passout = new($"_a{name}.Passout", Idle);
			Face = new($"{name}.Face");
			FaceBlink = new($"{name}.Face.Blink");

			Attacks = AniCode.GetAnimationArray($"_a{name}.Attack", -1);
			Attacks_Move = AniCode.GetAnimationArray($"_a{name}.AttackMove", -1, Attacks);
			Attacks_Air = AniCode.GetAnimationArray($"_a{name}.AttackAir", -1, Attacks);
			Attacks_Water = AniCode.GetAnimationArray($"_a{name}.AttackWater", -1, Attacks);

		}


		public void FrameUpdate () {

			int frame = Game.GlobalFrame;

			// Blink
			if (frame < BlinkingTime && (BlinkingTime - frame) % DamageBlinkRate < DamageBlinkRate / 2) return;

			// Damage
			if (frame < DamagingTime) {
				//var dCell = CellRenderer.Draw_Animation(
				CellRenderer.Draw_Animation(
					Damaging.Code,
					Character.X, Character.Y,
					500, 0, 0,
					Character.Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame,
					Damaging.LoopStart
				);
				//int scale = (DamagingTime - frame).PingPong(7) * 40 + 1000;
				//dCell.Width = dCell.Width * scale / 1000;
				//dCell.Height = dCell.Height * scale / 1000;
				return;
			}

			// Draw
			switch (Character.CharacterState) {
				case CharacterState.GamePlay:
					DrawBody();
					DrawFace();
					break;
				case CharacterState.Animate:
					CellRenderer.Draw_Animation(
						AnimationCode,
						Character.X,
						Character.Y,
						500, 0, (int)TargetRotation,
						AnimationFlipX ? Const.ORIGINAL_SIZE_NEGATAVE : Const.ORIGINAL_SIZE,
						AnimationFlipY ? Const.ORIGINAL_SIZE_NEGATAVE : Const.ORIGINAL_SIZE,
						AnimationFrame,
						AnimationLoopStart
					);
					break;
				case CharacterState.Sleep:
					DrawSleep();
					break;
				case CharacterState.Passout:
					CellRenderer.Draw_Animation(
						Passout.Code,
						Character.X, Character.Y,
						500, 0, 0,
						Character.Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame,
						Passout.LoopStart
					);
					break;
			}
		}


		private void DrawBody () {

			var movement = Character.Movement;
			var movementState = Character.MovementState;
			var ani = Idle;
			int frame = Game.GlobalFrame;

			// Get Ani
			if (Character.Attackness.IsAttacking) {
				// Attack
				var attacks = Attacks;
				switch (movementState) {
					case MovementState.SwimDash:
					case MovementState.SwimIdle:
					case MovementState.SwimMove:
						attacks = Attacks_Water;
						break;
					case MovementState.JumpDown:
					case MovementState.JumpUp:
					case MovementState.Roll:
						if (Character.InAir) attacks = Attacks_Air;
						break;
					case MovementState.Walk:
					case MovementState.Run:
						attacks = Attacks_Move;
						break;
				}
				if (attacks.Length > 0) {
					ani = attacks[Character.Attackness.Combo.Clamp(0, attacks.Length - 1)];
				}
			} else {
				// Movement
				ani = movementState switch {
					MovementState.Walk => Walk,
					MovementState.Run => Run,
					MovementState.JumpUp => JumpU,
					MovementState.JumpDown => JumpD,
					MovementState.SwimIdle => SwimIdle,
					MovementState.SwimMove => SwimMove,
					MovementState.SwimDash => SwimDash,
					MovementState.SquatIdle => SquatIdle,
					MovementState.SquatMove => SquatMove,
					MovementState.Dash => Dash,
					MovementState.Roll => Roll,
					MovementState.Pound => Pound,
					MovementState.Climb => Climb,
					MovementState.Fly => Fly,
					_ => Idle,
				};
			}

			// Rotation
			int pivotY = 0;
			int offsetY = 0;
			if (movement.UseFreeStyleSwim && Character.InWater && !Character.IsGrounded) {
				TargetRotation = Quaternion.LerpUnclamped(
					Quaternion.Euler(0, 0, TargetRotation),
					Quaternion.FromToRotation(
						Vector3.up, new(-movement.LastMoveDirection.x, movement.LastMoveDirection.y)
					),
					SwimRotationLerp / 1000f
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
			bool isPounding = movementState == MovementState.Pound;
			bool isClimbing = movementState == MovementState.Climb;
			bool isSquating = movementState == MovementState.SquatIdle || movementState == MovementState.SquatMove;
			var cell = CellRenderer.Draw_Animation(
				CurrentAni.Code,
				Character.X, Character.Y + offsetY, 500, pivotY, (int)TargetRotation,
				movement.FacingRight || isPounding || isClimbing ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
				Const.ORIGINAL_SIZE,
				CurrentAniFrame,
				ani.LoopStart
			);
			CurrentCode = CellRenderer.LastDrawnID;
			LastCellHeight = cell.Height;

			// Bouncy
			if (Bouncy > 0) {
				int bounce = 1000;
				int duration = BounceAmounts.Length;
				bool reverse = false;
				if (frame < LastRequireBounceFrame + duration) {
					bounce = BounceAmounts[frame - LastRequireBounceFrame];
				} else if (isPounding) {
					bounce = PoundingBounce;
				} else if (!isPounding && Character.IsGrounded && frame.InRangeExculde(movement.LastPoundingFrame, movement.LastPoundingFrame + duration)) {
					bounce = BounceAmountsBig[frame - movement.LastPoundingFrame];
				} else if (isSquating && frame.InRangeExculde(movement.LastSquatFrame, movement.LastSquatFrame + duration)) {
					bounce = BounceAmounts[frame - movement.LastSquatFrame];
				} else if (Character.IsGrounded && frame.InRangeExculde(movement.LastGroundFrame, movement.LastGroundFrame + duration)) {
					bounce = BounceAmounts[frame - movement.LastGroundFrame];
				} else if (!isSquating && frame.InRangeExculde(movement.LastSquatingFrame, movement.LastSquatingFrame + duration)) {
					bounce = BounceAmounts[frame - movement.LastSquatingFrame];
					reverse = true;
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
			if (ani != Climb) {
				// Normal
				CurrentAniFrame++;
			} else {
				// Climb
				int climbVelocity = movement.IntendedY != 0 ? movement.IntendedY : movement.IntendedX;
				if (climbVelocity > 0) {
					CurrentAniFrame++;
				} else if (climbVelocity < 0) {
					CurrentAniFrame--;
				}
			}

		}


		private void DrawFace () {

			if (
				Face.Count <= 0 ||
				!CellRenderer.TryGetMeta(CurrentCode, out var meta) ||
				!meta.Head.IsVailed ||
				!meta.Head.Front
			) return;

			int bounce = Mathf.Abs(CurrentBounce);
			int offsetY;
			if (CurrentBounce > 0) {
				offsetY = (meta.Head.Y + meta.Head.Height) * bounce / 1000;
			} else {
				offsetY = meta.Head.Y + meta.Head.Height;
				offsetY += offsetY * (1000 - bounce) / 1000;
			}
			var faceID = Face[FaceIndex.UMod(Face.Count)];
			CellRenderer.Draw_9Slice(
				Game.GlobalFrame % EyeBlinkRate > 8 ? faceID : FaceBlink.Code,
				Character.X - meta.SpriteWidth / 2 +
					(Character.Movement.FacingRight ?
						meta.Head.X :
						meta.SpriteWidth - (meta.Head.X + meta.Head.Width)
					),
				Character.Y + offsetY,
				0, 1000, 0,
				meta.Head.Width,
				Const.ORIGINAL_SIZE
			);
		}


		private void DrawSleep () {
			var backCell = CellRenderer.Draw_Animation(
				Sleep.Code,
				Character.X, Character.Y,
				Const.ORIGINAL_SIZE,
				Const.ORIGINAL_SIZE,
				Game.GlobalFrame,
				Sleep.LoopStart
			);
			var cell = CellRenderer.Draw_Animation(
				Sleep.Code,
				Character.X, Character.Y,
				Const.ORIGINAL_SIZE,
				Const.ORIGINAL_SIZE,
				Game.GlobalFrame,
				Sleep.LoopStart
			);
			if (CellRenderer.TryGetSprite(Sleep.Code, out var sprite) && sprite.GlobalBorder.Down != 0) {
				cell.Y -= sprite.GlobalBorder.Down;
				backCell.Y -= sprite.GlobalBorder.Down;
			}
			backCell.Color.a = 128;
			// Fill
			if (Character.SleepAmount < 1000) {
				cell.Shift.Up = Util.Remap(90, 0, 0, 1000, Character.SleepFrame);
			} else if (Character.SleepAmount >= 1000 && PrevSleepAmount < 1000) {
				// Spawn Particle
				var rect = cell.Rect;
				if (Game.Current.TryAddEntity(
					SLEEP_PARTICLE_CODE,
					rect.x + rect.width / 2,
					rect.y + rect.height / 2,
					out var particle
				)) {
					particle.Width = Const.CELL_SIZE * 2;
					particle.Height = Const.CELL_SIZE * 2;
				}
			}
			PrevSleepAmount = Character.SleepAmount;
		}


		#endregion




		#region --- API ---


		public void Bounce () => LastRequireBounceFrame = Game.GlobalFrame;


		public void Blink (int duration) => BlinkingTime = Game.GlobalFrame + duration;


		public void Damage (int duration) => DamagingTime = Game.GlobalFrame + duration;


		public void UpdateAnimation (int code, int frame, int loopStart = int.MinValue, bool flipX = false, bool flipY = false) {
			AnimationCode = code;
			AnimationFrame = frame;
			AnimationLoopStart = loopStart;
			AnimationFlipX = flipX;
			AnimationFlipY = flipY;
		}


		#endregion




	}
}