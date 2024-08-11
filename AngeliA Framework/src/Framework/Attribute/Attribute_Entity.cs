using System;
using System.Collections.Generic;

namespace AngeliA;

public static class EntityAttribute {


	// Character
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultSelectPlayerAttribute : Attribute {
		public int Priority = 0;
		public DefaultSelectPlayerAttribute (int priority = 0) => Priority = priority;
	}


	// Map Editor
	[AttributeUsage(AttributeTargets.Class)]
	public class ExcludeInMapEditorAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class MapEditorGroupAttribute : Attribute {
		public string GroupName = "";
		public MapEditorGroupAttribute (string groupName) => GroupName = groupName;
	}


	// Misc
	[AttributeUsage(AttributeTargets.Class)]
	public class LayerAttribute : Attribute {
		public int Layer;
		public LayerAttribute (int layer) {
			Layer = layer;
		}
	}


	[AttributeUsage(AttributeTargets.Class)]
	public class CapacityAttribute : Attribute {
		public int Value;
		public int PreSpawn;
		public CapacityAttribute (int capacity, int preSpawn = 0) {
			Value = capacity;
			PreSpawn = preSpawn.Clamp(0, capacity);
		}
	}


	[AttributeUsage(AttributeTargets.Class)]
	public class BoundsAttribute : Attribute {
		public IRect Value;
		public BoundsAttribute (int boundX, int boundY, int boundWidth, int boundHeight) => Value = new(boundX, boundY, boundWidth, boundHeight);
	}


	[AttributeUsage(AttributeTargets.Class)]
	public class DontDestroyOutOfRangeAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class DontSpawnFromWorld : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateOutOfRangeAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class DontDrawBehindAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class DontDestroyOnZChangedAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class StageOrderAttribute : Attribute {
		public int Order = 0;
		public StageOrderAttribute (int order) {
			Order = order;
		}
	}


	[AttributeUsage(AttributeTargets.Class)]
	public class RepositionWhenOutOfRangeAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class FromLevelBlockAttribute : Attribute {
		public int LevelID;
		public FromLevelBlockAttribute (string levelBlockName) {
			LevelID = levelBlockName.AngeHash();
		}
	}


}