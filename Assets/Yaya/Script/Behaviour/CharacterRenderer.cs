using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[System.Serializable]
	public class CharacterRenderer {




		#region --- SUB ---


		public class AniCode {


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


		// Api
		public eCharacter Character { get; private set; } = null;
		public int FaceIndex { get; set; } = 0;

		// Ser
		[SerializeField] int[] BounceAmounts = new int[] { 500, 200, 100, 50, 25, 50, 100, 200, 500, };
		[SerializeField] int[] BounceAmountsBig = new int[] { 0, -600, -900, -1200, -1400, -1200, -900, -600, 0, };
		[SerializeField] int Bouncy = 150;
		[SerializeField] int PoundingBounce = 1500;
		[SerializeField] int DamageScale = 1150;
		[SerializeField] int PassoutScale = 1200;
		[SerializeField] int SwimRotationLerp = 100;
		[SerializeField] int DamageBlinkRate = 8;
		[SerializeField] int EyeBlinkRate = 360;

		// Config
		public AniCode Sleep { get; private set; } = null;
		public AniCode Damaging { get; private set; } = null;
		public AniCode Passout { get; private set; } = null;
		public GroupCode Face { get; private set; } = null;
		public AniCode FaceBlink { get; private set; } = null;
		public AniCode[] Attacks { get; private set; } = null;
		public AniCode[] Attacks_Move { get; private set; } = null;
		public AniCode[] Attacks_Air { get; private set; } = null;
		public AniCode[] Attacks_Water { get; private set; } = null;
		public AniCode Idle { get; private set; } = null;
		public AniCode Walk { get; private set; } = null;
		public AniCode Run { get; private set; } = null;
		public AniCode JumpU { get; private set; } = null;
		public AniCode JumpD { get; private set; } = null;
		public AniCode Dash { get; private set; } = null;
		public AniCode SquatIdle { get; private set; } = null;
		public AniCode SquatMove { get; private set; } = null;
		public AniCode SwimIdle { get; private set; } = null;
		public AniCode SwimMove { get; private set; } = null;
		public AniCode SwimDash { get; private set; } = null;
		public AniCode Pound { get; private set; } = null;
		public AniCode Climb { get; private set; } = null;
		public AniCode Roll { get; private set; } = null;
		public AniCode Fly { get; private set; } = null;

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


		public void Update () {

			int frame = Game.GlobalFrame;

			// Blink
			if (frame < BlinkingTime && (BlinkingTime - frame) % DamageBlinkRate < DamageBlinkRate / 2) return;

			// Damage
			if (frame < DamagingTime) {
				var cell = CellRenderer.Draw_Animation(
					Damaging.Code,
					Character.X, Character.Y,
					500, 0, 0,
					Character.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame,
					Damaging.LoopStart
				);
				int damageScl = frame.PingPong(3) * 32 - 16 + DamageScale;
				cell.Width = cell.Width * damageScl / 1000;
				cell.Height = cell.Height * damageScl / 1000;
				return;
			}

			// Draw
			switch (Character.CharacterState) {
				case eCharacter.State.General:
					DrawBody();
					DrawFace();
					break;
				case eCharacter.State.Animate:


					break;
				case eCharacter.State.Sleep: {
					CellRenderer.Draw_Animation(
					Sleep.Code,
					Character.X, Character.Y,
					Const.ORIGINAL_SIZE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame,
					Sleep.LoopStart
				);
					break;
				}
				case eCharacter.State.Passout: {
					var cell = CellRenderer.Draw_Animation(
						Passout.Code,
						Character.X, Character.Y,
						500, 0, 0,
						Character.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame,
						Passout.LoopStart
					);
					cell.Width = cell.Width * PassoutScale / 1000;
					cell.Height = cell.Height * PassoutScale / 1000;
					break;
				}
			}

		}


		private void DrawBody () {

			var movementState = Character.GetMovementState();
			var ani = Idle;
			int frame = Game.GlobalFrame;
			//var movement = Character.Movement;

			// Get Ani
			if (Character.IsAttacking) {
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
					ani = attacks[Character.AttackCombo.Clamp(0, attacks.Length - 1)];
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
			if (Character.UseFreeStyleSwim && Character.InWater && !Character.IsGrounded) {
				TargetRotation = Quaternion.LerpUnclamped(
					Quaternion.Euler(0, 0, TargetRotation),
					Quaternion.FromToRotation(
						Vector3.up, new(-Character.LastMoveDirectionX, Character.LastMoveDirectionY)
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
				Character.FacingRight || isPounding || isClimbing ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
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
				} else if (!isPounding && Character.IsGrounded && frame.InRangeExculde(Character.LastPoundingFrame, Character.LastPoundingFrame + duration)) {
					bounce = BounceAmountsBig[frame - Character.LastPoundingFrame];
				} else if (isSquating && frame.InRangeExculde(Character.LastSquatFrame, Character.LastSquatFrame + duration)) {
					bounce = BounceAmounts[frame - Character.LastSquatFrame];
				} else if (Character.IsGrounded && frame.InRangeExculde(Character.LastGroundFrame, Character.LastGroundFrame + duration)) {
					bounce = BounceAmounts[frame - Character.LastGroundFrame];
				} else if (!isSquating && frame.InRangeExculde(Character.LastSquatingFrame, Character.LastSquatingFrame + duration)) {
					bounce = BounceAmounts[frame - Character.LastSquatingFrame];
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
				int climbVelocity = Character.IntendedY != 0 ? Character.IntendedY : Character.IntendedX;
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


			//var movement = Character.Movement;
			int bounce = Mathf.Abs(CurrentBounce);
			int offsetY;
			if (CurrentBounce > 0) {
				offsetY = (meta.Head.Y + meta.Head.Height) * bounce / 1000;
			} else {
				offsetY = meta.Head.Y + meta.Head.Height;
				offsetY += offsetY * (1000 - bounce) / 1000;
			}
			CellRenderer.Draw_9Slice(
				Game.GlobalFrame % EyeBlinkRate > 8 ?
					Face[FaceIndex.UMod(Face.Count)] :
					FaceBlink.Code,
				Character.X - meta.SpriteWidth / 2 +
					(Character.FacingRight ?
						meta.Head.X :
						meta.SpriteWidth - (meta.Head.X + meta.Head.Width)
					),
				Character.Y + offsetY,
				0, 1000, 0,
				meta.Head.Width,
				Const.ORIGINAL_SIZE
			);
		}


		#endregion




		#region --- API ---


		public void Bounce () => LastRequireBounceFrame = Game.GlobalFrame;


		public void Blink (int duration) => BlinkingTime = Game.GlobalFrame + duration;


		public void Damage (int duration) => DamagingTime = Game.GlobalFrame + duration;


		#endregion




	}
}