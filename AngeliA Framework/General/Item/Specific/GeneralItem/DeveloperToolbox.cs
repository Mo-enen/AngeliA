using System.Linq;
using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	[RequireSpriteFromField]
	[RequireLanguageFromField]
	public class iDeveloperToolbox : Item {




		#region --- SUB ---


		private class BarData {
			public IntToChars I2C;
			public int Value;
			public int Capacity;
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly SpriteCode[] BTN_SPRITES = { "DeveloperToolbox.Collider", "DeveloperToolbox.Bound", "DeveloperToolbox.Profiler", "DeveloperToolbox.Effect", "DeveloperToolbox.Editor", };
		private static readonly Byte4[] COLLIDER_TINTS = { Const.RED_BETTER, Const.ORANGE_BETTER, Const.YELLOW, Const.GREEN, Const.CYAN, Const.BLUE, Const.GREY_128, };
		private static readonly LanguageCode PROJECT_NAME = "DeveloperToolbox.ProjectName";
		private static readonly LanguageCode CREATE_PROJECT = "DeveloperToolbox.CreateProject";
		private static readonly LanguageCode BUILT_IN_PROJECT = "DeveloperToolbox.BuiltInProject";
		private static readonly LanguageCode PROJECT_SELECTOR_MSG = "DeveloperToolbox.ProjectSelectorMsg";
		private static readonly LanguageCode UI_CREATE = "UI.Create";
		private static readonly LanguageCode UI_CANCEL = "UI.Cancel";
		private static readonly LanguageCode NEW_PROJECT_NAME = "Project.NewProjectName";
		private static readonly LanguageCode NEW_PROJECT_ERROR_EXISTS = "Project.Error.NameExists";
		private const string NEW_PROJECT_DEFAULT_MSG = "Having ideas for your own level? Create a new project with the button below now. Unleash your imagination and make something unique and amazing!";

		// Api
		public override int MaxStackCount => 1;

		// Data
		private static iDeveloperToolbox Instance;
		private static System.Action OpenProjectCallback = null;
		private readonly BarData[] RenderingUsages = new BarData[RenderLayer.COUNT];
		private readonly BarData[] EntityUsages = new BarData[EntityLayer.COUNT];
		private BarData[] TextUsages = new BarData[0];
		private readonly List<PhysicsCell[,,]> CellPhysicsCells = new();
		private readonly bool[] EffectsEnabled = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
		private int RequireToolboxFrame = int.MinValue;
		private int RequireDataFrame = int.MinValue;
		private int DrawColliderFrame = int.MinValue;
		private int DrawBoundFrame = int.MinValue;
		private int SelectingPanelIndex = -1;
		private IRect PanelRect;
		private bool CreatingNewProject = false;
		private bool DataInitialized = false;
		private string CreatingNewProjectName = "";
		private string CreatingNewProjectErrorMessage = "";
		private int ProjectSelectorScrollIndex = 0;


		#endregion




		#region --- MSG ---


		public iDeveloperToolbox () {
			Instance = this;
			DataInitialized = false;
		}


		[OnProjectOpen]
		public static void OnProjectOpen () => OpenProjectCallback = null;


		[OnGameUpdateLater(-4097)]
		public static void OnGameUpdateLaterMin () => Instance?.DrawToolboxUI();


		[OnGameUpdateLater(4096)]
		public static void OnGameUpdateLaterMax () => Instance?.DrawToolboxGizmos();


		private void DrawToolboxUI () {

			if (Game.GlobalFrame > RequireToolboxFrame + 1) return;

			CursorSystem.RequireCursor();
			int oldLayer = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToUI();

			var panelRect = new IRect(CellRenderer.CameraRect.xMax, CellRenderer.CameraRect.yMax, 0, 0);
			int panelYMax = panelRect.y;

			// Tool Buttons
			int buttonSize = CellRendererGUI.Unify(36);
			int padding = CellRendererGUI.Unify(6);
			panelRect.height = buttonSize + padding * 2;
			panelRect.y -= panelRect.height;

			var rect = new IRect(panelRect.xMax - buttonSize - padding, panelRect.y + padding, buttonSize, buttonSize);

			// Dodge FPS Label
			if (Game.ShowFPS) rect.x -= CellRendererGUI.Unify(32);

			// Draw All Buttons
			for (int i = 0; i < BTN_SPRITES.Length; i++) {
				if (SelectingPanelIndex == i) {
					CellRenderer.Draw(Const.PIXEL, rect, Const.GREEN, int.MaxValue - 1);
				}
				int spriteCode = BTN_SPRITES[i];
				if (CellRendererGUI.Button(rect, spriteCode, spriteCode, spriteCode, 0, 0, 0, int.MaxValue)) {
					SelectingPanelIndex = SelectingPanelIndex != i ? i : -1;
					CreatingNewProject = false;
					CreatingNewProjectName = "";
					CreatingNewProjectErrorMessage = "";
					ProjectSelectorScrollIndex = 0;
					EffectsEnabled.FillWithValue(false);
				}
				rect.x -= i == BTN_SPRITES.Length - 1 ? padding : rect.width + padding;
			}
			panelRect.width = panelRect.x - rect.x;
			panelRect.x = rect.x;

			// Show Toolbox Panel
			switch (SelectingPanelIndex) {
				case 0:
					// Colliders
					DrawColliderFrame = Game.GlobalFrame;
					break;
				case 1:
					// Bounds
					DrawBoundFrame = Game.GlobalFrame;
					break;
				case 2:
					// Profiler
					DrawProfilerPanel(ref panelRect);
					break;
				case 3:
					// Effect
					DrawEffectPanel(ref panelRect);
					break;
				case 4:
					// Editor
					if (CreatingNewProject) {
						DrawNewProjectPanel(ref panelRect);
					} else {
						DrawProjectSelectorPanel(ref panelRect);
					}
					break;
			}

			// BG
			PanelRect = new IRect(panelRect.x, panelRect.y, panelRect.width, panelYMax - panelRect.y);
			CellRenderer.Draw(Const.PIXEL, PanelRect, Const.BLACK, int.MaxValue - 16);

			// Finish
			CellRenderer.SetLayer(oldLayer);
			if (FrameInput.MouseLeftButton && PanelRect.MouseInside()) {
				FrameInput.UseMouseKey(0);
				FrameInput.UseGameKey(Gamekey.Action);
			}
		}


		private void DrawToolboxGizmos () {
			if (Game.GlobalFrame > RequireToolboxFrame + 1) return;

			if (!DataInitialized) {
				DataInitialized = true;
				TextUsages = new BarData[CellRenderer.TextLayerCount];
				for (int i = 0; i < RenderingUsages.Length; i++) {
					int capa = CellRenderer.GetLayerCapacity(i);
					RenderingUsages[i] = new BarData() {
						Capacity = capa,
						I2C = new IntToChars($"{CellRenderer.GetLayerName(i)}  ", $" / {capa}"),
					};
				}
				for (int i = 0; i < TextUsages.Length; i++) {
					int capa = CellRenderer.GetTextLayerCapacity(i);
					TextUsages[i] = new BarData() {
						Capacity = capa,
						I2C = new IntToChars($"{CellRenderer.GetTextLayerName(i)}  ", $" / {capa}"),
					};
				}
				for (int i = 0; i < EntityUsages.Length; i++) {
					int capa = Stage.Entities[i].Length;
					EntityUsages[i] = new BarData() {
						Capacity = capa,
						I2C = new IntToChars($"{EntityLayer.LAYER_NAMES[i]}  ", $" / {capa}"),
					};
				}
			}

			// Collect Data
			if (Game.GlobalFrame <= RequireDataFrame + 1) {
				for (int i = 0; i < RenderLayer.COUNT; i++) {
					RenderingUsages[i].Value = CellRenderer.GetUsedCellCount(i);
				}
				for (int i = 0; i < CellRenderer.TextLayerCount; i++) {
					TextUsages[i].Value = CellRenderer.GetTextUsedCellCount(i);
				}
				for (int i = 0; i < EntityLayer.COUNT; i++) {
					EntityUsages[i].Value = Stage.EntityCounts[i];
				}
			}

			if (!PlayerMenuUI.ShowingUI) {
				// Draw Colliders
				if (Game.GlobalFrame <= DrawColliderFrame + 1) {
					// Init Cells
					if (CellPhysicsCells.Count == 0) {
						try {
							var layers = Util.GetStaticFieldValue(typeof(CellPhysics), "Layers") as System.Array;
							for (int layerIndex = 0; layerIndex < PhysicsLayer.COUNT; layerIndex++) {
								var layerObj = layers.GetValue(layerIndex);
								CellPhysicsCells.Add(Util.GetFieldValue(layerObj, "Cells") as PhysicsCell[,,]);
							}
						} catch (System.Exception ex) { Game.LogException(ex); }
						if (CellPhysicsCells.Count == 0) CellPhysicsCells.Add(null);
					}
					// Draw Cells
					if (CellPhysicsCells.Count > 0 && CellPhysicsCells[0] != null) {
						var cameraRect = CellRenderer.CameraRect;
						int thick = CellRendererGUI.Unify(1);
						for (int layer = 0; layer < CellPhysicsCells.Count; layer++) {
							try {
								var tint = COLLIDER_TINTS[layer.Clamp(0, COLLIDER_TINTS.Length - 1)];
								var cells = CellPhysicsCells[layer];
								int cellWidth = cells.GetLength(0);
								int cellHeight = cells.GetLength(1);
								int celDepth = cells.GetLength(2);
								for (int y = 0; y < cellHeight; y++) {
									for (int x = 0; x < cellWidth; x++) {
										for (int d = 0; d < celDepth; d++) {
											var cell = cells[x, y, d];
											if (cell.Frame != CellPhysics.CurrentFrame) break;
											if (!cell.Rect.Overlaps(cameraRect)) continue;
											DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Down, thick), tint, true);
											DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Up, thick), tint, true);
											DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Left, thick), tint, false);
											DrawGizmosRectAsLine(PanelRect, cell.Rect.EdgeInside(Direction4.Right, thick), tint, false);
										}
									}
								}
							} catch (System.Exception ex) { Game.LogException(ex); }
						}
					}
				}

				// Draw Bounds
				if (Game.GlobalFrame <= DrawBoundFrame + 1) {
					int thick = CellRendererGUI.Unify(1);
					for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
						var entities = Stage.Entities[layer];
						int count = Stage.EntityCounts[layer];
						for (int i = 0; i < count; i++) {
							var e = entities[i];
							if (!e.Active) continue;
							DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Down, thick), Const.BLUE_BETTER, true);
							DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Up, thick), Const.BLUE_BETTER, true);
							DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Left, thick), Const.BLUE_BETTER, false);
							DrawGizmosRectAsLine(PanelRect, e.GlobalBounds.EdgeInside(Direction4.Right, thick), Const.BLUE_BETTER, false);
						}
					}
				}
			}

			// Func
			static void DrawGizmosRectAsLine (IRect panelRect, IRect rect, Byte4 color, bool horizontal) {
				if (!rect.Overlaps(panelRect)) {
					Game.DrawGizmosRect(rect, color);
				} else if (horizontal) {
					if (rect.x > panelRect.x && rect.xMax < panelRect.xMax) return;
					if (rect.x < panelRect.x) {
						Game.DrawGizmosRect(new IRect(rect.x, rect.y, panelRect.x - rect.x, rect.height), color);
					}
					if (rect.xMax > panelRect.xMax) {
						Game.DrawGizmosRect(new IRect(panelRect.xMax, rect.y, rect.xMax - panelRect.xMax, rect.height), color);
					}
				} else {
					if (rect.y > panelRect.y && rect.yMax < panelRect.yMax) return;
					if (rect.y < panelRect.y) {
						Game.DrawGizmosRect(new IRect(rect.x, rect.y, rect.width, panelRect.y - rect.y), color);
					}
					if (rect.yMax > panelRect.yMax) {
						Game.DrawGizmosRect(new IRect(rect.x, panelRect.yMax, rect.width, rect.yMax - panelRect.yMax), color);
					}
				}
			}
		}


		public override void OnItemUpdate_FromInventory (Entity holder) {
			base.OnItemUpdate_FromInventory(holder);
			RequireToolboxFrame = Game.GlobalFrame;
		}


		private void DrawProfilerPanel (ref IRect panelRect) {

			RequireDataFrame = Game.GlobalFrame;
			int barHeight = CellRendererGUI.Unify(24);
			panelRect.height = barHeight * (EntityUsages.Length + TextUsages.Length + RenderingUsages.Length);
			panelRect.y -= panelRect.height;
			int barPadding = CellRendererGUI.Unify(4);
			var rect = new IRect(panelRect.x, panelRect.yMax - barHeight, panelRect.width, barHeight);

			// Entity
			for (int i = 0; i < EntityUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), EntityUsages[i], Const.CYAN);
				rect.y -= rect.height;
			}
			// Rendering
			for (int i = 0; i < RenderingUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), RenderingUsages[i], Const.GREEN);
				rect.y -= rect.height;
			}
			// Text
			for (int i = 0; i < TextUsages.Length; i++) {
				DrawBar(rect.Shrink(barPadding), TextUsages[i], Const.GREEN);
				rect.y -= rect.height;
			}

			// Func
			static void DrawBar (IRect rect, BarData data, Byte4 barColor) {
				int width = Util.RemapUnclamped(0, data.Capacity, 0, rect.width, data.Value);
				CellRenderer.Draw(Const.PIXEL, new IRect(rect.x, rect.y, width, rect.height), barColor, int.MaxValue);
				// Label
				int startIndex = CellRenderer.GetTextUsedCellCount();
				CellRendererGUI.Label(
					CellContent.Get(data.I2C.GetChars(data.Value), Const.GREY_230, 14, Alignment.MidMid), rect
				);
				if (CellRenderer.GetTextCells(out var cells, out int count)) {
					for (int i = startIndex; i < count && i < startIndex + data.I2C.Prefix.Length; i++) {
						cells[i].Color = new Byte4(96, 96, 96, 255);
					}
				}
			}
		}


		private void DrawEffectPanel (ref IRect panelRect) {
			int itemHeight = CellRendererGUI.Unify(28);
			int itemPadding = CellRendererGUI.Unify(8);
			panelRect.height = Const.SCREEN_EFFECT_COUNT * (itemHeight + itemPadding) + itemPadding;
			panelRect.y -= panelRect.height;
			var rect = new IRect(panelRect.x + itemPadding, panelRect.yMax, panelRect.width - itemPadding * 2, itemHeight);
			for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {

				rect.y -= itemHeight + itemPadding;

				// Label
				string name = Const.SCREEN_EFFECT_NAMES[i];
				CellRendererGUI.Label(CellContent.Get(
					name, Const.GREY_216, charSize: 18, alignment: Alignment.MidLeft
				), rect);

				// Enable Button
				var enableRect = rect.EdgeInside(Direction4.Right, itemHeight);
				bool enable = EffectsEnabled[i];
				if (CellRendererGUI.Button(
					enableRect,
					BuiltInIcon.CIRCLE_16, BuiltInIcon.CIRCLE_16, BuiltInIcon.CIRCLE_16,
					enable ? BuiltInIcon.CHECK_MARK_16 : 0,
					0, itemHeight / 5, z: int.MaxValue - 8,
					Const.GREY_32, Const.WHITE
				)) {
					EffectsEnabled[i] = !enable;
				}
				CursorSystem.SetCursorAsHand(enableRect);
			}

			// Update Values
			for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
				if (EffectsEnabled[i]) {
					Game.PassEffect(i);
				}
			}
			if (EffectsEnabled[Const.SCREEN_EFFECT_RETRO_DARKEN]) {
				Game.PassEffect_RetroDarken(Game.GlobalFrame.PingPong(60) / 60f);
			}
			if (EffectsEnabled[Const.SCREEN_EFFECT_RETRO_LIGHTEN]) {
				Game.PassEffect_RetroLighten(Game.GlobalFrame.PingPong(60) / 60f);
			}
			if (EffectsEnabled[Const.SCREEN_EFFECT_TINT]) {
				Game.PassEffect_Tint(Byte4.LerpUnclamped(new Byte4(255, 128, 196, 255), new Byte4(128, 255, 64, 255), Game.GlobalFrame.PingPong(120) / 120f));
			}

		}


		private void DrawNewProjectPanel (ref IRect panelRect) {

			int itemHeight = CellRendererGUI.Unify(48);
			int itemShrink = CellRendererGUI.Unify(10);
			int padding = CellRendererGUI.Unify(8);
			int border = CellRendererGUI.Unify(1.5f);
			int buttonShrink = CellRendererGUI.Unify(6);
			int labelWidth = CellRendererGUI.Unify(72);

			var rect = new IRect(panelRect.x, panelRect.y, panelRect.width, itemHeight);

			// Error Message
			if (!string.IsNullOrEmpty(CreatingNewProjectErrorMessage)) {
				int msgHeight = CellRendererGUI.Unify(72);
				rect.y -= msgHeight + padding;
				CellRendererGUI.Label(CellContent.Get(
					CreatingNewProjectErrorMessage, Const.RED_BETTER,
					charSize: 18, alignment: Alignment.MidMid, wrap: true
				), rect, out var msgBounds);
				rect.y = msgBounds.y - padding;
			}

			// Label
			rect.height = itemHeight;
			rect.y -= itemHeight + padding;
			CellRendererGUI.Label(
				CellContent.Get(PROJECT_NAME.Get("Name"), tint: Const.GREY_196, charSize: 18, alignment: Alignment.MidLeft),
				rect.EdgeInside(Direction4.Left, labelWidth).Shrink(itemShrink, 0, 0, 0)
			);

			// Field
			rect.height = itemHeight;
			var fieldRect = rect.EdgeInside(Direction4.Right, rect.width - labelWidth).Shrink(0, 0, itemShrink, itemShrink);
			CreatingNewProjectName = CellRendererGUI.TextField(83474, fieldRect, CreatingNewProjectName);
			CellRenderer.Draw_9Slice(
				BuiltInIcon.FRAME_16, fieldRect, border, border, border, border,
				Const.GREY_128, int.MaxValue - 6
			);

			// Create 
			rect.height = itemHeight;
			rect.y -= itemHeight + padding;
			if (CellRendererGUI.Button(
				rect.EdgeInside(Direction4.Left, rect.width / 2).Shrink(buttonShrink / 2, buttonShrink / 2, buttonShrink, buttonShrink),
				BuiltInIcon.UI_BUTTON,
				UI_CREATE.Get("Create"), out _,
				z: int.MaxValue - 9,
				Const.GREEN, Const.GREY_32
			) && !FrameTask.HasTask()) {
				if (Project.UserProjects.Any((p) => p.Info.ProjectName == CreatingNewProjectName)) {
					// Name Already Exists
					CreatingNewProjectErrorMessage = NEW_PROJECT_ERROR_EXISTS.Get("Name already exists");
				} else {
					// Create and Open
					Project.CreateProject(CreatingNewProjectName);
					CreatingNewProject = false;
					CreatingNewProjectName = string.Empty;
				}
			}

			// Cancel
			if (CellRendererGUI.Button(
				rect.EdgeInside(Direction4.Right, rect.width / 2).Shrink(buttonShrink / 2, buttonShrink / 2, buttonShrink, buttonShrink),
				BuiltInIcon.UI_BUTTON,
				UI_CANCEL.Get("Cancel"), out _,
				z: int.MaxValue - 9,
				Const.WHITE, Const.GREY_32
			)) {
				CreatingNewProject = false;
				CreatingNewProjectName = string.Empty;
			}

			// Final
			panelRect.height = panelRect.y - rect.y + padding;
			panelRect.y = rect.y - padding;
		}


		private void DrawProjectSelectorPanel (ref IRect panelRect) {

			int itemHeight = CellRendererGUI.Unify(48);
			int itemShrink = CellRendererGUI.Unify(10);
			int padding = 0;

			var rect = new IRect(panelRect.x, panelRect.y, panelRect.width, itemHeight);

			// Message
			if (Project.UserProjects.Count == 0) {
				int msgHeight = CellRendererGUI.Unify(48);
				rect.y -= msgHeight + padding;
				CellRendererGUI.Label(CellContent.Get(
					PROJECT_SELECTOR_MSG.Get(NEW_PROJECT_DEFAULT_MSG),
					tint: Const.GREY_128, charSize: 16, alignment: Alignment.TopMid, true
				), rect.Shrink(itemShrink, itemShrink, 0, 0), out var msgBounds);
				rect.y = msgBounds.y - padding;
			}

			// New Project Button
			rect.y -= itemHeight + padding;
			var buttonRect = rect.Shrink(itemShrink, itemShrink, itemShrink / 2, itemShrink / 2);
			if (CellRendererGUI.Button(
				buttonRect, BuiltInIcon.FRAME_16, CREATE_PROJECT.Get("Create Project"), out _,
				z: int.MaxValue - 9, Const.GREY_196, Const.GREY_196
			)) {
				CreatingNewProject = true;
				CreatingNewProjectErrorMessage = "";
				CreatingNewProjectName = NEW_PROJECT_NAME.Get("New Project");
			}

			// Built In Project
			rect.y -= itemHeight + padding;
			buttonRect = rect.Shrink(itemShrink, itemShrink, itemShrink / 2, itemShrink / 2);
			if (CellRendererGUI.Button(
				buttonRect,
				BuiltInIcon.UI_BUTTON,
				BUILT_IN_PROJECT.Get("Default Level"), out _,
				z: int.MaxValue - 9, Const.WHITE, Const.GREY_32,
				enable: !Project.OpeningBuiltInProject
			)) {
				ChangeProject(Project.BuiltInProject);
			}

			// User Projects
			int startCellIndex = CellRenderer.GetUsedCellCount();
			int startTextCellIndex = CellRenderer.GetTextUsedCellCount();
			int pageHeight = rect.y - CellRenderer.CameraRect.y;
			int pageItemCount = pageHeight / (itemHeight + padding);
			int clampMaxY = rect.y;

			// Scroll
			if (Project.UserProjects.Count > pageItemCount) {
				int scrollBarWidth = CellRendererGUI.Unify(24);
				rect.width -= scrollBarWidth;
				ProjectSelectorScrollIndex -= FrameInput.MouseWheelDelta * 2;
				ProjectSelectorScrollIndex = ProjectSelectorScrollIndex.Clamp(0, Project.UserProjects.Count - pageItemCount + 6);
				ProjectSelectorScrollIndex = CellRendererGUI.ScrollBar(
					new IRect(rect.xMax, rect.y, scrollBarWidth, pageHeight),
					z: int.MaxValue - 3,
					ProjectSelectorScrollIndex, Project.UserProjects.Count + 6, pageItemCount
				);
			} else {
				ProjectSelectorScrollIndex = 0;
			}

			// Open Project Button
			for (int i = ProjectSelectorScrollIndex; i < Project.UserProjects.Count; i++) {
				rect.y -= itemHeight + padding;
				if (rect.yMax < CellRenderer.CameraRect.y) break;
				var project = Project.UserProjects[i];
				bool isCurrent = project == Project.CurrentProject;
				if (CellRendererGUI.Button(
					rect.Shrink(itemShrink, itemShrink, itemShrink / 2, itemShrink / 2),
					BuiltInIcon.UI_BUTTON,
					project.Info.ProjectName, out _,
					z: int.MaxValue - 4,
					Const.WHITE, Const.GREY_32,
					enable: !isCurrent
				) && !FrameTask.HasTask() && !isCurrent) {
					ChangeProject(project);
				}
			}

			// Final
			panelRect.height = panelRect.y - rect.y + padding;
			panelRect.y = rect.y - padding - CellRendererGUI.Unify(10);
			if (Project.UserProjects.Count > pageItemCount) {
				var clampRect = new IRect(panelRect.x, panelRect.y, panelRect.width, clampMaxY - panelRect.y);
				CellRenderer.ClampCells(clampRect, startCellIndex);
				CellRenderer.ClampTextCells(clampRect, startTextCellIndex);
			}
		}


		#endregion




		#region --- LGC ---


		private static void ChangeProject (Project project) {
			FrameTask.EndAllTask();
			OpenProjectCallback += () => Project.OpenProject(project);
			FrameTask.AddToLast(FadeOutTask.TYPE_ID, 50);
			if (GlobalEditorUI.HasActiveInstance) {
				FrameTask.AddToLast(DespawnEntityTask.TYPE_ID, GlobalEditorUI.Instance);
			}
			FrameTask.AddToLast(MethodTask.TYPE_ID, OpenProjectCallback);
			FrameTask.AddToLast(RestartGameTask.TYPE_ID);
			FrameTask.AddToLast(DelayTask.TYPE_ID, 1);
			if (FrameTask.AddToLast(SpawnEntityTask.TYPE_ID) is SpawnEntityTask task) {
				task.EntityID = MapEditor.TYPE_ID;
				task.X = 0;
				task.Y = 0;
			}
		}


		#endregion




	}
}
