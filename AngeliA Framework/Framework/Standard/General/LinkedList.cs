using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class LinkedList<T> : IEnumerable<T> {




		#region --- SUB ---


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


		#endregion




		#region --- VAR ---


		// Api
		public int Capacity { get; init; }
		public bool HasValue => Head != null;

		// Data
		private Stack<Item> ItemPool { get; init; }
		private Item Head = null;
		private Item Tail = null;
		private readonly LinkedListEnumerator Enumerator = new();


		#endregion




		#region --- API ---


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


		#endregion




		#region --- LGC ---


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


		#endregion




	}
}