using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;



namespace PixelJelly.Editor {
	public partial class PixelJellyWindow {



		// Workflow
		public static bool ExportTexture (JellyBehaviour behaviour, string path) => ExportTexture(behaviour, path, out _, out _);
		public static bool ExportTexture (JellyBehaviour behaviour, string path, out Texture2D texture, out Sprite[] sprites) {
			texture = null;
			sprites = null;
			if (behaviour == null || string.IsNullOrEmpty(path)) { return false; }
			path = EditorUtil.FixedRelativePath(path);
			(texture, sprites) = behaviour.ExportTexture();
			if (texture == null || sprites == null) { return false; }
			var sMetaList = new List<SpriteMetaData>();
			foreach (var sprite in sprites) {
				if (sprite == null) { continue; }
				sMetaList.Add(new SpriteMetaData() {
					name = sprite.name,
					rect = sprite.rect,
					pivot = sprite.pivot / sprite.rect.size,
					border = sprite.border,
					alignment = 9,
				});
			}
			PixelPostprocessor.Add(path, new PixelPostprocessor.TextureImportData() {
				PixelPerUnit = 16,
				SpriteMetas = sMetaList.ToArray(),
			});
			if (Util.FileExists(path)) {
				Util.DeleteFile(path);
				Util.DeleteFile(path + ".meta");
			}
			Util.ByteToFile(texture.EncodeToPNG(), path);
			return true;
		}


		public static bool ExportAnimation (JellyBehaviour behaviour, string path) {
			if (behaviour == null || string.IsNullOrEmpty(path)) { return false; }
			string tPath = Util.ChangeExtension(path, "png");
			if (!ExportTexture(behaviour, tPath, out _, out var sprites)) { return false; }
			var metas = new PixelPostprocessor.AnimationFrameImportData[sprites.Length];
			for (int i = 0; i < sprites.Length; i++) {
				var sprite = sprites[i];
				metas[i] = new PixelPostprocessor.AnimationFrameImportData() {
					TexturePath = tPath,
					Duration = behaviour.GetFrameDuration(i) / 1000f,
					SpriteName = sprite.name,
				};
			}
			PixelPostprocessor.Add(EditorUtil.FixedRelativePath(path), metas);
			return true;
		}


