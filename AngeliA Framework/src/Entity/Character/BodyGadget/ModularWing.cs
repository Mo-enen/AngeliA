namespace AngeliA;

/// <summary>
/// Wing body gadget that auto generate from artwork sheet
/// </summary>
public sealed class ModularWing : Wing, IModularBodyGadget {
	string IModularBodyGadget.ModularName => "Wing";
}
