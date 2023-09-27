using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public abstract partial class Character : IDamageReceiver {




		#region --- VAR ---


		// Api
		public int HealthPoint { get; private set; } = 1;
		public int LastDamageFrame { get; private set; } = int.MinValue;
		public bool IsFullHealth => HealthPoint >= MaxHP;
		public bool IsEmptyHealth => HealthPoint <= 0;
		public bool IsInvincible => Game.GlobalFrame < InvincibleEndFrame;
		public bool TakingDamage => Game.GlobalFrame < LastDamageFrame + DamageStunDuration;
		int IDamageReceiver.Team => Const.TEAM_NEUTRAL;

		// Data
		private int InvincibleEndFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public void OnActivated_Health () {
			HealthPoint = MaxHP.FinalValue;
			InvincibleEndFrame = int.MinValue;
		}


		#endregion




		#region --- API ---


		void IDamageReceiver.TakeDamage (int damage, Entity sender) {
			if (!Active || damage <= 0 || HealthPoint <= 0) return;
			if (CharacterState != CharacterState.GamePlay || IsInvincible) return;
			if (InvincibleOnRush && IsRushing) return;
			if (InvincibleOnDash && IsDashing) return;
			OnTakeDamage(ref damage, sender);
		}


		public bool Heal (int heal) {
			int oldPoint = HealthPoint;
			HealthPoint = (HealthPoint + heal).Clamp(0, MaxHP);
			return oldPoint != HealthPoint;
		}


		public void SetHealth (int health) => HealthPoint = health.Clamp(0, MaxHP);


		public void MakeInvincible (int duration = 1) => InvincibleEndFrame = Game.GlobalFrame + duration;


		protected virtual void OnTakeDamage (ref int damage, Entity sender) {

			ChargeStartFrame = int.MaxValue;
			MakeInvincible(InvincibleDuration);

			HealthPoint = (HealthPoint - damage).Clamp(0, MaxHP);
			LastDamageFrame = Game.GlobalFrame;

			VelocityX = FacingRight ? -KnockBackSpeed : KnockBackSpeed;

		}


		#endregion




	}
}