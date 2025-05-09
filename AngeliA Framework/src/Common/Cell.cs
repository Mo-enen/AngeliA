﻿namespace AngeliA;


internal struct RawCell () {
	public int SpriteID = 0;
	public char TextChar = '\0';
	public int Order;
	public int X;
	public int Y;
	public int Z;
	public int Width;
	public int Height;
	public int Rotation1000;
	public float PivotX;
	public float PivotY;
	public Color32 Color = Color32.WHITE;
	public Int4 Shift;
	public Alignment BorderSide = Alignment.Full;
	public readonly IRect GetGlobalBounds () {
		if (Rotation1000.UMod(360_000) == 0) {
			int pOffsetX = (int)(PivotX * Width);
			int pOffsetY = (int)(PivotY * Height);
			return new IRect(X - pOffsetX, Y - pOffsetY, Width, Height);
		} else {
			var bl = GlobalLerp(0f, 0f);
			var br = GlobalLerp(1f, 0f);
			var tl = GlobalLerp(0f, 1f);
			var tr = GlobalLerp(1f, 1f);
			return IRect.MinMaxRect(
				Util.Min(Util.Min(bl.x, br.x), Util.Min(tl.x, tr.x)),
				Util.Min(Util.Min(bl.y, br.y), Util.Min(tl.y, tr.y)),
				Util.Max(Util.Max(bl.x, br.x), Util.Max(tl.x, tr.x)),
				Util.Max(Util.Max(bl.y, br.y), Util.Max(tl.y, tr.y))
			);
		}
	}
	private readonly Int2 GlobalLerp (float x01, float y01) {
		var result = new Int2(X, Y);
		var v = new Float2(
			x01 * Width - Width * PivotX,
			y01 * Height - Height * PivotY
		).Rotate(Rotation1000 / 1000f);
		result.x += (int)v.x;
		result.y += (int)v.y;
		return result;
	}
}


/// <summary>
/// Rendering cell
/// </summary>
public class Cell {

	public static readonly Cell EMPTY = new() { Sprite = null, TextSprite = null, SheetIndex = -1, };

	public AngeSprite Sprite;
	/// <summary>
	/// Sprite for rendering text character
	/// </summary>
	public CharSprite TextSprite { get; set; }
	/// <summary>
	/// Which sprite sheet does this cell use. -1 means main sheet.
	/// </summary>
	public int SheetIndex;
	internal int Order;
	public int X;
	public int Y;
	/// <summary>
	/// Z position for sorting
	/// </summary>
	public int Z;
	public int Width;
	public int Height;
	/// <summary>
	/// Rotation of the cell, 0 means up, 90 means right
	/// </summary>
	public int Rotation {
		get => Rotation1000 / 1000;
		set => Rotation1000 = value * 1000;
	}
	/// <summary>
	/// Actual rotation data of the cell, 0 means up, 90000 means right
	/// </summary>
	public int Rotation1000;
	/// <summary>
	/// Which place of the cell should be align with the X position. 0 means left edge, 1 means right edge.
	/// </summary>
	public float PivotX;
	/// <summary>
	/// Which place of the cell should be align with the Y position. 0 means bottom edge, 1 means top edge.
	/// </summary>
	public float PivotY;
	public Color32 Color;
	/// <summary>
	/// How many edge should be clip off. 0 means no clip off, set left into "width" means clip off all of them
	/// </summary>
	public Int4 Shift;
	/// <summary>
	/// Which side does this cell belongs to when it's rendered in 9-slice mode
	/// </summary>
	public Alignment BorderSide;

