namespace AngeliA;

/// <summary>
/// Face body gadget that auto generate from artwork sheet
/// </summary>
public sealed class ModularFace : Face, IModularBodyGadget {
	string IModularBodyGadget.ModularName => "Face";
}
