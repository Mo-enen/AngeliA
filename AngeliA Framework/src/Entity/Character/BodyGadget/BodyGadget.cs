using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AngeliA;

public enum BodyGadgetType { Face, Hair, Ear, Horn, Tail, Wing, }

internal interface IModularBodyGadget { }

public abstract class BodyGadget {


	private static readonly Dictionary<int, BodyGadget> Pool = [];
	private static Dictionary<int, int>[] DefaultPool = null;
	public int SheetIndex { get; private set; } = -1;
	protected abstract BodyGadgetType GadgetType { get; }
	public virtual bool SpriteLoaded => true;


	// MSG
	[OnGameInitialize(-129)]
	public static TaskResult BeforeGameInitialize () {

		if (!Renderer.IsReady) return TaskResult.Continue;

		// Init Pool
		Pool.Clear();
		foreach (var type in typeof(BodyGadget).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not BodyGadget gadget) continue;
			gadget.FillFromSheet(type.AngeName());
			int id = type.AngeHash();
			Pool.TryAdd(id, gadget);
		}

		// Get Modular Types
		int gadgetTypeCount = typeof(BodyGadgetType).EnumLength();
		var modularTypes = new System.Type[gadgetTypeCount];
		foreach (var mType in typeof(IModularBodyGadget).AllClassImplemented()) {
			if (System.Activator.CreateInstance(mType) is not BodyGadget gadget) continue;
			int typeIndex = (int)gadget.GadgetType;
			modularTypes[typeIndex] = mType;
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
				int ggID = $"{charName}.{gType.AngeName()}".AngeHash();
				Pool.TryAdd(ggID, temp);
				dPool.Add(charID, ggID);
			}
		}

		Pool.TrimExcess();
		for (int i = 0; i < DefaultPool.Length; i++) {
			DefaultPool[i].TrimExcess();
		}
		return TaskResult.End;

	}


	public abstract void DrawGadget (PoseCharacterRenderer character);


	public virtual bool FillFromSheet (string basicName) {
		SheetIndex = Renderer.CurrentSheetIndex;
		return true;
	}


	// API
	public static IEnumerable<KeyValuePair<int, BodyGadget>> ForAllGadget () {
		foreach (var pair in Pool) yield return pair;
	}


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