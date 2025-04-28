#if DEBUG
using System.Collections;
using System.Collections.Generic;
using JordanPeck;

namespace AngeliA;

internal class NoiseMaker : QMaker {


	// SUB
	private enum ViewMode { Value, Hue, Solid, }

	// VAR
	protected override string CheatCode => "Noise";
	protected override string Name => "NoiseMaker";
	protected override string FileExtension => "noise";
	protected override bool RequireFilePanel => true;
	protected override bool RequireViewPanel => true;
	protected override bool UseViewZoomUI => false;

	private const int VIEW_SIZE = Const.MAP;
	private const int SLOT_COUNT = 3;
	private readonly FastNoiseGroup MakingGroup = new(SLOT_COUNT);
	private readonly float[] FREQ_SCL = [0.01f, 0.1f, 1f, 10f, 100f];
	private readonly Float2[] NoiseRangeCache = new Float2[SLOT_COUNT];
	private ViewMode CurrentViewMode = ViewMode.Value;
	private string NoiseDataRoot;
	private bool Step;
	private float ValueRange0;


	// MSG
	protected override void OnActivated () {
		base.OnActivated();
		NoiseDataRoot = Util.CombinePaths(Universe.BuiltIn.UniverseMetaRoot, "Noise");
		Util.CreateFolder(NoiseDataRoot);
		foreach (var filePath in Util.EnumerateFiles(NoiseDataRoot, true, "*")) {
			string name = Util.GetNameWithoutExtension(filePath);
			string noiseSourcePath = Util.CombinePaths(FileRoot, $"{name}.{FileExtension}");
			if (!Util.FileExists(noiseSourcePath)) {
				Util.DeleteFile(filePath);
			}
		}
	}


	protected override void OnGUI () {

		// All Slots
		for (int slot = 0; slot < SLOT_COUNT; slot++) {

			QTest.SetCurrentWindow(slot, $"Slot {slot}");

			var noise = MakingGroup[slot];

			// Enable
			if (slot > 0) {
				bool enable = QTest.Bool("Enable", true);
				if (!enable) continue;
			}

			// General
			QTest.Group("General");

			// Seed
			noise.Seed = QTest.Int("Seed", 7300, 0, 100000, 100);

			// Frequency
			int freqScale = QTest.Int("FrequencyScale", 1, 0, 4);
			float freq = QTest.Float("Frequency", 0.2f, 0.1f, 1f, step: 0.1f);
			noise.Frequency = freq * FREQ_SCL[freqScale];

			// Min / Max
			noise.Min = QTest.Float("Min", 0f, -8f, 8f, step: 0.1f);
			noise.Max = QTest.Float("Max", 1f, -8f, 8f, step: 0.1f);

			// Noise Type
			noise.NoiseType = (NoiseType)QTest.Int(
				"Noise Type", 0, 0, 5,
				displayLabel: noise.NoiseType.ToString()
			);

			// Rotate Type
			noise.RotationType3D = ((RotationType3D)QTest.Int(
				"Rotation Type", 0, 0, 2,
				displayLabel: noise.RotationType3D.ToString()
			));

			// Cellular
			if (noise.NoiseType == NoiseType.Cellular) {
				QTest.Group("Cellular", folding: true);

				noise.CellularDistanceFunction = (CellularDistanceFunction)QTest.Int(
					"Cellular Dis-Func", 1, 0, 3,
					displayLabel: noise.CellularDistanceFunction.ToString()
				);
				noise.CellularReturnType = (CellularReturnType)QTest.Int(
					"Cellular Return Type", 1, 0, 6,
					displayLabel: noise.CellularReturnType.ToString()
				);
				noise.CellularJitterModifier = QTest.Float(
					"Cellular Jitter", 1.0f, 0f, 5f, 0.1f
				);
			}

			// Fractal
			QTest.Group("Fractal", folding: true);

			noise.FractalType = ((FractalType)QTest.Int(
				"Fractal Type", 3, 0, 5,
				displayLabel: noise.FractalType.ToString()
			));
			if (noise.FractalType != FractalType.None) {
				int oct = QTest.Int(
					"Fractal Octaves", 3, 1, 6
				);
				noise.Octaves = oct;
				if (oct > 1) {
					noise.Gain = QTest.Float(
						"Fractal Gain", 0.5f, 0f, 1f, 0.1f
					);
					noise.Lacunarity = QTest.Float(
						"Fractal Lacunarity", 2.0f, 0f, 10f, 0.1f
					);
					if (noise.FractalType != FractalType.DomainWarpIndependent && noise.FractalType != FractalType.DomainWarpProgressive) {
						noise.WeightedStrength = QTest.Float(
							"Fractal WeightedStrength", 0f, 0f, 2f, 0.1f
						);
					}
				}
				if (noise.FractalType == FractalType.PingPong) {
					noise.PingPongStrength = QTest.Float(
						"Fractal PingPongStrength", 2.0f, 0f, 5f, 0.1f
					);
				}
			}

			// Domain Warp
			QTest.Group("Domain Warp", folding: true);
			noise.DomainWarpAmp = QTest.Int("AMP", 0, -400, 400, step: 10);
			if (noise.DomainWarpAmp != 0) {
				noise.DomainWarpType = ((DomainWarpType)QTest.Int(
					"Domain Warp Type", 0, 0, 2,
					displayLabel: noise.DomainWarpType.ToString()
				));
			}

			// View
			QTest.Group("");
			QTest.StartDrawPixels("View", VIEW_SIZE, VIEW_SIZE, clearPrevPixels: false);
			int zoom = Zoom;
			float _z = CurrentPos.z;
			for (int j = 0; j < VIEW_SIZE; j++) {
				float _y = CurrentPos.y + j * zoom;
				for (int i = 0; i < VIEW_SIZE; i++) {
					float _x = CurrentPos.x + i * zoom;
					float noiseValue = noise.GetNoise(_x, _y, _z);
					float finalValue = Util.InverseLerpUnclamped(noise.Min, noise.Max, noiseValue);
					byte rgb = (byte)(finalValue * 256f).Clamp(0, 255);
					QTest.DrawPixel(i, j, new Color32(rgb, rgb, rgb, 255));
				}
			}

		}

	}


