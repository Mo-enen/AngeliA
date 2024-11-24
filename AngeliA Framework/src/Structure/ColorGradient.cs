namespace AngeliA;

[System.Serializable]
public class ColorGradient (params ColorGradient.Data[] values) {
	[System.Serializable]
	public struct Data (Color32 color, float time) {
		public float time = time;
		public Color32 color = color;
	}
	public Data[] Values = values;
	public Color32 Evaluate (float time) {
		float prevTime = 0f;
		var prevColor = Color32.CLEAR;
		for (int i = 0; i < Values.Length; i++) {
			var value = Values[i];
			if (value.time > time) {
				if (i == 0) {
					return value.color;
				} else if (value.time > prevTime) {
					return Color32.LerpUnclamped(prevColor, value.color, (time - prevTime) / (value.time - prevTime));
				} else {
					return value.color;
				}
			}
			prevTime = value.time;
			prevColor = value.color;
		}
		return prevColor;
	}
}