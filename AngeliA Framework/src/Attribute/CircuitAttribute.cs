using System;

namespace AngeliA;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CircuitOperatorAttribute : Attribute { }

public class OnCircuitWireActived : EventAttribute { }

public class OnCircuitOperatorTriggered : EventAttribute { }

