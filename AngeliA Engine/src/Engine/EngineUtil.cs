using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using AngeliA;
using Task = System.Threading.Tasks.Task;

namespace AngeliaEngine;


public static class EngineUtil {



	#region --- SUB ---


	public enum PackageExportMode { Library, LibraryAndArtwork, Artwork, EngineTheme, }


	#endregion




	#region --- VAR ---


	// Const
	public const int ERROR_PROJECT_OBJECT_IS_NULL = -100;
	public const int ERROR_PROJECT_FOLDER_INVALID = -101;
	public const int ERROR_PUBLISH_DIR_INVALID = -102;
	public const int ERROR_PROJECT_FOLDER_NOT_EXISTS = -103;
	public const int ERROR_PRODUCT_NAME_INVALID = -104;
	public const int ERROR_DEV_NAME_INVALID = -105;
	public const int ERROR_RESULT_DLL_NOT_FOUND = -106;
	public const int ERROR_RUNTIME_FILE_NOT_FOUND = -107;
	public const int ERROR_UNIVERSE_FOLDER_NOT_FOUND = -108;
	public const int ERROR_EXE_FOR_RUN_NOT_FOUND = -109;
	public const int ERROR_DOTNET_SDK_NOT_FOUND = -110;
	public const int ERROR_ENTRY_PROJECT_NOT_FOUND = -111;
	public const int ERROR_ENTRY_RESULT_NOT_FOUND = -112;
	public const int ERROR_USER_CODE_COMPILE_ERROR = -113;
	public const int ERROR_CSPROJ_NOT_EXISTS = -114;
	private const int BACK_GROUND_BUILD_LOG_ID = 102735648;
	private static readonly int[] ICON_SIZES = [64, 128, 256, 512, 1024, 2048];

