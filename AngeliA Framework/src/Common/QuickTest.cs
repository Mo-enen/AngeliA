using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AngeliA;

#if DEBUG
/// <summary>
/// Utility tool for quick testing ⚠Debug only. Do not work after game release.⚠
/// </summary>
public class QTest {




	#region --- SUB ---


	private enum DataType : byte { Bool, Int, Float, String, Pixels, Button, Func, }


	private class BoolData {
		public bool value;
		public string displayLabel;
		public KeyData Key;
	}


	private class IntData {
		public int value;
		public int defaultValue;
		public int min;
		public int max;
		public int step;
		public string displayLabel;
		public KeyData Key;
	}


	private class FloatData {
		public float value;
		public float defaultValue;
		public float min;
		public float max;
		public float step;
		public string displayLabel;
		public KeyData Key;
	}


	private class StringData {
		public string value;
		public KeyData Key;
	}


	private class PixelData {
		public Color32[,] pixels;
		public KeyData Key;
	}


	private class ButtonData {
		public Action this[int index] => index switch {
			0 => Action,
			1 => Action1,
			2 => Action2,
			3 => Action3,
			_ => Action,
		};
		public string GetLabel (int index) => index switch {
			0 => Label,
			1 => Label1,
			2 => Label2,
			3 => Label3,
			_ => Key.key,
		};
		public Action Action;
		public Action Action1;
		public Action Action2;
		public Action Action3;
		public KeyData Key;
		public string Label;
		public string Label1;
		public string Label2;
		public string Label3;
		public object Param;
		public object Icon;
		public int ActionCount = 1;
	}


	private class FuncData {
		public KeyData Key;
		public Func<IRect, int> Func;
		public bool ShowLabel;
		public object Param;
	}


	private class KeyData {
		public string key;
		public int Order;
		public DataType Type;
		public string Group;
		public int GroupOrder;
		public int UpdateFrame;
	}


	private class GroupData {
		public bool Folding;
		public int UpdateFrame;
		public int Order;
	}


	private class KeyComparer : IComparer<KeyData> {
		public static readonly KeyComparer Instance = new();
		public int Compare (KeyData a, KeyData b) {
			int result = a.GroupOrder.CompareTo(b.GroupOrder);
			return result != 0 ? result : a.Order.CompareTo(b.Order);
		}
	}


	#endregion




	#region --- VAR ---


	// Api
	/// <summary>
	/// Limit of testing window panels
	/// </summary>
	public const int MAX_WINDOW_COUNT = 16;
	/// <summary>
	/// True if the QTest is operating with window on screen
	/// </summary>
	public static bool Testing => ShowingWindow && !IgnoringWindow;
	/// <summary>
	/// True if the window include the fields that not update in current frame
	/// </summary>
	public static bool ShowNotUpdatedData { get; set; } = true;
	/// <summary>
	/// Unified height of a field
	/// </summary>
	public static int FieldHeight { get; set; } = 22;
	/// <summary>
	/// User data for QTest.Func
	/// </summary>
	public static object CurrentInvokingParam { get; private set; } = null;

	// Data
	private static readonly QTest[] Windows = new QTest[MAX_WINDOW_COUNT].FillWithNewValue();
	private static readonly List<(int startFrame, Int3 pos, int duration, int size, Color32 color)> Marks = [];
	private static bool ShowingWindow = false;
	private static bool IgnoringWindow = false;
	private static int CurrentWindowIndex = 0;
	private static int MouseDragMovingIndex = -1;
	private readonly Dictionary<string, BoolData> BoolPool = [];
	private readonly Dictionary<string, IntData> IntPool = [];
	private readonly Dictionary<string, FloatData> FloatPool = [];
	private readonly Dictionary<string, StringData> StringPool = [];
	private readonly Dictionary<string, PixelData> PixelsPool = [];
	private readonly Dictionary<string, ButtonData> ButtonPool = [];
	private readonly Dictionary<string, FuncData> FuncPool = [];
	private readonly Dictionary<string, object> ObjectPool = [];
	private readonly List<KeyData> Keys = [];
	private readonly Dictionary<string, GroupData> GroupFolding = [];
	private Int2 PanelPositionOffset = new(int.MinValue, int.MinValue);
	private Color32[,] CurrentPixels;
	private string CurrentGroup = "";
	private int CurrentOrder;
	private int CurrentGroupOrder;
	private int PrevKeyCount = 0;
	private int PanelMaxExpand = 0;
	private string Title = "";


	#endregion




	#region --- MSG ---


	[OnGameUpdateLater]
	internal static void UpdateMarks () {
		if (Testing) {
			Game.ForceGizmosOnTopOfUI(1);
			LightingSystem.IgnoreLighting(1);
			Input.IgnoreMouseToActionJump();
		}
		// Draw Trailing Mark
		if (Renderer.TryGetSprite(BuiltInSprite.CIRCLE_32, out var circleSP)) {
			var cameraRect = Renderer.CameraRect;
			using var _ = new UILayerScope(ignoreSorting: true);
			for (int i = 0; i < Marks.Count; i++) {
				var (frame, pos, dur, size, color) = Marks[i];
				int localFrame = Game.GlobalFrame - frame;
				if (localFrame >= dur) {
					Marks.RemoveAt(i);
					i--;
					continue;
				}
				if (pos.z != Stage.ViewZ) continue;
				Renderer.Draw(
					circleSP,
					pos.x.Clamp(cameraRect.xMin, cameraRect.xMax),
					pos.y.Clamp(cameraRect.yMin, cameraRect.yMax),
					500, 500, 0, size, size,
					color.WithNewA((dur - localFrame) * 360 / dur), z: int.MaxValue
				);
			}
		}
	}


	[OnGameUpdate(-256)]
	internal static void UpdateAllWindows () {
		if (!Testing) return;
		for (int i = 0; i < Windows.Length; i++) {
			Windows[i].UpdateWindow(i);
		}
	}


