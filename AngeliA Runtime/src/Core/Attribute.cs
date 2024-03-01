using System;
using AngeliA;

namespace AngeliaRuntime;


[AttributeUsage(AttributeTargets.Assembly)]
public class RequireEventWaitingAttribute : GlobalMarkAttribute {
	public static bool Required => IsMarked<RequireEventWaitingAttribute>(ref _Require);
	private static bool? _Require = null;
}


[AttributeUsage(AttributeTargets.Assembly)]
public class RequireTransparentWindowAttribute : GlobalMarkAttribute {
	public static bool Required => IsMarked<RequireTransparentWindowAttribute>(ref _Require);
	private static bool? _Require = null;
}