using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Entities {
	public abstract class Entity {


		// SUB
		public delegate void EntityLayerHandler (Entity entity, EntityLayer layer);
		public delegate ScriptableObject ScriptableObjectIntHandler (int value);

		// Api
		public static EntityLayerHandler AddNewEntity { get; set; } = null;
		public static ScriptableObjectIntHandler GetAsset { get; set; } = null;
		public static RectInt SpawnRect { get; set; }
		public static RectInt ViewRect { get; set; }
		public static RectInt CameraRect { get; set; }
		public bool Active { get; set; } = true;
		public virtual bool Despawnable { get; } = true;
		public int X { get; set; } = 0;
		public int Y { get; set; } = 0;

		// API
		public virtual void OnCreate (int frame) { }
		public virtual void FrameUpdate (int frame) { }
		public virtual void FillPhysics (int frame) { }


	}
}
