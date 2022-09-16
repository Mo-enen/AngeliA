using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace PixelJelly {



	// Class
	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
	public class CustomJellyInspectorAttribute : System.Attribute {
		public System.Type TargetType;
		public CustomJellyInspectorAttribute (System.Type targetType) {
			TargetType = targetType;
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class HideRandomButtonAttribute : System.Attribute { }


	[System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class OneShotAnimationAttribute : System.Attribute { }


	// Inspector
	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class HideInPixelJellyAttribute : System.Attribute { }



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class NullAlertAttribute : PropertyAttribute { }




	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class RandomButtonAttribute : PropertyAttribute {
		public float Min = 0f;
		public float Max = 1f;
		public int MinInt = int.MinValue;
		public int MaxInt = int.MaxValue;
		public RandomButtonAttribute () : this(int.MinValue, int.MaxValue) { }
		public RandomButtonAttribute (int min, int max) {
			MinInt = min;
			MaxInt = max;
			Min = min;
			Max = max;
		}
		public RandomButtonAttribute (float min, float max) {
			MinInt = (int)min;
			MaxInt = (int)max;
			Min = min;
			Max = max;
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class ArrowNumberAttribute : PropertyAttribute {
		public bool Enabled;
		public float Min;
		public float Max;
		public float Step;
		public int MinInt;
		public int MaxInt;
		public int StepInt;
		public bool Loop;
		public ArrowNumberAttribute () : this(0) { }
		public ArrowNumberAttribute (bool loop) : this(0) {
			Loop = loop;
		}
		public ArrowNumberAttribute (float min = float.MinValue, float max = float.MaxValue, float step = 1f, bool enabled = true, bool loop = false) {
			Min = min;
			Max = max;
			Enabled = enabled;
			Step = Mathf.Approximately(step, 0f) ? 1f : step;
			Loop = loop;
		}
		public ArrowNumberAttribute (int min = int.MinValue, int max = int.MaxValue, int step = 1, bool enabled = true, bool loop = false) {
			MinInt = min;
			MaxInt = max;
			Enabled = enabled;
			StepInt = step;
			Loop = loop;
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class ClampAttribute : PropertyAttribute {
		public float Min;
		public float Max;
		public int MinInt;
		public int MaxInt;
		public ClampAttribute (float min = float.MinValue, float max = float.MaxValue) {
			Min = min;
			Max = max;
		}
		public ClampAttribute (int min = int.MinValue, int max = int.MaxValue) {
			MinInt = min;
			MaxInt = max;
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class MinMaxSliderAttribute : PropertyAttribute {
		public float Min;
		public float Max;
		public MinMaxSliderAttribute (float min = float.MinValue, float max = float.MaxValue) {
			Min = min;
			Max = max;
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class EnumSwicherAttribute : PropertyAttribute { }




	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class ColorGradientAttribute : PropertyAttribute {
		public bool ThumbnailOnly = false;
		public float ThumbnailWidth = 32;
		public ColorGradientAttribute (bool thumbnailOnly = false, float thumbnailWidth = 32) {
			ThumbnailOnly = thumbnailOnly;
			ThumbnailWidth = thumbnailWidth;
		}
	}


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class PixelGalleryAttribute : PropertyAttribute { }


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class GradientGalleryAttribute : PropertyAttribute {
		public int Column;
		public GradientGalleryAttribute (int column = 7) {
			Column = column;
		}
	}


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class RectOffsetAttribute : PropertyAttribute { }




	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class PixelEditorAttribute : PropertyAttribute {
		public bool UseLabel = true;
		public bool Center = false;
		public bool FitBackground = true;
		public PixelEditorAttribute (bool useLabel = true, bool center = false, bool fitBackground = true) {
			UseLabel = useLabel;
			Center = center;
			FitBackground = fitBackground;
		}
	}



	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class MinMaxNumberAttribute : PropertyAttribute {
		public int MinInt;
		public int MaxInt;
		public int GapInt;
		public float Min;
		public float Max;
		public float Gap;
		public bool IgnoreMinMaxCompare;
		public MinMaxNumberAttribute (int min = int.MinValue, int max = int.MaxValue, int gap = 1, bool ignoreCompare = false) {
			MinInt = min;
			MaxInt = max;
			GapInt = gap;
			IgnoreMinMaxCompare = ignoreCompare;
		}
		public MinMaxNumberAttribute (float min = float.MinValue, float max = float.MaxValue, float gap = 0.1f, bool ignoreCompare = false) {
			Min = min;
			Max = max;
			Gap = gap;
			IgnoreMinMaxCompare = ignoreCompare;
		}
	}


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class LabelOnlyAttribute : PropertyAttribute { }


	[System.AttributeUsage(System.AttributeTargets.Field)]
	public class LabelAttribute : PropertyAttribute {
		public string Label = "";
		public LabelAttribute (string label) {
			Label = label;
		}
	}


}


