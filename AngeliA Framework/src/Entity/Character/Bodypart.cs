using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;


namespace AngeliA;


/// <summary>
/// How cloths is covering the bodypart
/// </summary>
public enum CoverMode { None, Covered, FullCovered }


public class BodyPartTransform {
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
}


/// <summary>
/// Representation of a bodypart for a pose-style character
/// </summary>
public class BodyPart {


	// Const
	private static readonly System.Type BASIC_CHARACTER_TYPE = typeof(Character);
	internal const int BODY_PART_COUNT = 17;
	internal static readonly int[] DEFAULT_BODY_PART_ID = ["DefaultCharacter.Head".AngeHash(), "DefaultCharacter.Body".AngeHash(), "DefaultCharacter.Hip".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.Shoulder".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.UpperArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.LowerArm".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.Hand".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.UpperLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.LowerLeg".AngeHash(), "DefaultCharacter.Foot".AngeHash(), "DefaultCharacter.Foot".AngeHash(),];
	internal static readonly string[] BODY_PART_NAME = ["Head", "Body", "Hip", "Shoulder", "Shoulder", "UpperArm", "UpperArm", "LowerArm", "LowerArm", "Hand", "Hand", "UpperLeg", "UpperLeg", "LowerLeg", "LowerLeg", "Foot", "Foot",];
	internal static readonly Int2[] BODY_DEF_PIVOT = [
		new(500, 0), new(500, 0), new(500, 0),
		new(1000, 1000), new(1000, 1000), new(1000, 1000), new(0, 1000), new(1000, 1000), new(0, 1000), new(1000, 1000), new(1000, 1000),
		new(0, 1000), new(1000, 1000), new(0, 1000), new(1000, 1000), new(0, 1000), new(0, 1000),
	];
	internal static readonly bool[] REQUIRE_LIMB_ROT = [
		false,false,false,
		false,false,true,true,true,true,true,true,
		true,true,true,true,true,true,
	];

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
	public bool UseLimbFlip { get; init; }
	/// <summary>
	/// Which bodypart does this bodypart attached on. Set to null if it's not a limb
	/// </summary>
	public BodyPart LimbParent { get; init; }
	/// <summary>
	/// True if the limb apply rotation from body of the character
	/// </summary>
	public bool RotateWithBody { get; init; }
	/// <summary>
	/// True if this bodypart is totaly covered by cloth
	/// </summary>
	public bool IsFullCovered => Transform.Covered == CoverMode.FullCovered;
	/// <summary>
	/// Return 1 if this bodypart is facing right
	/// </summary>
	public int FacingSign => Transform.Width.Sign();
	/// <summary>
	/// True if this bodypart is facing right
	/// </summary>
	public bool FacingRight => Transform.Width > 0;
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
	public int X { get => Transform.X; set => Transform.X = value; }
	/// <summary>
	/// Position Y in local space
	/// </summary>
	public int Y { get => Transform.Y; set => Transform.Y = value; }
	/// <summary>
	/// Z value for sort rendering cells
	/// </summary>
	public int Z { get => Transform.Z; set => Transform.Z = value; }
	/// <summary>
	/// Angle of this bodypart
	/// </summary>
	public int Rotation { get => Transform.Rotation; set => Transform.Rotation = value; }
	/// <summary>
	/// Horizontal size of this bodypart
	/// </summary>
	public int Width { get => Transform.Width; set => Transform.Width = value; }
	/// <summary>
	/// Vertical size of this bodypart
	/// </summary>
	public int Height { get => Transform.Height; set => Transform.Height = value; }
	/// <summary>
	/// Current pivot X of this bodypart (0 means left, 1000 means right)
	/// </summary>
	public int PivotX { get => Transform.PivotX; set => Transform.PivotX = value; }
	/// <summary>
	/// Current pivot Y of this bodypart (0 means bottom, 1000 means top)
	/// </summary>
	public int PivotY { get => Transform.PivotY; set => Transform.PivotY = value; }
	/// <summary>
	/// True if the bodypart is facing front
	/// </summary>
	public bool FrontSide { get => Transform.FrontSide; set => Transform.FrontSide = value; }
	/// <summary>
	/// How this bodypart is being covered by cloths
	/// </summary>
	public CoverMode Covered { get => Transform.Covered; set => Transform.Covered = value; }
	/// <summary>
	/// Color tint
	/// </summary>
	public Color32 Tint { get => Transform.Tint; set => Transform.Tint = value; }

	public readonly BodyPartTransform Transform = new();
	private readonly int DefaultPivotX;
	private readonly int DefaultPivotY;


	// MSG
	/// <summary>
	/// Representation of a bodypart for a pose-style character
	/// </summary>
	/// <param name="parent">Which bodypart does this bodypart attached on. Set to null if it's not a limb</param>
	/// <param name="useLimbFlip">True if the limb flip horizontaly when rotate over specified angle</param>
	/// <param name="rotateWithBody">True if the limb apply rotation from body of the character</param>
	/// <param name="defaultPivotX"></param>
	/// <param name="defaultPivotY"></param>
	public BodyPart (BodyPart parent, bool useLimbFlip, bool rotateWithBody, int defaultPivotX, int defaultPivotY) {
		UseLimbFlip = useLimbFlip;
		LimbParent = parent;
		RotateWithBody = rotateWithBody;
		DefaultPivotX = defaultPivotX;
		DefaultPivotY = defaultPivotY;
	}


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
				ref Transform.X, ref Transform.Y, ref Transform.PivotX, ref Transform.PivotY, ref Transform.Rotation, ref Transform.Width, ref Transform.Height,
				LimbParent.X, LimbParent.Y, LimbParent.Rotation, LimbParent.Width, LimbParent.Height,
				rotation, UseLimbFlip, grow
			);
		} else {
			FrameworkUtil.LimbRotate(
				ref Transform.X, ref Transform.Y, ref Transform.PivotX, ref Transform.PivotY, ref Transform.Rotation, ref Transform.Width, ref Transform.Height,
				rotation, UseLimbFlip, grow
			);
		}
	}


	internal void FixApproximatelyRotation () {
		if (LimbParent != null && (Rotation - LimbParent.Rotation).Abs() <= 1) {
			Rotation = LimbParent.Rotation;
		}
	}


	internal void SetPivotToDefault () {
		PivotX = DefaultPivotX;
		PivotY = DefaultPivotY;
	}


}
