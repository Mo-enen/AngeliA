using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using AngeliA;
using System.Linq;

namespace AngeliaEngine;

public class GameEditor : WindowUI {




	#region --- SUB ---


	private class ProfilerUiBarData {
		public string Name;
		public int Value;
		public int Capacity;
	}


	private enum PanelType { None, Profiler, Movement, Lighting, }


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
	private static readonly SpriteCode TOOLBAR_BG = "Engine.Game.Toolbar";
	private static readonly SpriteCode PANEL_BG = "Engine.Game.PanelBG";

	private static readonly LanguageCode TIP_PROFILER = ("Engine.Game.Tip.Profiler", "Profiler");
	private static readonly LanguageCode TIP_FRAME_DEBUG = ("Engine.Game.Tip.FrameDebug", "Frame Debugger");
	private static readonly LanguageCode TIP_NEXT_FRAME = ("Engine.Game.Tip.NextFrame", "Next Frame");
	private static readonly LanguageCode TIP_ENTITY_CLICER = ("Engine.Game.Tip.EntityClicker", "Entity Debugger");
	private static readonly LanguageCode TIP_COLLIDER = ("Engine.Game.Tip.Collider", "Collider");
	private static readonly LanguageCode TIP_LIGHTING = ("Engine.Game.Tip.Lighting", "Lighting System");
	private static readonly LanguageCode TIP_MOVEMENT = ("Engine.Game.Tip.Movement", "Movement System");
	private static readonly LanguageCode LABEL_DAYTIME = ("UI.RigEditor.Daytime", "In-Game Daytime");
	private static readonly LanguageCode LABEL_PIXEL_STYLE = ("UI.RigEditor.PixelStyle", "Use Pixel Style");
	private static readonly LanguageCode LABEL_SELF_LERP = ("UI.RigEditor.SelfLerp", "Self Lerp");
	private static readonly LanguageCode LABEL_SOLID_ILLU = ("UI.RigEditor.SolidIllu", "Solid Illuminance");
	private static readonly LanguageCode LABEL_AIR_ILLU_DAY = ("UI.RigEditor.AirIlluDay", "Air Illuminance (Day)");
	private static readonly LanguageCode LABEL_AIR_ILLU_NIGHT = ("UI.RigEditor.AirIlluNight", "Air Illuminance (Night)");
	private static readonly LanguageCode LABEL_BG_TINT = ("UI.RigEditor.BgTint", "Background Tint");
	private static readonly LanguageCode LABEL_LV_REMAIN = ("UI.RigEditor.LvRemain", "Solid Illuminate Remain");

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
	public int ToolbarLeftWidth => Unify(40);
	public bool LightMapSettingChanged { get; set; } = false;
	public float ForcingInGameDaytime { get; private set; } = -1f;

	// Data
	private static readonly Cell[] CacheForPanelSlice = new Cell[9];
	private readonly ProfilerUiBarData[] RenderingUsages = new ProfilerUiBarData[RenderLayer.COUNT];
	private readonly ProfilerUiBarData[] EntityUsages = new ProfilerUiBarData[EntityLayer.COUNT];
	private Project CurrentProject = null;
	private PanelType CurrentPanel = PanelType.None;


	#endregion




	#region --- MSG ---


	public GameEditor () => Instance = this;


