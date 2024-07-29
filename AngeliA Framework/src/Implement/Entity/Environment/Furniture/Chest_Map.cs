using System.Collections;
using System.Collections.Generic;


namespace AngeliA;
public abstract class MapChest : Furniture, IActionTarget, IBlockEntity {


	// VAR
	protected static readonly HashSet<Int3> OpenedChest = new();


	// MSG
	public override void LateUpdate () {
		bool opened = IsChestOpened(this);
		if (Renderer.TryGetSpriteFromGroup(TypeID, opened ? 1 : 0, out var sprite, false, true)) {
			var cell = Renderer.Draw(sprite, RenderingRect);
			if ((this as IActionTarget).IsHighlighted) {
				IActionTarget.HighlightBlink(cell);
			}
		}
	}


	// API
	protected static bool IsChestOpened (MapChest chest) => chest != null && chest.FromWorld && OpenedChest.Contains(chest.InstanceID);


	[OnGameRestart]
	public static void ClearOpenedMarks () => OpenedChest.Clear();


	bool IActionTarget.Invoke () {
		if (IsChestOpened(this)) return false;
		if (FromWorld) OpenedChest.TryAdd(InstanceID);
		ItemSystem.SpawnItemFromMap(X.ToUnit(), Y.ToUnit(), Stage.ViewZ);
		return true;
	}


	bool IActionTarget.AllowInvoke () => !IsChestOpened(this);


	void IBlockEntity.OnEntityPicked (Entity picker) {
		// TODO
	}


}