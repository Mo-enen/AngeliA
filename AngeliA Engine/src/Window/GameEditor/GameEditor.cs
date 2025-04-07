using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

internal partial class GameEditor : WindowUI {




	#region --- SUB ---


	private class ProfilerUiBarData {
		public string Name;
		public int Value;
		public int Capacity;
	}


	private enum PanelType { None, Profiler, Movement, Lighting, Location, }


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode BTN_COLLIDER = "Engine.Game.Collider";
	private static readonly SpriteCode BTN_ENTITY_CLICKER = "Engine.Game.Entity";
	private static readonly SpriteCode BTN_PROFILER = "Engine.Game.Profiler";
	private static readonly SpriteCode BTN_NEXT = "Engine.Game.NextFrame";
	private static readonly SpriteCode BTN_PLAY = "Engine.Game.Play";
	private static readonly SpriteCode BTN_PAUSE = "Engine.Game.Pause";
	private static readonly SpriteCode BTN_MOVEMENT = "Engine.Game.Movement";
	private static readonly SpriteCode BTN_LIGHTING = "Engine.Game.Lighting";
	private static readonly SpriteCode BTN_LOCATION = "Engine.Game.Location";
	private static readonly SpriteCode TOOLBAR_BG = "Engine.Game.Toolbar";
	private static readonly SpriteCode PANEL_BG = "Engine.Game.PanelBG";

	private static readonly LanguageCode TIP_PROFILER = ("Engine.Game.Tip.Profiler", "Profiler");
	private static readonly LanguageCode TIP_FRAME_DEBUG = ("Engine.Game.Tip.FrameDebug", "Frame Debugger");
	private static readonly LanguageCode TIP_NEXT_FRAME = ("Engine.Game.Tip.NextFrame", "Next Frame");
	private static readonly LanguageCode TIP_ENTITY_CLICER = ("Engine.Game.Tip.EntityClicker", "Entity Debugger");
	private static readonly LanguageCode TIP_COLLIDER = ("Engine.Game.Tip.Collider", "Collider");
	private static readonly LanguageCode TIP_LIGHTING = ("Engine.Game.Tip.Lighting", "Lighting System");
	private static readonly LanguageCode TIP_LOCATION = ("Engine.Game.Tip.Location", "View Positions");
	private static readonly LanguageCode TIP_MOVEMENT = ("Engine.Game.Tip.Movement", "Movement System");

	private static readonly IntToChars FrameCountToChars = new("Frame: ");

	// Api
	public static GameEditor Instance { get; private set; }
	public override string DefaultWindowName => "Game";
	public bool DrawCollider { get; private set; } = false;
	public bool EntityClickerOn { get; private set; } = false;
	public IRect PanelRect { get; private set; }
	public IRect ToolbarRect { get; private set; }
	public bool FrameDebugging { get; private set; } = false;
	public bool RequireNextFrame { get; set; } = false;
	public bool HavingGamePlay { get; set; } = false;
	public bool? RequireOpenOrCloseMovementPanel { get; set; } = null;
	public int ToolbarWidth => Unify(40);
	public bool IgnoreInGameGizmos => CurrentPanel == PanelType.Location && HavingGamePlay;

	// Data
	private static readonly Cell[] CacheForPanelSlice = new Cell[9];
	private Project CurrentProject = null;
	private PanelType CurrentPanel = PanelType.None;
	private int RiggedGameGlobalFrame;


	#endregion




	#region --- MSG ---


	public GameEditor () => Instance = this;


	public override void FirstUpdate () {
		base.FirstUpdate();
		Cursor.RequireCursor();
	}


	public override void UpdateWindowUI () {
		if (CurrentProject == null) return;
		OnGUI_Hotkey();
		OnGUI_FrameDebuggingUI();
		OnGUI_Toolbar();
	}


