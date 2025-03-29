using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AngeliA;

/// <summary>
/// Artwork sprite data for rendering the game
/// </summary>
public class AngeSprite {

	// VAR
	public static readonly AngeSprite EMPTY = new();
	private static readonly StringBuilder CacheBuilder = new(256);

	/// <summary>
	/// Global unique ID for this sprite. from RealName.AngeHash();
	/// </summary>
	public int ID;
	/// <summary>
	/// Uniaue logical name of this sprite.
	/// </summary>
	public string RealName;
	/// <summary>
	/// Width in global space. Calculate from the pixel rect width multiply Const.ART_SCALE
	/// </summary>
	public int GlobalWidth;
	/// <summary>
	/// Height in global space. Calculate from the pixel rect height multiply Const.ART_SCALE
	/// </summary>
	public int GlobalHeight;
	/// <summary>
	/// Rect position of the pixel data inside the atlas canvas
	/// </summary>
	public IRect PixelRect;
	/// <summary>
	/// Pivot X (0 means left edge. 1000 means right edge)
	/// </summary>
	public int PivotX;
	/// <summary>
	/// Pivot Y (0 means bottom edge. 1000 means top edge)
	/// </summary>
	public int PivotY;
	/// <summary>
	/// Z value for sort rendering cells
	/// </summary>
	public int LocalZ;
	/// <summary>
	/// Border value in global space.
	/// </summary>
	public Int4 GlobalBorder;
	/// <summary>
	/// AngeHash of the atlas this sprite belongs to
	/// </summary>
	public int AtlasID;
	/// <summary>
	/// Instance of the atlas this sprite belongs to
	/// </summary>
	public Atlas Atlas;
	/// <summary>
	/// Instance of the sprite group this sprite belongs to. Null if this sprite is individual.
	/// </summary>
	public SpriteGroup Group;
	/// <summary>
	/// True if this sprite is tagged as trigger.
	/// </summary>
	public bool IsTrigger;
	/// <summary>
	/// Rule for auto tiling map blocks
	/// </summary>
	public BlockRule Rule;
	/// <summary>
	/// Meta tag of this sprite. Multiple tags can be contains at same time.
	/// </summary>
	public Tag Tag;
	/// <summary>
	/// Animation duration of this sprite
	/// </summary>
	public int Duration;
	/// <summary>
	/// Average color of the pixels
	/// </summary>
	public Color32 SummaryTint;
	/// <summary>
	/// The pixels content data
	/// </summary>
	public Color32[] Pixels;
	internal AngeSprite AttachedSprite;

	private bool PixelDirty = false;

	// API
	internal void LoadFromBinary_v0 (BinaryReader reader, bool ignorePixels) {
		uint byteLen = reader.ReadUInt32();
		long endPos = reader.BaseStream.Position + byteLen;
		PixelDirty = false;
		try {
			// Name
			int nameLen = reader.ReadByte();
			CacheBuilder.Clear();
			for (int i = 0; i < nameLen; i++) {
				CacheBuilder.Append((char)reader.ReadByte());
			}
			RealName = CacheBuilder.ToString();

			// ID
			ID = RealName.AngeHash();

			// Rect
			PixelRect.x = reader.ReadInt16();
			PixelRect.y = reader.ReadInt16();
			PixelRect.width = reader.ReadInt16();
			PixelRect.height = reader.ReadInt16();
			GlobalWidth = PixelRect.width * Const.ART_SCALE;
			GlobalHeight = PixelRect.height * Const.ART_SCALE;

			// Pivot
			PivotX = reader.ReadInt16();
			PivotY = reader.ReadInt16();

			// Local Z
			LocalZ = reader.ReadInt32();

			// Global Border
			GlobalBorder = new Int4(
				reader.ReadUInt16(),
				reader.ReadUInt16(),
				reader.ReadUInt16(),
				reader.ReadUInt16()
			);

			// Atlas Index
			AtlasID = reader.ReadInt32();

			// Summary Tint
			SummaryTint = Util.IntToColor(reader.ReadInt32());

			// IsTrigger
			IsTrigger = reader.ReadByte() == 1;

			// Rule
			Rule = FrameworkUtil.DigitToBlockRule(reader.ReadInt32());

			// Tag
			Tag = (Tag)reader.ReadInt32();

			// Duration
			Duration = reader.ReadUInt16();

			// Group
			Group = null;

			// Pixels
			if (ignorePixels) {
				Pixels = new Color32[PixelRect.width * PixelRect.height];
				reader.BaseStream.Seek(PixelRect.width * PixelRect.height * 4, SeekOrigin.Current);
			} else {
				var bytes = reader.ReadBytes(PixelRect.width * PixelRect.height * 4);
				Pixels = bytes.Bytes_to_Pixels(PixelRect.width, PixelRect.height);
			}

		} catch (System.Exception ex) { Debug.LogException(ex); }
		reader.BaseStream.Position = endPos;
	}

