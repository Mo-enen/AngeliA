using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Entity {


		// Api
		public bool Active { get; set; } = true;
		public virtual bool Despawnable { get; } = true;
		public int X { get; set; } = 0;
		public int Y { get; set; } = 0;
		protected static RectInt SpawnRect { get; private set; }
		protected static RectInt ViewRect { get; private set; }


		// API
		public virtual void FrameUpdate () { }
		public virtual void FillPhysics () { }
		public static void SetSpawnRect (RectInt rect) => SpawnRect = rect;
		public static void SetViewRect (RectInt rect) => ViewRect = rect;


	}
}
