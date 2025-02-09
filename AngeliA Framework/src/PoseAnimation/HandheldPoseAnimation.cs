using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class HandheldPoseAnimation : PoseAnimation {

	public abstract void DrawTool (HandTool tool, PoseCharacterRenderer renderer);

}
