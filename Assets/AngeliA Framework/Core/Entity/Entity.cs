using System.Collections;
using System.Collections.Generic;



namespace AngeliaFramework {


	public class TestA : Entity {
		public override void FrameUpdate () {

		}
	}


	public class TestB : Entity {
		public override void FrameUpdate () {

		}
	}

	public class TestC : Entity {
		public override void FrameUpdate () {

		}
	}

	public class TestCC : Entity {
		public override void FrameUpdate () {

		}
	}




	public abstract class Entity {


		// SUB
		public delegate int IntStringHandler (string str);
		public delegate Entity EntityTypeLayerHandler (System.Type type, Layer layer);

		// Api
		public static IntStringHandler GetSpriteIndex { get; set; } = null;
		public static EntityTypeLayerHandler CreateEntity { get; set; } = null;
		public bool Active { get; set; } = true;
		public int PositionX { get; set; } = 0;
		public int PositionY { get; set; } = 0;
		public float PivotX { get; set; } = 0.5f;
		public float PivotY { get; set; } = 0.5f;
		public float Rotation { get; set; } = 0f;
		

		// MSG
		public abstract void FrameUpdate ();


		// API
		public void Destroy () => Active = false;


	}


}
