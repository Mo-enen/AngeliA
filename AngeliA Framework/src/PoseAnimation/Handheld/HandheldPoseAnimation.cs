using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Pose animation which override onto a character for holding a handtool
/// </summary>
public abstract class HandheldPoseAnimation : PoseAnimation {

	/// <summary>
	/// Rendering the given handtool
	/// </summary>
	public abstract void DrawTool (HandTool tool, PoseCharacterRenderer renderer);

}
