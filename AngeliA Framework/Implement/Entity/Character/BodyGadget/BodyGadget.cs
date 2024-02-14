using System.Collections;
using System.Collections.Generic;


namespace AngeliA.Framework {


	public enum BodyGadgetType { Face, Hair, Ear, Horn, Tail, Wing, }


	[RequireSprite("{2}")]
	public abstract class BodyGadget {


		protected abstract BodyGadgetType GadgetType { get; }
		private static readonly Dictionary<int, BodyGadget> Pool = new();
		private static readonly Dictionary<int, int[]> DefaultPool = new();
		private static readonly int BodyGadgetTypeLength = typeof(BodyGadgetType).EnumLength();


		// MSG
		[OnGameInitialize(-127)]
		public static void BeforeGameInitialize () {
			Pool.Clear();
			var charType = typeof(Character);
			foreach (var type in typeof(BodyGadget).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not BodyGadget gadget) continue;
				// ID
				int id = type.AngeHash();
				Pool.TryAdd(id, gadget);
				// Default
				var dType = type.DeclaringType;
				if (dType != null && dType.IsSubclassOf(charType)) {
					int dTypeID = dType.AngeHash();
					if (!DefaultPool.TryGetValue(dTypeID, out int[] ids)) {
						DefaultPool.Add(dTypeID, ids = new int[BodyGadgetTypeLength]);
					}
					ids[(int)gadget.GadgetType] = id;
				}
			}
		}


		public abstract void DrawGadget (PoseCharacter character);


		// API
		public static bool TryGetDefaultGadgetID (int characterID, BodyGadgetType type, out int gadgetID) {
			if (DefaultPool.TryGetValue(characterID, out var gadgetsID)) {
				gadgetID = gadgetsID[(int)type];
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
}