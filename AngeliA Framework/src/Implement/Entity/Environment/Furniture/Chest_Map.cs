using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework; 
public abstract class MapChest : Furniture, IActionTarget {


	// VAR
	protected static readonly HashSet<Int3> OpenedChest = new();
	private static readonly System.Random Ran = new(483623528);


	// MSG
	public override void FrameUpdate () {
		bool opened = IsChestOpened(this);
		if (CellRenderer.TryGetSpriteFromGroup(TypeID, opened ? 1 : 0, out var sprite, false, true)) {
			var cell = CellRenderer.Draw(sprite, RenderingRect);
			if ((this as IActionTarget).IsHighlighted) {
				IActionTarget.HighlightBlink(cell);
			}
		}
	}


	// API
	protected static bool IsChestOpened (MapChest chest) => chest != null && chest.FromWorld && OpenedChest.Contains(chest.InstanceID);


	[OnGameRestart]
	public static void ClearOpenedMarks () => OpenedChest.Clear();


	void IActionTarget.Invoke () {
		if (IsChestOpened(this)) return;
		if (FromWorld) OpenedChest.TryAdd(InstanceID);
		// Spawn Items
		var squad = WorldSquad.Front;
		for (int y = 1; y < 256; y++) {
			int unitY = Y.ToUnit() - y;
			int right = -1;
			for (int x = 0; x < 256; x++) {
				int id = squad.GetBlockAt(X.ToUnit() + x, unitY, BlockType.Element);
				if (id == 0 || !ItemSystem.HasItem(id)) break;
				right = x;
			}
			if (right == -1) break;
			int itemLocalIndex = Ran.Next(0, right + 1);
			int itemID = squad.GetBlockAt(X.ToUnit() + itemLocalIndex, unitY, BlockType.Element);
			if (ItemSystem.HasItem(itemID)) {
				// Spawn Item
				if (Stage.SpawnEntity(ItemHolder.TYPE_ID, X, Y) is ItemHolder holder) {
					holder.ItemID = itemID;
					holder.ItemCount = 1;
					holder.Jump();
				}
			}
		}
	}


	bool IActionTarget.AllowInvoke () => !IsChestOpened(this);


}