using System;
using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliaEngine;


internal class OnProjectBuiltInBackgroundAttribute (int order = 0) : EventAttribute(order) { }


[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field, AllowMultiple = false)]
internal class EngineSettingAttribute : Attribute {
	public string Group;
	public string DisplayLabel;
	public string RequireSettingName;
	public bool GameOnly;
	public EngineSettingAttribute () {
		Group = null;
		DisplayLabel = null;
		GameOnly = false;
	}
	public EngineSettingAttribute (string group, string displayLabel, string requireSettingPath = "", bool gameOnly = false) {
		Group = group;
		DisplayLabel = displayLabel;
		RequireSettingName = requireSettingPath;
		GameOnly = gameOnly;
	}
}
