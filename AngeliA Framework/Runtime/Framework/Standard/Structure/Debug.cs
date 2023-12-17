namespace AngeliaFramework {
	public static class Debug {

		public delegate void ObjectHandler (object obj);
		public delegate void ExceptionHandler (System.Exception ex);
		public delegate void SetEnableHandler (bool enable);
		public delegate bool GetEnableHandler ();
		public static event ObjectHandler OnLog;
		public static event ObjectHandler OnLogWarning;
		public static event ObjectHandler OnLogError;
		public static event ExceptionHandler OnLogException;
		public static event SetEnableHandler OnSetEnable;
		public static event GetEnableHandler OnGetEnable;

		public static bool GetEnable () => OnGetEnable != null && OnGetEnable.Invoke();
		public static void SetEnable (bool enable) => OnSetEnable?.Invoke(enable);
		public static void Log (object obj) => OnLog?.Invoke(obj);
		public static void LogWarning (object obj) => OnLogWarning?.Invoke(obj);
		public static void LogError (object obj) => OnLogError?.Invoke(obj);
		public static void LogException (System.Exception ex) => OnLogException?.Invoke(ex);
	}
}