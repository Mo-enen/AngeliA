using System;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Attribute for entities
/// </summary>
public static class EntityAttribute {


	// Map Editor
	/// <summary>
	/// Do not show this entity inside the palette panel of map editor.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class ExcludeInMapEditorAttribute : Attribute { }


	/// <summary>
	/// Which map editor palette group should this entity in 
	/// </summary>
	/// <param name="groupName"></param>
	/// <param name="order"></param>
	[AttributeUsage(AttributeTargets.Class, Inherited = true)]
	public class MapEditorGroupAttribute (string groupName, int order = 0) : Attribute {
		internal string GroupName = groupName;
		internal int Order = order;
	}


	// Misc
	/// <summary>
	/// Which layer should this entity spawn inside.
	/// </summary>
	/// <param name="layer"></param>
	[AttributeUsage(AttributeTargets.Class)]
	public class LayerAttribute (int layer) : Attribute {
		internal int Layer = layer;
	}


	/// <summary>
	/// Spawn limit count of this entity 
	/// </summary>
	/// <param name="capacity"></param>
	/// <param name="preSpawn">Create this many instance of the entity when game initialize</param>
	[AttributeUsage(AttributeTargets.Class)]
	public class CapacityAttribute (int capacity, int preSpawn = 0) : Attribute {
		internal int Value = capacity;
		internal int PreSpawn = preSpawn.Clamp(0, capacity);
	}


	/// <summary>
	/// Do not despawn this entity when it's out of view rect
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DontDespawnOutOfRangeAttribute : Attribute { }


	/// <summary>
	/// Do not spawn this entity when it's painted into the map
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DontSpawnFromWorld : Attribute { }


	/// <summary>
	/// This entity will update when it's outside view rect
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class UpdateOutOfRangeAttribute : Attribute { }


	/// <summary>
	/// This entity will not display on the behind layer
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DontDrawBehindAttribute : Attribute { }


	/// <summary>
	/// This entity do not despawn when player enter another map layer
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DontDestroyOnZChangedAttribute : Attribute { }


	/// <summary>
	/// The order in which this entity gets updated in it's layer
	/// </summary>
	/// <param name="order"></param>
	[AttributeUsage(AttributeTargets.Class)]
	public class StageOrderAttribute (int order) : Attribute {
		internal int Order = order;
	}


	/// <summary>
	/// When the entity despawn, reset the position in map. So next time it will be load from the position where it last appeared
	/// </summary>
	/// <param name="requireReposition">Disable reposition by setting this to False</param>
	[AttributeUsage(AttributeTargets.Class)]
	public class RepositionWhenInactiveAttribute (bool requireReposition = true) : Attribute {
		internal bool RequireReposition = requireReposition;
	}


	/// <summary>
	/// Player can use "Spawn---" to spawn this entity to stage.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class SpawnWithCheatCodeAttribute : Attribute { }


}