		private void ImportData (JellyBehaviour behaviour, string path) {
			if (behaviour == null || string.IsNullOrEmpty(path) || !Util.FileExists(path)) { return; }
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			path = EditorUtil.FixedRelativePath(path);
			string sourcePath = GetBehaviourAssetPath(behaviour.GetType());
			if (!Util.FileExists(sourcePath)) {
				Debug.LogWarning("[Pixel Jelly] Failed to get data source.");
				return;
			}
			try {
				var targetType = AssetDatabase.GetMainAssetTypeAtPath(path);
				if (targetType != behaviour.GetType()) {
					EditorUtil.Dialog("Warning", $"File type not match.\n(Expect \"{behaviour.GetType()}\", Get \"{targetType.Name}\")", "OK");
					return;
				}
			} catch { }
			Util.DeleteFile(sourcePath);
			Util.DeleteFile(sourcePath + ".meta");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Util.CopyFile(path, sourcePath);
			var obj = AssetDatabase.LoadAssetAtPath<Object>(sourcePath);
			if (obj != null) {
				EditorUtility.SetDirty(obj);
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtil.Dialog("Done", "Data Imported.", "OK");
			CanvasDirty = true;
			BehaviourPool.Clear();
		}


		private void ExportData (JellyBehaviour behaviour, string path) {
			if (behaviour == null || string.IsNullOrEmpty(path)) { return; }
			path = EditorUtil.FixedRelativePath(path);
			string sourcePath = GetBehaviourAssetPath(behaviour.GetType());
			if (!Util.FileExists(sourcePath)) {
				Debug.LogWarning("[Pixel Jelly] Failed to get data source.");
				return;
			}
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			Util.DeleteFile(path);
			Util.DeleteFile(path + ".meta");
			Util.CopyFile(sourcePath, path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
			if (obj != null) {
				EditorGUIUtility.PingObject(obj);
				Selection.activeObject = obj;
			}
		}


		private void ResetBehaviour (JellyBehaviour behaviour) {
			if (behaviour == null) { return; }
			if (!EditorUtil.Dialog("Confirm", $"Reset data for \"{behaviour.FinalDisplayName}\"?", "Reset", "Cancel")) { return; }
			string defaultPath = GetBehaviourDefaultPath(behaviour.GetType());
			string sourcePath = GetBehaviourAssetPath(behaviour.GetType());
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			try {
				Util.DeleteFile(sourcePath);
				Util.DeleteFile(sourcePath + ".meta");
				if (Util.FileExists(defaultPath)) {
					Util.CopyFile(defaultPath, sourcePath);
				}
			} catch { }
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			CanvasDirty = true;
			BehaviourPool.Clear();
		}


		private void SetBehaviourAsDefault (JellyBehaviour behaviour) {
			if (behaviour == null) { return; }
			AssetDatabase.SaveAssets();
			if (!EditorUtil.Dialog("Confirm", $"Set \"{behaviour.FinalDisplayName}\" as default value?", "Set as Default", "Cancel")) { return; }
			string sourcePath = GetBehaviourAssetPath(behaviour.GetType());
			if (!Util.FileExists(sourcePath)) {
				EditorUtil.Dialog("Fail", "Can not find source asset :(", "OK");
				return;
			}
			string defaultPath = GetBehaviourDefaultPath(behaviour.GetType());
			if (Util.FileExists(defaultPath)) {
				Util.DeleteFile(defaultPath);
			}
			if (Util.FileExists(defaultPath + ".meta")) {
				Util.DeleteFile(defaultPath + ".meta");
			}
			Util.CopyFile(sourcePath, defaultPath);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		// Behaviour
		private void InitBehaviourPool () {
			BehaviourPool.Clear();
			BehaviourPathPool.Clear();
			InspectorPool.Clear();
			BehaviourFoldMap.Clear();
			var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();
			// Get Opening Map
			if (!string.IsNullOrEmpty(OpeningConfig.Value)) {
				using var reader = new StringReader(OpeningConfig.Value);
				string key, value;
				while (
					!string.IsNullOrEmpty(key = reader.ReadLine()) &&
					!string.IsNullOrEmpty(value = reader.ReadLine())
				) {
					if (!BehaviourFoldMap.ContainsKey(key)) {
						BehaviourFoldMap.Add(key, value == "1");
					}
				}
			}
			// Load Inspectors
			foreach (var assembly in assemblies) {
				foreach (var type in assembly.GetTypes()) {
					var att = type.GetCustomAttribute<CustomJellyInspectorAttribute>(true);
					if (att != null && att.TargetType != null && !InspectorPool.ContainsKey(att.TargetType)) {
						if (System.Activator.CreateInstance(type) is JellyInspector ins) {
							InspectorPool.Add(att.TargetType, ins);
						}
					}
				}
			}
			// Load Pools
			foreach (var assembly in assemblies) {
				var types = assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(JellyBehaviour)));
				if (types.Count() == 0) { continue; }
				string assemblyName = assembly.GetName().Name;
				foreach (var type in types) {
					try {
						if (type.IsAbstract) { continue; }
						//if (type.IsAbstract || type.CustomAttributes.Any((att) => att.AttributeType == typeof(HideInPixelJellyAttribute))) { continue; }
						// Attribute Check
						bool hideBeh = false;
						bool hideRandomBtn = false;
						bool oneShotAnimation = false;
						foreach (var att in type.CustomAttributes) {
							if (att.AttributeType == typeof(HideInPixelJellyAttribute)) {
								hideBeh = true;
								break;
							}
							if (att.AttributeType == typeof(HideRandomButtonAttribute)) {
								hideRandomBtn = true;
							}
							if (att.AttributeType == typeof(OneShotAnimationAttribute)) {
								oneShotAnimation = true;
							}
						}
						if (hideBeh) { continue; }
						// Init Beh
						var pBehaviour = GetRefreshedAsset(type);
						if (pBehaviour != null) {
							string groupName = !string.IsNullOrEmpty(pBehaviour.DisplayGroup) ? pBehaviour.DisplayGroup.Replace("\n", "") : DEFAULT_GROUP_NAME;
							// Pool
							if (!BehaviourPool.ContainsKey(groupName)) {
								BehaviourPool.Add(groupName, new List<BehaviourSerializeData>());
								SetBehaviourFolding(groupName, !BehaviourFoldMap.ContainsKey(groupName) || BehaviourFoldMap[groupName]);
							}
							var bData = new BehaviourSerializeData(pBehaviour) {
								Group = groupName,
								ShowRandomButton = !hideRandomBtn,
								OneShotAnimation = oneShotAnimation,
							};
							BehaviourPool[groupName].Add(bData);
							// Path Pool
							var pKey = (assemblyName, type.Namespace, type.Name);
							if (!BehaviourPathPool.ContainsKey(pKey)) {
								BehaviourPathPool.Add(pKey, (bData, groupName, -1));
							}
						}
					} catch (System.Exception ex) { Debug.LogError(ex); }
				}
			}
			// Sort
			foreach (var pair in BehaviourPool) {
				if (pair.Value != null) {
					pair.Value.Sort((a, b) => a.Name.CompareTo(b.Name));
				}
			}
			// Path Pool Index
			foreach (var pair in BehaviourPool) {
				for (int i = 0; i < pair.Value.Count; i++) {
					var type = pair.Value[i].Behaviour.GetType();
					var pKey = (type.Assembly.GetName().Name, type.Namespace, type.Name);
					if (BehaviourPathPool.ContainsKey(pKey)) {
						var value = BehaviourPathPool[pKey];
						value.poolIndex = i;
						BehaviourPathPool[pKey] = value;
					}
				}
			}
			RefreshFavoriteForPool();
			// Load Selecting
			bool selectionSetted = false;
			foreach (var pair in BehaviourPool) {
				for (int i = 0; i < pair.Value.Count; i++) {
					var behaviourData = pair.Value[i];
					var type = behaviourData.Behaviour.GetType();
					if ($"{type.Assembly.GetName().Name}/{type.Name}" == SelectingBehaviourName.Value) {
						SetSelectingBehaviour(pair.Key, i);
						selectionSetted = true;
						break;
					}
				}
				if (selectionSetted) { break; }
			}
			if (!selectionSetted) {
				foreach (var pair in BehaviourPool) {
					for (int i = 0; i < pair.Value.Count; i++) {
						var behaviourData = pair.Value[i];
						if (behaviourData != null) {
							SetSelectingBehaviour(pair.Key, i);
							break;
						}
					}
				}
			}
			// Default Asset Cache
			RemoveUnusedDefaultAsset();
			// Unsed Asset Count
			UnusedAssetCount = 0;
			ForEachUnusedAssetCache((_) => UnusedAssetCount++);
			// Misc
			CheckerFloorTexture = null;
		}


		private void RemoveUnusedAssetCache () {
			ForEachUnusedAssetCache((file) => {
				Util.DeleteFile(file.FullName);
				Util.DeleteFile(file.FullName + ".meta");
			});
			var dirs = Util.GetFoldersIn(BehaviourAssetRoot, false);
			foreach (var dir in dirs) {
				if (Util.FolderExists(dir.FullName) && Util.GetFileCount(dir.FullName) == 0) {
					Util.DeleteDirectory(dir.FullName);
					Util.DeleteFile(dir.FullName + ".meta");
				}
			}
			JellyConfig.Main.ClearUnusedDataSlot();
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			// Unsed Asset Count
			UnusedAssetCount = 0;
			ForEachUnusedAssetCache((_) => UnusedAssetCount++);
		}


		private void RemoveUnusedDefaultAsset () {
			var files = Util.GetFilesIn(BehaviourDefaultRoot, false, "*.asset");
			// Asset
			foreach (var file in files) {
				try {
					var dAsset = AssetDatabase.LoadAssetAtPath<JellyBehaviour>(EditorUtil.FixedRelativePath(file.FullName));
					if (dAsset != null) {
						string bPath = GetBehaviourAssetPath(dAsset.GetType(), Util.GetNameWithoutExtension(file.Name));
						if (!Util.FileExists(bPath)) {
							Util.DeleteFile(file.FullName);
							Util.DeleteFile(file.FullName + ".meta");
						}
					}
				} catch { }
			}
			// Folder
			var folders = Util.GetFoldersIn(BehaviourDefaultRoot, false);
			foreach (var folder in folders) {
				if (Util.GetFileCount(folder.FullName) == 0) {
					Util.DeleteDirectory(folder.FullName);
					Util.DeleteFile(folder.FullName + ".meta");
				}
			}
			// Final
			AssetDatabase.Refresh();
		}


		private void ForEachUnusedAssetCache (System.Action<FileInfo> action) {
			var usedHash = new HashSet<string>();
			foreach (var pair in BehaviourPool) {
				foreach (var behData in pair.Value) {
					string path = Util.GetParentPath(Util.GetFullPath(GetBehaviourAssetPath(behData.Behaviour.GetType())));
					if (!usedHash.Contains(path)) {
						usedHash.Add(path);
					}
				}
			}
			if (usedHash.Count == 0) { return; }
			var files = Util.GetFilesIn(BehaviourAssetRoot, false, "*.asset");
			foreach (var file in files) {
				if (!usedHash.Contains(Util.GetParentPath(file.FullName))) {
					action(file);
				}
			}
		}


		private void CreateNewBehaviour (string path) {
			if (string.IsNullOrEmpty(path)) { return; }
			string _name = Util.GetNameWithoutExtension(path);
			string _pName = Util.GetNameWithoutExtension(Util.GetParentPath(path));
			// Get Namespace
			string nameSpace = "CustomJellyBehaviour";
			var assemblyPath = UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFilePathFromScriptPath(path);
			if (!string.IsNullOrEmpty(assemblyPath) && Util.FileExists(assemblyPath)) {
				var json = JsonUtility.FromJson<AssemblyJson>(Util.FileToText(assemblyPath));
				if (json != null) {
					nameSpace = json.rootNamespace;
				}
			}
			// Build Text
			var builder = new StringBuilder();

			builder.AppendLine("using System.Collections;");
			builder.AppendLine("using System.Collections.Generic;");
			builder.AppendLine("using UnityEngine;");
			builder.AppendLine("using PixelJelly;");
			builder.AppendLine();
			builder.AppendLine($"namespace {nameSpace} {{");
			builder.AppendLine($"\tpublic class {_name} : JellyBehaviour {{");
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("\t\t// VAR");
			builder.AppendLine($"\t\tpublic override string DisplayName => \"{_name}\";");
			builder.AppendLine($"\t\tpublic override string DisplayGroup => \"{_pName}\";");
			builder.AppendLine("\t\tpublic override int MaxFrameCount => 24;");
			builder.AppendLine("\t\tpublic override int MaxSpriteCount => 64;");
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("\t\t// MSG");
			builder.AppendLine("\t\tprotected override void OnCreated () {");
			builder.AppendLine("\t\t\tWidth = 32;");
			builder.AppendLine("\t\t\tHeight = 32;");
			builder.AppendLine("\t\t\tFrameCount = 1;");
			builder.AppendLine("\t\t\tSpriteCount = 12;");
			builder.AppendLine("\t\t}");
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("\t\tprotected override void BeforePopulateAllPixels () {");
			builder.AppendLine();
			builder.AppendLine("\t\t}");
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("\t\tprotected override void OnPopulatePixels (int width, int height, int frame, int frameCount, int sprite, int spriteCount, out Vector2 pivot, out RectOffset border) {");
			builder.AppendLine("\t\t\tpivot = new Vector2Int(width / 2, 0);");
			builder.AppendLine("\t\t\tborder = new RectOffset(0, 0, 0, 0);");
			builder.AppendLine();
			builder.AppendLine("\t\t\t///////////// Do Your Magic Here /////////////");
			builder.AppendLine();
			builder.AppendLine("\t\t}");
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("\t\tprotected override void AfterPopulateAllPixels () {");
			builder.AppendLine();
			builder.AppendLine("\t\t}");
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("\t}"); // Class End
			builder.AppendLine("}");

			Util.TextToFile(builder.ToString(), path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		private void CreateNewInspector (string path) {
			if (string.IsNullOrEmpty(path)) { return; }
			string _name = Util.GetNameWithoutExtension(path);
			string _behName = "";
			string _behNamespace = "";
			var sBeh = GetSelectingBehaviour();
			if (sBeh != null && sBeh.Behaviour != null) {
				var bType = sBeh.Behaviour.GetType();
				_behName = bType.Name;
				_behNamespace = bType.Namespace;
			}
			// Get Namespace
			string nameSpace = "CustomJellyInspector";
			var assemblyPath = UnityEditor.Compilation.CompilationPipeline.GetAssemblyDefinitionFilePathFromScriptPath(path);
			if (!string.IsNullOrEmpty(assemblyPath) && Util.FileExists(assemblyPath)) {
				var json = JsonUtility.FromJson<AssemblyJson>(Util.FileToText(assemblyPath));
				if (json != null) {
					nameSpace = json.rootNamespace;
				}
			}
			// Build Text
			var builder = new StringBuilder();
			builder.AppendLine("using System.Collections;");
			builder.AppendLine("using System.Collections.Generic;");
			builder.AppendLine("using UnityEngine;");
			builder.AppendLine("using UnityEditor;");
			builder.AppendLine("using PixelJelly;");
			builder.AppendLine("using PixelJelly.Editor;");
			if (!string.IsNullOrEmpty(_behNamespace) && _behNamespace != "PixelJelly") {
				builder.AppendLine($"using {_behNamespace};");
			}
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine($"namespace {nameSpace} {{");
			if (!string.IsNullOrEmpty(_behName)) {
				builder.AppendLine($"\t[CustomJellyInspector(typeof({_behName}))]");
			}
			builder.AppendLine($"\tpublic class {_name} : JellyInspector {{");
			builder.AppendLine();
			builder.AppendLine();
			builder.AppendLine("\t\tpublic override void OnPropertySwape (SerializedObject serializedObject) {");
			builder.AppendLine("\t\t\t//SwapeProperty(\"m_FrameCount\", \"\");");
			builder.AppendLine("\t\t\t//SwapeProperty(\"m_FrameDuration\", \"\");");
			builder.AppendLine("\t\t}");
			builder.AppendLine();
			builder.AppendLine();

			builder.AppendLine("\t}"); // Class End
			if (!string.IsNullOrEmpty(nameSpace)) {
				builder.AppendLine("}");
			}
			Util.TextToFile(builder.ToString(), path);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}


		private BehaviourSerializeData GetBehaviourFromPool (string group, int index) {
			if (BehaviourPool.ContainsKey(group)) {
				var pool = BehaviourPool[group];
				if (index >= 0 && index < pool.Count) {
					return pool[index];
				}
			}
			return null;
		}


		private BehaviourSerializeData GetBehaviourFromPool (string assembly, string nameSpace, string type, out string group, out int poolIndex) {
			if (BehaviourPathPool.ContainsKey((assembly, nameSpace, type))) {
				var value = BehaviourPathPool[(assembly, nameSpace, type)];
				group = value.group;
				poolIndex = value.poolIndex;
				return value.data;
			}
			group = "";
			poolIndex = 0;
			return null;
		}


		// Animation
		private void PlayAnimation () {
			if (ColorBlock.colors.GetLength(0) > 1) {
				IsPlayingAnimation = true;
				PrevFrameTime = EditorApplication.timeSinceStartup;
			} else {
				IsPlayingAnimation = false;
			}
		}


		private void PauseAnimation (BehaviourSerializeData beh) {
			IsPlayingAnimation = false;
			if (beh != null && beh.OneShotAnimation) {
				SeekFrame(0, beh.Behaviour.FrameCount);
			}
		}


		private void SeekFrame (int frame, int frameCount) => CurrentFrame = Mathf.RoundToInt(Mathf.Repeat(frame, frameCount - 0.00001f));


		// Assets
		private string GetBehaviourAssetPath (System.Type behaviorType, string slot = "") => GetAssetPathLogic(behaviorType, BehaviourAssetRoot, slot);
		private string GetBehaviourDefaultPath (System.Type behaviorType, string slot = "") => GetAssetPathLogic(behaviorType, BehaviourDefaultRoot, slot);
		private string GetAssetPathLogic (System.Type behaviorType, string root, string slot = "") {
			string typeName = Util.SanitizeFileName(behaviorType.Name);
			string nameSpace = !string.IsNullOrEmpty(behaviorType.Namespace) ?
				Util.SanitizeFileName(behaviorType.Namespace) : "Default";
			string assemblyName = Util.SanitizeFileName(behaviorType.Assembly.GetName().Name);
			if (string.IsNullOrEmpty(slot)) {
				slot = JellyConfig.Main.GetDataSlot(behaviorType);
			}
			return Util.CombinePaths(root, assemblyName, nameSpace, typeName, slot + ".asset");
		}


		private JellyBehaviour GetRefreshedAsset (System.Type type, string slot = "") {
			string path = GetBehaviourAssetPath(type, slot);
			if (Util.FileExists(path)) {
				//Get
				var pBehaviour = AssetDatabase.LoadAssetAtPath<JellyBehaviour>(path);
				Util.InvokeMethod(pBehaviour, "OnLoaded");
				return pBehaviour;
			} else {
				// Create
				var pBehaviour = CreateInstance(type) as JellyBehaviour;
				Util.InvokeMethod(pBehaviour, "OnCreated");
				Util.InvokeMethod(pBehaviour, "OnLoaded");
				Util.CreateFolder(Util.GetParentPath(path));
				AssetDatabase.CreateAsset(pBehaviour, path);
				return pBehaviour;
			}
		}


		private void ValidateBehaviourAssets () {
			foreach (var pair in BehaviourPool) {
				if (pair.Value != null) {
					for (int i = 0; i < pair.Value.Count; i++) {
						var bData = pair.Value[i];
						if (bData.Behaviour == null) {
							var pBehaviour = GetRefreshedAsset(bData.Type);
							var newBehaviourData = new BehaviourSerializeData(pBehaviour);
							if (m_SelectingGroup == pair.Key && m_SelectingBehIndex == i) {
								SetSelectingBehaviour(m_SelectingGroup, m_SelectingBehIndex);
							}
							pair.Value[i] = newBehaviourData;
						}
					}
				}
			}
		}


		// Selection
		private BehaviourSerializeData GetSelectingBehaviour () => GetBehaviourFromPool(m_SelectingGroup, m_SelectingBehIndex);


		private void SetSelectingBehaviour (string group, int index) {
			GUI.FocusControl("");
			var beh = GetBehaviourFromPool(group, index);
			if (beh == null) {
				SelectingBehaviourName.Value = "";
				ResizeColorBlock(0, 0, 0, 0);
				return;
			}
			m_SelectingGroup = group;
			m_SelectingBehIndex = index;
			PauseAnimation(beh);
			var behaviour = beh.Behaviour;
			CanvasDirty = true;
			ResizeColorBlock(behaviour.Width, behaviour.Height, behaviour.FrameCount, behaviour.SpriteCount);
			// Set Selecting Name
			var type = behaviour.GetType();
			SelectingBehaviourName.Value = $"{type.Assembly.GetName().Name}/{type.Name}";
			// Slot Cache
			RefreshSlotCache(type);
		}


		// Color Block
		private void ResizeColorBlock (int width, int height, int frameCount, int spriteCount) {
			int len = Mathf.Max(width * height, 0);
			ColorBlock.colors = new Color32[frameCount, spriteCount][];
			ColorBlock.width = width;
			ColorBlock.height = height;
			Comments = new List<CommentData>[frameCount, spriteCount];
			JellyBehaviour.Messages.Clear();
			for (int frame = 0; frame < frameCount; frame++) {
				for (int sprite = 0; sprite < spriteCount; sprite++) {
					Comments[frame, sprite] = new List<CommentData>();
				}
			}
			var CLEAR = new Color32(0, 0, 0, 0);
			for (int frame = 0; frame < frameCount; frame++) {
				for (int sprite = 0; sprite < spriteCount; sprite++) {
					ColorBlock.colors[frame, sprite] = new Color32[len];
					for (int i = 0; i < len; i++) {
						ColorBlock.colors[frame, sprite][i] = CLEAR;
					}
				}
			}
		}


		// Opening
		private bool GetBehaviourFolding (string key, bool defaultValue = true) => BehaviourFoldMap.ContainsKey(key) ? BehaviourFoldMap[key] : defaultValue;


		private void SetBehaviourFolding (string key, bool value) {
			if (BehaviourFoldMap.ContainsKey(key)) {
				BehaviourFoldMap[key] = value;
			} else {
				BehaviourFoldMap.Add(key, value);
			}
			// Save Opening Beh Name
			var builder = new StringBuilder();
			builder.AppendLine("Favorite");
			builder.AppendLine(GetBehaviourFolding("Favorite") ? "1" : "0");
			foreach (var _pair in BehaviourPool) {
				if (string.IsNullOrEmpty(_pair.Key)) { continue; }
				builder.AppendLine(_pair.Key);
				builder.AppendLine(GetBehaviourFolding(_pair.Key) ? "1" : "0");
			}
			OpeningConfig.Value = builder.ToString();
		}


		// Misc
		private static PixelJellyWindow GetWindowIfHasOpenInstances () {
			if (!HasOpenInstances<PixelJellyWindow>()) { return null; }
			var window = GetWindow<PixelJellyWindow>(JellyConfig.TITLE, false);
			return window;
		}


		private void BehaviourMenu (BehaviourSerializeData bData) {
			if (bData == null || bData.Behaviour == null) { return; }
			var menu = new GenericMenu();
			menu.AddItem(
				new GUIContent($"Edit \"{bData.Behaviour.FinalDisplayName}\""), false, () => {
					var script = MonoScript.FromScriptableObject(bData.Behaviour);
					if (script != null) {
						AssetDatabase.OpenAsset(script, 0);
					}
				}
			);
			menu.AddItem(
				new GUIContent(bData.Favorite ? "Unfavorite" : "Favorite"), false, () => {
					var type = bData.Behaviour.GetType();
					if (bData.Favorite) {
						JellyConfig.Main.RemoveFavoriteBeh(
							type.Assembly.GetName().Name, type.Namespace, type.Name
						);
					} else {
						JellyConfig.Main.AddFavoriteBeh(
							type.Assembly.GetName().Name, type.Namespace, type.Name, bData.Group
						);
					}
					RefreshFavoriteForPool();
					JellyConfig.Main.SaveChanges();
				}
			);
			menu.ShowAsContext();
		}


		private void SlotMenu (JellyBehaviour behaviour) {
			if (behaviour == null) { return; }
			var type = behaviour.GetType();
			string slot = JellyConfig.Main.GetDataSlot(type);
			var files = GetSlotFiles(type);
			// Menu
			var menu = new GenericMenu();
			int index = 0;
			int fileCount = files.Length;
			foreach (var file in files) {
				bool conflict = false;
				string name = Util.GetNameWithoutExtension(file.Name);
				if (name == "Create" || name == "Delete" || name == "Rename") {
					conflict = true;
				}
				menu.AddItem(
					new GUIContent(conflict ? name + "|" : name),
					name == slot,
					() => SetDataSlot(m_SelectingGroup, m_SelectingBehIndex, name)
				);
				index++;
			}
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Create"), false, () => {
				string newBasicName = "New Slot";
				string newName = "New Slot";
				string newPath = GetBehaviourAssetPath(type, newName);
				for (int i = 1; Util.FileExists(newPath); i++) {
					newPath = GetBehaviourAssetPath(type, $"{newBasicName} ({i})");
				}
				SlotFieldWindow.Open(
					"Create New Data", "Create", newName,
					Util.GetParentPath(GetBehaviourAssetPath(type, slot)),
					(_newSlot) => {
						if (NewDataSlot(type, _newSlot)) {
							SetDataSlot(m_SelectingGroup, m_SelectingBehIndex, _newSlot);
							RefreshSlotCache(type);
							return true;
						}
						return false;
					}
				);
			});
			menu.AddItem(new GUIContent("Rename"), false, () => SlotFieldWindow.Open(
				"Rename Data", "Rename", slot,
				Util.GetParentPath(GetBehaviourAssetPath(type, slot)),
				(_newSlot) => {
					if (RenameSlot(type, slot, _newSlot)) {
						SetDataSlot(m_SelectingGroup, m_SelectingBehIndex, _newSlot);
						RefreshSlotCache(type);
						return true;
					}
					return false;
				}
			));
			if (fileCount > 1) {
				menu.AddItem(new GUIContent("Delete"), false, () => {
					if (!EditorUtil.Dialog("Confirm", $"Delete data \"{slot}\"?", "Delete", "Cancel")) { return; }
					foreach (var file in files) {
						string name = Util.GetNameWithoutExtension(file.Name);
						if (name != slot) {
							SetDataSlot(m_SelectingGroup, m_SelectingBehIndex, name);
							break;
						}
					}
					DeleteDataSlot(type, slot);
					RefreshSlotCache(type);
				});
			} else {
				menu.AddDisabledItem(new GUIContent("Delete"), false);
			}
			menu.ShowAsContext();
		}


		private void ColorLineGUI (int width, int height, float alpha = 0.2f) => EditorGUI.DrawRect(
			Layout.Rect(width, height),
			EditorGUIUtility.isProSkin ? new Color(0f, 0f, 0f, alpha) : new Color(1f, 1f, 1f, alpha)
		);


		private void GUITextureBack (Rect rect, int targetWidth, int targetHeight, Color bgColor) {
			EditorGUI.DrawRect(rect, bgColor);
			// Checker Floor
			if (DrawCheckerFloor.Value) {
				if (CheckerFloorTexture == null || CheckerFloorTexture.width != targetWidth || CheckerFloorTexture.height != targetHeight) {
					CheckerFloorTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32, false) {
						filterMode = FilterMode.Point,
						alphaIsTransparency = true,
					};
					var COLOR0 = EditorGUIUtility.isProSkin ? new Color32(71, 71, 71, 255) : new Color32(203, 203, 203, 255);
					var COLOR1 = EditorGUIUtility.isProSkin ? new Color32(102, 102, 102, 255) : new Color32(255, 255, 255, 255);
					var pixels = new Color32[targetWidth * targetHeight];
					for (int j = 0; j < targetHeight; j++) {
						for (int i = 0; i < targetWidth; i++) {
							pixels[j * targetWidth + i] = (i + j) % 2 == 0 ? COLOR0 : COLOR1;
						}
					}
					CheckerFloorTexture.SetPixels32(pixels);
					CheckerFloorTexture.Apply();
				}
				GUI.DrawTexture(rect, CheckerFloorTexture, ScaleMode.StretchToFill);
			}
		}


