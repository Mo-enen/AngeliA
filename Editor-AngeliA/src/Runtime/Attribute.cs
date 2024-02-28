using System;
using AngeliA;

[AttributeUsage(AttributeTargets.Method)] public class OnQuitAttribute : OrderedAttribute { public OnQuitAttribute (int order = 0) : base(order) { } }