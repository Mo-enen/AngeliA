using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// Horn body gadget for pose style character
/// </summary>
public abstract class Horn : BodyGadget {





	#region --- VAR ---


	// Api
	public sealed override BodyGadgetType GadgetType => BodyGadgetType.Horn;
	public override bool SpriteLoaded => SpriteHornLeft.IsValid || SpriteHornRight.IsValid;
	/// <summary>
	/// True if the horn grows from character's face (like Ayame from Hololive)
	/// </summary>
	protected virtual bool AnchorOnFace => false;
	private OrientedSprite SpriteHornLeft;
	private OrientedSprite SpriteHornRight;


	#endregion




	#region --- MSG ---


	public override void DrawGadget (PoseCharacterRenderer renderer) {
		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);
		DrawSpriteAsHorn(
			renderer,
			SpriteHornLeft, SpriteHornRight,
			FrontOfHeadL(renderer),
			FrontOfHeadR(renderer),
			AnchorOnFace
		);
	}


	public override void DrawGadgetGizmos (IRect rect, Color32 tint, int z) {
		if (SpriteHornLeft.TryGetSpriteForGizmos(out var spriteL)) {
			Renderer.Draw(spriteL, rect.LeftHalf().Fit(spriteL).Shift(-rect.width / 10, 0), tint, z);
		}
		if (SpriteHornRight.TryGetSpriteForGizmos(out var spriteR)) {
			Renderer.Draw(spriteR, rect.RightHalf().Fit(spriteR).Shift(rect.width / 10, 0), tint, z);
		}
	}


	#endregion




	#region --- API ---


	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteHornLeft = new OrientedSprite(name, "HornLeft", "Horn");
		SpriteHornRight = new OrientedSprite(name, "HornRight", "Horn");
		return SpriteLoaded;
	}


	/// <summary>
	/// True if the left horn should render in front of character's head
	/// </summary>
	protected virtual bool FrontOfHeadL (PoseCharacterRenderer renderer) => true;


	/// <summary>
	/// True if the right horn should render in front of character's head
	/// </summary>
	protected virtual bool FrontOfHeadR (PoseCharacterRenderer renderer) => true;


	/// <summary>
	/// Draw horn gadget for given character
	/// </summary>
	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.HornID != 0 && TryGetGadget(renderer.HornID, out var horn)) {
			horn.DrawGadget(renderer);
		}
	}


	/// <summary>
	/// Draw given sprites as horn for given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="spriteLeft">Artwork sprite for left horn</param>
	/// <param name="spriteRight">Artwork sprite for right horn</param>
	/// <param name="frontOfHeadL">True if the left horn should render in front of character's head</param>
	/// <param name="frontOfHeadR">True if the right horn should render in front of character's head</param>
	/// <param name="onFace">True if the horn grows from character's face (like Ayame from Hololive)</param>
	public static void DrawSpriteAsHorn (
		PoseCharacterRenderer renderer, OrientedSprite spriteLeft, OrientedSprite spriteRight,
		bool frontOfHeadL = true, bool frontOfHeadR = true, bool onFace = false
	) {

		if (!spriteLeft.IsValid && !spriteRight.IsValid) return;
		var head = renderer.Head;
		if (head.Tint.a == 0) return;

		var headRect = head.GetGlobalRect();
		if (onFace) headRect = headRect.Shrink(head.Border);

		bool flipLR = !head.FrontSide && head.Height > 0;
		spriteLeft.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var spriteL);
		spriteRight.TryGetSprite(head.FrontSide, head.Width > 0, renderer.CurrentAnimationFrame, out var spriteR);
		if (flipLR) {
			(spriteL, spriteR) = (spriteR, spriteL);
		}

		// Twist
		int twist = renderer.HeadTwist;
		int twistWidth = 0;
		if (twist != 0) {
			twistWidth -= 16 * twist.Abs() / 500;
		}

		// Rotate
		if (spriteL != null) {
			var cell = Renderer.Draw(
				spriteL,
				headRect.xMin,
				head.Height > 0 ? headRect.yMax : headRect.yMin,
				spriteL.PivotX, spriteL.PivotY, 0,
				spriteL.GlobalWidth * (flipLR ? -1 : 1) + twistWidth,
				head.Height.Sign3() * spriteL.GlobalHeight,
				head.Z + (head.FrontSide == frontOfHeadL ? 34 : -34)
			);
			if (head.Rotation != 0) {
				cell.RotateAround(head.Rotation, head.GlobalX, head.GlobalY);
			}
		}

		if (spriteR != null) {
			var cell = Renderer.Draw(
				spriteR,
				headRect.xMax,
				head.Height > 0 ? headRect.yMax : headRect.yMin,
				spriteR.PivotX, spriteR.PivotY, 0,
				spriteR.GlobalWidth * (flipLR ? -1 : 1) + twistWidth,
				head.Height.Sign3() * spriteR.GlobalHeight,
				head.Z + (head.FrontSide == frontOfHeadR ? 34 : -34)
			);
			if (head.Rotation != 0) {
				cell.RotateAround(head.Rotation, head.GlobalX, head.GlobalY);
			}
		}

	}


	#endregion




}