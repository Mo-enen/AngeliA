using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public class BodyPartConfig {

		public int[] BodyParts;

		public int SuitHead;
		public int SuitBody;
		public int SuitHip;
		public int SuitHand;
		public int SuitFoot;

		public int FaceID;
		public int HairID;
		public int EarID;
		public int TailID;
		public int WingID;
		public int HornID;
	}



	public class BodyPart {


		// Api
		public int ID { get; private set; } = 0;
		public Vector4Int Border { get; private set; } = default;
		public int SpritePivotX { get; private set; } = 0;
		public int SpritePivotY { get; private set; } = 0;
		public int SizeX { get; private set; } = Const.CEL;
		public int SizeY { get; private set; } = Const.CEL;
		public bool UseLimbFlip { get; init; } = false;
		public BodyPart LimbParent { get; init; } = null;

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
		public bool FullyCovered;
		public Color32 Tint;


		// API
		public BodyPart (int id = 0, bool useLimbFlip = false, BodyPart parent = null) {
			if (CellRenderer.TryGetSpriteFromGroup(id, 0, out var sprite, false, true)) {
				SizeX = sprite.GlobalWidth;
				SizeY = sprite.GlobalHeight;
				Border = sprite.GlobalBorder;
				SpritePivotX = sprite.PivotX;
				SpritePivotY = sprite.PivotY;
			} else {
				SizeX = Const.CEL;
				SizeY = Const.CEL;
				Border = default;
				SpritePivotX = 0;
				SpritePivotY = 0;
			}
			ID = id;
			LimbParent = parent;
			UseLimbFlip = useLimbFlip;
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


		public RectInt GetGlobalRect () => new(
			GlobalX - PivotX * Width / 1000 + (Width > 0 ? 0 : Width),
			GlobalY - PivotY * Height / 1000 + (Height > 0 ? 0 : Height),
			Width.Abs(), Height.Abs()
		);


		public Vector2Int GetLocalCenter () {
			var v = Quaternion.Euler(0, 0, -Rotation) * new Vector2(
				Width / 2f - Width * PivotX / 1000f,
				Height / 2f - Height * PivotY / 1000f
			);
			return new Vector2Int(X + (int)v.x, Y + (int)v.y);
		}


		public Vector2Int GetGlobalCenter () {
			var v = Quaternion.Euler(0, 0, -Rotation) * new Vector2(
				Width / 2f - Width * PivotX / 1000f,
				Height / 2f - Height * PivotY / 1000f
			);
			return new Vector2Int(GlobalX + (int)v.x, GlobalY + (int)v.y);
		}


		public Vector2Int GlobalLerp (float x01, float y01) {
			var result = new Vector2Int(GlobalX, GlobalY);
			var v = Quaternion.Euler(0, 0, -Rotation) * new Vector2(
				x01 * Width - Width * PivotX / 1000f,
				y01 * Height - Height * PivotY / 1000f
			);
			result.x += (int)v.x;
			result.y += (int)v.y;
			return result;
		}


		public void LimbRotate (int rotation, int grow = 1000) {
			if (LimbParent != null) {
				AngeUtil.LimbRotate(
					ref X, ref Y, ref PivotX, ref PivotY, ref Rotation, ref Width, ref Height,
					LimbParent.X, LimbParent.Y, LimbParent.Rotation, LimbParent.Width, LimbParent.Height,
					rotation, UseLimbFlip, grow
				);
			} else {
				AngeUtil.LimbRotate(
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
			CellRenderer.TryGetSpriteFromGroup(newID, 0, out var sprite, false, true);
			if (sprite == null) return;
			SizeX = sprite.GlobalWidth;
			SizeY = sprite.GlobalHeight;
			Border = sprite.GlobalBorder;
			SpritePivotX = sprite.PivotX;
			SpritePivotY = sprite.PivotY;
		}


	}
}
