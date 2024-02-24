using System;

[AttributeUsage(System.AttributeTargets.Method)] public class OnQuitAttribute : Attribute { }
[AttributeUsage(System.AttributeTargets.Method)] public class OnTryingToQuitAttribute : Attribute { }