namespace AngeliA;

/// <summary>
/// Represent a list of colors that creates smooth transitions between each other
/// </summary>
[System.Serializable]
public class ColorGradient {

	public (Color32 color, float time)[] Values;

	/// <summary>
	/// Represent a list of colors that creates smooth transitions between each other
	/// </summary>
	/// <param name="values">Color array</param>
	public ColorGradient (params (Color32 color, float time)[] values) => Values = values;

	/// <summary>
	/// Get the smooth color transition for given value
	/// </summary>
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