using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;

public class RiggedMapEditor : WindowUI {




	#region --- SUB ---


	private class BarData {
		public string Name;
		public int Value;
		public int Capacity;
	}


	private enum PanelType { None, Profiler, Effect, Movement, }


	private enum MovementTabType {
		Move, Push, Jump, Roll, Dash, Rush,
		SlipCrash, Squat, Pound, Swim,
		Climb, Fly, Slide, Grab
	}


	#endregion




	#region --- VAR ---


	// Const
	private static readonly SpriteCode BTN_COLLIDER = "Engine.MapEditor.Collider";
	private static readonly SpriteCode BTN_ENTITY_CLICKER = "Engine.MapEditor.Entity";
	private static readonly SpriteCode BTN_PROFILER = "Engine.MapEditor.Profiler";
	private static readonly SpriteCode BTN_EFFECT = "Engine.MapEditor.Effect";
	private static readonly SpriteCode BTN_NEXT = "Engine.MapEditor.NextFrame";
	private static readonly SpriteCode BTN_PLAY = "Engine.MapEditor.Play";
	private static readonly SpriteCode BTN_PAUSE = "Engine.MapEditor.Pause";
	private static readonly SpriteCode BTN_MOVEMENT = "Engine.MapEditor.Movement";

	// Api
	public static RiggedMapEditor Instance { get; private set; }
	public override string DefaultName => "Map Editor";
	public bool DrawCollider { get; private set; } = false;
	public bool EntityClickerOn { get; private set; } = false;
	public IRect PanelRect { get; private set; }
	public bool FrameDebugging { get; private set; } = false;
	public bool RequireNextFrame { get; set; } = false;
	public bool HavingGamePlay { get; set; } = false;
	public bool RequireReloadPlayerMovement { get; set; } = false;

	// Data
	private readonly BarData[] RenderingUsages = new BarData[RenderLayer.COUNT];
	private readonly BarData[] EntityUsages = new BarData[EntityLayer.COUNT];
	private readonly bool[] EffectsEnabled = new bool[Const.SCREEN_EFFECT_COUNT].FillWithValue(false);
	private readonly LanguageCode[] MovementTabNames;
	private readonly IntToChars MovementTabLabelToChars;
	private readonly int MovementTabCount;
	private PanelType CurrentPanel = PanelType.None;
	private MovementTabType MovementTab = MovementTabType.Move;
	private bool IsMovementEditorDirty = false;


	#endregion




	#region --- MSG ---


	public RiggedMapEditor () {
		Instance = this;
		MovementTabCount = typeof(MovementTabType).EnumLength();
		MovementTabLabelToChars = new("  (", $"/{MovementTabCount})");
		MovementTabNames = new LanguageCode[MovementTabCount];
		for (int i = 0; i < MovementTabCount; i++) {
			MovementTabNames[i] = (
				$"UI.RigEditor.{(MovementTabType)i}",
				Util.GetDisplayName(((MovementTabType)i).ToString())
			);
		}
	}


