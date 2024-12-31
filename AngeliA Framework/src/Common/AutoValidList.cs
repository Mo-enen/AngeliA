using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class AutoValidList<T> (int capacity, System.Func<T, bool> updateFunc) {

	public int Count { get; private set; } = 0;
	public int Capacity => Data.Length;
	private readonly T[] Data = new T[capacity];
	private readonly System.Func<T, bool> UpdateFunc = updateFunc;

	public void Update () {
		int delta = 0;
		int oldCount = Count;
		for (int i = 0; i < oldCount; i++) {
			var ele = Data[i];
			if (UpdateFunc.Invoke(ele)) {
				if (delta > 0) {
					Data[i] = default;
					Data[i - delta] = ele;
				}
			} else {
				Data[i] = default;
				delta++;
				Count--;
			}
		}
	}

	public bool Add (T element) {
		if (Count < Data.Length) {
			Data[Count] = element;
			Count++;
			return true;
		} else {
			return false;
		}
	}

}
