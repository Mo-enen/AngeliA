using System.Collections;
using System.Collections.Generic;


namespace AngeliA;


public interface IUndoItem {
	public int Step { get; set; }
}


public class UndoRedo {


	// SUB
	private class LinkedList<T> : IEnumerable<T> {

		private class Item {
			public Item Prev = null;
			public Item Next = null;
			public T Value = default;
		}

		private class LinkedListEnumerator : IEnumerator<T> {
			public Item CurrentItem { get; set; }
			public T Current { get; private set; }
			object IEnumerator.Current => Current;
			public bool MoveNext () {
				Current = default;
				if (CurrentItem == null) return false;
				Current = CurrentItem.Value;
				CurrentItem = CurrentItem.Next;
				return true;
			}
			public void Dispose () {
				Current = default;
				CurrentItem = null;
			}
			public void Reset () {
				Current = default;
				CurrentItem = null;
			}
		}

		public int Capacity { get; init; }
		public bool HasValue => Head != null;

		private Stack<Item> ItemPool { get; init; }
		private Item Head = null;
		private Item Tail = null;
		private readonly LinkedListEnumerator Enumerator = new();

		public LinkedList (int capacity = 64) {
			Capacity = capacity;
			ItemPool = new(capacity);
			for (int i = 0; i < capacity; i++) {
				ItemPool.Push(new Item());
			}
		}

		public bool LinkToHead (T item) {
			if (TryPop(out var newItem)) {
				if (Head != null) {
					newItem.Next = Head;
					Head.Prev = newItem;
				} else {
					Tail = newItem;
				}
				Head = newItem;
				newItem.Value = item;
				return true;
			}
			return false;
		}

		public bool LinkToTail (T item) {
			if (TryPop(out var newItem)) {
				if (Tail != null) {
					newItem.Prev = Tail;
					Tail.Next = newItem;
				} else {
					Head = newItem;
				}
				Tail = newItem;
				newItem.Value = item;
				return true;
			}
			return false;
		}

		public bool TryPopHead (out T result) {
			result = default;
			if (Head == null) return false;
			result = Head.Value;
			ItemPool.Push(Head);
			Head = Head.Next;
			if (Head == null) {
				Tail = null;
			} else {
				Head.Prev = null;
			}
			return true;
		}

		public bool TryPopTail (out T result) {
			result = default;
			if (Tail == null) return false;
			result = Tail.Value;
			ItemPool.Push(Tail);
			Tail = Tail.Prev;
			if (Tail == null) {
				Head = null;
			} else {
				Tail.Next = null;
			}
			return true;
		}

		public bool TryPeekHead (out T result) {
			result = default;
			if (Head == null) return false;
			result = Head.Value;
			return true;
		}

		public bool TryPeekTail (out T result) {
			result = default;
			if (Tail == null) return false;
			result = Tail.Value;
			return true;
		}

		public void Clear () {
			for (var item = Head; item != null; item = item.Next) {
				ItemPool.Push(item);
			}
			Head = null;
			Tail = null;
		}

		public IEnumerator<T> GetEnumerator () {
			Enumerator.Reset();
			Enumerator.CurrentItem = Head;
			return Enumerator;
		}

		IEnumerator IEnumerable.GetEnumerator () => GetEnumerator();

		private bool TryPop (out Item item) {
			if (ItemPool.TryPop(out item)) {
				item.Next = null;
				item.Prev = null;
				return true;
			} else {
				item = null;
				return false;
			}
		}

	}


	// Api
	public int CurrentStep { get; private set; } = int.MinValue;

	// Data
	private readonly LinkedList<IUndoItem> UndoList = null;
	private readonly LinkedList<IUndoItem> RedoList = null;
	private readonly System.Action<IUndoItem> OnUndoPerformed = null;
	private readonly System.Action<IUndoItem> OnRedoPerformed = null;


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
		CurrentStep = int.MinValue;
	}


	public void GrowStep () => CurrentStep++;


}