	public override void UpdateWindowUI () {
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

		using var _ = new UILayerScope();

		var panelRect = new IRect(Renderer.CameraRect.xMax, Renderer.CameraRect.yMax, 0, 0);
		int panelYMax = panelRect.y;

		// BG
		var bgCell = Renderer.DrawPixel(default, Color32.BLACK);

		// Tool Buttons
		int buttonSize = GUI.Unify(28);
		int padding = GUI.Unify(6);
		panelRect.height = buttonSize + padding * 2;
		panelRect.y -= panelRect.height;

		var rect = new IRect(
			panelRect.xMax - buttonSize - padding,
			panelRect.y + padding,
			buttonSize, buttonSize
		);

		// Profiler Btn
		bool isOn = CurrentPanel == PanelType.Profiler;
		bool newIsOn = GUI.IconToggle(rect, isOn, BTN_PROFILER);
		if (isOn != newIsOn) {
			CurrentPanel = isOn ? PanelType.None : PanelType.Profiler;
		}
		rect.x -= rect.width + padding;

		// Effect Btn
		isOn = CurrentPanel == PanelType.Effect;
		newIsOn = GUI.IconToggle(rect, isOn, BTN_EFFECT);
		if (isOn != newIsOn) {
			CurrentPanel = isOn ? PanelType.None : PanelType.Effect;
		}
		rect.x -= rect.width + padding;

		if (HavingGamePlay) {

			// Movement
			isOn = CurrentPanel == PanelType.Movement;
			newIsOn = GUI.IconToggle(rect, isOn, BTN_MOVEMENT);
			if (isOn != newIsOn) {
				CurrentPanel = isOn ? PanelType.None : PanelType.Movement;
			}
			rect.SlideLeft(padding);

			// Collider
			DrawCollider = GUI.IconToggle(rect, DrawCollider, BTN_COLLIDER);
			rect.SlideLeft(padding);

			// Entity Clicker
			EntityClickerOn = GUI.IconToggle(rect, EntityClickerOn, BTN_ENTITY_CLICKER);
			rect.SlideLeft(padding);

			// Next Frame
			if (GUI.Button(rect, BTN_NEXT, Skin.IconButton)) {
				if (!FrameDebugging) {
					FrameDebugging = true;
				} else {
					RequireNextFrame = true;
				}
			}
			rect.x -= rect.width + padding;

			// Play/Pause
			if (GUI.Button(rect, FrameDebugging ? BTN_PLAY : BTN_PAUSE, Skin.IconButton)) {
				FrameDebugging = !FrameDebugging;
				RequireNextFrame = false;
			}
			rect.x -= rect.width + padding;

		} else {
			DrawCollider = false;
			EntityClickerOn = false;
			FrameDebugging = false;
			RequireNextFrame = false;
			if (CurrentPanel == PanelType.Movement) CurrentPanel = PanelType.None;
		}

		// Draw Panels
		panelRect.width = panelRect.x - rect.xMax;
		panelRect.x = rect.xMax;
		if (CurrentPanel != PanelType.None) {
			int minPanelWidth = Unify(196);
			if (panelRect.width < minPanelWidth) {
				panelRect.x = panelRect.xMax - minPanelWidth;
				panelRect.width = minPanelWidth;
			}
		}
		switch (CurrentPanel) {
			case PanelType.Profiler: DrawProfilerPanel(ref panelRect); break;
			case PanelType.Effect: DrawEffectPanel(ref panelRect); break;
			case PanelType.Movement: DrawMovementPanel(ref panelRect); break;
		}

		// Panel
		PanelRect = new IRect(panelRect.x, panelRect.y, panelRect.width, panelYMax - panelRect.y);
		bgCell.X = PanelRect.x;
		bgCell.Y = PanelRect.y;
		bgCell.Width = PanelRect.width;
		bgCell.Height = PanelRect.height;

	}


