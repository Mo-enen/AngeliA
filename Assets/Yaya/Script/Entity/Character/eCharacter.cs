using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[MapEditorGroup("Character")]
	public abstract class eCharacter : eYayaRigidbody, IAttackReceiver {




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
		public State CharacterState { get; private set; } = State.General;
		public int PassoutFrame { get; private set; } = int.MinValue;

		// Behaviour
		public CharacterMovement Movement { get; private set; }
		public CharacterRenderer Renderer { get; private set; }
		public Action Action { get; private set; }
		public Health Health { get; private set; }
		public Attackness Attackness { get; private set; }

		// Data
		private static readonly HitInfo[] c_DamageCheck = new HitInfo[16];


		#endregion




		#region --- MSG ---


		public override void OnInitialize (Game game) {

			base.OnInitialize(game);
			string typeName = GetType().Name;
			if (typeName.StartsWith("e")) typeName = typeName[1..];

			// Movement
			var movement = game.LoadMeta<CharacterMovement>($"{typeName}.Movement");
			Movement = movement ?? new();

			// Renderer
			var renderer = game.LoadMeta<CharacterRenderer>($"{typeName}.Renderer");
			Renderer = renderer ?? new();

			// Action
			var action = game.LoadMeta<Action>($"{typeName}.Action");
			Action = action ?? new();

			// Health
			var health = game.LoadMeta<Health>($"{typeName}.Health");
			Health = health ?? new();

			// Attackness
			var attackness = game.LoadMeta<Attackness>($"{typeName}.Attackness");
			Attackness = attackness ?? new();

		}


		public override void OnActived () {
			base.OnActived();
			Movement.Initialize(this);
			Renderer.Initialize(this);
			Action.Initialize(this);
			Health.Initialize(this);
			Attackness.Initialize(this);
			CharacterState = State.General;
			PassoutFrame = int.MinValue;
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

			// Level Damage Check
			int count = CellPhysics.OverlapAll(
				c_DamageCheck, YayaConst.MASK_DAMAGE, Rect, this, OperationMode.TriggerOnly
			);
			for (int i = 0; i < count; i++) {
				var hit = c_DamageCheck[i];
				TakeDamage(hit.Tag);
			}

			// Passout Check
			if (CharacterState != State.Passout && Health.EmptyHealth) {
				CharacterState = State.Passout;
				PassoutFrame = frame;
			}

			// Behaviour
			switch (CharacterState) {
				default:
				case State.General:
					if (frame < Health.LastDamageFrame + Health.DamageDurationValue) {
						// Tacking Damage
						Movement.AntiKnockback();
					} else if (Attackness.StopOnAttackValue && frame < Attackness.LastAttackFrame + Attackness.AttackDurationValue) {
						// Stop when Attacking
						if (IsGrounded) VelocityX = 0;
					} else {
						// Move as Normal
						Action.Update();
						Attackness.Update();
						Movement.Update();
					}
					Health.Update();
					base.PhysicsUpdate();
					break;
				case State.Animate:

					break;
				case State.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					if (!Health.FullHealth) Health.Heal(Health.MaxHealthPoint);
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


		// Invoke Behaviour
		public void TakeDamage (int damage) {
			if (Health.Damage(damage)) {
				VelocityX = Movement.FacingRight ? -Health.KnockBackSpeedValue : Health.KnockBackSpeedValue;
				Renderer.Damage(Health.DamageDurationValue);
				if (!Health.EmptyHealth) {
					Renderer.Blink(Health.InvincibleFrameDuration);
				}
			}
		}


		public void TakeHeal (int heal) {
			Health.Heal(heal);


		}


		public bool InvokeAction () {
			bool performed = Action.Invoke();



			return performed;
		}


		public void Sleep () {
			CharacterState = State.Sleep;
		}


		public void Wakeup () {
			CharacterState = State.General;
			X += Const.CELL_SIZE / 2;
			Renderer.Bounce();
		}


		// Misc
		protected override bool GroundedCheck (RectInt rect) => base.GroundedCheck(rect);


		protected override bool InsideGroundCheck () => CellPhysics.Overlap(YayaConst.MASK_LEVEL, new(X, Y + Height / 4, 1, 1), this);


		#endregion




	}
}
