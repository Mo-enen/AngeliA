using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace AngeliA;

public static partial class FrameworkUtil {


	// VAR
	private static readonly Dictionary<int, int> SYSTEM_NUMBER_POOL = new(10) {
		{ NumberZero.TYPE_ID, 0 },
		{ NumberOne.TYPE_ID, 1 },
		{ NumberTwo.TYPE_ID, 2 },
		{ NumberThree.TYPE_ID, 3 },
		{ NumberFour.TYPE_ID, 4 },
		{ NumberFive.TYPE_ID, 5 },
		{ NumberSix.TYPE_ID, 6 },
		{ NumberSeven.TYPE_ID, 7 },
		{ NumberEight.TYPE_ID, 8 },
		{ NumberNine.TYPE_ID, 9 },
	};
	private static readonly Dictionary<int, int> SYSTEM_NUMBER_POOL_ALT = new(10) {
		{ 0, NumberZero.TYPE_ID},
		{ 1, NumberOne.TYPE_ID},
		{ 2, NumberTwo.TYPE_ID},
		{ 3, NumberThree.TYPE_ID},
		{ 4, NumberFour.TYPE_ID},
		{ 5, NumberFive.TYPE_ID},
		{ 6, NumberSix.TYPE_ID},
		{ 7, NumberSeven.TYPE_ID},
		{ 8, NumberEight.TYPE_ID},
		{ 9, NumberNine.TYPE_ID},
	};
	private static readonly Int3[] WorldPosInViewCache = new Int3[256];
	private static readonly PhysicsCell[] BlockOperationCache = new PhysicsCell[32];
	private const int SEARCHLIGHT_DENSITY = 32;
	private const int REPOS_BASIC_ID = 0b01100100_000000000000_000000000000;


	// API
	public static void RedirectForRule (WorldStream stream, IRect unitRange, int z) {
		unitRange = unitRange.Expand(1);
		for (int i = unitRange.xMin; i < unitRange.xMax; i++) {
			for (int j = unitRange.yMin; j < unitRange.yMax; j++) {
				RedirectForRule(stream, i, j, z, BlockType.Level);
				RedirectForRule(stream, i, j, z, BlockType.Background);
				RedirectForRule(stream, i, j, z, BlockType.Element);
			}
		}
	}
	public static void RedirectForRule (WorldStream stream, int i, int j, int z, BlockType type) {
		int id = stream.GetBlockAt(i, j, z, type);
		if (id == 0) return;
		int oldID = id;
		if (!Renderer.TryGetSprite(id, out var sprite) || sprite.Group == null || !sprite.Group.WithRule) return;
		var group = sprite.Group;
		int ruleIndex = GetRuleIndex(
			group.Sprites, group.ID,
			stream.GetBlockAt(i - 1, j + 1, z, type),
			stream.GetBlockAt(i + 0, j + 1, z, type),
			stream.GetBlockAt(i + 1, j + 1, z, type),
			stream.GetBlockAt(i - 1, j + 0, z, type),
			stream.GetBlockAt(i + 1, j + 0, z, type),
			stream.GetBlockAt(i - 1, j - 1, z, type),
			stream.GetBlockAt(i + 0, j - 1, z, type),
			stream.GetBlockAt(i + 1, j - 1, z, type)
		);
		if (ruleIndex < 0 || ruleIndex >= group.Count) return;
		int newID = group.Sprites[ruleIndex].ID;
		if (newID == oldID) return;
		stream.SetBlockAt(i, j, z, type, newID);
	}


	public static int GetRuleIndex (IList<AngeSprite> sprites, int ruleID, int tl, int tm, int tr, int ml, int mr, int bl, int bm, int br) {
		int count = sprites.Count;
		for (int i = 0; i < count; i++) {
			var sprite = sprites[i];
			var rule = sprite.Rule;
			if (!CheckForTile(ruleID, tl, rule.RuleTL)) continue;
			if (!CheckForTile(ruleID, tm, rule.RuleT)) continue;
			if (!CheckForTile(ruleID, tr, rule.RuleTR)) continue;
			if (!CheckForTile(ruleID, ml, rule.RuleL)) continue;
			if (!CheckForTile(ruleID, mr, rule.RuleR)) continue;
			if (!CheckForTile(ruleID, bl, rule.RuleBL)) continue;
			if (!CheckForTile(ruleID, bm, rule.RuleB)) continue;
			if (!CheckForTile(ruleID, br, rule.RuleBR)) continue;
			return TryRandom(sprites, i, count);
		}
		return -1;
		// Func
		static bool CheckForTile (int ruleID, int targetID, Rule targetRule) => targetRule switch {
			Rule.SameTile => GpID(targetID) == ruleID,
			Rule.NotSameTile => GpID(targetID) != ruleID,
			Rule.AnyTile => targetID != 0,
			Rule.Empty => targetID == 0,
			_ => true,
		};
		static int TryRandom (IList<AngeSprite> sprites, int resultIndex, int count) {
			int lastIndex = resultIndex;
			bool jumpOut = false;
			var resultRule = sprites[resultIndex].Rule;
			for (int i = resultIndex + 1; i < count; i++) {
				var rule = sprites[i].Rule;
				for (int j = 0; j < 8; j++) {
					if (rule[j] != resultRule[j]) {
						jumpOut = true;
						break;
					}
				}
				if (jumpOut) break;
				lastIndex = i;
			}
			if (resultIndex != lastIndex) resultIndex = Util.QuickRandom(resultIndex, lastIndex + 1);
			return resultIndex;
		}
		static int GpID (int spriteID) => Renderer.TryGetSprite(spriteID, out var sprite) && sprite.Group != null ? sprite.Group.ID : 0;
	}


