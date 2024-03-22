using System.Collections;
using System.Collections.Generic;

[assembly: AngeliA.RequireGlobalSprite(atlas: "Entity", "Entity")]

namespace AngeliA; 


public interface IMapItem { }


[EntityAttribute.Capacity(1024, 0)]
[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL)]
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
	public int SpawnFrame { get; internal set; } = int.MinValue;
	public bool FromWorld => InstanceID.x != int.MinValue;
	public virtual IRect Rect => new(X, Y, Width, Height);
	public IRect GlobalBounds => LocalBounds.Shift(X, Y);
	public int InstanceOrder => FromWorld ? 0 : InstanceID.y;

	// Inter
	internal Int3 InstanceID { get; set; } = default;
	internal IRect LocalBounds { get; set; } = default;
	internal bool FrameUpdated { get; set; } = false;
	internal int PhysicsOperationStamp { get; set; } = int.MaxValue;
	internal bool DestroyOnZChanged { get; set; } = true;
	internal bool DespawnOutOfRange { get; set; } = true;
	internal bool UpdateOutOfRange { get; set; } = false;
	internal int Order { get; set; } = 0;

	// MSG
	public Entity () => TypeID = GetType().AngeHash();
	public virtual void OnActivated () { }
	public virtual void OnInactivated () { }
	public virtual void FirstUpdate () { }
	public virtual void BeforeUpdate () { }
	public virtual void Update () { }
	public virtual void LateUpdate () { }


}