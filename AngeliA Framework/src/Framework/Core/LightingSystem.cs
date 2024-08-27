using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public static class LightingSystem {




	#region --- VAR ---


	// Const
	private const int LIGHT_MAP_UNIT_PADDING = 10;
	private const int LIGHT_MAP_UNIT_PADDING_DOWN = 2;
	private const int LIGHT_MAP_UNIT_PADDING_TOP = 1;
	private static readonly float[] WEIGHTS = { 0.071f, 0.19f, 0.51f, 0.19f, 0.071f, };

	// Api
	public static bool Enable { get; private set; } = true;
	public static bool PixelStyle {
		get => s_PixelStyle.Value;
		set => s_PixelStyle.Value = value;
	}
	public static float SelfLerp {
		get => s_SelfLerp.Value / 1000f;
		set => s_SelfLerp.Value = (int)(value * 1000);
	}
	public static float SolidIlluminance {
		get => s_SolidIlluminance.Value / 1000f;
		set => s_SolidIlluminance.Value = (int)(value * 1000);
	}
	public static float AirIlluminance {
		get => s_AirIlluminance.Value / 1000f;
		set => s_AirIlluminance.Value = (int)(value * 1000);
	}
	public static float BackgroundTint {
		get => s_BackgroundTint.Value / 1000f;
		set => s_BackgroundTint.Value = (int)(value * 1000);
	}

	// Data
	private static float[,] Illuminances;
	private static int CellWidth;
	private static int CellHeight;
	private static int OriginUnitX;
	private static int OriginUnitY;
	private static int WeightLen;

	// Saving
	private static readonly SavingBool s_PixelStyle = new("LightSys.PixelStyle", false, SavingLocation.Global);
	private static readonly SavingInt s_SelfLerp = new("LightSys.SelfLerp", 900, SavingLocation.Global);
	private static readonly SavingInt s_SolidIlluminance = new("LightSys.SolidIllu", 1050, SavingLocation.Global);
	private static readonly SavingInt s_AirIlluminance = new("LightSys.AirIllu", 800, SavingLocation.Global);
	private static readonly SavingInt s_BackgroundTint = new("LightSys.BgTint", 800, SavingLocation.Global);


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Enable = !Game.IsToolApplication && Universe.BuiltInInfo.UseLightingSystem;
		if (!Enable) return;
		int maxHeight = Universe.BuiltInInfo.MaxViewHeight;
		CellWidth = Universe.BuiltInInfo.ViewRatio * maxHeight / 1000 / Const.CEL + LIGHT_MAP_UNIT_PADDING * 2;
		CellHeight = maxHeight / Const.CEL + LIGHT_MAP_UNIT_PADDING_TOP + LIGHT_MAP_UNIT_PADDING_DOWN;
		Illuminances = new float[CellWidth, CellHeight];
		WeightLen = WEIGHTS.Length;
	}


	[OnGameUpdate(-63)]
	internal static void CalculateAllIlluminance () {

		if (!Enable || !WorldSquad.Enable) return;

		OriginUnitX = Stage.ViewRect.x.ToUnit() - LIGHT_MAP_UNIT_PADDING;
		OriginUnitY = Stage.ViewRect.y.ToUnit() - LIGHT_MAP_UNIT_PADDING_DOWN;
		float solidIllu = SolidIlluminance;
		float airIllu = AirIlluminance;

		// First Row
		int originUnitTop = OriginUnitY + CellHeight - 1;
		for (int i = 0; i < CellWidth; i++) {
			Illuminances[i, CellHeight - 1] = GetSelfIlluminanceAt(OriginUnitX + i, originUnitTop, solidIllu, airIllu);
		}

		// Mix Iteration
		float sLerp = SelfLerp;
		for (int j = CellHeight - 2; j >= 0; j--) {
			int unitY = OriginUnitY + j;
			for (int i = 0; i < CellWidth; i++) {
				Illuminances[i, j] = Util.Lerp(
					GetSelfIlluminanceAt(OriginUnitX + i, unitY, solidIllu, airIllu),
					GetTopWeight(i, j),
					sLerp
				);
			}
		}

		// Tint for BG
		float bgIllu = BackgroundTint;
		for (int j = CellHeight - 2; j >= 0; j--) {
			int unitY = OriginUnitY + j;
			for (int i = 0; i < CellWidth; i++) {
				if (WorldSquad.Front.GetBlockAt(OriginUnitX + i, unitY, BlockType.Background) != 0) {
					Illuminances[i, j] *= bgIllu;
				}
			}
		}

	}


	[OnGameUpdateLater(4096)]
	internal static void RenderAllIlluminance () {

		if (!Enable || !WorldSquad.Enable) return;

		var cameraRect = Renderer.CameraRect;
		int left = (cameraRect.x.ToUnit() - 1 - OriginUnitX).Clamp(0, CellWidth - 1);
		int right = (cameraRect.xMax.ToUnit() + 1 - OriginUnitX).Clamp(0, CellWidth - 1);
		int down = (cameraRect.y.ToUnit() - 1 - OriginUnitY).Clamp(0, CellHeight - 1);
		int up = (cameraRect.yMax.ToUnit() + 1 - OriginUnitY).Clamp(0, CellHeight - 1);
		bool pixelStyle = PixelStyle;

		// Illuminance >> Alpha
		for (int j = down; j <= up; j++) {
			for (int i = left; i <= right; i++) {
				Illuminances[i, j] = 255 - (Illuminances[i, j] * 255f).Clamp(0f, 255f);
			}
		}

		// Draw Alpha
		var rect = new IRect(0, 0, Const.CEL, Const.CEL);
		int offsetX = OriginUnitX.ToGlobal();
		int offsetY = OriginUnitY.ToGlobal();
		if (pixelStyle) {
			// Pixel
			for (int j = down + 1; j <= up - 1; j++) {
				rect.y = offsetY + j * Const.CEL;
				for (int i = left + 1; i <= right - 1; i++) {
					rect.x = offsetX + i * Const.CEL;
					Game.DrawGizmosRect(
						rect,
						new Color32(0, 0, 0, (byte)Illuminances[i, j])
					);
				}
			}
		} else {
			// Smooth
			for (int j = down + 1; j <= up - 1; j++) {
				rect.y = offsetY + j * Const.CEL;
				for (int i = left + 1; i <= right - 1; i++) {
					float alphaTL = Illuminances[i - 1, j + 1];
					float alphaTM = Illuminances[i, j + 1];
					float alphaTR = Illuminances[i + 1, j + 1];
					float alphaML = Illuminances[i - 1, j];
					float alphaMM = Illuminances[i, j];
					float alphaMR = Illuminances[i + 1, j];
					float alphaBL = Illuminances[i - 1, j - 1];
					float alphaBM = Illuminances[i, j - 1];
					float alphaBR = Illuminances[i + 1, j - 1];
					byte aTL = (byte)((alphaTL + alphaTM + alphaML + alphaMM) / 4f);
					byte aTR = (byte)((alphaTM + alphaTR + alphaMM + alphaMR) / 4f);
					byte aBL = (byte)((alphaML + alphaMM + alphaBL + alphaBM) / 4f);
					byte aBR = (byte)((alphaMM + alphaMR + alphaBM + alphaBR) / 4f);
					if (aTL == 0 && aTR == 0 && aBL == 0 && aBR == 0) continue;
					rect.x = offsetX + i * Const.CEL;
					Game.DrawGizmosRect(
						rect,
						new Color32(0, 0, 0, aTL),
						new Color32(0, 0, 0, aTR),
						new Color32(0, 0, 0, aBL),
						new Color32(0, 0, 0, aBR)
					);
				}
			}
		}

	}


	#endregion




	#region --- API ---


	public static void Illuminate (int unitX, int unitY, int unitRadius, int amount = 1000) {

		if (!Enable || unitRadius <= 0) return;

		int localX = unitX - OriginUnitX;
		int localY = unitY - OriginUnitY;

		int left = (localX - unitRadius).Clamp(0, CellWidth - 1);
		int right = (localX + unitRadius).Clamp(0, CellWidth - 1);
		int down = (localY - unitRadius).Clamp(0, CellHeight - 1);
		int up = (localY + unitRadius).Clamp(0, CellHeight - 1);

		if (right < left || up < down) return;

		float radiusSq = (unitRadius + 0.5f) * (unitRadius + 0.5f);
		float pointX = localX;
		float pointY = localY;
		float amountF = amount / 1000f;
		for (int j = down; j <= up; j++) {
			for (int i = left; i <= right; i++) {
				float disSq = Util.SquareDistanceF(i, j, pointX, pointY);
				if (disSq > radiusSq) continue;
				Illuminances[i, j] += amountF * (radiusSq - disSq) / radiusSq;
			}
		}

	}


	#endregion




	#region --- LGC ---


	private static float GetSelfIlluminanceAt (int unitX, int unitY, float solidIllu, float airIllu) {
		if (Physics.Overlap(
			PhysicsMask.LEVEL,
			new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
			out var hit
		)) {
			// Level
			return 1f - (float)hit.Rect.width / Const.CEL / solidIllu;
		} else {
			// Air
			return airIllu;
		}
	}


	private static float GetTopWeight (int localX, int localY) {
		int realLeft = localX - WeightLen / 2;
		int left = Util.Max(realLeft, 0);
		int right = Util.Min(localX + WeightLen / 2, CellWidth - 1);
		int j = localY + 1;
		float illu = 0f;
		for (int i = left; i <= right; i++) {
			illu += Illuminances[i, j] * WEIGHTS[i - realLeft];
		}
		return illu;
	}


	#endregion




}
