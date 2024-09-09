namespace AngeliA;



public sealed class MapGeneratorStarter_Free : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_Free).AngeHash();
	public override Direction8? Direction => null;
}


public sealed class MapGeneratorStarter_Left : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_Left).AngeHash();
	public override Direction8? Direction => Direction8.Left;
}
public sealed class MapGeneratorStarter_Right : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_Right).AngeHash();
	public override Direction8? Direction => Direction8.Right;
}
public sealed class MapGeneratorStarter_Bottom : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_Bottom).AngeHash();
	public override Direction8? Direction => Direction8.Bottom;
}
public sealed class MapGeneratorStarter_Top : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_Top).AngeHash();
	public override Direction8? Direction => Direction8.Top;
}


public sealed class MapGeneratorStarter_BottomLeft : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_BottomLeft).AngeHash();
	public override Direction8? Direction => Direction8.BottomLeft;
}
public sealed class MapGeneratorStarter_BottomRight : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_BottomRight).AngeHash();
	public override Direction8? Direction => Direction8.BottomRight;
}
public sealed class MapGeneratorStarter_TopLeft : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_TopLeft).AngeHash();
	public override Direction8? Direction => Direction8.TopLeft;
}
public sealed class MapGeneratorStarter_TopRight : MapGeneratorStarter {
	public static readonly int TYPE_ID = typeof(MapGeneratorStarter_TopRight).AngeHash();
	public override Direction8? Direction => Direction8.TopRight;
}



public abstract class MapGeneratorStarter : SystemElement {
	public abstract Direction8? Direction { get; }
}
