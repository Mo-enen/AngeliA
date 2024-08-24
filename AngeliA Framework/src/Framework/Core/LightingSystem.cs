using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class LightingSystem {




	#region --- SUB ---



	#endregion




	#region --- VAR ---


	// Const
	private const int LIGHT_MAP_UNIT_PADDING = 10;
	private const int LIGHT_MAP_UNIT_PADDING_DOWN = 2;
	private static readonly float[] WEIGHTS = { 0.04f, 0.10f, 0.50f, 0.10f, 0.04f, };
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

		if (!Enable) return;

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
			Illuminances[i, j] = GetSelfIlluminanceAt(OriginUnitX + i, originUnitTop, out _);
		}

		// Mix Iteration
		j = CellHeight - 2;
		for (; j >= 0; j--) {
			int unitY = OriginUnitY + j;
			for (int i = 0; i < CellWidth; i++) {
				float selfIllu = GetSelfIlluminanceAt(OriginUnitX + i, unitY, out bool solid);
				//float topIllu = Illuminances[i, j + 1];
				float topW = GetTopWeight(i, j);




				//Illuminances[i, j] =;
			}
		}

	}


	private static void RenderAllIlluminance () {







	}


	#endregion




	#region --- API ---





	#endregion




	#region --- LGC ---


	private static float GetSelfIlluminanceAt (int unitX, int unitY, out bool solid) {
		if (Physics.Overlap(
			PhysicsMask.LEVEL,
			new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL),
			out var hit
		)) {
			solid = true;
			const float CEL = Const.CEL;
			return 1f - hit.Rect.width / CEL;
		} else {
			solid = false;
			return 1f;
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
		return illu / (right - left + 1);
	}


	#endregion




}
