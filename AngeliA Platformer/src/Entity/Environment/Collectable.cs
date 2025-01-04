using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;



[EntityAttribute.Capacity(1024, 0)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Collectable")]
public abstract class Collectable : Entity, IBlockEntity {

	// VAR
	protected virtual int ItemID => TypeID;
	protected virtual int ItemCount => 1;
	private Int4 TriggerExpand;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		TriggerExpand = default;
		if (Renderer.TryGetSprite(TypeID, out var sprite, false)) {
			TriggerExpand.left = TriggerExpand.right = (sprite.GlobalWidth - Const.CEL) / 2;
			TriggerExpand.down = TriggerExpand.up = (sprite.GlobalHeight - Const.CEL) / 2;
		}
	}

	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}

	public override void Update () {
		base.Update();
		if (Physics.GetEntity<Character>(
			Rect.Expand(TriggerExpand),
			PhysicsMask.CHARACTER, null, OperationMode.ColliderOnly
		) is Character character) {
			OnCollect(character);
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect.Expand(TriggerExpand));
	}

	public virtual bool OnCollect (Character collector) {
		Active = false;
		FrameworkUtil.RemoveFromWorldMemory(this);
		return !IgnoreReposition && Inventory.GiveItemToTarget(collector, ItemID, ItemCount);
	}

}