	public override void UpdateWindowUI () {
		if (CurrentProject == null) return;
		OnGUI_Hotkey();
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
			if (Input.KeyboardDown(KeyboardKey.Escape)) {
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
			CurrentPanel = CurrentPanel != PanelType.Profiler ? PanelType.None : CurrentPanel;
		}

		using var _ = new UILayerScope();

		int padding = Unify(6);
		int buttonSize = ToolbarLeftWidth - padding * 2;
		var barRect = ToolbarRect = WindowRect.EdgeLeft(buttonSize + padding * 2);

		// Draw Panels
		if (CurrentPanel != PanelType.None) {

			var panelRect = barRect.CornerOutside(Alignment.TopRight, Unify(220), Unify(384));

			// Draw Panel BG
			var cells = GUI.DrawSlice(PANEL_BG, panelRect);
			bool bgPainted = cells != null;
			cells?.CopyTo(CacheForPanelSlice, 0);

			// Draw Panel
			switch (CurrentPanel) {
				case PanelType.Profiler: DrawProfilerPanel(ref panelRect); break;
				case PanelType.Movement: break;
				case PanelType.Lighting: DrawLightingPanel(ref panelRect); break;
			}

			if (bgPainted) {
				using var __ = new GUIColorScope(Color32.CLEAR);
				var placeHolders = GUI.DrawSlice(PANEL_BG, panelRect);
				if (placeHolders != null) {
					for (int i = 0; i < 9; i++) {
						var cacheCell = CacheForPanelSlice[i];
						cacheCell.CopyFrom(placeHolders[i]);
						cacheCell.Color = Color32.WHITE;
					}
				} else {
					for (int i = 0; i < 9; i++) {
						CacheForPanelSlice[i].Color = Color32.CLEAR;
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
		GUI.DrawSlice(TOOLBAR_BG, barRect);

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
			GUI.BackgroundLabel(rect.EdgeLeft(1), TIP_PROFILER, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
		}
		rect.SlideDown(padding);

		if (HavingGamePlay) {
			// Movement
			isOn = CurrentPanel == PanelType.Movement;
			newIsOn = GUI.IconToggle(rect, isOn, BTN_MOVEMENT);
			if (isOn != newIsOn) {
				CurrentPanel = newIsOn ? PanelType.Movement : PanelType.None;
				RequireOpenOrCloseMovementPanel = newIsOn;
			}
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeLeft(1), TIP_MOVEMENT, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
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
					GUI.BackgroundLabel(rect.EdgeLeft(1), TIP_LIGHTING, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
				}
				rect.SlideDown(padding);
			}

			// Collider
			DrawCollider = GUI.IconToggle(rect, DrawCollider, BTN_COLLIDER);
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeLeft(1), TIP_COLLIDER, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
			}
			rect.SlideDown(padding);

			// Entity Clicker
			EntityClickerOn = GUI.IconToggle(rect, EntityClickerOn, BTN_ENTITY_CLICKER);
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeLeft(1), TIP_ENTITY_CLICER, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
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
					GUI.BackgroundLabel(rect.EdgeLeft(1), TIP_NEXT_FRAME, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
				}
				rect.SlideDown(padding);
			}

			// Play/Pause
			if (GUI.Button(rect, FrameDebugging ? BTN_PLAY : BTN_PAUSE, Skin.IconButton)) {
				FrameDebugging = !FrameDebugging;
				RequireNextFrame = false;
			}
			if (rect.MouseInside()) {
				GUI.BackgroundLabel(rect.EdgeLeft(1), FrameDebugging ? BuiltInText.UI_CONTINUE : TIP_FRAME_DEBUG, Color32.GREY_20, padding, style: GUI.Skin.SmallRightLabel);
			}
			rect.SlideDown(padding);

			if (!CurrentProject.Universe.Info.UseLightingSystem && CurrentPanel == PanelType.Lighting) CurrentPanel = PanelType.None;
		}

	}


	// Panel
	private void DrawProfilerPanel (ref IRect panelRect) {

		// Init
		if (RenderingUsages[0] == null || EntityUsages[0] == null) return;

		// Draw
		int barHeight = GUI.Unify(24);
		panelRect.height = barHeight * (EntityUsages.Length + RenderingUsages.Length + 2);
		panelRect.y -= panelRect.height;
		int barPadding = GUI.Unify(4);
		var rect = new IRect(panelRect.x, panelRect.yMax - barHeight, panelRect.width, barHeight);

		// Entity
		GUI.Label(rect, "Entity", GUI.Skin.SmallGreyLabel);
		rect.SlideDown();
		for (int i = 0; i < EntityUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), EntityUsages[i], Color32.CYAN);
			rect.SlideDown();
		}

