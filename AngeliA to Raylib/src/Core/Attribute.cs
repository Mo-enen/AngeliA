using System;
using AngeliA;

namespace AngeliaToRaylib;


[AttributeUsage(AttributeTargets.Assembly)]
public class RequireEventWaitingAttribute : Attribute {
	public static bool Required => _Required ?? (_Required = (Util.TryGetAttributeFromAllAssemblies<RequireEventWaitingAttribute>(out _, out var att) && att.LocalRequired)).Value;
	private static bool? _Required = null;
	private readonly bool LocalRequired = false;
	public RequireEventWaitingAttribute (bool require) => LocalRequired = require;
}


[AttributeUsage(AttributeTargets.Assembly)]
public class RequireTransparentWindowAttribute : Attribute {
	public static bool Required => _Required ?? (_Required = (Util.TryGetAttributeFromAllAssemblies<RequireTransparentWindowAttribute>(out _, out var att) && att.LocalRequired)).Value;
	private static bool? _Required = null;
	private readonly bool LocalRequired = false;
	public RequireTransparentWindowAttribute (bool require) => LocalRequired = require;
}