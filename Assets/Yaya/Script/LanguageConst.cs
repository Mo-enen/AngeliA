using System.Collections;
using System.Collections.Generic;


namespace Yaya {
	public static class LConst {


		public delegate string StringStringHandler (string key);
		public static StringStringHandler GetLanguage { get; set; } = null;

		// Dialog
		public static string QuitConfirmContent => GetLanguage("Dialog.QuitConfirmContent");
		public static string LabelOK => GetLanguage("Dialog.Ok");
		public static string LabelCancel => GetLanguage("Dialog.Cancel");
		public static string LabelQuit => GetLanguage("Dialog.Quit");

	}
}
