using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Threading;
using AngeliA;
using AngeliaRaylib;

namespace AngeliaRigged;

public partial class RiggedGame : Game {




	#region --- VAR ---


	// Const
	private static readonly Color32[] COLLIDER_TINTS = { Color32.RED_BETTER, Color32.ORANGE_BETTER, Color32.YELLOW, Color32.GREEN, Color32.CYAN, Color32.BLUE, Color32.GREY_128, };

	// Api
	public readonly RigCallingMessage CallingMessage = new();
	public readonly RigRespondMessage RespondMessage = new();

	// Data
	private static RiggedGame Instance;
	private readonly Process HostProcess;
	private readonly string MapName = "RiggedGameMapName";
	private readonly int StartWithZ = 0;
	private readonly List<PhysicsCell[,,]> CellPhysicsCells = new();
	private unsafe byte* BufferPointer = null;
	private MemoryMappedFile MemMap = null;
	private MemoryMappedViewAccessor ViewAccessor = null;
	private IRect StartWithView = default;
	private bool DrawCollider = false;
	private bool EntityClickerOn = false;
	private Entity DraggingEntity = null;
	private Entity HoveringEntity = null;
	private Int2 DraggingEntityOffset;
	private string HoveringEntityName = "";


	#endregion




	#region --- MSG ---


	static RiggedGame () => Util.AddAssembly(typeof(RiggedGame).Assembly);


	public RiggedGame (params string[] args) : base(args) {

		Instance = this;

		// Get Host pID
		RiggedFontCount = 0;
		foreach (string arg in args) {
			// Host Process
			if (arg.StartsWith("-pID:")) {
				if (int.TryParse(arg[5..], out int pID)) {
					try {
						HostProcess = Process.GetProcessById(pID);
					} catch { }
				}
			}
			// Memory Name
			if (arg.StartsWith("-map:")) {
				MapName = arg[5..];
			}
			// Font Count
			if (arg.StartsWith("-fontCount:")) {
				if (int.TryParse(arg[11..], out int fCount)) {
					RiggedFontCount = fCount;
				}
			}
			// View
			if (arg.StartsWith("-view:")) {
				string[] viewStrs = arg[6..].Split(',');
				int _viewX = StartWithView.x;
				int _viewY = StartWithView.y;
				int _viewWidth = StartWithView.width;
				int _viewHeight = StartWithView.height;
				int _z = 0;
				if (viewStrs.Length >= 4) {
					if (int.TryParse(viewStrs[0], out _viewX)) { }
					if (int.TryParse(viewStrs[1], out _viewY)) { }
					if (int.TryParse(viewStrs[2], out _viewHeight)) {
						_viewHeight = _viewHeight.GreaterOrEquel(1);
						_viewWidth = GetViewWidthFromViewHeight(_viewHeight);
					}
					if (int.TryParse(viewStrs[3], out _z)) { }
					StartWithView.x = _viewX;
					StartWithView.y = _viewY;
					StartWithView.width = _viewWidth;
					StartWithView.height = _viewHeight;
					StartWithZ = _z;
				}
			}
		}

		MapEditor.ResetCameraAtStart = StartWithView == default;

		// Init Stream
		Debug.OnLogException += LogException;
		Debug.OnLogError += LogError;
		Debug.OnLog += Log;
		Debug.OnLogWarning += LogWarning;

		// Init Raylib
		RayUtil.InitWindowForRiggedGame();

		// Debug
		KeyboardHoldingFrames = new int[typeof(KeyboardKey).EnumLength()].FillWithValue(-1);
		GamepadHoldingFrames = new int[typeof(GamepadKey).EnumLength()].FillWithValue(-1);

		static void Log (object msg) {
			System.Console.ResetColor();
			System.Console.WriteLine(msg);
		}
		static void LogWarning (object msg) {
			System.Console.WriteLine(msg);
		}
		static void LogError (object msg) {
			System.Console.WriteLine(msg);
		}
		static void LogException (System.Exception ex) {
			System.Console.WriteLine(ex.Source);
			System.Console.WriteLine(ex.GetType().Name);
			System.Console.WriteLine(ex.Message);
			if (ex.TargetSite != null) {
				System.Console.WriteLine(ex.TargetSite.Name);
			}
			System.Console.WriteLine(ex.StackTrace);
			System.Console.WriteLine();
		}
	}


