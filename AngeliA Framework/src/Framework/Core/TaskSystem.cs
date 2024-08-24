using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public enum TaskResult { Continue, End, }

public static class TaskSystem {




	#region --- VAR ---


	// Data
	private static readonly Dictionary<int, Task> Pool = new();
	private static readonly List<(Task task, object data)> Tasks = new(8);
	private static Task CurrentTask = null;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	public static void Initialize () {
		Pool.Clear();
		foreach (var type in typeof(Task).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is Task item) {
				Pool.TryAdd(type.AngeHash(), item);
			}
		}
	}


	[OnGameUpdateLater]
	internal static void OnGameUpdateLater () {
		for (int safe = 0; safe < 2048; safe++) {
			if (UpdateTask() == TaskResult.Continue) break;
		}
	}


	private static TaskResult UpdateTask () {

		var result = TaskResult.End;

		// Update Current
		if (CurrentTask != null) {
			if (CurrentTask.LocalFrame == 0) {
				CurrentTask.OnStart();
			}
			if (CurrentTask != null) {
				try {
					result = CurrentTask.FrameUpdate();
				} catch (System.Exception ex) {
					Debug.LogException(ex);
					result = TaskResult.End;
				}
				if (CurrentTask != null) {
					CurrentTask.GrowLocalFrame();
					if (result == TaskResult.End) {
						try {
							CurrentTask.OnEnd();
						} catch (System.Exception ex) { Debug.LogException(ex); }
						CurrentTask = null;
					}
				}
			}
		}

		// Switch to Next
		if (CurrentTask == null && Tasks.Count > 0) {
			CurrentTask = Tasks[0].task;
			CurrentTask.Reset(Tasks[0].data);
			Tasks.RemoveAt(0);
		}

		return result;
	}


	#endregion




	#region --- API ---


	public static bool TryAddToFirst (int id, out Task task) => (task = AddToFirst(id)) is not null;
	public static bool TryAddToFirst (int id, object userData, out Task task) => (task = AddToFirst(id, userData)) is not null;
	public static Task AddToFirst (int id, object userData = null) {
		if (!Pool.TryGetValue(id, out var task)) return null;
		if (CurrentTask == null) {
			CurrentTask = task;
			task.Reset(userData);
		} else {
			Tasks.Insert(0, (task, userData));
		}
		return task;
	}


	public static bool TryAddToLast (int id, out Task task) => (task = AddToLast(id)) is not null;
	public static bool TryAddToLast (int id, object userData, out Task task) => (task = AddToLast(id, userData)) is not null;
	public static Task AddToLast (int id, object userData = null) {
		if (!Pool.TryGetValue(id, out var task)) return null;
		if (CurrentTask == null) {
			CurrentTask = task;
			task.Reset(userData);
		} else {
			Tasks.Add((task, userData));
		}
		return task;
	}


	public static void ClearAllTask () {
		CurrentTask = null;
		Tasks.Clear();
	}


	public static void EndAllTask () {
		var cTask = CurrentTask;
		if (cTask != null) {
			cTask.OnEnd();
			CurrentTask = null;
		}
		foreach (var (task, _) in Tasks) {
			task.OnEnd();
		}
		Tasks.Clear();
	}


	public static Task GetTaskAt (int index) => Tasks[index].task;
	public static Task GetCurrentTask () => CurrentTask;
	public static int GetWaitingTaskCount () => Tasks.Count;


	public static bool HasTask () => CurrentTask != null;
	public static bool HasTask<T> () where T : Task {
		if (CurrentTask is T) return true;
		foreach (var s in Tasks) {
			if (s is T) return true;
		}
		return false;
	}


	public static bool IsTasking<T> () where T : Task => CurrentTask is T;


	public static Task PeekFromPool (int id) => Pool.TryGetValue(id, out var task) ? task : null;


	#endregion




}