using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {


	public interface IUndoItem {
		public int Step { get; set; }
	}


	public class UndoRedo {


		// Data
		private readonly LinkedList<IUndoItem> UndoList = null;
		private readonly LinkedList<IUndoItem> RedoList = null;
		private readonly System.Action<IUndoItem> OnUndoPerformed = null;
		private readonly System.Action<IUndoItem> OnRedoPerformed = null;
		private int CurrentStep = int.MinValue;


		// API
		public UndoRedo (
			int undoLimit = 4096,
			System.Action<IUndoItem> onUndoPerformed = null,
			System.Action<IUndoItem> onRedoPerformed = null
		) {
			UndoList = new LinkedList<IUndoItem>(undoLimit);
			RedoList = new LinkedList<IUndoItem>(undoLimit);
			OnUndoPerformed = onUndoPerformed;
			OnRedoPerformed = onRedoPerformed;
			CurrentStep = int.MinValue;
		}


		public void Register (IUndoItem data) {
			data.Step = CurrentStep;
			if (!UndoList.LinkToTail(data)) {
				// Free when Too Many
				int step = -1;
				while (UndoList.TryPeekHead(out var head)) {
					step = step < 0 ? head.Step : step;
					if (head.Step != step || !UndoList.TryPopHead(out _)) break;
				}
				// Link Again
				UndoList.LinkToTail(data);
			}
			RedoList.Clear();
		}


		public void Undo () {
			for (int safe = 0; safe < UndoList.Capacity && UndoList.HasValue; safe++) {
				if (!UndoList.TryPopTail(out var data)) break;
				RedoList.LinkToTail(data);
				OnUndoPerformed?.Invoke(data);
				if (!UndoList.TryPeekTail(out var tailData) || tailData.Step != data.Step) {
					break;
				}
			}
		}


		public void Redo () {
			for (int safe = 0; safe < RedoList.Capacity && RedoList.HasValue; safe++) {
				if (!RedoList.TryPopTail(out var data)) break;
				UndoList.LinkToTail(data);
				OnRedoPerformed?.Invoke(data);
				if (!RedoList.TryPeekTail(out var tailData) || tailData.Step != data.Step) {
					break;
				}
			}
		}


		public void Reset () {
			UndoList.Clear();
			RedoList.Clear();
		}


		public void GrowStep () => CurrentStep++;


	}
}