	public bool UpdateWithPipe () {

		if (HostProcess == null || HostProcess.HasExited) return false;

		bool success = Update_MemoryMap();
		if (!success) return false;

		Update_Calling();

		RespondMessage.Reset();
		RespondMessage.EffectEnable = CallingMessage.EffectEnable;

		Update_MapEditor();

		Update();

		success = Update_Respond();
		if (!success) return false;

		return true;
	}


	private bool Update_MemoryMap () {

		// Init Map
		if (MemMap == null) {
			MemMap = MemoryMappedFile.CreateOrOpen(MapName, capacity: Const.RIG_BUFFER_SIZE);
			ViewAccessor = MemMap.CreateViewAccessor(offset: 0, size: Const.RIG_BUFFER_SIZE);
			unsafe {
				ViewAccessor.SafeMemoryMappedViewHandle.AcquirePointer(ref BufferPointer);
			}
			RespondMessage.TransationStart();
		}

		// Read Calling from Pipe Mem
		unsafe {
			while (*BufferPointer == 1) {
				Thread.Sleep(1);
				if (HostProcess == null || HostProcess.HasExited) return false;
			}
			if (*BufferPointer == 255) return false;
			CallingMessage.ReadDataFromPipe(BufferPointer + 1);
		}
		CurrentPressedCharIndex = 0;
		CurrentPressedKeyIndex = 0;

		return true;
	}


	private void Update_Calling () {

		// Char Requirement
		for (int i = 0; i < CallingMessage.CharRequiredCount; i++) {
			var data = CallingMessage.RequiredChars[i];
			var key = new Int2(data.Char, data.FontIndex);
			if (CharPool.ContainsKey(key)) continue;
			CharPool.Add(key, data.Valid ? new CharSprite() {
				Char = data.Char,
				Advance = data.Advance,
				FontIndex = data.FontIndex,
				Offset = data.Offset,
				Texture = null,
			} : null);
		}

		// Input
		for (int i = 0; i < CallingMessage.HoldingKeyboardKeyCount; i++) {
			int keyIndex = CallingMessage.HoldingKeyboardKeys[i];
			if (keyIndex < 0 || keyIndex >= KeyboardHoldingFrames.Length) continue;
			KeyboardHoldingFrames[keyIndex] = PauselessFrame;
		}
		for (int i = 0; i < CallingMessage.HoldingGamepadKeyCount; i++) {
			int keyIndex = CallingMessage.HoldingGamepadKeys[i];
			if (keyIndex < 0 || keyIndex >= GamepadHoldingFrames.Length) continue;
			GamepadHoldingFrames[keyIndex] = PauselessFrame;
		}

		// Requirement
		if (CallingMessage.RequireGameMessageInvoke.GetBit(0)) {
			// Require Focus
			InvokeWindowFocusChanged(true);
		}
		if (CallingMessage.RequireGameMessageInvoke.GetBit(1)) {
			// Require Lost Focus
			InvokeWindowFocusChanged(false);
		}
		if (CallingMessage.RequireGameMessageInvoke.GetBit(2)) {
			// Require Clear Char Cache
			CharPool.Clear();
			Renderer.ClearCharSpritePool();
		}

		// Require Draw Colliders
		DrawCollider = CallingMessage.RequireGameMessageInvoke.GetBit(3);

		// Require Entity Clicker
		EntityClickerOn = CallingMessage.RequireGameMessageInvoke.GetBit(4);

		// Reload Character Movement
		if (CallingMessage.RequireGameMessageInvoke.GetBit(5) && Player.Selecting != null) {
			CharacterMovement.ReloadMovementConfigFromFile(Player.Selecting.GetType().AngeName());
		}

		// Toolset Command
		switch (CallingMessage.RequireToolsetCommand) {
			case RigCallingMessage.ToolCommand.RunCodeAnalysis:
				FrameworkUtil.RunAngeliaCodeAnalysis();
				break;
			case RigCallingMessage.ToolCommand.RunCodeAnalysisSilently:
				FrameworkUtil.RunAngeliaCodeAnalysis(true);
				break;
		}

	}


	private void Update_MapEditor () {

		// Start View
		if (StartWithView != default && MapEditor.Instance != null) {
			MapEditor.Instance.SetView(StartWithView, StartWithZ);
			StartWithView = default;
		}

		// Setting Changed
		if (CallingMessage.RequireSettingChange) {
			bool mapEditorActive = false;
			if (MapEditor.Instance != null) {
				MapEditor.Instance.AutoZoom = CallingMessage.Setting_MEDT_AutoZoom;
				MapEditor.Instance.QuickPlayerDrop = CallingMessage.Setting_MEDT_QuickPlayerDrop;
				MapEditor.Instance.ShowBehind = CallingMessage.Setting_MEDT_ShowBehind;
				MapEditor.Instance.ShowState = CallingMessage.Setting_MEDT_ShowState;
				mapEditorActive = MapEditor.Instance.Active;
			}
			bool enableMapEditor = CallingMessage.Setting_MEDT_Enable;
			if (mapEditorActive != enableMapEditor) {
				if (enableMapEditor) {
					Stage.SpawnEntity(MapEditor.TYPE_ID, 0, 0);
				} else {
					MapEditor.Instance.Active = false;
					RestartGame();
				}
			}
		}

	}


