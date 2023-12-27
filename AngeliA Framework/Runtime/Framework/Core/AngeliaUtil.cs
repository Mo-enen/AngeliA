using System.Collections;
using System.Collections.Generic;
using System.Text;


namespace AngeliaFramework {


	public static class AngeUtil {


		// Const
		private const string RULE_TILE_ERROR = "ERROR---";
		private static readonly System.Random GlobalRandom = new(2334768);


		// Sheet
		public static SpriteSheet CreateSpriteSheet (int textureWidth, int textureHeight, FlexSprite[] flexSprites) {

			if (textureWidth == 0 || textureHeight == 0) return null;

			var sheet = new SpriteSheet {
				Sprites = null,
				SpriteChains = null,
			};
			var spriteIDHash = new HashSet<int>();
			var chainPool = new Dictionary<string, (GroupType type, List<(int globalIndex, int localIndex, bool loopStart)> list)>();
			var groupHash = new HashSet<string>();
			var spriteList = new List<AngeSprite>();
			var metaList = new List<SpriteMeta>();
			var sheetNames = new List<string>();
			var sheetNamePool = new Dictionary<string, int>();
			for (int i = 0; i < flexSprites.Length; i++) {
				var flex = flexSprites[i];
				var uvBorder = flex.Border;
				uvBorder.x /= flex.Rect.width;
				uvBorder.y /= flex.Rect.height;
				uvBorder.z /= flex.Rect.width;
				uvBorder.w /= flex.Rect.height;
				string realName = GetBlockHashTags(
					flex.Name, out var groupType,
					out bool isTrigger, out int tag, out bool loopStart,
					out int rule, out bool noCollider, out int offsetZ,
					out int? pivotX, out int? pivotY
				);
				int globalWidth = flex.Rect.width.RoundToInt() * Const.CEL / Const.ART_CEL;
				int globalHeight = flex.Rect.height.RoundToInt() * Const.CEL / Const.ART_CEL;
				var globalBorder = new Int4() {
					left = Util.Clamp((int)(flex.Border.x * Const.CEL / Const.ART_CEL), 0, globalWidth),
					down = Util.Clamp((int)(flex.Border.y * Const.CEL / Const.ART_CEL), 0, globalHeight),
					right = Util.Clamp((int)(flex.Border.z * Const.CEL / Const.ART_CEL), 0, globalWidth),
					up = Util.Clamp((int)(flex.Border.w * Const.CEL / Const.ART_CEL), 0, globalHeight),
				};
				if (noCollider) {
					globalBorder.left = globalWidth;
					globalBorder.right = globalWidth;
				}
				int globalID = realName.AngeHash();

				if (!sheetNamePool.TryGetValue(flex.SheetName, out int sheetNameIndex)) {
					sheetNameIndex = sheetNames.Count;
					sheetNamePool.Add(flex.SheetName, sheetNameIndex);
					sheetNames.Add(flex.SheetName);
				}

				var newSprite = new AngeSprite() {
					GlobalID = globalID,
					UvBottomLeft = new(flex.Rect.xMin / textureWidth, flex.Rect.yMin / textureHeight),
					UvTopRight = new(flex.Rect.xMax / textureWidth, flex.Rect.yMax / textureHeight),
					GlobalWidth = globalWidth,
					GlobalHeight = globalHeight,
					UvBorder = uvBorder,// ldru
					GlobalBorder = globalBorder,
					MetaIndex = -1,
					SortingZ = flex.SheetZ * 1024 + offsetZ,
					PivotX = pivotX ?? flex.AngePivot.x,
					PivotY = pivotY ?? flex.AngePivot.y,
					RealName = GetBlockRealName(flex.Name),
					GroupType = groupType,
					SheetType = flex.SheetType,
					SheetNameIndex = sheetNameIndex,
				};

				bool isOneway = IsOnewayTag(tag);
				if (isOneway) isTrigger = true;
				spriteIDHash.TryAdd(newSprite.GlobalID);
				spriteList.Add(newSprite);

				var meta = new SpriteMeta() {
					Tag = tag,
					Rule = rule,
					IsTrigger = isTrigger,
				};

				// Has Meta
				if (
					meta.Tag != 0 ||
					meta.Rule != 0 ||
					meta.IsTrigger ||
					flex.SheetType != SheetType.General
				) {
					newSprite.MetaIndex = metaList.Count;
					metaList.Add(meta);
				}

				// Group
				if (
					!groupHash.Contains(realName) &&
					!string.IsNullOrEmpty(realName) &&
					realName[^1] >= '0' && realName[^1] <= '9'
				) {
					groupHash.TryAdd(realName.TrimEnd_NumbersEmpty());
				}

				// Chain
				if (groupType != GroupType.General) {
					string key = realName;
					int endIndex = key.Length - 1;
					while (endIndex >= 0) {
						char c = key[endIndex];
						if (c < '0' || c > '9') break;
						endIndex--;
					}
					key = key[..(endIndex + 1)].TrimEnd(' ');
					int _index = endIndex < realName.Length - 1 ? int.Parse(realName[(endIndex + 1)..]) : 0;
					if (!chainPool.ContainsKey(key)) chainPool.Add(key, (groupType, new()));
					chainPool[key].list.Add((spriteList.Count - 1, _index, loopStart));
				}

			}
			sheet.SheetNames = sheetNames.ToArray();

			// Load Groups
			var groups = new List<SpriteGroup>();
			foreach (var gName in groupHash) {
				var sprites = new List<int>();
				for (int i = 0; ; i++) {
					int id = $"{gName} {i}".AngeHash();
					if (spriteIDHash.Contains(id)) {
						sprites.Add(id);
					} else break;
				}
				if (sprites.Count > 0) {
					groups.Add(new SpriteGroup() {
						ID = gName.AngeHash(),
						SpriteIDs = sprites.ToArray(),
					});
				}
			}
			sheet.Groups = groups.ToArray();

			// Sort Chain
			foreach (var (_, (_, list)) in chainPool) list.Sort((a, b) => a.localIndex.CompareTo(b.localIndex));

			// Fix Duration
			foreach (var (name, (gType, list)) in chainPool) {
				if (list.Count <= 1) continue;
				if (gType != GroupType.Animated) continue;
				for (int i = 0; i < list.Count - 1; i++) {
					var item = list[i];
					var (_, nextLocal, _) = list[i + 1];
					if (nextLocal > item.localIndex + 1) {
						int _index = i;
						for (int j = 0; j < nextLocal - item.localIndex - 1; j++) {
							list.Insert(_index, item);
							i++;
						}
					}
				}
			}

			// Final
			var sChain = new AngeSpriteChain[chainPool.Count];
			int index = 0;
			foreach (var pair in chainPool) {
				var chain = sChain[index] = new AngeSpriteChain() {
					ID = pair.Key.AngeHash(),
					Type = pair.Value.type,
					Name = pair.Key,
					LoopStart = -1,
				};
				var result = chain.Chain = new List<int>();
				foreach (var (globalIndex, _, _loopStart) in pair.Value.list) {
					if (_loopStart && chain.LoopStart < 0) {
						chain.LoopStart = result.Count;
					}
					result.Add(globalIndex);
				}
				if (chain.LoopStart < 0) chain.LoopStart = 0;
				index++;
			}
			sheet.Sprites = spriteList.ToArray();
			sheet.SpriteChains = sChain;
			sheet.Metas = metaList.ToArray();
			return sheet;
		}


