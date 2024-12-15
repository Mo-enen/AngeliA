using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.UpdateOutOfRange]
public abstract class PingPongPlatform : StepTriggerPlatform {

	// Abs
	protected virtual uint SpeedX => 0;
	protected virtual uint SpeedY => 0;
	protected abstract Int2 PingPongDistance { get; }

	// Data
	private Int2 From = default;
	private Int2 To = default;
	private int DurationX = 0;
	private int DurationY = 0;
	private int MovingStartFrame = -1;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		MovingStartFrame = -1;
		From.x = X - PingPongDistance.x / 2;
		From.y = Y - PingPongDistance.y / 2;
		To.x = X + PingPongDistance.x / 2;
		To.y = Y + PingPongDistance.y / 2;
		DurationX = SpeedX > 0 ? PingPongDistance.x / (int)SpeedX : 0;
		DurationY = SpeedY > 0 ? PingPongDistance.y / (int)SpeedY : 0;
	}


	protected override void OnTriggered (object data) {
		base.OnTriggered(data);
		MovingStartFrame = Game.GlobalFrame;
	}


	protected override void Move () {
		if (MovingStartFrame < 0) return;
		if (DurationX > 0) {
			int localFrameX = (Game.GlobalFrame - MovingStartFrame + DurationX / 2).PingPong(DurationX);
			X = Util.RemapUnclamped(0, DurationX, From.x, To.x, localFrameX);
		}
		if (DurationY > 0) {
			int localFrameY = (Game.GlobalFrame - MovingStartFrame + DurationY / 2).PingPong(DurationY);
			Y = Util.RemapUnclamped(0, DurationY, From.y, To.y, localFrameY);
		}
	}


}
