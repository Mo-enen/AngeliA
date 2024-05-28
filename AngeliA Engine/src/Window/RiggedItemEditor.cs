using AngeliA;

namespace AngeliaEngine;

[RequireLanguageFromField]
public class RiggedItemEditor : WindowUI {
	public override string DefaultName => "Item";
	public override void UpdateWindowUI () { 
		Sky.ForceSkyboxTint(GUI.Skin.Background); 
	}
}