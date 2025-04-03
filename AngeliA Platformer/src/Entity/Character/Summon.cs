using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Represent a character that summoned by another character
/// </summary>
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.Capacity(128)]
[EntityAttribute.ExcludeInMapEditor]
public abstract class Summon : Character, IDamageReceiver, IActionTarget {



	#region --- VAR ---


	// Api
	/// <summary>
	/// The character that summons this character
	/// </summary>
	public Character Owner { get; set; } = null;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	internal int InventoryUpdatedFrame { get; set; } = -1;
	/// <summary>
	/// Behavior object to handle auto movement of this character
	/// </summary>
	public SummonNavigation Navigation { get; init; }
	/// <summary>
	/// True if this character despawn when owner is null or inactivated
	/// </summary>
	public virtual bool RequireOwner => false;
	/// <summary>
	/// True if the owner can bring back this character after it passout
	/// </summary>
	public virtual bool AllowRescueWhenPassout => true;
	public override bool AllowBeingPush => true;
	public override bool CarryOtherOnTop => false;
	public override int Team => Owner != null ? Owner.Team : Const.TEAM_NEUTRAL;
	public override int AttackTargetTeam => Owner != null ? Owner.AttackTargetTeam : 0;

	// Data
	private int SummonFrame = int.MinValue;
	private int PrevZ = int.MinValue;


	#endregion




	#region --- MSG ---


	public Summon () => Navigation = CreateSummonNavigation();


	public override void OnActivated () {
		base.OnActivated();

		Owner = null;
		DespawnAfterPassoutDelay = -1;

		Navigation.OnActivated();
		Navigation.NavigationState = RigidbodyNavigationState.Operation;
		Navigation.Refresh();
		Navigation.MakeFollowOwner();

		Movement.PushAvailable.BaseValue = false;
		Movement.MovementWidth.BaseValue = 256;
		Movement.MovementHeight.BaseValue = 256;

		Health.MaxHP.BaseValue = 12;
		Health.HP = Health.MaxHP.BaseValue;

	}


	public override void OnInactivated () {
		base.OnInactivated();
		SetCharacterState(CharacterState.GamePlay);
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		if (CharacterState != CharacterState.Sleep) {
			Physics.FillEntity(PhysicalLayer, this, true);
		}
	}


	/// <summary>
	/// This function is called when the character is being summoned
	/// </summary>
	/// <param name="create">True if this character was not on stage before being summon</param>
	public virtual void OnSummoned (bool create) {
		Bounce();
		Navigation.ResetNavigation();
		SummonFrame = Game.GlobalFrame;
		Navigation.NavigationState = RigidbodyNavigationState.Operation;
		Navigation.Refresh();
	}


	public override void Update () {

		if (!Active) return;

		// Inactive when Owner not Ready
		if (RequireOwner && (Owner == null || !Owner.Active)) {
			Active = false;
			return;
		}

		// when Z Changed
		if (PrevZ != Stage.ViewZ) {
			PrevZ = Stage.ViewZ;
			if (Navigation.IsFollowingOwner && Owner == PlayerSystem.Selecting) {
				if (CharacterState != CharacterState.Sleep) {
					if (Owner != null) {
						X = Owner.X;
						Y = Owner.Y;
					}
					Navigation.ResetNavigation();
					Navigation.NavigationState = RigidbodyNavigationState.Operation;
					Navigation.Refresh();
				}
			} else {
				Active = false;
			}
		}

		// Out of Range
		if (!Navigation.IsFollowingOwner && !Stage.SpawnRect.Overlaps(Rect)) {
			Active = false;
			return;
		}

		// Base
		base.Update();

		// Nav
		Navigation.PhysicsUpdate();

	}


	public override void LateUpdate () {
		if (!Active) return;

		// Base
		base.LateUpdate();

		// Highlight
		if (CharacterState == CharacterState.PassOut && Rendering is SheetCharacterRenderer sRendering) {
			IActionTarget.MakeCellAsActionTarget(this, sRendering.RenderedCell);
		}

	}


	public override void OnDamaged (Damage damage) {
		if (damage.Bullet is not MissileBullet && damage.Bullet is not MeleeBullet) return;
		base.OnDamaged(damage);
	}


	#endregion




	#region --- API ---


	protected override CharacterAttackness CreateNativeAttackness () => new SummonAttackness(this);
	protected virtual SummonNavigation CreateSummonNavigation () => new(this);


	public override void SetCharacterState (CharacterState state) {
		Navigation.ResetNavigation();
		base.SetCharacterState(state);
	}


	// Summon
	/// <inheritdoc cref="CreateSummon(Character, int, int, int, bool)"/>
	public static T CreateSummon<T> (Character owner, int x, int y, bool forceCreate = false) where T : Summon => CreateSummon(owner, typeof(T).AngeHash(), x, y, forceCreate) as T;
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T">Type of the summon</typeparam>
	/// <param name="owner">Instance of the summon's owner</param>
	/// <param name="typeID">Type of the summon</param>
	/// <param name="x">Position in global space to put this summon</param>
	/// <param name="y">Position in global space to put this summon</param>
	/// <param name="forceCreate">True if take an existing summon in same type and owner to summon it again</param>
	/// <returns>Instance of the summoned character</returns>
	public static Summon CreateSummon (Character owner, int typeID, int x, int y, bool forceCreate = false) {
		if (owner == null || !Stage.Enable) return null;
		if (Stage.SpawnEntity(typeID, x, y) is Summon summon) {
			// Create New
			summon.Owner = owner;
			summon.OnSummoned(true);
			return summon;
		} else if (forceCreate && Stage.FindEntity(typeID) is Summon stageSummon) {
			// Find from Stage
			stageSummon.Owner = owner;
			stageSummon.OnSummoned(false);
			return stageSummon;
		} else {
			// Find Existing Summon
			var entities = Stage.Entities[EntityLayer.GAME];
			int eLen = Stage.EntityCounts[EntityLayer.GAME];
			int minSpawnFrame = int.MaxValue;
			Summon old = null;
			for (int i = 0; i < eLen; i++) {
				var e = entities[i];
				if (
					e.TypeID == typeID &&
					e is Summon sum &&
					sum.Owner == owner &&
					sum.SummonFrame < minSpawnFrame
				) {
					minSpawnFrame = sum.SummonFrame;
					old = sum;
				}
			}
			// Swape Old
			if (old != null) {
				old.X = x;
				old.Y = y;
				old.Owner = owner;
				old.OnSummoned(false);
				return old;
			}
		}
		return null;
	}


	bool IActionTarget.Invoke () {
		if (!AllowRescueWhenPassout || PlayerSystem.Selecting != Owner) return false;
		Health.HP = Health.MaxHP;
		SetCharacterState(CharacterState.GamePlay);
		return true;
	}


	bool IActionTarget.AllowInvoke () =>
		AllowRescueWhenPassout &&
		CharacterState == CharacterState.PassOut &&
		Owner == PlayerSystem.Selecting;


	#endregion




}