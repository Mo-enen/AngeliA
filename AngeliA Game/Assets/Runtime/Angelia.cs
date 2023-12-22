using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


[assembly: AngeliA]
[assembly: AngeliaGameTitle("AngeliA")]
[assembly: AngeliaGameDeveloper("Moenen")]


namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }



namespace AngeliaGame {
	public static class Angelia {



#if UNITY_EDITOR
		[UnityEditor.InitializeOnLoadMethod]
		public static void InitializeOnLoadMethod () {
			UnityEditor.EditorApplication.playModeStateChanged += (state) => {
				Util.ClearAllTypeCache();
			};
		}
#endif


		// Debug
		[OnGameInitialize(-4096)]
		public static void OnGameInitialize () {
			AngeliaFramework.Debug.OnLog += UnityEngine.Debug.Log;
			AngeliaFramework.Debug.OnLogWarning += UnityEngine.Debug.LogWarning;
			AngeliaFramework.Debug.OnLogError += UnityEngine.Debug.LogError;
			AngeliaFramework.Debug.OnLogException += UnityEngine.Debug.LogException;
			AngeliaFramework.Debug.OnSetEnable += SetEnable;
			AngeliaFramework.Debug.OnGetEnable += GetEnable;
			static void SetEnable (bool enable) => UnityEngine.Debug.unityLogger.logEnabled = enable;
			static bool GetEnable () => UnityEngine.Debug.unityLogger.logEnabled;
		}


		public static void Test () {


		}


	}
}