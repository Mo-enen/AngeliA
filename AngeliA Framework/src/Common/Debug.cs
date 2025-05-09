﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AngeliA;



/// <summary>
/// Log debug messages to the console or screen
/// </summary>
public static class Debug {

	// VAR
	public static event Action<Exception> OnLogException;
	public static event Action<object> OnLogError;
	public static event Action<object> OnLogWarning;
	public static event Action<object> OnLog;
	public static event Action<int, string> OnLogInternal;
	public static event Action<int, string> OnLogErrorInternal;
	private static readonly StringBuilder ParamsCacheBuilder = new();
	private static int LastLogLabelFrame = -1;
	private static int LogLabelCount = 0;
	private static readonly Stack<long> TimeMeasurementTicks = [];

	// MSG
	[OnGameUpdatePauseless]
	internal static void OnGameUpdatePauseless () => TimeMeasurementTicks.Clear();

	// API
	public static void Log (params object[] messages) {
		if (messages == null || messages.Length == 0) return;
		ParamsCacheBuilder.Clear();
		foreach (var message in messages) {
			ParamsCacheBuilder.Append(message ?? "(null)");
			ParamsCacheBuilder.Append(' ');
		}
		Log(ParamsCacheBuilder.ToString());
	}
	public static void Log (object message) => OnLog?.Invoke(message);
	public static void Log (LanguageCode message) => OnLog?.Invoke(message.ToString());

	public static void LogWarning (params object[] messages) {
		if (messages == null || messages.Length == 0) return;
		ParamsCacheBuilder.Clear();
		foreach (var message in messages) {
			ParamsCacheBuilder.Append(message);
			ParamsCacheBuilder.Append(' ');
		}
		LogWarning(ParamsCacheBuilder.ToString());
	}
	public static void LogWarning (object message) => OnLogWarning?.Invoke(message);
	public static void LogWarning (LanguageCode message) => OnLogWarning?.Invoke(message.ToString());

	public static void LogError (params object[] messages) {
		if (messages == null || messages.Length == 0) return;
		ParamsCacheBuilder.Clear();
		foreach (var message in messages) {
			ParamsCacheBuilder.Append(message);
			ParamsCacheBuilder.Append(' ');
		}
		LogError(ParamsCacheBuilder.ToString());
	}
	public static void LogError (object message) => OnLogError?.Invoke(message);
	public static void LogError (LanguageCode message) => OnLogError?.Invoke(message.ToString());

	public static void LogException (Exception ex) => OnLogException?.Invoke(ex);


	/// <inheritdoc cref="LogLabel(string)"/>
	public static void LogLabel (params object[] objs) {
		ParamsCacheBuilder.Clear();
		foreach (var obj in objs) {
			ParamsCacheBuilder.Append(obj ?? "(null)");
			ParamsCacheBuilder.Append(' ');
		}
		LogLabel(ParamsCacheBuilder.ToString());
	}
	/// <inheritdoc cref="LogLabel(string)"/>
	public static void LogLabel (object obj) => LogLabel(obj.ToString());
	/// <summary>
	/// Draw a label on top-right of the screen for the current frame
	/// </summary>
	public static void LogLabel (string content) {
		if (Game.PauselessFrame != LastLogLabelFrame) {
			LastLogLabelFrame = Game.PauselessFrame;
			LogLabelCount = 0;
		}
		using var _ = new UILayerScope(false);
		int padding = GUI.Unify(6);
		int height = GUI.Unify(18);
		GUI.BackgroundLabel(
			Renderer.CameraRect.CornerInside(Alignment.TopRight, 1, height).Shift(0, -LogLabelCount * (height + padding)),
			content, Color32.BLACK, padding,
			style: GUI.Skin.AutoRightLabel
		);
		LogLabelCount++;
	}

	internal static void LogInternal (int id, string message) => OnLogInternal?.Invoke(id, message);

	internal static void LogErrorInternal (int id, string message) => OnLogErrorInternal?.Invoke(id, message);

	// Time Measurement
	public static void BeginTimeMeasuring () => TimeMeasurementTicks.Push(DateTime.UtcNow.Ticks);

	/// <summary>
	/// Get time duration from last Debug.BeginTimeMeasuring() call in milli-second
	/// </summary>
	public static float EndTimeMeasuring () {
		if (!TimeMeasurementTicks.TryPop(out long ticks)) return -1f;
		return (DateTime.UtcNow.Ticks - ticks).TicksToMilliSecond();
	}

	public static void LogTimeMeasuring (string label = "") => Log($"{label}: {EndTimeMeasuring():0.00} ms");

}