using System;

namespace AngeliA;

public static class Debug {

	public static event Action<Exception> OnLogException;
	public static event Action<object> OnLogError;
	public static event Action<object> OnLogWarning;
	public static event Action<object> OnLog;
	public static event Action<int, string> OnLogInternal;
	public static event Action<int, string> OnLogErrorInternal;

	public static void Log (object message) => OnLog?.Invoke(message);
	public static void Log (LanguageCode message) => OnLog?.Invoke(message.ToString());

	public static void LogError (object message) => OnLogError?.Invoke(message);
	public static void LogError (LanguageCode message) => OnLogError?.Invoke(message.ToString());

	public static void LogException (Exception ex) => OnLogException?.Invoke(ex);

	public static void LogWarning (object message) => OnLogWarning?.Invoke(message);
	public static void LogWarning (LanguageCode message) => OnLogWarning?.Invoke(message.ToString());

	public static void LogInternal (int id, string message) => OnLogInternal?.Invoke(id, message);

	public static void LogErrorInternal (int id, string message) => OnLogErrorInternal?.Invoke(id, message);

}