using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public interface IUndoItem {
	public int Step { get; set; }
}


public class UndoRedo {


	// Api
	public int CurrentStep { get; private set; } = int.MinValue;

	// Data
	private readonly Pipe<IUndoItem> UndoList = null;
	private readonly Pipe<IUndoItem> RedoList = null;
	private readonly System.Action<IUndoItem> OnUndoPerformed = null;
	private readonly System.Action<IUndoItem> OnRedoPerformed = null;
	private int StableLength = 0;


	// API
	public UndoRedo (
		int undoLimit = 4096,
		System.Action<IUndoItem> onUndoPerformed = null,
		System.Action<IUndoItem> onRedoPerformed = null
	) {
		UndoList = new Pipe<IUndoItem>(undoLimit);
		RedoList = new Pipe<IUndoItem>(undoLimit);
		OnUndoPerformed = onUndoPerformed;
		OnRedoPerformed = onRedoPerformed;
		CurrentStep = int.MinValue;
		StableLength = 0;
	}


	public void Register (IUndoItem data) {
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
		RedoList.Reset();
	}


	public void Undo () {
		for (int safe = 0; safe < UndoList.Capacity && UndoList.Length > 0; safe++) {
			if (!UndoList.TryPopTail(out var data)) break;
			RedoList.LinkToTail(data);
			OnUndoPerformed?.Invoke(data);
			if (!UndoList.TryPeekTail(out var tailData) || tailData.Step != data.Step) {
				break;
			}
		}
	}


	public void Redo () {
		for (int safe = 0; safe < RedoList.Capacity && RedoList.Length > 0; safe++) {
			if (!RedoList.TryPopTail(out var data)) break;
			UndoList.LinkToTail(data);
			OnRedoPerformed?.Invoke(data);
			if (!RedoList.TryPeekTail(out var tailData) || tailData.Step != data.Step) {
				break;
			}
		}
	}


	public void Reset () {
		UndoList.Reset();
		RedoList.Reset();
		CurrentStep = int.MinValue;
	}


	public void GrowStep () {
		CurrentStep++;
		StableLength = UndoList.Length;
	}


	public void MarkAsStabile () => StableLength = UndoList.Length;


	public void AbortUnstable () {
		if (UndoList.Length <= StableLength) return;
		int count = UndoList.Length - StableLength;
		for (int i = 0; i < count; i++) {
			if (!UndoList.TryPopTail(out _)) break;
		}
	}


}
