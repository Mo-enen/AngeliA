using System;
using System.Collections.Generic;

namespace AngeliA;


/// <summary>
/// Attribute for character class
/// </summary>
public static class CharacterAttribute {


	/// <summary>
	/// Make the character as default selected player
	/// </summary>
	/// <param name="priority"></param>
	[AttributeUsage(AttributeTargets.Class)]
	public class DefaultSelectedPlayerAttribute (int priority = 0) : Attribute {
		internal readonly int Priority = priority;
	}


}