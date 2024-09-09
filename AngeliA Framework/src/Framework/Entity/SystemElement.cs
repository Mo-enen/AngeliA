using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.MapEditorGroup("System")]
public abstract class SystemElement : IMapItem { }

// Number
public class NumberZero : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberZero).AngeHash();
}
public class NumberOne : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberOne).AngeHash();
}
public class NumberTwo : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberTwo).AngeHash();
}
public class NumberThree : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberThree).AngeHash();
}
public class NumberFour : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberFour).AngeHash();
}
public class NumberFive : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberFive).AngeHash();
}
public class NumberSix : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberSix).AngeHash();
}
public class NumberSeven : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberSeven).AngeHash();
}
public class NumberEight : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberEight).AngeHash();
}
public class NumberNine : SystemElement {
	public static readonly int TYPE_ID = typeof(NumberNine).AngeHash();
}

// Direction
public class CameraAutoDirection : SystemElement { }
