using System.Reflection;
using System.Linq;


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



	public enum ReleaseLifeCycle { Alpha = 0, Beta = 1, Release = 2, Final = 3, }


	[System.AttributeUsage(System.AttributeTargets.Assembly)]
	public class AngeliaVersionAttribute : System.Attribute {
		public int MajorVersion;
		public int MinorVersion;
		public int PatchVersion;
		public ReleaseLifeCycle LifeCycle;
		public AngeliaVersionAttribute (int majorVersion, int minorVersion, int patchVersion, ReleaseLifeCycle lifeCycle) {
			MajorVersion = majorVersion;
			MinorVersion = minorVersion;
			PatchVersion = patchVersion;
			LifeCycle = lifeCycle;
		}
		public static bool GetVersion (out int major, out int minor, out int patch, out ReleaseLifeCycle lifeCycle) {
			major = -1;
			minor = -1;
			patch = -1;
			lifeCycle = ReleaseLifeCycle.Release;
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				var att = assembly.GetCustomAttribute<AngeliaVersionAttribute>();
				if (att != null) {
					major = att.MajorVersion;
					minor = att.MinorVersion;
					patch = att.PatchVersion;
					lifeCycle = att.LifeCycle;
					return true;
				}
			}
			return false;
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Assembly)]
	public class AngeliaAllowMakerAttribute : System.Attribute {
		public static bool AllowMakerFeatures => System.AppDomain.CurrentDomain.GetAssemblies().Any(
			a => a.GetCustomAttribute<AngeliaGameTitleAttribute>() != null
		);
	}



}