	/// <summary>
	/// Set X,Y,Width,Height with a rect
	/// </summary>
	public void SetRect (IRect rect) {
		X = rect.x;
		Y = rect.y;
		Width = rect.width;
		Height = rect.height;
	}
	public void CopyFrom (Cell other) {
		Sprite = other.Sprite;
		TextSprite = other.TextSprite;
		SheetIndex = other.SheetIndex;
		X = other.X;
		Y = other.Y;
		Z = other.Z;
		Width = other.Width;
		Height = other.Height;
		Rotation1000 = other.Rotation1000;
		PivotX = other.PivotX;
		PivotY = other.PivotY;
		Color = other.Color;
		Shift = other.Shift;
		BorderSide = other.BorderSide;
	}
	public Int2 LocalToGlobal (int localX, int localY) {
		int pOffsetX = (int)(PivotX * Width);
		int pOffsetY = (int)(PivotY * Height);
		if (Rotation1000 == 0) {
			return new Int2(X + localX - pOffsetX, Y + localY - pOffsetY);
		}
		var globalCellV = new Float2(localX, localY).Rotate(Rotation1000 / 1000f);
		var globalPivotV = new Float2(pOffsetX, pOffsetY).Rotate(Rotation1000 / 1000f);
		var result = new Float2(X + globalCellV.x - globalPivotV.x, Y + globalCellV.y - globalPivotV.y);
		return result.RoundToInt();
	}
	public Int2 GlobalToLocal (int globalX, int globalY) {
		int pOffsetX = (int)(PivotX * Width);
		int pOffsetY = (int)(PivotY * Height);
		if (Rotation1000 == 0) {
			return new Int2(globalX + pOffsetX - X, globalY + pOffsetY - Y);
		}
		var globalPoint = new Float2(pOffsetX, pOffsetY).Rotate(Rotation1000 / 1000f);
		var globalOffset = new Float2(globalX - X + globalPoint.x, globalY - Y + globalPoint.y);
		var result = globalOffset.Rotate(Rotation1000 / -1000f);
		return result.RoundToInt();
	}
	public Int2 GlobalLerp (float x01, float y01) {
		var result = new Int2(X, Y);
		var v = new Float2(
			x01 * Width - Width * PivotX,
			y01 * Height - Height * PivotY
		).Rotate(Rotation);
		result.x += (int)v.x;
		result.y += (int)v.y;
		return result;
	}
	/// <summary>
	/// Set pivot of the cell into (0,0) and let position adapt, so the final position of the cell remain the same
	/// </summary>
	public void ReturnPivots () {
		if (Rotation1000 == 0) {
			X -= (Width * PivotX).RoundToInt();
			Y -= (Height * PivotY).RoundToInt();
		} else {
			var point = LocalToGlobal(0, 0);
			X = point.x;
			Y = point.y;
		}
		PivotX = 0;
		PivotY = 0;
	}
	/// <summary>
	/// Set pivot of the cell and let position adapt, so the final position of the cell remain the same
	/// </summary>
	public void ReturnPivots (float newPivotX, float newPivotY) {
		if (Rotation1000 == 0) {
			X -= (Width * (PivotX - newPivotX)).RoundToInt();
			Y -= (Height * (PivotY - newPivotY)).RoundToInt();
		} else {
			var point = LocalToGlobal((int)(newPivotX * Width), (int)(newPivotY * Height));
			X = point.x;
			Y = point.y;
		}
		PivotX = newPivotX;
		PivotY = newPivotY;
	}
	/// <summary>
	/// Set position of the cell and let pivot adapt, so the final position of the cell remain the same
	/// </summary>
	public void ReturnPosition (int globalX, int globalY) {
		var localPoint = GlobalToLocal(globalX, globalY);
		PivotX = (float)localPoint.x / Width;
		PivotY = (float)localPoint.y / Height;
		X = globalX;
		Y = globalY;
	}
	/// <summary>
	/// Rotate the cell around the given position
	/// </summary>
	public void RotateAround (int rotation, int pointX, int pointY) {
		if (rotation == 0 || Width == 0 || Height == 0) return;
		var localPoint = GlobalToLocal(pointX, pointY);
		PivotX = (float)localPoint.x / Width;
		PivotY = (float)localPoint.y / Height;
		X = pointX;
		Y = pointY;
		Rotation += rotation;
	}
	/// <summary>
	/// Scale the cell based on the given position
	/// </summary>
	public void ScaleFrom (int scale, int pointX, int pointY) {
		var localPoint = GlobalToLocal(pointX, pointY);
		PivotX = (float)localPoint.x / Width;
		PivotY = (float)localPoint.y / Height;
		X = pointX;
		Y = pointY;
		Width = Width * scale / 1000;
		Height = Height * scale / 1000;
	}
	/// <summary>
	/// Scale the cell based on the given position
	/// </summary>
	public void ScaleFrom (float scale, int pointX, int pointY) {
		var localPoint = GlobalToLocal(pointX, pointY);
		PivotX = (float)localPoint.x / Width;
		PivotY = (float)localPoint.y / Height;
		X = pointX;
		Y = pointY;
		Width = (Width * scale).RoundToInt();
		Height = (Height * scale).RoundToInt();
	}
	/// <summary>
	/// Get the boundary occupied by the cell
	/// </summary>
	public IRect GetGlobalBounds () {
		if (Rotation1000.UMod(360_000) == 0) {
			int pOffsetX = (int)(PivotX * Width);
			int pOffsetY = (int)(PivotY * Height);
			return new IRect(X - pOffsetX, Y - pOffsetY, Width, Height);
		} else {
			var bl = GlobalLerp(0f, 0f);
			var br = GlobalLerp(1f, 0f);
			var tl = GlobalLerp(0f, 1f);
			var tr = GlobalLerp(1f, 1f);
			return IRect.MinMaxRect(
				Util.Min(Util.Min(bl.x, br.x), Util.Min(tl.x, tr.x)),
				Util.Min(Util.Min(bl.y, br.y), Util.Min(tl.y, tr.y)),
				Util.Max(Util.Max(bl.x, br.x), Util.Max(tl.x, tr.x)),
				Util.Max(Util.Max(bl.y, br.y), Util.Max(tl.y, tr.y))
			);
		}
	}

