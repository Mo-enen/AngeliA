using System.Collections;
using System.Collections.Generic;
using AngeliA;
using AngeliA.Framework;

namespace AngeliaEngine;

public abstract class EngineWindow : WindowUI {
	private bool IsDirty = false;
	protected void SetDirty (bool dirty = true) => IsDirty = dirty;
	protected virtual void Save (bool forceSave = false) {
		if (forceSave || IsDirty) IsDirty = false;
	}
}
