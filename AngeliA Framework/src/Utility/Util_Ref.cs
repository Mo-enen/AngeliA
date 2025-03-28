using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using System.Text;

namespace AngeliA;

public static partial class Util {


	public static readonly List<Assembly> AllAssemblies = [];
	private static Type[] AllTypes = [];


	public static ReadOnlySpan<Type> GetAllTypeSpan () => AllTypes.GetReadOnlySpan();


	public static void AddAssembly (Assembly assembly) {
		if (AllAssemblies.Contains(assembly)) return;
		AllAssemblies.Add(assembly);
		var assemblyTypes = assembly.GetTypes();
		var newTypes = new Type[AllTypes.Length + assemblyTypes.Length];
		AllTypes.CopyTo(newTypes, 0);
		assemblyTypes.CopyTo(newTypes, AllTypes.Length);
		AllTypes = newTypes;
	}


	public static IEnumerable<Type> AllChildClass (this Type type, bool includeAbstract = false, bool includeInterface = false) {
		foreach (var t in AllChildClass(type, AllTypes, includeAbstract, includeInterface))
			yield return t;
	}
	private static IEnumerable<Type> AllChildClass (this Type type, Type[] types, bool includeAbstract = false, bool includeInterface = false) {
		int len = types.Length;
		for (int i = 0; i < len; i++) {
			var t = types[i];
			if (
				(includeAbstract || !t.IsAbstract) &&
				(includeInterface || !t.IsInterface) &&
				(t.IsSubclassOf(type) || (t.BaseType != null && t.BaseType.IsGenericType && t.BaseType.GetGenericTypeDefinition() == type))
			) {
				yield return t;
			}
		}
	}

	public static IEnumerable<int> AllChildClassID (this Type type, bool includeAbstract = false, bool includeInterface = false) {
		foreach (var _type in AllChildClass(type, includeAbstract, includeInterface)) {
			yield return _type.AngeHash();
		}
	}

	public static IEnumerable<(Assembly assembly, A attribyte)> ForAllAssemblyWithAttribute<A> () where A : Attribute {
		foreach (var assembly in AllAssemblies) {
			var atts = assembly.GetCustomAttributes<A>();
			if (atts == null) continue;
			foreach (var att in atts) {
				yield return new(assembly, att);
			}
		}
	}


	public static bool TryGetAttributeFromAllAssemblies<A> () where A : Attribute => TryGetAttributeFromAllAssemblies<A>(out _);
	public static bool TryGetAttributeFromAllAssemblies<A> (out A attribute) where A : Attribute {
		foreach (var (_assembly, att) in ForAllAssemblyWithAttribute<A>()) {
			attribute = att;
			return true;
		}
		attribute = null;
		return false;
	}


	public static IEnumerable<KeyValuePair<E, int>> AllEnumIdPairs<E> () where E : struct, Enum {
		foreach (var value in Enum.GetValues<E>()) {
			yield return new(value, value.ToString().AngeHash());
		}
	}


	// For All Types
	public static IEnumerable<Type> AllClassImplemented (this Type interfaceType, bool includeAbstract = false) => AllTypes.Where(
		t => !t.IsInterface && (includeAbstract || !t.IsAbstract) && interfaceType.IsAssignableFrom(t)
	);
	public static IEnumerable<int> AllClassImplementedID (this Type interfaceType, bool includeAbstract = false) {
		foreach (var t in AllClassImplemented(interfaceType, includeAbstract)) {
			yield return t.AngeHash();
		}
	}


