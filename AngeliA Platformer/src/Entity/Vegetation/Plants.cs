using System.Collections;
using System.Collections.Generic;


using AngeliA;
namespace AngeliA.Platformer;


[EntityAttribute.MapEditorGroup("Vegetation")]
[EntityAttribute.Capacity(128)]
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
			500, 0, 0,
			Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
		);
	}


	protected virtual void OnPlantBreak () {
		ItemSystem.DropItemFor(this);
		if (Universe.BuiltInInfo.UseProceduralMap) {
			FrameworkUtil.PickEntityBlock(this, false);
		} else {
			FrameworkUtil.RemoveFromWorldMemory(this);
		}
	}


	// API
	void IDamageReceiver.TakeDamage (Damage damage) {
		if (damage.Amount <= 0) return;
		// Particle
		if (Renderer.TryGetSprite(TypeID, out var sprite)) {
			FrameworkUtil.InvokeObjectBreak(TypeID, Rect.Fit(sprite), true);
		}
		// Disable
		Active = false;
		OnPlantBreak();
	}


}
