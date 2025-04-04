using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Interface to mark an entity as can be ride by characters
/// </summary>
public interface IRider {
	/// <summary>
	/// True if the entity can be ride currently
	/// </summary>
	public bool ReadyToRide => true;
}
