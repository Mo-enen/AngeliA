using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class LightingSystem {




	#region --- SUB ---



	#endregion




	#region --- VAR ---


	// Const
	private const float SELF_LERP = 0.9f;
	private const float SOLID_ILLU = 1.1f;
	private const float AIR_ILLU = 0.8f;
	private const int LIGHT_MAP_UNIT_PADDING = 10;
	private const int LIGHT_MAP_UNIT_PADDING_DOWN = 2;
	private static readonly float[] WEIGHTS = { 0.051f, 0.21f, 0.51f, 0.21f, 0.051f, };
	private static int CellWidth;
	private static int CellHeight;

	// Api
	public static bool Enable { get; private set; } = true;

	// Data
	private static float[,] Illuminances;
	private static int OriginUnitX;
	private static int OriginUnitY;
	private static int WeightLen;


	#endregion




	#region --- MSG ---


	[OnGameInitialize]
	internal static void OnGameInitialize () {
		Enable = !Game.IsToolApplication && Universe.BuiltInInfo.UseLightingSystem;
		if (!Enable) return;
		int maxHeight = Universe.BuiltInInfo.MaxViewHeight;
		CellWidth = Universe.BuiltInInfo.ViewRatio * maxHeight / 1000 / Const.CEL + LIGHT_MAP_UNIT_PADDING * 2;
		CellHeight = maxHeight / Const.CEL + LIGHT_MAP_UNIT_PADDING + LIGHT_MAP_UNIT_PADDING_DOWN;
		Illuminances = new float[CellWidth, CellHeight];
		WeightLen = WEIGHTS.Length;
	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {

		if (!Enable || !WorldSquad.Enable) return;

		OriginUnitX = Stage.ViewRect.x.ToUnit() - LIGHT_MAP_UNIT_PADDING;
		OriginUnitY = Stage.ViewRect.y.ToUnit() - LIGHT_MAP_UNIT_PADDING_DOWN;

		CalculateAllIlluminance();
		RenderAllIlluminance();

	}


	private static void CalculateAllIlluminance () {

		// First Row
		int j = CellHeight - 1;
		int originUnitTop = OriginUnitY + CellHeight - 1;
		for (int i = 0; i < CellWidth; i++) {
			Illuminances[i, j] = GetSelfIlluminanceAt(OriginUnitX + i, originUnitTop);
		}

		// Mix Iteration
		j = CellHeight - 2;
		for (; j >= 0; j--) {
			int unitY = OriginUnitY + j;
			for (int i = 0; i < CellWidth; i++) {
				float selfIllu = GetSelfIlluminanceAt(OriginUnitX + i, unitY);
				float topW = GetTopWeight(i, j);
				Illuminances[i, j] = Util.Lerp(selfIllu, topW, SELF_LERP);
			}
		}

	}


	private static void RenderAllIlluminance () {

		using var _ = new LayerScope(RenderLayer.MULT);
		var rect = new IRect(0, 0, Const.CEL, Const.CEL);
		int offsetX = OriginUnitX.ToGlobal();
		int offsetY = OriginUnitY.ToGlobal();
		var cameraRect = Renderer.CameraRect;
		int left = (cameraRect.x.ToUnit() - 1 - OriginUnitX).Clamp(0, CellWidth - 1);
		int right = (cameraRect.xMax.ToUnit() + 1 - OriginUnitX).Clamp(0, CellWidth - 1);
		int down = (cameraRect.y.ToUnit() - 1 - OriginUnitY).Clamp(0, CellHeight - 1);
		int up = (cameraRect.yMax.ToUnit() + 1 - OriginUnitY).Clamp(0, CellHeight - 1);

		// Illuminance >> Alpha
		for (int j = down; j <= up; j++) {
			rect.y = offsetY + j * Const.CEL;
			for (int i = left; i <= right; i++) {
				float illu = Illuminances[i, j];
				Illuminances[i, j] = 255 - (illu * 255f).Clamp(0f, 255f);
			}
		}

		// Draw Alpha
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


	#endregion




	#region --- API ---





	#endregion




	#region --- LGC ---


	private static float GetSelfIlluminanceAt (int unitX, int unitY) {
		if (Physics.Overlap(
			PhysicsMask.LEVEL,
			new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
			out var hit
		)) {
			return 1f - (float)hit.Rect.width / Const.CEL / SOLID_ILLU;
		} else {
			return AIR_ILLU;
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
