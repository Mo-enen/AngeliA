using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace AngeliaFramework.Editor {
	public class DialogueEditorWindow : UtilWindow {


		// VAR



		// MSG
		[MenuItem("AngeliA/Dialogue", false, 26)]
		public static void OpenDialogWindow () => OpenEditor<DialogueEditorWindow>("Dialogue");


		protected override void OnWindowGUI () {



		}


		protected override void OnLostFocus () { }


	}
}