		public static void FillSummaryForSheet (SpriteSheet sheet, int textureWidth, int textureHeight, Byte4[] pixels) {

			if (sheet == null) return;

			// Color Pool
			var pool = new Dictionary<int, Byte4>();
			for (int i = 0; i < sheet.Sprites.Length; i++) {
				var sp = sheet.Sprites[i];
				if (pool.ContainsKey(sp.GlobalID)) continue;
				var color = GetThumbnailColor(
					pixels, textureWidth, sp.GetTextureRect(textureWidth, textureHeight)
				);
				if (color.IsSame(Const.CLEAR)) continue;
				pool.Add(sp.GlobalID, color);
			}
			foreach (var chain in sheet.SpriteChains) {
				switch (chain.Type) {
					case GroupType.Animated:
						if (pool.ContainsKey(chain.ID)) continue;
						if (chain.Chain != null && chain.Chain.Count > 0) {
							int index = chain.Chain[0];
							if (index < 0 || index >= sheet.Sprites.Length) break;
							if (pool.TryGetValue(sheet.Sprites[index].GlobalID, out var _color)) {
								pool.Add(chain.ID, _color);
							}
						}
						break;
					case GroupType.General:
					case GroupType.Rule:
					case GroupType.Random: break;
					default: throw new System.NotImplementedException();
				}
			}

			// Set Values
			for (int i = 0; i < sheet.Sprites.Length; i++) {
				var sprite = sheet.Sprites[i];
				sprite.SummaryTint = pool.TryGetValue(sprite.GlobalID, out var color) ? color : default;
			}

			// Func
			static Byte4 GetThumbnailColor (Byte4[] pixels, int width, IRect rect) {
				var CLEAR = new Byte4(0, 0, 0, 0);
				if (rect.width <= 0 || rect.height <= 0) return CLEAR;
				var result = CLEAR;
				try {
					var sum = Float3.zero;
					float len = 0;
					int l = rect.x;
					int r = rect.xMax;
					int d = rect.y;
					int u = rect.yMax;
					for (int x = l; x < r; x++) {
						for (int y = d; y < u; y++) {
							var pixel = pixels[y * width + x];
							if (pixel.a != 0) {
								sum.x += pixel.r / 255f;
								sum.y += pixel.g / 255f;
								sum.z += pixel.b / 255f;
								len++;
							}
						}
					}
					return new Byte4((byte)(sum.x * 255f / len), (byte)(sum.y * 255f / len), (byte)(sum.z * 255f / len), 255);
				} catch (System.Exception ex) { Game.LogException(ex); }
				return result;
			}
		}


		public static string GetBlockRealName (string name) {
			int hashIndex = name.IndexOf('#');
			if (hashIndex >= 0) {
				name = name[..hashIndex];
			}
			return name.TrimEnd(' ');
		}


