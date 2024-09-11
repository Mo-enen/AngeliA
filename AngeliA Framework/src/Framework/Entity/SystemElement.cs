using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.MapEditorGroup("System")]
public abstract class SystemElement : IMapItem { }

// Number
public sealed class NumberZero : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberZero).AngeHash();
}
public sealed class NumberOne : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberOne).AngeHash();
}
public sealed class NumberTwo : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberTwo).AngeHash();
}
public sealed class NumberThree : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberThree).AngeHash();
}
public sealed class NumberFour : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberFour).AngeHash();
}
public sealed class NumberFive : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberFive).AngeHash();
}
public sealed class NumberSix : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberSix).AngeHash();
}
public sealed class NumberSeven : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberSeven).AngeHash();
}
public sealed class NumberEight : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberEight).AngeHash();
}
public sealed class NumberNine : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberNine).AngeHash();
}

// Direction
public sealed class CameraAutoDirection : SystemElement { }
