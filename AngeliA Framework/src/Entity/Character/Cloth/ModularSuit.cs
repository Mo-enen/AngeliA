namespace AngeliA;

public sealed class ModularBodySuit : BodyCloth, IModularCloth {
	public string ModularName => "BodySuit";
}

public sealed class ModularFootSuit : FootCloth, IModularCloth {
	public string ModularName => "FootSuit";
}

public sealed class ModularHandSuit : HandCloth, IModularCloth {
	public string ModularName => "HandSuit";
}

public sealed class ModularHeadSuit : HeadCloth, IModularCloth {
	public string ModularName => "HeadSuit";
}

public sealed class ModularHipSuit : HipCloth, IModularCloth {
	public string ModularName => "HipSuit";
}
