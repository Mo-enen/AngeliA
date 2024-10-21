using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using AngeliA;

namespace AngeliaEngine;

internal static class PackageUtil {


	public static PackageInfo GetInfoFromPackageFolder (string packageFolder) {
		string infoPath = Util.CombinePaths(packageFolder, "Info.json");
		if (!Util.FileExists(infoPath)) return null;
		if (JsonUtil.LoadJsonFromPath<PackageInfo>(infoPath) is not PackageInfo info) return null;
		string iconPath = Util.CombinePaths(packageFolder, "Icon.png");
		info.IconTexture = Game.PngBytesToTexture(Util.FileToBytes(iconPath));

		info.PackageFolderPath = packageFolder;
		info.DllPath = Util.CombinePaths(packageFolder, $"{info.PackageName}.dll");
		info.SheetPath = Util.CombinePaths(packageFolder, $"{info.PackageName}.{AngePath.SHEET_FILE_EXT}");
		info.ThemeFolder = Util.CombinePaths(packageFolder, "Theme");

		info.DllFounded = Util.FileExists(info.DllPath);
		info.SheetFounded = Util.FileExists(info.SheetPath);
		info.ThemeFounded = Util.GetFileCount(info.ThemeFolder, AngePath.SHEET_SEARCH_PATTERN, SearchOption.TopDirectoryOnly) > 0;

		return info;
	}


	public static bool IsPackagedInstalled (Project project, PackageInfo packageInfo) {
		if (project == null) return false;
		bool installed = false;
		// Check Dll
		if (packageInfo.DllFounded) {
			string dllName = $"{packageInfo.PackageName}.dll";
			installed =
				Util.FileExists(Util.CombinePaths(project.DllLibPath_Debug, dllName)) ||
				Util.FileExists(Util.CombinePaths(project.DllLibPath_Release, dllName));
		}
		// Check Sheet
		if (!installed && packageInfo.SheetFounded) {
			string targetSheetName = $"{packageInfo.PackageName}.{AngePath.SHEET_FILE_EXT}";
			installed = Util.FileExists(Util.CombinePaths(project.Universe.SheetRoot, targetSheetName));
		}
		// Check Theme
		if (!installed && packageInfo.ThemeFounded) {
			// Get Engine Theme Hash
			var engineThemeHash = new HashSet<int>();
			foreach (string themePath in Util.EnumerateFiles(EngineUtil.ThemeRoot, true, AngePath.SHEET_SEARCH_PATTERN)) {
				string themeName = Util.GetNameWithExtension(themePath);
				engineThemeHash.Add(themeName.AngeHash());
			}
			foreach (string themePath in Util.EnumerateFiles(packageInfo.ThemeFolder, true, AngePath.SHEET_SEARCH_PATTERN)) {
				string themeName = Util.GetNameWithExtension(themePath);
				if (engineThemeHash.Contains(themeName.AngeHash())) {
					installed = true;
					break;
				}
			}
		}
		return installed;
	}


	public static void InstallPackage (Project project, PackageInfo packageInfo) {

		if (project == null) return;
		string packageName = packageInfo.PackageName;

		// DLL
		if (packageInfo.DllFounded) {
			string dllName = $"{packageName}.dll";
			if (Util.FileExists(packageInfo.DllPath)) {
				Util.CopyFile(packageInfo.DllPath, Util.CombinePaths(project.DllLibPath_Debug, dllName));
				Util.CopyFile(packageInfo.DllPath, Util.CombinePaths(project.DllLibPath_Release, dllName));
			}
		}

		// Sheet
		if (packageInfo.SheetFounded) {
			string targetSheetName = $"{packageName}.{AngePath.SHEET_FILE_EXT}";
			if (Util.FileExists(packageInfo.SheetPath)) {
				Util.CopyFile(
					packageInfo.SheetPath,
					Util.CombinePaths(project.Universe.SheetRoot, targetSheetName)
				);
			}
		}

		// Theme
		if (packageInfo.ThemeFounded) {
			foreach (string themePath in Util.EnumerateFiles(packageInfo.ThemeFolder, true, AngePath.SHEET_SEARCH_PATTERN)) {
				string themeName = Util.GetNameWithExtension(themePath);
				Util.CopyFile(themePath, Util.CombinePaths(EngineUtil.ThemeRoot, themeName));
			}
		}

	}


	public static void UninstallPackage (Project project, PackageInfo packageInfo) {

		if (project == null) return;

		string dllName = $"{packageInfo.PackageName}.dll";
		string sheetName = $"{packageInfo.PackageName}.{AngePath.SHEET_FILE_EXT}";

		// Dll
		Util.DeleteFile(Util.CombinePaths(project.DllLibPath_Debug, dllName));
		Util.DeleteFile(Util.CombinePaths(project.DllLibPath_Release, dllName));

		// Sheet
		Util.DeleteFile(Util.CombinePaths(project.Universe.SheetRoot, sheetName));

		// Theme
		foreach (string themePath in Util.EnumerateFiles(packageInfo.ThemeFolder, true, AngePath.SHEET_SEARCH_PATTERN)) {
			string themeName = Util.GetNameWithExtension(themePath);
			Util.DeleteFile(Util.CombinePaths(EngineUtil.ThemeRoot, themeName));
		}

	}


