using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public class DialogueTask : TaskItem {


		// VAR
		public static int ConversationID { get; private set; } = 0;
		private static readonly Dictionary<int, string> PathPool = new();


		// API
		[OnGameInitialize(0)]
		public static void OnGameInitialize () {
			PathPool.Clear();
			foreach (string path in Util.EnumerateFiles(AngePath.DialogueRoot, false, $"*.{Const.CONVERSATION_FILE_EXT}")) {
				int id = Util.GetNameWithoutExtension(path).AngeHash();
				PathPool.TryAdd(id, path);
			}
		}


		public static void LoadConversation (int id) {
			if (!PathPool.TryGetValue(id, out string path) && Util.FileExists(path)) return;
			ConversationID = id;
			foreach (string line in Util.ForAllLines(path)) {
				if (string.IsNullOrWhiteSpace(line) || line[0] == '#') continue;

				

			}
		}


		public static void EndConversation () {
			ConversationID = 0;

		}


		public override TaskResult FrameUpdate () {
			if (ConversationID == 0) return TaskResult.End;






			return TaskResult.Continue;
		}


	}
}