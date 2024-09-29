using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum ClothType { Head, Body, Hand, Hip, Foot, }

internal interface IModularCloth { }

public abstract class Cloth {


	// Const
	private static readonly Cell[] SINGLE_CELL = [Cell.EMPTY];

	// Api
	public int TypeID { get; init; }
	protected abstract ClothType ClothType { get; }
	public virtual bool SpriteLoaded => true;
	public int SheetIndex { get; private set; } = -1;

	// Data
	protected static readonly Dictionary<int, Cloth> Pool = [];
	protected static Dictionary<int, int>[] DefaultPool = null;


	// MSG
	[OnGameInitialize(-129)]
	internal static TaskResult OnGameInitializeCloth () {

		if (!Renderer.IsReady) return TaskResult.Continue;

		// Init Pool
		Pool.Clear();
		var clothType = typeof(Cloth);
		foreach (var type in clothType.AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not Cloth cloth) continue;
			cloth.FillFromSheet(type.AngeName());
			int suitID = type.AngeHash();
			Pool.TryAdd(suitID, cloth);
		}

		// Get Modular Types
		int clothTypeCount = typeof(ClothType).EnumLength();
		var modularTypes = new System.Type[clothTypeCount];
		foreach (var mType in typeof(IModularCloth).AllClassImplemented()) {
			if (System.Activator.CreateInstance(mType) is not Cloth cloth) continue;
			int typeIndex = (int)cloth.ClothType;
			modularTypes[typeIndex] = mType;
		}

		// Init Default Pool
		DefaultPool = new Dictionary<int, int>[clothTypeCount].FillWithNewValue();
		foreach (var charType in typeof(Character).AllChildClass()) {
			string cName = charType.AngeName();
			int charID = cName.AngeHash();
			// From Sheet
			for (int i = 0; i < clothTypeCount; i++) {
				var clType = modularTypes[i];
				var dPool = DefaultPool[i];
				if (dPool.ContainsKey(charID)) continue;
				if (System.Activator.CreateInstance(clType) is not Cloth temp) continue;
				if (!temp.FillFromSheet(cName)) continue;
				int sID = $"{cName}.{clType.AngeName()}".AngeHash();
				Pool.TryAdd(sID, temp);
				dPool.Add(charID, sID);
			}
		}

		return TaskResult.End;

	}


	public Cloth () => TypeID = GetType().AngeHash();


	public abstract void DrawCloth (PoseCharacterRenderer renderer);


	public virtual bool FillFromSheet (string name) {
		SheetIndex = Renderer.CurrentSheetIndex;
		return true;
	}


	// Pool
	public static bool HasCloth (int clothID) => Pool.ContainsKey(clothID);
	public static bool TryGetCloth (int clothID, out Cloth cloth) => Pool.TryGetValue(clothID, out cloth);


	public static bool TryGetDefaultClothID (int characterID, ClothType suitType, out int suitID) {
		suitID = 0;
		int typeIndex = (int)suitType;
		if (typeIndex >= DefaultPool.Length) return false;
		var pool = DefaultPool[typeIndex];
		if (pool.TryGetValue(characterID, out suitID)) {
			return true;
		}
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
			bodyPart.Covered = !sprite.Tag.HasAll(Tag.ShowLimb) ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
		} else {
			bodyPart.Covered = sprite.Tag.HasAll(Tag.HideLimb) ?
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
			bodyPart.Covered = !sprite.Tag.HasAll(Tag.ShowLimb) ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
		} else {
			bodyPart.Covered = sprite.Tag.HasAll(Tag.HideLimb) ?
				BodyPart.CoverMode.FullCovered : BodyPart.CoverMode.Covered;
		}
		return result;
	}


	public string GetDisplayName () {
		string typeName = (GetType().DeclaringType ?? GetType()).AngeName();
		return $"{Language.Get($"{typeName}.{ClothType}".AngeHash(), Util.GetDisplayName(typeName))}";
	}


}
