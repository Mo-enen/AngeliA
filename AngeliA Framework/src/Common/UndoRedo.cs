using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public interface IUndoItem {
	public int Step { get; set; }
}


public class UndoRedo (
	int undoLimit = 4096,
	System.Action<IUndoItem> onUndoPerformed = null,
	System.Action<IUndoItem> onRedoPerformed = null
) {


	// Api
	public int CurrentStep { get; private set; } = int.MinValue;

	// Data
	protected readonly Pipe<IUndoItem> UndoList = new Pipe<IUndoItem>(undoLimit);
	protected readonly Pipe<IUndoItem> RedoList = new Pipe<IUndoItem>(undoLimit);
	private readonly System.Action<IUndoItem> OnUndoPerformed = onUndoPerformed;
	private readonly System.Action<IUndoItem> OnRedoPerformed = onRedoPerformed;
	private int StableLength = 0;

	public virtual void Register (IUndoItem data) {
		RedoList.Reset();
		data.Step = CurrentStep;
		if (!UndoList.LinkToTail(data)) {
			// Free when Too Many
			int step = int.MaxValue;
			while (UndoList.TryPeekHead(out var head)) {
				step = step == int.MaxValue ? head.Step : step;
				if (head.Step != step || !UndoList.TryPopHead(out _)) break;
			}
			// Link Again
			UndoList.LinkToTail(data);
		}
	}


	public void Undo () {
		for (int safe = 0; safe < UndoList.Capacity && UndoList.Length > 0; safe++) {
			if (!UndoList.TryPopTail(out var data)) break;
			RedoList.LinkToTail(data);
			InvokeUndoPerform(data);
			if (!UndoList.TryPeekTail(out var tailData) || tailData.Step != data.Step) {
				break;
			}
		}
	}


	public void Redo () {
		for (int safe = 0; safe < RedoList.Capacity && RedoList.Length > 0; safe++) {
			if (!RedoList.TryPopTail(out var data)) break;
			UndoList.LinkToTail(data);
			InvokeRedoPerform(data);
			if (!RedoList.TryPeekTail(out var tailData) || tailData.Step != data.Step) {
				break;
			}
		}
	}


	public virtual void Reset () {
		UndoList.Reset();
		RedoList.Reset();
		CurrentStep = int.MinValue;
	}


	public virtual void GrowStep () {
		CurrentStep++;
		StableLength = UndoList.Length;
	}


	public void MarkAsStable () => StableLength = UndoList.Length;


	public void AbortUnstable () {
		if (UndoList.Length <= StableLength) return;
		int count = UndoList.Length - StableLength;
		for (int i = 0; i < count; i++) {
			if (!UndoList.TryPopTail(out _)) break;
		}
	}


	protected virtual void InvokeUndoPerform (IUndoItem data) => OnUndoPerformed?.Invoke(data);
	protected virtual void InvokeRedoPerform (IUndoItem data) => OnRedoPerformed?.Invoke(data);



}
