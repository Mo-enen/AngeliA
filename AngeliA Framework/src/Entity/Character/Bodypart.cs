using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

/// <summary>
/// Representation of a bodypart for a pose style character
/// </summary>
/// <param name="parent">Which bodypart does this bodypart attached on. Set to null if it's not a limb</param>
/// <param name="useLimbFlip">True if the limb flip horizontaly when rotate over specified angle</param>
/// <param name="rotateWithBody">True if the limb apply rotation from body of the character</param>
public class BodyPart (BodyPart parent, bool useLimbFlip, bool rotateWithBody) {


	// Const
	private static readonly System.Type BASIC_CHARACTER_TYPE = typeof(Character);

	// SUB
	/// <summary>
	/// How cloths is covering the bodypart
	/// </summary>
	public enum CoverMode { None, Covered, FullCovered }

	// Api
	/// <summary>
	/// Global unique id for this bodypart
	/// </summary>
	public int ID { get; private set; } = 0;
	/// <summary>
	/// Artwork sprite border
	/// </summary>
	public Int4 Border { get; private set; } = default;
	/// <summary>
	/// Artwork sprite pivot X
	/// </summary>
	public int SpritePivotX { get; private set; } = 0;
	/// <summary>
	/// Artwork sprite pivot Y
	/// </summary>
	public int SpritePivotY { get; private set; } = 0;
	/// <summary>
	/// Artwork sprite Width in global size
	/// </summary>
	public int SizeX { get; private set; } = Const.HALF;
	/// <summary>
	/// Artwork sprite Height in global size
	/// </summary>
	public int SizeY { get; private set; } = Const.HALF;
	/// <summary>
	/// Height that changes with character's body height
	/// </summary>
	public int FlexableSizeY { get; set; } = Const.HALF;
	/// <summary>
	/// True if the limb flip horizontaly when rotate over specified angle	
	/// </summary>
	public bool UseLimbFlip { get; init; } = useLimbFlip;
	/// <summary>
	/// Which bodypart does this bodypart attached on. Set to null if it's not a limb
	/// </summary>
	public BodyPart LimbParent { get; init; } = parent;
	/// <summary>
	/// True if the limb apply rotation from body of the character
	/// </summary>
	public bool RotateWithBody { get; init; } = rotateWithBody;
	/// <summary>
	/// True if this bodypart is totaly covered by cloth
	/// </summary>
	public bool IsFullCovered => Covered == CoverMode.FullCovered;
	/// <summary>
	/// Return 1 if this bodypart is facing right
	/// </summary>
	public int FacingSign => Width.Sign();
	/// <summary>
	/// True if this bodypart is facing right
	/// </summary>
	public bool FacingRight => Width > 0;

	/// <summary>
	/// Position X in global space
	/// </summary>
	public int GlobalX;
	/// <summary>
	/// Position Y in global space
	/// </summary>
	public int GlobalY;
	/// <summary>
	/// Position X in local space
	/// </summary>
	public int X;
	/// <summary>
	/// Position Y in local space
	/// </summary>
	public int Y;
	/// <summary>
	/// Z value for sort rendering cells
	/// </summary>
	public int Z;
	/// <summary>
	/// Angle of this bodypart
	/// </summary>
	public int Rotation;
	/// <summary>
	/// Horizontal size of this bodypart
	/// </summary>
	public int Width;
	/// <summary>
	/// Vertical size of this bodypart
	/// </summary>
	public int Height;
	/// <summary>
	/// Current pivot X of this bodypart (0 means left, 1000 means right)
	/// </summary>
	public int PivotX;
	/// <summary>
	/// Current pivot Y of this bodypart (0 means bottom, 1000 means top)
	/// </summary>
	public int PivotY;
	/// <summary>
	/// True if the bodypart is facing front
	/// </summary>
	public bool FrontSide;
	/// <summary>
	/// How this bodypart is being covered by cloths
	/// </summary>
	public CoverMode Covered;
	/// <summary>
	/// Color tint
	/// </summary>
	public Color32 Tint;

	// API
	/// <summary>
	/// Get sprite id to rendering bodypart for given type of character
	/// </summary>
	/// <param name="characterType">Target type of character</param>
	/// <param name="bodyPartName">Basic name of this bodypart</param>
	/// <param name="checkForGroup">True if the sprite can be get from sprite group</param>
	/// <param name="id">Global ID of the result bodypart</param>
	/// <returns>True if the id is found</returns>
	public static bool TryGetSpriteIdFromSheet (System.Type characterType, string bodyPartName, bool checkForGroup, out int id) {
		id = 0;
		var type = characterType;
		while (true) {
			int newID = $"{type.AngeName()}.{bodyPartName}".AngeHash();
			if (checkForGroup) {
				if (Renderer.HasSpriteGroup(newID) || Renderer.HasSprite(newID)) {
					id = newID;
					return true;
				}
			} else if (Renderer.HasSprite(newID)) {
				id = newID;
				return true;
			}
			type = type.BaseType;
			if (type == null || type == BASIC_CHARACTER_TYPE) break;
		}
		return false;
	}


