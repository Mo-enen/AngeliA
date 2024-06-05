using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public enum BodyGadgetType { Face, Hair, Ear, Horn, Tail, Wing, }


public abstract class BodyGadget {


	protected abstract BodyGadgetType GadgetType { get; }
	private static readonly Dictionary<int, BodyGadget> Pool = new();
	private static readonly int BodyGadgetTypeLength = typeof(BodyGadgetType).EnumLength();
	private static Dictionary<int, int>[] DefaultPool = null;


	// MSG
	[OnGameInitialize(-127)]
	public static void BeforeGameInitialize () {
		DefaultPool = new Dictionary<int, int>[BodyGadgetTypeLength].FillWithNewValue();
		Pool.Clear();
		var charType = typeof(PoseCharacter);
		foreach (var type in typeof(BodyGadget).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is not BodyGadget gadget) continue;
			var dType = type.DeclaringType;
			if (dType != null && dType.IsSubclassOf(charType)) {
				// Declaring
				int id = $"{dType.AngeName()}.{type.AngeName()}".AngeHash();
				Pool.TryAdd(id, gadget);
				// Default
				var dPool = DefaultPool[(int)gadget.GadgetType];
				dPool[dType.AngeHash()] = id;
			} else {
				// Normal
				int id = type.AngeHash();
				Pool.TryAdd(id, gadget);
			}
		}
	}


	public abstract void DrawGadget (PoseCharacter character);


	// API
	public static bool TryGetDefaultGadgetID (int characterID, BodyGadgetType type, out int gadgetID) {
		if (DefaultPool[(int)type].TryGetValue(characterID, out gadgetID)) {
			return true;
		} else {
			gadgetID = 0;
			return false;
		}
	}


	public static bool TryGetGadget (int gadgetID, out BodyGadget gadget) => Pool.TryGetValue(gadgetID, out gadget);


	public string GetDisplayName () {
		string name = (GetType().DeclaringType ?? GetType()).AngeName();
		return Language.Get($"{name}.{GadgetType}".AngeHash(), Util.GetDisplayName(name));
	}


}