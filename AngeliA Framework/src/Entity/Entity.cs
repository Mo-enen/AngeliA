using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


[EntityAttribute.Capacity(1024, 0)]
[EntityAttribute.MapEditorGroup("Entity")]
[EntityAttribute.Layer(EntityLayer.GAME)]
public abstract class Entity : IMapItem {


	// Api
	public bool Active { get; set; } = true;
	public int X { get; set; } = 0;
	public int Y { get; set; } = 0;
	public int Width { get; set; } = Const.CEL;
	public int Height { get; set; } = Const.CEL;
	public int TypeID { get; init; }
	public int SpawnFrame { get; internal protected set; } = int.MinValue;
	public bool FromWorld => InstanceID.x != int.MinValue;
	public virtual IRect Rect => new(X, Y, Width, Height);
	public int InstanceOrder => InstanceID.x != int.MinValue ? 0 : InstanceID.y;
	public Int3? MapUnitPos => InstanceID.x != int.MinValue ? InstanceID : null;
	public Int3 InstanceID { get; internal set; } = default;
	public bool IgnoreReposition { get; set; } = false;
	public Int2 XY {
		get => new(X, Y);
		set {
			X = value.x;
			Y = value.y;
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
	public virtual void OnActivated () { }
	public virtual void OnInactivated () { }
	public virtual void FirstUpdate () { }  // 0 >> 1
	public virtual void BeforeUpdate () { } // 1 >> 2
	public virtual void Update () { }       // 2 >> 3
	public virtual void LateUpdate () { }   // 3 >> 4

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

	public void IgnoreDespawnFromMap (int duration = 1) => IgnoreDespawnFromMapFrame = Game.GlobalFrame + duration;
	public void CancelIgnoreDespawnFromMap () => IgnoreDespawnFromMapFrame = -1;

	public Cell Draw () => Renderer.Draw(TypeID, Rect);

}