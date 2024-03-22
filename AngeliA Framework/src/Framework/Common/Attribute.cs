namespace AngeliA;


// Project
[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaGameTitleAttribute : System.Attribute {
	public readonly string Title = "";
	public readonly string DisTitle = "";
	public AngeliaGameTitleAttribute (string title, string displayTitle = "") {
		Title = title;
		DisTitle = string.IsNullOrEmpty(displayTitle) ? title : displayTitle;
	}
}



[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaGameDeveloperAttribute : System.Attribute {
	public readonly string Developer = "";
	public readonly string DisName = "";
	public AngeliaGameDeveloperAttribute (string developer, string displayName = "") {
		Developer = developer;
		DisName = string.IsNullOrEmpty(displayName) ? developer : displayName;
	}
}


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaVersionAttribute : System.Attribute {
	public readonly Int3 Version = Int3.zero;
	public AngeliaVersionAttribute (int majorVersion, int minorVersion, int patchVersion) => Version = new Int3(majorVersion, minorVersion, patchVersion);
}



[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaAllowMakerFeaturesAttribute : System.Attribute { }


[System.AttributeUsage(System.AttributeTargets.Assembly)]
public class AngeliaProjectTypeAttribute : System.Attribute {
	public readonly ProjectType Type = ProjectType.Game;
	public AngeliaProjectTypeAttribute (ProjectType type) => Type = type;
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