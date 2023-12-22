using System.Reflection;


namespace AngeliaFramework {


	[System.AttributeUsage(System.AttributeTargets.Assembly)]
	public class AngeliAAttribute : UnityEngine.PropertyAttribute { }


	[System.AttributeUsage(System.AttributeTargets.Assembly)]
	public class AngeliaGameTitleAttribute : UnityEngine.PropertyAttribute {
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
	public class AngeliaGameDeveloperAttribute : UnityEngine.PropertyAttribute {
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