	private bool Update_Respond () {

		// Quit Check
		unsafe {
			if (*BufferPointer == 255) return false;
		}

		// Renderer Layer/Cells >> Message Layer/Cells
		for (int layer = 0; layer < RenderLayer.COUNT; layer++) {
			if (RespondMessage.Layers[layer] == null) {
				RespondMessage.Layers[layer] = new RigRespondMessage.RenderingLayerData(Renderer.GetLayerCapacity(layer));
			}
			if (GlobalFrame < 3) continue;
			var layerData = RespondMessage.Layers[layer];
			layerData.CellCount = 0;
			if (!Renderer.GetCells(layer, out var cells, out int count)) continue;
			count = Util.Min(count, layerData.Cells.Length);
			layerData.CellCount = count;
			for (int i = 0; i < count; i++) {
				var source = cells[i];
				var target = layerData.Cells[i];
				target.SpriteID = source.Sprite != null ? source.Sprite.ID : 0;
				target.TextSpriteChar = source.TextSprite != null ? source.TextSprite.Char : '\0';
				target.FontIndex = source.TextSprite != null ? source.TextSprite.FontIndex : 0;
				target.X = source.X;
				target.Y = source.Y;
				target.Z = source.Z;
				target.Width = source.Width;
				target.Height = source.Height;
				target.Rotation1000 = source.Rotation1000;
				target.PivotX = source.PivotX;
				target.PivotY = source.PivotY;
				target.Color = source.Color;
				target.Shift = source.Shift;
				target.BorderSide = source.BorderSide;
			}
		}

		// Finish
		RespondMessage.GamePlaying = WorldSquad.Enable;
		RespondMessage.ViewX = Stage.ViewRect.x;
		RespondMessage.ViewY = Stage.ViewRect.y;
		RespondMessage.ViewZ = MapEditor.Instance != null ? MapEditor.Instance.CurrentZ : 0;
		RespondMessage.ViewHeight = Stage.ViewRect.height;
		RespondMessage.SkyBottom = Sky.SkyTintBottomColor;
		RespondMessage.SkyTop = Sky.SkyTintTopColor;
		for (int i = 0; i < RenderLayer.COUNT; i++) {
			RespondMessage.RenderUsages[i] = Renderer.GetUsedCellCount(i);
			RespondMessage.RenderCapacities[i] = Renderer.GetLayerCapacity(i);
		}
		for (int i = 0; i < EntityLayer.COUNT; i++) {
			RespondMessage.EntityUsages[i] = Stage.EntityCounts[i];
			RespondMessage.EntityCapacities[i] = Stage.Entities[i].Length;
		}
		RespondMessage.MusicVolume = MusicVolume;
		RespondMessage.SoundVolume = SoundVolume;
		RespondMessage.IsTyping = GUI.IsTyping;
		RespondMessage.SelectingPlayerID = Player.Selecting != null ? Player.Selecting.TypeID : 0;

		// Respond to Memory
		unsafe {
			RespondMessage.WriteDataToPipe(BufferPointer + 1);
			unsafe {
				if (*BufferPointer == 255) return false;
			}
			*BufferPointer = 1;
		}

		return true;
	}


	public void OnQuitting () {
		InvokeGameQuitting();
		MemMap?.Dispose();
		ViewAccessor?.Dispose();
	}


	[OnGameUpdateLater(4096)]
	internal static void OnGameUpdateLater () {
		Instance?.UpdateColliderGizmos();
		Instance?.UpdateEntityClicker();
	}


