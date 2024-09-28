using System.Collections;
using System.Collections.Generic;
using System.Reflection;


namespace AngeliA;
public abstract class SheetCharacter : Character {




	#region --- SUB ---


	private class AnimationSheet {

		public int Sleep = 0;
		public int Damaging = 0;
		public int PassOut = 0;
		public int DoorFront = 0;
		public int DoorBack = 0;

		public int Idle = 0;
		public int Walk = 0;
		public int Run = 0;
		public int JumpU = 0;
		public int JumpD = 0;
		public int Dash = 0;
		public int Rush = 0;
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
		public int GrabTopMove = 0;
		public int GrabSide = 0;
		public int GrabSideMove = 0;
		public int GrabFlip = 0;
		public int[] Attack = [];

		private static readonly List<int> CacheList = [];

		public AnimationSheet (System.Type characterType) {

			string name = characterType.Name;

			Idle = LoadAniCode($"{name}.Idle");
			Walk = LoadAniCode($"{name}.Walk", Idle);
			Run = LoadAniCode($"{name}.Run", Walk);
			JumpU = LoadAniCode($"{name}.JumpU", Idle);
			JumpD = LoadAniCode($"{name}.JumpD", Idle);
			Roll = LoadAniCode($"{name}.Roll", Run);
			Dash = LoadAniCode($"{name}.Dash", Roll);
			Rush = LoadAniCode($"{name}.Rush", Dash);
			SquatIdle = LoadAniCode($"{name}.SquatIdle", Idle);
			SquatMove = LoadAniCode($"{name}.SquatMove", Idle);
			SwimIdle = LoadAniCode($"{name}.SwimIdle", Idle);
			SwimMove = LoadAniCode($"{name}.SwimMove", Walk);
			SwimDash = LoadAniCode($"{name}.SwimDash", Run);
			Pound = LoadAniCode($"{name}.Pound", Idle);
			Climb = LoadAniCode($"{name}.Climb", Idle);
			Fly = LoadAniCode($"{name}.Fly", Run);
			Slide = LoadAniCode($"{name}.Slide", JumpD);
			GrabTop = LoadAniCode($"{name}.GrabTop", JumpD);
			GrabTopMove = LoadAniCode($"{name}.GrabTopMove", GrabTop);
			GrabSide = LoadAniCode($"{name}.GrabSide", Slide);
			GrabSideMove = LoadAniCode($"{name}.GrabSideMove", GrabSide);
			GrabFlip = LoadAniCode($"{name}.GrabFlip", Roll);
			Sleep = LoadAniCode($"{name}.Sleep", Idle);
			Damaging = LoadAniCode($"{name}.Damage", Idle);
			PassOut = LoadAniCode($"{name}.PassOut", Idle);
			DoorFront = LoadAniCode($"{name}.DoorFront", Idle);
			DoorBack = LoadAniCode($"{name}.DoorBack", Idle);
			Attack = LoadAniCodeGroup($"{name}.Attack");

		}

		public int GetMovementCode (Character character) =>
			character.Movement.IsRolling ? Roll :
			character.Movement.MovementState switch {
				CharacterMovementState.Idle => Idle,
				CharacterMovementState.Walk => Walk,
				CharacterMovementState.Run => Run,
				CharacterMovementState.JumpUp => JumpU,
				CharacterMovementState.JumpDown => JumpD,
				CharacterMovementState.SwimIdle => SwimIdle,
				CharacterMovementState.SwimMove => SwimMove,
				CharacterMovementState.SquatIdle => SquatIdle,
				CharacterMovementState.SquatMove => SquatMove,
				CharacterMovementState.Dash => Dash,
				CharacterMovementState.Rush => Rush,
				CharacterMovementState.Pound => Pound,
				CharacterMovementState.Climb => Climb,
				CharacterMovementState.Fly => Fly,
				CharacterMovementState.Slide => Slide,
				CharacterMovementState.GrabTop => character.Movement.IntendedX != 0 ? GrabTopMove : GrabTop,
				CharacterMovementState.GrabSide => character.Movement.IntendedY != 0 ? GrabSideMove : GrabSide,
				CharacterMovementState.GrabFlip => GrabFlip,
				_ => Idle,
			};

		private static int LoadAniCode (string name, int failback = 0) {
			int code = name.AngeHash();
			if (Renderer.HasSpriteGroup(code) || Renderer.HasSprite(code)) {
				return code;
			} else {
				return failback;
			}
		}

