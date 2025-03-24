using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Behavior class that handle health logic for character
/// </summary>
public partial class CharacterHealth {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Current health point of the character
	/// </summary>
	public int HP { get; set; } = 1;
	/// <summary>
	/// The frame when character took damage last time
	/// </summary>
	public int LastDamageFrame { get; set; } = int.MinValue;
	/// <summary>
	/// The frame when character's invincibility ends
	/// </summary>
	public int InvincibleEndFrame { get; set; } = int.MinValue;
	/// <summary>
	/// True if the character is full of health
	/// </summary>
	public bool IsFullHealth => HP >= MaxHP;
	/// <summary>
	/// True if the character have no health point
	/// </summary>
	public bool IsEmptyHealth => HP <= 0;
	/// <summary>
	/// True if the character is currently invincible
	/// </summary>
	public bool IsInvincible => Game.GlobalFrame < InvincibleEndFrame;
	/// <summary>
	/// True if the character is currently taking damage
	/// </summary>
	public bool TakingDamage => Game.GlobalFrame < LastDamageFrame + DamageStunDuration;

	// Meta
	/// <summary>
	/// Upper limit of the HP
	/// </summary>
	public readonly FrameBasedInt MaxHP = new(1);
	/// <summary>
	/// How many frames does the character's Invincibility last
	/// </summary>
	public readonly FrameBasedInt InvincibleDuration = new(120);
	/// <summary>
	/// How many frames does the character stun when they take damage
	/// </summary>
	public readonly FrameBasedInt DamageStunDuration = new(24);
	/// <summary>
	/// How fast should the character get knock back when they take damage
	/// </summary>
	public readonly FrameBasedInt KnockbackSpeed = new(64);
	/// <summary>
	/// Movement deceleration when the character is knocking back after take damage
	/// </summary>
	public readonly FrameBasedInt KnockbackDeceleration = new(16);
	/// <summary>
	/// Should character be invincible when dashing
	/// </summary>
	public readonly FrameBasedBool InvincibleOnDash = new(false);
	/// <summary>
	/// Should character be invincible when rushing
	/// </summary>
	public readonly FrameBasedBool InvincibleOnRush = new(false);


	#endregion




	#region --- MSG ---


	/// <summary>
	/// Callback when character entity get activated
	/// </summary>
	public void OnActivated () {
		HP = MaxHP.FinalValue;
		InvincibleEndFrame = int.MinValue;
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Add given amount of HP for the character
	/// </summary>
	/// <returns>True when character get healed</returns>
	public bool Heal (int heal) {
		int oldPoint = HP;
		HP = (HP + heal).Clamp(0, MaxHP);
		return oldPoint != HP;
	}


	/// <summary>
	/// Let the character become invincible for given frames
	/// </summary>
	public void MakeInvincible (int duration = 1) => InvincibleEndFrame = Game.GlobalFrame + duration;


	#endregion




}