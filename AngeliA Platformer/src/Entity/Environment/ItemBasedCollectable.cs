using AngeliA;namespace AngeliA.Platformer;

public abstract class ItemBasedCollectable<I> : Collectable where I : Item {
	private int ItemID { get; init; }
	public ItemBasedCollectable () => ItemID = typeof(I).AngeHash();
	public override bool OnCollect (Entity source) {
		base.OnCollect(source);
		return ItemSystem.GiveItemToTarget(source, ItemID, 1);
	}
}
