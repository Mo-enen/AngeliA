namespace AngeliA;
public class Cell {
	public static readonly Cell EMPTY = new() { Sprite = null, TextSprite = null, SheetIndex = -1, };
	public AngeSprite Sprite;
	public CharSprite TextSprite;
	public int SheetIndex;
	public int Order;
	public int X;
	public int Y;
	public int Z;
	public int Width;
	public int Height;
	public int Rotation;
	public float PivotX;
	public float PivotY;
	public Color32 Color;
	public Int4 Shift;
	public Alignment BorderSide;
	public void CopyFrom (Cell other) {
		Sprite = other.Sprite;
		TextSprite = other.TextSprite;
		SheetIndex = other.SheetIndex;
		X = other.X;
		Y = other.Y;
		Z = other.Z;
		Width = other.Width;
		Height = other.Height;
		Rotation = other.Rotation;
		PivotX = other.PivotX;
		PivotY = other.PivotY;
		Color = other.Color;
		BorderSide = other.BorderSide;
		Shift = other.Shift;
	}
	public Int2 LocalToGlobal (int localX, int localY) {
		int pOffsetX = (int)(PivotX * Width);
		int pOffsetY = (int)(PivotY * Height);
		if (Rotation == 0) {
			return new Int2(X + localX - pOffsetX, Y + localY - pOffsetY);
		}
		var globalCellV = new Float2(localX, localY).Rotate(Rotation);
		var globalPivotV = new Float2(pOffsetX, pOffsetY).Rotate(Rotation);
		var result = new Float2(X + globalCellV.x - globalPivotV.x, Y + globalCellV.y - globalPivotV.y);
		return result.RoundToInt();
	}
	public Int2 GlobalToLocal (int globalX, int globalY) {
		int pOffsetX = (int)(PivotX * Width);
		int pOffsetY = (int)(PivotY * Height);
		if (Rotation == 0) {
			return new Int2(globalX + pOffsetX - X, globalY + pOffsetY - Y);
		}
		var globalPoint = new Float2(pOffsetX, pOffsetY).Rotate(Rotation);
		var globalOffset = new Float2(globalX - X + globalPoint.x, globalY - Y + globalPoint.y);
		var result = globalOffset.Rotate(-Rotation);
		return result.RoundToInt();
	}
	public void ReturnPivots () {
		if (Rotation == 0) {
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
	public void ReturnPivots (float newPivotX, float newPivotY) {
		if (Rotation == 0) {
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
	public void ReturnPosition (int globalX, int globalY) {
		var localPoint = GlobalToLocal(globalX, globalY);
		PivotX = (float)localPoint.x / Width;
		PivotY = (float)localPoint.y / Height;
		X = globalX;
		Y = globalY;
	}
	public void RotateAround (int rotation, int pointX, int pointY) {
		if (rotation == 0 || Width == 0 || Height == 0) return;
		var localPoint = GlobalToLocal(pointX, pointY);
		PivotX = (float)localPoint.x / Width;
		PivotY = (float)localPoint.y / Height;
		X = pointX;
		Y = pointY;
		Rotation += rotation;
	}
	public void ScaleFrom (int scale, int pointX, int pointY) {
		var localPoint = GlobalToLocal(pointX, pointY);
		PivotX = (float)localPoint.x / Width;
		PivotY = (float)localPoint.y / Height;
		X = pointX;
		Y = pointY;
		Width = Width * scale / 1000;
		Height = Height * scale / 1000;
	}
}