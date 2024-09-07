using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public static class LightingSystem {




	#region --- VAR ---


	// Const
	private const int LIGHT_MAP_UNIT_PADDING = 10;
	private const int LIGHT_MAP_UNIT_PADDING_BOTTOM = 6;
	private const int LIGHT_MAP_UNIT_PADDING_TOP = 6;
	private static readonly float[] WEIGHTS = { 0.071f, 0.19f, 0.51f, 0.19f, 0.071f, };

	// Data
	private static bool Enable = true;
	private static float[,] Illuminances;
	private static int CellWidth;
	private static int CellHeight;
	private static int OriginUnitX;
	private static int OriginUnitY;
	private static int WeightLen;
	private static float CameraScale = 1f;
	private static int ForceCameraScaleFrame = -1;
	private static float ForceAirLerpValue = 1f;
	private static int ForceAirLerpFrame = -1;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Enable = !Game.IsToolApplication && Universe.BuiltInInfo.UseLightingSystem;
		if (!Enable) return;
		int maxHeight = Universe.BuiltInInfo.MaxViewHeight;
		CellWidth = Universe.BuiltInInfo.ViewRatio * maxHeight / 1000 / Const.CEL + LIGHT_MAP_UNIT_PADDING * 2;
		CellHeight = maxHeight / Const.CEL + LIGHT_MAP_UNIT_PADDING_TOP + LIGHT_MAP_UNIT_PADDING_BOTTOM;
		Illuminances = new float[CellWidth, CellHeight];
		WeightLen = WEIGHTS.Length;
	}


	[OnGameUpdate(-63)]
	internal static void CalculateAllIlluminance () {

		if (!Enable || !WorldSquad.Enable) return;

		var info = Universe.BuiltInInfo;
		OriginUnitX = Stage.ViewRect.x.ToUnit() - LIGHT_MAP_UNIT_PADDING;
		OriginUnitY = Stage.ViewRect.y.ToUnit() - LIGHT_MAP_UNIT_PADDING_BOTTOM;
		float solidIllu = info.LightMap_SolidIlluminance;
		float airIllu = Util.LerpUnclamped(
			info.LightMap_AirIlluminanceNight,
			info.LightMap_AirIlluminanceDay,
			Sky.InGameDaytime01
		);
		float sLerp = info.LightMap_SelfLerp;
		float bgIllu = info.LightMap_BackgroundTint;

		// First Row
		int originUnitTop = OriginUnitY + CellHeight - 1;
		for (int i = 0; i < CellWidth; i++) {
			Illuminances[i, CellHeight - 1] = GetSelfIlluminanceAt(OriginUnitX + i, originUnitTop, solidIllu, airIllu);
		}

		// Mix Iteration
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

		var info = Universe.BuiltInInfo;
		bool scaling = Game.PauselessFrame <= ForceCameraScaleFrame;
		bool lerpingToAir = Game.PauselessFrame <= ForceAirLerpFrame;
		float airIllu = 256f - 256f * Util.LerpUnclamped(
			info.LightMap_AirIlluminanceNight,
			info.LightMap_AirIlluminanceDay,
			Sky.InGameDaytime01
		);
		bool pixelStyle = info.LightMap_PixelStyle;

		var cameraCenter = Renderer.CameraRect.center.CeilToInt();
		var cameraRect = Renderer.CameraRect;
		int left = cameraRect.x.ToUnit() - 1 - OriginUnitX;
		int right = cameraRect.xMax.ToUnit() + 1 - OriginUnitX;
		int down = cameraRect.y.ToUnit() - 1 - OriginUnitY;
		int up = cameraRect.yMax.ToUnit() + 1 - OriginUnitY;
		if (scaling) {
			left -= LIGHT_MAP_UNIT_PADDING;
			right += LIGHT_MAP_UNIT_PADDING;
			down -= LIGHT_MAP_UNIT_PADDING_BOTTOM;
			up += LIGHT_MAP_UNIT_PADDING_TOP;
		}
		left = left.Clamp(0, CellWidth - 1);
		right = right.Clamp(0, CellWidth - 1);
		down = down.Clamp(0, CellHeight - 1);
		up = up.Clamp(0, CellHeight - 1);

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
						scaling ? rect.ScaleFrom(CameraScale, cameraCenter.x, cameraCenter.y) : rect,
						new Color32(
							0, 0, 0,
							(byte)(lerpingToAir ? Util.LerpUnclamped(Illuminances[i, j], airIllu, ForceAirLerpValue) : Illuminances[i, j])
						)
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
					float tl = (alphaTL + alphaTM + alphaML + alphaMM) / 4f;
					float tr = (alphaTM + alphaTR + alphaMM + alphaMR) / 4f;
					float bl = (alphaML + alphaMM + alphaBL + alphaBM) / 4f;
					float br = (alphaMM + alphaMR + alphaBM + alphaBR) / 4f;
					if (lerpingToAir) {
						tl = Util.LerpUnclamped(tl, airIllu, ForceAirLerpValue);
						tr = Util.LerpUnclamped(tr, airIllu, ForceAirLerpValue);
						bl = Util.LerpUnclamped(bl, airIllu, ForceAirLerpValue);
						br = Util.LerpUnclamped(br, airIllu, ForceAirLerpValue);
					}
					byte aTL = (byte)tl;
					byte aTR = (byte)tr;
					byte aBL = (byte)bl;
					byte aBR = (byte)br;
					if (aTL == 0 && aTR == 0 && aBL == 0 && aBR == 0) continue;
					rect.x = offsetX + i * Const.CEL;
					Game.DrawGizmosRect(
						scaling ? rect.ScaleFrom(CameraScale, cameraCenter.x, cameraCenter.y) : rect,
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

		float remain = Universe.BuiltInInfo.LightMap_LevelIlluminateRemain.Clamp01();
		float radiusSq = (unitRadius + 0.5f) * (unitRadius + 0.5f);
		float amountF = amount / 1000f;
		for (int j = down; j <= up; j++) {
			for (int i = left; i <= right; i++) {
				float disSq = Util.SquareDistanceF(i, j, localX, localY);
				if (disSq > radiusSq) continue;
				float add = amountF * (radiusSq - disSq) / radiusSq;
				if (HasSolidBlockAt(i + OriginUnitX, j + OriginUnitY, out _)) {
					add *= remain;
				}
				Illuminances[i, j] += add;
			}
		}

	}


	public static void ForceCameraScale (float scale, int duration = 1) {
		ForceCameraScaleFrame = Game.PauselessFrame + duration;
		CameraScale = scale;
	}


	public static void ForceAirLerp (float lerp, int duration = 1) {
		ForceAirLerpFrame = Game.PauselessFrame + duration;
		ForceAirLerpValue = lerp;
	}


	#endregion




	#region --- LGC ---


	private static float GetSelfIlluminanceAt (int unitX, int unitY, float solidIllu, float airIllu) {
		if (HasSolidBlockAt(unitX, unitY, out var hitRect)) {
			// Level
			return 1f - (float)hitRect.width / Const.CEL / solidIllu;
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


	private static bool HasSolidBlockAt (int unitX, int unitY, out IRect rect) {
		if (Physics.Overlap(
			PhysicsMask.LEVEL,
			new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
			out var hit
		)) {
			rect = hit.Rect;
			return true;
		} else {
			rect = default;
			return false;
		}
	}


	#endregion




}
