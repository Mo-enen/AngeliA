using System.Collections;
using System.Collections.Generic;
using AngeliA.Platformer;
using AngeliA;

namespace MarioTemplate;

[NoItemCombination]
[EntityAttribute.MapEditorGroup("Entity")]
public class Track : AngeliA.Platformer.Track {

	// Const
	private static readonly SpriteCode BODY_SP = "Track.Body";
	private static readonly SpriteCode BODY_TILT = "Track.Tilt";
	private static readonly SpriteCode BODY_CENTER = "Track.Center";

	// Api
	protected override int HangGap => 0;
	protected override SpriteCode BodySprite => BODY_SP;
	protected override SpriteCode BodyTiltSprite => BODY_TILT;
	protected override SpriteCode CenterSprite => BODY_CENTER;


}
