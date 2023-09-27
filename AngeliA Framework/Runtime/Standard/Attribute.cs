namespace AngeliaFramework {


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class DisableAttribute : UnityEngine.PropertyAttribute { }



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class NullAlertAttribute : UnityEngine.PropertyAttribute { }



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class ClampAttribute : UnityEngine.PropertyAttribute {
		public readonly float Min, Max;
		public ClampAttribute (float min, float max) {
			Min = min;
			Max = max;
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class HelpAttribute : UnityEngine.PropertyAttribute {
		public readonly string Link;
		public HelpAttribute (string link) {
			Link = link;
		}
	}


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class DisableAtRuntimeAttribute : UnityEngine.PropertyAttribute { }


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class DisableAtEdittimeAttribute : UnityEngine.PropertyAttribute { }


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class HideAtRuntimeAttribute : UnityEngine.PropertyAttribute { }


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class HideAtEdittimeAttribute : UnityEngine.PropertyAttribute { }



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class DisplayLabelAttribute : UnityEngine.PropertyAttribute {
		public string Name = "";
		public string IconPath = "";
		public DisplayLabelAttribute (string name, string iconPath = "") {
			Name = name;
			IconPath = iconPath;
		}
	}

}