namespace AngeliA;

/// <summary>
/// Horn body gadget that auto generate from artwork sheet
/// </summary>
public sealed class ModularHorn : Horn, IModularBodyGadget {
	string IModularBodyGadget.ModularName => "Horn";
}
