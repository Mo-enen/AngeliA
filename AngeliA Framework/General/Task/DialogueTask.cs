using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class DialogueTask : TaskItem {




		#region --- SUB ---


		private class Line {
			public string Content;
			public int CharacterID;
			public int FontSize;
			public Color32 Color;
		}


		#endregion




		#region --- VAR ---


		// Data
		private static DialogueTask Main;
		private static readonly Dictionary<int, string> PathPool = new();
		private static SystemLanguage LoadedLanguage = SystemLanguage.Unknown;
		private readonly List<Line> Lines = new();
		private int CurrentLine = 0;


		#endregion




		#region --- MSG ---


		public DialogueTask () => Main = this;


		public override TaskResult FrameUpdate () {

			if (Lines.Count == 0 || CurrentLine >= Lines.Count) return TaskResult.End;

			DialogueUI.Enable();






			return TaskResult.Continue;
		}


		#endregion




		#region --- API ---


		public static void StartConversation (int globalId) {

			if (Main == null) return;

			// Reload Path Pool
			if (LoadedLanguage != Language.CurrentLanguage) {
				LoadedLanguage = Language.CurrentLanguage;
				PathPool.Clear();
				string rootPath = Util.CombinePaths(AngePath.DialogueRoot, Language.CurrentLanguage.ToString());
				foreach (string path in Util.EnumerateFiles(rootPath, true, $"*.{Const.CONVERSATION_FILE_EXT}")) {
					int id = Util.GetNameWithoutExtension(path).AngeHash();
					PathPool.TryAdd(id, path);
				}
			}

			// Load Conversation
			Main.CurrentLine = 0;
			Main.Lines.Clear();
			if (PathPool.TryGetValue(globalId, out string conversationPath) && Util.FileExists(conversationPath)) {
				int currentCharacterID = 0;
				int currentFontSize = 24;
				var currentColor = Const.WHITE;
				bool prevLineEmpty = true;
				foreach (string line in Util.ForAllLines(conversationPath, System.Text.Encoding.UTF8)) {

					// Empty Line
					if (string.IsNullOrWhiteSpace(line)) {
						if (!prevLineEmpty) {
							prevLineEmpty = true;
							Main.Lines.Add(null);
						}
						continue;
					}

					// Solid Line
					prevLineEmpty = false;
					switch (line[0]) {
						case '@':
							// Character Line
							if (line.Length > 1) {
								currentCharacterID = line[1..].AngeHash();
							}
							break;
						case '#':
							// Config Line
							if (line.Length <= 1) break;
							int equalIndex = line.IndexOf('=');
							switch (line[1]) {
								case 'c':
									// Color
									if (equalIndex >= 0 && equalIndex < line.Length - 1 && ColorUtility.TryParseHtmlString(line[(equalIndex + 1)..], out var newColor)) {
										currentColor = newColor;
									} else {
										currentColor = Const.WHITE;
									}
									break;
								case 's':
									// Size
									if (equalIndex >= 0 && equalIndex < line.Length - 1 && int.TryParse(line[(equalIndex + 1)..], out int newSize)) {
										currentFontSize = 24 * newSize / 100;
									} else {
										currentFontSize = 24;
									}
									break;
							}
							break;
						default:
							// Content Line
							Main.Lines.Add(new Line() {
								Content = line.StartsWith("\\@") || line.StartsWith("\\#") ? line[1..] : line,
								CharacterID = currentCharacterID,
								Color = currentColor,
								FontSize = currentFontSize,
							});
							break;
					}
				}
			}

		}


		public static void EndConversation () {
			if (Main == null) return;
			Main.Lines.Clear();
			Main.CurrentLine = 0;
		}


		#endregion




		#region --- LGC ---





		#endregion




	}
}