	/// <summary>
	/// Set bodypart data by giving a sprite ID
	/// </summary>
	public void SetData (int id) {
		if (Renderer.TryGetSpriteFromGroup(id, 0, out var sprite, false, true)) {
			SizeX = sprite.GlobalWidth;
			SizeY = FlexableSizeY = sprite.GlobalHeight;
			Border = sprite.GlobalBorder;
			SpritePivotX = sprite.PivotX;
			SpritePivotY = sprite.PivotY;
		} else {
			SizeX = Const.HALF;
			SizeY = FlexableSizeY = Const.HALF;
			Border = default;
			SpritePivotX = 0;
			SpritePivotY = 0;
		}
		ID = id;
	}


	/// <summary>
	/// Copy motion data from another bodypart without changing anything about data about source sprite
	/// </summary>
	public void Imitate (BodyPart other) {
		X = other.X;
		Y = other.Y;
		Rotation = other.Rotation;
		Width = other.Width;
		Height = other.Height;
		PivotX = other.PivotX;
		PivotY = other.PivotY;
		FrontSide = other.FrontSide;
		Tint = other.Tint;
	}


	/// <summary>
	/// Get current position rect in global space
	/// </summary>
	/// <returns></returns>
	public IRect GetGlobalRect () {
		return new IRect(
			GlobalX - PivotX * Width / 1000 + (Width > 0 ? 0 : Width),
			GlobalY - PivotY * Height / 1000 + (Height > 0 ? 0 : Height),
			Width.Abs(), Height.Abs()
		);
	}


	/// <summary>
	/// Get current center position in local space
	/// </summary>
	public Int2 GetLocalCenter () {
		var v = new Float2(
			Width / 2f - Width * PivotX / 1000f,
			Height / 2f - Height * PivotY / 1000f
		).Rotate(-Rotation);
		return new Int2(X + (int)v.x, Y + (int)v.y);
	}


	/// <summary>
	/// Get current center position in global space
	/// </summary>
	public Int2 GetGlobalCenter () {
		var v = new Float2(
			Width / 2f - Width * PivotX / 1000f,
			Height / 2f - Height * PivotY / 1000f
		).Rotate(-Rotation);
		return new Int2(GlobalX + (int)v.x, GlobalY + (int)v.y);
	}


	/// <summary>
	/// Get global position that lerp from given value
	/// </summary>
	/// <param name="x01">X lerp value (0 means left, 1 means right)</param>
	/// <param name="y01">Y lerp value (0 means bottom, 1 means top)</param>
	/// <param name="natural">True if this lerping logic respect character's natural orientation (like Tokino Sora's hairpin should always on her left side)</param>
	public Int2 GlobalLerp (float x01, float y01, bool natural = false) {
		if (natural && (Height > 0 == Width > 0) != FrontSide) {
			x01 = 1f - x01;
		}
		var result = new Int2(GlobalX, GlobalY);
		var v = new Float2(
			x01 * Width - Width * PivotX / 1000f,
			y01 * Height - Height * PivotY / 1000f
		).Rotate(Rotation);
		result.x += (int)v.x;
		result.y += (int)v.y;
		return result;
	}


	/// <summary>
	/// Rotate the bodypart with "LimbRotate" logic
	/// </summary>
	/// <param name="rotation"></param>
	/// <param name="grow">How much does the limb grow it's size from the rotation (0 means don't grow. 1000 means general amount)</param>
	public void LimbRotate (int rotation, int grow = 1000) {
		if (LimbParent != null) {
			FrameworkUtil.LimbRotate(
				ref X, ref Y, ref PivotX, ref PivotY, ref Rotation, ref Width, ref Height,
				LimbParent.X, LimbParent.Y, LimbParent.Rotation, LimbParent.Width, LimbParent.Height,
				rotation, UseLimbFlip, grow
			);
		} else {
			FrameworkUtil.LimbRotate(
				ref X, ref Y, ref PivotX, ref PivotY, ref Rotation, ref Width, ref Height,
				rotation, UseLimbFlip, grow
			);
		}
	}


	internal void FixApproximatelyRotation () {
		if (LimbParent != null && (Rotation - LimbParent.Rotation).Abs() <= 1) {
			Rotation = LimbParent.Rotation;
		}
	}


}
