using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// A linked list that can add/remove from head/tail. No heap pressure.
/// </summary>
public class Pipe<T> (int capacity = 1024) {




	#region --- VAR ---


	// Api
	/// <summary>
	/// Get filled data at index
	/// </summary>
	/// <param name="index">Local index from "Start"</param>
	public T this[int index] {
		get => Data[(Start + index) % Capacity];
		set => Data[(Start + index) % Capacity] = value;
	}
	/// <summary>
	/// Raw data of this pipe
	/// </summary>
	public T[] Data { get; init; } = new T[capacity];
	/// <summary>
	/// Total length of the data
	/// </summary>
	public int Capacity { get; init; } = capacity;
	/// <summary>
	/// Head index of the filled data
	/// </summary>
	public int Start { get; private set; } = 0;
	/// <summary>
	/// Length of the filled data
	/// </summary>
	public int Length { get; private set; } = 0;
	/// <summary>
	/// True if the pipe reached max capacity
	/// </summary>
	public bool IsFull => Length >= Capacity;


	#endregion




	#region --- API ---


	// Peek
	/// <summary>
	/// Get data at head without remove the data from pipe
	/// </summary>
	/// <returns>True if length of pipe is not 0</returns>
	public bool TryPeekHead (out T data) {
		data = default;
		if (Length <= 0) return false;
		data = Data[Start];
		return true;
	}


	/// <summary>
	/// Get data at tail without remove the data from pipe
	/// </summary>
	/// <returns>True if length of pipe is not 0</returns>
	public bool TryPeekTail (out T data) {
		data = default;
		if (Length <= 0) return false;
		data = Data[(Start + Length - 1) % Capacity];
		return true;
	}


	// Link
	/// <summary>
	/// Add data before head
	/// </summary>
	/// <returns>True if the data is added</returns>
	public bool LinkToHead (T data) {
		if (Length >= Capacity) return false;
		Start = Start <= 0 ? Capacity - 1 : Start - 1;
		Data[Start] = data;
		Length++;
		return true;
	}


	/// <summary>
	/// Add data after tail
	/// </summary>
	/// <returns>True if the data is added</returns>
	public bool LinkToTail (T data) {
		if (Length >= Capacity) return false;
		int index = (Start + Length) % Capacity;
		Data[index] = data;
		Length++;
		return true;
	}


	// Pop
	/// <summary>
	/// Get and remove data at head
	/// </summary>
	/// <returns>True if pipe is not empty</returns>
	public bool TryPopHead (out T data) {
		data = default;
		if (Length <= 0) return false;
		data = Data[Start];
		Data[Start] = default;
		Start = (Start + 1) % Capacity;
		Length--;
		return true;
	}


	/// <summary>
	/// Get and remove data at tail
	/// </summary>
	/// <returns>True if pipe is not empty</returns>
	public bool TryPopTail (out T data) {
		data = default;
		if (Length <= 0) return false;
		int i = (Start + Length - 1) % Capacity;
		data = Data[i];
		Data[i] = default;
		Length--;
		return true;
	}


	// Misc
	public void Reset () {
		Start = 0;
		Length = 0;
	}


	/// <summary>
	/// Move data at head to the first of the internal array
	/// </summary>
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