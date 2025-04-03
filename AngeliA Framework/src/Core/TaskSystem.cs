using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Represent result of the task at current frame
/// </summary>
public enum TaskResult {
	/// <summary>
	/// The task should keep one
	/// </summary>
	Continue,
	/// <summary>
	/// The task should end
	/// </summary>
	End,
}

/// <summary>
/// Core system to handle in-game task that interrupt the gameplay
/// </summary>
public static class TaskSystem {




	#region --- VAR ---


	// Data
	private static readonly Dictionary<int, Task> Pool = [];
	private static readonly List<(Task task, object data)> Tasks = new(8);
	private static Task CurrentTask = null;


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-128)]
	internal static void Initialize () {
		Pool.Clear();
		foreach (var type in typeof(Task).AllChildClass()) {
			if (System.Activator.CreateInstance(type) is Task item) {
				Pool.TryAdd(type.AngeHash(), item);
			}
		}
		Pool.TrimExcess();
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


	/// <summary>
	/// Add a new task to first in current queue
	/// </summary>
	/// <param name="id">Type ID of the task. Use typeof(YourTask).AngeHash() to cache this ID</param>
	/// <param name="task">Instance of the task</param>
	/// <returns>True if successfuly added</returns>
	public static bool TryAddToFirst (int id, out Task task) => (task = AddToFirst(id)) is not null;
	/// <summary>
	/// Add a new task to first in current queue with custom data
	/// </summary>
	/// <param name="id">Type ID of the task. Use typeof(YourTask).AngeHash() to cache this ID</param>
	/// <param name="userData">Custom data of this operation. Use Task.UserData to get this data in the task's internal logic.</param>
	/// <param name="task">Instance of the task</param>
	/// <returns>True if successfuly added</returns>
	public static bool TryAddToFirst (int id, object userData, out Task task) => (task = AddToFirst(id, userData)) is not null;
	/// <inheritdoc cref="TryAddToFirst(int, object, out Task)"/>
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

	/// <summary>
	/// Add a new task to last in current queue
	/// </summary>
	/// <param name="id">Type ID of the task. Use typeof(YourTask).AngeHash() to cache this ID</param>
	/// <param name="task">Instance of the task</param>
	/// <returns>True if successfuly added</returns>
	public static bool TryAddToLast (int id, out Task task) => (task = AddToLast(id)) is not null;
	/// <summary>
	/// Add a new task to last in current queue with custom data
	/// </summary>
	/// <param name="id">Type ID of the task. Use typeof(YourTask).AngeHash() to cache this ID</param>
	/// <param name="userData">Custom data of this operation. Use Task.UserData to get this data in the task's internal logic.</param>
	/// <param name="task">Instance of the task</param>
	/// <returns>True if successfuly added</returns>
	public static bool TryAddToLast (int id, object userData, out Task task) => (task = AddToLast(id, userData)) is not null;
	/// <inheritdoc cref="TryAddToLast(int, object, out Task)"/>
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

	/// <summary>
	/// Reset all current performing tasks
	/// </summary>
	public static void ClearAllTask () {
		CurrentTask = null;
		Tasks.Clear();
	}


	/// <summary>
	/// Make all current performing task ends
	/// </summary>
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


	/// <summary>
	/// Get instance of task inside current performing queue
	/// </summary>
	public static Task GetTaskAt (int index) => Tasks[index].task;
	/// <summary>
	/// Get instance of current performing task
	/// </summary>
	public static Task GetCurrentTask () => CurrentTask;
	/// <summary>
	/// Get how many tasks are currently waiting for the current task
	/// </summary>
	public static int GetWaitingTaskCount () => Tasks.Count;


	/// <summary>
	/// True if there are any task performing
	/// </summary>
	/// <returns></returns>
	public static bool HasTask () => CurrentTask != null;
	/// <summary>
	/// True if there are any task in given type performing
	/// </summary>
	public static bool HasTask<T> () where T : Task {
		if (CurrentTask is T) return true;
		foreach (var s in Tasks) {
			if (s is T) return true;
		}
		return false;
	}


	/// <summary>
	/// Get global single instance of the task for given type ID
	/// </summary>
	public static Task PeekFromPool (int id) => Pool.TryGetValue(id, out var task) ? task : null;


	#endregion




}