using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class SleepParticle : Particle {


	private static readonly int TYPE_ID = typeof(SleepParticle).AngeHash();
	public override int Duration => 120;
	public override bool Loop => false;
	private static int GlobalShift = 0;


	[OnCharacterSleeping_Entity]
	internal static void OnSleeping (Entity target) {
		var rect = target.Rect;
		Stage.TrySpawnEntity(TYPE_ID, rect.CenterX(), rect.CenterY(), out _);
	}


	public override void OnActivated () {
		base.OnActivated();
		Width = 0;
		Height = 0;
		X += GlobalShift * 12 + Const.HALF;
		Y += Const.HALF;
		GlobalShift = (GlobalShift + 1) % 3;
		Scale = 800;
	}


	public override void LateUpdate () {
		base.LateUpdate();
		int frame = LocalFrame;
		X += (frame + X).PingPong(40) / (frame / 12 + 3);
		Y += (frame + Y + 16).PingPong(40) / (frame / 12 + 3);
		Tint = new(255, 255, 255, (byte)Util.Remap(0, Duration, 600, 0, frame).Clamp(0, 255));
	}


}