	// Api
	public static string DotnetSdkPath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "dotnet", "dotnet.exe");
	public static string ProjectTemplatePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate");
	public static string TemplateFrameworkDll_Debug => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate", "lib", "Debug", "AngeliA Framework.dll");
	public static string TemplateFrameworkDll_Release => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "ProjectTemplate", "lib", "Release", "AngeliA Framework.dll");
	public static string EntryExePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Entry.exe");
	public static string RiggedExePath => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Debug", "AngeliA Rigged.exe");
	public static string EntryProjectFolder => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Release");
	public static string EntryProjectCsproj => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Runtime", "Release", "AngeliA Entry for Publish.csproj");
	public static string ThemeRoot => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Theme");
	public static string PackagesRoot => Util.CombinePaths(AngePath.BuiltInUniverseRoot, "Packages");
	public static string CustomPackagesRoot => Util.CombinePaths(AngePath.PersistentDataPath, "Packages");
	public static bool BuildingProjectInBackground => BuildProjectTask != null && BuildProjectTask.Status == TaskStatus.Running;
	public static long LastBackgroundBuildModifyDate { get; private set; }
	public static int LastBackgroundBuildReturnCode { get; private set; }
	public static Queue<string> BackgroundBuildMessages { get; } = new(capacity: 32);

	// Cache
	private static event System.Action<int> OnProjectBuiltInBackgroundHandler;

	// Data
	private static readonly StringBuilder CacheBuilder = new();
	private static Task BuildProjectTask = null;


	#endregion




	#region --- MSG ---


	[OnGameInitializeLater]
	internal static void OnGameInitializeLater () {
		Util.LinkEventWithAttribute<OnProjectBuiltInBackgroundAttribute>(typeof(EngineUtil), nameof(OnProjectBuiltInBackgroundHandler));
		Debug.OnLogInternal += OnLogMessage;
		Debug.OnLogErrorInternal += OnLogMessage;
	}


	#endregion




	#region --- API ---


	// AngeliA
	public static int BuildAngeliaProject (Project project) {
		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;
		return BuildAngeliaProjectLogic(project, "", publish: false, logID: 0);
	}


	public static int PublishAngeliaProject (Project project, string publishDir) {
		if (project == null) return ERROR_PROJECT_OBJECT_IS_NULL;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return ERROR_DEV_NAME_INVALID;
		return BuildAngeliaProjectLogic(
			project, publishDir, publish: true, logID: 0
		);
	}


	public static bool BuildAngeliaProjectInBackground (Project project, long srcModifyDate) {

		// Gate
		if (project == null) return false;
		if (!Util.IsValidForFileName(project.Universe.Info.DeveloperName)) return false;
		if (BuildingProjectInBackground) return false;

		// Project >> Cache
		LastBackgroundBuildReturnCode = int.MinValue;
		LastBackgroundBuildModifyDate = srcModifyDate;

		// Task
		BuildProjectTask = Task.Factory.StartNew(BuildFromCache, project);

		return true;

		// Func
		static void BuildFromCache (object projectObj) {
			try {
				if (projectObj is not Project project) return;
				BackgroundBuildMessages.Clear();
				LastBackgroundBuildReturnCode = BuildAngeliaProjectLogic(project, "", publish: false);
				OnProjectBuiltInBackgroundHandler?.Invoke(LastBackgroundBuildReturnCode);
			} catch (System.Exception ex) {
				System.Console.WriteLine(ex.Message + "\n" + ex.Source);
			}
		}
	}


	public static void RunAngeliaBuild (Project project) {
		string entryPath = EntryExePath;
		if (!Util.FileExists(entryPath)) return;
		var process = new Process();
		process.StartInfo.FileName = entryPath;
		process.StartInfo.UseShellExecute = false;
		process.StartInfo.CreateNoWindow = false;
		process.StartInfo.Arguments = $"DontCloseCmd -uni:{Util.Path_to_ArgPath(project.UniversePath)} -lib:{Util.Path_to_ArgPath(project.BuildPath)}";
		process.StartInfo.WorkingDirectory = Util.GetParentPath(entryPath);
		process.Start();
	}


	// Modyfy Date
	public static long GetScriptModifyDate (Project project) {
		if (project == null || !Util.FolderExists(project.SourceCodePath)) return 0;
		long result = 0;
		foreach (var path in Util.EnumerateFiles(project.SourceCodePath, false, "*.cs")) {
			result = System.Math.Max(result, Util.GetFileModifyDate(path));
		}
		return result;
	}


	public static long GetBuildLibraryModifyDate (Project project) {
		if (project == null || !Util.FolderExists(project.BuildPath)) return 0;
		long result = 0;
		foreach (string path in Util.EnumerateFiles(project.BuildPath, true, "*.dll")) {
			result = System.Math.Max(result, Util.GetFileModifyDate(path));
		}
		return result;
	}


	// Res
	public static object[] LoadTexturesFromIco (string icoPath, bool firstOneOnly = false) {

		using var stream = File.OpenRead(icoPath);
		using var reader = new BinaryReader(stream);

		reader.ReadUInt16(); // ignore. Should be 0

		ushort type = reader.ReadUInt16();
		if (type != 1) {
			throw new System.Exception("Invalid type. The stream is not an icon file");
		}
		ushort num_of_images = reader.ReadUInt16();

		if (num_of_images == 0) return [];

		if (firstOneOnly) num_of_images = 1;
		var results = new object[num_of_images];
		var cache = new (uint offset, uint size, int bpp)[num_of_images];

		for (var i = 0; i < num_of_images; i++) {
			var width = reader.ReadByte();
			var height = reader.ReadByte();
			var colors = reader.ReadByte();
			reader.ReadByte(); // ignore. Should be 0
			var color_planes = reader.ReadUInt16(); // should be 0 or 1
			ushort bits_per_pixel = reader.ReadUInt16();
			uint size = reader.ReadUInt32();
			uint offset = reader.ReadUInt32();
			cache[i] = (offset, size, bits_per_pixel / 8);
		}

		for (int imgIndex = 0; imgIndex < num_of_images; imgIndex++) {
			var (offset, size, bpp) = cache[imgIndex];
			if (reader.BaseStream.Position < offset) {
				var dummy_bytes_to_read = (int)(offset - reader.BaseStream.Position);
				if (stream.CanSeek) {
					stream.Seek(dummy_bytes_to_read, SeekOrigin.Current);
				} else {
					reader.ReadBytes(dummy_bytes_to_read);
				}
			}
			var bytes = reader.ReadBytes((int)size);

			if (bytes == null || bytes.Length <= 4) continue;

			if (bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4e && bytes[3] == 0x47) {
				// PNG
				results[imgIndex] = Game.PngBytesToTexture(bytes);
			} else {
				// BMP
				int dataOffset = bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
				int width = bytes[4] | (bytes[5] << 8) | (bytes[6] << 16) | (bytes[7] << 24);
				int height = (bytes[8] | (bytes[9] << 8) | (bytes[10] << 16) | (bytes[11] << 24)).Abs() / 2;
				if (width != height) {
					Debug.LogWarning($"Icon with must be same with height ({width} x {height})");
					continue;
				}
				if (width > 4096 || height > 4096) continue;
				var colors = new Color32[width * height];
				int index = 0;
				if (bpp == 4) {
					// BGRA
					for (int i = dataOffset; i < bytes.Length - 3 && index < colors.Length; i += 4, index++) {
						colors[index].b = bytes[i + 0];
						colors[index].g = bytes[i + 1];
						colors[index].r = bytes[i + 2];
						colors[index].a = bytes[i + 3];
					}
				} else if (bpp == 3) {
					// BGR
					for (int i = dataOffset; i < bytes.Length - 2 && index < colors.Length; i += 3, index++) {
						colors[index].b = bytes[i + 0];
						colors[index].g = bytes[i + 1];
						colors[index].r = bytes[i + 2];
						colors[index].a = 255;
					}
				} else if (bpp == 1) {
					// Grey
					for (int i = dataOffset; i < bytes.Length && index < colors.Length; i++, index++) {
						byte value = bytes[i];
						colors[index].a = 255;
						colors[index].r = value;
						colors[index].g = value;
						colors[index].b = value;
					}
				}

				results[imgIndex] = Game.GetTextureFromPixels(colors, width, height);

			}

		}

		return results;
	}


	public static bool CreateIcoFromPng (string pngPath, string icoPath) {
		if (
			!Util.FileExists(pngPath) ||
			Game.PngBytesToTexture(Util.FileToBytes(pngPath)) is not object texture ||
			!Game.IsTextureReady(texture)
		) return false;
		var pngBytes = new byte[ICON_SIZES.Length][];
		for (int i = 0; i < ICON_SIZES.Length; i++) {
			int size = ICON_SIZES[i];
			var newTexture = Game.GetResizedTexture(texture, size, size);
			if (Game.IsTextureReady(newTexture)) {
				pngBytes[i] = Game.TextureToPngBytes(newTexture);
			} else {
				return false;
			}
			Game.UnloadTexture(newTexture);
		}
		Game.UnloadTexture(texture);
		return CreateIcoFromPng(pngBytes, icoPath);
	}


	public static bool CreateIcoFromPng (byte[][] pngBytes, string icoPath) {
		if (pngBytes == null || pngBytes.Length == 0) return false;

		using var stream = File.OpenWrite(icoPath);
		using var writer = new BinaryWriter(stream);

		int numOfImage = pngBytes.Length;

		writer.Write((ushort)0); // (ignore)

		writer.Write((ushort)1); // type
		writer.Write((ushort)numOfImage); // num_of_images

		const int INFO_SIZE = 16;
		int offsetHead = (int)writer.BaseStream.Position + INFO_SIZE * numOfImage;
		int currentOffset = offsetHead;

		for (int i = 0; i < numOfImage; i++) {
			writer.Write((byte)0); // w
			writer.Write((byte)0); // h
			writer.Write((byte)0); // colors
			writer.Write((byte)0); // (ignore)
			writer.Write((ushort)1); // color_planes
			writer.Write((ushort)32); // bits_per_pixel
			writer.Write((uint)pngBytes[i].Length);   // size
			writer.Write((uint)currentOffset);   // offset
			currentOffset += pngBytes[i].Length;
		}

		for (int i = 0; i < numOfImage; i++) {
			writer.Write(pngBytes[i]); // data
		}

		writer.Flush();
		return true;
	}


	// Dialog
	public static void TryCompileDialogueFiles (string workspace, string exportRoot, bool forceCompile) {

		var ignoreDelete = new HashSet<string>();

		// For all Editable Conversation Files
		foreach (var path in Util.EnumerateFiles(workspace, false, $"*.{AngePath.EDITABLE_CONVERSATION_FILE_EXT}")) {

			string globalName = Util.GetNameWithoutExtension(path);
			string conFolderPath = Util.CombinePaths(exportRoot, globalName);
			ignoreDelete.Add(globalName);

			// Check Dirty
			long modTime = Util.GetFileModifyDate(path);
			long creationTime = Util.GetFileCreationDate(path);
			if (!forceCompile && modTime == creationTime && Util.FolderExists(conFolderPath)) continue;
			Util.SetFileModifyDate(path, creationTime);

			// Delete Existing for All Languages
			Util.DeleteFolder(conFolderPath);
			Util.CreateFolder(conFolderPath);

			// Compile
			var builder = new StringBuilder();
			string currentIso = "en";
			bool contentFlag0 = false;
			bool contentFlag1 = false;
			foreach (string line in Util.ForAllLinesInFile(path, Encoding.UTF8)) {

				string trimedLine = line.TrimStart(' ', '\t');

				// Empty
				if (string.IsNullOrWhiteSpace(trimedLine)) {
					if (contentFlag1) {
						contentFlag0 = contentFlag1;
						contentFlag1 = false;
					}
					continue;
				}

				if (trimedLine[0] == '>') {
					// Switch Language
					string iso = trimedLine[1..];
					if (trimedLine.Length > 1 && Util.IsSupportedLanguageISO(iso) && currentIso != iso) {
						// Make File
						string targetPath = Util.CombinePaths(
							conFolderPath,
							$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
						);
						Util.TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
						builder.Clear();
						currentIso = iso;
					}
					contentFlag0 = false;
					contentFlag1 = false;
				} else {
					// Line
					if (trimedLine.StartsWith("\\>")) {
						trimedLine = trimedLine[1..];
					}
					if (trimedLine.Length > 0) {
						// Add Gap
						if (trimedLine[0] == '@') {
							contentFlag0 = false;
							contentFlag1 = false;
						} else {
							if (contentFlag0 && !contentFlag1) {
								builder.Append('\n');
							}
							contentFlag0 = contentFlag1;
							contentFlag1 = true;
						}
						// Append Line
						if (builder.Length != 0) builder.Append('\n');
						builder.Append(trimedLine);
					}
				}
			}

			// Make File for Last Language 
			if (builder.Length != 0) {
				string targetPath = Util.CombinePaths(
					conFolderPath,
					$"{currentIso}.{AngePath.CONVERSATION_FILE_EXT}"
				);
				Util.TextToFile(builder.ToString(), targetPath, Encoding.UTF8);
				builder.Clear();
			}

		}

		// Delete Useless Old Files
		if (ignoreDelete != null) {
			List<string> deleteList = null;
			foreach (var path in Util.EnumerateFolders(exportRoot, true, "*")) {
				if (ignoreDelete.Contains(Util.GetNameWithoutExtension(path))) continue;
				deleteList ??= [];
				deleteList.Add(path);
			}
			if (deleteList != null) {
				foreach (var path in deleteList) {
					Util.DeleteFolder(path);
					Util.DeleteFile(path + ".meta");
				}
			}
		}
	}


	// Resources
	public static void ImportMusicFile (Project project, string filePath) {
		if (project == null) return;
		Util.CopyFile(filePath, Util.CombinePaths(
			project.Universe.MusicRoot,
			Util.GetNameWithExtension(filePath)
		));
		Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, project.UniversePath);
	}


	public static void ImportSoundFile (Project project, string filePath) {
		if (project == null) return;
		Util.CopyFile(filePath, Util.CombinePaths(
			project.Universe.SoundRoot,
			Util.GetNameWithExtension(filePath)
		));
		Game.SyncAudioPool(Universe.BuiltIn.UniverseRoot, project.UniversePath);
	}


	public static void ImportFontFile (Project project, string filePath) {
		if (project == null) return;
		string newName = Util.GetNameWithExtension(filePath);
		Util.CopyFile(filePath, Util.CombinePaths(project.Universe.FontRoot, newName));
		Game.SyncFontsWithPool(project.Universe.FontRoot);
	}


	public static bool ImportIconFile (Project project, string filePath) {
		if (project == null) return false;
		return CreateIcoFromPng(filePath, project.IconPath);
	}


	// Package
	public static PackageInfo GetInfoFromPackageFolder (string packageFolder) {
		string infoPath = Util.CombinePaths(packageFolder, "Info.json");
		if (!Util.FileExists(infoPath)) return null;
		if (JsonUtil.LoadJsonFromPath<PackageInfo>(infoPath) is not PackageInfo info) return null;
		string iconPath = Util.CombinePaths(packageFolder, "Icon.png");
		info.IconTexture = Game.PngBytesToTexture(Util.FileToBytes(iconPath));
		info.DllPath = Util.CombinePaths(packageFolder, $"{info.PackageName}.dll");
		info.SheetPath = Util.CombinePaths(packageFolder, $"{info.PackageName}.{AngePath.SHEET_FILE_EXT}");
		info.ThemeRoot = Util.CombinePaths(packageFolder, "Theme");
		info.DllFounded = Util.FileExists(info.DllPath);
		info.SheetFounded = Util.FileExists(info.SheetPath);
		info.ThemeFounded = Util.GetFileCount(info.ThemeRoot, $"*.{AngePath.SHEET_FILE_EXT}", System.IO.SearchOption.TopDirectoryOnly) > 0;
		return info;
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
			foreach (string themePath in Util.EnumerateFiles(packageInfo.ThemeRoot, true, $"*.{AngePath.SHEET_FILE_EXT}")) {
				string themeName = Util.GetNameWithExtension(themePath);
				Util.CopyFile(themePath, Util.CombinePaths(ThemeRoot, themeName));
			}
		}

	}


	public static void UninstallPackage (Project project, PackageInfo packageInfo) {
		if (project == null) return;
		string dllName = $"{packageInfo.PackageName}.dll";
		string sheetName = $"{packageInfo.PackageName}.{AngePath.SHEET_FILE_EXT}";
		Util.DeleteFile(Util.CombinePaths(project.DllLibPath_Debug, dllName));
		Util.DeleteFile(Util.CombinePaths(project.DllLibPath_Release, dllName));
		Util.DeleteFile(Util.CombinePaths(project.Universe.SheetRoot, sheetName));
		if (packageInfo.ThemeFounded) {
			foreach (string themePath in Util.EnumerateFiles(packageInfo.ThemeRoot, true, $"*.{AngePath.SHEET_FILE_EXT}")) {
				string themeName = Util.GetNameWithExtension(themePath);
				Util.DeleteFile(Util.CombinePaths(ThemeRoot, themeName));
			}
		}
	}


	public static bool IsPackagedInstalled (Project project, PackageInfo packageInfo) {
		if (project == null) return false;
		string dllName = $"{packageInfo.PackageName}.dll";
		string dllPathDebug = Util.CombinePaths(project.DllLibPath_Debug, dllName);
		string dllPathRelease = Util.CombinePaths(project.DllLibPath_Release, dllName);
		// Get Installed
		bool installed = false;
		if (packageInfo.DllFounded) {
			installed = Util.FileExists(dllPathDebug) || Util.FileExists(dllPathRelease);
		}
		if (!installed && packageInfo.SheetFounded) {
			string targetSheetName = $"{packageInfo.PackageName}.{AngePath.SHEET_FILE_EXT}";
			installed = Util.FileExists(Util.CombinePaths(project.Universe.SheetRoot, targetSheetName));
		}
		if (!installed && packageInfo.ThemeFounded) {
			// Get Engine Theme Hash
			var engineThemeHash = new HashSet<int>();
			foreach (string themePath in Util.EnumerateFiles(ThemeRoot, true, $"*.{AngePath.SHEET_FILE_EXT}")) {
				string themeName = Util.GetNameWithExtension(themePath);
				engineThemeHash.Add(themeName.AngeHash());
			}
			foreach (string themePath in Util.EnumerateFiles(packageInfo.ThemeRoot, true, $"*.{AngePath.SHEET_FILE_EXT}")) {
				string themeName = Util.GetNameWithExtension(themePath);
				if (engineThemeHash.Contains(themeName.AngeHash())) {
					installed = true;
					break;
				}
			}
		}
		return installed;
	}


	public static bool ImportFileAsCustomPackage (string packagePath, out PackageInfo packInfo) {
		packInfo = null;
		if (!Util.FileExists(packagePath)) return false;
		try {
			string tempPath = Util.CombinePaths(AngePath.TempDataPath, System.Guid.NewGuid().ToString());
			ZipFile.ExtractToDirectory(packagePath, tempPath);
			var info = GetInfoFromPackageFolder(tempPath);
			if (info == null || string.IsNullOrWhiteSpace(info.PackageName)) return false;
			string finalPath = Util.CombinePaths(CustomPackagesRoot, info.PackageName);
			Util.DeleteFolder(finalPath);
			return Util.MoveFolder(tempPath, finalPath);
		} catch (System.Exception ex) {
			Debug.LogException(ex);
			return false;
		}
	}


	public static bool ExportProjectAsCustomPackageFile (Project project, string packageName, string displayName, string description, string exportPath, PackageExportMode mode, out string errorMsg) {
		try {
			errorMsg = "";
			if (project == null) return false;

			string tempFolder = Util.CombinePaths(AngePath.TempDataPath, System.Guid.NewGuid().ToString());

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
			if (mode == PackageExportMode.Library || mode == PackageExportMode.LibraryAndArtwork) {
				string dllName = GetGameLibraryDllNameWithoutExtension(project.Universe.Info.ProductName);
				dllName = $"{dllName}.dll";
				string dllPath = Util.CombinePaths(project.BuildPath, dllName);
				if (!Util.FileExists(dllPath)) {
					errorMsg = "Game library dll file not found. Try recompile the project.";
					return false;
				}
				Util.CopyFile(dllPath, Util.CombinePaths(tempFolder, $"{packageName}.dll"));
			}

			// Artwork Sheet
			if (mode == PackageExportMode.Artwork || mode == PackageExportMode.LibraryAndArtwork) {
				string sourceSheetPath = Util.CombinePaths(project.Universe.GameSheetPath);
				if (!Util.FileExists(sourceSheetPath)) {
					errorMsg = "Game artwork sheet file not found.";
					return false;
				}
				Util.CopyFile(sourceSheetPath, Util.CombinePaths(tempFolder, $"{packageName}.{AngePath.SHEET_FILE_EXT}"));
			}

			// Theme
			if (mode == PackageExportMode.EngineTheme) {
				string sourceSheetPath = Util.CombinePaths(project.Universe.GameSheetPath);
				if (!Util.FileExists(sourceSheetPath)) {
					errorMsg = "Game artwork sheet file not found.";
					return false;
				}
				Util.CopyFile(sourceSheetPath, Util.CombinePaths(tempFolder, "Theme", $"{packageName}.{AngePath.SHEET_FILE_EXT}"));
			}

			// Icon
			if (Util.FileExists(project.IconPath)) {
				var iconTextures = LoadTexturesFromIco(project.IconPath, firstOneOnly: false);
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


	#endregion




	#region --- LGC ---


	private static int BuildAngeliaProjectLogic (Project project, string publishDir, bool publish, int logID = BACK_GROUND_BUILD_LOG_ID) {

		var info = project.Universe.Info;
		string projectPath = project.ProjectPath;
		string csprojPath = project.CsprojPath;
		string productName = info.ProductName;
		string buildPath = project.BuildPath;
		string tempBuildPath = project.TempBuildPath;
		string tempPublishPath = project.TempPublishPath;
		string tempRoot = project.TempRoot;
		string iconPath = project.IconPath;
		string universePath = project.UniversePath;
		string versionStr = $"{info.MajorVersion}.{info.MinorVersion}.{info.PatchVersion}";
		string libAssemblyName = GetGameLibraryDllNameWithoutExtension(productName);

#if DEBUG
		var watch = Stopwatch.StartNew();
		Debug.Log("Start to Build AngeliA Project");
#endif
		if (!Util.IsPathValid(projectPath)) return ERROR_PROJECT_FOLDER_INVALID;
		if (publish && !Util.IsPathValid(publishDir)) return ERROR_PUBLISH_DIR_INVALID;
		if (publish && !Util.FolderExists(EntryProjectFolder)) return ERROR_ENTRY_PROJECT_NOT_FOUND;
		if (!Util.FolderExists(projectPath)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;
		if (!Util.FileExists(csprojPath)) return ERROR_CSPROJ_NOT_EXISTS;
		if (!Util.FileExists(DotnetSdkPath)) return ERROR_DOTNET_SDK_NOT_FOUND;
		if (!Util.IsValidForFileName(productName)) return ERROR_PRODUCT_NAME_INVALID;


		// ===== Build =====


		// Delete Build Library Folder
		Util.DeleteFolder(buildPath);

		// Build Dotnet Project
		int returnCode = BuildDotnetProject(
			projectPath,
			DotnetSdkPath,
			csprojPath,
			publish: false,
			debug: !publish,
			assemblyName: libAssemblyName,
			version: versionStr,
			outputPath: tempBuildPath,
			logID: logID
		);

		if (returnCode != 0) return ERROR_USER_CODE_COMPILE_ERROR;

		string resultDllPath = Util.CombinePaths(tempBuildPath, $"{libAssemblyName}.dll");
		if (!Util.FileExists(resultDllPath)) return ERROR_RESULT_DLL_NOT_FOUND;

		// Copy Result Dll to Lib Folder
		string gameLibDllName = Util.GetNameWithExtension(resultDllPath);
		string gameLibBuildPath = Util.CombinePaths(buildPath, gameLibDllName);
		Util.CreateFolder(buildPath);
		Util.CopyFile(resultDllPath, gameLibBuildPath);

		// Copy Package Dlls to Lib Folder
		string debugLibFolder = Project.GetLibraryFolderPath(projectPath, true);
		foreach (var packagDllPath in Util.EnumerateFiles(debugLibFolder, true, "*.dll")) {
			string dllName = Util.GetNameWithExtension(packagDllPath);
			if (dllName == "AngeliA Framework.dll") continue;
			Util.CopyFile(packagDllPath, Util.CombinePaths(buildPath, dllName));
		}


		// ===== Publish =====


		if (publish) {

			// Build Entry Exe for Publish
			int pubReturnCode = BuildDotnetProject(
				EntryProjectFolder,
				DotnetSdkPath,
				EntryProjectCsproj,
				publish: true,
				debug: false,
				assemblyName: productName,
				version: versionStr,
				outputPath: tempBuildPath,
				publishDir: tempPublishPath,
				iconPath: iconPath,
				logID: -1
			);
			if (pubReturnCode != 0) return returnCode;

			// Move Result Exe to Publish Folder
			string resultExePath = Util.CombinePaths(tempPublishPath, $"{productName}.exe");
			if (!Util.FileExists(resultExePath)) return ERROR_ENTRY_RESULT_NOT_FOUND;
			string pubResultExePath = Util.CombinePaths(publishDir, $"{productName}.exe");
			Util.CopyFile(resultExePath, pubResultExePath);

			// Copy Game Libs to Publish Folder
			if (!Util.FileExists(gameLibBuildPath)) return ERROR_RESULT_DLL_NOT_FOUND;
			string publishLibRoot = Util.CombinePaths(publishDir, "Library");
			Util.CopyFile(gameLibBuildPath, Util.CombinePaths(publishLibRoot, gameLibDllName));

			// Copy Package Dlls to Lib Folder
			string releaseLibFolder = Project.GetLibraryFolderPath(projectPath, false);
			foreach (var packagDllPath in Util.EnumerateFiles(releaseLibFolder, true, "*.dll")) {
				string dllName = Util.GetNameWithExtension(packagDllPath);
				if (dllName == "AngeliA Framework.dll") continue;
				Util.CopyFile(packagDllPath, Util.CombinePaths(publishLibRoot, dllName));
			}

			// Copy Universe to Publish Folder
			if (!Util.FolderExists(universePath)) return ERROR_UNIVERSE_FOLDER_NOT_FOUND;
			Util.CreateFolder(publishDir);
			string universePublishFolderPath = Util.CombinePaths(publishDir, "Universe");
			Util.CopyFolder(universePath, universePublishFolderPath, true, true);

		}

		// Delete Temp Folder
		Util.DeleteFolder(tempRoot);
#if DEBUG
		watch.Stop();
		Debug.Log($"{watch.ElapsedMilliseconds / 1000f:0.00}s AngeliA Project Built Finish");
#endif
		return 0;
	}


	private static int BuildDotnetProject (
		string projectFolder, string sdkPath, string csprojPath, bool publish, bool debug, int logID,
		string assemblyName = "", string version = "", string outputPath = "",
		string publishDir = "", string iconPath = ""
	) {

		if (!Util.FolderExists(projectFolder)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;
		if (!Util.FileExists(csprojPath)) return ERROR_PROJECT_FOLDER_NOT_EXISTS;

		CacheBuilder.Clear();

		// SDK
		CacheBuilder.AppendWithDoubleQuotes(sdkPath);

		// Build
		CacheBuilder.Append(publish ? " publish " : " build ");

		// Project Path
		CacheBuilder.AppendWithDoubleQuotes(csprojPath);

		// Config
		CacheBuilder.Append(debug ? " -c debug" : " -c release");

		// Dependencies
		CacheBuilder.Append(" --no-dependencies");

		// Prop
		if (!string.IsNullOrWhiteSpace(assemblyName)) {
			CacheBuilder.Append($" -p:AssemblyName=\"{assemblyName}\"");
		}
		if (!string.IsNullOrWhiteSpace(version)) {
			CacheBuilder.Append($" -p:Version={version}");
		}
		if (!string.IsNullOrWhiteSpace(outputPath)) {
			CacheBuilder.Append($" -p:OutputPath=\"{outputPath}\"");
		}
		if (!string.IsNullOrWhiteSpace(publishDir)) {
			CacheBuilder.Append($" -p:PublishDir=\"{publishDir}\"");
		}
		if (!string.IsNullOrWhiteSpace(iconPath) && Util.FileExists(iconPath)) {
			CacheBuilder.Append($" -p:ApplicationIcon=\"{iconPath}\"");
		}

		int resultID = Util.ExecuteCommand(projectFolder, CacheBuilder.ToStringWithDoubleQuotes(), logID: logID);

		return resultID;
	}


	private static void OnLogMessage (int id, string message) {
		if (id != BACK_GROUND_BUILD_LOG_ID) return;
		BackgroundBuildMessages.Enqueue(message);
	}


	private static string GetGameLibraryDllNameWithoutExtension (string productName) => $"lib.{productName}";


	#endregion




}