using System.Collections;
using System.Collections.Generic;


using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Represent a breakable plant on stage
/// </summary>
[EntityAttribute.MapEditorGroup("Vegetation")]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class Plant : Entity, IBlockEntity, ICombustible, IDamageReceiver {


	// Api
	int ICombustible.BurnStartFrame { get; set; }
	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;

	// MSG
	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(
			TypeID, X + Width / 2, Y,
			Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
		);
	}


	/// <summary>
	/// This function is called when the plant is break by something
	/// </summary>
	protected virtual void OnPlantBreak () {
		if (!IgnoreReposition) {
			ItemSystem.DropItemFor(this);
		}
		FrameworkUtil.BreakEntityBlock(this);
	}


	// API
	void IDamageReceiver.OnDamaged (Damage damage) {
		if (damage.Amount <= 0) return;
		Active = false;
		OnPlantBreak();
	}


}
