using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;


namespace Yaya {
	public static class LConst {


		public delegate string StringIntHandler (int key);
		public static StringIntHandler GetLanguage { get; set; } = null;

		private static readonly int QuitConfirmContentID = "Dialog.QuitConfirmContent".ACode();
		private static readonly int LabelOKID = "Dialog.Ok".ACode();
		private static readonly int LabelCancelID = "Dialog.Cancel".ACode();
		private static readonly int LabelQuitID = "Dialog.Quit".ACode();

		// Dialog
		public static string QuitConfirmContent => GetLanguage(QuitConfirmContentID);
		public static string LabelOK => GetLanguage(LabelOKID);
		public static string LabelCancel => GetLanguage(LabelCancelID);
		public static string LabelQuit => GetLanguage(LabelQuitID);

	}
}
