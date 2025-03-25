namespace AngeliA;

/// <summary>
/// Ear body gadget that auto generate from artwork sheet
/// </summary>
public sealed class ModularEar : Ear, IModularBodyGadget {
	string IModularBodyGadget.ModularName => "Ear";
}
