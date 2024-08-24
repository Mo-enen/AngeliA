using System.Collections;

namespace AngeliA;

public abstract class Task {

	public int LocalFrame { get; private set; } = 0;
	public object UserData { get; internal set; } = null;

	public virtual void OnStart () { }
	public virtual void OnEnd () { }
	public abstract TaskResult FrameUpdate ();

	internal void Reset (object userData) {
		LocalFrame = 0;
		UserData = userData;
	}
	internal void GrowLocalFrame () => LocalFrame++;

}
