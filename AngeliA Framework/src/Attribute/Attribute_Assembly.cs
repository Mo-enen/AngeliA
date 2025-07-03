using System;


namespace AngeliA;


/// <summary>
/// Treat specified sprites as bodyset for pose-characters
/// </summary>
/// <param name="name">The name of the bodyset</param>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class BodySetAttribute (string name) : Attribute {
	internal readonly string Name = name;
}