	internal void SaveToBinary_v0 (BinaryWriter writer) {
		long markPos = writer.BaseStream.Position;
		writer.Write((uint)0);
		long startPos = writer.BaseStream.Position;
		if (PixelDirty) {
			PixelDirty = false;
			SummaryTint = FrameworkUtil.GetSummaryTint(Pixels);
		}
		try {

			// Name
			int len = RealName.Length.Clamp(0, 255);
			writer.Write((byte)len);
			for (int i = 0; i < len; i++) {
				writer.Write((byte)RealName[i]);
			}

			// Rect
			writer.Write((short)PixelRect.x);
			writer.Write((short)PixelRect.y);
			writer.Write((short)PixelRect.width);
			writer.Write((short)PixelRect.height);

			// Pivot
			writer.Write((short)PivotX);
			writer.Write((short)PivotY);

			// Local Z
			writer.Write((int)LocalZ);

			// Global Border
			writer.Write((ushort)GlobalBorder.x);
			writer.Write((ushort)GlobalBorder.y);
			writer.Write((ushort)GlobalBorder.z);
			writer.Write((ushort)GlobalBorder.w);

			// Atlas Index
			writer.Write((int)AtlasID);

			// Summary Tint
			writer.Write((int)Util.ColorToInt(SummaryTint));

			// IsTrigger
			writer.Write((byte)(IsTrigger ? 1 : 0));

			// Rule
			writer.Write((int)FrameworkUtil.BlockRuleToDigit(Rule));

			// Tag
			writer.Write((int)Tag);

			// Duration
			writer.Write((ushort)Duration);

			// Pixels
			Pixels ??= new Color32[PixelRect.width * PixelRect.height];
			var bytes = Pixels.Pixels_to_Bytes();
			writer.Write(bytes);

		} catch (System.Exception ex) { Debug.LogException(ex); }
		long endPos = writer.BaseStream.Position;
		writer.BaseStream.Position = markPos;
		writer.Write((uint)(endPos - startPos));
		writer.BaseStream.Position = endPos;
	}

	/// <summary>
	/// Set pixel content data to new size without delete the data inside
	/// </summary>
	/// <param name="newRect">New pixel rect position</param>
	/// <param name="resizeBorder">True if auto resize the border</param>
	/// <param name="contentChanged">True if any pixel data changed</param>
	public void ResizePixelRect (IRect newRect, bool resizeBorder, out bool contentChanged) {
		newRect.width = newRect.width.GreaterOrEquel(1);
		newRect.height = newRect.height.GreaterOrEquel(1);
		contentChanged = false;
		// Pixels
		if (newRect != PixelRect) {
			// Check for Content Change
			for (int i = 0; i < Pixels.Length; i++) {
				var color = Pixels[i];
				if (color.a == 0) continue;
				int x = PixelRect.x + i % PixelRect.width;
				int y = PixelRect.y + i / PixelRect.width;
				if (!newRect.Contains(x, y)) {
					contentChanged = true;
					break;
				}
			}
			// Change Pixels
			int newLen = newRect.width * newRect.height;
			var newPixels = new Color32[newLen];
			int left = Util.Max(PixelRect.xMin, newRect.xMin);
			int right = Util.Min(PixelRect.xMax, newRect.xMax);
			int down = Util.Max(PixelRect.yMin, newRect.yMin);
			int up = Util.Min(PixelRect.yMax, newRect.yMax);
			for (int x = left; x < right; x++) {
				for (int y = down; y < up; y++) {
					newPixels[(y - newRect.y) * newRect.width + (x - newRect.x)] =
						Pixels[(y - PixelRect.y) * PixelRect.width + (x - PixelRect.x)];
				}
			}
			Pixels = newPixels;
		}
		// Border
		if (resizeBorder) {
			// Left
			GlobalBorder.left -= (newRect.xMin - PixelRect.xMin) * Const.ART_SCALE;
			if (newRect.xMin != PixelRect.xMin) {
				GlobalBorder.left = GlobalBorder.left.Clamp(0, newRect.width * Const.ART_SCALE - GlobalBorder.right);
			}
			// Right
			GlobalBorder.right += (newRect.xMax - PixelRect.xMax) * Const.ART_SCALE;
			if (newRect.xMax != PixelRect.xMax) {
				GlobalBorder.right = GlobalBorder.right.Clamp(0, newRect.width * Const.ART_SCALE - GlobalBorder.left);
			}
			// Down
			GlobalBorder.down -= (newRect.yMin - PixelRect.yMin) * Const.ART_SCALE;
			if (newRect.yMin != PixelRect.yMin) {
				GlobalBorder.down = GlobalBorder.down.Clamp(0, newRect.height * Const.ART_SCALE - GlobalBorder.up);
			}
			// Up
			GlobalBorder.up += (newRect.yMax - PixelRect.yMax) * Const.ART_SCALE;
			if (newRect.yMax != PixelRect.yMax) {
				GlobalBorder.up = GlobalBorder.up.Clamp(0, newRect.height * Const.ART_SCALE - GlobalBorder.down);
			}
		}
		// Rect
		PixelRect = newRect;
		GlobalWidth = PixelRect.width * Const.ART_SCALE;
		GlobalHeight = PixelRect.height * Const.ART_SCALE;
	}