	internal RawCell GetRawData () {
		return new() {
			SpriteID = Sprite != null ? Sprite.ID : 0,
			TextChar = TextSprite != null ? TextSprite.Char : '\0',
			X = X,
			Y = Y,
			Z = Z,
			Width = Width,
			Height = Height,
			Rotation1000 = Rotation1000,
			PivotX = PivotX,
			PivotY = PivotY,
			Color = Color,
			Shift = Shift,
			BorderSide = BorderSide,
		};
	}

	internal void LoadFromRawData (RawCell data, int sheetIndex = int.MinValue) {
		if (sheetIndex == int.MinValue) {
			sheetIndex = Renderer.CurrentSheetIndex;
		}
		using var _ = new SheetIndexScope(sheetIndex);
		Renderer.TryGetSprite(data.SpriteID, out Sprite, true);
		if (data.TextChar != '\0' && Renderer.RequireCharForPool(data.TextChar, out var cSprite)) {
			TextSprite = cSprite;
		} else {
			TextSprite = null;
		}
		X = data.X;
		Y = data.Y;
		Z = data.Z;
		Width = data.Width;
		Height = data.Height;
		Rotation1000 = data.Rotation1000;
		PivotX = data.PivotX;
		PivotY = data.PivotY;
		Color = data.Color;
		Shift = data.Shift;
		BorderSide = data.BorderSide;
	}

	/// <summary>
	/// Clamp the content inside the given rect range
	/// </summary>
	public void Clamp (IRect rect) {
		var cellRect = new IRect(
			X - (int)(Width * PivotX),
			Y - (int)(Height * PivotY),
			Width, Height
		);
		cellRect.FlipNegative();
		if (cellRect.CompleteInside(rect)) return;
		if (!cellRect.Overlaps(rect)) {
			Width = 0;
			return;
		}
		// Clamp
		int cellL = cellRect.x;
		int cellR = cellRect.x + cellRect.width;
		int cellD = cellRect.y;
		int cellU = cellRect.y + cellRect.height;
		if (cellL < rect.x) {
			if (Width > 0) {
				Shift.left = rect.x - cellL;
			} else {
				Shift.right = rect.x - cellL;
			}
		}
		if (cellR > rect.x + rect.width) {
			if (Width > 0) {
				Shift.right = cellR - rect.x - rect.width;
			} else {
				Shift.left = cellR - rect.x - rect.width;
			}
		}
		if (cellD < rect.y) {
			if (Height > 0) {
				Shift.down = rect.y - cellD;
			} else {
				Shift.up = rect.y - cellD;
			}
		}
		if (cellU > rect.y + rect.height) {
			if (Height > 0) {
				Shift.up = cellU - rect.y - rect.height;
			} else {
				Shift.down = cellU - rect.y - rect.height;
			}
		}
	}

}