using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;


[AttributeUsage(AttributeTargets.Method)]
public class OnProjectBuiltInBackgroundAttribute (int order = 0) : OrderedAttribute(order) { }


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
public class EngineSettingAttribute : Attribute {
	public string Group;
	public string DisplayLabel;
	public string RequireSettingName;
	public EngineSettingAttribute () {
		Group = null;
		DisplayLabel = null;
	}
	public EngineSettingAttribute (string group, string displayLabel, string requireSettingPath = "") {
		Group = group;
		DisplayLabel = displayLabel;
		RequireSettingName = requireSettingPath;
	}
}
