using System;

[AttributeUsage(AttributeTargets.Method)] public class OnQuitAttribute : Attribute { }
[AttributeUsage(AttributeTargets.Method)] public class OnTryingToQuitAttribute : Attribute { }