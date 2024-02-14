namespace AngeliA {
    [System.Serializable]
	public class ColorGradient {
		[System.Serializable]
		public struct Data {
			public float time;
			public Byte4 color;
			public Data (Byte4 color, float time) {
				this.time = time;
				this.color = color;
			}
		}
		public Data[] Values;
		public ColorGradient (params Data[] values) => Values = values;
		public Byte4 Evaluate (float time) {
			float prevTime = 0f;
			var prevColor = Const.CLEAR;
			for (int i = 0; i < Values.Length; i++) {
				var value = Values[i];
				if (value.time > time) {
					if (i == 0) {
						return value.color;
					} else if (value.time > prevTime) {
						return Byte4.LerpUnclamped(prevColor, value.color, (time - prevTime) / (value.time - prevTime));
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
}