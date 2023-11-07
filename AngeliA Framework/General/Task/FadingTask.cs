using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class FadeInTask : TaskItem {
		public static readonly int TYPE_ID = typeof(FadeInTask).AngeHash();
		private int Duration => UserData is int i ? i : 20;
		public override void OnStart () {
			base.OnStart();
			ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, true);
		}
		public override void OnEnd () {
			base.OnEnd();
			ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, false);
		}
		public override TaskResult FrameUpdate () {
			RetroDarkenEffect.SetAmount(1f - (float)LocalFrame / Duration);
			return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
		}
	}
	public class FadeOutTask : TaskItem {
		public static readonly int TYPE_ID = typeof(FadeOutTask).AngeHash();
		private int Duration => UserData is int i ? i : 20;
		public override void OnStart () {
			base.OnStart();
			ScreenEffect.SetEffectEnable(RetroDarkenEffect.TYPE_ID, true);
		}
		public override TaskResult FrameUpdate () {
			RetroDarkenEffect.SetAmount((float)LocalFrame / Duration);
			return LocalFrame < Duration ? TaskResult.Continue : TaskResult.End;
		}
	}
}
