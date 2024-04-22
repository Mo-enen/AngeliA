using System;

namespace AngeliA;

public static class Debug {

	public static event Action<Exception> OnLogException;
	public static event Action<object> OnLogError;
	public static event Action<object> OnLogWarning;
	public static event Action<object> OnLog;

	public static void Log (object message) => OnLog?.Invoke(message);

	public static void LogError (object message) => OnLogError?.Invoke(message);

	public static void LogException (Exception ex) => OnLogException?.Invoke(ex);

	public static void LogWarning (object message) => OnLogWarning?.Invoke(message);

}