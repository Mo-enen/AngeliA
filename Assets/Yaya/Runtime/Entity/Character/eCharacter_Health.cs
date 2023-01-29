using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Moenen.Standard;

namespace Yaya {
	public abstract partial class eCharacter {




		#region --- VAR ---


		// Api
		public int HealthPoint { get; private set; } = 1;
		public int LastDamageFrame { get; private set; } = int.MinValue;
		public bool IsFullHealth => HealthPoint >= MaxHP;
		public bool IsEmptyHealth => HealthPoint <= 0;
		public bool Invincible => Game.GlobalFrame < InvincibleEndFrame;
		public bool IsSafe => Game.GlobalFrame <= SafeFrame;

		// Buff
		public BuffInt MaxHP { get; private set; } = new(1);
		public BuffInt InvincibleFrame { get; private set; } = new(120);
		public BuffInt KnockBackSpeed { get; private set; } = new(64);
		public BuffInt DamageStunDuration { get; private set; } = new(24);

		// Data
		private int InvincibleEndFrame = int.MinValue;
		private int SafeFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public void OnActived_Health () {
			HealthPoint = MaxHP.FinalValue;
			InvincibleEndFrame = int.MinValue;
			SafeFrame = int.MinValue;
		}


		#endregion




		#region --- API ---


		public void TakeDamage (int damage) {

			if (
				CharacterState != CharacterState.GamePlay || damage <= 0 ||
				Invincible || HealthPoint <= 0 || Game.GlobalFrame <= SafeFrame
			) return;

			// Health Down
			HealthPoint = (HealthPoint - damage).Clamp(0, MaxHP);
			InvincibleEndFrame = Game.GlobalFrame + InvincibleFrame;
			LastDamageFrame = Game.GlobalFrame;

			// Render
			VelocityX = FacingRight ? -KnockBackSpeed : KnockBackSpeed;
			RenderDamage(DamageStunDuration);
			if (!IsEmptyHealth) {
				RenderBlink(InvincibleFrame);
			}
		}


		public bool Heal (int heal) {
			int oldPoint = HealthPoint;
			HealthPoint = (HealthPoint + heal).Clamp(0, MaxHP);
			return oldPoint != HealthPoint;
		}


		public void SetHealth (int health) => HealthPoint = health.Clamp(0, MaxHP);


		public void MakeSafe (int duration = 0) => SafeFrame = Game.GlobalFrame + duration;


		#endregion




		#region --- LGC ---




		#endregion




	}
}