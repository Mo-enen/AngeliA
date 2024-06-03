using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[System.Serializable]
public class UniverseInfo {
	public string ProductName = "";
	public string DeveloperName = "";
	public long ModifyDate = 0;
	public int MajorVersion = 0;
	public int MinorVersion = 0;
	public int PatchVersion = 0;
	public uint EngineBuildVersion = 0;
}


public class Universe {

	// Path
	public string UniverseRoot { get; private set; }
	public string SheetPath { get; private set; }
	public string ConversationRoot { get; private set; }
	public string EditableConversationRoot { get; private set; }
	public string UniverseMetaRoot { get; private set; }
	public string MapRoot { get; private set; }
	public string LanguageRoot { get; private set; }
	public string AsepriteRoot { get; private set; }
	public string SavingRoot { get; private set; }
	public string SavingMetaRoot { get; private set; }
	public string ProcedureMapRoot { get; private set; }
	public string InfoPath { get; private set; }
	public string MusicRoot { get; private set; }
	public string SoundRoot { get; private set; }
	public string FontRoot { get; private set; }
	public string CharacterConfigRoot { get; private set; }
	public string CharacterInfoPath { get; private set; }

	// Api
	public UniverseInfo Info { get; private set; }
	public bool Readonly { get; private set; }

	// MSG
	public static Universe LoadUniverse (string universeFolder, bool @readonly, bool useBuiltInSavingRoot = false) {
		string infoPath = AngePath.GetUniverseInfoPath(universeFolder);
		var result = new Universe {
			Readonly = @readonly,
			UniverseRoot = universeFolder,
			SheetPath = AngePath.GetSheetPath(universeFolder),
			ConversationRoot = AngePath.GetConversationRoot(universeFolder),
			EditableConversationRoot = AngePath.GetEditableConversationRoot(universeFolder),
			UniverseMetaRoot = AngePath.GetUniverseMetaRoot(universeFolder),
			LanguageRoot = AngePath.GetLanguageRoot(universeFolder),
			MapRoot = AngePath.GetMapRoot(universeFolder),
			AsepriteRoot = AngePath.GetAsepriteRoot(universeFolder),
			InfoPath = infoPath,
			Info = JsonUtil.LoadOrCreateJsonFromPath<UniverseInfo>(infoPath),
			MusicRoot = AngePath.GetUniverseMusicRoot(universeFolder),
			SoundRoot = AngePath.GetUniverseSoundRoot(universeFolder),
			FontRoot = AngePath.GetUniverseFontRoot(universeFolder),
			CharacterInfoPath = AngePath.GetCharacterInfoPath(universeFolder),
		};
		result.SavingRoot = useBuiltInSavingRoot ?
			Util.CombinePaths(AngePath.GetPersistentDataPath(result.Info.DeveloperName, result.Info.ProductName), "Built In Saving") :
			Util.CombinePaths(universeFolder, "Saving");
		result.SavingMetaRoot = AngePath.GetSavingMetaRoot(result.SavingRoot);
		result.ProcedureMapRoot = AngePath.GetProcedureMapRoot(result.SavingRoot);
		result.CharacterConfigRoot = AngePath.GetSavingMetaCharacterConfigRoot(result.SavingRoot);
		result.CreateFolders();
		return result;
	}

	// API
	public void CreateFolders () {
		Util.CreateFolder(ConversationRoot);
		Util.CreateFolder(UniverseMetaRoot);
		Util.CreateFolder(MapRoot);
		Util.CreateFolder(ProcedureMapRoot);
		Util.CreateFolder(SavingRoot);
		Util.CreateFolder(SavingMetaRoot);
	}

}
