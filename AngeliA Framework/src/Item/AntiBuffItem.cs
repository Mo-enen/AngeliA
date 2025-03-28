
namespace AngeliA;

/// <summary>
/// A type of item that prevents a given type of buff from giving to the host
/// </summary>
/// <typeparam name="B">Type of the buff to prevent</typeparam>
public abstract class AntiBuffItem<B> : Item where B : Buff {

	// SUB
	/// <summary>
	/// How anti buff item been trigger
	/// </summary>
	protected enum TriggerMode { Auto, Manual, }

	// VAR
	/// <summary>
	/// How this item been trigger
	/// </summary>
	protected abstract TriggerMode Mode { get; }
	/// <summary>
	/// How long does it work after been use in frame
	/// </summary>
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

	/// <summary>
	/// This function is called when the item is triggered
	/// </summary>
	/// <param name="target">Target character that will get the effect</param>
	public virtual void OnAntiBuffTriggerd (Character target) { }

}