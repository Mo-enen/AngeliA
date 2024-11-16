using System.Collections;
using System.Collections.Generic;

using AngeliA;
namespace AngeliA.Platformer;

public abstract class MapChest : Furniture, IActionTarget, IBlockEntity {


	// VAR
	protected static readonly HashSet<Int3> OpenedChest = [];
	protected virtual int ItemPlaceHolder => 0;


	// MSG
	public override void LateUpdate () {
		bool opened = IsChestOpened(this);
		if (Renderer.TryGetSpriteFromGroup(TypeID, opened ? 1 : 0, out var sprite, false, true)) {
			var cell = Renderer.Draw(sprite, RenderingRect);
			(this as IActionTarget).BlinkIfHighlight(cell);
		}
	}


	// API
	protected static bool IsChestOpened (MapChest chest) => chest != null && chest.FromWorld && OpenedChest.Contains(chest.InstanceID);


	[OnGameRestart]
	public static void ClearOpenedMarks_Restart () {
		OpenedChest.Clear();
	}


	[OnMapEditorModeChange]
	public static void ClearOpenedMarks (OnMapEditorModeChange.Mode mode) {
		if (mode == OnMapEditorModeChange.Mode.ExitEditMode) {
			OpenedChest.Clear();
		}
	}


	bool IActionTarget.Invoke () {
		if (IsChestOpened(this)) return false;
		if (FromWorld) OpenedChest.Add(InstanceID);
		FrameworkUtil.SpawnItemFromMap(
			WorldSquad.Front, X.ToUnit(), Y.ToUnit(), Stage.ViewZ,
			placeHolderID: ItemPlaceHolder
		);
		return true;
	}


	bool IActionTarget.AllowInvoke () => !IsChestOpened(this);


}