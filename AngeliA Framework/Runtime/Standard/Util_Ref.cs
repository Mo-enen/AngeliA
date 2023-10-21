using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using System.Reflection.Emit;
using System.ComponentModel;
using System.Linq;


namespace AngeliaFramework {
	public static partial class Util {


		// Event/Delegate
		public static void LinkEventWithAttribute<T> (System.Type sender, string eventName) where T : System.Attribute {
			var info = sender.GetEvent(eventName, BindingFlags.Public | BindingFlags.Static);
			if (info == null) return;
			foreach (var (method, _) in Util.AllStaticMethodWithAttribute<T>()) {
				try {
					info.AddEventHandler(null, System.Delegate.CreateDelegate(
						info.EventHandlerType, method
					));
				} catch (System.Exception ex) { Debug.LogException(ex); }
			}
		}


		// Method
		public static object InvokeStaticMethod (System.Type type, string methodName, params object[] param) {
			if (string.IsNullOrEmpty(methodName)) { return null; }
			try {
				var method = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
				return method?.Invoke(null, param);
			} catch (System.Exception ex) { Debug.LogError(ex); }
			return null;
		}


		public static object InvokeMethod (object obj, string methodName, params object[] param) {
			if (string.IsNullOrEmpty(methodName)) { return null; }
			//try {
			param ??= new object[0];
			var type = obj.GetType();
			var method = type.GetMethod(
				methodName,
				BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance
			);
			if (method == null) {
				Debug.LogWarning("method is null");
				return null;
			}
			//Debug.Log(method + "\n" + obj + " \n" + param);
			return method.Invoke(obj, param);
			//} catch (System.Exception ex) { Debug.LogError(ex); }
			//return null;
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


		// All Class
		private static readonly List<Assembly> AllAssembliesCache = new();
		private static System.Type[] AllTypesCache {
			get {
				if (_AllTypesCache == null) {
#if UNITY_EDITOR
					if (!UnityEditor.EditorApplication.isPlaying) {
						AllAssembliesCache.Clear();
						AllAssembliesCache.AddRange(System.AppDomain.CurrentDomain.GetAssemblies());
					}
#endif
					var list = new List<System.Type>();
					foreach (var assembly in AllAssembliesCache)
						list.AddRange(assembly.GetTypes());
					_AllTypesCache = list.ToArray();
				}
				return _AllTypesCache;
			}
		}
		private static System.Type[] _AllTypesCache = null;


		public static IEnumerable<System.Type> AllChildClass (this System.Type type, bool includeAbstract = false, bool includeInterface = false) {
			foreach (var t in AllChildClass(type, AllTypesCache, includeAbstract, includeInterface))
				yield return t;
		}
		public static IEnumerable<System.Type> AllChildClass (this System.Type type, Assembly assembly, bool includeAbstract = false, bool includeInterface = false) =>
			AllChildClass(type, assembly.GetTypes(), includeAbstract, includeInterface);
		private static IEnumerable<System.Type> AllChildClass (this System.Type type, System.Type[] types, bool includeAbstract = false, bool includeInterface = false) =>
			types.Where(t => t.IsSubclassOf(type) &&
				 (includeAbstract || !t.IsAbstract) &&
				 (includeInterface || !t.IsInterface)
			);


		public static void InitializeAssembly (string keyword) {
			AllAssembliesCache.Clear();
			_AllTypesCache = null;
			foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies()) {
				if (assembly.GetName().Name.StartsWith(keyword, System.StringComparison.OrdinalIgnoreCase)) {
					AllAssembliesCache.Add(assembly);
				}
			}
		}


		// For All Types
		public static IEnumerable<System.Type> AllClassImplemented (this System.Type type, bool includeAbstract = false) => AllTypesCache.Where(
			t => !t.IsInterface && (includeAbstract || !t.IsAbstract) && type.IsAssignableFrom(t)
		);


		public static IEnumerable<(System.Type, System.Attribute)> AllClassWithAttribute (this System.Type attribute, bool ignoreAbstract = true, bool ignoreInterface = true) {
			foreach (var target in AllTypesCache.Where(type =>
				(!ignoreAbstract || !type.IsAbstract) &&
				(!ignoreInterface || !type.IsInterface))
			) {
				var att = target.GetCustomAttribute(attribute, false);
				if (att == null) continue;
				yield return (target, att);
			}
		}


		public static IEnumerable<KeyValuePair<MethodInfo, T>> AllStaticMethodWithAttribute<T> () where T : System.Attribute {
			foreach (var method in AllTypesCache
				.SelectMany(t => t.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
			) {
				var att = method.GetCustomAttribute<T>(false);
				if (att != null) {
					yield return new KeyValuePair<MethodInfo, T>(method, att);
				}
			}
		}



	}
}