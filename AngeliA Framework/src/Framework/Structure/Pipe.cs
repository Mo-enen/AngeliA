using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public class Pipe<T> {




	#region --- VAR ---


	// Api
	public int Capacity { get; init; } = 1024;
	public int Length { get; private set; } = 0;

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


	public bool LinkToHead (T data) {
		if (Length >= Capacity) return false;
		Start = (Start - 1).UMod(Capacity);
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


	public bool TryPopHead (out T data) {
		data = default;
		if (Length <= 0) return false;
		data = Data[Start];
		Start = (Start + 1) % Capacity;
		Length--;
		return true;
	}


	public bool TryPopTail (out T data) {
		data = default;
		if (Length <= 0) return false;
		data = Data[(Start + Length - 1) % Capacity];
		Length--;
		return true;
	}


	public void Reset () {
		Start = 0;
		Length = 0;
	}


	#endregion




	#region --- LGC ---



	#endregion




}