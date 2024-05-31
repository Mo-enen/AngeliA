using System.Collections;
using System.Collections.Generic;
using System.Text;
using AngeliA;
using GeorgeMamaladze;

namespace AngeliaEngine;

[RequireLanguageFromField]
public class ItemEditor : WindowUI {




	#region --- SUB ---


	private class LineComparer : IComparer<CombinationLine> {
		public static readonly LineComparer Instance = new();
		public int Compare (CombinationLine x, CombinationLine y) => x.Order.CompareTo(y.Order);
	}


	private class ItemNameComparer : IComparer<ItemName> {
		public static readonly ItemNameComparer Instance = new();
		public int Compare (ItemName x, ItemName y) => x.TypePath.CompareTo(y.TypePath);
	}


	private class CombinationLine {
		public int Order = 0;
		public string NameA = "";
		public string NameB = "";
		public string NameC = "";
		public string NameD = "";
		public int ItemA = 0;
		public int ItemB = 0;
		public int ItemC = 0;
		public int ItemD = 0;
		public int Result = 0;
		public string ResultName = "";
		public int ResultCount = 1;
		public bool KeepA = false;
		public bool KeepB = false;
		public bool KeepC = false;
		public bool KeepD = false;
	}


	private struct ItemName {
		public string TypePath;
		public string Name;
		public int ID;
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode UI_PANEL_ITEM = "UI.Panel.Item";

	// Api
	public static ItemEditor Instance { get; private set; }
	public override string DefaultName => "Item";
	public Project CurrentProject { get; private set; }
	public int LastItemNameCheckFrame { get; set; } = int.MinValue;

	// Data
	private readonly List<CombinationLine> Lines = new();
	private readonly Trie<ItemName> SearchTrie = new();
	private readonly List<ItemName> ItemNames = new();
	private int MasterScroll = 0;


	#endregion




	#region --- MSG ---


	[OnGameQuitting]
	internal static void OnGameQuitting_ItemEditor () => Instance?.Save();


	public ItemEditor () {
		Instance = this;
		//LoadFromPool(ItemSystem.CombinationPool);
		//// Search
		//SearchTrie = new Trie<Item>();
		//foreach (var (_, data) in ItemSystem.ItemPool) {
		//	SearchTrie.AddForSearching(data.DefaultName, data.Item);
		//}
	}


	public override void UpdateWindowUI () {
		if (WorldSquad.Enable) Game.StopGame();
		GUI_Toolbar(WindowRect.EdgeInside(Direction4.Up, Unify(32)));
		GUI_Editor(WindowRect.Shrink(Unify(24), Unify(24), 0, Unify(42)));
		GUI_Hotkey();
		CheckItemNamesChange();
	}


	private void GUI_Toolbar (IRect toolbarRect) {

		// BG
		Renderer.DrawPixel(toolbarRect, Color32.GREY_32);




	}


	private void GUI_Editor (IRect panelRect) {

		int fieldPadding = Unify(6);
		int fieldHeight = Unify(128);
		int fieldWidth = fieldHeight * 2;
		int column = (panelRect.width / (fieldWidth + fieldPadding)).GreaterOrEquel(1);
		int row = Lines.Count.CeilDivide(column);
		int extendedContentHeight = row * (fieldHeight + fieldPadding) + Unify(64);
		int offsetX = (panelRect.width - column * (fieldWidth + fieldPadding)) / 2;

		using (var scroll = Scope.GUIScroll(panelRect, MasterScroll, 0, (extendedContentHeight - panelRect.height).GreaterOrEquelThanZero())) {
			MasterScroll = scroll.ScrollPosition;
			var rect = new IRect(0, 0, fieldWidth, fieldHeight);
			for (int index = 0; index < Lines.Count; index++) {
				var line = Lines[index];
				rect.x = panelRect.x + (index % column) * (rect.width + fieldPadding) + offsetX;
				rect.y = panelRect.yMax - ((index / column) + 1) * (rect.height + fieldPadding);

				// BG
				Renderer.DrawSlice(UI_PANEL_ITEM, rect);

				// Com Icons




				// Result Icon





			}
		}

		MasterScroll = GUI.ScrollBar(
			234622,
			WindowRect.EdgeInside(Direction4.Right, Unify(16)).ShrinkUp(Unify(32)),
			MasterScroll,
			extendedContentHeight,
			panelRect.height
		);

	}


