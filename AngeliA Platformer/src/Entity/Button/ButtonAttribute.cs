using System;
using AngeliA;

namespace AngeliA.Platformer;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ButtonOperatorAttribute : Attribute { }


public class OnButtonWireActivedAttribute : EventAttribute { }
public class OnButtonOperatorTriggeredAttribute : EventAttribute { }

