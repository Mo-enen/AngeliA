using System;

namespace AngeliA;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CircuitOperator_Int3UnitPos_IntStampAttribute : Attribute { }

public class OnCircuitWireActived_Int3UnitPosAttribute : EventAttribute { }

public class OnCircuitOperatorTriggered_Int3UnitPosAttribute : EventAttribute { }