		private static string GetBlockHashTags (
			string name, out GroupType groupType,
			out bool isTrigger, out int tag, out bool loopStart,
			out int rule, out bool noCollider, out int offsetZ,
			out int? pivotX, out int? pivotY
		) {
			isTrigger = false;
			tag = 0;
			rule = 0;
			loopStart = false;
			noCollider = false;
			offsetZ = 0;
			pivotX = null;
			pivotY = null;
			groupType = GroupType.General;
			const System.StringComparison OIC = System.StringComparison.OrdinalIgnoreCase;
			int hashIndex = name.IndexOf('#');
			if (hashIndex >= 0) {
				var hashs = name[hashIndex..].Replace(" ", "").Split('#');
				foreach (var hashTag in hashs) {

					if (string.IsNullOrWhiteSpace(hashTag)) continue;

					// Bool
					if (hashTag.Equals("isTrigger", OIC)) {
						isTrigger = true;
						continue;
					}

					// Tag
					if (hashTag.Equals("OnewayUp", OIC)) { tag = Const.ONEWAY_UP_TAG; continue; }
					if (hashTag.Equals("OnewayDown", OIC)) { tag = Const.ONEWAY_DOWN_TAG; continue; }
					if (hashTag.Equals("OnewayLeft", OIC)) { tag = Const.ONEWAY_LEFT_TAG; continue; }
					if (hashTag.Equals("OnewayRight", OIC)) { tag = Const.ONEWAY_RIGHT_TAG; continue; }
					if (hashTag.Equals("Climb", OIC)) { tag = Const.CLIMB_TAG; continue; }
					if (hashTag.Equals("ClimbStable", OIC)) { tag = Const.CLIMB_STABLE_TAG; continue; }
					if (hashTag.Equals("Quicksand", OIC)) { tag = Const.QUICKSAND_TAG; isTrigger = true; continue; }
					if (hashTag.Equals("Water", OIC)) { tag = Const.WATER_TAG; isTrigger = true; continue; }
					if (hashTag.Equals("Slip", OIC)) { tag = Const.SLIP_TAG; continue; }
					if (hashTag.Equals("Slide", OIC)) { tag = Const.SLIDE_TAG; continue; }
					if (hashTag.Equals("NoSlide", OIC)) { tag = Const.NO_SLIDE_TAG; continue; }
					if (hashTag.Equals("GrabTop", OIC)) { tag = Const.GRAB_TOP_TAG; continue; }
					if (hashTag.Equals("GrabSide", OIC)) { tag = Const.GRAB_SIDE_TAG; continue; }
					if (hashTag.Equals("Grab", OIC)) { tag = Const.GRAB_TAG; continue; }
					if (hashTag.Equals("ShowLimb", OIC)) { tag = Const.SHOW_LIMB_TAG; continue; }
					if (hashTag.Equals("HideLimb", OIC)) { tag = Const.HIDE_LIMB_TAG; continue; }
					if (hashTag.Equals("Damage", OIC)) { tag = Const.DAMAGE_TAG; continue; }
					if (hashTag.Equals("ExplosiveDamage", OIC)) { tag = Const.DAMAGE_EXPLOSIVE_TAG; continue; }
					if (hashTag.Equals("MagicalDamage", OIC)) { tag = Const.DAMAGE_MAGICAL_TAG; continue; }

					if (hashTag.Equals("loopStart", OIC)) {
						loopStart = true;
						continue;
					}

					if (hashTag.Equals("noCollider", OIC) || hashTag.Equals("ignoreCollider", OIC)) {
						noCollider = true;
						continue;
					}

					// Bool-Group
					if (hashTag.Equals("animated", OIC) || hashTag.Equals("ani", OIC)) {
						groupType = GroupType.Animated;
						continue;
					}
					if (hashTag.Equals("rule", OIC) || hashTag.Equals("rul", OIC)) {
						groupType = GroupType.Rule;
						continue;
					}
					if (hashTag.Equals("random", OIC) || hashTag.Equals("ran", OIC)) {
						groupType = GroupType.Random;
						continue;
					}

					// Int
					if (hashTag.StartsWith("tag=", OIC)) {
						tag = hashTag[4..].AngeHash();
						continue;
					}

					if (hashTag.StartsWith("rule=", OIC)) {
						rule = RuleStringToDigit(hashTag[5..]);
						groupType = GroupType.Rule;
						continue;
					}

					if (hashTag.StartsWith("z=", OIC)) {
						if (int.TryParse(hashTag[2..], out int _offsetZ)) {
							offsetZ = _offsetZ;
						}
						continue;
					}

					if (hashTag.StartsWith("pivot", OIC)) {

						switch (hashTag) {
							case var _ when hashTag.StartsWith("pivotX=", OIC):
								if (int.TryParse(hashTag[7..], out int _px)) pivotX = _px;
								continue;
							case var _ when hashTag.StartsWith("pivotY=", OIC):
								if (int.TryParse(hashTag[7..], out int _py)) pivotY = _py;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottomLeft", OIC):
								pivotX = 0;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottomRight", OIC):
								pivotX = 1000;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=bottom", OIC):
								pivotX = 500;
								pivotY = 0;
								continue;
							case var _ when hashTag.StartsWith("pivot=topLeft", OIC):
								pivotX = 0;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=topRight", OIC):
								pivotX = 1000;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=top", OIC):
								pivotX = 500;
								pivotY = 1000;
								continue;
							case var _ when hashTag.StartsWith("pivot=left", OIC):
								pivotX = 0;
								pivotY = 500;
								continue;
							case var _ when hashTag.StartsWith("pivot=right", OIC):
								pivotX = 1000;
								pivotY = 500;
								continue;
							case var _ when hashTag.StartsWith("pivot=center", OIC):
							case var _ when hashTag.StartsWith("pivot=mid", OIC):
							case var _ when hashTag.StartsWith("pivot=middle", OIC):
								pivotX = 500;
								pivotY = 500;
								continue;
						}
					}

					Game.LogWarning($"Unknown hash \"{hashTag}\" for {name}");

				}
				// Trim Name
				name = name[..hashIndex];
			}
			return name.TrimEnd(' ');
		}


