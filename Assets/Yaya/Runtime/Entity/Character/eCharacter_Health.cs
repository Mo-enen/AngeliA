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
		public bool Invincible => Game.GlobalFrame < InvincibleStartFrame + InvincibleFrame;

		// Buff
		public BuffInt MaxHP { get; private set; } = new();
		public BuffInt InvincibleFrame { get; private set; } = new();
		public BuffInt KnockBackSpeed { get; private set; } = new();
		public BuffInt DamageStunDuration { get; private set; } = new();

		// Ser
#pragma warning disable
		[SerializeField] int _MaxHP = 1;
		[SerializeField] int _InvincibleFrame = 120;
		[SerializeField] int _KnockBackSpeed = 64;
		[SerializeField] int _DamageStunDuration = 24;
#pragma warning restore

		// Data
		private int InvincibleStartFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public void OnActived_Health () {
			HealthPoint = MaxHP.FinalValue;
			InvincibleStartFrame = int.MinValue;
		}


		#endregion




		#region --- API ---


		// Health
		public bool Heal (int heal) {
			int oldPoint = HealthPoint;
			HealthPoint = (HealthPoint + heal).Clamp(0, MaxHP);
			return oldPoint != HealthPoint;
		}


		public void SetHealth (int health) => HealthPoint = health.Clamp(0, MaxHP);


		#endregion




		#region --- LGC ---




		#endregion




	}
}