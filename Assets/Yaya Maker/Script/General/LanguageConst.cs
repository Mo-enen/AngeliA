using System.Collections;
using System.Collections.Generic;


namespace YayaMaker {
	public static class LConst {


		public delegate string StringStringHandler (string key);
		public static StringStringHandler GetLanguage { get; set; } = null;

		// Dialog
		public static string QuitConfirmContent => GetLanguage("Dialog.QuitConfirmContent");
		public static string LabelOK => GetLanguage("Dialog.Ok");
		public static string LabelCancel => GetLanguage("Dialog.Cancel");
		public static string LabelQuit => GetLanguage("Dialog.Quit");

		public const int Normal = 0;
		public const int Green = 1;
		public const int Red = 2;

		// Project
		public static string ProjectPathEmpty => GetLanguage("Project.ProjectPathEmpty");
		public static string ProjectPathNotExists => GetLanguage("Project.ProjectPathNotExists");
		public static string FailToLoadProject => GetLanguage("Project.FailToLoadProject");




	}
}
