using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	// Attribute
	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public class AtlasAttribute : System.Attribute {
		public string Name;
		public AtlasAttribute (string name) => Name = name;
	}


	[System.AttributeUsage(System.AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
	public class RequireSpriteAttribute : System.Attribute {
		public string[] Names; // {0} = (GetType().DeclaringType ?? GetType()).AngeName()   {1} = GetType().AngeName()
		public RequireSpriteAttribute (params string[] names) => Names = names;
	}


	[System.AttributeUsage(System.AttributeTargets.Assembly, AllowMultiple = true)]
	public class RequireGlobalSpriteAttribute : System.Attribute {
		public string[] Names;
		public string AtlasName;
		public RequireGlobalSpriteAttribute (string atlasName, params string[] names) {
			AtlasName = atlasName;
			Names = names;
		}
	}


	// Data
	public class SpriteCode {
		public readonly string Name;
		public readonly int ID;
		public SpriteCode (string name) {
			Name = name;
			ID = name.AngeHash();
		}
		public static implicit operator SpriteCode (string value) => new(value);
		public static implicit operator int (SpriteCode code) => code.ID;
	}

}