using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[System.Serializable]
	public class CharacterRenderer : EntityBehaviour<eCharacter> {




		#region --- SUB ---


		public class AniCode {


			public int Code = 0;
			public int LoopStart = 0;


			public AniCode (string name, params string[] failbacks) : this(name.AngeHash(), failbacks) { }


			public AniCode (int code, params string[] failbacks) {
				if (Load(code)) return;
				foreach (var failback in failbacks) {
					code = failback.AngeHash();
					if (Load(code)) break;
				}
			}


			public bool Load (int nameCode) {
				if (CellRenderer.TryGetSpriteChain(nameCode, out var chain)) {
					Code = nameCode;
					LoopStart = chain.LoopStart;
					return true;
				} else if (CellRenderer.TryGetSprite(nameCode, out _)) {
					Code = nameCode;
					LoopStart = 0;
				}
				return false;
			}


			public static AniCode[] GetAnimationArray (string keyName, int defaultLoopStart = 0) {
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


		public override void OnActived (eCharacter source) {
			base.OnActived(source);
			string name = Source.GetType().Name;
			if (name.StartsWith('e')) name = name[1..];

			Idle = new($"_a{name}.Idle", $"_a{name}");
			Walk = new($"_a{name}.Walk", $"_a{name}.Run", $"_a{name}.Idle");
			Run = new($"_a{name}.Run", $"_a{name}.Walk", $"_a{name}.Idle");
			JumpU = new($"_a{name}.JumpU", $"_a{name}.Idle");
			JumpD = new($"_a{name}.JumpD", $"_a{name}.Idle");
			Roll = new($"_a{name}.Roll", $"_a{name}.Run");
			Dash = new($"_a{name}.Dash", $"_a{name}.Roll");
			SquatIdle = new($"_a{name}.SquatIdle", $"_a{name}.Idle");
			SquatMove = new($"_a{name}.SquatMove", $"_a{name}.Walk");
			SwimIdle = new($"_a{name}.SwimIdle", $"_a{name}.Swim", $"_a{name}.Idle");
			SwimMove = new($"_a{name}.SwimMove", $"_a{name}.SwimIdle", $"_a{name}.Swim", $"_a{name}.Run");
			SwimDash = new($"_a{name}.SwimDash", $"_a{name}.SwimMove", $"_a{name}.Swim", $"_a{name}.Dash");
			Pound = new($"_a{name}.Pound", $"_a{name}.Idle");
			Climb = new($"_a{name}.Climb", $"_a{name}.Walk");
			Fly = new($"_a{name}.Fly", $"_a{name}.Run");

			Sleep = new($"_a{name}.Sleep", $"_a{name}.Idle", $"_a{name}");
			Damaging = new($"_a{name}.Damage", $"_a{name}.Idle", $"_a{name}");
			Passout = new($"_a{name}.Passout", $"_a{name}.Idle", $"_a{name}");
			Face = new($"{name}.Face");
			FaceBlink = new AniCode($"{name}.Face.Blink", $"{name}.Face");

			Attacks = AniCode.GetAnimationArray($"_a{name}.Attack", -1);
			Attacks_Move = AniCode.GetAnimationArray($"_a{name}.AttackMove", -1);
			Attacks_Air = AniCode.GetAnimationArray($"_a{name}.AttackAir", -1);
			Attacks_Water = AniCode.GetAnimationArray($"_a{name}.AttackWater", -1);

		}


		public override void Update () {

			base.Update();
			int frame = Game.GlobalFrame;

			// Blink
			if (frame < BlinkingTime && (BlinkingTime - frame) % DamageBlinkRate < DamageBlinkRate / 2) return;

			// Damage
			if (frame < DamagingTime) {
				var cell = CellRenderer.Draw_Animation(
					Damaging.Code,
					Source.X, Source.Y,
					500, 0, 0,
					Source.Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
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
			switch (Source.CharacterState) {
				case eCharacter.State.General:
					DrawBody();
					DrawFace();
					break;
				case eCharacter.State.Animate:


					break;
				case eCharacter.State.Sleep: {
					CellRenderer.Draw_Animation(
						Sleep.Code,
						Source.X, Source.Y,
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
						Source.X, Source.Y,
						500, 0, 0,
						Source.Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
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

			var ani = Idle;
			int frame = Game.GlobalFrame;
			var movement = Source.Movement;
			var attackness = Source.Attackness;

			// Get Ani
			if (attackness.IsAttacking) {
				// Attack
				var attacks = Attacks;
				if (movement.InWater && Attacks_Water.Length > 0) {
					attacks = Attacks_Water;
				} else if (movement.InAir && Attacks_Air.Length > 0) {
					attacks = Attacks_Air;
				} else if (movement.IsMoving && Attacks_Move.Length > 0) {
					attacks = Attacks_Move;
				}
				if (attacks.Length > 0) {
					ani = attacks[attackness.Combo.Clamp(0, attacks.Length - 1)];
				}
			} else {
				// Movement
				if (movement.IsFlying) {
					ani = Fly;
				} else if (movement.IsClimbing) {
					ani = Climb;
				} else if (movement.IsPounding) {
					ani = Pound;
				} else if (movement.IsRolling) {
					ani = Roll;
				} else if (movement.IsDashing) {
					ani = !movement.IsGrounded && movement.InWater ? SwimDash : Dash;
				} else if (movement.IsSquating) {
					ani = movement.IsMoving ? SquatMove : SquatIdle;
				} else if (movement.InWater && !movement.IsGrounded) {
					ani = movement.IsMoving ? SwimMove : SwimIdle;
				} else if (movement.InAir) {
					ani = movement.FinalVelocityY > 0 ? JumpU : JumpD;
				} else if (movement.IsRunning) {
					ani = Run;
				} else if (movement.IsMoving) {
					ani = Walk;
				}
			}

			// Rotation
			int pivotY = 0;
			int offsetY = 0;
			if (movement.UseFreeStyleSwim && movement.InWater && !movement.IsGrounded) {
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
			var cell = CellRenderer.Draw_Animation(
				CurrentAni.Code,
				Source.X, Source.Y + offsetY, 500, pivotY, (int)TargetRotation,
				movement.FacingRight || movement.IsPounding || movement.IsClimbing ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
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
				} else if (movement.IsPounding) {
					bounce = PoundingBounce;
				} else if (!movement.IsPounding && movement.IsGrounded && frame.InRangeExculde(movement.LastPoundingFrame, movement.LastPoundingFrame + duration)) {
					bounce = BounceAmountsBig[frame - movement.LastPoundingFrame];
				} else if (movement.IsSquating && frame.InRangeExculde(movement.LastSquatFrame, movement.LastSquatFrame + duration)) {
					bounce = BounceAmounts[frame - movement.LastSquatFrame];
				} else if (movement.IsGrounded && frame.InRangeExculde(movement.LastGroundFrame, movement.LastGroundFrame + duration)) {
					bounce = BounceAmounts[frame - movement.LastGroundFrame];
				} else if (!movement.IsSquating && frame.InRangeExculde(movement.LastSquatingFrame, movement.LastSquatingFrame + duration)) {
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


			var movement = Source.Movement;
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
				Source.X - meta.SpriteWidth / 2 +
					(movement.FacingRight ?
						meta.Head.X :
						meta.SpriteWidth - (meta.Head.X + meta.Head.Width)
					),
				Source.Y + offsetY,
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