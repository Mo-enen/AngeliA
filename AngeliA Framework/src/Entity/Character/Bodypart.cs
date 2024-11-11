using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public class BodyPart (BodyPart parent, bool useLimbFlip) {


	// Const
	private static readonly System.Type BASIC_CHARACTER_TYPE = typeof(Character);

	// SUB
	public enum CoverMode { None, Covered, FullCovered }

	// Api
	public int ID { get; private set; } = 0;
	public Int4 Border { get; private set; } = default;
	public int SpritePivotX { get; private set; } = 0;
	public int SpritePivotY { get; private set; } = 0;
	public int SizeX { get; private set; } = Const.HALF;
	public int SizeY { get; private set; } = Const.HALF;
	public int FlexableSizeY { get; set; } = Const.HALF;
	public bool UseLimbFlip { get; init; } = useLimbFlip;
	public BodyPart LimbParent { get; init; } = parent;
	public bool IsFullCovered => Covered == CoverMode.FullCovered;
	public int FacingSign => Width.Sign();
	public bool FacingRight => Width > 0;

	public int GlobalX;
	public int GlobalY;
	public int X;
	public int Y;
	public int Z;
	public int Rotation;
	public int Width;
	public int Height;
	public int PivotX;
	public int PivotY;
	public bool FrontSide;
	public CoverMode Covered;
	public Color32 Tint;


	// API
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


	public IRect GetGlobalRect () {
		return new IRect(
			GlobalX - PivotX * Width / 1000 + (Width > 0 ? 0 : Width),
			GlobalY - PivotY * Height / 1000 + (Height > 0 ? 0 : Height),
			Width.Abs(), Height.Abs()
		);
	}


	public Int2 GetLocalCenter () {
		var v = new Float2(
			Width / 2f - Width * PivotX / 1000f,
			Height / 2f - Height * PivotY / 1000f
		).Rotate(-Rotation);
		return new Int2(X + (int)v.x, Y + (int)v.y);
	}


	public Int2 GetGlobalCenter () {
		var v = new Float2(
			Width / 2f - Width * PivotX / 1000f,
			Height / 2f - Height * PivotY / 1000f
		).Rotate(-Rotation);
		return new Int2(GlobalX + (int)v.x, GlobalY + (int)v.y);
	}


	public Int2 GlobalLerp (float x01, float y01) {
		var result = new Int2(GlobalX, GlobalY);
		var v = new Float2(
			x01 * Width - Width * PivotX / 1000f,
			y01 * Height - Height * PivotY / 1000f
		).Rotate(Rotation);
		result.x += (int)v.x;
		result.y += (int)v.y;
		return result;
	}


	public Int2 NaturalLerp (float x01, float y01) {
		if ((Height > 0 == Width > 0) != FrontSide) x01 = 1f - x01;
		return GlobalLerp(x01, y01);
	}


	public void LimbRotate (int rotation, int grow = 1000) {
		if (LimbParent != null) {
			Util.LimbRotate(
				ref X, ref Y, ref PivotX, ref PivotY, ref Rotation, ref Width, ref Height,
				LimbParent.X, LimbParent.Y, LimbParent.Rotation, LimbParent.Width, LimbParent.Height,
				rotation, UseLimbFlip, grow
			);
		} else {
			Util.LimbRotate(
				ref X, ref Y, ref PivotX, ref PivotY, ref Rotation, ref Width, ref Height,
				rotation, UseLimbFlip, grow
			);
		}
	}


	public void FixApproximatelyRotation () {
		if (LimbParent != null && (Rotation - LimbParent.Rotation).Abs() <= 1) {
			Rotation = LimbParent.Rotation;
		}
	}


	public void SetSpriteID (int newID) {
		ID = newID;
		Renderer.TryGetSpriteFromGroup(newID, 0, out var sprite, false, true);
		if (sprite == null) return;
		SizeX = sprite.GlobalWidth;
		SizeY = FlexableSizeY = sprite.GlobalHeight;
		Border = sprite.GlobalBorder;
		SpritePivotX = sprite.PivotX;
		SpritePivotY = sprite.PivotY;
	}


}
