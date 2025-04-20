using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public enum ClothType { Head, Body, Hand, Hip, Foot, }


/// <summary>
/// Cloth for pose-style character (not for equipment items). Get instance with Cloth.TryGetCloth(id, out var result)
/// </summary>
public abstract class Cloth {




	#region --- VAR ---


	// Const
	private static readonly Cell[] SINGLE_CELL = [Cell.EMPTY];

	// Api
	public static bool ClothSystemReady { get; private set; } = false;
	/// <summary>
	/// Global unique id for this type of cloth
	/// </summary>
	public int ClothID { get; private set; }
	/// <summary>
	/// Type name for this type of cloth
	/// </summary>
	public string ClothName { get; private set; }
	/// <summary>
	/// Where should characters wear this cloth 
	/// </summary>
	public abstract ClothType ClothType { get; }
	/// <summary>
	/// True if the artwork sprites are loaded
	/// </summary>
	public virtual bool SpriteLoaded => true;
	/// <summary>
	/// Which artwork sheet does this cloth get it's artwork from
	/// </summary>
	public int SheetIndex { get; private set; } = -1;

	// Data
	private static readonly Dictionary<int, Cloth> Pool = [];
	private static Dictionary<int, int>[] DefaultPool = null;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-129)]
	internal static TaskResult InitializePool () {

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


	[OnMainSheetReload]
	internal static void OnMainSheetReload () {
		if (Game.GlobalFrame != 0) InitializePool();
	}


	/// <summary>
	/// Draw cloth for the given character
	/// </summary>
	public abstract void DrawCloth (PoseCharacterRenderer renderer);


	/// <summary>
	/// Draw cloth as gizmos
	/// </summary>
	/// <param name="rect">Rect position in global space</param>
	/// <param name="tint">Color tint</param>
	/// <param name="z">Z value for sort rendering cell</param>
	public virtual void DrawClothGizmos (IRect rect, Color32 tint, int z) {
		if (Renderer.TryGetSpriteForGizmos(ClothID, out var sprite)) {
			Renderer.Draw(sprite, rect.Fit(sprite), tint, z);
		}
	}


	#endregion




	#region --- API ---


	/// <summary>
	/// Load sprite data from Renderer.CurrentSheet
	/// </summary>
	public virtual bool FillFromSheet (string name) {
		SheetIndex = Renderer.CurrentSheetIndex;
		return true;
	}


	// Pool
	/// <summary>
	/// Does cloth with given id exists in the pool
	/// </summary>
	public static bool HasCloth (int clothID) => Pool.ContainsKey(clothID);


	/// <summary>
	/// Get cloth instance from pool
	/// </summary>
	public static bool TryGetCloth (int clothID, out Cloth cloth) => Pool.TryGetValue(clothID, out cloth);


	/// <summary>
	/// Get ID of given character's default cloth. Return 0 if not found
	/// </summary>
	public static int GetDefaultClothID (int characterID, ClothType suitType) {
		int typeIndex = (int)suitType;
		if (typeIndex >= DefaultPool.Length) return 0;
		var pool = DefaultPool[typeIndex];
		if (pool.TryGetValue(characterID, out int suitID)) return suitID;
		return 0;
	}


	/// <summary>
	/// Iterate through all cloth instance in pool
	/// </summary>
	public static IEnumerable<KeyValuePair<int, Cloth>> ForAllCloth () {
		foreach (var pair in Pool) yield return pair;
	}


	// Draw
	/// <summary>
	/// Attach the given artwork sprite as a general cloth to given bodypart. Cloth will use it's own size no matter how big the bodypart is.
	/// </summary>
	/// <param name="bodyPart">Target bodypart</param>
	/// <param name="sprite">Artwork sprite</param>
	/// <param name="locationX">Position X in bodypart's local space</param>
	/// <param name="locationY">Position Y in bodypart's local space</param>
	/// <param name="localZ">Local Z value the sort rendering cells</param>
	/// <param name="widthAmount">Horizontal size scaling (0 means 0%, 1000 means 100%)</param>
	/// <param name="heightAmount">Vertical size scaling (0 means 0%, 1000 means 100%)</param>
	/// <param name="localRotation">Rotation of the cloth in bodypart's local space</param>
	/// <param name="shiftPixelX">Position offset X</param>
	/// <param name="shiftPixelY">Position offset Y</param>
	/// <param name="defaultHideLimb">True if it requires the bodypart to be hiden. Artwork sprite's tag will override this value.</param>
	/// <returns>Rendering cells which holds the cloth rendering data</returns>
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
				CoverMode.FullCovered : CoverMode.Covered;
		} else {
			bodyPart.Covered = sprite.Tag.HasAll(Tag.HideLimb) ?
				CoverMode.FullCovered : CoverMode.Covered;
		}
		return result;
	}


	/// <inheritdoc cref="CoverClothOn(BodyPart, AngeSprite, int, Color32, bool)"/>
	public static Cell[] CoverClothOn (BodyPart bodyPart, AngeSprite sprite) => CoverClothOn(bodyPart, sprite, 1, Color32.WHITE, true);
	/// <inheritdoc cref="CoverClothOn(BodyPart, AngeSprite, int, Color32, bool)"/>
	public static Cell[] CoverClothOn (BodyPart bodyPart, AngeSprite sprite, int localZ) => CoverClothOn(bodyPart, sprite, localZ, Color32.WHITE, true);
	/// <summary>
	/// Cover the given artwork sprite as a general cloth to given bodypart. Size of the sprite will change based on how big the bodypart is.
	/// </summary>
	/// <param name="bodyPart">Target bodypart</param>
	/// <param name="sprite">Artwork sprite</param>
	/// <param name="localZ">Local Z value the sort rendering cells</param>
	/// <param name="tint">Color tint</param>
	/// <param name="defaultHideLimb">True if it requires the bodypart to be hiden. Artwork sprite's tag will override this value.</param>
	/// <returns>Rendering cells which holds the cloth rendering data</returns>
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
				CoverMode.FullCovered : CoverMode.Covered;
		} else {
			bodyPart.Covered = sprite.Tag.HasAll(Tag.HideLimb) ?
				CoverMode.FullCovered : CoverMode.Covered;
		}
		return result;
	}


	// UI
	/// <summary>
	/// Get display name for this cloth from language system.
	/// </summary>
	public string GetDisplayName (out int languageID) {
		string typeName = GetType().AngeName();
		languageID = $"{typeName}.{ClothType}".AngeHash();
		return $"{Language.Get(languageID, Util.GetDisplayName(typeName))}";
	}


	#endregion




}
