using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.JordanPeck;

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


	public NoiseMatching SetMatching (int index, NoiseMatching matching) => Matchings[index] = matching;


	public float GetNoise (float x, float y, float z) {
		float finalValue = Noises[0].GetNoise(x, y, z);
		for (int i = 1; i < Length; i++) {
			float value = Noises[i].GetNoise(x, y, z);
			switch (Matchings[i]) {
				case NoiseMatching.Plus:
					finalValue += value;
					break;
				case NoiseMatching.Minus:
					finalValue -= value;
					break;
				case NoiseMatching.Multiply:
					finalValue *= value;
					break;
				case NoiseMatching.Divide:
					finalValue /= Util.Max(value, 0.000001f);
					break;
			}
		}
		return finalValue;
	}


	#endregion




	#region --- LGC ---



	#endregion




}
