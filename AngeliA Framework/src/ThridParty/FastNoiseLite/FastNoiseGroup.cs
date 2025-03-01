using System.Collections;
using System.Collections.Generic;
using System.Text;
using JordanPeck;

namespace AngeliA;


public enum NoiseMatching {
	Plus, Minus, Multiply, Divide, Disable,
}


public class FastNoiseGroup (int length) {




	#region --- VAR ---


	// Api
	public readonly int Length = length;
	public FastNoiseLite this[int index] => Noises[index];

	// Data
	private readonly FastNoiseLite[] Noises = new FastNoiseLite[length].FillWithNewValue();
	private readonly NoiseMatching[] Matchings = new NoiseMatching[length];


	#endregion




	#region --- API ---


	public void SetMatching (int index, NoiseMatching matching) => Matchings[index] = matching;


	public NoiseMatching GetMatching (int index) => Matchings[index];


	public float GetNoise (float x, float y, float z) {
		float finalValue = Noises[0].GetNoise(x, y, z);
		for (int i = 1; i < Length; i++) {
			switch (Matchings[i]) {
				case NoiseMatching.Plus:
					finalValue += Noises[i].GetNoise(x, y, z);
					break;
				case NoiseMatching.Minus:
					finalValue -= Noises[i].GetNoise(x, y, z);
					break;
				case NoiseMatching.Multiply:
					finalValue *= Noises[i].GetNoise(x, y, z);
					break;
				case NoiseMatching.Divide:
					finalValue /= Util.Max(Noises[i].GetNoise(x, y, z), 0.000001f);
					break;
			}
		}
		return finalValue;
	}


	public void FillAltitude (int[,] altitude, float left, float down, float z, float deltaX, float deltaY, out int altitudeMax) {
		altitudeMax = 0;
		if (Length == 0) return;
		int width = altitude.GetLength(0);
		int height = altitude.GetLength(1);
		for (int i = 0; i < width; i++) {
			float x = left + i * deltaX;
			int currentAltitude = 0;
			int currentRegion = -1;
			for (int j = 0; j < height; j++) {
				float y = down + j * deltaY;
				float value = GetNoise(x, y, z);
				int regionIndex = (int)(value).UFloor(1f);
				if (regionIndex != currentRegion) {
					if (currentRegion == -1) {
						// First Block
						currentAltitude = 0;
						for (int safe = 1; safe < 2048; safe++) {
							float valueFirst = GetNoise(x, y - safe * deltaY, z);
							int indexFirst = (int)(valueFirst).UFloor(1f);
							if (regionIndex != indexFirst) break;
							currentAltitude++;
						}
					} else {
						// Region Changed
						currentAltitude = 0;
					}
					currentRegion = regionIndex;
				} else {
					// Same Region
					currentAltitude++;
				}
				altitudeMax = Util.Max(altitudeMax, currentAltitude);
				altitude[i, j] = currentAltitude;
			}
		}
	}


	public string GetCSharpCode (string groupName) {

		int activeSlotCount = 0;
		for (int i = 0; i < Noises.Length; i++) {
			if (Matchings[i] != NoiseMatching.Disable) activeSlotCount++;
		}
		if (activeSlotCount == 0) return "";

		var builder = new StringBuilder();

		builder.AppendLine();
		builder.AppendLine($"public static FastNoiseGroup Create{groupName} () {{");
		builder.AppendLine("\t");
		builder.AppendLine($"\tvar group = new FastNoiseGroup({activeSlotCount});");
		builder.AppendLine("\t");
		builder.AppendLine("\tFastNoiseLite noise;");
		builder.AppendLine("\t");

		int index = 0;
		for (int i = 0; i < Noises.Length; i++) {
			var match = Matchings[i];
			if (match == NoiseMatching.Disable) continue;
			builder.AppendLine("\t");
			builder.AppendLine($"\t// Noise {index}");
			if (index != 0) {
				builder.AppendLine($"\tgroup.SetMatching({index}, NoiseMatching.{match});");
			}
			builder.AppendLine($"\tnoise = group[{index}];");
			Noises[i].AppendCSharpCode(builder, "noise", 1);
			builder.AppendLine("\t");
			index++;
		}

		builder.AppendLine("\treturn group;");
		builder.AppendLine("}");
		builder.AppendLine();

		return builder.ToString();
	}


	#endregion




	#region --- LGC ---



	#endregion




}
