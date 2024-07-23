using System.Collections;
using System.Collections.Generic;

namespace AngeliA;


[System.Serializable]
public class UniverseInfo {
	public string ProductName = "";
	public string DeveloperName = "";
	public int MajorVersion = 0;
	public int MinorVersion = 0;
	public int PatchVersion = 0;
	public uint EngineBuildVersion = 0;
}


public class Universe {

	public static Universe BuiltIn { get; private set; }

	public string UniverseRoot { get; private set; }
	public string SheetPath { get; private set; }
	public string InfoPath { get; private set; }
	public string ConversationRoot { get; private set; }
	public string EditableConversationRoot { get; private set; }
	public string UniverseMetaRoot { get; private set; }
	public string MapRoot { get; private set; }
	public string UserMapRoot { get; private set; }
	public string LanguageRoot { get; private set; }
	public string AsepriteRoot { get; private set; }
	public string SavingRoot { get; private set; }
	public string SavingMetaRoot { get; private set; }
	public string ProcedureMapRoot { get; private set; }
	public string MusicRoot { get; private set; }
	public string SoundRoot { get; private set; }
	public string FontRoot { get; private set; }
	public string CharacterRenderingConfigRoot { get; private set; }
	public string CharacterMovementConfigRoot { get; private set; }
	public string CharacterAnimationRoot { get; private set; }
	public UniverseInfo Info { get; private set; }

	// MSG
	[OnGameInitialize(int.MinValue)]
	internal static void OnGameInitializeMin () {
		BuiltIn = LoadFromFile(
			AngePath.BuiltInUniverseRoot,
			useBuiltInSavingRoot: true
		);
		AngePath.SetCurrentUserPath(BuiltIn.Info.DeveloperName, BuiltIn.Info.ProductName);
	}

	// API
	public static Universe LoadFromFile (string universeFolder, bool useBuiltInSavingRoot = false) {
		string infoPath = AngePath.GetUniverseInfoPath(universeFolder);
		var result = new Universe {
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
			CharacterAnimationRoot = AngePath.GetCharacterAnimationRoot(universeFolder),
			CharacterMovementConfigRoot = AngePath.GetCharacterMovementConfigRoot(universeFolder),
		};
		result.SavingRoot = useBuiltInSavingRoot ?
			Util.CombinePaths(AngePath.GetPersistentDataPath(result.Info.DeveloperName, result.Info.ProductName), "Built In Saving") :
			Util.CombinePaths(universeFolder, "Saving");
		result.SavingMetaRoot = AngePath.GetSavingMetaRoot(result.SavingRoot);
		result.ProcedureMapRoot = AngePath.GetProcedureMapRoot(result.SavingRoot);
		result.UserMapRoot = AngePath.GetUserMapRoot(result.SavingRoot);
		result.CharacterRenderingConfigRoot = AngePath.GetSavingMetaCharacterConfigRoot(result.SavingRoot);

		// Create Folders
		Util.CreateFolder(result.ConversationRoot);
		Util.CreateFolder(result.EditableConversationRoot);
		Util.CreateFolder(result.UniverseMetaRoot);
		Util.CreateFolder(result.MapRoot);
		Util.CreateFolder(result.LanguageRoot);
		Util.CreateFolder(result.SavingRoot);
		Util.CreateFolder(result.SavingMetaRoot);
		Util.CreateFolder(result.ProcedureMapRoot);
		Util.CreateFolder(result.UserMapRoot);
		Util.CreateFolder(result.MusicRoot);
		Util.CreateFolder(result.SoundRoot);
		Util.CreateFolder(result.FontRoot);
		Util.CreateFolder(result.CharacterRenderingConfigRoot);
		Util.CreateFolder(result.CharacterMovementConfigRoot);
		Util.CreateFolder(result.CharacterAnimationRoot);

		return result;
	}

}
