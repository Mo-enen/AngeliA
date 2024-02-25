using System.Collections;
using System.Collections.Generic;


namespace AngeliA; 
public static class EntityAttribute {


	// Character
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class DefaultSelectPlayerAttribute : System.Attribute {
		public int Priority = 0;
		public DefaultSelectPlayerAttribute (int priority = 0) => Priority = priority;
	}


	// Map Editor
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class ExcludeInMapEditorAttribute : System.Attribute { }


	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
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
		public IRect Value;
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