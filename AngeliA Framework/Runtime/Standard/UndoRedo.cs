using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public abstract class UndoItem {
		public int Step = 0;
	}
	public class UndoRedoEcho<T> where T : UndoItem {


		// SUB
		private struct Echo {
			public T Before;
			public T After;
			public Echo (T before, T after) {
				Before = before;
				After = after;
			}
		}


		// Api
		public int CurrentStep { get; private set; }

		// Data
		private readonly List<Echo> UndoList = new();
		private readonly List<Echo> RedoList = new();
		private readonly int UndoLimit = int.MaxValue;
		private readonly System.Action<T> OnUndoPerformed = null;
		private readonly System.Action<T> OnRedoPerformed = null;


		// API
		public UndoRedoEcho (int undoLimit = int.MaxValue, System.Action<T> onUndoPerformed = null, System.Action<T> onRedoPerformed = null) {
			CurrentStep = 1;
			UndoLimit = undoLimit;
			OnUndoPerformed = onUndoPerformed;
			OnRedoPerformed = onRedoPerformed;
		}


		public void RegisterBegin (T obj) {
			obj.Step = CurrentStep;
			if (UndoList.Count > UndoLimit) UndoList.RemoveAt(0);
			UndoList.Add(new Echo(obj, null));
			RedoList.Clear();
		}


		public void RegisterEnd (T obj, bool growStep = true) {
			obj.Step = CurrentStep;
			for (int i = UndoList.Count - 1; i >= 0; i--) {
				var after = UndoList[i].After;
				if (after == null && (i == 0 || UndoList[i - 1].After != null)) {
					UndoList[i] = new Echo(UndoList[i].Before, obj);
					break;
				}
			}
			if (growStep) CurrentStep++;
		}


		public void Undo () {
			if (UndoList.Count <= 0) return;
			var obj = UndoList[^1];
			UndoList.RemoveAt(UndoList.Count - 1);
			RedoList.Add(obj);
			OnUndoPerformed?.Invoke(obj.Before);
			// Peek and Do Next or Not
			var prevStep = UndoList.Count > 0 ? UndoList[^1].Before : null;
			if (prevStep != null && prevStep.Step == obj.Before.Step) {
				Undo();
			}
		}


		public void Redo () {
			if (RedoList.Count <= 0) return;
			var obj = RedoList[^1];
			RedoList.RemoveAt(RedoList.Count - 1);
			UndoList.Add(obj);
			OnRedoPerformed?.Invoke(obj.After);
			// Peek and Do Next or Not
			var prevStep = RedoList.Count > 0 ? RedoList[^1].After : null;
			if (prevStep != null && prevStep.Step == obj.After.Step) {
				Redo();
			}
		}


		public void Reset () {
			CurrentStep = 1;
			UndoList.Clear();
			RedoList.Clear();
		}


	}



	public class UndoRedo<T> where T : UndoItem {


		// Api
		public int CurrentUndoStep { get; private set; } = 1;

		// Data
		private readonly List<T> UndoList = new();
		private readonly List<T> RedoList = new();
		private readonly int UndoLimit = int.MaxValue;
		private readonly System.Action<T> OnUndoPerformed = null;
		private readonly System.Action<T> OnRedoPerformed = null;


		// API
		public UndoRedo (int undoLimit = int.MaxValue, System.Action<T> onUndoPerformed = null, System.Action<T> onRedoPerformed = null) {
			UndoLimit = undoLimit;
			OnUndoPerformed = onUndoPerformed;
			OnRedoPerformed = onRedoPerformed;
		}


		public void Register (T obj) {
			obj.Step = CurrentUndoStep;
			if (UndoList.Count > UndoLimit) UndoList.RemoveAt(0);
			UndoList.Add(obj);
			RedoList.Clear();
		}


		public void Undo () {
			if (UndoList.Count <= 0) return;
			var obj = UndoList[^1];
			UndoList.RemoveAt(UndoList.Count - 1);
			RedoList.Add(obj);
			OnUndoPerformed?.Invoke(obj);
			// Peek and Do Next or Not
			var prevStep = UndoList.Count > 0 ? UndoList[^1] : null;
			if (prevStep != null && prevStep.Step == obj.Step) {
				Undo();
			}
		}


		public void Redo () {
			if (RedoList.Count <= 0) return;
			var obj = RedoList[^1];
			RedoList.RemoveAt(RedoList.Count - 1);
			UndoList.Add(obj);
			OnRedoPerformed?.Invoke(obj);
			// Peek and Do Next or Not
			var prevStep = RedoList.Count > 0 ? RedoList[^1] : null;
			if (prevStep != null && prevStep.Step == obj.Step) {
				Redo();
			}
		}


		public void Reset () {
			UndoList.Clear();
			RedoList.Clear();
		}


	}
}
