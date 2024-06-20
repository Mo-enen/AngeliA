using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

public enum BodyGadgetType { Face, Hair, Ear, Horn, Tail, Wing, }

public abstract class BodyGadget {


	private static readonly Dictionary<int, BodyGadget> Pool = new();
	private static Dictionary<int, int>[] DefaultPool = null;
	protected abstract BodyGadgetType GadgetType { get; }
	public virtual bool SpriteLoaded => true;


	// MSG
	[OnGameInitialize(-129)]
	public static void BeforeGameInitialize () {
		// Init Pool
		Pool.Clear();
		var bodyGadBaseType = typeof(BodyGadget);
		var bgTypes = new List<System.Type>();
		foreach (var type in bodyGadBaseType.AllChildClass()) {
			if (type.BaseType == bodyGadBaseType) {
				bgTypes.Add(type);
			} else {
				if (System.Activator.CreateInstance(type) is not BodyGadget gadget) continue;
				gadget.FillFromSheet(type.AngeName());
				int id = type.AngeHash();
				Pool.TryAdd(id, gadget);
			}
		}
		// Init DefaultPool
		var templates = new BodyGadget[bgTypes.Count];
		DefaultPool = new Dictionary<int, int>[bgTypes.Count].FillWithNewValue();
		foreach (var charType in typeof(PoseCharacter).AllChildClass()) {
			string charName = charType.AngeName();
			int charID = charName.AngeHash();
			for (int i = 0; i < bgTypes.Count; i++) {
				var gType = bgTypes[i];
				if (gType == null) break;
				// Create Template
				var temp = templates[i];
				temp ??= templates[i] = System.Activator.CreateInstance(gType) as BodyGadget;
				if (temp == null) continue;
				if (!temp.FillFromSheet(charName)) continue;
				// Founded
				templates[i] = null;
				int ggID = $"{charName}.{gType.AngeName()}".AngeHash();
				Pool.TryAdd(ggID, temp);
				DefaultPool[(int)temp.GadgetType].TryAdd(charID, ggID);
			}
		}
	}


	public abstract void DrawGadget (PoseCharacter character);


	public abstract bool FillFromSheet (string basicName);


	// API
	public static bool TryGetDefaultGadgetID (int characterID, BodyGadgetType type, out int gadgetID) {
		gadgetID = 0;
		int typeIndex = (int)type;
		if (typeIndex < 0 || typeIndex >= DefaultPool.Length) return false;
		if (DefaultPool[typeIndex].TryGetValue(characterID, out gadgetID)) {
			return gadgetID != 0;
		} else {
			return false;
		}
	}


	public static bool TryGetGadget (int gadgetID, out BodyGadget gadget) => Pool.TryGetValue(gadgetID, out gadget);


	public string GetDisplayName (string typeName) {
		string name = GetType().AngeName();
		return Language.Get($"{name}.{typeName}".AngeHash(), Util.GetDisplayName(name));
	}


}