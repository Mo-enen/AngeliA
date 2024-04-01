using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static partial class GUISkin {


	[System.Flags]
	public enum LabelPattern : int {
		Default = 0,
		Normal = 0, Small = 0b01, Large = 0b10, Auto = 0b11,
		Light = 0, Grey = 0b0100, Dark = 0b1000,
		MidLeft = 0, MidMid = 0b010000, TopLeft = 0b100000, TopMid = 0b110000,
		WordWrap = 0b1000000,
		Clip = 0b10000000,
		NoBorder = 0, SmallBorder = 0b0100000000, Border = 0b1000000000, LargeBorder = 0b1100000000,
	}


	public static GUIStyle ProduceLabel (LabelPattern pattern) {
		bool dark = pattern.HasFlag(LabelPattern.Dark);
		bool grey = pattern.HasFlag(LabelPattern.Grey);
		return new() {
			BodySprite = 0,
			BodySpriteHover = 0,
			BodySpriteDown = 0,
			BodySpriteDisable = 0,

			BodyColor = Color32.CLEAR,
			BodyColorHover = Color32.CLEAR,
			BodyColorDown = Color32.CLEAR,
			BodyColorDisable = Color32.CLEAR,

			ContentColor = dark ? Color32.GREY_32 : grey ? Color32.GREY_128 : Color32.GREY_245,
			ContentColorHover = dark ? Color32.GREY_32 : grey ? Color32.GREY_128 : Color32.GREY_245,
			ContentColorDown = dark ? Color32.GREY_20 : grey ? Color32.GREY_112 : Color32.GREY_216,
			ContentColorDisable = (dark ? Color32.GREY_32 : grey ? Color32.GREY_128 : Color32.GREY_245).WithNewA(128),

			ContentBorder =
				pattern.HasFlag(LabelPattern.LargeBorder) ? Int4.one * Const.ART_SCALE * 4 :
				pattern.HasFlag(LabelPattern.SmallBorder) ? Int4.one * Const.ART_SCALE :
				pattern.HasFlag(LabelPattern.Border) ? Int4.one * Const.ART_SCALE * 2 :
				null,
			CharSize =
				pattern.HasFlag(LabelPattern.Auto) ? -1 :
				pattern.HasFlag(LabelPattern.Large) ? 28 :
				pattern.HasFlag(LabelPattern.Small) ? 14 :
				21,
			Alignment =
				pattern.HasFlag(LabelPattern.TopMid) ? Alignment.TopMid :
				pattern.HasFlag(LabelPattern.MidMid) ? Alignment.MidMid :
				pattern.HasFlag(LabelPattern.TopLeft) ? Alignment.TopLeft : Alignment.MidLeft,
			Wrap = pattern.HasFlag(LabelPattern.WordWrap) ? WrapMode.WordWrap : WrapMode.NoWrap,
			Clip = pattern.HasFlag(LabelPattern.Clip),
		};
	}

	// Label
	public static readonly GUIStyle LargeLabel = ProduceLabel(LabelPattern.Large);
	public static readonly GUIStyle Label = ProduceLabel(LabelPattern.Normal);
	public static readonly GUIStyle SmallLabel = ProduceLabel(LabelPattern.Small);
	public static readonly GUIStyle AutoLabel = ProduceLabel(LabelPattern.Auto);

	// Grey
	public static readonly GUIStyle SmallGreyLabel = ProduceLabel(LabelPattern.Small | LabelPattern.Grey);
	public static readonly GUIStyle GreyLabel = ProduceLabel(LabelPattern.Normal | LabelPattern.Grey);
	public static readonly GUIStyle LargeGreyLabel = ProduceLabel(LabelPattern.Large | LabelPattern.Grey);
	public static readonly GUIStyle AutoGreyLabel = ProduceLabel(LabelPattern.Auto | LabelPattern.Grey);

	// Center Grey
	public static readonly GUIStyle SmallCenterGreyLabel = ProduceLabel(LabelPattern.Small | LabelPattern.Grey | LabelPattern.MidMid);
	public static readonly GUIStyle CenterGreyLabel = ProduceLabel(LabelPattern.Normal | LabelPattern.Grey | LabelPattern.MidMid);
	public static readonly GUIStyle LargeCenterGreyLabel = ProduceLabel(LabelPattern.Large | LabelPattern.Grey | LabelPattern.MidMid);
	public static readonly GUIStyle AutoCenterGreyLabel = ProduceLabel(LabelPattern.Auto | LabelPattern.Grey | LabelPattern.MidMid);

	// Dark
	public static readonly GUIStyle SmallDarkLabel = ProduceLabel(LabelPattern.Small | LabelPattern.Dark);
	public static readonly GUIStyle DarkLabel = ProduceLabel(LabelPattern.Normal | LabelPattern.Dark);
	public static readonly GUIStyle LargeDarkLabel = ProduceLabel(LabelPattern.Large | LabelPattern.Dark);
	public static readonly GUIStyle AutoDarkLabel = ProduceLabel(LabelPattern.Auto | LabelPattern.Dark);


	// Center
	public static readonly GUIStyle LargeCenterLabel = ProduceLabel(LabelPattern.Large | LabelPattern.MidMid);
	public static readonly GUIStyle CenterLabel = ProduceLabel(LabelPattern.Normal | LabelPattern.MidMid);
	public static readonly GUIStyle SmallCenterLabel = ProduceLabel(LabelPattern.Small | LabelPattern.MidMid);
	public static readonly GUIStyle AutoCenterLabel = ProduceLabel(LabelPattern.Auto | LabelPattern.MidMid);

	// MSG
	public static readonly GUIStyle SmallMessage = ProduceLabel(LabelPattern.Small | LabelPattern.TopMid | LabelPattern.WordWrap | LabelPattern.Clip | LabelPattern.SmallBorder);
	public static readonly GUIStyle Message = ProduceLabel(LabelPattern.Normal | LabelPattern.TopMid | LabelPattern.WordWrap | LabelPattern.Clip | LabelPattern.Border);
	public static readonly GUIStyle LargeMessage = ProduceLabel(LabelPattern.Large | LabelPattern.TopMid | LabelPattern.WordWrap | LabelPattern.Clip | LabelPattern.LargeBorder);

	// Text Area
	public static readonly GUIStyle LargeTextArea = ProduceLabel(LabelPattern.Large | LabelPattern.TopLeft | LabelPattern.WordWrap | LabelPattern.Clip);
	public static readonly GUIStyle TextArea = ProduceLabel(LabelPattern.Normal | LabelPattern.TopLeft | LabelPattern.WordWrap | LabelPattern.Clip);
	public static readonly GUIStyle SmallTextArea = ProduceLabel(LabelPattern.Small | LabelPattern.TopLeft | LabelPattern.WordWrap | LabelPattern.Clip);



}