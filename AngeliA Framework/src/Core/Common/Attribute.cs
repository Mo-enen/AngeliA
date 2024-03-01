using System.Reflection;

namespace AngeliA;


// Project
[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaGameTitleAttribute : System.Attribute {
	public static string Title => !string.IsNullOrEmpty(_Title) ? _Title : _Title = (Util.TryGetAttributeFromAllAssemblies<AngeliaGameTitleAttribute>(out _, out var att) ? att.LocalTitle : "(No Title)");
	private static string _Title = "";
	private readonly string LocalTitle = "";
	public AngeliaGameTitleAttribute (string title) => LocalTitle = title;
}



[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaGameDeveloperAttribute : System.Attribute {
	public static string Developer => !string.IsNullOrEmpty(_Developer) ? _Developer : _Developer = (Util.TryGetAttributeFromAllAssemblies<AngeliaGameDeveloperAttribute>(out _, out var att) ? att.LocalDeveloper : "(No Developer)");
	private static string _Developer = "";
	private readonly string LocalDeveloper = "";
	public AngeliaGameDeveloperAttribute (string developer) => LocalDeveloper = developer;
}


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaVersionAttribute : System.Attribute {
	public static int MajorVersion => Version.x;
	public static int MinorVersion => Version.y;
	public static int PatchVersion => Version.z;
	public static Int3 Version => _Version ?? (_Version = (Util.TryGetAttributeFromAllAssemblies<AngeliaVersionAttribute>(out _, out var att) ? att.LocalVersion : Int3.zero)).Value;
	private static Int3? _Version = null;
	public Int3 LocalVersion = Int3.zero;
	public AngeliaVersionAttribute (int majorVersion, int minorVersion, int patchVersion) => LocalVersion = new Int3(majorVersion, minorVersion, patchVersion);
	public static string GetVersionString (bool prefixV = true) => $"{(prefixV ? "v" : "")}{Version.x}.{Version.y}.{Version.z}";
}



[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaAllowMakerFeaturesAttribute : GlobalMarkAttribute {
	public static bool AllowMakerFeatures => IsMarked<AngeliaAllowMakerFeaturesAttribute>(ref _AllowMakerFeatures);
	private static bool? _AllowMakerFeatures = null;
}


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaDontStartGameAttribute : GlobalMarkAttribute {
	public static bool DontStartGame => IsMarked<AngeliaDontStartGameAttribute>(ref _DontStartGame);
	private static bool? _DontStartGame = null;
}


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public abstract class GlobalMarkAttribute : System.Attribute {
	protected static bool IsMarked<T> (ref bool? cache) where T : GlobalMarkAttribute => cache ?? (cache = Util.TryGetAttributeFromAllAssemblies<T>(out _, out _)).Value;
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