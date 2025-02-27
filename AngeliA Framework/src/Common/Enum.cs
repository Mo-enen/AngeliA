namespace AngeliA;


public enum WrapMode : byte {
	NoWrap = 0,
	Wrap = 1,
	WordWrap = 2,
}


public enum GUIState : byte {
	Normal = 0,
	Hover = 1,
	Press = 2,
	Disable = 3,
}


public enum Direction2 : sbyte {
	Negative = -1,
	Left = -1,
	Down = -1,
	Horizontal = -1,
	Positive = 1,
	Up = 1,
	Right = 1,
	Vertical = 1,
}


public enum Direction3 : sbyte {
	None = 0,
	Negative = -1,
	Left = -1,
	Down = -1,
	Horizontal = -1,
	Positive = 1,
	Up = 1,
	Right = 1,
	Vertical = 1,
}


public enum Direction4 : byte {
	Up = 0,
	Down = 1,
	Left = 2,
	Right = 3,
}


public enum Direction5 : byte {
	Center = 0,
	Up = 1,
	Down = 2,
	Left = 3,
	Right = 4,
}


public enum Direction8 : byte {
	Top = 0,
	TopRight = 1,
	Right = 2,
	BottomRight = 3,
	Bottom = 4,
	BottomLeft = 5,
	Left = 6,
	TopLeft = 7,
}


public enum Alignment : byte {
	TopLeft = 0,
	TopMid = 1,
	TopRight = 2,
	MidLeft = 3,
	MidMid = 4,
	MidRight = 5,
	BottomLeft = 6,
	BottomMid = 7,
	BottomRight = 8,
	Full = 9,
}



public enum FittingPose {
	Unknown = 0,
	Left = 1,
	Down = 1,
	Mid = 2,
	Right = 3,
	Up = 3,
	Single = 4,
}



