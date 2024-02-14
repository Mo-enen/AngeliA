using System.Collections;
using System.Collections.Generic;


namespace AngeliA {
	public class EchoDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {




		#region --- VAR ---


		// Api
		public TValue this[TKey key] {
			get => GetValue(key);
			set => SetValue(key, value);
		}
		public TKey this[TValue tValue] {
			get => GetKey(tValue);
			set => SetValue(value, tValue);
		}
		public int Count => Pool.Count;

		// Data
		private readonly Dictionary<TKey, TValue> Pool = new();
		private readonly Dictionary<TValue, TKey> ReversedPool = new();


		#endregion




		#region --- API ---


		public bool TryAdd (TKey key, TValue value) {
			if (Pool.ContainsKey(key) || ReversedPool.ContainsKey(value)) return false;
			Pool.Add(key, value);
			ReversedPool.Add(value, key);
			return true;
		}
		public void Add (TKey key, TValue value) {
			if (Pool.ContainsKey(key)) throw new System.ArgumentException("Key already exists.");
			if (ReversedPool.ContainsKey(value)) throw new System.ArgumentException("Value already exists.");
			Pool.Add(key, value);
			ReversedPool.Add(value, key);
		}


		public bool ContainsKey (TKey key) => Pool.ContainsKey(key);
		public bool ContainsValue (TValue value) => ReversedPool.ContainsKey(value);
		public bool ContainsAny (TKey key, TValue value) => Pool.ContainsKey(key) || ReversedPool.ContainsKey(value);
		public bool ContainsPair (TKey key, TValue value) => Pool.TryGetValue(key, out var currentValue) && currentValue.Equals(value);


		public bool TryGetValue (TKey key, out TValue value) => Pool.TryGetValue(key, out value);
		public bool TryGetKey (TValue value, out TKey key) => ReversedPool.TryGetValue(value, out key);


		public bool Remove (TKey key) => Remove(key, out _);
		public bool Remove (TKey key, out TValue value) {
			if (Pool.Remove(key, out value)) {
				ReversedPool.Remove(value);
				return true;
			}
			return false;
		}
		public bool Remove (TValue value) => Remove(value, out _);
		public bool Remove (TValue value, out TKey key) {
			if (ReversedPool.Remove(value, out key)) {
				Pool.Remove(key);
				return true;
			}
			return false;
		}


		public TValue GetValue (TKey key) => Pool[key];
		public TKey GetKey (TValue value) => ReversedPool[value];
		public void SetValue (TKey key, TValue value) {
			if (Pool.TryGetValue(key, out var oldValue)) {
				if (oldValue.Equals(value)) return;
				ReversedPool.Remove(oldValue);
			}
			if (ReversedPool.TryGetValue(value, out var oldKey)) {
				Pool.Remove(oldKey);
			}
			Pool[key] = value;
			ReversedPool[value] = key;
		}


		public void Clear () {
			Pool.Clear();
			ReversedPool.Clear();
		}


		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator () => Pool.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator () => Pool.GetEnumerator();


		#endregion




	}
}