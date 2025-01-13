using System;
using System.Collections.Generic;

namespace AngeliA;

public static class EntityAttribute {


	// Character
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultSelectedPlayerAttribute (int priority = 0) : Attribute {
		public int Priority = priority;
	}


	// Map Editor
	[AttributeUsage(AttributeTargets.Class)]
	public class ExcludeInMapEditorAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class MapEditorGroupAttribute (string groupName, int order = 0) : Attribute {
		public string GroupName = groupName;
		public int Order = order;
	}


	// Misc
	[AttributeUsage(AttributeTargets.Class)]
	public class LayerAttribute (int layer) : Attribute {
		public int Layer = layer;
	}


	[AttributeUsage(AttributeTargets.Class)]
	public class CapacityAttribute (int capacity, int preSpawn = 0) : Attribute {
		public int Value = capacity;
		public int PreSpawn = preSpawn.Clamp(0, capacity);
	}


	[AttributeUsage(AttributeTargets.Class)]
	public class DontDespawnOutOfRangeAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class DontSpawnFromWorld : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateOutOfRangeAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class DontDrawBehindAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class DontDestroyOnZChangedAttribute : Attribute { }


	[AttributeUsage(AttributeTargets.Class)]
	public class StageOrderAttribute (int order) : Attribute {
		public int Order = order;
	}


	[AttributeUsage(AttributeTargets.Class)]
	public class RepositionWhenInactiveAttribute : Attribute { }


}