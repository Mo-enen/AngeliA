using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AngeliA;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field, AllowMultiple = false)]
public abstract class EventAttribute (int order = 0) : OrderedAttribute(order) {


	// Data
	private static List<(MethodInfo method, EventAttribute att)> MethodCache = null;


	// MSG
	[OnGameInitialize(int.MinValue)]
	internal static void OnGameInitialize () {
		foreach (var type in Util.AllTypes) {
			foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)) {
				var att = field.GetCustomAttribute<EventAttribute>(true);
				if (att == null) continue;
				LinkEventWithAttribute(att.GetType(), type, field);
			}
		}
	}


	[OnGameInitializeLater(int.MaxValue)]
	internal static void ClearAllMethodCache () => MethodCache = null;


	// API
	public static IEnumerable<(MethodInfo method, EventAttribute att)> ForAllStaticMethodWithEventAttribute () {
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


	// LGC
	private static void LinkEventWithAttribute (Type attributeType, Type sender, FieldInfo actionField) {

		if (!IsAction(actionField.FieldType)) return;

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
		static bool IsAction (Type type) {
			if (type == typeof(System.Action)) return true;
			Type generic = null;
			if (type.IsGenericTypeDefinition) generic = type;
			else if (type.IsGenericType) generic = type.GetGenericTypeDefinition();
			if (generic == null) return false;
			if (generic == typeof(System.Action<>)) return true;
			if (generic == typeof(System.Action<,>)) return true;
			if (generic == typeof(System.Action<,,>)) return true;
			if (generic == typeof(System.Action<,,,>)) return true;
			if (generic == typeof(System.Action<,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,,,,,,,,>)) return true;
			if (generic == typeof(System.Action<,,,,,,,,,,,,,,,>)) return true;
			return false;
		}
	}


}

