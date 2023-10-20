// This code is distributed under MIT license. Copyright (c) 2013 George Mamaladze
// See license.txt or http://opensource.org/licenses/mit-license.php
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AngeliaFramework {


	[Serializable]
	public class Trie<TValue> : TrieNode<TValue> {

		private readonly StringBuilder Builder = new();
		private readonly StringBuilder FinalBuilder = new();

		public IEnumerable<TValue> Retrieve (string query) {
			return Retrieve(query, 0);
		}

		public void Add (string key, TValue value) {
			Add(key, 0, value);
		}

		public void AddForSearching (string name, TValue value) {

			Builder.Clear();
			FinalBuilder.Clear();

			// Add String Parts from Name
			for (int i = 0; i < name.Length; i++) {
				char c = name[i];
				if (c != ' ' && c != '_') {
					Builder.Append(c);
					FinalBuilder.Append(c);
				} else if (Builder.Length > 0) {
					Add(Builder.ToString().ToLower(), value);
					Builder.Clear();
				}
			}
			if (Builder.Length > 0) {
				Add(Builder.ToString().ToLower(), value);
				Builder.Clear();
			}

			// Add Whole Name
			Add(FinalBuilder.ToString().ToLower(), value);
			FinalBuilder.Clear();

		}

	}



	[Serializable]
	public class TrieNode<TValue> : TrieNodeBase<TValue> {
		private readonly Dictionary<char, TrieNode<TValue>> m_Children;
		private readonly Queue<TValue> m_Values;

		protected TrieNode () {
			m_Children = new Dictionary<char, TrieNode<TValue>>();
			m_Values = new Queue<TValue>();
		}

		protected override int KeyLength {
			get { return 1; }
		}

		protected override IEnumerable<TrieNodeBase<TValue>> Children () {
			return m_Children.Values;
		}

		protected override IEnumerable<TValue> Values () {
			return m_Values;
		}

		protected override TrieNodeBase<TValue> GetOrCreateChild (char key) {
			if (!m_Children.TryGetValue(key, out TrieNode<TValue> result)) {
				result = new TrieNode<TValue>();
				m_Children.Add(key, result);
			}
			return result;
		}

		protected override TrieNodeBase<TValue> GetChildOrNull (string query, int position) {
			if (query == null) throw new ArgumentNullException("query");
			return m_Children.TryGetValue(query[position], out TrieNode<TValue> childNode) ? childNode : null;
		}

		protected override void AddValue (TValue value) {
			m_Values.Enqueue(value);
		}

	}




	[Serializable]
	public abstract class TrieNodeBase<TValue> {
		protected abstract int KeyLength { get; }

		protected abstract IEnumerable<TValue> Values ();

		protected abstract IEnumerable<TrieNodeBase<TValue>> Children ();

		public long Size () {
			return Children().Sum(o => o.Size()) + 1;
		}

		public void Add (string key, int position, TValue value) {
			if (key == null) throw new ArgumentNullException("key");
			if (EndOfString(position, key)) {
				AddValue(value);
				return;
			}

			TrieNodeBase<TValue> child = GetOrCreateChild(key[position]);
			child.Add(key, position + 1, value);
		}

		protected abstract void AddValue (TValue value);

		protected abstract TrieNodeBase<TValue> GetOrCreateChild (char key);

		protected virtual IEnumerable<TValue> Retrieve (string query, int position) {
			return
				EndOfString(position, query)
					? ValuesDeep()
					: SearchDeep(query, position);
		}

		protected virtual IEnumerable<TValue> SearchDeep (string query, int position) {
			TrieNodeBase<TValue> nextNode = GetChildOrNull(query, position);
			return nextNode != null
					   ? nextNode.Retrieve(query, position + nextNode.KeyLength)
					   : Enumerable.Empty<TValue>();
		}

		protected abstract TrieNodeBase<TValue> GetChildOrNull (string query, int position);

		private static bool EndOfString (int position, string text) {
			return position >= text.Length;
		}

		private IEnumerable<TValue> ValuesDeep () {
			return
				Subtree()
					.SelectMany(node => node.Values());
		}

		protected IEnumerable<TrieNodeBase<TValue>> Subtree () {
			return
				Enumerable.Repeat(this, 1)
					.Concat(Children().SelectMany(child => child.Subtree()));
		}
	}


}