		private void GUIComments (Rect rect, int width, int height, List<CommentData> comments) {
			if (!ShowComment.Value || comments == null || comments.Count == 0 || width <= 0 || height <= 0) { return; }
			var oldC = GUI.color;
			var oldB = GUI.backgroundColor;
			foreach (var comment in comments) {
				if (string.IsNullOrEmpty(comment.Content)) { continue; }
				var con = new GUIContent(comment.Content);
				var size = CommentLabelStyle.CalcSize(con);
				var _rect = new Rect(
					Util.RemapUnclamped(0, width, rect.x, rect.x + rect.width, comment.X),
					Util.RemapUnclamped(height, 0, rect.y, rect.y + rect.height, comment.Y) - size.y,
					size.x, size.y
				);
				GUI.color = comment.FontColor;
				GUI.backgroundColor = comment.BackColor;
				GUI.Label(_rect, comment.Content, CommentLabelStyle);
			}
			GUI.color = oldC;
			GUI.backgroundColor = oldB;
		}


		private void RandomSeed (BehaviourSerializeData behaviour) {
			if (behaviour == null || behaviour.SerializedObject == null || behaviour.SeedProperty == null) { return; }
			behaviour.SerializedObject.Update();
			behaviour.SeedProperty.intValue = new System.Random().Next(int.MinValue, int.MaxValue);
			behaviour.SerializedObject.ApplyModifiedProperties();
			CanvasDirty = true;
			EditorUtility.SetDirty(behaviour.Behaviour);
			EditorSceneManager.MarkAllScenesDirty();
		}


