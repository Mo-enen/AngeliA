using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Entity {


		// SUB
		public delegate RectInt RectIntHandler ();
		public delegate void EntityLayerHandler (Entity entity, EntityLayer layer);

		// Api
		public static RectIntHandler GetSpawnRect { get; set; } = null;
		public static RectIntHandler GetViewRect { get; set; } = null;
		public static RectIntHandler GetCameraRect { get; set; } = null;
		public static EntityLayerHandler AddNewEntity { get; set; } = null;
		public bool Active { get; set; } = true;
		public virtual bool Despawnable { get; } = true;
		public int X { get; set; } = 0;
		public int Y { get; set; } = 0;

		// API
		public virtual void OnCreate () { }
		public virtual void FrameUpdate () { }
		public virtual void FillPhysics () { }


	}
}
