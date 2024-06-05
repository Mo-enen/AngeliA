using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum ClothType { Head, Body, Hand, Hip, Foot, }

public abstract class Cloth {


	// Const
	private static readonly Cell[] SINGLE_CELL = { Cell.EMPTY };

	// Api
	public int TypeID { get; init; }
	protected abstract ClothType ClothType { get; }

	// Data
	protected static readonly Dictionary<int, Cloth> Pool = new();
	protected static readonly Dictionary<int, int[]> DefaultPool = new();
	private static readonly int ClothTypeCount = typeof(ClothType).EnumLength();


	// MSG
	[OnGameInitialize(-127)]
	public static void BeforeGameInitialize () {
		Pool.Clear();
		var charType = typeof(PoseCharacter);
		foreach (var type in typeof(Cloth).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not Cloth cloth) continue;
			int suitID = type.AngeHash();
			Pool.TryAdd(suitID, cloth);
		}
		// Default Suit
		foreach (var (suitID, cloth) in Pool) {
			var dType = cloth.GetType().DeclaringType;
			if (dType == null || !dType.IsSubclassOf(charType)) continue;
			int charID = dType.AngeHash();
			if (DefaultPool.TryGetValue(charID, out var suitArray)) {
				suitArray[(int)cloth.ClothType] = suitID;
			} else {
				var arr = new int[ClothTypeCount];
				arr[(int)cloth.ClothType] = suitID;
				DefaultPool.TryAdd(charID, arr);
			}
		}
	}

	public Cloth () => TypeID = GetType().AngeHash();

	public abstract void Draw (PoseCharacter character);


	// Pool
	public static bool HasCloth (int clothID) => Pool.ContainsKey(clothID);
	public static bool TryGetCloth (int clothID, out Cloth cloth) => Pool.TryGetValue(clothID, out cloth);


	public static bool TryGetDefaultClothID (int characterID, ClothType suitType, out int suitID) {
		if (DefaultPool.TryGetValue(characterID, out var suitArray)) {
			suitID = suitArray[(int)suitType];
			return true;
		}
		suitID = 0;
		return false;
	}


	// Draw
	public static Cell[] AttachClothOn (
		BodyPart bodyPart, AngeSprite sprite, int locationX, int locationY, int localZ,
		int widthAmount = 1000, int heightAmount = 1000,
		int localRotation = 0, int shiftPixelX = 0, int shiftPixelY = 0, bool defaultHideLimb = true
	) {
		var location = bodyPart.GlobalLerp(locationX / 1000f, locationY / 1000f);
		location.x += shiftPixelX;
		location.y += shiftPixelY;
		Cell[] result;
		if (sprite.GlobalBorder.IsZero) {
			var cell = Renderer.Draw(
				sprite,
				location.x,
				location.y,
				sprite.PivotX, sprite.PivotY, bodyPart.Rotation + localRotation,
				(bodyPart.Width > 0 ? sprite.GlobalWidth : -sprite.GlobalWidth) * widthAmount / 1000,
				(bodyPart.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight) * heightAmount / 1000,
				bodyPart.Z + localZ
			);
			result = SINGLE_CELL;
			result[0] = cell;
		} else {
			result = Renderer.DrawSlice(
				sprite, location.x, location.y,
				sprite.PivotX, sprite.PivotY, bodyPart.Rotation + localRotation,
				(bodyPart.Width > 0 ? sprite.GlobalWidth : -sprite.GlobalWidth) * widthAmount / 1000,
				(bodyPart.Height > 0 ? sprite.GlobalHeight : -sprite.GlobalHeight) * heightAmount / 1000,
				bodyPart.Z + localZ
			);
		}
		if (defaultHideLimb) {
			bodyPart.Covered = sprite.Tag != SpriteTag.SHOW_LIMB_TAG ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
		} else {
			bodyPart.Covered = sprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
		}
		return result;
	}


	public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID) => CoverClothOn(bodyPart, spriteID, 1, Color32.WHITE, true);
	public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID, int localZ) => CoverClothOn(bodyPart, spriteID, localZ, Color32.WHITE, true);
	public static Cell[] CoverClothOn (BodyPart bodyPart, int spriteID, int localZ, Color32 tint, bool defaultHideLimb = true) {
		if (spriteID == 0 || bodyPart.IsFullCovered || !Renderer.TryGetSprite(spriteID, out var sprite)) return null;
		Cell[] result;
		if (sprite.GlobalBorder.IsZero) {
			SINGLE_CELL[0] = Renderer.Draw(
				sprite, bodyPart.GlobalX, bodyPart.GlobalY,
				bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation,
				bodyPart.Width, bodyPart.Height, tint, bodyPart.Z + localZ
			);
			result = SINGLE_CELL;
		} else {
			result = Renderer.DrawSlice(
				sprite, bodyPart.GlobalX, bodyPart.GlobalY,
				bodyPart.PivotX, bodyPart.PivotY, bodyPart.Rotation,
				bodyPart.Width, bodyPart.Height, tint, bodyPart.Z + localZ
			);
		}
		if (defaultHideLimb) {
			bodyPart.Covered = sprite.Tag != SpriteTag.SHOW_LIMB_TAG ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
		} else {
			bodyPart.Covered = sprite.Tag == SpriteTag.HIDE_LIMB_TAG ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
		}
		return result;
	}


	public string GetDisplayName () {
		string typeName = (GetType().DeclaringType ?? GetType()).AngeName();
		return $"{Language.Get($"{typeName}.{ClothType}".AngeHash(), Util.GetDisplayName(typeName))}";
	}


}
