using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;

public class PassOutStarParticle : Particle {

	private static readonly int TYPE_ID = typeof(PassOutStarParticle).AngeHash();
	private static readonly SpriteCode STAR_CODE = "PassOutStar";

	public override int Duration => 66;
	public override bool Loop => true;
	public Character Character => UserData as Character;

	[OnCharacterPassOut_Entity]
	internal static void OnPassOut (Entity target) {
		var rect = target.Rect;
		if (Stage.SpawnEntity(TYPE_ID, rect.CenterX(), rect.y) is Particle particle) {
			particle.UserData = target;
		}
	}

	public override void LateUpdate () {
		if (!Active) return;
		var character = Character;
		if (character == null || !character.Active || character.CharacterState != CharacterState.PassOut) {
			Active = false;
			return;
		}
		DrawStar(Duration * 0 / 3, Duration * 1 / 3);
		DrawStar(Duration * 1 / 3, Duration * 0 / 3);
		DrawStar(Duration * 2 / 3, Duration * 2 / 3);
	}

	private void DrawStar (int posOffset, int sizeOffset) {

		int sizeDuration = Duration + 24;
		int posFrame = (LocalFrame + posOffset).UMod(Duration);
		int sizeFrame = (LocalFrame + sizeOffset).UMod(sizeDuration);

		var charRect = Character.Rect;
		int centerX = charRect.x + charRect.width / 2;
		int centerY = charRect.yMax;
		const int rangeX = Const.HALF;
		const int rangeY = Const.HALF / 3;

		int x = Util.Remap(
			-1f, 1f, centerX - rangeX, centerX + rangeX,
			Util.Cos(Util.Remap(0, Duration, 0f, 360f, posFrame) * Util.Deg2Rad)
		).RoundToInt();

		int y = Util.Remap(
			-1f, 1f, centerY - rangeY, centerY + rangeY,
			Util.Sin(Util.Remap(0, Duration, 0f, 360f, posFrame) * Util.Deg2Rad)
		).RoundToInt();

		int size = Util.Remap(
			0, sizeDuration / 2,
			60, 140,
			sizeFrame.PingPong(sizeDuration / 2)
		);

		var cell = Renderer.Draw(STAR_CODE, x, y, 500, 500, 0, size, size);
		cell.Z *= posFrame < Duration / 2 ? 1 : -1;

	}

}
