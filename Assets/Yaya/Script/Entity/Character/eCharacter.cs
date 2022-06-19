using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[MapEditorGroup("Character")]
	public abstract class eCharacter : eRigidbody {




		#region --- SUB ---


		public enum State {
			General = 0,
			Animate = 1,
			Sleep = 2,
			Passout = 3,
		}


		#endregion




		#region --- VAR ---


		// Api
		public override int PhysicsLayer => YayaConst.LAYER_CHARACTER;
		public override int CollisionMask => YayaConst.MASK_SOLID;
		public override bool CarryRigidbodyOnTop => false;
		public override bool IsInAir => base.IsInAir && !Movement.IsClimbing;
		public override int AirDragX => 0;
		public override int AirDragY => 0;
		public override bool IgnoreRiseGravityShift => true;
		public bool InBuilding { get; private set; } = false;
		public State CharacterState { get; set; } = State.General;
		protected int PassoutFrame { get; private set; } = int.MinValue;

		// Behaviour
		public Movement Movement { get; private set; }
		public Action Action { get; private set; }
		public Health Health { get; private set; }
		public Attackness Attackness { get; private set; }
		public CharacterRenderer Renderer { get; private set; }

		// Data
		private static readonly HitInfo[] c_DamageCheck = new HitInfo[16];
		private int DamageEndFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void OnInitialize (Game game) {

			base.OnInitialize(game);
			string typeName = GetType().Name;
			if (typeName.StartsWith("e")) typeName = typeName[1..];

			// Movement
			var movement = game.LoadMeta<Movement>($"{typeName}.Movement");
			Movement = movement ?? new();
			Movement.Initialize(this);

			// Renderer
			var renderer = game.LoadMeta<CharacterRenderer>($"{typeName}.Renderer");
			Renderer = renderer ?? new();
			Renderer.Initialize(this);

			// Action
			var action = game.LoadMeta<Action>($"{typeName}.Action");
			Action = action ?? new();
			Action.Initialize(this);

			// Health
			var health = game.LoadMeta<Health>($"{typeName}.Health");
			Health = health ?? new();
			Health.Initialize(this);

			// Attackness
			var attackness = game.LoadMeta<Attackness>($"{typeName}.Attackness");
			Attackness = attackness ?? new();
			Attackness.Initialize(this);

		}


		public override void OnActived () {
			base.OnActived();
			CharacterState = State.General;
			PassoutFrame = int.MinValue;
			DamageEndFrame = int.MinValue;
		}


		public override void FillPhysics () {
			switch (CharacterState) {
				case State.General:
					base.FillPhysics();
					break;
			}
		}


		public override void PhysicsUpdate () {

			int frame = Game.GlobalFrame;

			// Cache
			InBuilding = CellPhysics.Overlap(
				YayaConst.MASK_MAP, Rect, this, OperationMode.TriggerOnly, Const.BUILDING_TAG
			);

			// Level Damage Check
			int count = CellPhysics.OverlapAll(
				c_DamageCheck, YayaConst.MASK_DAMAGE, Rect, this, OperationMode.TriggerOnly
			);
			for (int i = 0; i < count; i++) {
				var hit = c_DamageCheck[i];
				TakeDamage(hit.Tag);
			}

			// Passout Check
			if (Health.EmptyHealth) {
				CharacterState = State.Passout;
				PassoutFrame = frame;
			}

			// Behaviour
			switch (CharacterState) {
				default:
				case State.General:
					if (frame > DamageEndFrame) {
						Action.Update();
						Attackness.Update();
						Movement.Update();
					} else {
						Movement.AntiKnockback();
					}
					Health.Update();
					base.PhysicsUpdate();
					break;
				case State.Animate:

					break;
				case State.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					break;
				case State.Passout:
					VelocityX = 0;
					base.PhysicsUpdate();
					break;
			}

		}


		public override void FrameUpdate () {
			Renderer.Update();
			base.FrameUpdate();
		}


		#endregion




		#region --- API ---


		public void TakeDamage (int damage) {
			if (Health.Damage(damage)) {
				DamageEndFrame = Game.GlobalFrame + Health.DamageFrameValue;
				Renderer.Blink(Health.InvincibleFrameDuration);
				Renderer.Damage(Health.DamageFrameValue);
				VelocityX = Movement.FacingRight ? -Health.KnockBackSpeedValue : Health.KnockBackSpeedValue;
			}
		}


		protected override bool GroundedCheck (RectInt rect) => base.GroundedCheck(rect);


		protected override bool InsideGroundCheck () => CellPhysics.Overlap(YayaConst.MASK_LEVEL, new(X, Y + Height / 4, 1, 1), this);


		#endregion




	}
}
