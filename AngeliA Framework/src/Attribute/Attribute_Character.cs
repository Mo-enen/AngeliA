using System;
using System.Collections.Generic;

namespace AngeliA;

public static class CharacterAttribute {


	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultSelectedPlayerAttribute (int priority = 0) : Attribute { public readonly int Priority = priority; }



}