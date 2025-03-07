namespace AngeliA;

[EntityAttribute.MapEditorGroup("System")]
public abstract class BlockColor : IMapItem {
	public abstract Color32 Color { get; }
}

public sealed class BlockColorWhite : BlockColor {

	public static readonly int TYPE_ID = typeof(BlockColorWhite).AngeHash();
	public override Color32 Color => Color32.WHITE;
}
public sealed class BlockColorBlack : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorBlack).AngeHash();
	public override Color32 Color => Color32.BLACK;
}
public sealed class BlockColorGrey : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorGrey).AngeHash();
	public override Color32 Color => Color32.GREY_128;
}
public sealed class BlockColorRed : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorRed).AngeHash();
	public override Color32 Color => Color32.RED;
}
public sealed class BlockColorOrange : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorOrange).AngeHash();
	public override Color32 Color => Color32.ORANGE;
}
public sealed class BlockColorYellow : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorYellow).AngeHash();
	public override Color32 Color => Color32.YELLOW;
}
public sealed class BlockColorGreen : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorGreen).AngeHash();
	public override Color32 Color => Color32.GREEN;
}
public sealed class BlockColorCyan : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorCyan).AngeHash();
	public override Color32 Color => Color32.CYAN;
}
public sealed class BlockColorBlue : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorBlue).AngeHash();
	public override Color32 Color => Color32.BLUE;
}
public sealed class BlockColorPurple : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorPurple).AngeHash();
	public override Color32 Color => Color32.PURPLE;
}
public sealed class BlockColorPink : BlockColor {
	public static readonly int TYPE_ID = typeof(BlockColorPink).AngeHash();
	public override Color32 Color => Color32.PINK;
}