		private void RefreshFavoriteForPool () {
			foreach (var pair in BehaviourPathPool) {
				var bData = pair.Value.data;
				bData.Favorite = false;
			}
			var enu = JellyConfig.Main.GetFavoriteEnumerator();
			while (enu.MoveNext()) {
				var pathData = enu.Current;
				var bData = GetBehaviourFromPool(pathData.Assembly, pathData.Namespace, pathData.Type, out _, out _);
				bData.Favorite = true;
			}
		}


		// Slot
		private void SetDataSlot (string behGroup, int behIndex, string slot) {
			Undo.RecordObjects(new Object[] { JellyConfig.Main, this }, "Set Slot");
			var sBeh = GetBehaviourFromPool(behGroup, behIndex);
			JellyConfig.Main.SetDataSlot(sBeh.Type, slot);
			JellyConfig.Main.SaveChanges();
			sBeh.Load(GetRefreshedAsset(sBeh.Type));
			SetSelectingBehaviour(behGroup, behIndex);
		}


		private void SwitchDataSlot (bool next) {
			var sbeh = GetSelectingBehaviour();
			if (sbeh == null) { return; }
			var files = GetSlotFiles(sbeh.Type);
			string slot = JellyConfig.Main.GetDataSlot(sbeh.Type);
			int index = -1;
			for (int i = 0; i < files.Length; i++) {
				if (Util.GetNameWithoutExtension(files[i].Name) == slot) {
					index = i;
					break;
				}
			}
			if (index >= 0) {
				int targetIndex = Mathf.Clamp(next ? index + 1 : index - 1, 0, files.Length - 1);
				if (targetIndex != index) {
					SetDataSlot(
						m_SelectingGroup,
						m_SelectingBehIndex,
						Util.GetNameWithoutExtension(files[targetIndex].Name)
					);
				}
			}
		}