	private void UpdateColliderGizmos () {

		if (PlayerMenuUI.ShowingUI || !DrawCollider) return;

		// Init Cells
		if (CellPhysicsCells.Count == 0) {
			try {
				var layers = Util.GetStaticFieldValue(typeof(Physics), "Layers") as System.Array;
				for (int layerIndex = 0; layerIndex < PhysicsLayer.COUNT; layerIndex++) {
					var layerObj = layers.GetValue(layerIndex);
					CellPhysicsCells.Add(Util.GetFieldValue(layerObj, "Cells") as PhysicsCell[,,]);
				}
			} catch (System.Exception ex) { Debug.LogException(ex); }
			if (CellPhysicsCells.Count == 0) CellPhysicsCells.Add(null);
		}

		// Draw Cells
		if (CellPhysicsCells.Count > 0 && CellPhysicsCells[0] != null) {
			int thick = GUI.Unify(1);
			var cameraRect = Renderer.CameraRect;
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
								if (cell.Frame != Physics.CurrentFrame) break;
								if (!cell.Rect.Overlaps(cameraRect)) continue;
								DrawGizmosRect(cell.Rect.Edge(Direction4.Down, thick), tint);
								DrawGizmosRect(cell.Rect.Edge(Direction4.Up, thick), tint);
								DrawGizmosRect(cell.Rect.Edge(Direction4.Left, thick), tint);
								DrawGizmosRect(cell.Rect.Edge(Direction4.Right, thick), tint);
							}
						}
					}
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
		}
	}


	private void UpdateEntityClicker () {

		if (PlayerMenuUI.ShowingUI || !EntityClickerOn) return;

		Input.IgnoreMouseToActionJump(ignoreAction: true, ignoreJump: false, useMidButtonAsAction: true);
		bool mouseDown = Input.MouseLeftButtonDown;
		bool mouseHolding = Input.MouseLeftButtonHolding;
		if (!mouseHolding) DraggingEntity = null;
		int thick = GUI.Unify(1);

		// For all Entity
		bool hoverFlag = false;
		bool dragging = mouseHolding && DraggingEntity != null;
		for (int layer = 0; layer < EntityLayer.COUNT; layer++) {
			var entities = Stage.Entities[layer];
			int count = Stage.EntityCounts[layer];
			for (int i = 0; i < count; i++) {
				var e = entities[i];
				if (!e.Active) continue;
				var bounds = e.GlobalBounds;
				var gizmosTint = Color32.CYAN_BETTER;
				// Click
				bool mouseInside = bounds.MouseInside();
				if (!dragging && !hoverFlag && mouseInside) {
					hoverFlag = true;
					if (e != HoveringEntity) {
						HoveringEntityName = e.GetType().AngeName();
					}
					HoveringEntity = e;
					gizmosTint = Color32.WHITE;
				}
				if (mouseDown && mouseInside) {
					DraggingEntity = e;
					dragging = mouseHolding;
					DraggingEntityOffset = Input.MouseGlobalPosition - bounds.position;
				}
				// Gizmos
				DrawGizmosRect(bounds.Edge(Direction4.Down, thick), gizmosTint);
				DrawGizmosRect(bounds.Edge(Direction4.Up, thick), gizmosTint);
				DrawGizmosRect(bounds.Edge(Direction4.Left, thick), gizmosTint);
				DrawGizmosRect(bounds.Edge(Direction4.Right, thick), gizmosTint);
			}
		}

		// Entity Dragging
		if (dragging) {
			if (DraggingEntity.Active) {
				var mousePos = Input.MouseGlobalPosition;
				var bounds = DraggingEntity.GlobalBounds;
				int deltaX = mousePos.x - bounds.x - DraggingEntityOffset.x;
				int deltaY = mousePos.y - bounds.y - DraggingEntityOffset.y;
				DraggingEntity.X += deltaX;
				DraggingEntity.Y += deltaY;
				// Snap
				if (IsKeyboardKeyHolding(KeyboardKey.LeftAlt)) {
					const int SNAP = Const.HALF / 2;
					DraggingEntity.X = ((float)DraggingEntity.X / SNAP).RoundToInt() * SNAP;
					DraggingEntity.Y = ((float)DraggingEntity.Y / SNAP).RoundToInt() * SNAP;
				}
				// Delete
				if (Input.KeyboardDown(KeyboardKey.Delete)) {
					DraggingEntity.Active = false;
					DraggingEntity = null;
				}
			} else {
				DraggingEntity = null;
			}
		} else if (hoverFlag && HoveringEntity != null && !string.IsNullOrEmpty(HoveringEntityName)) {
			// Entity Name
			var mousePos = Input.MouseGlobalPosition;
			using (new UILayerScope()) {
				GUI.ShadowLabel(
					new IRect(mousePos.x, mousePos.y + GUI.Unify(24), 1, GUI.Unify(24)),
					HoveringEntityName,
					style: GUISkin.Default.SmallCenterLabel
				);
			}
		}

	}


	#endregion




}