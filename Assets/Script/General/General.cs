using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;

namespace System.Runtime.CompilerServices { internal static class IsExternalInit { } }

namespace Yaya {


	public static class YayaUtil {

		public static readonly int VINE_TAG = "Vine".AngeHash();

	}


	public static class LConst {


		public delegate string StringIntHandler (int key);
		public static StringIntHandler GetLanguage { get; set; } = null;

		private static readonly int QuitConfirmContentID = "Dialog.QuitConfirmContent".AngeHash();
		private static readonly int LabelOKID = "Dialog.Ok".AngeHash();
		private static readonly int LabelCancelID = "Dialog.Cancel".AngeHash();
		private static readonly int LabelQuitID = "Dialog.Quit".AngeHash();

		// Dialog
		public static string QuitConfirmContent => GetLanguage(QuitConfirmContentID);
		public static string LabelOK => GetLanguage(LabelOKID);
		public static string LabelCancel => GetLanguage(LabelCancelID);
		public static string LabelQuit => GetLanguage(LabelQuitID);

	}
}
