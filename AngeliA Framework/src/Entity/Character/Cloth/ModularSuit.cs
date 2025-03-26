namespace AngeliA;


/// <summary>
/// Body cloth that auto generate from artwork sheet
/// </summary>
public sealed class ModularBodySuit : BodyCloth, IModularCloth {
	public string ModularName => "BodySuit";
}

/// <summary>
/// Shoes that auto generate from artwork sheet
/// </summary>
public sealed class ModularFootSuit : FootCloth, IModularCloth {
	public string ModularName => "FootSuit";
}

/// <summary>
/// Gloves that auto generate from artwork sheet
/// </summary>
public sealed class ModularHandSuit : HandCloth, IModularCloth {
	public string ModularName => "HandSuit";
}

/// <summary>
/// Hat that auto generate from artwork sheet
/// </summary>
public sealed class ModularHeadSuit : HeadCloth, IModularCloth {
	public string ModularName => "HeadSuit";
}

/// <summary>
/// Pants or skirt that auto generate from artwork sheet
/// </summary>
public sealed class ModularHipSuit : HipCloth, IModularCloth {
	public string ModularName => "HipSuit";
}