	private void OnGUI_Hotkey () {

		// Next Frame
		if (EngineSetting.Hotkey_FrameDebug_Next.Value.DownGUI()) {
			FrameDebugging = true;
			RequireNextFrame = true;
		}

		if (FrameDebugging) {
			// Cancel Frame Debugging
			if (EngineSetting.Hotkey_FrameDebug_End.Value.Down() || Input.KeyboardDown(KeyboardKey.Escape)) {
				FrameDebugging = false;
			}
		}

	}


	private void OnGUI_Toolbar () {

		if (!HavingGamePlay) {
			DrawCollider = false;
			EntityClickerOn = false;
			FrameDebugging = false;
			RequireNextFrame = false;
			switch (CurrentPanel) {
				case PanelType.Movement:
					RequireOpenOrCloseMovementPanel = false;
					CurrentPanel = PanelType.None;
					break;
				case PanelType.Lighting:
					CurrentPanel = PanelType.None;
					break;
			}
		}

		using var _ = new UILayerScope();

		int padding = Unify(6);
		int buttonSize = ToolbarWidth - padding * 2;
		var barRect = ToolbarRect = WindowRect.EdgeInsideRight(buttonSize + padding * 2);
		var oldPanel = CurrentPanel;
		bool allowLocationPanel = !HavingGamePlay || !CurrentProject.Universe.Info.UseProceduralMap;
		if (!allowLocationPanel && CurrentPanel == PanelType.Location) {
			CurrentPanel = PanelType.None;
		}

		// Draw Panels
		if (CurrentPanel != PanelType.None) {

			var panelRect = barRect.CornerOutside(Alignment.TopLeft, Unify(220), Unify(384));

			// Draw Panel BG
			Renderer.SetLayer(!Game.ShowingDoodle && CurrentPanel == PanelType.Location ? RenderLayer.DEFAULT : RenderLayer.UI);
			var cells = GUI.DrawSlice(PANEL_BG, panelRect);
			Renderer.SetLayer(RenderLayer.UI);
			bool bgPainted = cells != null;
			cells?.CopyTo(CacheForPanelSlice, 0);

			// Draw Panel
			switch (CurrentPanel) {
				case PanelType.Profiler: DrawProfilerPanel(ref panelRect); break;
				case PanelType.Movement: break;
				case PanelType.Lighting: DrawLightingPanel(ref panelRect); break;
				case PanelType.Location: DrawLocationPanel(ref panelRect); break;
			}

			if (bgPainted) {
				using var __ = new GUIColorScope(Color32.CLEAR);
				var placeHolders = GUI.DrawSlice(PANEL_BG, panelRect);
				if (placeHolders != null) {
					for (int i = 0; i < 9; i++) {
						var cacheCell = CacheForPanelSlice[i];
						cacheCell.CopyFrom(placeHolders[i]);
						cacheCell.Color = Color32.WHITE;
						if (CurrentPanel == PanelType.Location) {
							cacheCell.Z = int.MaxValue;
						}
					}
				} else {
					for (int i = 0; i < 9; i++) {
						var cacheCell = CacheForPanelSlice[i];
						cacheCell.Color = Color32.CLEAR;
						if (CurrentPanel == PanelType.Location) {
							cacheCell.Z = int.MaxValue;
						}
					}
				}
			}

			PanelRect = panelRect;

			if (panelRect.MouseInside()) {
				Cursor.SetCursorAsNormal(4096);
			}

		} else {
			PanelRect = default;
		}

		// Toolbar BG
		GUI.DrawSlice(TOOLBAR_BG, barRect.Expand(Unify(1), 0, 0, 0));

		// Toolbar Buttons
		var rect = new IRect(
			barRect.x + padding,
			barRect.yMax - padding - buttonSize,
			buttonSize, buttonSize
		);

		// Profiler Btn
		bool isOn = CurrentPanel == PanelType.Profiler;
		bool newIsOn = GUI.IconToggle(rect, isOn, BTN_PROFILER);
		if (isOn != newIsOn) {
			CurrentPanel = newIsOn ? PanelType.Profiler : PanelType.None;
		}
		if (rect.MouseInside()) {
			GUI.BackgroundLabel(rect.EdgeInsideLeft(1).Shift(-padding, 0), TIP_PROFILER, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
		}
		rect.SlideDown(padding);

		// Location
		if (allowLocationPanel) {
			isOn = CurrentPanel == PanelType.Location;
			newIsOn = GUI.IconToggle(rect, isOn, BTN_LOCATION);
			if (isOn != newIsOn) {
				CurrentPanel = newIsOn ? PanelType.Location : PanelType.None;
			}
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeInsideLeft(1).Shift(-padding, 0), TIP_LOCATION, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
			}
			rect.SlideDown(padding);
		}

		if (HavingGamePlay) {
			// Movement
			isOn = CurrentPanel == PanelType.Movement;
			newIsOn = GUI.IconToggle(rect, isOn, BTN_MOVEMENT);
			if (isOn != newIsOn) {
				CurrentPanel = newIsOn ? PanelType.Movement : PanelType.None;
			}
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeInsideLeft(1).Shift(-padding, 0), TIP_MOVEMENT, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
			}
			rect.SlideDown(padding);

			// Lighting
			if (CurrentProject.Universe.Info.UseLightingSystem) {
				isOn = CurrentPanel == PanelType.Lighting;
				newIsOn = GUI.IconToggle(rect, isOn, BTN_LIGHTING);
				if (isOn != newIsOn) {
					CurrentPanel = newIsOn ? PanelType.Lighting : PanelType.None;
				}
				if (rect.MouseInside()) {
					GUI.BackgroundLabel(rect.EdgeInsideLeft(1).Shift(-padding, 0), TIP_LIGHTING, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
				}
				rect.SlideDown(padding);
			}

			// Collider
			DrawCollider = GUI.IconToggle(rect, DrawCollider, BTN_COLLIDER);
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeInsideLeft(1).Shift(-padding, 0), TIP_COLLIDER, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
			}
			rect.SlideDown(padding);

			// Entity Clicker
			EntityClickerOn = GUI.IconToggle(rect, EntityClickerOn, BTN_ENTITY_CLICKER);
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeInsideLeft(1).Shift(-padding, 0), TIP_ENTITY_CLICER, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
			}
			rect.SlideDown(padding);

