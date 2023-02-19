using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;
using Moenen.Standard;

namespace Yaya {
	public abstract partial class eCharacter : IDamageReceiver {




		#region --- VAR ---


		// Api
		public int HealthPoint { get; private set; } = 1;
		public int LastDamageFrame { get; private set; } = int.MinValue;
		public bool IsFullHealth => HealthPoint >= MaxHP;
		public bool IsEmptyHealth => HealthPoint <= 0;
		public bool Invincible => Game.GlobalFrame < InvincibleEndFrame;
		public bool IsSafe => Game.GlobalFrame <= SafeFrame;
		public bool TakingDamage => Game.GlobalFrame < LastDamageFrame + DamageStunDuration;
		public virtual bool AllowDamageFromLevel => true;
		public virtual bool AllowDamageFromBullet => true;
		public virtual int Team => YayaConst.TEAM_NEUTRAL;

		// Buff
		public readonly BuffInt MaxHP = new(1);
		public readonly BuffInt InvincibleDuration = new(120);
		public readonly BuffInt DamageStunDuration = new(24);
		public readonly BuffInt KnockBackSpeed = new(64);

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

			ChargeStartFrame = int.MaxValue;

			// Health Down
			HealthPoint = (HealthPoint - damage).Clamp(0, MaxHP);
			InvincibleEndFrame = Game.GlobalFrame + InvincibleDuration;
			LastDamageFrame = Game.GlobalFrame;

			// Render
			VelocityX = FacingRight ? -KnockBackSpeed : KnockBackSpeed;
			RenderDamage(DamageStunDuration);
			if (!IsEmptyHealth) {
				RenderBlink(InvincibleDuration);
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