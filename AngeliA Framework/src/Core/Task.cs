using System.Collections;

namespace AngeliA;

/// <summary>
/// Single unit to hold logic for TaskSystem. ⚠ Use global single instance from TaskSystem.PeekFromPool ⚠
/// </summary>
public abstract class Task {

	/// <summary>
	/// Frame that start from 0 when the task begins
	/// </summary>
	public int LocalFrame { get; private set; } = 0;
	/// <summary>
	/// Custom data for the internal logic
	/// </summary>
	public object UserData { get; internal set; } = null;

	/// <summary>
	/// This function is called when this task start
	/// </summary>
	public virtual void OnStart () { }
	/// <summary>
	/// This function is called when this task end
	/// </summary>
	public virtual void OnEnd () { }
	/// <summary>
	/// This function is used to handle the internal logic.
	/// </summary>
	/// <returns>"Continue" if the task should keep on after this frame. "End" if the task should end.</returns>
	public abstract TaskResult FrameUpdate ();

	internal void Reset (object userData) {
		LocalFrame = 0;
		UserData = userData;
	}
	internal void GrowLocalFrame () => LocalFrame++;

}
