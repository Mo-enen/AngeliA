using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using System.Text;

namespace AngeliA;

public static partial class Util {


	internal static readonly List<Assembly> AllAssemblies = [];
	private static Type[] AllTypes = [];


	/// <summary>
	/// Get a ReadOnlySpan for all AngeliA related System.Types
	/// </summary>
	public static ReadOnlySpan<Type> GetAllTypeSpan () => AllTypes.GetReadOnlySpan();


	/// <summary>
	/// Add a new assembly into AngeliA. Make sure the assembly is already loaded into .NET. ⚠ Only call this function inside a static constructor ⚠
	/// </summary>
	public static void AddAssembly (Assembly assembly) {
		if (AllAssemblies.Contains(assembly)) return;
		AllAssemblies.Add(assembly);
		var assemblyTypes = assembly.GetTypes();
		var newTypes = new Type[AllTypes.Length + assemblyTypes.Length];
		AllTypes.CopyTo(newTypes, 0);
		assemblyTypes.CopyTo(newTypes, AllTypes.Length);
		AllTypes = newTypes;
	}


	/// <summary>
	/// Iterate through all child classes of given type
	/// </summary>
	/// <param name="type"></param>
	/// <param name="includeAbstract">True if abstract class should be included</param>
	/// <param name="includeInterface">True if interface should be included</param>
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

	/// <summary>
	/// Iterate through all child class's ID of given type
	/// </summary>
	/// <param name="type"></param>
	/// <param name="includeAbstract">True if abstract class should be included</param>
	/// <param name="includeInterface">True if interface should be included</param>
	public static IEnumerable<int> AllChildClassID (this Type type, bool includeAbstract = false, bool includeInterface = false) {
		foreach (var _type in AllChildClass(type, includeAbstract, includeInterface)) {
			yield return _type.AngeHash();
		}
	}

	/// <summary>
	/// Iterate through all given assembly attribute with the inatance of the assembly
	/// </summary>
	/// <typeparam name="A">Type of the attribute</typeparam>
	/// <returns></returns>
	public static IEnumerable<(Assembly assembly, A attribute)> ForAllAssemblyWithAttribute<A> () where A : Attribute {
		foreach (var assembly in AllAssemblies) {
			var atts = assembly.GetCustomAttributes<A>();
			if (atts == null) continue;
			foreach (var att in atts) {
				yield return new(assembly, att);
			}
		}
	}


	/// <summary>
	/// Get first given attribute from all assemblies if found
	/// </summary>
	/// <typeparam name="A">Type of the attribute</typeparam>
	/// <returns>True if the attribute is found</returns>
	public static bool TryGetAttributeFromAllAssemblies<A> () where A : Attribute => TryGetAttributeFromAllAssemblies<A>(out _);

	/// <summary>
	/// Get first given attribute from all assemblies if found
	/// </summary>
	/// <typeparam name="A">Type of the attribute</typeparam>
	/// <param name="attribute">Instance of the founded attribute</param>
	/// <returns>True if the attribute is found</returns>
	public static bool TryGetAttributeFromAllAssemblies<A> (out A attribute) where A : Attribute {
		foreach (var (_assembly, att) in ForAllAssemblyWithAttribute<A>()) {
			attribute = att;
			return true;
		}
		attribute = null;
		return false;
	}


	/// <summary>
	/// Iterate through all value and name AngeHash for the given enum
	/// </summary>
	/// <typeparam name="E">Type of the enum</typeparam>
	public static IEnumerable<KeyValuePair<E, int>> AllEnumIdPairs<E> () where E : struct, Enum {
		foreach (var value in Enum.GetValues<E>()) {
			yield return new(value, value.ToString().AngeHash());
		}
	}


	// For All Types
	/// <summary>
	/// Iterate through all class that implement the given interface
	/// </summary>
	/// <param name="interfaceType">Type of the target interface</param>
	/// <param name="includeAbstract">True if abstract class is included</param>
	public static IEnumerable<Type> AllClassImplemented (this Type interfaceType, bool includeAbstract = false) => AllTypes.Where(
		t => !t.IsInterface && (includeAbstract || !t.IsAbstract) && interfaceType.IsAssignableFrom(t)
	);


	/// <summary>
	/// Iterate through all class's ID that implement the given interface
	/// </summary>
	/// <param name="interfaceType">Type of the target interface</param>
	/// <param name="includeAbstract">True if abstract class is included</param>
	public static IEnumerable<int> AllClassImplementedID (this Type interfaceType, bool includeAbstract = false) {
		foreach (var t in AllClassImplemented(interfaceType, includeAbstract)) {
			yield return t.AngeHash();
		}
	}


	/// <summary>
	/// Iterate through all classes with given attribute attaching on
	/// </summary>
	/// <param name="ignoreAbstract">True if abstract classes should be excluded</param>
	/// <param name="ignoreInterface">True if interfaces should be excluded</param>
	/// <param name="attribute">Type of the target attribute</param>
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


	/// <summary>
	/// Iterate through all classes with given attribute attaching on
	/// </summary>
	/// <typeparam name="A">Type of the target attribute</typeparam>
	/// <param name="ignoreAbstract">True if abstract classes should be excluded</param>
	/// <param name="ignoreInterface">True if interfaces should be excluded</param>
	/// <param name="inherit">Set to true to inspect the ancestors of element</param>
	/// <returns></returns>
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


	/// <summary>
	/// Iterate through all static method from all classes that with given attribute attached on
	/// </summary>
	/// <typeparam name="T">Type of the attribute</typeparam>
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


