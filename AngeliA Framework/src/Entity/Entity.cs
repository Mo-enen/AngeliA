using System.Collections;
using System.Collections.Generic;


namespace AngeliA;

/// <summary>
/// General object with logic which can spawn into the stage
/// </summary>
[EntityAttribute.Capacity(4096, 0)]
[EntityAttribute.MapEditorGroup("Entity")]
[EntityAttribute.Layer(EntityLayer.GAME)]
public abstract class Entity : IMapItem {


	// Api
	/// <summary>
	/// True if the entity is currently in stage
	/// </summary>
	public bool Active { get; set; } = true;
	/// <summary>
	/// Position X of this entity in global space
	/// </summary>
	public int X { get; set; } = 0;
	/// <summary>
	/// Position Y of this entity in global space
	/// </summary>
	public int Y { get; set; } = 0;
	/// <summary>
	/// Size X of thie entity in global space
	/// </summary>
	public int Width { get; set; } = Const.CEL;
	/// <summary>
	/// Size Y of thie entity in global space
	/// </summary>
	public int Height { get; set; } = Const.CEL;
	/// <summary>
	/// Unique ID represent what type of entity is it
	/// </summary>
	public int TypeID { get; init; }
	/// <summary>
	/// Which frame does this entity get spawned into the stage
	/// </summary>
	public int SpawnFrame { get; internal protected set; } = int.MinValue;
	/// <summary>
	/// True if the entity is spawned by the world squad
	/// </summary>
	public bool FromWorld => InstanceID.x != int.MinValue;
	/// <summary>
	/// Rect position of this entity in global space
	/// </summary>
	public virtual IRect Rect => new(X, Y, Width, Height);
	/// <summary>
	/// Unique index for this entity that distinguish from other same-type entities on stage
	/// </summary>
	public int InstanceOrder => InstanceID.x != int.MinValue ? 0 : InstanceID.y;
	/// <summary>
	/// The position of this entity on the map in unit space
	/// </summary>
	public Int3? MapUnitPos => InstanceID.x != int.MinValue ? InstanceID : null;
	/// <summary>
	/// Unique ID for this entity as a instance on stage
	/// </summary>
	public Int3 InstanceID { get; internal set; } = default;
	/// <summary>
	/// True if the entity do not reposition when it out of view
	/// </summary>
	public bool IgnoreReposition { get; set; } = false;
	/// <summary>
	/// Position in global space
	/// </summary>
	public Int2 XY {
		get => new(X, Y);
		set {
			X = value.x;
			Y = value.y;
		}
	}
	/// <summary>
	/// Size in global space
	/// </summary>
	public Int2 Size {
		get => new(Width, Height);
		set {
			Width = value.x;
			Height = value.y;
		}
	}
	/// <summary>
	/// Center position of the Rect in global space
	/// </summary>
	public Int2 Center => Rect.CenterInt();
	/// <summary>
	/// Center position X of the Rect in global space
	/// </summary>
	public int CenterX => Rect.CenterX();
	/// <summary>
	/// Center position Y of the Rect in global space
	/// </summary>
	public int CenterY => Rect.CenterY();
	/// <summary>
	/// Position that this entity belongs to in unit space. Get the MapUnitPos when it's from world. Get rect center's unit position when not from world.
	/// </summary>
	public Int3 PivotUnitPosition {
		get {
			Int3 pos;
			if (MapUnitPos.HasValue) {
				pos = MapUnitPos.Value;
			} else {
				var center = Center;
				pos = new Int3(center.x.ToUnit(), center.y.ToUnit(), Stage.ViewZ);
			}
			return pos;
		}
	}

	// Inter
	internal byte UpdateStep { get; set; } = 0;
	internal int Stamp { get; set; } = int.MaxValue;
	internal bool DespawnOnZChanged { get; set; } = true;
	internal bool DespawnOutOfRange { get; set; } = true;
	internal bool UpdateOutOfRange { get; set; } = false;
	internal int Order { get; set; } = 0;
	internal int IgnoreDespawnFromMapFrame { get; private set; } = -1;

	// MSG
	public Entity () => TypeID = GetType().AngeHash();
	/// <summary>
	/// This function is called when entity enter the stage
	/// </summary>
	public virtual void OnActivated () { }
	/// <summary>
	/// This function is called when entity leave the stage
	/// </summary>
	public virtual void OnInactivated () { }
	/// <summary>
	/// [1/4] This function is called every frame when entity is in stage. Prioritize using this function to fill collider in to physics system.
	/// </summary>
	public virtual void FirstUpdate () { }  // 0 >> 1
	/// <summary>
	/// [2/4] This function is called every frame when entity is in stage. Prioritize using this function to update physics logic.
	/// </summary>
	public virtual void BeforeUpdate () { } // 1 >> 2
	/// <summary>
	/// [3/4] This function is called every frame when entity is in stage. Prioritize using this function to update physics logic.
	/// </summary>
	public virtual void Update () { }       // 2 >> 3
	/// <summary>
	/// [4/4] This function is called every frame when entity is in stage. Prioritize using this function to render the entity.
	/// </summary>
	public virtual void LateUpdate () { }   // 3 >> 4
	/// <summary>
	/// This function is called when the entity's map position got repositioned by stage
	/// </summary>
	public virtual void AfterReposition (Int3 fromUnitPos, Int3 toUnitPos) { }


	internal void UpdateToFirst () {
		FirstUpdate();
		UpdateStep = 1;
	}
	internal void UpdateToBefore () {
		if (UpdateStep == 0) FirstUpdate();
		BeforeUpdate();
		UpdateStep = 2;
	}
	internal void UpdateToUpdate () {
		if (UpdateStep == 0) FirstUpdate();
		if (UpdateStep <= 1) BeforeUpdate();
		Update();
		UpdateStep = 3;
	}
	internal void UpdateToLate () {
		if (UpdateStep == 0) FirstUpdate();
		if (UpdateStep <= 1) BeforeUpdate();
		if (UpdateStep <= 2) Update();
		LateUpdate();
		UpdateStep = 4;
	}

	/// <summary>
	/// Force this entity not despawn by stage when out of range for given frames long
	/// </summary>
	public void IgnoreDespawnFromMap (int duration = 1) => IgnoreDespawnFromMapFrame = Game.GlobalFrame + duration;

	/// <summary>
	/// Do not force this entity not despawn by stage
	/// </summary>
	public void CancelIgnoreDespawnFromMap () => IgnoreDespawnFromMapFrame = -1;

	/// <summary>
	/// Draw this entity by it's type ID and rect position
	/// </summary>
	public Cell Draw () => Renderer.Draw(TypeID, Rect);

}