		private void DeleteDataSlot (System.Type type, string slot) {
			if (string.IsNullOrEmpty(slot)) { return; }
			string path = GetBehaviourAssetPath(type, slot);
			if (Util.FileExists(path)) {
				AssetDatabase.DeleteAsset(path);
				AssetDatabase.Refresh();
			}
			string dPath = GetBehaviourDefaultPath(type, slot);
			if (Util.FileExists(dPath)) {
				AssetDatabase.DeleteAsset(dPath);
				AssetDatabase.Refresh();
			}
		}


		private bool NewDataSlot (System.Type type, string slot) {
			string newSlotPath = GetBehaviourAssetPath(type, slot);
			if (Util.FileExists(newSlotPath)) { return false; }
			GetRefreshedAsset(type, slot);
			return true;
		}


		private bool RenameSlot (System.Type type, string oldSlot, string newSlot) {
			if (oldSlot == newSlot) { return false; }
			// Slot
			string oldSlotPath = GetBehaviourAssetPath(type, oldSlot);
			string newSlotPath = GetBehaviourAssetPath(type, newSlot);
			if (Util.FileExists(newSlotPath) || !Util.FileExists(oldSlotPath)) { return false; }
			Util.MoveFile(oldSlotPath, newSlotPath);
			if (Util.FileExists(oldSlotPath + ".meta")) {
				Util.MoveFile(oldSlotPath + ".meta", newSlotPath + ".meta");
			}
			// Default Asset
			string oldDefaultAssetPath = GetBehaviourDefaultPath(type, oldSlot);
			string newDefaultAssetPath = GetBehaviourDefaultPath(type, newSlot);
			Util.DeleteFile(newDefaultAssetPath);
			Util.DeleteFile(newDefaultAssetPath + ".meta");
			if (Util.FileExists(oldDefaultAssetPath)) {
				Util.MoveFile(oldDefaultAssetPath, newDefaultAssetPath);
				Util.MoveFile(oldDefaultAssetPath + ".meta", newDefaultAssetPath + ".meta");
			}
			// Final
			AssetDatabase.Refresh();
			AssetDatabase.SaveAssets();
			return true;
		}


		private void RefreshSlotCache (System.Type type) {
			SelectingSlotCount = Util.GetFileCount(Util.GetParentPath(GetBehaviourAssetPath(type)), "*.asset", SearchOption.TopDirectoryOnly);
			if (SelectingSlotCount <= 1) {
				//SelectingSlotIndex = 0;
			} else {
				var files = GetSlotFiles(type);
				for (int i = 0; i < files.Length; i++) {
					if (Util.GetNameWithoutExtension(files[i].Name) == JellyConfig.Main.GetDataSlot(type)) {
						//SelectingSlotIndex = i;
						break;
					}
				}
			}
		}


		private FileInfo[] GetSlotFiles (System.Type type) => Util.GetFilesIn(
			Util.GetParentPath(GetBehaviourAssetPath(type)),
			true,
			"*.asset"
		);


	}
}
