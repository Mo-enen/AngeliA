using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	// Game Event
	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnGameInitialize : System.Attribute {
		public int Order;
		public OnGameInitialize (int order) => Order = order;
	}


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnGameUpdateAttribute : System.Attribute { }


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnGameUpdateLaterAttribute : System.Attribute { }


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnGameUpdatePauselessAttribute : System.Attribute { }


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnViewZChangedAttribute : System.Attribute { }


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnGameRestartAttribute : System.Attribute { }


	[System.AttributeUsage(System.AttributeTargets.Method)]
	public class OnGameTryingToQuitAttribute : System.Attribute { }


	// Entity Attribute
	public static class EntityAttribute {


		// Map Editor
		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class ExcludeInMapEditorAttribute : System.Attribute { }


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class MapEditorGroupAttribute : System.Attribute {
			public string GroupName = "";
			public MapEditorGroupAttribute (string groupName) => GroupName = groupName;
		}


		// Misc
		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class LayerAttribute : System.Attribute {
			public int Layer;
			public LayerAttribute (int layer) {
				Layer = layer;
			}
		}


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class CapacityAttribute : System.Attribute {
			public int Value;
			public int PreSpawn;
			public CapacityAttribute (int capacity, int preSpawn = 0) {
				Value = capacity;
				PreSpawn = preSpawn.Clamp(0, capacity);
			}
		}


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class BoundsAttribute : System.Attribute {
			public RectInt Value;
			public BoundsAttribute (int boundX, int boundY, int boundWidth, int boundHeight) => Value = new(boundX, boundY, boundWidth, boundHeight);
		}


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class DontDestroyOutOfRangeAttribute : System.Attribute { }


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class DontSpawnFromWorld : System.Attribute { }


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class UpdateOutOfRangeAttribute : System.Attribute { }


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class DontDrawBehindAttribute : System.Attribute { }


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class DontDestroyOnSquadTransitionAttribute : System.Attribute { }


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class ForceSpawnAttribute : System.Attribute { }


		[System.AttributeUsage(System.AttributeTargets.Class)]
		public class StageOrderAttribute : System.Attribute {
			public int Order = 0;
			public StageOrderAttribute (int order) {
				Order = order;
			}
		}


	}
}