	protected override void ViewPanelGUI () {

		QTest.SetCurrentWindow(QTest.MAX_WINDOW_COUNT - 2, "View");
		QTest.Group("");

		// View Mode
		CurrentViewMode = (ViewMode)QTest.Int("View Mode", 0, 0, 2, displayLabel: CurrentViewMode.ToString());

		// ViewMode
		Step = false;
		switch (CurrentViewMode) {
			case ViewMode.Value:
				Step = QTest.Bool("Step", false);
				break;
			case ViewMode.Hue:
				Step = QTest.Bool("Step", false);
				break;
			case ViewMode.Solid:
				MakingGroup.SolidMin = QTest.Float("Solid Min", -8f, -8f, 8f, 0.1f);
				MakingGroup.SolidMax = QTest.Float("Solid Max", 0.5f, -8f, 8f, 0.1f);
				break;
		}

		// Final View
		var _noise0 = MakingGroup[0];
		ValueRange0 = _noise0.Max - _noise0.Min;
		ValueRange0 = ValueRange0.AlmostZero() ? 0.000001f : ValueRange0;

		// Noise >> Cache
		for (int i = 0; i < SLOT_COUNT; i++) {
			var noise = MakingGroup[i];
			NoiseRangeCache[i] = new(noise.Min, noise.Max);
			if (!QTest.GetBool("Enable", true, i)) {
				noise.Min = 0;
				noise.Max = 0;
			}
		}

		// Render
		base.ViewPanelGUI();

		// Cache >> Noise
		for (int i = 0; i < SLOT_COUNT; i++) {
			var noise = MakingGroup[i];
			(noise.Min, noise.Max) = NoiseRangeCache[i];
		}

	}


	protected override Color32 GetViewColor (float x, float y, float z, int i, int j) {

		var _noise0 = MakingGroup[0];
		float value = MakingGroup.GetNoise(x, y, z);

		// Draw Pixel
		Color32 color;
		switch (CurrentViewMode) {
			default:
			case ViewMode.Value:
				byte rgb = (byte)((Step ? value.RoundToInt() : value) * 256f).Clamp(0, 255);
				color = new Color32(rgb, rgb, rgb, 255);
				break;
			case ViewMode.Hue:
				float hueValue = Step ? (value - _noise0.Min).FloorToInt() : value - _noise0.Min;
				color = Util.HsvToRgb(hueValue / ValueRange0, 1f, 1f);
				break;
			case ViewMode.Solid:
				color = MakingGroup.IsSolid(value) ?
					new Color32(28, 25, 24) :
					new Color32(56, 187, 228);
				break;
		}

		return color;
	}


	protected override void SaveData () {
		base.SaveData();
		string basicName = string.IsNullOrWhiteSpace(SaveName) ? Util.GetTimeString() : SaveName;
		string dataPath = Util.CombinePaths(NoiseDataRoot, basicName);
		FastNoiseGroup.SaveToFile(MakingGroup, dataPath);
	}


	protected override (object texture, byte[] png) GetCurrentThumbnail () {
		var pngBytes = QTest.GetPngByteFromPixels(QTest.MAX_WINDOW_COUNT - 2, out var icon);
		return (icon, pngBytes);
	}


}
#endif
