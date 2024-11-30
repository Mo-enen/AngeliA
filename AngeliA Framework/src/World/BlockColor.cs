namespace AngeliA;

[EntityAttribute.MapEditorGroup("BlockColor", -512)]
public abstract class BlockColor : SystemElement, IMapItem {
	bool IMapItem.Avaliable => BlockColoringSystem.Enable;
	public abstract Color32 Color { get; }
}

public sealed class BlockColorWhite : BlockColor { public override Color32 Color => Color32.WHITE; }
public sealed class BlockColorBlack : BlockColor { public override Color32 Color => Color32.BLACK; }
public sealed class BlockColorGrey : BlockColor { public override Color32 Color => Color32.GREY_128; }
public sealed class BlockColorRed : BlockColor { public override Color32 Color => Color32.RED; }
public sealed class BlockColorOrange : BlockColor { public override Color32 Color => Color32.ORANGE; }
public sealed class BlockColorYellow : BlockColor { public override Color32 Color => Color32.YELLOW; }
public sealed class BlockColorGreen : BlockColor { public override Color32 Color => Color32.GREEN; }
public sealed class BlockColorCyan : BlockColor { public override Color32 Color => Color32.CYAN; }
public sealed class BlockColorBlue : BlockColor { public override Color32 Color => Color32.BLUE; }
public sealed class BlockColorPurple : BlockColor { public override Color32 Color => Color32.PURPLE; }
public sealed class BlockColorPink : BlockColor { public override Color32 Color => Color32.PINK; }

