using System.Reflection;


namespace AngeliaFramework {


	[System.AttributeUsage(System.AttributeTargets.Assembly)]
	public class AngeliAAttribute : System.Attribute { }


	[System.AttributeUsage(System.AttributeTargets.Assembly)]
	public class AngeliaGameTitleAttribute : System.Attribute {
		public string Title;
		public AngeliaGameTitleAttribute (string title) => Title = title;
		public static string GetTitle () {
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				var att = assembly.GetCustomAttribute<AngeliaGameTitleAttribute>();
				if (att != null) return att.Title;
			}
			return "";
		}
	}


	[System.AttributeUsage(System.AttributeTargets.Assembly)]
	public class AngeliaGameDeveloperAttribute : System.Attribute {
		public string Developer;
		public AngeliaGameDeveloperAttribute (string developer) => Developer = developer;
		public static string GetDeveloper () {
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				var att = assembly.GetCustomAttribute<AngeliaGameDeveloperAttribute>();
				if (att != null) return att.Developer;
			}
			return "";
		}
	}


}