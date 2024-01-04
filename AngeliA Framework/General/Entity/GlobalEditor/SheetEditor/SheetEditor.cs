using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace AngeliaFramework {
	public partial class SheetEditor : GlobalEditorUI {




		#region --- VAR ---


		// Const
		public static readonly int TYPE_ID = typeof(SheetEditor).AngeHash();
		private const int PANEL_WIDTH = 300;

		// Api
		public new static bool IsActived => Instance != null && Instance.Active;
		public new static SheetEditor Instance => GlobalEditorUI.Instance as SheetEditor;

		// Short
		private EditableAtlas SelectingAtlas => SelectingAtlasIndex >= 0 && SelectingAtlasIndex < Atlas.Count ? Atlas[SelectingAtlasIndex] : null;
		private EditableUnit SelectingUnit => SelectingAtlas != null && SelectingUnitIndex >= 0 && SelectingUnitIndex < SelectingAtlas.Units.Count ? SelectingAtlas.Units[SelectingUnitIndex] : null;

		// Data
		private readonly List<EditableAtlas> Atlas = new();
		private int SelectingAtlasIndex = 0;
		private int SelectingUnitIndex = 0;
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
			SelectingAtlasIndex = 0;
			SelectingUnitIndex = 0;
			FilePanelScroll = 0;
			System.GC.Collect();
		}


		public override void OnInactivated () {
			base.OnInactivated();
			WorldSquad.Enable = true;
			if (IsDirty) {
				SaveToDisk(forceSave: true);
				UniverseGenerator.RebuildSheetAndTextureToDisk(
					Atlas, AngePath.EditableAtlasRoot, AngePath.EditableSheetRoot, AngePath.EditableTexturePath
				);
				Version.GrowVersion(AngePath.EditableSheetRoot);
				CellRenderer.RequireReloadUserSheet();
			}
			Atlas.Clear();
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
			ControlHintUI.ForceOffset(Unify(PANEL_WIDTH), 0);

			if (IsDirty && Game.GlobalFrame % 600 == 0) SaveToDisk();

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
			Atlas.Clear();
			UniverseGenerator.SyncSheet(
				AngePath.SheetRoot, AngePath.SheetTexturePath, AngePath.EditableAtlasRoot
			);
			UniverseGenerator.LoadAtlasFromDisk(AngePath.EditableAtlasRoot, Atlas);
		}


		private void SaveToDisk (bool forceSave = false) {
			IsDirty = false;
			UniverseGenerator.SaveAtlasToDisk(Atlas, AngePath.EditableAtlasRoot, forceSave);
		}


		private void SetDirty (int atlasIndex, int unitIndex, int spriteIndex) {
			IsDirty = true;
			if (atlasIndex >= 0 && atlasIndex < Atlas.Count) {
				var atlas = Atlas[atlasIndex];
				atlas.IsDirty = true;
				if (unitIndex >= 0 && unitIndex < atlas.Units.Count) {
					var unit = atlas.Units[unitIndex];
					unit.IsDirty = true;
					if (spriteIndex >= 0 && spriteIndex < unit.Sprites.Count) {
						var sprite = unit.Sprites[spriteIndex];
						sprite.IsDirty = true;
					}
				}
			}
		}


		private void SelectUnit (int newAtlasIndex, int newUnitIndex) {
			if (SelectingAtlasIndex == newAtlasIndex && SelectingUnitIndex == newUnitIndex) return;
			SelectingAtlasIndex = newAtlasIndex;
			SelectingUnitIndex = newUnitIndex;




		}


		#endregion




	}
}