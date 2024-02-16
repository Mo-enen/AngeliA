using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class UniverseUtil {

	public static Universe CreateUniverse (string workspace, string universeName, string mapTemplateRoot, string creator) {

		string universeFolderPath = Util.CombinePaths(workspace, System.Guid.NewGuid().ToString());

		Util.CreateFolder(universeFolderPath);
		string newUniversePath = AngePath.GetUniverseRoot(universeFolderPath);

		// Copy Maps
		Util.CopyFolder(
			mapTemplateRoot,
			AngePath.GetMapRoot(newUniversePath),
			copySubDirs: true,
			ignoreHidden: true
		);

		// Create Universe Object
		AngeliaVersionAttribute.GetVersion(out int majorVersion, out int minorVersion, out int patchVersion, out _);
		var universe = new Universe(universeFolderPath, @readonly: false);
		var info = universe.Info;
		info.UniverseName = universeName;
		info.Creator = creator;
		info.ModifyDate = info.CreatedDate = System.DateTime.Now.ToFileTime();
		info.EditorMajorVersion = majorVersion;
		info.EditorMinorVersion = minorVersion;
		info.EditorPatchVersion = patchVersion;

		// Save to Disk
		universe.SaveUniverseInfoToDisk();

		return universe;
	}

}