using AngeliA;

namespace AngeliaEngine;

[RequireLanguageFromField]
public class RiggedMapEditor : WindowUI {
	public override string DefaultName => "Map Editor";
	public static RiggedMapEditor Instance { get; private set; }
	public RiggedMapEditor () => Instance = this;
	public override void UpdateWindowUI () { }
}