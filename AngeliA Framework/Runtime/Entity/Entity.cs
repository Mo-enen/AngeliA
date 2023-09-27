using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[EntityAttribute.Capacity(1024, 0)]
	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL)]
	[EntityAttribute.MapEditorGroup("Entity")]
	public abstract class Entity : IMapEditorItem {


		// Api
		public bool Active { get; set; } = true;
		public int X { get; set; } = 0;
		public int Y { get; set; } = 0;
		public int Width { get; set; } = Const.CEL;
		public int Height { get; set; } = Const.CEL;
		public int TypeID => _TypeID != 0 ? _TypeID : (_TypeID = GetType().AngeHash());
		public int SpawnFrame { get; internal set; } = int.MinValue;
		public bool FromWorld => InstanceID.x != int.MinValue;
		public virtual RectInt Rect => new(X, Y, Width, Height);
		public RectInt GlobalBounds => LocalBounds.Shift(X, Y);

		// Inter
		internal Vector3Int InstanceID { get; set; } = default;
		internal RectInt LocalBounds { get; set; } = default;
		internal bool FrameUpdated { get; set; } = false;
		internal int PhysicsOperationStamp { get; set; } = int.MaxValue;
		internal bool DespawnOutOfRange { get; set; } = true;
		internal bool UpdateOutOfRange { get; set; } = false;
		internal int Order { get; set; } = 0;

		// Data
		private int _TypeID = 0;

		// MSG
		public Entity () { }
		public virtual void OnActivated () { }
		public virtual void OnInactivated () { }
		public virtual void FillPhysics () { }
		public virtual void BeforePhysicsUpdate () { }
		public virtual void PhysicsUpdate () { }
		public virtual void FrameUpdate () { }


	}
}