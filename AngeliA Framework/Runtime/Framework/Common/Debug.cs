using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFrameworkUnity {
	public static class AngeliaUnity {


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



	}
}