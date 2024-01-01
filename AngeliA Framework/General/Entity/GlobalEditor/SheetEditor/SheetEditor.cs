using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace AngeliaFramework {
	public partial class SheetEditor : GlobalEditorUI {




		#region --- SUB ---


		// Comparer
		private class GroupComparer : IComparer<EditalbeGroup> {
			public static readonly GroupComparer Instance = new();
			public int Compare (EditalbeGroup a, EditalbeGroup b) => a.Order.CompareTo(b.Order);
		}


		private class FlexUnitComparer : IComparer<EditableUnit> {
			public static readonly FlexUnitComparer Instance = new();
			public int Compare (EditableUnit a, EditableUnit b) => a.Order.CompareTo(b.Order);
		}


		private class EditableSpriteComparer : IComparer<EditableSprite> {
			public static readonly EditableSpriteComparer Instance = new();
			public int Compare (EditableSprite a, EditableSprite b) => a.Order.CompareTo(b.Order);
		}


		// Data
		[JsonObject(MemberSerialization.OptIn)]
		private class EditalbeGroup {
			public List<EditableUnit> Units = new();
			public string Guid = "";
			[JsonProperty] public int Order = 0;
			[JsonProperty] public int SheetZ = 0;
			[JsonProperty] public string Name = "";
			[JsonProperty] public SheetType SheetType = SheetType.General;
		}


		[JsonObject(MemberSerialization.OptIn)]
		private class EditableUnit {
			public List<EditableSprite> Sprites = new();
			public string Guid = "";
			[JsonProperty] public int Order = 0;
			[JsonProperty] public string Name = "";
			[JsonProperty] public GroupType GroupType = GroupType.General;
		}


		[JsonObject(MemberSerialization.OptIn)]
		private class EditableSprite {

			[JsonProperty] public int Order;
			[JsonProperty] public int AngePivotX;
			[JsonProperty] public int AngePivotY;
			[JsonProperty] public int BorderL;
			[JsonProperty] public int BorderR;
			[JsonProperty] public int BorderD;
			[JsonProperty] public int BorderU;
			[JsonProperty] public int Width;
			[JsonProperty] public int Height;
			[JsonProperty] public bool IsTrigger;
			[JsonProperty] public string RuleString;
			[JsonProperty] public string TagString;
			[JsonProperty] public bool LoopStart = false;
			[JsonProperty] public bool NoCollider = false;
			[JsonProperty] public int OffsetZ = 0;

			public string Guid = "";
			public Byte4[] Pixels;

			private static readonly StringBuilder NameBuilder = new();

			public string GetFullName (EditalbeGroup group, EditableUnit unit, int index) {

				NameBuilder.Clear();
				NameBuilder.Append(unit.Name);
				if (index >= 0) {
					NameBuilder.Append(' ');
					NameBuilder.Append(index);
				}

				if (IsTrigger) {
					NameBuilder.Append(" #isTrigger");
				}

				if (!string.IsNullOrWhiteSpace(TagString)) {
					NameBuilder.Append($" #tag={TagString}");
				}

				if (unit.GroupType != GroupType.General) {
					switch (unit.GroupType) {
						case GroupType.Rule:
							if (!string.IsNullOrWhiteSpace(RuleString)) {
								NameBuilder.Append($" #rule={RuleString}");
							}
							break;
						case GroupType.Random:
							NameBuilder.Append(" #ran");
							break;
						case GroupType.Animated:
							NameBuilder.Append(" #ani");
							if (LoopStart) {
								NameBuilder.Append(" #loopStart");
							}
							break;
					}
				}

				if (group.SheetType == SheetType.Level && NoCollider) {
					NameBuilder.Append(" #noCollider");
				}

				if (OffsetZ != 0) {
					NameBuilder.Append($" #z={OffsetZ}");
				}

				return NameBuilder.ToString();
			}

		}


		#endregion




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(SheetEditor).AngeHash();

		// Api
		public new static bool IsActived => Instance != null && Instance.Active;
		public new static SheetEditor Instance => GlobalEditorUI.Instance as SheetEditor;

		// Data
		private readonly List<EditalbeGroup> Groups = new();
		private bool TaskingRoute;
		private bool CtrlHolding;
		private bool ShiftHolding;
		private bool AltHolding;
		private bool IsDirty;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			WorldSquad.Enable = false;
			Stage.DespawnAllEntitiesFromWorld();
			if (Player.Selecting != null) {
				Player.Selecting.Active = false;
			}
			LoadFromDisk();
			System.GC.Collect();
		}


		public override void OnInactivated () {
			base.OnInactivated();
			WorldSquad.Enable = true;
			if (IsDirty) {
				SaveToDisk();
				Rebuild();
			}
			Groups.Clear();
			System.GC.Collect();
		}


		public override void UpdateUI () {
			base.UpdateUI();
			Skybox.ForceSkyboxTint(Const.GREY_32, Const.GREY_32);
			Update_Misc();
			Update_Panel();
			Update_Hotkey();
			Update_Editor();
			Update_Inspector();
			Update_Canvas();
		}


		private void Update_Misc () {

			CursorSystem.RequireCursor(int.MinValue);

			TaskingRoute = FrameTask.HasTask();
			CtrlHolding = FrameInput.KeyboardHolding(KeyboardKey.LeftCtrl) || FrameInput.KeyboardHolding(KeyboardKey.RightCtrl) || FrameInput.KeyboardHolding(KeyboardKey.CapsLock);
			ShiftHolding = FrameInput.KeyboardHolding(KeyboardKey.LeftShift) || FrameInput.KeyboardHolding(KeyboardKey.RightShift);
			AltHolding = FrameInput.KeyboardHolding(KeyboardKey.LeftAlt) || FrameInput.KeyboardHolding(KeyboardKey.RightAlt);

			ControlHintUI.ForceShowHint();
			ControlHintUI.ForceHideGamepad();



		}


		private void Update_Hotkey () {
			if (TaskingRoute || CellRendererGUI.IsTyping) return;
			if (GenericPopupUI.ShowingPopup || GenericDialogUI.ShowingDialog) return;

			// No Operation Holding
			if (!CtrlHolding && !ShiftHolding && !AltHolding) {


			}

			// Ctrl
			if (CtrlHolding) {
				// Save
				if (FrameInput.KeyboardDown(KeyboardKey.S)) {
					if (IsDirty) SaveToDisk();
				}


			}



		}


		#endregion




		#region --- LGC ---


		private void LoadFromDisk () {
			IsDirty = false;
			Groups.Clear();
			// From File
			foreach (var groupPath in Util.EnumerateFolders(AngePath.FlexibleSheetRoot, true, "*")) {
				// Load Meta
				string groupMetaPath = Util.CombinePaths(groupPath, "Meta.json");
				var group = JsonUtil.LoadOrCreateJsonFromPath<EditalbeGroup>(groupMetaPath);
				group.Guid = Util.GetNameWithExtension(groupPath);
				// Load Units
				foreach (var unitPath in Util.EnumerateFolders(groupPath, true, "*")) {
					// Load Meta
					string unitMetaPath = Util.CombinePaths(unitPath, "Meta.json");
					var unit = JsonUtil.LoadOrCreateJsonFromPath<EditableUnit>(unitMetaPath);
					unit.Guid = Util.GetNameWithExtension(unitPath);
					// Load Sprites
					foreach (var spritePath in Util.EnumerateFolders(unitPath, true, "*")) {
						// Sprite
						string jsonPath = Util.CombinePaths(spritePath, "Sprite.json");
						var sprite = JsonUtil.LoadOrCreateJson<EditableSprite>(jsonPath);
						sprite.Guid = Util.GetNameWithExtension(spritePath);
						// Pixels
						string pixelPath = Util.CombinePaths(spritePath, "Pixels");
						var pixels = Util.FileToByte4(pixelPath);
						// Final
						sprite.Pixels = pixels;
						unit.Sprites.Add(sprite);
					}
					unit.Sprites.Sort(EditableSpriteComparer.Instance);
					group.Units.Add(unit);
				}
				group.Units.Sort(FlexUnitComparer.Instance);
				Groups.Add(group);
			}
			Groups.Sort(GroupComparer.Instance);
		}


		private void SaveToDisk () {
			IsDirty = false;
			for (int groupIndex = 0; groupIndex < Groups.Count; groupIndex++) {
				// Save Group
				var group = Groups[groupIndex];
				group.Order = groupIndex;
				if (string.IsNullOrWhiteSpace(group.Guid)) group.Guid = System.Guid.NewGuid().ToString();
				string groupPath = Util.CombinePaths(AngePath.FlexibleSheetRoot, group.Guid);
				string groupMetaPath = Util.CombinePaths(groupPath, "Meta.json");
				JsonUtil.SaveJsonToPath(group, groupMetaPath, false);
				for (int unitIndex = 0; unitIndex < group.Units.Count; unitIndex++) {
					// Save Unit
					var unit = group.Units[unitIndex];
					unit.Order = unitIndex;
					if (string.IsNullOrWhiteSpace(unit.Guid)) unit.Guid = System.Guid.NewGuid().ToString();
					string unitPath = Util.CombinePaths(groupPath, unit.Guid);
					string unitMetaPath = Util.CombinePaths(unitPath, "Meta.json");
					JsonUtil.SaveJsonToPath(unit, unitMetaPath, false);
					for (int spriteIndex = 0; spriteIndex < unit.Sprites.Count; spriteIndex++) {
						// Save Sprite
						var sprite = unit.Sprites[spriteIndex];
						sprite.Order = spriteIndex;
						if (string.IsNullOrWhiteSpace(sprite.Guid)) sprite.Guid = System.Guid.NewGuid().ToString();
						string spritePath = Util.CombinePaths(unitPath, sprite.Guid);
						string spriteMetaPath = Util.CombinePaths(spritePath, "Sprite.json");
						JsonUtil.SaveJsonToPath(sprite, spriteMetaPath, false);
						// Save Pixels
						string pixelPath = Util.CombinePaths(spritePath, "Pixels");
						Util.Byte4ToFile(sprite.Pixels, pixelPath);
					}
				}
			}
		}


		private void Rebuild () {

			// Editable >> Flex
			var tResults = new List<(object texture, FlexSprite[] flexs)>();

			for (int groupIndex = 0; groupIndex < Groups.Count; groupIndex++) {
				var group = Groups[groupIndex];
				for (int unitIndex = 0; unitIndex < group.Units.Count; unitIndex++) {
					var unit = group.Units[unitIndex];
					for (int spriteIndex = 0; spriteIndex < unit.Sprites.Count; spriteIndex++) {
						var sprite = unit.Sprites[spriteIndex];
						tResults.Add(
							(Game.GetTextureFromPixels(sprite.Pixels, sprite.Width, sprite.Height),
							new FlexSprite[]{ new FlexSprite() {
								Name = sprite.GetFullName(group, unit, unit.Sprites.Count > 1 ? spriteIndex: -1),
								AngePivot = new Int2(sprite.AngePivotX, sprite.AngePivotY),
								Border = new Float4(sprite.BorderL, sprite.BorderD, sprite.BorderR, sprite.BorderU),
								Rect = new FRect(0, 0, sprite.Width, sprite.Height),
								SheetName = group.Name,
								SheetType = group.SheetType,
								SheetZ = group.SheetZ,
							} })
						);
					}
				}
			}

			// Combine Flex
			UniverseGenerator.CombineFlexTextures(
				tResults, out var sheetTexturePixels, out int textureWidth, out int textureHeight, out var flexSprites
			);

			// Flex Sprites >> Sheet
			var sheet = UniverseGenerator.CreateSpriteSheet(sheetTexturePixels, textureWidth, textureHeight, flexSprites);
			if (sheet != null) {
				JsonUtil.SaveJson(sheet, AngePath.SheetRoot);
			}

			// Save Texture to File
			var texture = Game.GetTextureFromPixels(sheetTexturePixels, textureWidth, textureHeight);
			if (texture != null) {
				Game.SaveTextureAsPNGFile(texture, AngePath.SheetTexturePath);
			}

			// Init CellRenderer
			CellRenderer.InitializePool();
			Game.SetTextureForRenderer(texture);

		}


		#endregion




	}
}