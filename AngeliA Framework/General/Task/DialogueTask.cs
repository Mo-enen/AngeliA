using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace AngeliaFramework {
	public class DialogueTask : TaskItem {




		#region --- SUB ---


		private class Section {
			public int CharacterID;
			public string Content;
			public Color32[] Colors;
		}


		#endregion




		#region --- VAR ---


		// Const
		private static readonly int TASK_ID = typeof(DialogueTask).AngeHash();

		// Data
		private static DialogueTask Main;
		private readonly List<Section> Sections = new();
		private int LoadedSection = -1;
		private int CurrentSection = 0;
		private DialogueUI DialogueUI = null;


		#endregion




		#region --- MSG ---


		public DialogueTask () => Main = this;


		public override TaskResult FrameUpdate () {

			if (Sections.Count == 0 || CurrentSection >= Sections.Count || DialogueUI == null) return TaskResult.End;

			if (LoadedSection != CurrentSection) {
				var section = Sections[CurrentSection];
				LoadedSection = CurrentSection;
				DialogueUI.SetData(section.Content, section.CharacterID, section.Colors);
			}
			DialogueUI.Update();

			// Roll
			if (FrameInput.GameKeyDown(Gamekey.Action)) {
				FrameInput.UseGameKey(Gamekey.Action);
				if (DialogueUI.Roll()) {
					CurrentSection++;
				}
			}

			return TaskResult.Continue;
		}


		#endregion




		#region --- API ---


		public static void StartConversation<D> (string globalName) where D : DialogueUI {

			string conversationPath = Util.CombinePaths(
				AngePath.DialogueRoot, globalName,
				$"{Util.LanguageToIso(Language.CurrentLanguage)}.{AngePath.CONVERSATION_FILE_EXT}"
			);
			if (Main == null || FrameTask.HasTask<DialogueTask>() || !Util.FileExists(conversationPath)) return;

			Main.CurrentSection = 0;
			Main.LoadedSection = -1;
			Main.Sections.Clear();
			Main.DialogueUI = Stage.SpawnEntity<D>(0, 0) ?? Stage.GetEntity<D>();
			if (Main.DialogueUI == null) return;
			Main.DialogueUI.Active = true;

			// Add Task
			FrameTask.AddToLast(TASK_ID);

			// Load Conversation
			bool prevLineConfig = false;
			int currentCharacterID = 0;
			var currentColor = Const.WHITE;
			var builder = new StringBuilder();
			var colors = new List<Color32>();
			foreach (string line in Util.ForAllLines(conversationPath, Encoding.UTF8)) {

				// Empty Line
				if (string.IsNullOrWhiteSpace(line)) {
					AppendContent("\n");
					prevLineConfig = false;
					continue;
				}

				// Solid Line
				switch (line[0]) {
					case '@':
						// Character Line
						MakeSection();
						currentCharacterID = line.AngeHash();
						prevLineConfig = false;
						break;
					case '#':
						// Config Line
						prevLineConfig = true;
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
						}
						break;
					default:
						// Content Line
						if (builder.Length > 0 && !prevLineConfig) AppendContent("\n");
						AppendContent(line.StartsWith("\\@") || line.StartsWith("\\#") ? line[1..] : line);
						prevLineConfig = false;
						break;
				}
			}

			// Make Last Section
			MakeSection();

			// Func
			void AppendContent (string content) {
				// Append Missing Color
				if (colors.Count == 0 && !currentColor.IsSame(Const.WHITE)) {
					int colorCount = builder.Length;
					for (int i = 0; i < colorCount; i++) {
						colors.Add(Const.WHITE);
					}
				}
				// Append Colors for Current Content
				if (colors.Count != 0 || !currentColor.IsSame(Const.WHITE)) {
					for (int i = 0; i < content.Length; i++) {
						colors.Add(currentColor);
					}
				}
				// Append to Builder
				builder.Append(content);
			}
			void MakeSection () {
				if (builder.Length > 0) {
					Main.Sections.Add(new Section() {
						CharacterID = currentCharacterID,
						Content = builder.ToString(),
						Colors = colors.ToArray(),
					});
				}
				currentCharacterID = 0;
				currentColor = Const.WHITE;
				colors.Clear();
				builder.Clear();
			}
		}


		public static void EndConversation () {
			if (Main == null) return;
			Main.Sections.Clear();
			Main.CurrentSection = 0;
			Main.LoadedSection = -1;
			Main.DialogueUI = null;
		}


		#endregion




		#region --- LGC ---





		#endregion




	}
}