		// File
		public static void CreateAngeFolders () {
			Util.CreateFolder(AngePath.UniverseRoot);
			Util.CreateFolder(AngePath.SheetRoot);
			Util.CreateFolder(AngePath.DialogueRoot);
			Util.CreateFolder(AngePath.LanguageRoot);
			Util.CreateFolder(AngePath.MetaRoot);
			Util.CreateFolder(AngePath.BuiltInMapRoot);
		}


		[OnSlotCreated]
		public static void CreateSlotFolders () {
			Util.CreateFolder(AngePath.SaveSlotRoot);
			Util.CreateFolder(AngePath.UserMapRoot);
			Util.CreateFolder(AngePath.UserDataRoot);
			Util.CreateFolder(AngePath.ProcedureMapRoot);
			Util.CreateFolder(AngePath.DownloadMapRoot);
		}


		// API
		public static int GetOnewayTag (Direction4 gateDirection) =>
		gateDirection switch {
			Direction4.Down => Const.ONEWAY_DOWN_TAG,
			Direction4.Up => Const.ONEWAY_UP_TAG,
			Direction4.Left => Const.ONEWAY_LEFT_TAG,
			Direction4.Right => Const.ONEWAY_RIGHT_TAG,
			_ => Const.ONEWAY_UP_TAG,
		};


		public static bool IsOnewayTag (int tag) => tag == Const.ONEWAY_UP_TAG || tag == Const.ONEWAY_DOWN_TAG || tag == Const.ONEWAY_LEFT_TAG || tag == Const.ONEWAY_RIGHT_TAG;


		public static bool TryGetOnewayDirection (int tag, out Direction4 direction) {
			if (tag == Const.ONEWAY_LEFT_TAG) {
				direction = Direction4.Left;
				return true;
			}
			if (tag == Const.ONEWAY_RIGHT_TAG) {
				direction = Direction4.Right;
				return true;
			}
			if (tag == Const.ONEWAY_DOWN_TAG) {
				direction = Direction4.Down;
				return true;
			}
			if (tag == Const.ONEWAY_UP_TAG) {
				direction = Direction4.Up;
				return true;
			}
			direction = default;
			return false;
		}


		public static void DrawSegmentHealthBar (int x, int y, int heartLeftCode, int heartRightCode, int emptyHeartLeftCode, int emptyHeartRightCode, int dropParticleID, int hp, int maxHP, int prevHP = int.MinValue) {

			const int SIZE = Const.HALF;
			const int COLUMN = 4;
			const int MAX = 8;

			int maxHp = Util.Min(maxHP, MAX);
			int left = x - SIZE * COLUMN / 4;

			// Draw Hearts
			var rect = new IRect(0, 0, SIZE / 2, SIZE);
			bool isLeft = true;
			for (int i = 0; i < maxHp; i++) {
				rect.x = left + (i % COLUMN) * SIZE / 2;
				rect.y = y - (i / COLUMN + 1) * SIZE;
				if (i < hp) {
					// Heart
					CellRenderer.Draw(isLeft ? heartLeftCode : heartRightCode, rect, 0);
				} else {
					// Empty Heart
					CellRenderer.Draw(isLeft ? emptyHeartLeftCode : emptyHeartRightCode, rect, 0);
					// Spawn Drop Particle
					if (i < prevHP) {
						Entity heart;
						if (isLeft) {
							heart = Stage.SpawnEntity(dropParticleID, rect.x, rect.y);
						} else {
							heart = Stage.SpawnEntity(dropParticleID, rect.x, rect.y);
						}
						if (heart != null) {
							heart.Width = rect.width + 8;
							heart.Height = rect.height + 16;
						}
					}
				}
				isLeft = !isLeft;
			}
		}


		public static Int2 GetFlyingFormation (Int2 pos, int column, int instanceIndex) {

			int sign = instanceIndex % 2 == 0 ? -1 : 1;
			int _row = instanceIndex / 2 / column;
			int _column = (instanceIndex / 2 % column + 1) * sign;
			int rowSign = (_row % 2 == 0) == (sign == 1) ? 1 : -1;

			int instanceOffsetX = _column * Const.CEL * 3 / 2 + rowSign * Const.HALF / 2;
			int instanceOffsetY = _row * Const.CEL + Const.CEL - _column.Abs() * Const.HALF / 3;

			return new(pos.x + instanceOffsetX, pos.y + instanceOffsetY);
		}


		public static IEnumerable<KeyValuePair<Int4, string>> ForEachPlayerCustomizeSpritePattern (string[] patterns, string suffix0, string suffix1 = "", string suffix2 = "", string suffix3 = "") {
			if (patterns == null || patterns.Length == 0) yield break;
			foreach (string pat in patterns) {
				if (string.IsNullOrEmpty(pat)) {
					yield return new(new Int4(0, 0, 0, 0), "");
				} else if (pat[0] == '#') {
					yield return new(new Int4(0, 0, 0, 0), pat);
				} else {
					yield return new(new Int4(
						$"{pat}{suffix0}".AngeHash(),
						string.IsNullOrEmpty(suffix1) ? 0 : $"{pat}{suffix1}".AngeHash(),
						string.IsNullOrEmpty(suffix2) ? 0 : $"{pat}{suffix2}".AngeHash(),
						string.IsNullOrEmpty(suffix3) ? 0 : $"{pat}{suffix3}".AngeHash()
					), pat);
				}
			}
		}


