using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[System.Serializable]
	public class CharacterRenderer : EntityBehaviour<eCharacter> {




		#region --- SUB ---


		private class MovementCodes {
			public AniCode Idle { get; init; } = null;
			public AniCode Walk { get; init; } = null;
			public AniCode Run { get; init; } = null;
			public AniCode JumpU { get; init; } = null;
			public AniCode JumpD { get; init; } = null;
			public AniCode Dash { get; init; } = null;
			public AniCode SquatIdle { get; init; } = null;
			public AniCode SquatMove { get; init; } = null;
			public AniCode SwimIdle { get; init; } = null;
			public AniCode SwimMove { get; init; } = null;
			public AniCode SwimDash { get; init; } = null;
			public AniCode Pound { get; init; } = null;
			public AniCode Climb { get; init; } = null;
			public AniCode Roll { get; init; } = null;
		}


		private class GeneralCodes {
			public AniCode Sleep { get; init; } = null;
			public AniCode Damage { get; init; } = null;
			public AniCode Passout { get; init; } = null;
			public GroupCode Face { get; init; } = null;
			public int FaceBlink { get; init; } = 0;
			public AniCode[] Attacks { get; init; } = null;
		}


		private class AniCode {
			public readonly int Code = 0;
			public readonly int LoopStart = 0;
			public readonly bool UsingFailback = false;
			public AniCode (string name, params string[] failbacks) : this(name.AngeHash(), failbacks) { }
			public AniCode (int code, params string[] failbacks) {
				if (CellRenderer.TryGetSprite(code, out _, 0)) {
					Code = code;
					UsingFailback = false;
					if (CellRenderer.TryGetSpriteChain(code, out var chain)) {
						LoopStart = chain.LoopStart;
					}
				} else {
					foreach (var failback in failbacks) {
						code = failback.AngeHash();
						if (CellRenderer.TryGetSprite(code, out _, 0)) {
							Code = code;
							UsingFailback = true;
							if (CellRenderer.TryGetSpriteChain(code, out var chain)) {
								LoopStart = chain.LoopStart;
							}
							break;
						}
					}
				}
			}
			public static AniCode[] GetAnimationArray (string keyName) {
				var result = new List<AniCode>();
				int code = keyName.AngeHash();
				if (CellRenderer.TryGetSprite(code, out _, 0)) {
					result.Add(new AniCode(code));
				}
				for (char c = 'A'; c <= 'Z'; c++) {
					code = $"{keyName}{c}".AngeHash();
					if (CellRenderer.TryGetSprite(code, out _, 0)) {
						result.Add(new AniCode(code));
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

		// Data
		private MovementCodes MovementAni = null;
		private GeneralCodes GeneralAni = null;
		private AniCode CurrentAni = null;
		private float TargetSwimRotation = 0f;
		private int CurrentAniFrame = 0;
		private int CurrentCode = 0;
		private int CurrentBounce = 1000;
		private int LastCellHeight = Const.CELL_SIZE;
		private int LastRequireBounceFrame = int.MinValue;
		private int BlinkingTime = int.MinValue;
		private int DamagingTime = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void Initialize (eCharacter source) {
			base.Initialize(source);
			string name = Source.GetType().Name;
			if (name.StartsWith('e')) name = name[1..];
			MovementAni = new() {
				Idle = new($"_a{name}.Idle", $"_a{name}"),
				Walk = new($"_a{name}.Walk", $"_a{name}.Run", $"_a{name}.Idle"),
				Run = new($"_a{name}.Run", $"_a{name}.Walk", $"_a{name}.Idle"),
				JumpU = new($"_a{name}.JumpU", $"_a{name}.Idle"),
				JumpD = new($"_a{name}.JumpD", $"_a{name}.Idle"),
				Roll = new($"_a{name}.Roll", $"_a{name}.Run"),
				Dash = new($"_a{name}.Dash", $"_a{name}.Roll"),
				SquatIdle = new($"_a{name}.SquatIdle", $"_a{name}.Idle"),
				SquatMove = new($"_a{name}.SquatMove", $"_a{name}.Walk"),
				SwimIdle = new($"_a{name}.SwimIdle", $"_a{name}.Swim", $"_a{name}.Idle"),
				SwimMove = new($"_a{name}.SwimMove", $"_a{name}.SwimIdle", $"_a{name}.Swim", $"_a{name}.Run"),
				SwimDash = new($"_a{name}.SwimDash", $"_a{name}.SwimMove", $"_a{name}.Swim", $"_a{name}.Dash"),
				Pound = new($"_a{name}.Pound", $"_a{name}.Idle"),
				Climb = new($"_a{name}.Climb", $"_a{name}.Walk"),
			};
			GeneralAni = new() {
				Sleep = new($"_a{name}.Sleep", $"_a{name}.Idle", $"_a{name}"),
				Damage = new($"_a{name}.Damage", $"_a{name}.Idle", $"_a{name}"),
				Passout = new($"_a{name}.Passout", $"_a{name}.Idle", $"_a{name}"),
				Attacks = AniCode.GetAnimationArray($"_a{name}.Attack"),
				Face = new($"{name}.Face"),
				FaceBlink = new AniCode($"{name}.Face.Blink", $"{name}.Face").Code,
			};
		}


		public override void Update () {

			base.Update();
			int frame = Game.GlobalFrame;

			// Blink
			if (frame < BlinkingTime && (BlinkingTime - frame) % DamageBlinkRate < DamageBlinkRate / 2) return;

			// Damage
			if (frame < DamagingTime) {
				ref var cell = ref CellRenderer.Draw_Animation(
					GeneralAni.Damage.Code,
					Source.X, Source.Y,
					500, 0, 0,
					Source.Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame,
					GeneralAni.Damage.LoopStart
				);
				int damageScl = frame.PingPong(3) * 32 - 16 + DamageScale;
				cell.Width = cell.Width * damageScl / 1000;
				cell.Height = cell.Height * damageScl / 1000;
				return;
			}

			// Draw
			switch (Source.CharacterState) {
				case eCharacter.State.General:
					Draw();
					DrawFace();
					break;
				case eCharacter.State.Animate:


					break;
				case eCharacter.State.Sleep:
					CellRenderer.Draw_Animation(
						GeneralAni.Sleep.Code,
						Source.X, Source.Y,
						Const.ORIGINAL_SIZE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame,
						GeneralAni.Sleep.LoopStart
					);
					break;
				case eCharacter.State.Passout:
					ref var cell = ref CellRenderer.Draw_Animation(
						GeneralAni.Passout.Code,
						Source.X, Source.Y,
						500, 0, 0,
						Source.Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame,
						GeneralAni.Passout.LoopStart
					);
					cell.Width = cell.Width * PassoutScale / 1000;
					cell.Height = cell.Height * PassoutScale / 1000;
					break;
			}

		}


		private void Draw () {

			AniCode ani;
			int frame = Game.GlobalFrame;

			// Movement
			var movement = Source.Movement;
			if (movement.IsClimbing) {
				ani = MovementAni.Climb;
			} else if (movement.IsPounding) {
				ani = MovementAni.Pound;
			} else if (movement.IsRolling) {
				ani = MovementAni.Roll;
			} else if (movement.IsDashing) {
				ani = !movement.IsGrounded && movement.InWater ? MovementAni.SwimDash : MovementAni.Dash;
			} else if (movement.IsSquating) {
				ani = movement.IsMoving ? MovementAni.SquatMove : MovementAni.SquatIdle;
			} else if (movement.InWater && !movement.IsGrounded) {
				ani = movement.IsMoving ? MovementAni.SwimMove : MovementAni.SwimIdle;
			} else if (movement.IsInAir) {
				ani = movement.FinalVelocityY > 0 ? MovementAni.JumpU : MovementAni.JumpD;
			} else {
				ani = movement.IsRunning ? MovementAni.Run : movement.IsMoving ? MovementAni.Walk : MovementAni.Idle;
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

			// Reset Frame when Switch Ani
			if (CurrentAni != ani) {
				CurrentAniFrame = 0;
				CurrentAni = ani;
			}

			// Draw
			ref var cell = ref CellRenderer.Draw_Animation(
				CurrentAni.Code,
				Source.X, Source.Y + offsetY, 500, pivotY, (int)TargetSwimRotation,
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
			if (ani != MovementAni.Climb) {
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
			CellRenderer.TryGetMeta(CurrentCode, out var m);
			if (
				Source.CharacterState != eCharacter.State.General ||
				GeneralAni.Face.Count <= 0 ||
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
					GeneralAni.Face[FaceIndex.UMod(GeneralAni.Face.Count)] :
					GeneralAni.FaceBlink,
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