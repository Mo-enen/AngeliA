namespace AngeliA;

/// <summary>
/// Hair body gadget that auto generate from artwork sheet
/// </summary>
public sealed class ModularHair : Hair, IModularBodyGadget {
	string IModularBodyGadget.ModularName => "Hair";
}