	private void GUI_Hotkey () {
		// Save
		if (Input.KeyboardDownWithCtrl(KeyboardKey.S)) {
			Save(forceSave: true);
		}
	}


	#endregion



	#region --- API ---


	public void SetCurrentProject (Project project) {
		CurrentProject = project;
		Lines.Clear();
		SearchTrie.Clear();
		ItemNames.Clear();
		LastItemNameCheckFrame = int.MinValue;
		if (project == null) return;
		LoadFromFile(project.Universe.ItemCombinationPath);
	}


	public void LoadFromFile (string filePath) {
		// Content
		Lines.Clear();
		foreach (var (com, data) in ItemCombination.ForAllCombinationInFile(filePath)) {
			var itemA = ItemSystem.GetItem(com.x);
			var itemB = ItemSystem.GetItem(com.y);
			var itemC = ItemSystem.GetItem(com.z);
			var itemD = ItemSystem.GetItem(com.w);
			var itemResult = ItemSystem.GetItem(data.Result);
			Lines.Add(new CombinationLine() {
				Order = data.Order,
				ItemA = com.x,
				ItemB = com.y,
				ItemC = com.z,
				ItemD = com.w,
				KeepA = data.Keep(com.x),
				KeepB = data.Keep(com.y),
				KeepC = data.Keep(com.z),
				KeepD = data.Keep(com.w),
				NameA = itemA != null ? itemA.GetType().AngeName() : "",
				NameB = itemB != null ? itemB.GetType().AngeName() : "",
				NameC = itemC != null ? itemC.GetType().AngeName() : "",
				NameD = itemD != null ? itemD.GetType().AngeName() : "",
				Result = data.Result,
				ResultName = itemResult != null ? itemResult.GetType().AngeName() : "",
				ResultCount = data.ResultCount,
			});
		}
		Lines.Sort(LineComparer.Instance);


	}


	public override void Save (bool forceSave = false) {
		base.Save(forceSave);
		if (!IsDirty && !forceSave) return;
		var builder = new StringBuilder();
		foreach (var line in Lines) {
			if (line.ItemA != 0) {
				if (line.KeepA) builder.Append('^');
				builder.Append(line.NameA);
			}
			if (line.ItemB != 0) {
				builder.Append(' ');
				builder.Append('+');
				builder.Append(' ');
				if (line.KeepB) builder.Append('^');
				builder.Append(line.NameB);
			}
			if (line.ItemC != 0) {
				builder.Append(' ');
				builder.Append('+');
				builder.Append(' ');
				if (line.KeepC) builder.Append('^');
				builder.Append(line.NameC);
			}
			if (line.ItemD != 0) {
				builder.Append(' ');
				builder.Append('+');
				builder.Append(' ');
				if (line.KeepD) builder.Append('^');
				builder.Append(line.NameD);
			}
			builder.Append(' ');
			builder.Append('=');
			builder.Append(' ');
			if (line.ResultCount > 1) {
				builder.Append(line.ResultCount);
				builder.Append(' ');
			}
			builder.Append(line.ResultName);
			builder.Append('\n');
		}
		// Save to File
		Util.TextToFile(builder.ToString(), UniverseSystem.BuiltInUniverse.ItemCombinationPath);
	}


	#endregion




	#region --- LGC ---


	private void CheckItemNamesChange () {
		if (LastItemNameCheckFrame == int.MaxValue || CurrentProject == null) return;
		if (Game.PauselessFrame < LastItemNameCheckFrame + 30) return;
		LastItemNameCheckFrame = Game.PauselessFrame;
		string path = CurrentProject.Universe.ItemNamePath;
		if (!Util.FileExists(path)) return;
		ItemNames.Clear();
		SearchTrie.Clear();
		foreach (string line in Util.ForAllLinesInFile(path)) {
			int emptyIndex = line.IndexOf(' ');
			if (emptyIndex < 0) continue;
			string name = line[..emptyIndex];
			var iName = new ItemName() {
				ID = name.AngeHash(),
				Name = name,
				TypePath = line[(emptyIndex + 1)..],
			};
			ItemNames.Add(iName);
			SearchTrie.AddForSearching(name, iName);
		}
		ItemNames.Sort(ItemNameComparer.Instance);
		LastItemNameCheckFrame = int.MaxValue;
	}


	#endregion


}