	public static IEnumerable<(Type, Attribute)> AllClassWithAttribute (this Type attribute, bool ignoreAbstract = true, bool ignoreInterface = true) {
		foreach (var target in AllTypes.Where(type =>
			(!ignoreAbstract || !type.IsAbstract) &&
			(!ignoreInterface || !type.IsInterface))
		) {
			foreach (var att in target.GetCustomAttributes(attribute, inherit: false)) {
				yield return (target, att as Attribute);
			}
		}
	}
	public static IEnumerable<(Type, A)> AllClassWithAttribute<A> (bool ignoreAbstract = true, bool ignoreInterface = true, bool inherit = false) where A : Attribute {
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


	public static IEnumerable<KeyValuePair<MethodInfo, T>> AllStaticMethodWithAttribute<T> () where T : Attribute {
		var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
		foreach (var method in AllTypes.SelectMany(t => t.GetMethods(flags))) {
			foreach (var att in method.GetCustomAttributes<T>(false)) {
				if (method.DeclaringType.ContainsGenericParameters) {
					var args = method.DeclaringType.GetGenericArguments();
					var gTypes = new Type[args.Length];
					for (int i = 0; i < args.Length; i++) gTypes[i] = args[i].BaseType;
					var newType = method.DeclaringType.MakeGenericType(gTypes);
					var newMethod = newType.GetMethod(method.Name, flags);
					yield return new KeyValuePair<MethodInfo, T>(newMethod, att);
				} else {
					yield return new KeyValuePair<MethodInfo, T>(method, att);
				}
			}
		}
	}


	public static void InvokeAllStaticMethodWithAttribute<A> () where A : Attribute {
		var methods = new List<KeyValuePair<MethodInfo, A>>(AllStaticMethodWithAttribute<A>());
		foreach (var (method, _) in methods) {
			try {
				method.Invoke(null, null);
			} catch (Exception ex) { Debug.LogException(ex); }
		}
	}
	public static void InvokeAllStaticMethodWithAttribute<A> (Func<KeyValuePair<MethodInfo, A>, bool> predicte) where A : Attribute {
		var methods = new List<KeyValuePair<MethodInfo, A>>(
			AllStaticMethodWithAttribute<A>().Where(predicte)
		);
		foreach (var (method, _) in methods) {
			try {
				method.Invoke(null, null);
			} catch (Exception ex) { Debug.LogException(ex); }
		}
	}
	public static void InvokeAllStaticMethodWithAttribute<A> (Comparison<KeyValuePair<MethodInfo, A>> comparison) where A : Attribute {
		var methods = new List<KeyValuePair<MethodInfo, A>>(AllStaticMethodWithAttribute<A>());
		methods.Sort(comparison);
		foreach (var (method, _) in methods) {
			try {
				method.Invoke(null, null);
			} catch (Exception ex) { Debug.LogException(ex); }
		}
	}


	// Method
	public static object InvokeStaticMethod (Type type, string methodName, params object[] param) {
		if (string.IsNullOrEmpty(methodName)) return null;
		try {
			var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			return method?.Invoke(null, param);
		} catch (Exception ex) { Debug.LogException(ex); }
		return null;
	}


	public static object InvokeMethod (object obj, string methodName, params object[] param) {
		if (string.IsNullOrEmpty(methodName)) { return null; }
		try {
			param ??= [];
			var type = obj.GetType();
			var method = type.GetMethod(
				methodName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
			);
			if (method == null) return null;
			return method.Invoke(obj, param);
		} catch (System.Exception ex) { Debug.LogException(ex); }
		return null;
	}


	// Prop
	public static object GetStaticPropertyValue (Type type, string name) => type.GetProperty(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	).GetValue(null);


	public static object GetPropertyValue (object target, string name) =>
		GetProperty(target.GetType(), name).GetValue(target);


	public static PropertyInfo GetProperty (Type type, string name) => type.GetProperty(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
	);


	public static void SetStaticPropertyValue (Type type, string name, object value) => type.GetProperty(
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
	public static object GetFieldValue (object target, string fieldName) => GetField(target.GetType(), fieldName)?.GetValue(target);


	public static FieldInfo GetField (Type type, string name) {
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


	public static object GetStaticFieldValue (Type type, string name) => type.GetField(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	).GetValue(null);


	public static void SetStaticFieldValue (Type type, string name, object value) => type.GetField(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	)?.SetValue(null, value);


	public static Type GetFieldType (object target, string name) =>
		target.GetType().GetField(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		)?.FieldType;


	public static Type GetStaticFieldType (Type type, string name) =>
		type.GetField(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		).FieldType;

}