	/// <summary>
	/// Invoke all static method from all classes that with given attribute attached on
	/// </summary>
	/// <typeparam name="A">Type of the attribute</typeparam>
	public static void InvokeAllStaticMethodWithAttribute<A> () where A : Attribute {
		var methods = new List<KeyValuePair<MethodInfo, A>>(AllStaticMethodWithAttribute<A>());
		foreach (var (method, _) in methods) {
			try {
				method.Invoke(null, null);
			} catch (Exception ex) { Debug.LogException(ex); }
		}
	}


	/// <summary>
	/// Invoke all static method from all classes that with given attribute attached on
	/// </summary>
	/// <typeparam name="A">Type of the attribute</typeparam>
	/// <param name="predicte">Only invoke the method if this function returns true</param>
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


	/// <summary>
	/// Invoke all static method from all classes that with given attribute attached on
	/// </summary>
	/// <typeparam name="A">Type of the attribute</typeparam>
	/// <param name="comparison">Sort the list with this cmoparison</param>
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
	/// <summary>
	/// Invoke a static method from given name and type
	/// </summary>
	/// <param name="type">Type of the class that holds the method</param>
	/// <param name="methodName">Name of the static method</param>
	/// <param name="param">Param of the static method</param>
	/// <returns>Return value of the static method</returns>
	public static object InvokeStaticMethod (Type type, string methodName, params object[] param) {
		if (string.IsNullOrEmpty(methodName)) return null;
		try {
			var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
			return method?.Invoke(null, param);
		} catch (Exception ex) { Debug.LogException(ex); }
		return null;
	}


	/// <summary>
	/// Invoke a method from given name and type
	/// </summary>
	/// <param name="obj">Instance that holds the method</param>
	/// <param name="methodName">Name of the method</param>
	/// <param name="param">Param of the method</param>
	/// <returns>Return value of the method</returns>
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
	/// <summary>
	/// Get current value of a static property
	/// </summary>
	/// <param name="type">Type of the class that holds the property</param>
	/// <param name="name">Name of the property</param>
	/// <returns>Value of the property</returns>
	public static object GetStaticPropertyValue (Type type, string name) => type.GetProperty(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	).GetValue(null);


	/// <summary>
	/// Get current value of a property
	/// </summary>
	/// <param name="target">Instance that holds the property</param>
	/// <param name="name">Name of the property</param>
	/// <returns>Value of the property</returns>
	public static object GetPropertyValue (object target, string name) =>
		GetProperty(target.GetType(), name).GetValue(target);


	/// <summary>
	/// Get PropertyInfo of the given type and name
	/// </summary>
	/// <param name="type">Type of class that holds the property</param>
	/// <param name="name">Name of the property</param>
	/// <returns>Instance of the PropertyInfo</returns>
	public static PropertyInfo GetProperty (Type type, string name) => type.GetProperty(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
	);


	/// <summary>
	/// Set current value of a static property
	/// </summary>
	/// <param name="type">Type of class that holds the property</param>
	/// <param name="name">Name of the property</param>
	/// <param name="value">Value of the property</param>
	public static void SetStaticPropertyValue (Type type, string name, object value) => type.GetProperty(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	).SetValue(null, value);


	/// <summary>
	/// Set current value of a property
	/// </summary>
	/// <param name="target">Instance that holds the property</param>
	/// <param name="name">Name of the property</param>
	/// <param name="value">Value of the property</param>
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
	/// <summary>
	/// Get value of the given field
	/// </summary>
	/// <param name="target">Instance that holds the field</param>
	/// <param name="fieldName">Name of the field</param>
	/// <returns>Value of the field</returns>
	public static object GetFieldValue (object target, string fieldName) => GetField(target.GetType(), fieldName)?.GetValue(target);


	/// <summary>
	///  Get FieldInfo of the given type and name
	/// </summary>
	/// <param name="type">Type of class that holds the field</param>
	/// <param name="name">Name of the field</param>
	/// <returns>Instance of the FieldInfo</returns>
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


	/// <summary>
	/// Set value of the given field
	/// </summary>
	/// <param name="target">Instance that holds the field</param>
	/// <param name="name">Name of the field</param>
	/// <param name="value">Value of the field</param>
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


	/// <summary>
	/// Get value of the given static field
	/// </summary>
	/// <param name="type">Type of class that holds the field</param>
	/// <param name="name">Name of the field</param>
	/// <returns>Value of the field</returns>
	public static object GetStaticFieldValue (Type type, string name) => type.GetField(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	).GetValue(null);


	/// <summary>
	/// Set value of the given static field
	/// </summary>
	/// <param name="type">Type of class that holds the field</param>
	/// <param name="name">Name of the field</param>
	/// <param name="value">Value of the field</param>
	public static void SetStaticFieldValue (Type type, string name, object value) => type.GetField(
		name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static
	)?.SetValue(null, value);


	/// <summary>
	/// Get what type of data the given field holds
	/// </summary>
	/// <param name="target">Instance that holds the field</param>
	/// <param name="name">Name of the field</param>
	/// <returns>System.Type that this field holds</returns>
	public static Type GetFieldType (object target, string name) =>
		target.GetType().GetField(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		)?.FieldType;


	/// <summary>
	/// Get what type of data the given static field holds
	/// </summary>
	/// <param name="type">Type of class that holds the field</param>
	/// <param name="name">Name of the field</param>
	/// <returns>System.Type that this field holds</returns>
	public static Type GetStaticFieldType (Type type, string name) =>
		type.GetField(
			name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
		).FieldType;


	public static bool IsActionType (Type type) {
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