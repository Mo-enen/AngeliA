using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AngeliA;

public enum BodyGadgetType { Face, Hair, Ear, Horn, Tail, Wing, }

internal interface IModularBodyGadget {
	public string ModularName { get; }
}

/// <summary>
/// Representation of a pose character's extra body part. Always use instance from pool.
/// </summary>
/// <example><code>
/// BodyGadget.TryGetGadget(id, out var result);
/// </code></example>
public abstract class BodyGadget {


	private static readonly Dictionary<int, BodyGadget> Pool = [];
	private static Dictionary<int, int>[] DefaultPool = null;
	public static bool BodyGadgetSystemReady { get; private set; } = false;
	/// <summary>
	/// Which artwork sheet does this body gadget get it's artwork from
	/// </summary>
	public int SheetIndex { get; private set; } = -1;
	/// <summary>
	/// Global unique AngeHash of this body gadget
	/// </summary>
	public int GadgetID { get; private set; } = 0;
	/// <summary>
	/// Global unique AngeName of this body gadget 
	/// </summary>
	public string GadgetName { get; private set; } = "";
	/// <summary>
	/// Type of this body gadget
	/// </summary>
	public abstract BodyGadgetType GadgetType { get; }
	/// <summary>
	/// True if artwork sprite is loaded
	/// </summary>
	public virtual bool SpriteLoaded => true;


	// MSG
	[OnGameInitialize(-129)]
	internal static TaskResult InitializePool () {

		if (!Renderer.IsReady) return TaskResult.Continue;

		// Init Pool
		Pool.Clear();
		foreach (var type in typeof(BodyGadget).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not BodyGadget gadget) continue;
			string name = type.AngeName();
			gadget.FillFromSheet(name);
			gadget.GadgetName = name;
			gadget.GadgetID = name.AngeHash();
			Pool.TryAdd(gadget.GadgetID, gadget);
		}

		// Get Modular Types
		int gadgetTypeCount = typeof(BodyGadgetType).EnumLength();
		var modularTypes = new System.Type[gadgetTypeCount];
		var modularNames = new string[gadgetTypeCount];
		foreach (var mType in typeof(IModularBodyGadget).AllClassImplemented()) {
			if (System.Activator.CreateInstance(mType) is not BodyGadget gadget) continue;
			int typeIndex = (int)gadget.GadgetType;
			modularTypes[typeIndex] = mType;
			modularNames[typeIndex] = (gadget as IModularBodyGadget).ModularName;
		}

		// Fill Default
		DefaultPool = new Dictionary<int, int>[gadgetTypeCount].FillWithNewValue();
		foreach (var charType in typeof(Character).AllChildClass()) {
			string charName = charType.AngeName();
			int charID = charName.AngeHash();
			// Get Default from Sheet
			for (int i = 0; i < gadgetTypeCount; i++) {
				var gType = modularTypes[i];
				var dPool = DefaultPool[i];
				if (dPool.ContainsKey(charID)) continue;
				if (System.Activator.CreateInstance(gType) is not BodyGadget temp) continue;
				if (!temp.FillFromSheet(charName)) continue;
				temp.GadgetName = $"{charName}.{modularNames[i]}";
				temp.GadgetID = temp.GadgetName.AngeHash();
				Pool.TryAdd(temp.GadgetID, temp);
				dPool.Add(charID, temp.GadgetID);
			}
		}

		Pool.TrimExcess();
		for (int i = 0; i < DefaultPool.Length; i++) {
			DefaultPool[i].TrimExcess();
		}

		BodyGadgetSystemReady = true;
		return TaskResult.End;

	}


	[OnMainSheetReload]
	internal static void OnMainSheetReload () {
		if (Game.GlobalFrame != 0) InitializePool();
	}


	/// <summary>
	/// Render the gadget for the given character
	/// </summary>
	public abstract void DrawGadget (PoseCharacterRenderer renderer);


	/// <summary>
	/// Load sprite data from Renderer.CurrentSheet
	/// </summary>
	public virtual bool FillFromSheet (string basicName) {
		SheetIndex = Renderer.CurrentSheetIndex;
		return true;
	}


	/// <summary>
	/// Render the gadget as gizmos
	/// </summary>
	/// <param name="rect">Global rect position</param>
	/// <param name="tint">Color tint</param>
	/// <param name="z">Z position for sorting</param>
	public virtual void DrawGadgetGizmos (IRect rect, Color32 tint, int z) {
		if (Renderer.TryGetSpriteForGizmos(GadgetID, out var sprite)) {
			Renderer.Draw(sprite, rect.Fit(sprite), tint, z);
		}
	}


	// API
	/// <summary>
	/// Iterate through all body gadgets
	/// </summary>
	/// <returns></returns>
	public static IEnumerable<KeyValuePair<int, BodyGadget>> ForAllGadget () {
		foreach (var pair in Pool) yield return pair;
	}


	/// <summary>
	/// Get body gadget id of the default gadget for the given character
	/// </summary>
	public static int GetDefaultGadgetID (int characterID, BodyGadgetType type) {
		int typeIndex = (int)type;
		if (typeIndex < 0 || typeIndex >= DefaultPool.Length) return 0;
		DefaultPool[typeIndex].TryGetValue(characterID, out int gadgetID);
		return gadgetID;
	}


	/// <summary>
	/// Get body gadget instance from system pool
	/// </summary>
	public static bool TryGetGadget (int gadgetID, out BodyGadget gadget) => Pool.TryGetValue(gadgetID, out gadget);


	public string GetDisplayName (string typeName, out int languageID) {
		string name = GetType().AngeName();
		languageID = $"{name}.{typeName}".AngeHash();
		return Language.Get(languageID, Util.GetDisplayName(name));
	}


}