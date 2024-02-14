namespace AngeliA {
	public static class Ease {


		public enum EaseType : byte { InLiner, OutLiner, InOutLiner, InQuad, OutQuad, InOutQuad, InCubic, OutCubic, InOutCubic, InQuart, OutQuart, InOutQuart, InQuint, OutQuint, InOutQuint, InSine, OutSine, InOutSine, InCirc, OutCirc, InOutCirc, InElastic, OutElastic, InOutElastic, InExpo, OutExpo, InOutExpo, InBack, OutBack, InOutBack, InBounce, OutBounce, InOutBounce, }


		public static float Invoke (EaseType type, float ease) => type switch {
			EaseType.InLiner => InLiner(ease),
			EaseType.OutLiner => OutLiner(ease),
			EaseType.InOutLiner => InOutLiner(ease),
			EaseType.InQuad => InQuad(ease),
			EaseType.OutQuad => OutQuad(ease),
			EaseType.InOutQuad => InOutQuad(ease),
			EaseType.InCubic => InCubic(ease),
			EaseType.OutCubic => OutCubic(ease),
			EaseType.InOutCubic => InOutCubic(ease),
			EaseType.InQuart => InQuart(ease),
			EaseType.OutQuart => OutQuart(ease),
			EaseType.InOutQuart => InOutQuart(ease),
			EaseType.InQuint => InQuint(ease),
			EaseType.OutQuint => OutQuint(ease),
			EaseType.InOutQuint => InOutQuint(ease),
			EaseType.InSine => InSine(ease),
			EaseType.OutSine => OutSine(ease),
			EaseType.InOutSine => InOutSine(ease),
			EaseType.InCirc => InCirc(ease),
			EaseType.OutCirc => OutCirc(ease),
			EaseType.InOutCirc => InOutCirc(ease),
			EaseType.InElastic => InElastic(ease),
			EaseType.OutElastic => OutElastic(ease),
			EaseType.InOutElastic => InOutElastic(ease),
			EaseType.InExpo => InExpo(ease),
			EaseType.OutExpo => OutExpo(ease),
			EaseType.InOutExpo => InOutExpo(ease),
			EaseType.InBack => InBack(ease),
			EaseType.OutBack => OutBack(ease),
			EaseType.InOutBack => InOutBack(ease),
			EaseType.InBounce => InBounce(ease),
			EaseType.OutBounce => OutBounce(ease),
			EaseType.InOutBounce => InOutBounce(ease),
			_ => 0f,
		};


		public static float InLiner (float x) => x;
		public static float OutLiner (float x) => 1f - x;
		public static float InOutLiner (float x) => x < 0.5f ? x * 2f : 2f - 2f * x;

		public static float InQuad (float x) => x * x;
		public static float OutQuad (float x) => 1f - (1f - x) * (1f - x);
		public static float InOutQuad (float x) => x < 0.5f ? 2f * x * x : 1f - Util.Pow(-2f * x + 2f, 2f) / 2f;

		public static float InCubic (float x) => x * x * x;
		public static float OutCubic (float x) => 1f - Util.Pow(1f - x, 3f);
		public static float InOutCubic (float x) => x < 0.5f ? 4f * x * x * x : 1f - Util.Pow(-2f * x + 2f, 3f) / 2f;

		public static float InQuart (float x) => x * x * x * x;
		public static float OutQuart (float x) => 1f - Util.Pow(1f - x, 4f);
		public static float InOutQuart (float x) => x < 0.5f ? 8f * x * x * x * x : 1f - Util.Pow(-2f * x + 2f, 4f) / 2f;

		public static float InQuint (float x) => x * x * x * x * x;
		public static float OutQuint (float x) => 1f - Util.Pow(1f - x, 5f);
		public static float InOutQuint (float x) => x < 0.5f ? 16f * x * x * x * x * x : 1f - Util.Pow(-2f * x + 2f, 5f) / 2f;




		public static float InSine (float x) => 1f - Util.Cos(x * Util.PI / 2f);
		public static float OutSine (float x) => Util.Sin((x * Util.PI) / 2f);
		public static float InOutSine (float x) => -(Util.Cos(Util.PI * x) - 1f) / 2f;

		public static float InCirc (float x) => 1f - Util.Sqrt(1f - Util.Pow(x, 2f));
		public static float OutCirc (float x) => Util.Sqrt(1f - Util.Pow(x - 1f, 2f));
		public static float InOutCirc (float x) => x < 0.5f
			  ? (1f - Util.Sqrt(1f - Util.Pow(2f * x, 2f))) / 2f
			  : (Util.Sqrt(1f - Util.Pow(-2f * x + 2f, 2f)) + 1f) / 2f;

		public static float InElastic (float x) {
			const float c4 = (2f * Util.PI) / 3f;
			return x.AlmostZero() ? 0 : x.Almost(1f) ? 1f :
				-Util.Pow(2f, 10f * x - 10f) * Util.Sin((x * 10f - 10.75f) * c4);
		}
		public static float OutElastic (float x) {
			const float c4 = (2f * Util.PI) / 3f;
			return x.AlmostZero() ? 0 : x.Almost(1f) ? 1f :
			   Util.Pow(2f, -10f * x) * Util.Sin((x * 10f - 0.75f) * c4) + 1f;
		}
		public static float InOutElastic (float x) {
			const float c5 = (2f * Util.PI) / 4.5f;
			return x.AlmostZero() ? 0 : x.Almost(1f) ? 1f :
			  x < 0.5 ?
				-(Util.Pow(2f, 20f * x - 10f) * Util.Sin((20f * x - 11.125f) * c5)) / 2f :
				 (Util.Pow(2f, -20f * x + 10f) * Util.Sin((20f * x - 11.125f) * c5)) / 2f + 1f;
		}

		public static float InExpo (float x) => x == 0 ? 0 : Util.Pow(2f, 10f * x - 10f);
		public static float OutExpo (float x) => x.Almost(1f) ? 1f : 1f - Util.Pow(2f, -10f * x);
		public static float InOutExpo (float x) => x.AlmostZero() ? 0 : x.Almost(1f) ? 1f :
			x < 0.5f ? Util.Pow(2f, 20f * x - 10f) / 2f :
			(2f - Util.Pow(2f, -20f * x + 10f)) / 2f;

		public static float InBack (float x) {
			const float c1 = 1.70158f;
			const float c3 = c1 + 1f;
			return c3 * x * x * x - c1 * x * x;
		}
		public static float OutBack (float x) {
			const float c1 = 1.70158f;
			const float c3 = c1 + 1;
			return 1f + c3 * Util.Pow(x - 1f, 3f) + c1 * Util.Pow(x - 1f, 2f);
		}
		public static float InOutBack (float x) {
			const float c1 = 1.70158f;
			const float c2 = c1 * 1.525f;
			return x < 0.5f ?
				Util.Pow(2f * x, 2f) * ((c2 + 1f) * 2f * x - c2) / 2f :
				(Util.Pow(2f * x - 2f, 2f) * ((c2 + 1f) * (x * 2f - 2f) + c2) + 2f) / 2f;
		}

		public static float InBounce (float x) => 1f - OutBounce(1f - x);
		public static float OutBounce (float x) {
			const float n1 = 7.5625f;
			const float d1 = 2.75f;
			if (x < 1f / d1) {
				return n1 * x * x;
			} else if (x < 2f / d1) {
				return n1 * (x -= 1.5f / d1) * x + 0.75f;
			} else if (x < 2.5f / d1) {
				return n1 * (x -= 2.25f / d1) * x + 0.9375f;
			} else {
				return n1 * (x -= 2.625f / d1) * x + 0.984375f;
			}
		}
		public static float InOutBounce (float x) => x < 0.5f ?
			(1f - OutBounce(1f - 2f * x)) / 2f :
			(1f + OutBounce(2f * x - 1f)) / 2f;



	}
}