	// Panel
	private void DrawProfilerPanel (ref IRect panelRect) {

		// Init
		if (RenderingUsages[0] == null || EntityUsages[0] == null) return;

		// Draw
		int barHeight = GUI.Unify(24);
		panelRect.height = barHeight * (EntityUsages.Length + RenderingUsages.Length);
		panelRect.y -= panelRect.height;
		int barPadding = GUI.Unify(4);
		var rect = new IRect(panelRect.x, panelRect.yMax - barHeight, panelRect.width, barHeight);

		// Entity
		for (int i = 0; i < EntityUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), EntityUsages[i], Color32.CYAN);
			rect.y -= rect.height;
		}
		// Rendering
		for (int i = 0; i < RenderingUsages.Length; i++) {
			DrawBar(rect.Shrink(barPadding), RenderingUsages[i], Color32.GREEN);
			rect.y -= rect.height;
		}

		// Func
		static void DrawBar (IRect rect, BarData data, Color32 barColor) {
			int width = Util.RemapUnclamped(0, data.Capacity, 0, rect.width, data.Value);
			Renderer.DrawPixel(new IRect(rect.x, rect.y, width, rect.height), barColor, int.MaxValue);
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


	private void DrawEffectPanel (ref IRect panelRect) {
		int itemHeight = GUI.Unify(28);
		int itemPadding = GUI.Unify(8);
		panelRect.height = Const.SCREEN_EFFECT_COUNT * (itemHeight + itemPadding) + itemPadding;
		panelRect.y -= panelRect.height;
		var rect = new IRect(panelRect.x + itemPadding, panelRect.yMax, panelRect.width - itemPadding * 2, itemHeight);
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {

			rect.y -= itemHeight + itemPadding;

			// Label
			GUI.SmallLabel(rect, Const.SCREEN_EFFECT_NAMES[i]);

			// Toggle
			var enableRect = rect.Edge(Direction4.Right, rect.height);
			EffectsEnabled[i] = GUI.Toggle(enableRect, EffectsEnabled[i]);
		}

		// Update Values
		for (int i = 0; i < Const.SCREEN_EFFECT_COUNT; i++) {
			if (EffectsEnabled[i]) Game.PassEffect(i);
		}
		if (EffectsEnabled[Const.SCREEN_EFFECT_RETRO_DARKEN]) {
			Game.PassEffect_RetroDarken(Game.GlobalFrame.PingPong(60) / 60f);
		}
		if (EffectsEnabled[Const.SCREEN_EFFECT_RETRO_LIGHTEN]) {
			Game.PassEffect_RetroLighten(Game.GlobalFrame.PingPong(60) / 60f);
		}
		if (EffectsEnabled[Const.SCREEN_EFFECT_TINT]) {
			Game.PassEffect_Tint(Color32.LerpUnclamped(new Color32(255, 128, 196, 255), new Color32(128, 255, 64, 255), Game.GlobalFrame.PingPong(120) / 120f));
		}
		if (EffectsEnabled[Const.SCREEN_EFFECT_VIGNETTE]) {
			Game.PassEffect_Vignette(0.95f, 0.6f, 0f, 0f, 0f);
		}
	}


	private void DrawMovementPanel (ref IRect panelRect) {

		int padding = Unify(6);
		int toolbarSize = Unify(28);
		int top = panelRect.y;
		var rect = new IRect(panelRect.x, panelRect.y - toolbarSize, panelRect.width, toolbarSize);

		// Tab Bar
		using (new GUIContentColorScope(Color32.GREY_196)) {
			if (GUI.Button(rect.EdgeLeft(rect.height), BuiltInSprite.ICON_TRIANGLE_LEFT, Skin.SmallIconButton)) {
				MovementTab = (MovementTabType)(((int)MovementTab) - 1).Clamp(0, MovementTabCount - 1);
			}
			if (GUI.Button(rect.EdgeRight(rect.height), BuiltInSprite.ICON_TRIANGLE_RIGHT, Skin.SmallIconButton)) {
				MovementTab = (MovementTabType)(((int)MovementTab) + 1).Clamp(0, MovementTabCount - 1);
			}
		}

		// Name Label
		GUI.Label(rect.ShrinkRight(rect.height), MovementTabNames[(int)MovementTab], out var bounds, Skin.SmallCenterLabel);

		// Number Label
		GUI.Label(
			bounds.EdgeRight(1),
			MovementTabLabelToChars.GetChars(((int)MovementTab) + 1),
			Skin.SmallGreyLabel
		);
		rect.SlideDown(padding);

		// Props
		switch (MovementTab) {
			case MovementTabType.Move:
				break;
			case MovementTabType.Push:
				break;
			case MovementTabType.Jump:
				break;
			case MovementTabType.Roll:
				break;
			case MovementTabType.Dash:
				break;
			case MovementTabType.Rush:
				break;
			case MovementTabType.SlipCrash:
				break;
			case MovementTabType.Squat:
				break;
			case MovementTabType.Pound:
				break;
			case MovementTabType.Swim:
				break;
			case MovementTabType.Climb:
				break;
			case MovementTabType.Fly:
				break;
			case MovementTabType.Slide:
				break;
			case MovementTabType.Grab:
				break;
		}

		// Apply Button
		rect.yMin = rect.yMax - GUI.FieldHeight;
		using (new GUIEnableScope(IsMovementEditorDirty))
		using (new GUIBodyColorScope(IsMovementEditorDirty ? Color32.GREEN_BETTER : Color32.WHITE)) {
			if (GUI.Button(rect.Shrink(rect.width / 6, rect.width / 6, 0, 0), BuiltInText.UI_APPLY, Skin.SmallDarkButton)) {
				RequireReloadPlayerMovement = true;
				IsMovementEditorDirty = false;
			}
		}
		rect.SlideDown(padding);

		// Final
		panelRect.height = top - rect.yMax;
		panelRect.y -= panelRect.height;
	}


	#endregion




	#region --- API ---


	public void UpdateUsageData (int[] renderUsages, int[] renderCapacities, int[] entityUsages, int[] entityCapacities) {
		if (RenderingUsages[0] == null) {
			for (int i = 0; i < RenderLayer.COUNT; i++) {
				RenderingUsages[i] = new BarData() {
					Name = RenderLayer.NAMES[i],
				};
			}
		}
		if (EntityUsages[0] == null) {
			for (int i = 0; i < EntityLayer.COUNT; i++) {
				EntityUsages[i] = new BarData() {
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