		// Rendering
		GUI.Label(rect, "Rendering", GUI.Skin.SmallGreyLabel);
		rect.SlideDown();
		for (int i = 0; i < RenderingUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), RenderingUsages[i], Color32.GREEN);
			rect.SlideDown();
		}

		// Func
		static void DrawBar (IRect rect, ProfilerUiBarData data, Color32 barColor) {
			int width = Util.RemapUnclamped(0, data.Capacity, 0, rect.width, data.Value);
			Renderer.DrawPixel(new IRect(rect.x, rect.y, width, rect.height), data.Value < data.Capacity ? barColor : Color32.RED, int.MaxValue);
			// Label
			int padding = Unify(6);
			GUI.Label(rect.ShrinkLeft(padding), data.Name, GUI.Skin.SmallLabel);
			using (new GUIColorScope(new Color32(96, 96, 96, 255))) {
				GUI.IntLabel(rect.ShrinkRight(padding), data.Capacity, out var bounds, GUI.Skin.SmallRightLabel);
				GUI.Label(bounds.EdgeOutside(Direction4.Left, rect.width).ShrinkRight(padding), "/", out bounds, GUI.Skin.SmallRightLabel);
				GUI.IntLabel(bounds.EdgeOutside(Direction4.Left, rect.width).ShrinkRight(padding), data.Value, GUI.Skin.SmallRightLabel);
			}
		}
	}


	private void DrawLightingPanel (ref IRect panelRect) {

		// Min Width
		int minWidth = Unify(396);
		if (panelRect.width < minWidth) {
			panelRect.width = minWidth;
		}

		// Content
		int panelPadding = Unify(12);
		int padding = GUI.FieldPadding;
		int toolbarSize = Unify(28);
		int top = panelRect.y;
		int digitWidth = GUI.Unify(64);
		var rect = new IRect(panelRect.x, panelRect.y - toolbarSize, panelRect.width, toolbarSize);
		rect = rect.Shrink(panelPadding, panelPadding, 0, 0);
		var info = CurrentProject.Universe.Info;
		GUI.BeginChangeCheck();

		// Daytime
		GUI.SmallLabel(rect, LABEL_DAYTIME);
		int newDaytime = GUI.HandleSlider(
			8126534, rect.ShrinkLeft(GUI.LabelWidth),
			(int)(ForcingInGameDaytime * 1200), 0, 1200, step: 100
		);
		if (newDaytime != 0) {
			if (newDaytime != (int)(ForcingInGameDaytime * 1200f)) {
				ForcingInGameDaytime = newDaytime / 1200f;
			}
		}
		rect.SlideDown(padding);

		// Pixel Style
		info.LightMap_PixelStyle = GUI.Toggle(
			rect, info.LightMap_PixelStyle, LABEL_PIXEL_STYLE,
			labelStyle: GUI.Skin.SmallLabel
		);
		rect.SlideDown(padding);

		// Self Lerp
		GUI.SmallLabel(rect, LABEL_SELF_LERP);
		info.LightMap_SelfLerp = GUI.HandleSlider(
			37423672, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_SelfLerp * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_SelfLerp * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Solid Illuminance
		GUI.SmallLabel(rect, LABEL_SOLID_ILLU);
		info.LightMap_SolidIlluminance = GUI.HandleSlider(
			37423673, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_SolidIlluminance * 1000), 1000, 2000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_SolidIlluminance * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Air Illuminance D
		GUI.SmallLabel(rect, LABEL_AIR_ILLU_DAY);
		info.LightMap_AirIlluminanceDay = GUI.HandleSlider(
			37423674, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_AirIlluminanceDay * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_AirIlluminanceDay * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Air Illuminance N
		GUI.SmallLabel(rect, LABEL_AIR_ILLU_NIGHT);
		info.LightMap_AirIlluminanceNight = GUI.HandleSlider(
			37413674, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_AirIlluminanceNight * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_AirIlluminanceNight * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Background Tint
		GUI.SmallLabel(rect, LABEL_BG_TINT);
		info.LightMap_BackgroundTint = GUI.HandleSlider(
			37423675, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_BackgroundTint * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_BackgroundTint * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Level Illu Remain
		GUI.SmallLabel(rect, LABEL_LV_REMAIN);
		info.LightMap_LevelIlluminateRemain = GUI.HandleSlider(
			37423676, rect.Shrink(GUI.LabelWidth, digitWidth, 0, 0),
			(int)(info.LightMap_LevelIlluminateRemain * 1000), 0, 1000
		) / 1000f;
		GUI.IntLabel(rect.EdgeRight(digitWidth), (int)(info.LightMap_LevelIlluminateRemain * 1000), GUI.Skin.SmallCenterLabel);
		rect.SlideDown(padding);

		// Reset
		rect.SlideDown();
		if (GUI.DarkButton(rect, BuiltInText.UI_RESET)) {
			info.LightMap_PixelStyle = false;
			info.LightMap_SelfLerp = 0.88f;
			info.LightMap_SolidIlluminance = 1f;
			info.LightMap_AirIlluminanceDay = 0.95f;
			info.LightMap_AirIlluminanceNight = 0.3f;
			info.LightMap_BackgroundTint = 0.5f;
			info.LightMap_LevelIlluminateRemain = 0.3f;
			LightMapSettingChanged = true;
			ForcingInGameDaytime = -1f;
		}
		rect.SlideDown(padding);

		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;
		if (GUI.EndChangeCheck()) {
			LightMapSettingChanged = true;
		}

	}


	#endregion




	#region --- API ---


	public void SetCurrentProject (Project currentProject) => CurrentProject = currentProject;


	public void UpdateUsageData (int[] renderUsages, int[] renderCapacities, int[] entityUsages, int[] entityCapacities) {
		if (RenderingUsages[0] == null) {
			for (int i = 0; i < RenderLayer.COUNT; i++) {
				RenderingUsages[i] = new ProfilerUiBarData() {
					Name = RenderLayer.NAMES[i],
				};
			}
		}
		if (EntityUsages[0] == null) {
			for (int i = 0; i < EntityLayer.COUNT; i++) {
				EntityUsages[i] = new ProfilerUiBarData() {
					Name = EntityLayer.LAYER_NAMES[i],
				};
			}
		}
		for (int i = 0; i < RenderLayer.COUNT; i++) {
			RenderingUsages[i].Value = renderUsages[i];
			RenderingUsages[i].Capacity = renderCapacities[i];
		}
		for (int i = 0; i < EntityLayer.COUNT; i++) {
			EntityUsages[i].Value = entityUsages[i];
			EntityUsages[i].Capacity = entityCapacities[i];
		}
	}


	#endregion




}
