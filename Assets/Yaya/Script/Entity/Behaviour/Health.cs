using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	public class Health : EntityBehaviour<Entity>, ITxtMeta {




		#region --- VAR ---


		// Api
		public int HealthPoint { get; private set; } = 1;
		public int LastDamageFrame { get; private set; } = int.MinValue;
		public bool FullHealth => HealthPoint >= MaxHP;
		public bool EmptyHealth => HealthPoint <= 0;
		public bool Invincible => Game.GlobalFrame < InvincibleStartFrame + InvincibleFrame;
		public int InvincibleFrameDuration => InvincibleFrame;
		public int KnockBackSpeedValue => KnockBackSpeed;
		public int DamageDurationValue => DamageDuration;
		public int MaxHealthPoint => MaxHP;

		// Ser
		[SerializeField] BuffInt MaxHP = new(1);
		[SerializeField] BuffInt InvincibleFrame = new(120);
		[SerializeField] BuffInt KnockBackSpeed = new(64);
		[SerializeField] BuffInt DamageDuration = new(24);

		// Data
		private int InvincibleStartFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void Initialize (Entity source) {
			base.Initialize(source);
			HealthPoint = MaxHP.FinalValue;
			InvincibleStartFrame = int.MinValue;
		}


		#endregion




		#region --- API ---


		// Meta
		public void LoadFromText (string text) => BuffValue.LoadBuffMetaFromText(this, text);


		public string SaveToText () => BuffValue.SaveBuffMetaToText(this);


		// Health
		public bool Damage (int damage, bool ignoreInvincible = false, bool triggerInvincible = true) {
			if (!ignoreInvincible && Invincible) return false;
			if (HealthPoint <= 0) return false;
			HealthPoint = (HealthPoint - damage).Clamp(0, MaxHP);
			if (triggerInvincible) InvincibleStartFrame = Game.GlobalFrame;
			LastDamageFrame = Game.GlobalFrame;
			return true;
		}


		public bool Heal (int heal) {
			int oldPoint = HealthPoint;
			HealthPoint = (HealthPoint + heal).Clamp(0, MaxHP);
			return oldPoint != HealthPoint;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}
