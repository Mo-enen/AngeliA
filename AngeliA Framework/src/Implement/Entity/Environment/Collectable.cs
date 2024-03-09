using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public class Coin : ItemCollectable<iItemCoin> {

}


public abstract class ItemCollectable<TItem> : Collectable where TItem : Item {
	private int ItemID { get; init; }
	public ItemCollectable () => ItemID = typeof(TItem).AngeHash();
	public override bool OnCollect (Entity source) => ItemSystem.GiveItemTo(source.TypeID, ItemID, 1);
}


[EntityAttribute.Capacity(1024, 0)]
[EntityAttribute.MapEditorGroup("Collectable")]
public abstract class Collectable : EnvironmentEntity {


	public override void OnActivated () {
		base.OnActivated();
		if (Renderer.TryGetSprite(TypeID, out var sprite)) {
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


	public abstract bool OnCollect (Entity source);


}