	[OnGameUpdateLater(int.MaxValue)]
	internal static void ResetWindowIndex () => CurrentWindowIndex = 0;


	private void UpdateWindow (int windowIndex) {

		if (Keys.Count == 0) return;

		int validWindowCount = 0;
		int lastWindowIndex = -1;
		for (int i = 0; i < Windows.Length; i++) {
			if (Windows[i].Keys.Count > 0) {
				validWindowCount++;
				lastWindowIndex = i;
			}
		}

		CurrentGroup = "";
		CurrentOrder = 0;
		CurrentGroupOrder = 0;
		if (Keys.Count != PrevKeyCount) {
			// Update Group Order
			foreach (var key in Keys) {
				if (GroupFolding.TryGetValue(key.Group, out var gData)) {
					key.GroupOrder = gData.Order;
				}
			}
			// Sort Keys
			PrevKeyCount = Keys.Count;
			Keys.Sort(KeyComparer.Instance);
		}

		using var _ = new UILayerScope();
		var cameraRect = Renderer.CameraRect;
		int basicPanelWidth = cameraRect.width / (validWindowCount + 1).LessOrEquel(5);
		if (PanelPositionOffset.x == int.MinValue) {
			PanelPositionOffset.x = 1024 + windowIndex * basicPanelWidth;
			PanelPositionOffset.y = 1024 + cameraRect.height;
		}
		var panelRect = new IRect(
			cameraRect.x + PanelPositionOffset.x,
			cameraRect.y + PanelPositionOffset.y,
			basicPanelWidth, 1
		);
		int padding = GUI.FieldPadding;
		int fieldHeight = GUI.Unify(FieldHeight);

		Input.CancelIgnoreMouseInput();
		bool mouseHoldingL = Game.IsMouseLeftHolding;
		bool mouseMidDown = Input.MouseMidButtonDown;
		bool ignoreStep = Input.HoldingAlt;
		var rect = panelRect.EdgeInsideUp(fieldHeight).Shrink(padding, padding, 0, 0);

		// BG
		int border = GUI.Unify(1);
		var bgCell = Renderer.DrawPixel(panelRect, Color32.BLACK);

		// Drag to Move
		var titleCell = Renderer.DrawPixel(rect, Color32.GREY_20);
		GUI.Label(rect, string.IsNullOrEmpty(Title) ? $"Quick Test [{windowIndex}]" : Title, GUI.Skin.SmallCenterGreyLabel);
		if (!mouseHoldingL) MouseDragMovingIndex = -1;

		if (
			(rect.Expand(0, PanelMaxExpand, 0, 0).MouseInside() && MouseDragMovingIndex == -1) ||
			MouseDragMovingIndex == windowIndex
		) {
			Cursor.SetCursorAsMove();
			if (Input.MouseLeftButtonDown) {
				MouseDragMovingIndex = windowIndex;
			}
			if (MouseDragMovingIndex == windowIndex && mouseHoldingL) {
				PanelPositionOffset += Input.MouseGlobalPositionDelta;
			}
		}

		// Ignore Btn
		if (lastWindowIndex == windowIndex) {
			var ignoreRect = rect.EdgeInsideRight(rect.height).Shift(PanelMaxExpand, 0);
			if (GUI.Button(ignoreRect, BuiltInSprite.ICON_DELETE, GUI.Skin.SmallIconButton)) {
				IgnoringWindow = true;
				Input.UseAllMouseKey();
			}
		}


		rect.SlideDown(padding);

		// Content
		GUI.BeginChangeCheck();
		int index = 0;
		int indent = GUI.Unify(26);
		rect = rect.ShrinkLeft(indent);
		int rectLeft = rect.x;
		rect = rect.ShrinkLeft(indent);
		foreach (var kData in Keys) {

			string key = kData.key;
			string group = kData.Group;
			bool groupNotEmpty = !string.IsNullOrEmpty(group);
			bool folding = GroupFolding.TryGetValue(group, out var gData) && gData.Folding;
			rect.x = groupNotEmpty ? rectLeft + indent : rectLeft;

			// Update Check
			if (!ShowNotUpdatedData && kData.UpdateFrame < Game.PauselessFrame) continue;

			// Group Fold
			if (group != CurrentGroup) {
				CurrentGroup = group;
				if (groupNotEmpty) {
					using var __ = new GUIContentColorScope(Color32.GREY_128);
					// Tri Mark
					Renderer.Draw(
						folding ? BuiltInSprite.ICON_TRIANGLE_RIGHT : BuiltInSprite.ICON_TRIANGLE_DOWN,
						rect.EdgeOutsideLeft(rect.height - padding).Shift(-indent - padding / 2, 0).Fit(1, 1),
						Color32.GREY_96
					);
					// Fold Button
					if (GUI.Button(rect.Expand(indent, 0, 0, 0), group, GUI.Skin.SmallLabelButton)) {
						gData.Folding = !folding;
						PrevKeyCount = -1;
					}
					rect.SlideDown();
				}
			}

			// Fold Check
			if (folding) continue;

			// Value
			var valueRect = rect.ShrinkLeft(rect.width / 3);
			switch (kData.Type) {
				// Bool
				case DataType.Bool: {
					var data = BoolPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					data.value = GUI.Toggle(valueRect, data.value);
					// Display Label
					if (data.displayLabel != null) {
						GUI.BackgroundLabel(
							valueRect.ShrinkLeft(valueRect.height + padding + padding),
							data.displayLabel,
							Color32.BLACK,
							out var bounds,
							padding,
							style: GUI.Skin.SmallLabel
						);
						int maxX = bounds.xMax + padding;
						if (maxX > panelRect.xMax) {
							panelRect.xMax = maxX;
						}
					}
					break;
				}
				// Int
				case DataType.Int: {
					var data = IntPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					int valueLabelWidth = valueRect.height * 2;
					data.value = GUI.HandleSlider(
						3126784 + index + windowIndex * 624123,
						valueRect.ShrinkRight(valueLabelWidth),
						data.value, data.min, data.max,
						step: ignoreStep ? 0 : data.step
					);
					if (mouseMidDown && valueRect.MouseInside()) {
						data.value = data.defaultValue;
					}
					if (data.displayLabel == null) {
						GUI.IntLabel(valueRect.EdgeInsideRight(valueLabelWidth), data.value, GUI.Skin.SmallCenterLabel);
					} else {
						GUI.BackgroundLabel(
							valueRect.EdgeInsideRight(valueLabelWidth).Shift(padding, 0),
							data.displayLabel,
							Color32.BLACK, out var bounds, padding,
							style: GUI.Skin.SmallLabel
						);
						int maxX = bounds.xMax + padding;
						if (maxX > panelRect.xMax) {
							panelRect.xMax = maxX;
						}
					}
					break;
				}
				// Float
				case DataType.Float: {
					var data = FloatPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					int valueLabelWidth = valueRect.height * 2;

					data.value = GUI.HandleSlider(
						3126784 + index + windowIndex * 624123,
						valueRect.ShrinkRight(valueLabelWidth),
						(data.value * 10000f).RoundToInt(), (data.min * 10000f).RoundToInt(), (data.max * 10000f).RoundToInt(),
						step: ignoreStep ? 0 : (data.step * 10000f).RoundToInt()
					) / 10000f;

					if (mouseMidDown && valueRect.MouseInside()) {
						data.value = data.defaultValue;
					}

					if (data.displayLabel == null) {
						GUI.Label(
							valueRect.EdgeInsideRight(valueLabelWidth),
							data.value.ToString("0.00"),
							GUI.Skin.SmallCenterLabel
						);
					} else {
						GUI.BackgroundLabel(
							valueRect.EdgeInsideRight(valueLabelWidth).Shift(padding, 0),
							data.displayLabel,
							Color32.BLACK, out var bounds, padding,
							style: GUI.Skin.SmallLabel
						);
						int maxX = bounds.xMax + padding;
						if (maxX > panelRect.xMax) {
							panelRect.xMax = maxX;
						}
					}
					break;
				}
				// String
				case DataType.String: {
					var data = StringPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					data.value = GUI.SmallInputField(
						3126784 + index + windowIndex * 624123,
						valueRect,
						data.value
					);
					break;
				}
				// Pixels
				case DataType.Pixels: {
					var data = PixelsPool[key];
					// Label
					GUI.SmallLabel(rect, key);
					// Value
					int width = data.pixels.GetLength(0);
					int height = data.pixels.GetLength(1);
					if (width <= 0 || height <= 0) break;
					IRect canvasRect;
					if ((float)valueRect.width / valueRect.height < (float)width / height) {
						// Wide
						canvasRect = valueRect.Fit(width, height, 500, 1000);
					} else {
						// Tall
						canvasRect = valueRect.Envelope(width, height);
						canvasRect.y = valueRect.yMax - canvasRect.height;
					}
					rect = canvasRect;
					float pxWidth = (float)canvasRect.width / width;
					float pxHeight = (float)canvasRect.height / height;
					float pixelFixW = 512f / width;
					float pixelFixH = 512f / height;
					var pRect = new IRect(
						0, 0,
						((pxWidth + pixelFixW) * 5f / 4f).CeilToInt(),
						((pxHeight + pixelFixH) * 5f / 4f).CeilToInt()
					);
					for (int j = 0; j < height; j++) {
						pRect.y = canvasRect.y + (j * pxHeight).RoundToInt();
						for (int i = 0; i < width; i++) {
							pRect.x = canvasRect.x + (i * pxWidth).RoundToInt();
							Game.DrawGizmosRect(pRect, data.pixels[i, j]);
						}
					}
					break;
				}
				// Button
				case DataType.Button: {
					var data = ButtonPool[key];
					var btnRect = rect.ShrinkLeft(data.Icon != null ? rect.height + padding : 0);
					for (int i = 0; i < data.ActionCount; i++) {
						if (GUI.Button(
							rect.PartHorizontal(i, data.ActionCount).ShrinkRight(padding / 2),
							data.GetLabel(i),
							GUI.Skin.SmallDarkButton
						)) {
							CurrentInvokingParam = data.Param;
							data[i]?.Invoke();
							CurrentInvokingParam = null;
						}
					}
					if (data.Icon != null) {
						Game.DrawGizmosTexture(rect.EdgeInsideLeft(rect.height), data.Icon);
					}
					break;
				}
				// Func
				case DataType.Func: {
					var data = FuncPool[key];
					// Label
					if (data.ShowLabel) {
						GUI.SmallLabel(rect, key);
					}
					// Func
					CurrentInvokingParam = data.Param;
					int height = data.Func.Invoke(data.ShowLabel ? valueRect : rect);
					CurrentInvokingParam = null;
					rect.yMin = rect.yMax - height;
					break;
				}
			}

			// Next
			rect.SlideDown(padding);
			if (rect.height != fieldHeight) {
				rect.yMin = rect.yMax - fieldHeight;
			}

			index++;
		}

		if (GUI.EndChangeCheck()) {
			PrevKeyCount = -1;
		}

		// Final
		PanelMaxExpand = Util.Max(PanelMaxExpand, panelRect.width - basicPanelWidth);
		panelRect.yMin = rect.yMax - padding;
		panelRect.width = Util.Max(panelRect.width, basicPanelWidth + PanelMaxExpand);
		bgCell.SetRect(panelRect);
		titleCell.SetRect(panelRect.EdgeInsideUp(titleCell.Height));
		CurrentGroup = "";

		// Clamp
		PanelPositionOffset.Clamp(
			0,
			Util.Min(panelRect.height, cameraRect.height),
			cameraRect.width - panelRect.width,
			cameraRect.height
		);

		// Block Event
		if (panelRect.MouseInside() || MouseDragMovingIndex == windowIndex) {
			Input.IgnoreMouseInput(0);
			Cursor.SetCursorAsNormal(-1);
			Cursor.SetCursor(Cursor.CurrentCursorIndex, int.MaxValue - 1);
		}

	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Set which window should include the fields comes after
	/// </summary>
	public static void SetCurrentWindow (int index, string title = "") {
		CurrentWindowIndex = index.Clamp(0, MAX_WINDOW_COUNT - 1);
		Windows[CurrentWindowIndex].Title = title;
	}


	/// <summary>
	/// Load testing data from file. This will override current data.
	/// </summary>
	/// <param name="path">Path of the data file (not a folder)</param>
	/// <param name="ignorePanelOffset">True if this function do not adjust position of window panels</param>
	public static void LoadAllDataFromFile (string path, bool ignorePanelOffset = false) {
		ClearAll();
		if (!Util.FileExists(path)) return;

		using var fs = File.OpenRead(path);
		using var rd = new BinaryReader(fs);

		int windowCount = rd.ReadInt32();
		for (int windowI = 0; windowI < windowCount; windowI++) {
			int windowIndex = rd.ReadByte();
			var window = Windows[windowIndex];

			// Panel Offset
			int panelOffsetX = rd.ReadInt32();
			int panelOffsetY = rd.ReadInt32();
			if (!ignorePanelOffset) {
				window.PanelPositionOffset.x = panelOffsetX;
				window.PanelPositionOffset.y = panelOffsetY;
			}

			// Keys
			int keyCount = rd.ReadInt32();
			var keyPool = new Dictionary<(string, DataType), KeyData>();
			for (int i = 0; i < keyCount; i++) {
				string key = rd.ReadString();
				var type = (DataType)rd.ReadByte();
				string group = rd.ReadString();
				var keyData = new KeyData() {
					key = key,
					Group = group,
					Order = 0,
					Type = type,
					UpdateFrame = -1,
				};
				window.Keys.Add(keyData);
				keyPool.TryAdd((key, type), keyData);
			}

			// Bool
			int boolCount = rd.ReadInt32();
			for (int i = 0; i < boolCount; i++) {
				string key = rd.ReadString();
				bool value = rd.ReadByte() == 1;
				if (!keyPool.TryGetValue((key, DataType.Bool), out var kData)) {
					kData = new KeyData() {
						key = key,
						Group = "",
						Order = 0,
						Type = DataType.Bool,
						UpdateFrame = -1,
					};
					window.Keys.Add(kData);
				}
				window.BoolPool.Add(key, new BoolData() {
					displayLabel = "",
					Key = kData,
					value = value,
				});
			}

			// Int
			int intCount = rd.ReadInt32();
			for (int i = 0; i < intCount; i++) {
				string key = rd.ReadString();
				int value = rd.ReadInt32();
				int min = rd.ReadInt32();
				int max = rd.ReadInt32();
				int step = rd.ReadInt32();
				if (!keyPool.TryGetValue((key, DataType.Int), out var kData)) {
					kData = new KeyData() {
						key = key,
						Group = "",
						Order = 0,
						Type = DataType.Int,
						UpdateFrame = -1,
					};
					window.Keys.Add(kData);
				}
				window.IntPool.Add(key, new IntData() {
					displayLabel = "",
					Key = kData,
					value = value,
					defaultValue = value,
					min = min,
					max = max,
					step = step,
				});
			}

			// Float
			int floatCount = rd.ReadInt32();
			for (int i = 0; i < floatCount; i++) {
				string key = rd.ReadString();
				float value = rd.ReadSingle();
				float min = rd.ReadSingle();
				float max = rd.ReadSingle();
				float step = rd.ReadSingle();
				if (!keyPool.TryGetValue((key, DataType.Float), out var kData)) {
					kData = new KeyData() {
						key = key,
						Group = "",
						Order = 0,
						Type = DataType.Float,
						UpdateFrame = -1,
					};
					window.Keys.Add(kData);
				}
				window.FloatPool.Add(key, new FloatData() {
					displayLabel = "",
					Key = kData,
					value = value,
					defaultValue = value,
					min = min,
					max = max,
					step = step,
				});
			}

			// String
			int stringCount = rd.ReadInt32();
			for (int i = 0; i < stringCount; i++) {
				string key = rd.ReadString();
				string value = rd.ReadString();
				if (!keyPool.TryGetValue((key, DataType.String), out var kData)) {
					kData = new KeyData() {
						key = key,
						Group = "",
						Order = 0,
						Type = DataType.String,
						UpdateFrame = -1,
					};
					window.Keys.Add(kData);
				}
				window.StringPool.Add(key, new StringData() {
					Key = kData,
					value = value,
				});
			}

		}
	}


	/// <summary>
	/// Save current testing data into file
	/// </summary>
	/// <param name="path">Path of a file with any extension you want (not a folder)</param>
	public static void SaveAllDataToFile (string path) {

		using var fs = File.OpenWrite(path);
		using var wr = new BinaryWriter(fs);

		wr.Write((int)0);
		int validContentCount = 0;
		for (int i = 0; i < Windows.Length; i++) {
			var window = Windows[i];
			if (window.Keys.Count == 0) continue;
			validContentCount++;

			wr.Write((byte)i);
			wr.Write((int)window.PanelPositionOffset.x);
			wr.Write((int)window.PanelPositionOffset.y);

			// Keys
			var _keys = window.Keys.DistinctBy((_data) => _data.key).ToList();
			int rCount = _keys.RemoveAll(_data => _data.UpdateFrame < 0);
			wr.Write((int)_keys.Count);
			foreach (var keyData in _keys) {
				wr.Write((string)keyData.key);
				wr.Write((byte)keyData.Type);
				wr.Write((string)keyData.Group);
			}

			// Bool
			foreach (var (key, value) in window.BoolPool) {
				if (value.Key.UpdateFrame < 0) {
					window.BoolPool.Remove(key);
				}
			}
			wr.Write((int)window.BoolPool.Count);
			foreach (var (key, value) in window.BoolPool) {
				wr.Write((string)key);
				wr.Write((byte)(value.value ? 1 : 0));
			}

			// Int
			foreach (var (key, value) in window.IntPool) {
				if (value.Key.UpdateFrame < 0) {
					window.IntPool.Remove(key);
				}
			}
			wr.Write((int)window.IntPool.Count);
			foreach (var (key, value) in window.IntPool) {
				wr.Write((string)key);
				wr.Write((int)value.value);
				wr.Write((int)value.min);
				wr.Write((int)value.max);
				wr.Write((int)value.step);
			}

			// Float
			foreach (var (key, value) in window.FloatPool) {
				if (value.Key.UpdateFrame < 0) {
					window.FloatPool.Remove(key);
				}
			}
			wr.Write((int)window.FloatPool.Count);
			foreach (var (key, value) in window.FloatPool) {
				wr.Write((string)key);
				wr.Write((float)value.value);
				wr.Write((float)value.min);
				wr.Write((float)value.max);
				wr.Write((float)value.step);
			}

			// String
			foreach (var (key, value) in window.StringPool) {
				if (value.Key.UpdateFrame < 0) {
					window.StringPool.Remove(key);
				}
			}
			wr.Write((int)window.StringPool.Count);
			foreach (var (key, value) in window.StringPool) {
				wr.Write((string)key);
				wr.Write((string)value.value);
			}

		}
		fs.Seek(0, SeekOrigin.Begin);
		wr.Write((int)validContentCount);

	}


	// Data
	/// <summary>
	/// Require a toggle inside the current test window
	/// </summary>
	/// <param name="key">Unique key to identify this field</param>
	/// <param name="defaultValue"></param>
	/// <param name="displayLabel">Text content for rendering only</param>
	/// <param name="windowIndex">Force field into given window instead of current window</param>
	/// <returns>The current data of this field</returns>
	public static bool Bool (string key, bool defaultValue = false, string displayLabel = null, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].BoolLogic(key, defaultValue, displayLabel);
	private bool BoolLogic (string key, bool defaultValue = false, string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (BoolPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.Group = CurrentGroup;
			kData.UpdateFrame = Game.PauselessFrame;
			return result.value;
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Bool,
			Group = CurrentGroup,
			UpdateFrame = Game.PauselessFrame,
		};
		BoolPool.Add(key, new BoolData() {
			value = defaultValue,
			displayLabel = displayLabel,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
		return defaultValue;
	}


	/// <summary>
	/// Require a intager slider inside the current test window
	/// </summary>
	/// <param name="key">Unique key to identify this field</param>
	/// <param name="defaultValue"></param>
	/// <param name="min">Minimal limitation of this value</param>
	/// <param name="max">Maximal limitation of this value</param>
	/// <param name="step">Step count when dragging the slider. 0 means no step.</param>
	/// <param name="displayLabel">Text content for rendering only</param>
	/// <param name="windowIndex">Force field into given window instead of current window</param>
	/// <returns>The current data of this field</returns>
	public static int Int (string key, int defaultValue = 0, int min = 0, int max = 100, int step = 0, string displayLabel = null, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].IntLogic(key, defaultValue, min, max, step, displayLabel);
	private int IntLogic (string key, int defaultValue = 0, int min = 0, int max = 100, int step = 0, string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (IntPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			result.min = min;
			result.max = max;
			result.step = step;
			result.defaultValue = defaultValue;
			kData.UpdateFrame = Game.PauselessFrame;
			kData.Group = CurrentGroup;
			return result.value.Clamp(result.min, result.max);
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Int,
			Group = CurrentGroup,
			UpdateFrame = Game.PauselessFrame,
		};
		IntPool.Add(key, new IntData() {
			value = defaultValue.Clamp(min, max),
			defaultValue = defaultValue.Clamp(min, max),
			displayLabel = displayLabel,
			min = min,
			max = max,
			step = step,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
		return defaultValue.Clamp(min, max);
	}


	/// <summary>
	/// Require a float slider inside the current test window
	/// </summary>
	/// <param name="key">Unique key to identify this field</param>
	/// <param name="defaultValue"></param>
	/// <param name="min">Minimal limitation of this value</param>
	/// <param name="max">Maximal limitation of this value</param>
	/// <param name="step">Step count when dragging the slider. 0 means no step.</param>
	/// <param name="displayLabel">Text content for rendering only</param>
	/// <param name="windowIndex">Force field into given window instead of current window</param>
	/// <returns>The current data of this field</returns>
	public static float Float (string key, float defaultValue = 0, float min = 0, float max = 1f, float step = 0, string displayLabel = null, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].FloatLogic(key, defaultValue, min, max, step, displayLabel);
	private float FloatLogic (string key, float defaultValue = 0, float min = 0, float max = 1f, float step = 0f, string displayLabel = null) {
		ShowingWindow = true;
		CurrentOrder++;
		if (FloatPool.TryGetValue(key, out var result)) {
			result.displayLabel = displayLabel;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			result.min = min;
			result.max = max;
			result.defaultValue = defaultValue;
			result.step = step;
			kData.UpdateFrame = Game.PauselessFrame;
			kData.Group = CurrentGroup;
			return result.value.Clamp(result.min, result.max);
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Float,
			Group = CurrentGroup,
			UpdateFrame = Game.PauselessFrame,
		};
		FloatPool.Add(key, new FloatData() {
			value = defaultValue.Clamp(min, max),
			defaultValue = defaultValue.Clamp(min, max),
			displayLabel = displayLabel,
			min = min,
			max = max,
			step = step,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
		return defaultValue.Clamp(min, max);
	}


	/// <summary>
	/// Require a text input field inside the current test window
	/// </summary>
	/// <param name="key">Unique key to identify this field</param>
	/// <param name="defaultValue"></param>
	/// <param name="windowIndex">Force field into given window instead of current window</param>
	/// <returns>The current data of this field</returns>
	public static string String (string key, string defaultValue = "", int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].StringLogic(key, defaultValue);
	private string StringLogic (string key, string defaultValue = "") {
		ShowingWindow = true;
		CurrentOrder++;
		if (StringPool.TryGetValue(key, out var result)) {
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			kData.Group = CurrentGroup;
			return result.value;
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.String,
			Group = CurrentGroup,
			UpdateFrame = Game.PauselessFrame,
		};
		StringPool.Add(key, new StringData() {
			value = defaultValue,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
		return defaultValue;
	}


	/// <summary>
	/// Require a set of buttons that invoke the given System.Action when pressed
	/// </summary>
	/// <param name="key">Unique key to identify this field</param>
	/// <param name="action">First button's logic</param>
	/// <param name="icon">Icon for rendering. This should be a texture instance.</param>
	/// <param name="param">Custom user data for this field. (Use Qtest.CurrentInvokingParam inside the "action" to get this data)</param>
	/// <param name="windowIndex">Force field into given window instead of current window</param>
	/// <param name="action1">Second button's logic</param>
	/// <param name="action2">Third button's logic</param>
	/// <param name="action3">Fourth button's logic</param>
	/// <param name="label">First button's label content</param>
	/// <param name="label1">Second button's label content</param>
	/// <param name="label2">Third button's label content</param>
	/// <param name="label3">Fourth button's label content</param>
	public static void Button (string key, Action action, object icon = null, object param = null, int windowIndex = -1, Action action1 = null, string label = "", string label1 = "", Action action2 = null, string label2 = "", Action action3 = null, string label3 = "") => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].ButtonLogic(key, action, icon, param, action1, action2, action3, label, label1, label2, label3);
	private void ButtonLogic (string key, Action action, object icon, object param, Action action1, Action action2, Action action3, string label, string label1, string label2, string label3) {
		ShowingWindow = true;
		CurrentOrder++;
		if (ButtonPool.TryGetValue(key, out var result)) {
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			kData.Group = CurrentGroup;
			result.Action = action;
			result.Action1 = action1;
			result.Action2 = action2;
			result.Action3 = action3;
			result.Param = param;
			result.Icon = icon;
			result.ActionCount = 1 + (action1 != null ? 1 : 0) + (action2 != null ? 1 : 0) + (action3 != null ? 1 : 0);
			result.Label = string.IsNullOrEmpty(label) ? key : label;
			result.Label1 = label1;
			result.Label2 = label2;
			result.Label3 = label3;
			return;
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Button,
			Group = CurrentGroup,
			UpdateFrame = Game.PauselessFrame,
		};
		ButtonPool.Add(key, new ButtonData() {
			Action = action,
			Key = keyData,
			Param = param,
			Icon = icon,
			ActionCount = 1 + (action1 != null ? 1 : 0) + (action2 != null ? 1 : 0) + (action3 != null ? 1 : 0),
			Label = string.IsNullOrEmpty(label) ? key : label,
			Label1 = label1,
			Label2 = label2,
			Label3 = label3,
			Action1 = action1,
			Action2 = action2,
			Action3 = action3,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
	}


	/// <summary>
	/// GUI box that display custom content inside a given window area
	/// </summary>
	/// <param name="key">Unique key to identify this field</param>
	/// <param name="func">GUI rendering function. The param is the rect position that this element should rendering content into. Return value is the height of this field.</param>
	/// <param name="param">Custom user data for this field. (Use Qtest.CurrentInvokingParam inside the "action" to get this data)</param>
	/// <param name="showLabel">True if this content require the label name on left side.</param>
	/// <param name="windowIndex">Force field into given window instead of current window</param>
	public static void Func (string key, Func<IRect, int> func, object param = null, bool showLabel = false, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].FuncLogic(key, func, param, showLabel);
	private void FuncLogic (string key, Func<IRect, int> func, object param, bool showLabel) {
		ShowingWindow = true;
		CurrentOrder++;
		if (FuncPool.TryGetValue(key, out var result)) {
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			kData.Group = CurrentGroup;
			result.Func = func;
			result.Param = param;
			result.ShowLabel = showLabel;
			return;
		}
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Func,
			Group = CurrentGroup,
			UpdateFrame = Game.PauselessFrame,
		};
		FuncPool.Add(key, new FuncData() {
			Func = func,
			Key = keyData,
			Param = param,
			ShowLabel = showLabel,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
	}


	/// <summary>
	/// Reset all data.
	/// </summary>
	/// <param name="windowIndex">Force reset given window instead of current window. Set to -1 to reset all windows.</param>
	public static void ClearAll (int windowIndex = -1) {
		if (windowIndex < 0) {
			foreach (var win in Windows) {
				win.ClearAllLogic();
			}
		} else {
			Windows[CurrentWindowIndex].ClearAllLogic();
		}
	}
	private void ClearAllLogic () {
		BoolPool.Clear();
		IntPool.Clear();
		FloatPool.Clear();
		StringPool.Clear();
		PixelsPool.Clear();
		ObjectPool.Clear();
		ButtonPool.Clear();
		FuncPool.Clear();
		Keys.Clear();
		GroupFolding.Clear();
		PrevKeyCount = -1;
	}


	// Set Data
	/// <summary>
	/// Set a bool value without requiring any field.
	/// </summary>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="value"></param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static void SetBool (string key, bool value, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetBoolLogic(key, value);
	private void SetBoolLogic (string key, bool value) {
		if (BoolPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	/// <summary>
	/// Set a intager value without requiring any field.
	/// </summary>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="value"></param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static void SetInt (string key, int value, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetIntLogic(key, value);
	private void SetIntLogic (string key, int value) {
		if (IntPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	/// <summary>
	/// Set a float value without requiring any field.
	/// </summary>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="value"></param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static void SetFloat (string key, float value, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetFloatLogic(key, value);
	private void SetFloatLogic (string key, float value) {
		if (FloatPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	/// <summary>
	/// Set a string value without requiring any field.
	/// </summary>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="value"></param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static void SetString (string key, string value, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetStringLogic(key, value);
	private void SetStringLogic (string key, string value) {
		if (StringPool.TryGetValue(key, out var data)) {
			data.value = value;
		}
	}

	/// <summary>
	/// Make given group fold or unfold
	/// </summary>
	/// <param name="key">Unique key to identify the group</param>
	/// <param name="folding">True if the group should fold</param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static void SetGroupFolding (string key, bool folding, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetGroupFoldingLogic(key, folding);
	private void SetGroupFoldingLogic (string key, bool folding) {
		if (GroupFolding.TryGetValue(key, out var gData)) {
			gData.Folding = folding;
		} else {
			GroupFolding[key] = new GroupData() {
				Folding = folding,
				UpdateFrame = Game.PauselessFrame,
			};
		}
	}


	// Get Data
	/// <summary>
	/// Get a bool value without requiring any field.
	/// </summary>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="defaultValue"></param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static bool GetBool (string key, bool defaultValue = false, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GetBoolLogic(key, defaultValue);
	private bool GetBoolLogic (string key, bool defaultValue = false) => BoolPool.TryGetValue(key, out var data) ? data.value : defaultValue;

	/// <summary>
	/// Get a intager value without requiring any field.
	/// </summary>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="defaultValue"></param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static int GetInt (string key, int defaultValue = 0, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GetIntLogic(key, defaultValue);
	private int GetIntLogic (string key, int defaultValue = 0) => IntPool.TryGetValue(key, out var data) ? data.value : defaultValue;

	/// <summary>
	/// Get a float value without requiring any field.
	/// </summary>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="defaultValue"></param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static float GetFloat (string key, float defaultValue = 0, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GetFloatLogic(key, defaultValue);
	private float GetFloatLogic (string key, float defaultValue = 0f) => FloatPool.TryGetValue(key, out var data) ? data.value : defaultValue;

	/// <summary>
	/// Get a string value without requiring any field.
	/// </summary>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="defaultValue"></param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static string GetString (string key, string defaultValue = "", int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GetStringLogic(key, defaultValue);
	private string GetStringLogic (string key, string defaultValue = "") => StringPool.TryGetValue(key, out var data) ? data.value : defaultValue;


	// Pixels
	/// <summary>
	/// Start to draw 1D pixel column
	/// </summary>
	/// <param name="key">Unique key to identify the field</param>
	/// <param name="size">Horizontal size in pixel</param>
	/// <param name="clearPrevPixels">True if the existing pixels should be reset into clear</param>
	/// <param name="windowIndex">Force start inside for given window instead of current window</param>
	public static void StartDrawColumn (string key, int size, bool clearPrevPixels = true, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].StartDrawColumnLogic(key, size, clearPrevPixels);
	private void StartDrawColumnLogic (string key, int size, bool clearPrevPixels = true) => StartDrawPixelsLogic(key, size, (int)(size * 0.618f), clearPrevPixels);


	/// <summary>
	/// Start to draw 2D pixel texture
	/// </summary>
	/// <param name="key">Unique key to identify the field</param>
	/// <param name="width">Width in pixel</param>
	/// <param name="height">Height in pixel</param>
	/// <param name="clearPrevPixels">True if the existing pixels should be reset into clear</param>
	/// <param name="windowIndex">Force start inside for given window instead of current window</param>
	public static void StartDrawPixels (string key, int width, int height, bool clearPrevPixels = true, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].StartDrawPixelsLogic(key, width, height, clearPrevPixels);
	private void StartDrawPixelsLogic (string key, int width, int height, bool clearPrevPixels = true) {
		ShowingWindow = true;
		CurrentOrder++;
		if (PixelsPool.TryGetValue(key, out var result)) {
			if (clearPrevPixels) {
				Array.Clear(result.pixels);
			}
			CurrentPixels = result.pixels;
			var kData = result.Key;
			kData.Order = CurrentOrder;
			kData.UpdateFrame = Game.PauselessFrame;
			return;
		}
		CurrentPixels = new Color32[width, height];
		var keyData = new KeyData() {
			key = key,
			Order = CurrentOrder,
			Type = DataType.Pixels,
			Group = CurrentGroup,
			UpdateFrame = Game.PauselessFrame,
		};
		PixelsPool.Add(key, new PixelData() {
			pixels = CurrentPixels,
			Key = keyData,
		});
		Keys.Add(keyData);
		IgnoringWindow = false;
	}


	/// <summary>
	/// Draw a pixel column. Only call this after QTest.StartDrawColumn
	/// </summary>
	/// <param name="x">Position X in local pixel space</param>
	/// <param name="value01">Height of the column. 0 is no size, 1 is full size.</param>
	/// <param name="color">Color of this column</param>
	/// <param name="bgColor">Color of the background</param>
	/// <param name="windowIndex">Force paint into given window instead of current window</param>
	public static void DrawColumn (int x, float value01, Color32 color, Color32 bgColor, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].DrawColumnLogic(x, value01, color, bgColor);
	private void DrawColumnLogic (int x, float value01, Color32 color, Color32 bgColor) {
		int height = CurrentPixels.GetLength(1);
		int valueHeight = (height * value01).RoundToInt().Clamp(0, height);
		for (int i = 0; i < height; i++) {
			CurrentPixels[x, i] = i < valueHeight ? color : bgColor;
		}
	}

	/// <summary>
	/// Draw a pixel. Only call this after QTest.StartDrawPixels
	/// </summary>
	/// <param name="x">Position X in local pixel space</param>
	/// <param name="y">Position Y in local pixel space</param>
	/// <param name="pixel">Color of this pixel</param>
	/// <param name="windowIndex">Force paint into given window instead of current window</param>
	public static void DrawPixel (int x, int y, Color32 pixel, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].CurrentPixels[x, y] = pixel;


	/// <summary>
	/// Get png texture file byte array from given window's current drawing cache.
	/// </summary>
	/// <param name="windowIndex">Index of the window</param>
	/// <param name="texture">Result texture instance</param>
	/// <returns>The png byte array. Return null if currently don't have any pixels data</returns>
	public static byte[] GetPngByteFromPixels (int windowIndex, out object texture) => Windows[windowIndex].GetPngByteFromPixels(out texture);
	private byte[] GetPngByteFromPixels (out object texture) {
		texture = null;
		if (CurrentPixels == null || CurrentPixels.Length == 0) return null;
		int w = CurrentPixels.GetLength(0);
		int h = CurrentPixels.GetLength(1);
		int len = w * h;
		var arr = new Color32[len];
		var span = arr.GetSpan();
		for (int i = 0; i < len; i++) {
			span[i] = CurrentPixels[i % w, i / w];
		}
		texture = Game.GetTextureFromPixels(arr, w, h);
		return Game.TextureToPngBytes(texture);
	}


	// Mark
	/// <inheritdoc cref="Mark(int, int, Color32, int, int)"/>
	public static void Mark (Int2 pos, int duration = 60, int size = 42) => Marks.Add((Game.GlobalFrame, new Int3(pos.x, pos.y, Stage.ViewZ), duration, size, Color32.RED));
	/// <inheritdoc cref="Mark(int, int, Color32, int, int)"/>
	public static void Mark (int x, int y, int duration = 60, int size = 42) => Marks.Add((Game.GlobalFrame, new Int3(x, y, Stage.ViewZ), duration, size, Color32.RED));
	/// <inheritdoc cref="Mark(int, int, Color32, int, int)"/>
	public static void Mark (Int2 pos, Color32 color, int duration = 60, int size = 42) => Marks.Add((Game.GlobalFrame, new Int3(pos.x, pos.y, Stage.ViewZ), duration, size, color));
	/// <summary>
	/// Paint a circle mark at screen for given frames long
	/// </summary>
	/// <param name="pos">Position in global space</param>
	/// <param name="x">Position X in global space</param>
	/// <param name="y">Position Y in global space</param>
	/// <param name="color">Color tint of the circle</param>
	/// <param name="duration">Length in frame</param>
	/// <param name="size">Diameter in global space</param>
	public static void Mark (int x, int y, Color32 color, int duration = 60, int size = 42) => Marks.Add((Game.GlobalFrame, new Int3(x, y, Stage.ViewZ), duration, size, color));


	// Obj
	/// <summary>
	/// Create/set the value of an object in given type
	/// </summary>
	/// <typeparam name="T">Type of the object</typeparam>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="obj">Value of the object</param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	/// <returns>Instance of the object</returns>
	public static T SetObject<T> (string key, T obj, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].SetObjectLogic(key, obj);
	private T SetObjectLogic<T> (string key, T obj) => (T)(ObjectPool[key] = obj);


	/// <summary>
	/// Get an existing object data
	/// </summary>
	/// <typeparam name="T">Type of the object</typeparam>
	/// <param name="key">Unique key to identify the data</param>
	/// <param name="result">Instance of the result object</param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	/// <returns>True if the object is found</returns>
	public static bool TryGetObject<T> (string key, out T result, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].TryGetObjectLogic(key, out result);
	private bool TryGetObjectLogic<T> (string key, out T result) {
		if (ObjectPool.TryGetValue(key, out var obj) && obj is T tObj) {
			result = tObj;
			return true;
		} else {
			result = default;
			return false;
		}
	}


	// Misc
	/// <summary>
	/// Hide the testing windows
	/// </summary>
	public static void HideTest () {
		ShowingWindow = false;
		IgnoringWindow = true;
	}


	/// <summary>
	/// Show the testing windows
	/// </summary>
	public static void ShowTest () {
		ShowingWindow = true;
		IgnoringWindow = false;
	}


	/// <summary>
	/// Following field requires will be inside this group
	/// </summary>
	/// <param name="group">Name/key of the group</param>
	/// <param name="folding">True if the group is default folding</param>
	/// <param name="windowIndex">Force value set for given window instead of current window</param>
	public static void Group (string group, bool folding = false, int windowIndex = -1) => Windows[windowIndex >= 0 ? windowIndex : CurrentWindowIndex].GroupLogic(group, folding);
	private void GroupLogic (string group, bool folding = false) {
		CurrentGroup = group;
		if (!GroupFolding.TryGetValue(group, out var gData)) {
			GroupFolding[group] = gData = new GroupData();
			gData.Folding = folding;
			PrevKeyCount = 0;
		}
		if (gData.UpdateFrame != Game.PauselessFrame) {
			gData.Order = CurrentGroupOrder;
			CurrentGroupOrder++;
		}
		gData.UpdateFrame = Game.PauselessFrame;
	}


	public static void SortAllKeys () {
		for (int i = 0; i < MAX_WINDOW_COUNT; i++) {
			Windows[i].PrevKeyCount = -1;
		}
	}


	#endregion




}
#endif