	public static bool ImportFileAsCustomPackage (string packagePath, out PackageInfo packInfo) {
		packInfo = null;
		if (!Util.FileExists(packagePath)) return false;
		try {
			string tempPath = Util.CombinePaths(AngePath.TempDataPath, System.Guid.NewGuid().ToString());
			ZipFile.ExtractToDirectory(packagePath, tempPath);
			packInfo = GetInfoFromPackageFolder(tempPath);
			if (packInfo == null || string.IsNullOrWhiteSpace(packInfo.PackageName)) return false;
			string finalPath = Util.CombinePaths(EngineUtil.CustomPackagesRoot, packInfo.PackageName);
			Util.DeleteFolder(finalPath);
			return Util.MoveFolder(tempPath, finalPath);
		} catch (System.Exception ex) {
			Debug.LogException(ex);
			return false;
		}
	}


	public static bool ExportProjectAsCustomPackageFile (Project project, string packageName, string displayName, string description, string exportPath, out string errorMsg) {
		try {
			errorMsg = "";
			if (project == null) {
				errorMsg = "Project object not found.";
				return false;
			}

			string tempFolder = Util.CombinePaths(AngePath.TempDataPath, System.Guid.NewGuid().ToString());
			var type = project.Universe.Info.ProjectType;

			// Info
			var info = new PackageInfo() {
				PackageName = packageName,
				DisplayName = displayName,
				Description = description,
				CreatorName = project.Universe.Info.DeveloperName,
				Priority = 0,
			};
			JsonUtil.SaveJsonToPath(info, Util.CombinePaths(tempFolder, "Info.json"), true);

			// Dll
			if (type == ProjectType.Game) {
				string dllName = EngineUtil.GetGameLibraryDllNameWithoutExtension(project.Universe.Info.ProductName);
				dllName = $"{dllName}.dll";
				string dllPath = Util.CombinePaths(project.BuildPath, dllName);
				if (!Util.FileExists(dllPath)) {
					errorMsg = "Game library dll file not found. Try recompile the project.";
					return false;
				}
				Util.CopyFile(dllPath, Util.CombinePaths(tempFolder, $"{packageName}.dll"));
			}

			// Artwork Sheet
			if (type == ProjectType.Artwork || type == ProjectType.Game) {
				string sourceSheetPath = Util.CombinePaths(project.Universe.GameSheetPath);
				if (Util.FileExists(sourceSheetPath)) {
					Util.CopyFile(sourceSheetPath, Util.CombinePaths(tempFolder, $"{packageName}.{AngePath.SHEET_FILE_EXT}"));
				} else if (type == ProjectType.Artwork) {
					errorMsg = "Game artwork sheet file not found.";
					return false;
				}
			}

			// Theme
			if (type == ProjectType.EngineTheme) {
				string sourceSheetPath = Util.CombinePaths(project.Universe.GameSheetPath);
				if (!Util.FileExists(sourceSheetPath)) {
					errorMsg = "Game artwork sheet file not found.";
					return false;
				}
				Util.CopyFile(sourceSheetPath, Util.CombinePaths(tempFolder, "Theme", $"{packageName}.{AngePath.SHEET_FILE_EXT}"));
			}

			// Icon
			if (Util.FileExists(project.IconPath)) {
				var iconTextures = EngineUtil.LoadTexturesFromIco(project.IconPath, firstOneOnly: false);
				if (iconTextures != null && iconTextures.Length > 0) {
					int maxSizeIndex = -1;
					int maxSize = -1;
					for (int i = 0; i < iconTextures.Length; i++) {
						var size = Game.GetTextureSize(iconTextures[i]);
						if (size.x > maxSize) {
							maxSize = size.x;
							maxSizeIndex = i;
						}
					}
					if (maxSizeIndex >= 0) {
						var png = Game.TextureToPngBytes(iconTextures[maxSizeIndex]);
						Util.BytesToFile(png, Util.CombinePaths(tempFolder, "Icon.png"));
					}
					for (int i = 0; i < iconTextures.Length; i++) {
						Game.UnloadTexture(iconTextures[i]);
					}
				}
			}

			// Zip
			ZipFile.CreateFromDirectory(tempFolder, exportPath);

			return true;
		} catch (System.Exception ex) {
			errorMsg = $"{ex.Source}\n{ex.Message}";
			return false;
		}
	}


}