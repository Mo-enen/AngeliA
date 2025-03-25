using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// Wing body gadget for pose style character
/// </summary>
public abstract class Wing : BodyGadget {




	#region --- VAR ---


	// Api
	/// <summary>
	/// True if this wing is in tail-propeller style (like character "Tails" from Sonic)
	/// </summary>
	public bool IsPropeller { get; private set; } = false;
	public override bool SpriteLoaded => SpriteWing.IsValid;
	public sealed override BodyGadgetType GadgetType => BodyGadgetType.Wing;
	/// <summary>
	/// Size scale for the wing (0 means 0%, 1000 means 100%)
	/// </summary>
	protected virtual int Scale => 1000;
	private OrientedSprite SpriteWing;


	#endregion




	#region --- MSG ---


	public override void DrawGadget (PoseCharacterRenderer renderer) {

		if (!SpriteLoaded) return;
		using var _ = new SheetIndexScope(SheetIndex);

		DrawSpriteAsWing(renderer, SpriteWing, IsPropeller, Scale);
		if (IsPropeller && renderer.TargetCharacter.AnimationType == CharacterAnimationType.Fly) {
			renderer.TailID.Override(0, 1, priority: 4096);
		}
	}


	public override void DrawGadgetGizmos (IRect rect, Color32 tint, int z) {
		if (SpriteWing.TryGetSpriteForGizmos(out var sprite)) {
			if (IsPropeller) {
				Renderer.Draw(sprite, rect.Fit(sprite), tint, z);
			} else {
				Renderer.Draw(sprite, rect.LeftHalf().Fit(sprite), tint, z);
				Renderer.Draw(sprite, rect.RightHalf().Fit(sprite).GetFlipHorizontal(), tint, z);
			}
		}
	}


	#endregion




	#region --- API ---


	public override bool FillFromSheet (string name) {
		base.FillFromSheet(name);
		SpriteWing = new OrientedSprite(name, "Wing", "Propeller");
		IsPropeller = SpriteWing.AttachmentName == "Propeller";
		return SpriteLoaded;
	}


	/// <summary>
	/// Draw gadget for given character
	/// </summary>
	public static void DrawGadgetFromPool (PoseCharacterRenderer renderer) {
		if (renderer.WingID != 0 && TryGetGadget(renderer.WingID, out var wing)) {
			wing.DrawGadget(renderer);
		}
	}


	/// <summary>
	/// Draw given artwork sprite as wing for given character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="oSprite">Artwork sprite</param>
	/// <param name="isPropeller">True if this wing is in propeller style (like character "Tails" from Sonic)</param>
	/// <param name="scale">Size scale (0 means 0%, 1000 means 100%)</param>
	public static void DrawSpriteAsWing (PoseCharacterRenderer renderer, OrientedSprite oSprite, bool isPropeller, int scale = 1000) {

		if (!oSprite.IsValid) return;

		var body = renderer.Body;
		if (!oSprite.TryGetSprite(
			body.FrontSide, body.Width > 0, renderer.CurrentAnimationFrame, out var sprite
		)) return;

		var singleSprite = oSprite.TryGetSprite(
			body.FrontSide, body.Width > 0, 0, out var firstSprite
		) ? firstSprite : sprite;

		var aniType = renderer.TargetCharacter.AnimationType;
		int z = body.FrontSide ? -33 : 33;
		int signY = aniType == CharacterAnimationType.Rolling && !body.FrontSide ? -1 : 1;

		// Get Wing Position
		int xLeft;
		int yLeft;
		int xRight;
		int yRight;
		if (
			aniType != CharacterAnimationType.Sleep &&
			aniType != CharacterAnimationType.PassOut &&
			aniType != CharacterAnimationType.Fly
		) {
			// Standing Up
			var bodyRect = body.GetGlobalRect();
			xLeft = bodyRect.xMin;
			yLeft = bodyRect.y;
			xRight = bodyRect.xMax;
			yRight = bodyRect.y;
		} else {
			// Lying Down
			xLeft = renderer.UpperLegL.GlobalX;
			yLeft = renderer.UpperLegL.GlobalY;
			xRight = renderer.UpperLegR.GlobalX;
			yRight = renderer.UpperLegR.GlobalY;
		}

		if (aniType == CharacterAnimationType.Fly) {
			// Flying
			if (isPropeller) {
				// Propeller
				Renderer.Draw(
					sprite,
					(xLeft + xRight) / 2,
					(yLeft + yRight) / 2,
					sprite.PivotX, sprite.PivotY, 0,
					sprite.GlobalWidth * scale / 1000,
					sprite.GlobalHeight * scale / 1000,
					z
				);

			} else {
				// Wings
				Renderer.Draw(
					sprite,
					xLeft, yLeft, sprite.PivotX, sprite.PivotY, 0,
					sprite.GlobalWidth * scale / 1000,
					signY * sprite.GlobalHeight * scale / 1000,
					z
				);
				Renderer.Draw(
					sprite,
					xRight, yRight, sprite.PivotX, sprite.PivotY, 0,
					-sprite.GlobalWidth * scale / 1000,
					signY * sprite.GlobalHeight * scale / 1000,
					z
				);
			}
		} else if (!isPropeller) {
			// Not Flying with Wing
			int rot = (Game.GlobalFrame.PingPong(120) - 60) / 12;
			int rotBodyOffset = body.Rotation / 2;
			int signW = body.Width.Sign3();
			int facingScaleL = 1000;
			int facingScaleR = 1000;
			if (body.FrontSide) {
				var mov = renderer.TargetCharacter.Movement;
				int facingSclAmount = ((Game.GlobalFrame - mov.LastFacingChangeFrame) * 20).Clamp(0, 200);
				facingScaleL = 1000 + signW * facingSclAmount;
				facingScaleR = 1000 - signW * facingSclAmount;
			}
			// L
			Renderer.Draw(
				singleSprite,
				xLeft, yLeft, singleSprite.PivotX, singleSprite.PivotY, -rot - rotBodyOffset,
				singleSprite.GlobalWidth * scale / 1000 * facingScaleL / 1000,
				signY * singleSprite.GlobalHeight * scale / 1000,
				z
			);
			// R
			Renderer.Draw(
				singleSprite,
				xRight, yRight, singleSprite.PivotX, singleSprite.PivotY, rot - rotBodyOffset,
				-singleSprite.GlobalWidth * scale / 1000 * facingScaleR / 1000,
				signY * singleSprite.GlobalHeight * scale / 1000,
				z
			);
		}

	}


	/// <summary>
	/// True if the given wing is a propeller style wing
	/// </summary>
	public static bool IsPropellerWing (int wingID) => wingID != 0 && TryGetGadget(wingID, out var gadget) && gadget is Wing wing && wing.IsPropeller;


	#endregion




}
