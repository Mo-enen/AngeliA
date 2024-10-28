using AngeliA;
namespace AngeliA.Platformer;

[EntityAttribute.UpdateOutOfRange]
public abstract class PingPongPlatform : Platform {

	// Abs
	protected virtual uint SpeedX => 0;
	protected virtual uint SpeedY => 0;
	protected abstract Int2 PingPongDistance { get; }

	// Data
	private Int2 From = default;
	private Int2 To = default;
	private int DurationX = 0;
	private int DurationY = 0;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		From.x = X - PingPongDistance.x / 2;
		From.y = Y - PingPongDistance.y / 2;
		To.x = X + PingPongDistance.x / 2;
		To.y = Y + PingPongDistance.y / 2;
		DurationX = SpeedX > 0 ? PingPongDistance.x / (int)SpeedX : 0;
		DurationY = SpeedY > 0 ? PingPongDistance.y / (int)SpeedY : 0;
	}


	protected override void Move () {
		if (DurationX > 0) {
			int localFrameX = (Game.SettleFrame + DurationX / 2).PingPong(DurationX);
			X = Util.RemapUnclamped(0, DurationX, From.x, To.x, localFrameX);
		}
		if (DurationY > 0) {
			int localFrameY = (Game.SettleFrame + DurationY / 2).PingPong(DurationY);
			Y = Util.RemapUnclamped(0, DurationY, From.y, To.y, localFrameY);
		}
	}


}
