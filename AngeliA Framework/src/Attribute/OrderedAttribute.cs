using System;
using System.Collections.Generic;
using System.Reflection;

namespace AngeliA;


[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public abstract class OrderedAttribute (int order) : Attribute {

	public int Order { get; init; } = order;

	public static void InvokeAsAutoOrderingTask<A> () where A : Attribute {

		var methods = new List<KeyValuePair<MethodInfo, A>>(Util.AllStaticMethodWithAttribute<A>());
		var taskType = typeof(TaskResult);

		// Sort
		methods.Sort(Compare);

		// Invoke
		int count = methods.Count;
		for (int safe = 0; safe < 1024; safe++) {
			bool changed = false;
			bool skip = false;
			// Start Iteration
			for (int i = 0; i < count; i++) {
				try {
					var (method, att) = methods[i];
					if (att == null) continue;
					if (method.ReturnType == taskType) {
						// Task
						var result = (TaskResult)method.Invoke(null, null);
						if (result == TaskResult.End) {
							methods[i] = new(null, null);
							changed = true;
						} else {
							skip = true;
						}
					} else {
						// Non-Task
						method.Invoke(null, null);
						methods[i] = new(null, null);
						changed = true;
					}
				} catch (Exception ex) { Debug.LogException(ex); }
			}

			// End Iteration
			if (!changed || !skip) break;

		}

		// Func
		static int Compare (KeyValuePair<MethodInfo, A> a, KeyValuePair<MethodInfo, A> b) =>
			(a.Value is OrderedAttribute oa ? oa.Order : 0).CompareTo(
				b.Value is OrderedAttribute ob ? ob.Order : 0
			);
	}

}

