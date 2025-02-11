using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum ClothType { Head, Body, Hand, Hip, Foot, }

public abstract class Cloth {




	#region --- VAR ---


	// Const
	private static readonly Cell[] SINGLE_CELL = [Cell.EMPTY];

	// Api
	public static bool ClothSystemReady { get; private set; } = false;
	public int ClothID { get; private set; }
	public string ClothName { get; private set; }
	public abstract ClothType ClothType { get; }
	public virtual bool SpriteLoaded => true;
	public int SheetIndex { get; private set; } = -1;

	// Data
	protected static readonly Dictionary<int, Cloth> Pool = [];
	protected static Dictionary<int, int>[] DefaultPool = null;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-129)]
	internal static TaskResult OnGameInitializeCloth () {

		if (!Renderer.IsReady) return TaskResult.Continue;

		// Init Pool
		Pool.Clear();
		var clothType = typeof(Cloth);
		foreach (var type in clothType.AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not Cloth cloth) continue;
			cloth.FillFromSheet(type.AngeName());
			cloth.ClothName = type.AngeName();
			cloth.ClothID = cloth.ClothName.AngeHash();
			Pool.TryAdd(cloth.ClothID, cloth);
		}

		// Get Modular Types
		int clothTypeCount = typeof(ClothType).EnumLength();
		var modularTypes = new System.Type[clothTypeCount];
		var modularNames = new string[clothTypeCount];
		foreach (var mType in typeof(IModularCloth).AllClassImplemented()) {
			if (System.Activator.CreateInstance(mType) is not Cloth cloth) continue;
			int typeIndex = (int)cloth.ClothType;
			modularTypes[typeIndex] = mType;
			modularNames[typeIndex] = (cloth as IModularCloth).ModularName;
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
				if (System.Activator.CreateInstance(clType) is not Cloth cloth) continue;
				if (!cloth.FillFromSheet(cName)) continue;
				string sName = $"{cName}.{modularNames[i]}";
				int sID = sName.AngeHash();
				cloth.ClothID = sID;
				cloth.ClothName = sName;
				Pool.TryAdd(sID, cloth);
				dPool.Add(charID, sID);
			}
		}

		Pool.TrimExcess();
		for (int i = 0; i < DefaultPool.Length; i++) {
			DefaultPool[i].TrimExcess();
		}
		ClothSystemReady = true;
		return TaskResult.End;

	}


	public abstract void DrawCloth (PoseCharacterRenderer renderer);


	public virtual void DrawClothGizmos (IRect rect, Color32 tint, int z) {
		if (Renderer.TryGetSpriteForGizmos(ClothID, out var sprite)) {
			Renderer.Draw(sprite, rect.Fit(sprite), tint, z);
		}
	}


	#endregion




	#region --- API ---


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


	public static IEnumerable<KeyValuePair<int, Cloth>> ForAllCloth () {
		foreach (var pair in Pool) yield return pair;
	}


	// Draw
	public static Cell[] AttachClothOn (
		BodyPart bodyPart, AngeSprite sprite, int locationX, int locationY, int localZ,
		int widthAmount = 1000, int heightAmount = 1000,
		int localRotation = 0, int shiftPixelX = 0, int shiftPixelY = 0, bool defaultHideLimb = true
	) {
		var location = bodyPart.GlobalLerp(locationX / 1000f, locationY / 1000f, true);
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


	public static Cell[] CoverClothOn (BodyPart bodyPart, AngeSprite sprite) => CoverClothOn(bodyPart, sprite, 1, Color32.WHITE, true);
	public static Cell[] CoverClothOn (BodyPart bodyPart, AngeSprite sprite, int localZ) => CoverClothOn(bodyPart, sprite, localZ, Color32.WHITE, true);
	public static Cell[] CoverClothOn (BodyPart bodyPart, AngeSprite sprite, int localZ, Color32 tint, bool defaultHideLimb = true) {
		if (sprite == null || bodyPart.IsFullCovered) return null;
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


	// UI
	public string GetDisplayName (out int languageID) {
		string typeName = GetType().AngeName();
		languageID = $"{typeName}.{ClothType}".AngeHash();
		return $"{Language.Get(languageID, Util.GetDisplayName(typeName))}";
	}


	#endregion




	#region --- LGC ---



	#endregion




}
