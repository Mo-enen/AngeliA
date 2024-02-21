using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;


namespace AngeliA;


public abstract class OrderedAttribute : System.Attribute {
	public int Order { get; init; }
	public OrderedAttribute (int order) => Order = order;
}


public static partial class Util {



	// All Class
	public static Assembly[] AllAssemblies {
		get {
			if (_AllAssemblies == null) {
				var list = new List<Assembly>();
				foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
					if (assembly.GetCustomAttribute<AngeliAAttribute>() != null) {
						list.Add(assembly);
					}
				}
				foreach (var dllpath in EnumerateFiles("Library", false, "*.dll")) {
					if (
						Assembly.LoadFrom(dllpath) is Assembly assembly &&
						assembly.GetCustomAttribute<AngeliAAttribute>() != null
					) {
						list.Add(assembly);
					}
				}
				_AllAssemblies = list.ToArray();
			}
			return _AllAssemblies;
		}
	}
	private static Assembly[] _AllAssemblies = null;

	public static System.Type[] AllTypes {
		get {
			if (_AllTypes == null) {
				var list = new List<System.Type>();
				foreach (var assembly in AllAssemblies) {
					list.AddRange(assembly.GetTypes());
				}
				_AllTypes = list.ToArray();
			}
			return _AllTypes;
		}
	}
	private static System.Type[] _AllTypes = null;


	public static IEnumerable<System.Type> AllChildClass (this System.Type type, bool includeAbstract = false, bool includeInterface = false) {
		foreach (var t in AllChildClass(type, AllTypes, includeAbstract, includeInterface))
			yield return t;
	}
	public static IEnumerable<System.Type> AllChildClass (this System.Type type, Assembly assembly, bool includeAbstract = false, bool includeInterface = false) =>
		AllChildClass(type, assembly.GetTypes(), includeAbstract, includeInterface);
	private static IEnumerable<System.Type> AllChildClass (this System.Type type, System.Type[] types, bool includeAbstract = false, bool includeInterface = false) =>
		types.Where(t =>
			(includeAbstract || !t.IsAbstract) &&
			(includeInterface || !t.IsInterface) &&
			(t.IsSubclassOf(type) || (t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == type))
		);


	public static void ClearAllTypeCache () => _AllTypes = null;
	public static void ClearAllAssembliesCache () => _AllAssemblies = null;


	// For All Types
	public static IEnumerable<System.Type> AllClassImplemented (this System.Type type, bool includeAbstract = false) => AllTypes.Where(
		t => !t.IsInterface && (includeAbstract || !t.IsAbstract) && type.IsAssignableFrom(t)
	);


	public static IEnumerable<(System.Type, System.Attribute)> AllClassWithAttribute (this System.Type attribute, bool ignoreAbstract = true, bool ignoreInterface = true) {
		foreach (var target in AllTypes.Where(type =>
			(!ignoreAbstract || !type.IsAbstract) &&
			(!ignoreInterface || !type.IsInterface))
		) {
			foreach (var att in target.GetCustomAttributes(attribute, inherit: false)) {
				yield return (target, att as System.Attribute);
			}
		}
	}
	public static IEnumerable<(System.Type, A)> AllClassWithAttribute<A> (bool ignoreAbstract = true, bool ignoreInterface = true, bool inherit = false) where A : System.Attribute {
		var typeofA = typeof(A);
		foreach (var target in AllTypes.Where(type =>
			(!ignoreAbstract || !type.IsAbstract) &&
			(!ignoreInterface || !type.IsInterface))
		) {
			foreach (var att in target.GetCustomAttributes<A>(inherit)) {
				yield return (target, att);
			}
		}
	}


	public static IEnumerable<KeyValuePair<MethodInfo, T>> AllStaticMethodWithAttribute<T> () where T : System.Attribute {
		var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
		foreach (var method in AllTypes.SelectMany(t => t.GetMethods(flags))) {
			if (method.GetCustomAttribute<T>(false) is not T att) continue;
			if (method.DeclaringType.ContainsGenericParameters) {
				var args = method.DeclaringType.GetGenericArguments();
				var gTypes = new System.Type[args.Length];
				for (int i = 0; i < args.Length; i++) gTypes[i] = args[i].BaseType;
				var newType = method.DeclaringType.MakeGenericType(gTypes);
				var newMethod = newType.GetMethod(method.Name, flags);
				yield return new KeyValuePair<MethodInfo, T>(newMethod, att);
			} else {
				yield return new KeyValuePair<MethodInfo, T>(method, att);
			}
		}
	}


	public static void InvokeAllStaticMethodWithAttribute<A> () where A : System.Attribute {
		var methods = new List<KeyValuePair<MethodInfo, A>>(AllStaticMethodWithAttribute<A>());
		foreach (var (method, _) in methods) {
			try {
				method.Invoke(null, null);
			} catch (System.Exception ex) { Util.LogException(ex); }
		}
	}
	public static void InvokeAllStaticMethodWithAttribute<A> (System.Func<KeyValuePair<MethodInfo, A>, bool> predicte) where A : System.Attribute {
		var methods = new List<KeyValuePair<MethodInfo, A>>(
			AllStaticMethodWithAttribute<A>().Where(predicte)
		);
		foreach (var (method, _) in methods) {
			try {
				method.Invoke(null, null);
			} catch (System.Exception ex) { Util.LogException(ex); }
		}
	}
	public static void InvokeAllStaticMethodWithAttribute<A> (System.Comparison<KeyValuePair<MethodInfo, A>> comparison) where A : System.Attribute {
		var methods = new List<KeyValuePair<MethodInfo, A>>(AllStaticMethodWithAttribute<A>());
		methods.Sort(comparison);
		foreach (var (method, _) in methods) {
			try {
				method.Invoke(null, null);
			} catch (System.Exception ex) { Util.LogException(ex); }
		}
	}


	// Event/Delegate
	public static void LinkEventWithAttribute<T> (System.Type sender, string eventName) where T : System.Attribute {
		var info = sender.GetEvent(
			eventName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static
		);
		if (info == null || info.EventHandlerType == null) return;
		// Get List
		var list = new List<(MethodInfo method, int order)>();
		foreach (var (method, att) in Util.AllStaticMethodWithAttribute<T>()) {
			try {
				int order = 0;
				if (att is OrderedAttribute orderAtt) order = orderAtt.Order;
				list.Add((method, order));
			} catch (System.Exception ex) { Util.LogException(ex); }
		}
		// Sort 
		list.Sort((a, b) => a.order.CompareTo(b.order));
		// Fill List
		foreach (var (method, _) in list) {
			try {
				var addMethod = info.GetAddMethod(true);
				var del = System.Delegate.CreateDelegate(info.EventHandlerType, method);
				addMethod?.Invoke(null, new object[] { del });
			} catch (System.Exception ex) { Util.LogException(ex); }
		}
	}


	// Method
	public static object InvokeStaticMethod (System.Type type, string methodName, params object[] param) {
		if (string.IsNullOrEmpty(methodName)) return null;
		try {
			var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			return method?.Invoke(null, param);
		} catch (System.Exception ex) { Util.LogException(ex); }
		return null;
	}


	public static object InvokeMethod (object obj, string methodName, params object[] param) {
		if (string.IsNullOrEmpty(methodName)) { return null; }
		try {
			param ??= System.Array.Empty<object>();
			var type = obj.GetType();
			var method = type.GetMethod(
				methodName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
			);
			if (method == null) return null;
			return method.Invoke(obj, param);
		} catch (System.Exception ex) { Util.LogException(ex); }
		return null;
	}


	// Prop
	public static object GetStaticPropertyValue (System.Type type, string name) => type.GetProperty(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	).GetValue(null);


	public static object GetPropertyValue (object target, string name) =>
		GetProperty(target.GetType(), name).GetValue(target);


	public static PropertyInfo GetProperty (System.Type type, string name) => type.GetProperty(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
	);


	public static void SetStaticPropertyValue (System.Type type, string name, object value) => type.GetProperty(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	).SetValue(null, value);


	public static void SetPropertyValue (object target, string name, object value) {
		var type = target.GetType();
		var property = type.GetProperty(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		);
		while (property == null && type != typeof(object)) {
			type = type.BaseType;
			property = type.GetProperty(
				name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
			);
		}
		property?.SetValue(target, value);
	}


	// Field
	public static object GetFieldValue (object target, string name) => GetField(target.GetType(), name)?.GetValue(target);


	public static FieldInfo GetField (System.Type type, string name) {
		var field = type.GetField(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		);
		while (field == null && type != typeof(object)) {
			type = type.BaseType;
			field = type.GetField(
				name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
			);
		}
		return field;
	}


	public static void SetFieldValue (object target, string name, object value) {
		var type = target.GetType();
		var field = type.GetField(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		);
		while (field == null && type != typeof(object)) {
			type = type.BaseType;
			field = type.GetField(
				name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
			);
		}
		field?.SetValue(target, value);
	}


	public static object GetStaticFieldValue (System.Type type, string name) => type.GetField(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	).GetValue(null);


	public static void SetStaticFieldValue (System.Type type, string name, object value) => type.GetField(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	)?.SetValue(null, value);


	public static System.Type GetFieldType (object target, string name) =>
		target.GetType().GetField(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		)?.FieldType;


	public static System.Type GetStaticFieldType (System.Type type, string name) =>
		type.GetField(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		).FieldType;


}