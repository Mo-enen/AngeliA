using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.MapEditorGroup("Character")]
	public abstract class eCharacter : eYayaRigidbody, IDamageReceiver {




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
		public override bool InAir => base.InAir && !Movement.IsClimbing;
		public override int AirDragX => 0;
		public override int AirDragY => 0;
		public override bool IgnoreRiseGravityShift => true;
		public State CharacterState { get; private set; } = State.General;
		public int PassoutFrame { get; private set; } = int.MinValue;

		// Behaviour
		public Movement Movement { get; private set; }
		public CharacterRenderer Renderer { get; private set; }
		public Action Action { get; private set; }
		public Health Health { get; private set; }
		public Attackness Attackness { get; private set; }


		#endregion




		#region --- MSG ---


		public override void OnInitialize (Game game) {

			base.OnInitialize(game);
			string typeName = GetType().Name;
			if (typeName.StartsWith("e")) typeName = typeName[1..];

			Movement = game.LoadMeta<Movement>(typeName, "Movement") ?? new();
			Renderer = game.LoadMeta<CharacterRenderer>(typeName, "Renderer") ?? new();
			Action = game.LoadMeta<Action>(typeName, "Action") ?? new();
			Health = game.LoadMeta<Health>(typeName, "Health") ?? new();
			Attackness = game.LoadMeta<Attackness>(typeName, "Attackness") ?? new();
		}


		public override void OnActived () {
			base.OnActived();
			Movement.OnActived(this);
			Renderer.OnActived(this);
			Action.OnActived(this);
			Health.OnActived(this);
			Attackness.OnActived(this);
			CharacterState = State.General;
			PassoutFrame = int.MinValue;
		}


		public override void FillPhysics () {
			if (CharacterState == State.General) {
				base.FillPhysics();
			}
		}


		public override void PhysicsUpdate () {

			int frame = Game.GlobalFrame;

			// Passout Check
			if (CharacterState != State.Passout && Health.EmptyHealth) {
				CharacterState = State.Passout;
				PassoutFrame = frame;
			}

			// Behaviour
			switch (CharacterState) {
				default:
				case State.General:
					if (frame < Health.LastDamageFrame + Health.DamageStunDuration) {
						// Tacking Damage
						Movement.AntiKnockback();
					} else if (Attackness.StopMoveOnAttack && frame < Attackness.LastAttackFrame + Attackness.Duration) {
						// Stop when Attacking
						if (IsGrounded) VelocityX = 0;
					} else {
						// Move as Normal
						Action.Update();
						Attackness.Update();
						Movement.Update();
					}
					base.PhysicsUpdate();
					break;
				case State.Animate:

					break;
				case State.Sleep:
					VelocityX = 0;
					VelocityY = 0;
					if (!Health.FullHealth) Health.Heal(Health.MaxHP);
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
				VelocityX = Movement.FacingRight ? -Health.KnockBackSpeed : Health.KnockBackSpeed;
				Renderer.Damage(Health.DamageStunDuration);
				if (!Health.EmptyHealth) {
					Renderer.Blink(Health.InvincibleFrame);
				}
			}
		}


		public void Sleep () {
			CharacterState = State.Sleep;
		}


		public void Wakeup () {
			CharacterState = State.General;
			X += Const.CELL_SIZE / 2;
			Renderer.Bounce();
			Action.Update();
		}


		// Misc
		protected override bool GroundedCheck (RectInt rect) => base.GroundedCheck(rect);


		protected override bool InsideGroundCheck () => CellPhysics.Overlap(YayaConst.MASK_LEVEL, new(X, Y + Height / 4, 1, 1), this);


		#endregion




	}
}
