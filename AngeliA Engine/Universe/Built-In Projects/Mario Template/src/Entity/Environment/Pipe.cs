using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Platformer;

namespace MarioTemplate;


public class PipeLeft : CarryingPipe {
	protected override Direction4 Direction => Direction4.Left;
	private static readonly SpriteCode EDGE_SP = "Pipe.Edge";
	private static readonly SpriteCode MID_SP = "Pipe.Mid";
	private static readonly SpriteCode INSERT_SP = "Pipe.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
}


public class PipeRight : CarryingPipe {
	protected override Direction4 Direction => Direction4.Right;
	private static readonly SpriteCode EDGE_SP = "Pipe.Edge";
	private static readonly SpriteCode MID_SP = "Pipe.Mid";
	private static readonly SpriteCode INSERT_SP = "Pipe.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
}


public class PipeDown : CarryingPipe {
	protected override Direction4 Direction => Direction4.Down;
	private static readonly SpriteCode EDGE_SP = "Pipe.Edge";
	private static readonly SpriteCode MID_SP = "Pipe.Mid";
	private static readonly SpriteCode INSERT_SP = "Pipe.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
}


public class PipeUp : CarryingPipe {
	protected override Direction4 Direction => Direction4.Up;
	private static readonly SpriteCode EDGE_SP = "Pipe.Edge";
	private static readonly SpriteCode MID_SP = "Pipe.Mid";
	private static readonly SpriteCode INSERT_SP = "Pipe.Insert";
	protected override SpriteCode EdgeSprite => EDGE_SP;
	protected override SpriteCode MidSprite => MID_SP;
	protected override SpriteCode InsertSprite => INSERT_SP;
}