		private static int[] LoadAniCodeGroup (string name) {
			CacheList.Clear();
			int code = name.AngeHash();
			if (Renderer.HasSpriteGroup(code)) {
				CacheList.Add(code);
			}
			for (char c = 'A'; c <= 'Z'; c++) {
				code = $"{name}.{c}".AngeHash();
				if (Renderer.HasSpriteGroup(code)) {
					CacheList.Add(code);
				} else {
					break;
				}
			}
			var result = CacheList.ToArray();
			CacheList.Clear();
			return result;
		}

	}



	#endregion




	#region --- VAR ---


	// Api
	protected Cell RenderedCell { get; private set; } = null;
	public override int DespawnAfterPassoutDelay => 60;

	// Data
	private static readonly Dictionary<int, AnimationSheet> AnimationSheetPool = [];
	private int CurrentSheetAni = 0;


	#endregion




	#region --- MSG ---


	[OnMainSheetReload]
	internal static void OnMainSheetReload_Sheet () {
		AnimationSheetPool.Clear();
		foreach (var type in typeof(Character).AllChildClass()) {
			if (!type.IsSubclassOf(typeof(SheetCharacter))) continue;
			AnimationSheetPool.Add(type.AngeHash(), new AnimationSheet(type));
		}
	}


	protected override void RenderCharacter () {

		if (!AnimationSheetPool.TryGetValue(TypeID, out var sheet)) return;

		// Damage
		if (Health.TakingDamage) {
			Renderer.DrawAnimation(
				sheet.Damaging,
				X, Y, 500, 0, 0,
				Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
				Const.ORIGINAL_SIZE,
				Game.GlobalFrame
			);
			return;
		}

		// Door
		if (Teleporting) {
			LastRequireBounceFrame = int.MinValue;
			Renderer.DrawAnimation(
				TeleportEndFrame > 0 ? sheet.DoorFront : sheet.DoorBack,
				X, Y, 500, 0, 0,
				Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
				Const.ORIGINAL_SIZE,
				Game.GlobalFrame
			);
			return;
		}

		// Draw
		RenderedCell = null;
		switch (CharacterState) {
			default:
			case CharacterState.GamePlay:
				RenderedCell = DrawSheetBody(sheet);
				BounceCellForSheet(RenderedCell, CurrentRenderingBounce);
				break;
			case CharacterState.Sleep:
				RenderedCell = Renderer.DrawAnimation(
					sheet.Sleep, X, Y, 500, 0, 0,
					Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE, Game.GlobalFrame
				);
				if (Renderer.TryGetSprite(sheet.Sleep, out var sleepSprite)) {
					if (sleepSprite.GlobalBorder.down > 0) {
						RenderedCell.Y -= sleepSprite.GlobalBorder.down;
					}
				}
				break;
			case CharacterState.PassOut:
				// Blink for Passout
				if (DespawnAfterPassoutDelay >= 0 && (Game.GlobalFrame - PassOutFrame) % 8 >= 4) {
					break;
				}
				// Draw Passout
				RenderedCell = Renderer.DrawAnimation(
					sheet.PassOut,
					X, Y,
					500, 0, 0,
					Movement.FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame
				);
				break;
		}
	}


	private Cell DrawSheetBody (AnimationSheet sheet) {

		int ani = sheet.Idle;

		// Get Ani
		if (!Attackness.IsAttacking) {
			// Movement
			ani = sheet.GetMovementCode(this);
		} else {
			// Attack
			if (sheet.Attack.Length != 0) {
				ani = sheet.Attack[Attackness.AttackStyleIndex.Clamp(0, sheet.Attack.Length - 1)];
				if (Game.GlobalFrame <= Attackness.LastAttackFrame) {
					CurrentAnimationFrame = 0;
				}
			}
		}

		// Reset Frame when Switch Ani
		if (CurrentSheetAni != ani) {
			CurrentAnimationFrame = 0;
			CurrentSheetAni = ani;
		}

		// Draw
		if (Renderer.TryGetSprite(ani, out var sprite)) {
			return Renderer.DrawAnimation(
				ani, X, Y, sprite.PivotX, sprite.PivotY, 0,
				Movement.FacingRight || Movement.IsClimbing || Movement.IsPounding ? sprite.GlobalWidth : -sprite.GlobalWidth,
				sprite.GlobalHeight, CurrentAnimationFrame
			);
		}
		return Cell.EMPTY;
	}


	private void BounceCellForSheet (Cell cell, int bounce) {
		bool reverse = bounce < 0;
		bounce = bounce.Abs();
		if (bounce == 1000) return;
		if (reverse) {
			cell.Width = cell.Width * bounce / 1000;
			cell.Height += cell.Height * (1000 - bounce) / 1000;
		} else {
			cell.Width += cell.Width * (1000 - bounce) / 1000;
			cell.Height = cell.Height * bounce / 1000;
		}
	}


	#endregion




}