		// Random
		public static int RandomInt (int min = int.MinValue, int max = int.MaxValue) => GlobalRandom.Next(min, max);
		public static float RandomFloat01 () => (float)GlobalRandom.NextDouble();
		public static double RandomDouble01 () => GlobalRandom.NextDouble();
		public static Byte4 RandomColor (int minH = 0, int maxH = 360, int minS = 0, int maxS = 100, int minV = 0, int maxV = 100, int minA = 0, int maxA = 255) {
			var result = Util.HsvToRgb(
				RandomInt(minH, maxH) / 360f,
				RandomInt(minS, maxS) / 100f,
				RandomInt(minV, maxV) / 100f
			);
			result.a = (byte)RandomInt(minA, maxA);
			return result;
		}


		// Pose Animation
		public static void LimbRotate (
			ref int targetX, ref int targetY, ref int targetPivotX, ref int targetPivotY, ref int targetRotation, ref int targetWidth, ref int targetHeight,
			int rotation, bool useFlip, int grow
		) {
			targetPivotY = 1000;
			targetWidth = targetWidth.Abs();
			bool bigRot = rotation.Abs() > 90;

			// Rotate
			targetRotation = rotation;
			int newPivotX = rotation > 0 != bigRot ? 1000 : 0;
			if (newPivotX != targetPivotX) {
				targetPivotX = newPivotX;
				targetX += newPivotX == 1000 ? targetWidth : -targetWidth;
			}

			// Flip
			if (targetPivotX > 500) {
				targetPivotX = 1000 - targetPivotX;
				if (bigRot || useFlip) {
					targetWidth = -targetWidth;
				} else {
					targetX -= (int)(Util.Cos(targetRotation * Util.Deg2Rad) * targetWidth);
					targetY += (int)(Util.Sin(targetRotation * Util.Deg2Rad) * targetWidth);
				}
			}

			// Fix for Big Rot
			if (bigRot) {
				targetWidth = -targetWidth;
				targetHeight -= targetWidth.Abs();
			}

			// Grow
			if (rotation != 0 && grow != 0) {
				targetHeight += rotation.Abs() * grow * targetWidth.Abs() / 96000;
			}

		}


		public static void LimbRotate (
			ref int targetX, ref int targetY, ref int targetPivotX, ref int targetPivotY, ref int targetRotation, ref int targetWidth, ref int targetHeight,
			int parentX, int parentY, int parentRotation, int parentWidth, int parentHeight,
			int rotation, bool useFlip, int grow
		) {
			targetPivotY = 1000;
			targetWidth = targetWidth.Abs();
			bool bigRot = rotation.Abs() > 90;

			// Rotate
			targetRotation = parentRotation + rotation;
			targetX = parentX - (int)(Util.Sin(parentRotation * Util.Deg2Rad) * parentHeight);
			targetY = parentY - (int)(Util.Cos(parentRotation * Util.Deg2Rad) * parentHeight);
			targetPivotX = rotation > 0 != bigRot ? 1000 : 0;
			if (parentWidth < 0 != targetPivotX > 500) {
				int pWidth = parentWidth.Abs();
				int sign = targetPivotX > 500 ? -1 : 1;
				targetX -= (int)(Util.Cos(parentRotation * Util.Deg2Rad) * pWidth) * sign;
				targetY += (int)(Util.Sin(parentRotation * Util.Deg2Rad) * pWidth) * sign;
			}

			// Flip
			if (targetPivotX > 500) {
				targetPivotX = 1000 - targetPivotX;
				if (bigRot || useFlip) {
					targetWidth = -targetWidth;
				} else {
					targetX -= (int)(Util.Cos(targetRotation * Util.Deg2Rad) * targetWidth);
					targetY += (int)(Util.Sin(targetRotation * Util.Deg2Rad) * targetWidth);
				}
			}

			// Fix for Big Rot
			if (bigRot) {
				targetWidth = -targetWidth;
				targetHeight -= parentWidth.Abs();
			}

			// Grow
			if (rotation != 0 && grow != 0) {
				targetHeight += rotation.Abs() * grow * targetWidth.Abs() / 96000;
			}

		}


