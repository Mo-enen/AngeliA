using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Entity {




		#region --- SUB ---


		public delegate void EntityLayerHandler (Entity entity);
		public delegate ScriptableObject ScriptableObjectIntHandler (int value);


		#endregion




		#region --- VAR ---


		// Api
		public static EntityLayerHandler AddNewEntity { get; set; } = null;
		public static ScriptableObjectIntHandler GetAsset { get; set; } = null;
		public static RectInt SpawnRect { get; set; }
		public static RectInt ViewRect { get; set; }
		public static RectInt CameraRect { get; set; }
		public static Vector2Int MousePosition { get; set; }

		public virtual RectInt Rect {
			get {
				_Rect.x = X;
				_Rect.y = Y;
				_Rect.width = Width;
				_Rect.height = Height;
				return _Rect;
			}
		}

		public bool Active { get; set; } = true;
		public long InstanceID { get; set; } = 0;
		public virtual bool Despawnable { get; } = true;
		public abstract EntityLayer Layer { get; }
		public virtual int X { get; set; } = 0;
		public virtual int Y { get; set; } = 0;
		public virtual int Width { get; set; } = Const.CELL_SIZE;
		public virtual int Height { get; set; } = Const.CELL_SIZE;

		// Data
		private static long CurrentDynamicInstanceID = 0;
		private RectInt _Rect = new();


		#endregion




		#region --- API ---


		public virtual void OnCreate (int frame) { }
		public virtual void FillPhysics (int frame) { }
		public virtual void PhysicsUpdate (int frame) { }
		public virtual void FrameUpdate (int frame) { }
		public virtual void OnDespawn (int frame) { }


		public static long NewDynamicInstanceID () {
			CurrentDynamicInstanceID--;
			return CurrentDynamicInstanceID;
		}


		#endregion




		#region --- LGC ---




		#endregion




	}
}
