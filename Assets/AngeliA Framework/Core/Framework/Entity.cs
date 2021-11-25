using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Entity {


		// SUB
		public delegate uint UIntStringHandler (string str);
		public delegate Entity EntityTypeLayerHandler (System.Type type, EntityLayer layer);

		// Api
		public static EntityTypeLayerHandler CreateEntity { get; set; } = null;
		public bool Active { get; set; } = true;
		public int X { get; set; } = 0;
		public int Y { get; set; } = 0;


		// MSG
		public virtual void FrameUpdate () { }
		public virtual void FillPhysics () { }


		// API
		public void Destroy () => Active = false;
		public static int GetGlobalTypeID (System.Type type) => type.FullName.GetAngeliaHashCode();


	}


}
