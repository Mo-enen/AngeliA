using System.Collections.Generic;

namespace AngeliA;

public class Pipe<T> {




	#region --- VAR ---


	// Api
	public int Capacity { get; init; } = 1024;
	public int Length { get; private set; } = 0;
	public bool IsFull => Length >= Capacity;
	public T this[int index] {
		get => Data[(Start + index) % Capacity];
		set => Data[(Start + index) % Capacity] = value;
	}

	// Data
	private T[] Data { get; init; }
	private int Start = 0;


	#endregion




	#region --- MSG ---


	public Pipe (int capacity = 1024) {
		Capacity = capacity;
		Data = new T[capacity];
		Start = 0;
		Length = 0;
	}


	#endregion




	#region --- API ---


	// Peek
	public bool TryPeekHead (out T data) {
		data = default;
		if (Length <= 0) return false;
		data = Data[Start];
		return true;
	}


	public bool TryPeekTail (out T data) {
		data = default;
		if (Length <= 0) return false;
		data = Data[(Start + Length - 1) % Capacity];
		return true;
	}


	// Link
	public bool LinkToHead (T data) {
		if (Length >= Capacity) return false;
		Start = Start <= 0 ? Capacity - 1 : Start - 1;
		Data[Start] = data;
		Length++;
		return true;
	}


	public bool LinkToTail (T data) {
		if (Length >= Capacity) return false;
		int index = (Start + Length) % Capacity;
		Data[index] = data;
		Length++;
		return true;
	}


	// Pop
	public bool TryPopHead (out T data) {
		data = default;
		if (Length <= 0) return false;
		data = Data[Start];
		Data[Start] = default;
		Start = (Start + 1) % Capacity;
		Length--;
		return true;
	}


	public bool TryPopTail (out T data) {
		data = default;
		if (Length <= 0) return false;
		int i = (Start + Length - 1) % Capacity;
		data = Data[i];
		Data[i] = default;
		Length--;
		return true;
	}


	// Detch
	public void DetchHead (int newLength) {
		if (newLength >= Length) return;
		Start += Length - newLength;
		Length = newLength;
	}


	public void DetchTail (int newLength) {
		if (newLength >= Length) return;
		Length = newLength;
	}


	// Misc
	public void Reset () {
		Start = 0;
		Length = 0;
	}


	public void Reorganize () {
		if (Start == 0 || Length == 0) return;
		if (Length < Capacity) {
			int index = 1;
			for (int i = Length; i < Capacity; i++, index++) {
				(Data[i], Data[Start - index]) = (Data[Start - index], Data[i]);
			}
		}
		Start = 0;
	}


	public void Sort (IComparer<T> comparer) {
		Reorganize();
		Util.QuickSort(Data, 0, Length - 1, comparer);
	}


	#endregion




}