	public static bool BreakEntityBlock (Entity target) {

		if (!target.MapUnitPos.HasValue || target is not IBlockEntity eBlock) {
			InvokeObjectBreak(target.TypeID, target.Rect);
			return false;
		}

		target.Active = false;
		var mapPos = target.MapUnitPos.Value;

		// Remove from Map
		WorldSquad.Front.SetBlockAt(mapPos.x, mapPos.y, BlockType.Entity, 0);

		// Event
		eBlock.OnEntityPicked();
		if (eBlock.ContainEntityAsElement) {
			WorldSquad.Front.SetBlockAt(mapPos.x, mapPos.y, BlockType.Element, 0);
		}
		InvokeObjectBreak(target.TypeID, target.Rect);
		return true;
	}


	public static bool PickBlockAt (int unitX, int unitY, bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true, bool dropItemAfterPicked = true, bool allowMultiplePick = false) {

		bool result = false;

		// Try Pick Block Entity
		if (allowPickBlockEntity) {
			var hits = Physics.OverlapAll(
				PhysicsMask.MAP,
				new IRect(unitX.ToGlobal() + 1, unitY.ToGlobal() + 1, Const.CEL - 2, Const.CEL - 2),
				out int count, null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {

				var e = hits[i].Entity;
				if (e is not IBlockEntity eBlock) continue;

				e.Active = false;
				e.IgnoreReposition = true;
				var mapPos = e.MapUnitPos;

				// Remove from Map
				if (mapPos.HasValue) {
					WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, BlockType.Entity, 0);
					result = true;
					// Remove Embed Element
					if (eBlock.ContainEntityAsElement) {
						WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, BlockType.Element, 0);
					}
				}

				// Event
				eBlock.OnEntityPicked();
				if (dropItemAfterPicked && e.FromWorld && ItemSystem.HasItem(e.TypeID)) {
					// Drop Item
					var rect = e.Rect;
					ItemSystem.SpawnItem(e.TypeID, rect.CenterX(), rect.CenterY(), jump: false);
					InvokeBlockPicked(e.TypeID, rect);
				} else {
					// Break
					InvokeObjectBreak(e.TypeID, new IRect(e.X, e.Y, Const.CEL, Const.CEL));
				}

				// Refresh Nearby
				int nearByCount = Physics.OverlapAll(
					BlockOperationCache, PhysicsMask.MAP,
					new IRect((e.X + 1).ToUnifyGlobal(), (e.Y + 1).ToUnifyGlobal(), Const.CEL, Const.CEL).Expand(Const.CEL - 1),
					e, OperationMode.ColliderAndTrigger
				);
				for (int j = 0; j < nearByCount; j++) {
					var nearByHit = BlockOperationCache[j];
					if (nearByHit.Entity is not IBlockEntity nearByEntity) continue;
					nearByEntity.OnEntityRefresh();
				}

				// Mul Gate
				if (!allowMultiplePick) return true;

			}
		}

		// Try Pick Level Block
		if (allowPickLevelBlock) {
			int blockID = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level);
			if (blockID != 0) {
				int realBlockID = blockID;
				var blockRect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL);