			// Next Frame
			if (FrameDebugging) {
				if (GUI.Button(rect, BTN_NEXT, Skin.IconButton)) {
					if (!FrameDebugging) {
						FrameDebugging = true;
					} else {
						RequireNextFrame = true;
					}
				}
				if (rect.MouseInside()) {
					GUI.BackgroundLabel(rect.EdgeInsideLeft(1).Shift(-padding, 0), TIP_NEXT_FRAME, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
				}
				rect.SlideDown(padding);
			}

			// Play/Pause
			if (GUI.Button(rect, FrameDebugging ? BTN_PLAY : BTN_PAUSE, Skin.IconButton)) {
				FrameDebugging = !FrameDebugging;
				RequireNextFrame = false;
			}
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeInsideLeft(1).Shift(-padding, 0), FrameDebugging ? BuiltInText.UI_CONTINUE : TIP_FRAME_DEBUG, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
			}
			rect.SlideDown(padding);

			if (!CurrentProject.Universe.Info.UseLightingSystem && CurrentPanel == PanelType.Lighting) CurrentPanel = PanelType.None;
		}

		// Require Open/Close Movement 
		if ((oldPanel == PanelType.Movement) != (CurrentPanel == PanelType.Movement)) {
			RequireOpenOrCloseMovementPanel = CurrentPanel == PanelType.Movement;
		}

		// Click Outside to Close Panel
		if (CurrentPanel == PanelType.Location && Input.MouseLeftButtonDown && !PanelRect.MouseInside() && !ToolbarRect.MouseInside()) {
			CurrentPanel = PanelType.None;
		}

	}


	private void OnGUI_FrameDebuggingUI () {

		if (!FrameDebugging || !HavingGamePlay) return;
		if (!EngineSetting.ShowKeyPressWhenFrameDebugging.Value) return;

		int btnSize = Unify(52);
		int padding = Unify(8);
		int iconPadding = Unify(16);
		int panelPadding = Unify(24);
		var panelRect = WindowRect.CornerInside(
			Alignment.TopMid,
			(btnSize + padding) * 12 + btnSize * 2,
			btnSize + panelPadding * 2
		).Shrink(panelPadding);
		var rect = panelRect.EdgeInsideSquareLeft();
		var iconRect = rect;

		// BG
		Renderer.DrawPixel(panelRect.Expand(panelPadding / 2), Color32.BLACK_32);

		// Key Press UI
		using (new GUIContentColorScope(Color32.GREY_32)) {

			// Up
			iconRect = DrawButton(rect, Input.GameKeyHolding(Gamekey.Up));
			GUI.Icon(iconRect.Shrink(iconPadding), BuiltInSprite.ICON_TRIANGLE_UP);
			rect.SlideRight(padding);

			// Down
			iconRect = DrawButton(rect, Input.GameKeyHolding(Gamekey.Down));
			GUI.Icon(iconRect.Shrink(iconPadding), BuiltInSprite.ICON_TRIANGLE_DOWN);
			rect.SlideRight(padding);

			// Left
			iconRect = DrawButton(rect, Input.GameKeyHolding(Gamekey.Left));
			GUI.Icon(iconRect.Shrink(iconPadding), BuiltInSprite.ICON_TRIANGLE_LEFT);
			rect.SlideRight(padding);

			// Right
			iconRect = DrawButton(rect, Input.GameKeyHolding(Gamekey.Right));
			GUI.Icon(iconRect.Shrink(iconPadding), BuiltInSprite.ICON_TRIANGLE_RIGHT);
			rect.SlideRight(padding);
			rect.SlideRight(padding);

			// Select
			iconRect = DrawButton(rect, Input.GameKeyHolding(Gamekey.Select));
			GUI.Label(iconRect, FrameworkUtil.GetGameKeyLabel(Gamekey.Select), Skin.SmallCenterLabel);
			rect.SlideRight(padding);
			rect.SlideRight(padding);

			// Action
			iconRect = DrawButton(rect, Input.GameKeyHolding(Gamekey.Action));
			GUI.Label(iconRect, FrameworkUtil.GetGameKeyLabel(Gamekey.Action), Skin.SmallCenterLabel);
			rect.SlideRight(padding);

			// Jump
			iconRect = DrawButton(rect, Input.GameKeyHolding(Gamekey.Jump));
			GUI.Label(iconRect, FrameworkUtil.GetGameKeyLabel(Gamekey.Jump), Skin.SmallCenterLabel);
			rect.SlideRight(padding);
			rect.SlideRight(padding);

			// Next Frame
			rect.width *= 2;
			iconRect = DrawButton(rect, EngineSetting.Hotkey_FrameDebug_Next.Value.Holding());
			GUI.Label(iconRect, TIP_NEXT_FRAME, Skin.SmallCenterLabel);
			rect.SlideRight(padding);
			rect.width /= 2;
		}

		// Frame Count UI
		rect.x += padding * 2;
		GUI.ShadowLabel(rect, FrameCountToChars.GetChars(RiggedGameGlobalFrame));

		static IRect DrawButton (IRect rect, bool press) {
			int border = Unify(6);
			var color = Color32.WHITE;
			if (press) {
				rect = rect.Shift(0, -Unify(6));
				color = Color32.GREY_160;
			}
			Renderer.DrawSlice(BuiltInSprite.HINT_BUTTON, rect, border, border, border, border, color);
			return rect;
		}
	}


	#endregion




	#region --- API ---


	public void SetCurrentProject (Project currentProject) {
		CurrentProject = currentProject;
		LocationSlotDirty = false;
		RequireSetViewPos = null;
		LocationThumbnailStream = currentProject != null ? new(currentProject.Universe.BuiltInMapRoot) : null;
		LoadLocationSlotsFromFile();
	}


	public void SetRiggedGameInfo (int riggedGameGlobalFrame) {
		RiggedGameGlobalFrame = riggedGameGlobalFrame;
	}


	#endregion




}
