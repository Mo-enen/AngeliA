using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


public class Universe {

	public static Universe BuiltIn { get; private set; }
	public static UniverseInfo BuiltInInfo { get; private set; }

	// Universe Path
	public string UniverseRoot { get; private set; }
	public string SheetRoot { get; private set; }
	public string BuiltInSheetPath { get; private set; }
	public string GameSheetPath { get; private set; }
	public string InfoPath { get; private set; }
	public string ConversationRoot { get; private set; }
	public string UniverseMetaRoot { get; private set; }
	public string BuiltInMapRoot { get; private set; }
	public string LanguageRoot { get; private set; }
	public string MusicRoot { get; private set; }
	public string SoundRoot { get; private set; }
	public string FontRoot { get; private set; }
	public string CharacterMovementConfigRoot { get; private set; }
	public UniverseInfo Info { get; private set; }

	// User Saving Path
	public int CurrentSavingSlot { get; private set; }
	public string SavingRoot { get; private set; }
	public string SlotRoot { get; private set; }
	public string SlotMetaRoot { get; private set; }
	public string SlotUserMapRoot { get; private set; }
	public string SlotCharacterRenderingConfigRoot { get; private set; }
	public string SlotInventoryRoot { get; private set; }


	// MSG
	[OnGameInitialize(int.MinValue)]
	internal static void OnGameInitializeMin () {
		BuiltIn = LoadFromFile(AngePath.BuiltInUniverseRoot);
		BuiltInInfo = BuiltIn.Info;
		AngePath.SetCurrentUserPath(BuiltIn.Info.DeveloperName, BuiltIn.Info.ProductName);
	}


	// API
	public static Universe LoadFromFile (string universeFolder, bool useBuiltInSavingRoot = true) {
		string infoPath = AngePath.GetUniverseInfoPath(universeFolder);
		var result = new Universe {
			UniverseRoot = universeFolder,
			SheetRoot = AngePath.GetSheetRoot(universeFolder),
			BuiltInSheetPath = AngePath.GetBuiltInSheetPath(universeFolder),
			GameSheetPath = AngePath.GetGameSheetPath(universeFolder),
			ConversationRoot = AngePath.GetConversationRoot(universeFolder),
			UniverseMetaRoot = AngePath.GetUniverseMetaRoot(universeFolder),
			LanguageRoot = AngePath.GetLanguageRoot(universeFolder),
			BuiltInMapRoot = AngePath.GetMapRoot(universeFolder),
			InfoPath = infoPath,
			Info = JsonUtil.LoadOrCreateJsonFromPath<UniverseInfo>(infoPath),
			MusicRoot = AngePath.GetUniverseMusicRoot(universeFolder),
			SoundRoot = AngePath.GetUniverseSoundRoot(universeFolder),
			FontRoot = AngePath.GetUniverseFontRoot(universeFolder),
			CharacterMovementConfigRoot = AngePath.GetCharacterMovementConfigRoot(universeFolder),
		};

		// Load Saving & Slot
		int currentSlot = 0;
		string savingRoot = useBuiltInSavingRoot ?
			Util.CombinePaths(AngePath.GetPersistentDataPath(result.Info.DeveloperName, result.Info.ProductName), "Saving") :
			Util.CombinePaths(universeFolder, "Saving");
		string currentSlotStr = Util.FileToText(Util.CombinePaths(savingRoot, "CurrentSlot.txt"));
		if (!string.IsNullOrWhiteSpace(currentSlotStr) && int.TryParse(currentSlotStr, out currentSlot)) {
			currentSlot = currentSlot.GreaterOrEquelThanZero();
		}
		result.SetSavingRoot(savingRoot, currentSlot);

		// Create Folders
		Util.CreateFolder(result.ConversationRoot);
		Util.CreateFolder(result.UniverseMetaRoot);
		Util.CreateFolder(result.BuiltInMapRoot);
		Util.CreateFolder(result.LanguageRoot);
		Util.CreateFolder(result.SlotRoot);
		Util.CreateFolder(result.SlotMetaRoot);
		Util.CreateFolder(result.SlotUserMapRoot);
		Util.CreateFolder(result.MusicRoot);
		Util.CreateFolder(result.SoundRoot);
		Util.CreateFolder(result.FontRoot);
		Util.CreateFolder(result.SlotCharacterRenderingConfigRoot);
		Util.CreateFolder(result.CharacterMovementConfigRoot);
		Util.CreateFolder(result.SlotInventoryRoot);
		Util.CreateFolder(result.SheetRoot);

		return result;
	}


	public void ReloadSavingSlot (int newSlot, bool forceReload = false) {
		if (!forceReload && newSlot == CurrentSavingSlot) return;
		OrderedAttribute.InvokeAsAutoOrderingTask<BeforeSavingSlotChanged>();
		CurrentSavingSlot = newSlot;
		SetSavingRoot(SavingRoot, newSlot);
		OrderedAttribute.InvokeAsAutoOrderingTask<OnSavingSlotChanged>();
	}


	public void SetSavingRoot (string newSavingRoot, int slot) {
		SavingRoot = newSavingRoot;
		SlotRoot = AngePath.GetSlotRoot(newSavingRoot, slot);
		SlotMetaRoot = AngePath.GetSlotMetaRoot(newSavingRoot, slot);
		SlotInventoryRoot = AngePath.GetSlotInventoryRoot(newSavingRoot, slot);
		SlotUserMapRoot = AngePath.GetSlotUserMapRoot(newSavingRoot, slot);
		SlotCharacterRenderingConfigRoot = AngePath.GetSlotMetaCharacterConfigRoot(newSavingRoot, slot);
	}


}
