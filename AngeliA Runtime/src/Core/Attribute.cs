using System;
using AngeliA;

namespace AngeliaRuntime;


[AttributeUsage(AttributeTargets.Assembly)]
public class RequireEventWaitingAttribute : Attribute { }


[AttributeUsage(AttributeTargets.Assembly)]
public class RequireTransparentWindowAttribute : Attribute { }