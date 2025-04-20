using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AngeliA;

/// <summary>
/// Attribute for link all static methods with a System.Action. When the action is invoked, all methods get called.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
public abstract class EventAttribute (int order = 0) : OrderedAttribute(order) {


	// Data
	private static readonly Dictionary<Delegate, Delegate[]> ActionDelegatePool = [];
	private static List<(MethodInfo method, EventAttribute att)> MethodCache = null;
	private static readonly object[] ParamCache1 = [null];
	private static readonly object[] ParamCache2 = [null, null];
	private static readonly object[] ParamCache3 = [null, null, null];


	// MSG
	[OnGameInitialize(int.MinValue)]
	internal static void OnGameInitialize () {
		foreach (var type in Util.GetAllTypeSpan()) {
			foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
				var att = field.GetCustomAttribute<EventAttribute>(true);
				if (att == null) continue;
				LinkEventWithAttribute(att.GetType(), type, field);
			}
		}
	}


	[OnGameInitializeLater(int.MaxValue)]
	internal static void ClearAllMethodCache () => MethodCache = null;


	internal static void InvokeActionFromDelegatePool (Action action) {
		if (!ActionDelegatePool.TryGetValue(action, out var list) || list == null || list.Length == 0) return;
		foreach (var del in list) {
			try {
				del.Method.Invoke(null, null);
			} catch (Exception ex) { Debug.LogException(ex); }
		}
	}
	internal static void InvokeActionFromDelegatePool<T> (Action<T> action, T param) {
		if (!ActionDelegatePool.TryGetValue(action, out var list) || list == null || list.Length == 0) return;
		foreach (var del in list) {
			try {
				ParamCache1[0] = param;
				del.Method.Invoke(null, ParamCache1);
			} catch (Exception ex) { Debug.LogException(ex); }
		}
	}
	internal static void InvokeActionFromDelegatePool<T0, T1> (Action<T0, T1> action, T0 param0, T1 param1) {
		if (!ActionDelegatePool.TryGetValue(action, out var list) || list == null || list.Length == 0) return;
		foreach (var del in list) {
			try {
				ParamCache2[0] = param0;
				ParamCache2[1] = param1;
				del.Method.Invoke(null, ParamCache2);
			} catch (Exception ex) { Debug.LogException(ex); }
		}
	}
	internal static void InvokeActionFromDelegatePool<T0, T1, T2> (Action<T0, T1, T2> action, T0 param0, T1 param1, T2 param2) {
		if (!ActionDelegatePool.TryGetValue(action, out var list) || list == null || list.Length == 0) return;
		foreach (var del in list) {
			try {
				ParamCache3[0] = param0;
				ParamCache3[1] = param1;
				ParamCache3[2] = param2;
				del.Method.Invoke(null, ParamCache3);
			} catch (Exception ex) { Debug.LogException(ex); }
		}
	}


	// LGC
	private static IEnumerable<(MethodInfo method, EventAttribute att)> ForAllStaticMethodWithEventAttribute () {
		if (MethodCache == null) {
			MethodCache = [];
			foreach (var (method, att) in Util.AllStaticMethodWithAttribute<EventAttribute>()) {
				MethodCache.Add((method, att));
			}
			MethodCache.Sort(static (a, b) => a.att.Order.CompareTo(b.att.Order));
		}
		foreach (var (method, att) in MethodCache) {
			yield return (method, att);
		}
	}


	private static void LinkEventWithAttribute (Type attributeType, Type sender, FieldInfo actionField) {

		if (!Util.IsActionType(actionField.FieldType)) return;

		if (actionField.IsInitOnly) {
			Debug.LogWarning($"{sender.Name}.{actionField.Name} can not be readonly.");
			return;
		}

		Delegate hostDel = null;

		// Fill List
		foreach (var (method, att) in ForAllStaticMethodWithEventAttribute()) {
			try {
				if (att.GetType() != attributeType) continue;

				// Check Param Type
				var infoParams = actionField.FieldType.GetGenericArguments();
				var methodParams = method.GetParameters();
				if (infoParams.Length != methodParams.Length) goto _PARAM_ERROR_;
				for (int i = 0; i < infoParams.Length; i++) {
					if (infoParams[i] != methodParams[i].ParameterType) {
						goto _PARAM_ERROR_;
					}
				}

				// Add to List
				var del = Delegate.CreateDelegate(actionField.FieldType, method);
				if (hostDel == null) {
					hostDel = del;
				} else {
					hostDel = Delegate.Combine(hostDel, del);
				}

				// Set Value to Field
				if (hostDel != null) {
					actionField.SetValue(null, hostDel);
				}

				continue;

				_PARAM_ERROR_:;
#if DEBUG
				// Check Params
				var infoMsg = new StringBuilder();
				var methodMsg = new StringBuilder();
				for (int i = 0; i < infoParams.Length; i++) {
					var param = infoParams[i];
					infoMsg.Append(param.Name);
					if (i < infoParams.Length - 1) {
						infoMsg.Append(',');
						infoMsg.Append(' ');
					}
				}
				for (int i = 0; i < methodParams.Length; i++) {
					var param = methodParams[i];
					methodMsg.Append(param.ParameterType.Name);
					if (i < methodParams.Length - 1) {
						methodMsg.Append(',');
						methodMsg.Append(' ');
					}
				}
				Debug.LogError($"[{att.GetType().Name}] \"{method.DeclaringType.Name}.{method.Name}\" is having wrong param. Expect ({infoMsg}) Get ({methodMsg})");
#endif

			} catch (Exception ex) {
				Debug.LogException(ex);
			}
		}

		// Add Action to Pool
		if (actionField.GetValue(null) is Delegate action) {
			if (Util.IsActionType(action.GetType())) {
				ActionDelegatePool.TryAdd(action, action.GetInvocationList());
			} else {
				Debug.LogError($"{actionField.DeclaringType.Name}.{actionField.Name} should have action value.");
			}
		}

	}


}

