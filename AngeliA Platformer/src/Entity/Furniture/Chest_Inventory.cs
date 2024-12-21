using System.Collections;
using System.Collections.Generic;

using AngeliA;

namespace AngeliA.Platformer;

public abstract class InventoryChest : InventoryFurniture<InventoryPartnerUI> {

	private static readonly int[] LockingCheckCache = new int[64];

	public static bool UnlockableCheck (int lockInvID, int keyInvID, int lockId0, int lockId1, int lockId2, int keyId0, int keyId1, int keyId2) {

		// Get Lock Pattern
		int patternLen = 0;
		int len = Inventory.GetInventoryCapacity(lockInvID);
		for (int i = 0; i < len; i++) {
			int itemID = Inventory.GetItemAt(lockInvID, i, out int iCount);
			if (itemID == 0 || iCount <= 0) continue;
			if (itemID == lockId0) {
				LockingCheckCache[patternLen] = 0;
			} else if (itemID == lockId1) {
				LockingCheckCache[patternLen] = 1;
			} else if (itemID == lockId2) {
				LockingCheckCache[patternLen] = 2;
			} else {
				continue;
			}
			patternLen++;
			if (patternLen >= LockingCheckCache.Length) break;
		}

		// Not Locked
		if (patternLen == 0) return true;

		// Check Key Pattern
		int patternIndex = 0;
		int matchCount = 0;
		len = Inventory.GetInventoryCapacity(keyInvID);
		for (int i = 0; i < len; i++) {
			int itemID = Inventory.GetItemAt(keyInvID, i, out int iCount);
			if (itemID == 0 || iCount <= 0) continue;
			if (itemID == keyId0) {
				if (LockingCheckCache[patternIndex] == 0) {
					matchCount++;
				} else {
					return false;
				}
			} else if (itemID == keyId1) {
				if (LockingCheckCache[patternIndex] == 1) {
					matchCount++;
				} else {
					return false;
				}
			} else if (itemID == keyId2) {
				if (LockingCheckCache[patternIndex] == 2) {
					matchCount++;
				} else {
					return false;
				}
			} else {
				continue;
			}
			patternIndex++;
			if (patternIndex >= patternLen) break;
		}

		// Final
		return matchCount >= patternLen;
	}

}