	/// <summary>
	/// Create a new sprite instance with same data with this one
	/// </summary>
	public AngeSprite CreateCopy () => CopyTo(new AngeSprite());

	/// <summary>
	/// Copy the data to the given sprite
	/// </summary>
	/// <returns>The target sprite</returns>
	public AngeSprite CopyTo (AngeSprite target) {
		var pixels = new Color32[Pixels.Length];
		Pixels.CopyTo(pixels, 0);
		target.ID = ID;
		target.RealName = RealName;
		target.GlobalWidth = GlobalWidth;
		target.GlobalHeight = GlobalHeight;
		target.PixelRect = PixelRect;
		target.PivotX = PivotX;
		target.PivotY = PivotY;
		target.LocalZ = LocalZ;
		target.GlobalBorder = GlobalBorder;
		target.AtlasID = AtlasID;
		target.Atlas = Atlas;
		target.Group = null;
		target.IsTrigger = IsTrigger;
		target.Rule = Rule;
		target.Tag = Tag;
		target.Duration = Duration;
		target.SummaryTint = SummaryTint;
		target.Pixels = pixels;
		target.AttachedSprite = AttachedSprite;
		return target;
	}

	/// <summary>
	/// Make sure the borders don't overlaps each others
	/// </summary>
	public void ValidBorders (Direction8? priority = null) {
		priority ??= Direction8.BottomLeft;
		if (GlobalBorder.horizontal >= GlobalWidth) {
			if (priority.Value.IsLeft()) {
				GlobalBorder.left = GlobalWidth - GlobalBorder.right;
			} else {
				GlobalBorder.right = GlobalWidth - GlobalBorder.left;
			}
			GlobalBorder.left = GlobalBorder.left.Clamp(0, GlobalWidth);
			GlobalBorder.right = GlobalBorder.right.Clamp(0, GlobalWidth);
		}
		if (GlobalBorder.vertical >= GlobalHeight) {
			if (priority.Value.IsBottom()) {
				GlobalBorder.down = GlobalHeight - GlobalBorder.up;
			} else {
				GlobalBorder.up = GlobalHeight - GlobalBorder.down;
			}
			GlobalBorder.down = GlobalBorder.down.Clamp(0, GlobalHeight);
			GlobalBorder.up = GlobalBorder.up.Clamp(0, GlobalHeight);
		}
	}

	/// <summary>
	/// Mark this sprite as changed without save
	/// </summary>
	public void SetPixelDirty () => PixelDirty = true;

	/// <summary>
	/// Link the given sprite to the texture
	/// </summary>
	/// <param name="texture"></param>
	/// <param name="sheet"></param>
	public void MakeDedicatedForTexture (object texture, Sheet sheet) {
		RemoveFromDedicatedTexture(sheet);
		sheet.TexturePool[ID] = texture;
		sheet.SpritePool[ID] = this;
		var tSize = Game.GetTextureSize(texture);
		PixelRect = new IRect(0, 0, tSize.x, tSize.y);
		GlobalWidth = tSize.x * Const.ART_SCALE;
		GlobalHeight = tSize.y * Const.ART_SCALE;
	}

	/// <summary>
	/// Remove the texture dedicated sprite 
	/// </summary>
	/// <param name="sheet"></param>
	public void RemoveFromDedicatedTexture (Sheet sheet) {
		if (sheet.TexturePool.ContainsKey(ID)) {
			Game.UnloadTexture(sheet.TexturePool[ID]);
		}
		sheet.TexturePool.Remove(ID);
		sheet.SpritePool.Remove(ID);
	}

}
