using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[EntityAttribute.MapEditorGroup("Vegetation")]
[EntityAttribute.Capacity(128)]
public abstract class Plant : EnvironmentEntity, ICombustible, IDamageReceiver {


	// Api
	int ICombustible.BurnStartFrame { get; set; }
	int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;

	// Data
	private int ArtworkIndex = -1;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		if (Renderer.HasSpriteGroup(TypeID)) {
			ArtworkIndex = (X.ToUnit() * 11 + Y.ToUnit() * 7).Abs();
		} else {
			ArtworkIndex = -1;
		}
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (ArtworkIndex >= 0 && Renderer.TryGetSpriteFromGroup(TypeID, ArtworkIndex, out var sprite, true)) {
			Renderer.Draw(
				sprite, X + Width / 2, Y,
				500, 0, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
			);
		} else {
			Renderer.Draw(
				TypeID, X + Width / 2, Y,
				500, 0, 0,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE
			);
		}

	}


	protected virtual void OnPlantBreak () {
		bool itemDropped = ItemSystem.DropItemFor(this);
		if (Universe.BuiltInInfo.UseProceduralMap) {
			FrameworkUtil.PickEntityBlock(this, !itemDropped);
		} else {
			FrameworkUtil.RemoveFromWorldMemory(this);
		}
	}


	// API
	void IDamageReceiver.TakeDamage (Damage damage) {
		if (damage.Amount <= 0) return;
		// Particle
		int id = TypeID;
		var rect = Rect;
		if (ArtworkIndex >= 0 && Renderer.TryGetSpriteFromGroup(TypeID, ArtworkIndex, out var sprite, true)) {
			id = sprite.ID;
			rect.height = sprite.GlobalHeight;
			if (rect.width != sprite.GlobalWidth) {
				rect.x -= (sprite.GlobalWidth - rect.width) / 2;
				rect.width = sprite.GlobalWidth;
			}
		}
		GlobalEvent.InvokeObjectBreak(id, rect, true);
		// Disable
		Active = false;
		OnPlantBreak();
	}


}