		public static bool DrawPoseCharacterAsUI (IRect rect, PoseCharacter character, int animationFrame, int z, out IRect globalRect, out IRect uiRect) {

			globalRect = default;
			uiRect = default;

			// Draw Player
			int oldLayerIndex = CellRenderer.CurrentLayerIndex;
			CellRenderer.SetLayerToUI();
			int layerIndex = CellRenderer.CurrentLayerIndex;
			int cellIndexStart = CellRenderer.GetUsedCellCount();
			int oldAniFrame = character.CurrentAnimationFrame;
			character.CurrentAnimationFrame = animationFrame;
			character.FrameUpdate();
			character.CurrentAnimationFrame = oldAniFrame;
			int cellIndexEnd = CellRenderer.GetUsedCellCount();
			CellRenderer.SetLayer(oldLayerIndex);
			if (cellIndexStart == cellIndexEnd) return false;
			if (!CellRenderer.GetCells(layerIndex, out var cells, out int count)) return false;

			// Get Min Max
			bool flying = character.AnimationType == CharacterAnimationType.Fly;
			int originalMinX = character.X - Const.HALF - 16;
			int originalMinY = character.Y - 16 + (flying ? character.PoseRootY / 2 : 0);
			int originalMaxX = character.X + Const.HALF + 16;
			int originalMaxY = character.Y + Const.CEL * 2 + 16;
			if (flying) {
				originalMinY -= Const.HALF;
				originalMaxY -= Const.HALF;
			}
			globalRect.SetMinMax(originalMinX, originalMaxX, originalMinY, originalMaxY);

			// Move Cells
			int originalWidth = originalMaxX - originalMinX;
			int originalHeight = originalMaxY - originalMinY;
			var targetRect = uiRect = rect.Fit(originalWidth, originalHeight, 500, 0);
			int minZ = int.MaxValue;
			for (int i = cellIndexStart; i < count && i < cellIndexEnd; i++) {
				var cell = cells[i];
				minZ = Util.Min(minZ, cell.Z);
				cell.X = targetRect.x + (cell.X - originalMinX) * targetRect.width / originalWidth;
				cell.Y = targetRect.y + (cell.Y - originalMinY) * targetRect.height / originalHeight;
				cell.Width = cell.Width * targetRect.width / originalWidth;
				cell.Height = cell.Height * targetRect.height / originalHeight;
				if (!cell.Shift.IsZero) {
					cell.Shift = new Int4(
						cell.Shift.left * targetRect.width / originalWidth,
						cell.Shift.right * targetRect.width / originalWidth,
						cell.Shift.down * targetRect.height / originalHeight,
						cell.Shift.up * targetRect.height / originalHeight
					);
				}
			}

			// Fix Z
			if (minZ != int.MaxValue) {
				for (int i = cellIndexStart; i < count && i < cellIndexEnd; i++) {
					var cell = cells[i];
					cell.Z = z + cell.Z - minZ;
				}
			}

			return true;

		}


		// Drawing
		public static Cell DrawEnvironmentShadow (Cell source, int offsetX = -Const.HALF / 2, int offsetY = 0, byte alpha = 64, int z = -64 * 1024 + 16) {
			var result = CellRenderer.Draw(Const.PIXEL, default);
			result.CopyFrom(source);
			result.X += offsetX;
			result.Y += offsetY;
			result.Z = z;
			result.Color = new Byte4(0, 0, 0, alpha);
			return result;
		}


