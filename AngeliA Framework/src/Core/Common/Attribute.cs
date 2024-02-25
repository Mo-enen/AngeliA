using System.Reflection;
using System.Linq;

namespace AngeliA;


// Project
[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliAAttribute : System.Attribute { }



[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaGameTitleAttribute : System.Attribute {
	public string Title;
	public AngeliaGameTitleAttribute (string title) => Title = title;
	public static string GetTitle () {
		foreach (var assembly in Util.AllAssemblies) {
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
	public static string GetDeveloperName () {
		foreach (var assembly in Util.AllAssemblies) {
			var att = assembly.GetCustomAttribute<AngeliaGameDeveloperAttribute>();
			if (att != null) return att.Developer;
		}
		return "";
	}
}



public enum ReleaseLifeCycle { Alpha = 0, Beta = 1, Release = 2, Final = 3, }



[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaVersionAttribute : System.Attribute {
	public static int MajorVersion = 0;
	public static int MinorVersion = 0;
	public static int PatchVersion = 0;
	public static ReleaseLifeCycle? LifeCycle = null;
	public AngeliaVersionAttribute (int majorVersion, int minorVersion, int patchVersion, ReleaseLifeCycle lifeCycle) {
		MajorVersion = majorVersion;
		MinorVersion = minorVersion;
		PatchVersion = patchVersion;
		LifeCycle = lifeCycle;
	}
	public static string GetVersionString (bool prefixV = true, bool lifeCycle = true) =>
		$"{(prefixV ? "v" : "")}{MajorVersion}.{MinorVersion}.{PatchVersion}{(lifeCycle ? LifeCycle switch {
			ReleaseLifeCycle.Alpha => "a",
			ReleaseLifeCycle.Beta => "b",
			ReleaseLifeCycle.Final => "f",
			_ => "",
		} : "")}";
}



[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaAllowMakerAttribute : System.Attribute {
	public static bool AllowMakerFeatures => Util.AllAssemblies.Any(
		a => a.GetCustomAttribute<AngeliaAllowMakerAttribute>() != null
	);
}



// Item
[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
public class ItemCombinationAttribute : System.Attribute {
	public System.Type ItemA = null;
	public System.Type ItemB = null;
	public System.Type ItemC = null;
	public System.Type ItemD = null;
	public int Count = 1;
	public bool ConsumeA = true;
	public bool ConsumeB = true;
	public bool ConsumeC = true;
	public bool ConsumeD = true;
	public ItemCombinationAttribute (System.Type itemA, int count = 1, bool consumeA = true) : this(itemA, null, null, null, count, consumeA, true, true, true) { }
	public ItemCombinationAttribute (System.Type itemA, System.Type itemB, int count = 1, bool consumeA = true, bool consumeB = true) : this(itemA, itemB, null, null, count, consumeA, consumeB, true, true) { }
	public ItemCombinationAttribute (System.Type itemA, System.Type itemB, System.Type itemC, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true) : this(itemA, itemB, itemC, null, count, consumeA, consumeB, consumeC, true) { }
	public ItemCombinationAttribute (System.Type itemA, System.Type itemB, System.Type itemC, System.Type itemD, int count = 1, bool consumeA = true, bool consumeB = true, bool consumeC = true, bool consumeD = true) {
		ItemA = itemA;
		ItemB = itemB;
		ItemC = itemC;
		ItemD = itemD;
		Count = count;
		ConsumeA = consumeA;
		ConsumeB = consumeB;
		ConsumeC = consumeC;
		ConsumeD = consumeD;
	}
}