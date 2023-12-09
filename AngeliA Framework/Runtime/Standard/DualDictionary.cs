using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class DualDictionary<TKey, TValue> {




		#region --- VAR ---


		// Api
		public TValue this[TKey key] {
			get => Main.TryGetValue(key, out var value) ? value : Secondary[key];
			set {
				if (!MainReadOnly) Main[key] = value;
				if (!SecondaryReadOnly) Secondary[key] = value;
			}
		}
		public TValue this[TKey key, bool main] {
			get => (main ? Main : Secondary).TryGetValue(key, out var value) ? value : default;
			set {
				if (!MainReadOnly && main) Main[key] = value;
				if (!SecondaryReadOnly && !main) Secondary[key] = value;
			}
		}
		public bool MainReadOnly { get; init; }
		public bool SecondaryReadOnly { get; init; }

		// Data
		private Dictionary<TKey, TValue> Main { get; init; }
		private Dictionary<TKey, TValue> Secondary { get; init; }


		#endregion




		#region --- MSG ---


		public DualDictionary (
			int capacityMain = 0,
			int capacitySecondary = 0,
			bool mainReadOnly = false,
			bool secondaryReadOnly = false,
			Dictionary<TKey, TValue> main = null,
			Dictionary<TKey, TValue> secondary = null
		) {
			Main = main ?? new(capacityMain);
			Secondary = secondary ?? new(capacitySecondary);
			MainReadOnly = mainReadOnly;
			SecondaryReadOnly = secondaryReadOnly;
		}


		#endregion




		#region --- API ---


		// Both
		public bool ContainsKey (TKey key) => Main.ContainsKey(key) || Secondary.ContainsKey(key);
		public bool ContainsValue (TValue value) => Main.ContainsValue(value) || Secondary.ContainsValue(value);
		public void Clear () {
			if (!MainReadOnly) Main.Clear();
			if (!SecondaryReadOnly) Secondary.Clear();
		}
		public bool TryGetValue (TKey key, out TValue value) => Main.TryGetValue(key, out value) || Secondary.TryGetValue(key, out value);
		public void Add (TKey key, TValue value) {
			if (!MainReadOnly) Main.Add(key, value);
			if (!SecondaryReadOnly) Secondary.Add(key, value);
		}
		public void TryAdd (TKey key, TValue value) {
			if (!MainReadOnly) Main.TryAdd(key, value);
			if (!SecondaryReadOnly) Secondary.TryAdd(key, value);
		}
		public bool Remove (TKey key) {
			bool main = !MainReadOnly && Main.Remove(key);
			bool secondary = !SecondaryReadOnly && Secondary.Remove(key);
			return main || secondary;
		}

		// Main
		public bool ContainsKeyMain (TKey key) => Main.ContainsKey(key);
		public bool ContainsValueMain (TValue value) => Main.ContainsValue(value);
		public void ClearMain () {
			if (!MainReadOnly) Main.Clear();
		}
		public bool TryGetValueMain (TKey key, out TValue value) => Main.TryGetValue(key, out value);
		public void AddMain (TKey key, TValue value) {
			if (!MainReadOnly) Main.Add(key, value);
		}
		public void TryAddMain (TKey key, TValue value) {
			if (!MainReadOnly) Main.TryAdd(key, value);
		}
		public bool RemoveMain (TKey key) => !MainReadOnly && Main.Remove(key);
		public bool RemoveMain (TKey key, out TValue value) {
			value = default;
			return !MainReadOnly && Main.Remove(key, out value);
		}

		// Secondary
		public bool ContainsKeySecondary (TKey key) => Secondary.ContainsKey(key);
		public bool ContainsValueSecondary (TValue value) => Secondary.ContainsValue(value);
		public void ClearSecondary () {
			if (!SecondaryReadOnly) Secondary.Clear();
		}
		public bool TryGetValueSecondary (TKey key, out TValue value) => Secondary.TryGetValue(key, out value);
		public void AddSecondary (TKey key, TValue value) {
			if (!SecondaryReadOnly) Secondary.Add(key, value);
		}
		public void TryAddSecondary (TKey key, TValue value) {
			if (!SecondaryReadOnly) Secondary.TryAdd(key, value);
		}
		public bool RemoveSecondary (TKey key) => !SecondaryReadOnly && Secondary.Remove(key);
		public bool RemoveSecondary (TKey key, out TValue value) {
			value = default;
			return !SecondaryReadOnly && Secondary.Remove(key, out value);
		}


		#endregion




	}
}