				// Remove from Map
				WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Level, 0);
				result = true;

				// Event
				if (Renderer.TryGetSprite(blockID, out var sprite, true) && sprite.Group != null) {
					blockID = sprite.Group.ID;
					// Rule
					if (sprite.Group.WithRule) {
						RedirectForRule(
							WorldSquad.Stream, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}

				// Spawn Embedded Item
				int ele = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element);
				if (ele != 0 && ItemSystem.GetItem(ele) is Item _ele && _ele.EmbedIntoLevel) {
					ItemSystem.SpawnItem(ele, unitX.ToGlobal(), unitY.ToGlobal(), 1, false);
					WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Element, 0);
				}

				// Final
				if (dropItemAfterPicked && ItemSystem.HasItem(blockID)) {
					// Drop Item
					ItemSystem.SpawnItem(blockID, unitX.ToGlobal() + Const.HALF, unitY.ToGlobal() + Const.HALF, jump: false);
					InvokeBlockPicked(blockID, blockRect);
				} else {
					// Break
					InvokeBlockPicked(realBlockID, blockRect);
				}

				if (!allowMultiplePick) {
					return true;
				}
			}
		}

		// Try Pick BG Block
		if (allowPickBackgroundBlock) {
			int blockID = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Background);
			if (blockID != 0) {
				int realBlockID = blockID;
				var blockRect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL);

				// Remove from Map
				WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Background, 0);
				result = true;

				// Rule
				if (Renderer.TryGetSprite(blockID, out var sprite, true) && sprite.Group != null) {
					blockID = sprite.Group.ID;
					if (sprite.Group.WithRule) {
						RedirectForRule(
							WorldSquad.Stream, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}

				// Spawn Embedded Item
				int ele = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element);
				if (ele != 0 && ItemSystem.GetItem(ele) is Item _ele && _ele.EmbedIntoLevel) {
					ItemSystem.SpawnItem(ele, unitX.ToGlobal() + Const.HALF, unitY.ToGlobal() + Const.HALF, 1, false);
					WorldSquad.Front.SetBlockAt(unitX, unitY, BlockType.Element, 0);
				}

				// Final
				if (dropItemAfterPicked && ItemSystem.HasItem(blockID)) {
					// Drop Item
					ItemSystem.SpawnItem(blockID, unitX.ToGlobal(), unitY.ToGlobal(), jump: false);
					InvokeBlockPicked(realBlockID, blockRect);
				} else {
					// Break
					InvokeBlockPicked(realBlockID, blockRect);
				}

				if (!allowMultiplePick) {
					return true;
				}
			}
		}

		return result;
	}


	public static bool HasPickableBlockAt (int unitX, int unitY, bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true) {
		// Check for Block Entity
		if (allowPickBlockEntity) {
			var hits = Physics.OverlapAll(
				PhysicsMask.MAP,
				new IRect(unitX.ToGlobal() + 1, unitY.ToGlobal() + 1, Const.CEL - 2, Const.CEL - 2),
				out int count, null, OperationMode.ColliderAndTrigger
			);
			for (int i = 0; i < count; i++) {
				if (hits[i].Entity is IBlockEntity) return true;
			}
		}

		// Check for Level Block
		if (allowPickLevelBlock && WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level) != 0) {
			return true;
		}

		// Check for BG Block
		if (allowPickBackgroundBlock && WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Background) != 0) {
			return true;
		}

		return false;
	}


	public static bool PutBlockTo (int blockID, BlockType blockType, int targetUnitX, int targetUnitY) {

		bool success = false;
		var squad = WorldSquad.Stream;
		int z = Stage.ViewZ;

		switch (blockType) {
			case BlockType.Level:
			case BlockType.Background: {

				// Set Block to Map
				int finalID = 0;
				if (Renderer.TryGetSprite(blockID, out var sprite, false)) {
					finalID = blockID;
				} else if (Renderer.TryGetSpriteFromGroup(blockID, 0, out sprite)) {
					finalID = sprite.ID;
				}
				if (finalID != 0) {
					squad.SetBlockAt(targetUnitX, targetUnitY, z, blockType, finalID);
					// Rule
					if (sprite.Group != null && sprite.Group.WithRule) {
						RedirectForRule(
							squad, new IRect(targetUnitX - 1, targetUnitY - 1, 3, 3), Stage.ViewZ
						);
					}
				} else {
					squad.SetBlockAt(targetUnitX, targetUnitY, z, blockType, blockID);
				}
				success = true;
				break;
			}
			case BlockType.Entity: {


				success = true;

				// Set Block to World
				squad.SetBlockAt(targetUnitX, targetUnitY, z, BlockType.Entity, blockID);

				// Spawn Entity
				var e = Stage.SpawnEntityFromWorld(blockID, targetUnitX.ToGlobal(), targetUnitY.ToGlobal(), Stage.ViewZ, forceSpawn: true);
				if (e is IBlockEntity bEntity) {
					bEntity.OnEntityPut();
					// Refresh Nearby
					var hits = Physics.OverlapAll(
						PhysicsMask.MAP,
						new IRect((e.X + 1).ToUnifyGlobal(), (e.Y + 1).ToUnifyGlobal(), Const.CEL, Const.CEL).Expand(Const.CEL - 1),
						out int count,
						e, OperationMode.ColliderAndTrigger
					);
					for (int j = 0; j < count; j++) {
						var nearByHit = hits[j];
						if (nearByHit.Entity is not IBlockEntity nearbyEntity) continue;
						nearbyEntity.OnEntityRefresh();
					}
				}
				break;
			}

			case BlockType.Element: {

				// Embed as Element
				success = true;
				squad.SetBlockAt(targetUnitX, targetUnitY, z, BlockType.Element, blockID);

				// Refresh Block Entity
				var hits = Physics.OverlapAll(
					PhysicsMask.ENTITY,
					new IRect(targetUnitX.ToGlobal(), targetUnitY.ToGlobal(), Const.CEL, Const.CEL),
					out int count, null, OperationMode.ColliderAndTrigger
				);
				for (int i = 0; i < count; i++) {
					var hit = hits[i];
					if (hit.Entity is not IBlockEntity bEntity) return false;
					if (!bEntity.ContainEntityAsElement) return false;
					bEntity.OnEntityRefresh();
				}

				break;
			}
		}

		return success;
	}


	public static bool TryGetEmptyPlaceNearbyForEntity (
		int unitX, int unitY, int z, out int resultUnitX, out int resultUnitY,
		int maxRange = 8, bool preferNoSolidLevel = true
	) {

		var squad = WorldSquad.Front;
		resultUnitX = int.MinValue;
		resultUnitY = int.MinValue;

		// Center Check
		if (squad.GetBlockAt(unitX, unitY, z, BlockType.Entity) == 0) {
			resultUnitX = unitX;
			resultUnitY = unitY;
			if (!preferNoSolidLevel || !IsSolidLevel(unitX, unitY)) {
				return true;
			}
		}

		// Range Check
		for (int range = 1; range <= maxRange; range++) {
			int len = range * 2;
			int l = unitX - range;
			int r = unitX + range;
			int d = unitY - range;
			int u = unitY + range;
			for (int i = 0; i < len; i++) {
				if (Check(l, d + i, ref resultUnitX, ref resultUnitY)) return true;
				if (Check(r, u - i, ref resultUnitX, ref resultUnitY)) return true;
				if (Check(l + i, d, ref resultUnitX, ref resultUnitY)) return true;
				if (Check(r - i, u, ref resultUnitX, ref resultUnitY)) return true;
			}
		}

		// Final
		return resultUnitX != int.MinValue;

		// Func
		bool Check (int x, int y, ref int _resultUnitX, ref int _resultUnitY) {

			if (squad.GetBlockAt(x, y, z, BlockType.Entity) != 0) return false;

			if (!preferNoSolidLevel || !IsSolidLevel(x, y)) {
				_resultUnitX = x;
				_resultUnitY = y;
				return true;
			} else {
				if (_resultUnitX == int.MinValue) {
					_resultUnitX = x;
					_resultUnitY = y;
				}
				return false;
			}

		}

		bool IsSolidLevel (int x, int y) {
			int id = squad.GetBlockAt(x, y, z, BlockType.Level);
			return id != 0 && (!Renderer.TryGetSprite(id, out var sp, false) || !sp.IsTrigger);
		}
	}


	public static void RemoveFromWorldMemory (Entity entity) {
		var mapPos = entity.MapUnitPos;
		if (mapPos.HasValue) {
			WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, BlockType.Entity, 0);
		}
	}


	public static bool SearchlightBlockCheck (IBlockSquad squad, Int3 startUnitPoint, Direction8? direction, int unitDistance = Const.MAP, bool reverse = false) {
		(int deltaX, int deltaY) = direction.HasValue ? direction.Value.Normal() : Int2.zero;
		int z = startUnitPoint.z;
		if (deltaX == 0 && deltaY == 0) {
			// No Direction
			int len = unitDistance * 14142 / 10000 / SEARCHLIGHT_DENSITY;
			if (!reverse && ContentCheck(squad, startUnitPoint.x, startUnitPoint.y, z)) {
				return true;
			}
			for (int i = 1; i < len; i++) {
				int index = reverse ? len - 1 - i : i;
				int desIndex = index * SEARCHLIGHT_DENSITY;
				int pL = startUnitPoint.x - desIndex;
				int pR = startUnitPoint.x + desIndex;
				int pD = startUnitPoint.y - desIndex;
				int pU = startUnitPoint.y + desIndex;
				for (int j = 0; j < index; j++) {
					int desJ = j * SEARCHLIGHT_DENSITY;
					if (
						ContentCheck(squad, pL + desJ, startUnitPoint.y + desJ, z) ||
						ContentCheck(squad, startUnitPoint.x + desJ, pU - desJ, z) ||
						ContentCheck(squad, pR - desJ, startUnitPoint.y - desJ, z) ||
						ContentCheck(squad, startUnitPoint.x - desJ, pD + desJ, z)
					) {
						return true;
					}
				}
			}
			if (reverse && ContentCheck(squad, startUnitPoint.x, startUnitPoint.y, z)) {
				return true;
			}
		} else if (deltaX != 0 && deltaY != 0) {
			// Tilt
			int len = unitDistance * 14142 / 10000 / SEARCHLIGHT_DENSITY;
			int desDeltaX = SEARCHLIGHT_DENSITY * deltaX;
			int desDeltaY = SEARCHLIGHT_DENSITY * deltaY;
			for (int i = 0; i < len; i++) {
				int index = reverse ? len - 1 - i : i;
				int wide = index + 1;
				int pointX = startUnitPoint.x + deltaX * index * SEARCHLIGHT_DENSITY;
				int pointY = startUnitPoint.y;
				for (int j = 0; j < wide; j++) {
					if (ContentCheck(squad, pointX, pointY, z)) {
						return true;
					}
					pointX -= desDeltaX;
					pointY += desDeltaY;
				}
			}
		} else {
			// Straight
			int len = unitDistance / SEARCHLIGHT_DENSITY;
			int desDeltaX = SEARCHLIGHT_DENSITY * deltaX;
			int desDeltaY = SEARCHLIGHT_DENSITY * deltaY;
			int desDeltaAltX = SEARCHLIGHT_DENSITY * (1 - deltaX.Abs());
			int desDeltaAltY = SEARCHLIGHT_DENSITY * (1 - deltaY.Abs());
			for (int i = 0; i < len; i++) {
				int index = reverse ? len - 1 - i : i;
				int wide = index + 1;
				int pX = startUnitPoint.x + index * desDeltaX;
				int pY = startUnitPoint.y + index * desDeltaY;
				for (int j = 0; j < wide; j++) {
					if (
						ContentCheck(squad, pX + desDeltaAltX * j, pY + desDeltaAltY * j, z) ||
						ContentCheck(squad, pX - desDeltaAltX * j, pY - desDeltaAltY * j, z)
					) {
						return true;
					}
				}
			}
		}
		return false;
		// Func
		static bool ContentCheck (IBlockSquad squad, int unitX, int unitY, int z) {
			var (level, bg, entity, _) = squad.GetAllBlocksAt(unitX, unitY, z);
			return level != 0 || bg != 0 || entity != 0;
		}
	}


	public static FittingPose GetEntityPose (int typeID, int unitX, int unitY, bool horizontal) {
		bool n = WorldSquad.Front.GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
		bool p = WorldSquad.Front.GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
		return
			n && p ? FittingPose.Mid :
			!n && p ? FittingPose.Left :
			n && !p ? FittingPose.Right :
			FittingPose.Single;
	}


	public static FittingPose GetEntityPose (Entity entity, bool horizontal, int mask, out Entity left_down, out Entity right_up, OperationMode mode = OperationMode.ColliderOnly, Tag tag = 0) {
		left_down = null;
		right_up = null;
		int unitX = entity.X.ToUnit();
		int unitY = entity.Y.ToUnit();
		int typeID = entity.TypeID;
		bool n = WorldSquad.Front.GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
		bool p = WorldSquad.Front.GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
		if (n) {
			left_down = Physics.GetEntity(typeID, entity.Rect.EdgeOutside(horizontal ? Direction4.Left : Direction4.Down), mask, entity, mode, tag);
		}
		if (p) {
			right_up = Physics.GetEntity(typeID, entity.Rect.EdgeOutside(horizontal ? Direction4.Right : Direction4.Up), mask, entity, mode, tag);
		}
		return
			n && p ? FittingPose.Mid :
			!n && p ? FittingPose.Left :
			n && !p ? FittingPose.Right :
			FittingPose.Single;
	}


	public static Int3[] ForAllWorldInRange (IRect overlapUnitRange, int z, out int count) {
		int left = overlapUnitRange.xMin.UDivide(Const.MAP);
		int right = (overlapUnitRange.xMax + 1).UDivide(Const.MAP);
		int down = overlapUnitRange.yMin.UDivide(Const.MAP);
		int up = (overlapUnitRange.yMax + 1).UDivide(Const.MAP);
		int index = 0;
		int maxCount = WorldPosInViewCache.Length;
		for (int i = left; i <= right; i++) {
			for (int j = down; j <= up; j++) {
				WorldPosInViewCache[index] = new Int3(i, j, z);
				index++;
				if (index >= maxCount) {
					goto _END_;
				}
			}
		}
		_END_:;
		count = index;
		return WorldPosInViewCache;
	}


	public static Int3[] ForAllExistsWorldInRange (IBlockSquad squad, IRect overlapUnitRange, int z, out int count) {
		int left = overlapUnitRange.xMin.UDivide(Const.MAP);
		int right = (overlapUnitRange.xMax + 1).UDivide(Const.MAP);
		int down = overlapUnitRange.yMin.UDivide(Const.MAP);
		int up = (overlapUnitRange.yMax + 1).UDivide(Const.MAP);
		int index = 0;
		int maxCount = WorldPosInViewCache.Length;
		for (int i = left; i <= right; i++) {
			for (int j = down; j <= up; j++) {
				var worldPos = new Int3(i, j, z);
				if (!squad.WorldExists(worldPos)) continue;
				WorldPosInViewCache[index] = worldPos;
				index++;
				if (index >= maxCount) {
					goto _END_;
				}
			}
		}
		_END_:;
		count = index;
		return WorldPosInViewCache;
	}


	public static void PaintBlock (int unitX, int unitY, int blockColorID, bool overlapExistingElement = false) {
		var squad = WorldSquad.Front;
		var (lv, bg, _, ele) = squad.GetAllBlocksAt(unitX, unitY, Stage.ViewZ);
		if (lv == 0 && bg == 0) return;
		if (!overlapExistingElement && !BlockColoringSystem.TryGetColor(ele, out _) && ele != 0) return;
		squad.SetBlockAt(unitX, unitY, BlockType.Element, blockColorID);
	}


	public static void TryEjectOutsideGround (Rigidbody rig, int collisionMask = 0, int unitRange = 2, int speed = 32) {
		var centerPos = rig.Rect.CenterInt();
		var targetDir = Int2.zero;
		collisionMask = collisionMask == 0 ? rig.CollisionMask : collisionMask;
		if (CheckForDir(centerPos, Int2.up, unitRange, collisionMask)) {
			// Up
			targetDir = Int2.up;
		} else if (CheckForDir(centerPos, Int2.right, unitRange, collisionMask)) {
			// Right
			targetDir = Int2.right;
		} else if (CheckForDir(centerPos, Int2.left, unitRange, collisionMask)) {
			// Left
			targetDir = Int2.left;
		} else if (CheckForDir(centerPos, Int2.down, unitRange, collisionMask)) {
			// Down
			targetDir = Int2.down;
		}
		// Perform Eject
		if (targetDir != Int2.zero) {
			rig.PerformMove(targetDir.x * speed, targetDir.y * speed, ignoreCarry: true);
		}
		// Func
		static bool CheckForDir (Int2 center, Int2 dir, int unitRange, int mask) {
			for (int i = 1; i <= unitRange; i++) {
				int delta = i * Const.CEL;
				if (!Physics.Overlap(mask, IRect.Point(center.x + dir.x * delta, center.y + dir.y * delta))) {
					return true;
				}
			}
			return false;
		}
	}


	public static void DeleteAllEmptyMaps (string mapRoot) {
		foreach (var path in Util.EnumerateFiles(mapRoot, false, AngePath.MAP_SEARCH_PATTERN)) {
			try {
				if (Util.IsExistingFileEmpty(path)) Util.DeleteFile(path);
			} catch (System.Exception ex) { Debug.LogException(ex); }
		}
	}


	/* // Reposition Element Code Test
	[OnGameInitialize]
	internal static void RepositionElementCodeTest () {
		//var b = new StringBuilder();
		const int MAX_RANGE = 8 * Const.CEL ;
		int minX = 9999;
		int minY = 9999;
		int maxX = -9999;
		int maxY = -9999;
		for (int x = -MAX_RANGE; x < MAX_RANGE; x++) {
			for (int y = -MAX_RANGE; y < MAX_RANGE; y++) {
				int code = GetRepositionElementCode(x, y);
				if (TryGetRepositionElementDelta(code, out int newX, out int newY)
				) {
					//b.AppendLine($"({x - newX} {y - newY}) {x} -> {newX} {y} -> {newY}");
					minX = Util.Min(minX, x - newX);
					minY = Util.Min(minY, y - newY);
					maxX = Util.Max(maxX, x - newX);
					maxY = Util.Max(maxY, y - newY);
				} else {
					Debug.LogError("error");
				}
			}
		}
		Debug.Log("minX", minX, "minY", minY, "maxX", maxX, "maxY", maxY);
		//string path = Util.CombinePaths(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "Test.txt");
		//Util.TextToFile(b.ToString(), path);
		//Game.OpenUrl(path);
	}
	//*/


	public static int GetRepositionElementCode (int deltaGlobalX, int deltaGlobalY) {
		const int MAX_RANGE = 8 * Const.CEL;
		deltaGlobalX = deltaGlobalX.Clamp(-MAX_RANGE, MAX_RANGE) + MAX_RANGE;
		deltaGlobalY = deltaGlobalY.Clamp(-MAX_RANGE, MAX_RANGE) + MAX_RANGE;
		return REPOS_BASIC_ID | (deltaGlobalX << 12) | deltaGlobalY;
	}


	public static bool TryGetRepositionElementDelta (int elementCode, out int deltaGlobalX, out int deltaGlobalY) {
		const int MAX_RANGE = 8 * Const.CEL;
		deltaGlobalX = 0;
		deltaGlobalY = 0;
		if ((REPOS_BASIC_ID >> 24) != (elementCode >> 24)) return false;
		deltaGlobalX = (int)(((uint)(elementCode << 8)) >> 20) - MAX_RANGE;
		deltaGlobalY = (int)(((uint)(elementCode << 20)) >> 20) - MAX_RANGE;
		return true;
	}


	// System Number
	public static int SystemNumberID_to_Number (int id) => SYSTEM_NUMBER_POOL.TryGetValue(id, out int number) ? number : -1;


	public static int Number_to_SystemNumberID (int number) => SYSTEM_NUMBER_POOL_ALT.TryGetValue(number, out int id) ? id : 0;


	public static bool ReadSystemNumber (IBlockSquad squad, int unitX, int unitY, int z, out int number) {
		number = 0;
		bool hasH = ReadSystemNumber(squad, unitX, unitY, z, Direction4.Right, out int numberH);
		bool hasV = ReadSystemNumber(squad, unitX, unitY, z, Direction4.Down, out int numberV);
		if (!hasH && !hasV) return false;
		if (hasH == hasV) {
			number = Util.Max(numberH, numberV);
			return true;
		} else {
			number = hasH ? numberH : numberV;
			return true;
		}
	}


	public static bool ReadSystemNumber (IBlockSquad squad, int unitX, int unitY, int z, Direction4 direction, out int number) {

		number = 0;
		int digitCount = 0;
		int x = unitX;
		int y = unitY;
		var delta = direction.Normal();

		// Find Start
		int left = int.MinValue;
		int down = int.MinValue;
		while (HasSystemNumber(squad, x, y, z)) {
			left = x;
			down = y;
			x -= delta.x;
			y -= delta.y;
		}
		if (left == int.MinValue) return false;

		// Get Number
		x = left;
		y = down;
		while (digitCount < 9 && TryGetSingleSystemNumber(squad, x, y, z, out int digit)) {
			number *= 10;
			number += digit;
			digitCount++;
			x += delta.x;
			y += delta.y;
		}
		return digitCount > 0;
	}


	public static bool HasSystemNumber (IBlockSquad squad, int unitX, int unitY, int z) {
		int id = squad.GetBlockAt(unitX, unitY, z, BlockType.Element);
		return id != 0 && SystemNumberID_to_Number(id) != -1;
	}


	public static bool TryGetSingleSystemNumber (IBlockSquad squad, int unitX, int unitY, int z, out int digitValue) {
		int id = squad.GetBlockAt(unitX, unitY, z, BlockType.Element);
		digitValue = SystemNumberID_to_Number(id);
		return digitValue >= 0;
	}


	// Block Aiming
	public static bool GetAimingBuilderPositionFromMouse (Character holder, BlockType blockType, out int targetUnitX, out int targetUnitY, out bool requireEmbedAsElement) {

		requireEmbedAsElement = false;
		var mouseUnitPos = Input.MouseGlobalPosition.ToUnit();
		targetUnitX = mouseUnitPos.x;
		targetUnitY = mouseUnitPos.y;

		// Overlap with Holder Check
		var mouseRect = new IRect(targetUnitX.ToGlobal(), targetUnitY.ToGlobal(), Const.CEL, Const.CEL);
		if (holder.Rect.Overlaps(mouseRect)) {
			return false;
		}

		// Block Empty Check
		return ValidForPutBlock(targetUnitX, targetUnitY, blockType, out requireEmbedAsElement);

	}


	public static bool GetAimingBuilderPositionFromKey (Character holder, BlockType blockType, out int targetUnitX, out int targetUnitY, out bool requireEmbedAsElement, bool ignoreValid = false) {

		bool valid;
		requireEmbedAsElement = false;
		var aim = holder.Attackness.AimingDirection;
		var aimNormal = aim.Normal();
		if (!holder.Movement.IsClimbing) {
			// Normal
			int pointX = holder.Rect.CenterX();
			int pointY = aim.IsTop() ? holder.Rect.yMax - Const.HALF / 2 : holder.Rect.y + Const.HALF;
			targetUnitX = pointX.ToUnit() + aimNormal.x;
			targetUnitY = pointY.ToUnit() + aimNormal.y;
		} else {
			// Climbing
			int pointX = holder.Rect.CenterX();
			int pointY = holder.Rect.yMax - Const.HALF / 2;
			targetUnitX = holder.Movement.FacingRight ? pointX.ToUnit() + 1 : pointX.ToUnit() - 1;
			targetUnitY = pointY.ToUnit() + aimNormal.y;
		}

		valid = ignoreValid || ValidForPutBlock(targetUnitX, targetUnitY, blockType, out requireEmbedAsElement);

		// Redirect
		if (!valid) {
			int oldTargetX = targetUnitX;
			int oldTargetY = targetUnitX;
			if (aim.IsBottom()) {
				if (aim == Direction8.Bottom) {
					targetUnitX += holder.Movement.FacingRight ? 1 : -1;
				}
			} else if (aim.IsTop()) {
				if (aim == Direction8.Top) {
					targetUnitX += holder.Movement.FacingRight ? 1 : -1;
				}
			} else {
				targetUnitY++;
			}
			if (oldTargetX != targetUnitX || oldTargetY != targetUnitY) {
				valid = ignoreValid || ValidForPutBlock(targetUnitX, targetUnitY, blockType, out requireEmbedAsElement);
			}
		}

		return valid;
	}


	private static bool ValidForPutBlock (int unitX, int unitY, BlockType blockType, out bool requireEmbedAsElement) {

		requireEmbedAsElement = false;

		// Non Entity
		if (blockType != BlockType.Entity) {
			return WorldSquad.Front.GetBlockAt(unitX, unitY, blockType) == 0;
		}

		// For Entity
		// Check Overlaping Level Block
		var rect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL);
		if (Physics.Overlap(
			PhysicsMask.LEVEL, rect, null, OperationMode.ColliderOnly
		)) return false;

		// Check Overlaping Entity
		var hits = Physics.OverlapAll(PhysicsMask.ENTITY, rect, out int count, null, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not IBlockEntity bEntity) return false;
			if (!bEntity.ContainEntityAsElement) return false;
			requireEmbedAsElement = true;
		}

		return true;
	}


	public static bool GetAimingPickerPositionFromMouse (
		Character holder, int unitRange, out int targetUnitX, out int targetUnitY, out bool inRange,
		bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true
	) {

		var mouseUnitPos = Input.MouseGlobalPosition.ToUnit();
		targetUnitX = mouseUnitPos.x;
		targetUnitY = mouseUnitPos.y;

		// Range Check
		int holderUnitX = holder.Rect.CenterX().ToUnit();
		int holderUnitY = (holder.Rect.y + Const.HALF).ToUnit();
		inRange = targetUnitX.InRangeInclude(holderUnitX - unitRange, holderUnitX + unitRange) &&
				targetUnitY.InRangeInclude(holderUnitY - unitRange, holderUnitY + unitRange);

		if (!inRange) return false;

		// Pickable Check
		return HasPickableBlockAt(
			targetUnitX, targetUnitY, allowPickBlockEntity, allowPickLevelBlock, allowPickBackgroundBlock
		);
	}


	public static bool GetAimingPickerPositionFromKey (
		Character pHolder, out int targetUnitX, out int targetUnitY,
		bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true
	) {

		var aim = pHolder.Attackness.AimingDirection;
		var aimNormal = aim.Normal();
		int pointX = aim.IsTop() ? pHolder.Rect.CenterX() : pHolder.Movement.FacingRight ? pHolder.Rect.xMax - 16 : pHolder.Rect.xMin + 16;
		int pointY = pHolder.Rect.yMax - 16;
		targetUnitX = pointX.ToUnit() + aimNormal.x;
		targetUnitY = pointY.ToUnit() + aimNormal.y;
		bool hasTraget = HasPickableBlockAt(
			targetUnitX, targetUnitY,
			allowPickBlockEntity, allowPickLevelBlock, allowPickBackgroundBlock
		);

		// Redirect
		if (!hasTraget) {
			int oldTargetX = targetUnitX;
			int oldTargetY = targetUnitX;
			if (aim.IsBottom()) {
				if (aim == Direction8.Bottom) {
					targetUnitX += pointX.UMod(Const.CEL) < Const.HALF ? -1 : 1;
				}
			} else if (aim.IsTop()) {
				if (aim == Direction8.Top) {
					targetUnitX += pHolder.Movement.FacingRight ? 1 : -1;
				}
			} else {
				targetUnitY--;
			}
			if (oldTargetX != targetUnitX || oldTargetY != targetUnitY) {
				hasTraget = HasPickableBlockAt(
					targetUnitX, targetUnitY,
					allowPickBlockEntity, allowPickLevelBlock, allowPickBackgroundBlock
				);
			}
		}

		return hasTraget;

	}


	// Rule
	public static BlockRule DigitToBlockRule (int digit) {
		// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
		//              000        001        010           011       100
		// eg: "02022020,11111111,01022020,02012020,..."
		// digit: int32 10000000 000 000 000 000 000 000 000 000
		if (!digit.GetBit(0)) return BlockRule.EMPTY;
		var result = BlockRule.EMPTY;
		for (int i = 0; i < 8; i++) {
			int tileStrNumber = 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 0) ? 4 : 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 1) ? 2 : 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 2) ? 1 : 0;
			result[i] = (Rule)tileStrNumber;
		}
		return result;
	}

	public static void DigitToRuleByte (int digit, byte[] bytes) {
		if (!digit.GetBit(0)) {
			System.Array.Clear(bytes);
			return;
		}
		for (int i = 0; i < 8; i++) {
			int tileStrNumber = 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 0) ? 4 : 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 1) ? 2 : 0;
			tileStrNumber += digit.GetBit(8 + i * 3 + 2) ? 1 : 0;
			bytes[i] = (byte)tileStrNumber;
		}
	}

	public static int BlockRuleToDigit (BlockRule ruleStr) {
		// ↖↑↗←→↙↓↘,... 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty NaN=Error
		//              000        001        010           011       100
		// eg: "02022020,11111111,01022020,02012020,..."
		// digit: int32 10000000 000 000 000 000 000 000 000 000
		int digit = 0;
		digit.SetBit(0, true);
		for (int i = 0; i < 8; i++) {
			byte b = (byte)ruleStr[i];
			digit.SetBit(8 + i * 3 + 0, (b / 4) % 2 == 1);
			digit.SetBit(8 + i * 3 + 1, (b / 2) % 2 == 1);
			digit.SetBit(8 + i * 3 + 2, (b / 1) % 2 == 1);
		}
		return digit;
	}

	public static int RuleByteToDigit (byte[] singleRule) {
		// ↖↑↗←→↙↓↘ 0=Whatever 1=SameTile 2=NotSameTile 3=AnyTile 4=Empty 255=Error
		//              000        001        010           011       100
		// eg: "02022020,11111111,01022020,02012020,..."
		// digit: int32 10000000 000 000 000 000 000 000 000 000
		int digit = 0;
		digit.SetBit(0, true);
		for (int i = 0; i < 8; i++) {
			byte b = singleRule[i];
			digit.SetBit(8 + i * 3 + 0, (b / 4) % 2 == 1);
			digit.SetBit(8 + i * 3 + 1, (b / 2) % 2 == 1);
			digit.SetBit(8 + i * 3 + 2, (b / 1) % 2 == 1);
		}
		return digit;
	}


}
