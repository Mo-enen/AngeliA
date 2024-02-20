using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AngeliA;
public class Universe {

	// SUB
	[System.Serializable]
	public class UniverseInfo {
		public string UniverseName = "(no name)";
		public string Creator = "";
		public long CreatedDate = 0;
		public long ModifyDate = 0;
		public int EditorMajorVersion = -1;
		public int EditorMinorVersion = -1;
		public int EditorPatchVersion = -1;
	}

	// Path
	public string UniverseRoot { get; init; }
	public string SheetPath { get; init; }
	public string ConversationRoot { get; init; }
	public string EditableConversationRoot { get; init; }
	public string UniverseMetaRoot { get; init; }
	public string MapRoot { get; init; }
	public string ArtworkRoot { get; init; }
	public string SavingRoot { get; init; }
	public string ItemCustomizationRoot { get; init; }
	public string SavingMetaRoot { get; init; }
	public string ProcedureMapRoot { get; init; }

	// Api
	public UniverseInfo Info { get; init; }
	public bool Readonly { get; init; }

	// MSG
	public Universe (string universeFolder, bool @readonly) : this(Util.CombinePaths(universeFolder, "Universe"), Util.CombinePaths(universeFolder, "Saving"), @readonly) { }

	public Universe (string universeFolder, string savingFolder, bool @readonly) {

		Readonly = @readonly;

		// Universe
		UniverseRoot = universeFolder;
		SheetPath = AngePath.GetSheetPath(universeFolder);
		ConversationRoot = AngePath.GetConversationRoot(universeFolder);
		EditableConversationRoot = AngePath.GetEditableConversationRoot(universeFolder);
		UniverseMetaRoot = AngePath.GetUniverseMetaRoot(universeFolder);
		MapRoot = AngePath.GetMapRoot(universeFolder);
		ArtworkRoot = AngePath.GetArtworkRoot(universeFolder);

		// Saving
		SavingRoot = savingFolder;
		ItemCustomizationRoot = AngePath.GetItemCustomizationRoot(savingFolder);
		SavingMetaRoot = AngePath.GetSavingMetaRoot(savingFolder);
		ProcedureMapRoot = AngePath.GetProcedureMapRoot(savingFolder);

		// Create Folders 
		CreateFolders();

		// Load Info
		Info = JsonUtil.LoadOrCreateJson<UniverseInfo>(universeFolder);

	}

	// API
	public void SaveUniverseInfoToDisk () => JsonUtil.SaveJson(Info, UniverseRoot);

	public void CreateFolders () {
		Util.CreateFolder(ConversationRoot);
		Util.CreateFolder(UniverseMetaRoot);
		Util.CreateFolder(MapRoot);
		Util.CreateFolder(SavingRoot);
		Util.CreateFolder(ItemCustomizationRoot);
		Util.CreateFolder(SavingMetaRoot);
		Util.CreateFolder(ProcedureMapRoot);
	}

}
