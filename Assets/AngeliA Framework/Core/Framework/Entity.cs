using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {


	public class TestA : Entity {
		public override void FrameUpdate () { }
		public override void FillPhysics () { }
	}


	public class TestB : Entity {
		public override void FrameUpdate () { }
		public override void FillPhysics () { }
	}

	public class TestC : Entity {
		public override void FrameUpdate () { }
		public override void FillPhysics () { }
	}

	public class TestCC : Entity {
		public override void FrameUpdate () { }
		public override void FillPhysics () { }
	}




	public abstract class Entity {


		// SUB
		public delegate int IntStringHandler (string str);
		public delegate Entity EntityTypeLayerHandler (System.Type type, Layer layer);

		// Api
		public static IntStringHandler GetSpriteIndex { get; set; } = null;
		public static EntityTypeLayerHandler CreateEntity { get; set; } = null;
		public bool Active { get; set; } = true;
		public int X { get; set; } = 0;
		public int Y { get; set; } = 0;
		public int PivotX { get; set; } = 0;
		public int PivotY { get; set; } = 0;
		public int Rotation { get; set; } = 0;


		// MSG
		public abstract void FrameUpdate ();
		public abstract void FillPhysics ();


		// API
		public void Destroy () => Active = false;


	}


}
