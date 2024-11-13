using System;
using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public class FullObjectUndo (int undoLimit = 4096, Action<IUndoItem> onUndoRedoPerformed = null) : UndoRedo(undoLimit, onUndoRedoPerformed, onUndoRedoPerformed) {


	private struct ItemPair (IUndoItem before, IUndoItem after, bool hasBefore) : IUndoItem {
		public int Step { get; set; }
		public IUndoItem Before = before;
		public IUndoItem After = after;
		public bool HasBefore = hasBefore;
	}

	public override void Register (IUndoItem data) {
		IUndoItem before = default;
		bool hasBefore = false;
		if (UndoList.TryPeekTail(out var undoTail) && undoTail is ItemPair pair) {
			before = pair.After;
			hasBefore = true;
		}
		base.Register(new ItemPair(before, data, hasBefore));
	}


	protected override void InvokeUndoPerform (IUndoItem data) {
		if (data is ItemPair pair) {
			if (pair.HasBefore) {
				base.InvokeUndoPerform(pair.Before);
			}
		} else {
			base.InvokeUndoPerform(data);
		}
	}


	protected override void InvokeRedoPerform (IUndoItem data) {
		if (data is ItemPair pair) {
			base.InvokeUndoPerform(pair.After);
		} else {
			base.InvokeUndoPerform(data);
		}
	}


}
