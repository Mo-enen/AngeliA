namespace AngeliA;

/// <summary>
/// Interface that mark the character as playable character
/// </summary>
public interface IPlayable {
	/// <summary>
	/// Lighting system illuminate size in global space
	/// </summary>
	int IlluminateRadius => Const.CEL * 3;
	/// <summary>
	/// Lighting system illuminate amount. 0 means no illuminate. 1000 means general amount.
	/// </summary>
	int IlluminateAmount => 1000;
}
