namespace AngeliA;

/// <summary>
/// Interface that receive callback functions when Serialized with JsonUtil
/// </summary>
public interface IJsonSerializationCallback {
	/// <summary>
	/// This function is called before json object save to file
	/// </summary>
	void OnBeforeSaveToDisk ();
	/// <summary>
	/// This function is called after json object load from file
	/// </summary>
	void OnAfterLoadedFromDisk ();
}
