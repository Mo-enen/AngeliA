using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public static class UniverseUtil {

	public static Universe CreateUniverse (
		string workspace, string universeName, string mapTemplateRoot,
		string creator, Int3 version
	) {

		string projectFolderPath = Util.CombinePaths(workspace, System.Guid.NewGuid().ToString());

		Util.CreateFolder(projectFolderPath);
		string newUniversePath = AngePath.GetUniverseRoot(projectFolderPath);

		// Copy Maps
		Util.CopyFolder(
			mapTemplateRoot,
			AngePath.GetMapRoot(newUniversePath),
			copySubDirs: true,
			ignoreHidden: true
		);

		// Create Universe Object
		var universe = new Universe(projectFolderPath, @readonly: false);
		var info = universe.Info;
		info.UniverseName = universeName;
		info.Creator = creator;
		info.ModifyDate = info.CreatedDate = System.DateTime.Now.ToFileTime();
		info.EditorMajorVersion = version.x;
		info.EditorMinorVersion = version.y;
		info.EditorPatchVersion = version.z;

		// Save to Disk
		universe.SaveUniverseInfoToDisk();

		return universe;
	}

}