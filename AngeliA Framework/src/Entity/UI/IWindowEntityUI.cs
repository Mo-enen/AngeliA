namespace AngeliA;

/// <summary>
/// Interface that indicate the entity is a UI window
/// </summary>
public interface IWindowEntityUI {
	/// <summary>
	/// Rect position of the background in global space
	/// </summary>
	public IRect BackgroundRect { get; }
}
