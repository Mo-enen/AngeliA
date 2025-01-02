using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[EntityAttribute.Capacity(1024, 0)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Collectable")]
public abstract class Collectable : Entity, IBlockEntity {

	// VAR
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
			Active = false;
		}
	}

	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect.Expand(TriggerExpand));
	}

	public virtual bool OnCollect (Character collector) {
		FrameworkUtil.RemoveFromWorldMemory(this);
		return true;
	}

}