using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {
	public abstract partial class Character {




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

			public int Attack = 0;

			// API
			public AnimationSheet (System.Type characterType) {

				string name = characterType.Name;
				if (name.StartsWith('e')) name = name[1..];

				Idle = LoadAniCode($"{name}.Idle");
				if (Idle == 0) Idle = LoadAniCode($"{name}");
				Walk = LoadAniCode($"{name}.Walk", Idle);
				Run = LoadAniCode($"{name}.Run", Walk);
				int jump = LoadAniCode($"{name}.Jump", Idle);
				JumpU = LoadAniCode($"{name}.JumpU", jump);
				JumpD = LoadAniCode($"{name}.JumpD", jump);
				Roll = LoadAniCode($"{name}.Roll", Run);
				Dash = LoadAniCode($"{name}.Dash", Roll);
				Rush = LoadAniCode($"{name}.Rush", Dash);
				int squat = LoadAniCode($"{name}.Squat", Idle);
				SquatIdle = LoadAniCode($"{name}.SquatIdle", squat);
				SquatMove = LoadAniCode($"{name}.SquatMove", squat);
				int swim = LoadAniCode($"{name}.Swim", Run);
				SwimIdle = LoadAniCode($"{name}.SwimIdle", swim);
				SwimMove = LoadAniCode($"{name}.SwimMove", swim);
				SwimDash = LoadAniCode($"{name}.SwimDash", swim);
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
				PassOut = LoadAniCode($"{name}.PassOut");

				DoorFront = LoadAniCode($"{name}.DoorFront", Idle);
				DoorBack = LoadAniCode($"{name}.DoorBack", Idle);

				Attack = LoadAniCode($"{name}.Attack");

			}


			public int GetMovementCode (Character character) => character.IsRolling ? Roll :
				character.MovementState switch {
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
					CharacterMovementState.GrabTop => character.IntendedX != 0 ? GrabTopMove : GrabTop,
					CharacterMovementState.GrabSide => character.IntendedY != 0 ? GrabSideMove : GrabSide,
					CharacterMovementState.GrabFlip => GrabFlip,
					_ => Idle,
				};


			private static int LoadAniCode (string name, int failback = 0) => LoadAniCode(
				name.AngeHash(), failback
			);


			private static int LoadAniCode (int code, int failback = 0) {
				if (CellRenderer.HasSpriteGroup(code) || CellRenderer.HasSprite(code)) {
					return code;
				} else {
					return failback;
				}
			}


		}



		#endregion




		#region --- VAR ---


		// Data
		private static readonly Dictionary<int, AnimationSheet> AnimationSheetPool = new();
		private int CurrentSheetAni = 0;


		#endregion




		#region --- MSG ---


		[OnGameInitialize(-64)]
		public static void Initialize_Sheet () {
			AnimationSheetPool.Clear();
			foreach (var type in typeof(Character).AllChildClass()) {
				if (type.GetCustomAttribute<EntityAttribute.RenderWithSheetAttribute>() == null) continue;
				AnimationSheetPool.Add(type.AngeHash(), new AnimationSheet(type));
			}
		}


		private void FrameUpdate_SheetRendering () {

			if (!AnimationSheetPool.TryGetValue(TypeID, out var sheet)) return;

			// Damage
			if (TakingDamage) {
				CellRenderer.DrawAnimation(
					sheet.Damaging,
					X, Y, 500, 0, 0,
					FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
					Const.ORIGINAL_SIZE,
					Game.GlobalFrame
				);
				return;
			}

			// Door
			if (Teleporting) {
				LastRequireBounceFrame = int.MinValue;
				CellRenderer.DrawAnimation(
					TeleportEndFrame > 0 ? sheet.DoorFront : sheet.DoorBack,
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
					var cell = DrawSheetBody(sheet);
					BounceCellForSheet(cell, CurrentRenderingBounce);
					break;
				case CharacterState.Sleep:
					var sleepCell = CellRenderer.DrawAnimation(
						sheet.Sleep, X, Y, 500, 0, 0,
						Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE, Game.GlobalFrame
					);
					if (CellRenderer.TryGetSprite(sheet.Sleep, out var sleepSprite)) {
						if (sleepSprite.GlobalBorder.down > 0) {
							sleepCell.Y -= sleepSprite.GlobalBorder.down;
						}
					}
					break;
				case CharacterState.PassOut:
					CellRenderer.DrawAnimation(
						sheet.PassOut,
						X, Y,
						500, 0, 0,
						FacingRight ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE,
						Const.ORIGINAL_SIZE,
						Game.GlobalFrame
					);
					break;
			}
		}


		private Cell DrawSheetBody (AnimationSheet sheet) {

			int ani = sheet.Idle;

			// Get Ani
			if (!IsAttacking) {
				// Movement
				ani = sheet.GetMovementCode(this);
			} else {
				// Attack
				if (sheet.Attack != 0) {
					ani = sheet.Attack;
					if (Game.GlobalFrame <= LastAttackFrame) CurrentAnimationFrame = 0;
				}
			}

			// Reset Frame when Switch Ani
			if (CurrentSheetAni != ani) {
				CurrentAnimationFrame = 0;
				CurrentSheetAni = ani;
			}

			// Draw
			int pivotX = 500;
			int pivotY = 0;
			if (CellRenderer.TryGetSprite(ani, out var sprite)) {
				pivotX = sprite.PivotX;
				pivotY = sprite.PivotY;
			}
			return CellRenderer.DrawAnimation(
				ani, X, Y, pivotX, pivotY, 0,
				FacingRight || IsClimbing || IsPounding ? Const.ORIGINAL_SIZE : Const.ORIGINAL_SIZE_NEGATAVE, Const.ORIGINAL_SIZE,
				CurrentAnimationFrame
			);
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
}