using AngeliA;namespace AngeliA.Platformer;

public abstract class ItemBasedCollectable<I> : Collectable where I : Item {
	private int ItemID { get; init; }
	public ItemBasedCollectable () => ItemID = typeof(I).AngeHash();
	public override bool OnCollect (Entity source) {
		base.OnCollect(source);
		return Inventory.GiveItemToTarget(source, ItemID, 1);
	}
}