		public static void DrawGlitchEffect (Cell cell, int frame) {

			if (frame.UMod(0096) == 0) DrawGlitch(cell, RandomInt(-012, 012), RandomInt(-007, 007), RandomInt(0900, 1100), RandomInt(0500, 1100), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0061) == 0) DrawGlitch(cell, RandomInt(-002, 041), RandomInt(-019, 072), RandomInt(0500, 1200), RandomInt(0500, 1200), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0121) == 0) DrawGlitch(cell, RandomInt(-016, 016), RandomInt(-007, 007), RandomInt(0400, 1100), RandomInt(0600, 1100), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0127) == 0) DrawGlitch(cell, RandomInt(-016, 016), RandomInt(-007, 007), RandomInt(0900, 1100), RandomInt(0500, 1300), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0185) == 0) DrawGlitch(cell, RandomInt(-011, 021), RandomInt(-018, 018), RandomInt(0900, 1300), RandomInt(0900, 1300), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0187) == 0) DrawGlitch(cell, RandomInt(-034, 042), RandomInt(-012, 008), RandomInt(0800, 1100), RandomInt(0900, 1400), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0193) == 0) DrawGlitch(cell, RandomInt(-052, 002), RandomInt(-091, 077), RandomInt(0700, 1400), RandomInt(0800, 1100), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(0274) == 0) DrawGlitch(cell, RandomInt(-033, 072), RandomInt(-031, 079), RandomInt(0800, 1100), RandomInt(0900, 1800), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(1846) == 0) DrawGlitch(cell, RandomInt(-094, 012), RandomInt(-077, 112), RandomInt(0900, 1500), RandomInt(0900, 1300), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(3379) == 0) DrawGlitch(cell, RandomInt(-194, 112), RandomInt(-177, 212), RandomInt(0900, 1600), RandomInt(0700, 1900), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));
			if (frame.UMod(9379) == 0) DrawGlitch(cell, RandomInt(-293, 211), RandomInt(-079, 011), RandomInt(0900, 1700), RandomInt(0900, 1100), RandomColor(0, 360, 100, 100, 100, 100, 128, 255));

			// Func
			static void DrawGlitch (Cell cell, int offsetX, int offsetY, int scaleX, int scaleY, Byte4 color) {

				var cursedCell = CellRenderer.Draw(Const.PIXEL, default, 0);
				cursedCell.Index = cell.Index;
				cursedCell.X = cell.X;
				cursedCell.Y = cell.Y;
				cursedCell.Z = cell.Z + 1;
				cursedCell.Rotation = cell.Rotation;
				cursedCell.Width = cell.Width * scaleX / 1000;
				cursedCell.Height = cell.Height * scaleY / 1000;
				cursedCell.PivotX = cell.PivotX;
				cursedCell.PivotY = cell.PivotY;
				cursedCell.Shift = cell.Shift;

				cursedCell.Color = color;
				cursedCell.X += offsetX;
				cursedCell.Y += offsetY;

			}
		}


		// Extension
		public static bool IsHorizontal (this Direction4 dir) => dir == Direction4.Left || dir == Direction4.Right;
		public static bool IsVertical (this Direction4 dir) => dir == Direction4.Down || dir == Direction4.Up;


		public static Direction4 Opposite (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Up,
			Direction4.Up => Direction4.Down,
			Direction4.Left => Direction4.Right,
			Direction4.Right => Direction4.Left,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction3 Opposite (this Direction3 dir) => dir switch {
			Direction3.Down => Direction3.Up,
			Direction3.Up => Direction3.Down,
			Direction3.None => Direction3.None,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction4 Clockwise (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Left,
			Direction4.Left => Direction4.Up,
			Direction4.Up => Direction4.Right,
			Direction4.Right => Direction4.Down,
			_ => throw new System.NotImplementedException(),
		};


		public static Direction4 AntiClockwise (this Direction4 dir) => dir switch {
			Direction4.Down => Direction4.Right,
			Direction4.Right => Direction4.Up,
			Direction4.Up => Direction4.Left,
			Direction4.Left => Direction4.Down,
			_ => throw new System.NotImplementedException(),
		};


		public static Int2 Normal (this Direction4 dir) => dir switch {
			Direction4.Down => new(0, -1),
			Direction4.Up => new(0, 1),
			Direction4.Left => new(-1, 0),
			Direction4.Right => new(1, 0),
			_ => throw new System.NotImplementedException(),
		};


		public static IRect EdgeInside (this IRect rect, Direction4 edge, int thickness = 1) => edge switch {
			Direction4.Up => rect.Shrink(0, 0, rect.height - thickness, 0),
			Direction4.Down => rect.Shrink(0, 0, 0, rect.height - thickness),
			Direction4.Left => rect.Shrink(0, rect.width - thickness, 0, 0),
			Direction4.Right => rect.Shrink(rect.width - thickness, 0, 0, 0),
			_ => throw new System.NotImplementedException(),
		};
		public static IRect EdgeOutside (this IRect rect, Direction4 edge, int thickness = 1) => edge switch {
			Direction4.Up => rect.Shrink(0, 0, rect.height, -thickness),
			Direction4.Down => rect.Shrink(0, 0, -thickness, rect.height),
			Direction4.Left => rect.Shrink(-thickness, rect.width, 0, 0),
			Direction4.Right => rect.Shrink(rect.width, -thickness, 0, 0),
			_ => throw new System.NotImplementedException(),
		};
		public static FRect Edge (this FRect rect, Direction4 edge, float thickness = 1f) => edge switch {
			Direction4.Up => rect.Shrink(0, 0, rect.height, -thickness),
			Direction4.Down => rect.Shrink(0, 0, -thickness, rect.height),
			Direction4.Left => rect.Shrink(-thickness, rect.width, 0, 0),
			Direction4.Right => rect.Shrink(rect.width, -thickness, 0, 0),
			_ => throw new System.NotImplementedException(),
		};


		public static int GetRotation (this Direction4 dir) => dir switch {
			Direction4.Up => 0,
			Direction4.Down => 180,
			Direction4.Left => -90,
			Direction4.Right => 90,
			_ => 0,
		};


		public static int ToUnit (this int globalPos) => globalPos.UDivide(Const.CEL);
		public static int ToGlobal (this int unitPos) => unitPos * Const.CEL;
		public static int ToUnifyGlobal (this int globalPos) => globalPos.UDivide(Const.CEL) * Const.CEL;


		public static Int2 ToUnit (this Int2 globalPos) => globalPos.UDivide(Const.CEL);
		public static Int2 ToGlobal (this Int2 unitPos) => unitPos * Const.CEL;
		public static Int2 ToUnifyGlobal (this Int2 globalPos) => globalPos.ToUnit().ToGlobal();

		public static Int3 ToUnit (this Int3 globalPos) => new(
			globalPos.x.UDivide(Const.CEL),
			globalPos.y.UDivide(Const.CEL),
			globalPos.z
		);
		public static Int3 ToGlobal (this Int3 unitPos) => new(
			unitPos.x * Const.CEL,
			unitPos.y * Const.CEL,
			unitPos.z
		);
		public static Int3 ToUnifyGlobal (this Int3 globalPos) => new(
			globalPos.x.ToUnit().ToGlobal(),
			globalPos.y.ToUnit().ToGlobal(),
			globalPos.z
		);


		public static IRect ToUnit (this IRect global) => global.UDivide(Const.CEL);
		public static IRect ToGlobal (this IRect unit) => new(
			unit.x * Const.CEL,
			unit.y * Const.CEL,
			unit.width * Const.CEL,
			unit.height * Const.CEL
		);
		public static IRect ToUnifyGlobal (this IRect global) => global.ToUnit().ToGlobal();


		// Map Editor
		public static string GetTileRuleString (int groupID) {
			if (!CellRenderer.HasSpriteGroup(groupID, out int groupLength)) return "";
			var builder = new StringBuilder();
			for (int i = 0; i < groupLength; i++) {
				if (CellRenderer.TryGetSpriteFromGroup(groupID, i, out var sp, false, true)) {
					if (CellRenderer.TryGetMeta(sp.GlobalID, out var meta)) {
						int ruleDigit = meta.Rule;
						builder.Append(RuleDigitToString(ruleDigit));
					} else {
						builder.Append(RuleDigitToString(0));
					}
				} else {
					builder.Append(RULE_TILE_ERROR);
				}
			}
			return builder.ToString();
		}


		public static string RuleDigitToString (int digit) {
			// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
			// eg: "02022020,11111111,01022020,02012020,..."
			// digit: int32 10000000 000 000 000 000 000 000 000 000
			//              01234567 890 123 456 789 012 345 678 901
			//                         1             2            3
			if (!digit.GetBit(0)) return RULE_TILE_ERROR;
			var builder = new StringBuilder();
			for (int i = 0; i < 8; i++) {
				int tileStrNumber = 0;
				tileStrNumber += digit.GetBit(8 + i * 3 + 0) ? 4 : 0;
				tileStrNumber += digit.GetBit(8 + i * 3 + 1) ? 2 : 0;
				tileStrNumber += digit.GetBit(8 + i * 3 + 2) ? 1 : 0;
				builder.Append(tileStrNumber);
			}
			return builder.ToString();
		}


		public static int RuleStringToDigit (string str) {
			// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
			//                       000        001        010           011       100
			// eg: "02022020,11111111,01022020,02012020,..."
			// digit: int32 10000000 000 000 000 000 000 000 000 000
			//              01234567 890 123 456 789 012 345 678 901
			//                         1             2            3
			if (string.IsNullOrEmpty(str) || str.Length < 8) return 0;
			int digit = 0;
			digit.SetBit(0, true);
			for (int i = 0; i < 8; i++) {
				char c = str[i];
				if (c < '0' || c > '9') continue;
				int strNumber = str[i] - '0';
				digit.SetBit(8 + i * 3 + 0, (strNumber / 4) % 2 == 1);
				digit.SetBit(8 + i * 3 + 1, (strNumber / 2) % 2 == 1);
				digit.SetBit(8 + i * 3 + 2, (strNumber / 1) % 2 == 1);
			}
			return digit;
		}


		public static int GetRuleIndex (
			string rule, int ruleID,
			int tl0, int tm0, int tr0, int ml0, int mr0, int bl0, int bm0, int br0,
			int tl1, int tm1, int tr1, int ml1, int mr1, int bl1, int bm1, int br1
		) {
			// 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty
			int count = rule.Length / 8;
			for (int i = 0; i < count; i++) {
				char first = rule[i * 8 + 0];
				if (first < '0' || first > '9') continue;
				int _tl = rule[i * 8 + 0] - '0';
				int _tm = rule[i * 8 + 1] - '0';
				int _tr = rule[i * 8 + 2] - '0';
				int _ml = rule[i * 8 + 3] - '0';
				int _mr = rule[i * 8 + 4] - '0';
				int _bl = rule[i * 8 + 5] - '0';
				int _bm = rule[i * 8 + 6] - '0';
				int _br = rule[i * 8 + 7] - '0';
				if (!CheckForTile(tl0, tl1, _tl)) continue;
				if (!CheckForTile(tm0, tm1, _tm)) continue;
				if (!CheckForTile(tr0, tr1, _tr)) continue;
				if (!CheckForTile(ml0, ml1, _ml)) continue;
				if (!CheckForTile(mr0, mr1, _mr)) continue;
				if (!CheckForTile(bl0, bl1, _bl)) continue;
				if (!CheckForTile(bm0, bm1, _bm)) continue;
				if (!CheckForTile(br0, br1, _br)) continue;
				return TryRandom(i);
			}
			return -1;
			// Func
			bool CheckForTile (int _targetID0, int _targetID1, int _targetRule) => _targetRule switch {
				1 => _targetID0 == ruleID || _targetID1 == ruleID,
				2 => _targetID0 != ruleID && _targetID1 != ruleID,
				3 => _targetID0 != 0,
				4 => _targetID0 == 0,
				_ => true,
			};
			int TryRandom (int resultIndex) {
				int lastIndex = resultIndex;
				bool jumpOut = false;
				for (int i = resultIndex + 1; i < count; i++) {
					for (int j = 0; j < 8; j++) {
						if (rule[i * 8 + j] != rule[resultIndex * 8 + j]) {
							jumpOut = true;
							break;
						}
					}
					if (jumpOut) break;
					lastIndex = i;
				}
				if (resultIndex != lastIndex) resultIndex = AngeUtil.RandomInt(resultIndex, lastIndex + 1);
				return resultIndex;
			}
		}


		public static void DeleteAllEmptyMaps (string mapRoot) {
			var world = new World();
			foreach (var path in Util.EnumerateFiles(mapRoot, false, $"*.{AngePath.MAP_FILE_EXT}")) {
				try {
					if (!world.LoadFromDisk(path)) continue;
					if (world.EmptyCheck()) {
						Util.DeleteFile(path);
						Util.DeleteFile(path + ".meta");
					}
				} catch (System.Exception ex) { Game.LogException(ex); }
			}
		}


		public static string AngeName (this System.Type type) {
			string name = type.Name;
			if (char.IsLower(name[0])) name = name[1..];
			return name;
		}


		// AngeliA Hash Code
		public static int[] GetAngeHashs (System.Type[] types) {
			if (types == null || types.Length == 0) return new int[0];
			var results = new int[types.Length];
			for (int i = 0; i < results.Length; i++) {
				results[i] = GetAngeHash(types[i]);
			}
			return results;
		}


		public static int GetAngeHash (System.Type type) => type != null ? type.AngeName().AngeHash() : 0;


		public static int AngeHash (this System.Type type) => type.AngeName().AngeHash();


		public static int AngeHash (this string str, int avoid = 0) {
			const int p = 31;
			const int m = 1837465129;
			int hash_value = 0;
			int p_pow = 1;
			for (int i = 0; i < str.Length; i++) {
				char c = str[i];
				hash_value = (hash_value + (c - 'a' + 1) * p_pow) % m;
				p_pow = (p_pow * p) % m;
			}
			return hash_value == avoid ? avoid + 1 : hash_value;
		}


	}
}