namespace AngeliA;

public abstract class ItemCollectable<TItem> : Collectable where TItem : Item {
	private int ItemID { get; init; }
	public ItemCollectable () => ItemID = typeof(TItem).AngeHash();
	public override bool OnCollect (Entity source) {
		base.OnCollect(source);
		return ItemSystem.GiveItemToTarget(source, ItemID, 1);
	}
}
