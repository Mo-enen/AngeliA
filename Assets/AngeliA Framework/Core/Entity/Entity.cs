using System.Collections;
using System.Collections.Generic;



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


		public delegate int IntStringHandler (string str);
		public static IntStringHandler GetSpriteIndex { get; set; } = null;


	}


}
