using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {
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


		void IDamageReceiver.TakeDamage (Damage damage) {
			if (!Active || damage.Amount <= 0 || HealthPoint <= 0) return;
			if (CharacterState != CharacterState.GamePlay || IsInvincible) return;
			if (InvincibleOnRush && IsRushing) return;
			if (InvincibleOnDash && IsDashing) return;
			OnTakeDamage(damage.Amount, damage.Sender);
		}


		public bool Heal (int heal) {
			int oldPoint = HealthPoint;
			HealthPoint = (HealthPoint + heal).Clamp(0, MaxHP);
			return oldPoint != HealthPoint;
		}


		public void SetHealth (int health) => HealthPoint = health.Clamp(0, MaxHP);


		public void MakeInvincible (int duration = 1) => InvincibleEndFrame = Game.GlobalFrame + duration;


		protected virtual void OnTakeDamage (int damage, Entity sender) {

			// Equipment
			for (int i = 0; i < EquipmentTypeCount && damage > 0; i++) {
				GetEquippingItem((EquipmentType)i)?.OnTakeDamage_FromEquipment(this, sender, ref damage);
			}

			// Inventory
			int iCount = GetInventoryCapacity();
			for (int i = 0; i < iCount && damage > 0; i++) {
				GetItemFromInventory(i)?.OnTakeDamage_FromInventory(this, sender, ref damage);
			}

			// Deal Damage
			damage = damage.GreaterOrEquelThanZero();
			HealthPoint = (HealthPoint - damage).Clamp(0, MaxHP);

			VelocityX = FacingRight ? -KnockBackSpeed : KnockBackSpeed;

			InvincibleEndFrame = Game.GlobalFrame + InvincibleDuration;
			LastDamageFrame = Game.GlobalFrame;

		}


		#endregion




	}
}