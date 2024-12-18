
namespace AngeliA;

public abstract class AntiBuffItem<B> : Item where B : Buff {

	// SUB
	protected enum TriggerMode { Auto, Manual, }

	// VAR
	protected abstract TriggerMode Mode { get; }
	public abstract int Duration { get; }
	private int BuffID { get; init; }

	// MSG
	public AntiBuffItem () => BuffID = typeof(B).AngeHash();

	public override void BeforeItemUpdate_FromInventory (Character character, int inventoryID, int itemIndex) {
		base.BeforeItemUpdate_FromInventory(character, inventoryID, itemIndex);
		if (Mode != TriggerMode.Auto) return;
		var buff = character.Buff;
		if (!buff.HasBuff(BuffID)) return;
		// Perform
		buff.PreventBuff(BuffID, Duration);
		int id = Inventory.GetItemAt(inventoryID, itemIndex, out int count);
		Inventory.SetItemAt(inventoryID, itemIndex, id, (count - 1).GreaterOrEquelThanZero());
		FrameworkUtil.InvokeItemLost(character, TypeID);
		// Callback
		OnAntiBuffTriggerd(character);
	}

	public override bool CanUse (Character character) => Mode == TriggerMode.Manual;

	public override bool Use (Character character, int inventoryID, int itemIndex, out bool consume) {
		consume = false;
		if (Mode != TriggerMode.Manual) return false;
		var buff = character.Buff;
		if (buff.IsBuffPrevented(BuffID)) return false;
		consume = true;
		buff.PreventBuff(BuffID, Duration);
		OnAntiBuffTriggerd(character);
		return true;
	}

	public virtual void OnAntiBuffTriggerd (Character target) { }

}