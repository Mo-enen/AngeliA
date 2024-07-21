using System.Collections;
using System.Collections.Generic;

namespace AngeliA;
public static class GeneralNotifications {

	private static readonly LanguageCode CHEAT_PERFORMED = ("Notify.CheatPerform", "Cheat Performed");

	[OnCheatPerform]
	internal static void CheatPerformed () {
		NotificationUI.SpawnNotification(CHEAT_PERFORMED, 0);
	}

}
