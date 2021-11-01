using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public class TestA : Entity {


	}


	public class TestB : Entity {


	}

	public class TestC : Entity {


	}

	public class TestCC : Entity {


	}




	public abstract class Entity {


		// SUB
		public delegate (int sheet, int id) IntIntStringHandler (string str);
		public delegate Entity EntityTypeLayerHandler (System.Type type, EntityLayer layer);

		// Api
		public static IntIntStringHandler GetSpriteSheetAndID { get; set; } = null;
		public static EntityTypeLayerHandler CreateEntity { get; set; } = null;
		public bool Active { get; set; } = true;
		public int X { get; set; } = 0;
		public int Y { get; set; } = 0;
		public int Rotation { get; set; } = 0;


		// MSG
		public virtual void FrameUpdate () { }
		public virtual void FillPhysics () { }


		// API
		public void Destroy () => Active = false;


	}


}
