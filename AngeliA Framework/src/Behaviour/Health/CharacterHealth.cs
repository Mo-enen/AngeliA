using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public partial class CharacterHealth {




	#region --- VAR ---


	// Api
	public int HP { get; set; } = 1;
	public int LastDamageFrame { get; set; } = int.MinValue;
	public int InvincibleEndFrame { get; set; } = int.MinValue;
	public bool IsFullHealth => HP >= MaxHP;
	public bool IsEmptyHealth => HP <= 0;
	public bool IsInvincible => Game.GlobalFrame < InvincibleEndFrame;
	public bool TakingDamage => Game.GlobalFrame < LastDamageFrame + DamageStunDuration;

	// Meta
	public readonly FrameBasedInt MaxHP = new(1);
	public readonly FrameBasedInt InvincibleDuration = new(120);
	public readonly FrameBasedInt DamageStunDuration = new(24);
	public readonly FrameBasedInt KnockBackSpeed = new(64);
	public readonly FrameBasedInt KnockbackDeceleration = new(16);
	public readonly FrameBasedBool InvincibleOnDash = new(false);
	public readonly FrameBasedBool InvincibleOnRush = new(false);


	#endregion




	#region --- MSG ---


	public void OnActivated () {
		HP = MaxHP.FinalValue;
		InvincibleEndFrame = int.MinValue;
	}


	#endregion




	#region --- API ---


	public bool Heal (int heal) {
		int oldPoint = HP;
		HP = (HP + heal).Clamp(0, MaxHP);
		return oldPoint != HP;
	}


	public void MakeInvincible (int duration = 1) => InvincibleEndFrame = Game.GlobalFrame + duration;


	#endregion




}