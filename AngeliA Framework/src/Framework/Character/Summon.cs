using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.DontDestroyOutOfRange]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.Capacity(128)]
[EntityAttribute.ExcludeInMapEditor]
public abstract class Summon : SheetCharacter, IDamageReceiver, IActionTarget {




	#region --- VAR ---


	// Api
	public Character Owner { get; set; } = null;
	int IDamageReceiver.Team => Owner != null ? (Owner as IDamageReceiver).Team : Const.TEAM_NEUTRAL;
	bool IDamageReceiver.TakeDamageFromLevel => false;
	public override int AttackTargetTeam => Owner != null ? Owner.AttackTargetTeam : 0;
	public int InventoryUpdatedFrame { get; set; } = -1;

	// Data
	private SummonNavigation SummonNavigation;
	private int SummonFrame = int.MinValue;
	private int PrevZ = int.MinValue;


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Owner = null;
		Navigation.NavigationState = CharacterNavigationState.Operation;
		SummonNavigation.Refresh();
		SummonNavigation.FollowOwner = true;
		Movement.PushAvailable.BaseValue = false;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		SummonNavigation.Owner = Owner;
		if (CharacterState == CharacterState.GamePlay) {
			IgnorePhysics(1);
		}
		Physics.FillEntity(EntityLayer.CHARACTER, this, true);
	}


	public virtual void OnSummoned (bool create) {
		Bounce();
		Navigation.ResetNavigation();
		SummonFrame = Game.GlobalFrame;
		Navigation.NavigationState = CharacterNavigationState.Operation;
		SummonNavigation.Refresh();
	}


	public override void Update () {

		if (!Active) return;

		// Inactive when Owner not Ready
		if (Owner == null || !Owner.Active) {
			Active = false;
			return;
		}

		// when Z Changed
		if (PrevZ != Stage.ViewZ) {
			PrevZ = Stage.ViewZ;
			if (SummonNavigation.FollowOwner) {
				if (CharacterState != CharacterState.Sleep) {
					X = Owner.X;
					Y = Owner.Y;
					Navigation.ResetNavigation();
					Navigation.NavigationState = CharacterNavigationState.Operation;
					SummonNavigation.Refresh();
				}
			} else {
				Active = false;
			}
		}

		// Out of Range
		if (!SummonNavigation.FollowOwner && !Stage.SpawnRect.Overlaps(Rect)) {
			Active = false;
			return;
		}

		base.Update();

	}


	public override void LateUpdate () {
		if (!Active) return;

		// Base
		base.LateUpdate();

		// Highlight
		(this as IActionTarget).BlinkIfHighlight(RenderedCell);

	}


	#endregion




	#region --- API ---


	protected override CharacterNavigation CreateNativeNavigation () => SummonNavigation = new SummonNavigation(this);


	// Summon
	public static T CreateSummon<T> (Character owner, int x, int y) where T : Summon => CreateSummon(owner, typeof(T).AngeHash(), x, y) as T;
	public static Summon CreateSummon (Character owner, int typeID, int x, int y) {
		if (owner == null || !Stage.Enable) return null;
		if (Stage.SpawnEntity(typeID, x, y) is Summon summon) {
			// Create New
			summon.Owner = owner;
			summon.OnSummoned(true);
			return summon;
		} else {
			// Find Old
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
		Health.HP = Health.MaxHP;
		SetCharacterState(CharacterState.GamePlay);
		return true;
	}


	bool IActionTarget.AllowInvoke () => CharacterState == CharacterState.PassOut;


	#endregion




}