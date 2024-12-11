using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[EntityAttribute.Capacity(1024, 0)]
[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
[EntityAttribute.MapEditorGroup("Collectable")]
public abstract class Collectable : Entity, IBlockEntity {


	public override void OnActivated () {
		base.OnActivated();
		if (Renderer.TryGetSprite(TypeID, out var sprite, false)) {
			X += (Width - sprite.GlobalWidth) / 2;
			Y += (Height - sprite.GlobalHeight) / 2;
			Width = sprite.GlobalWidth;
			Height = sprite.GlobalHeight;
		}
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		Physics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);
	}


	public override void LateUpdate () {
		base.LateUpdate();
		Renderer.Draw(TypeID, Rect);
	}


	public virtual bool OnCollect (Entity collector) {
		FrameworkUtil.RemoveFromWorldMemory(this);
		return true;
	}


}