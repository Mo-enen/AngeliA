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
	private static readonly Int3[] WorldPosInViewCache = new Int3[512];
	private const int SEARCHLIGHT_DENSITY = 32;
	private const int REPOS_BASIC_ID = 0b01100100_000000000000_000000000000;


	// Rule
	/// <summary>
	/// Auto tile all map blocks inside given range
	/// </summary>
	/// <param name="squad">Source of the map blocks</param>
	/// <param name="unitRange">Target range in unit space</param>
	/// <param name="z">Position Z</param>
	public static void RedirectForRule (IBlockSquad squad, IRect unitRange, int z) {
		unitRange = unitRange.Expand(1);
		for (int i = unitRange.xMin; i < unitRange.xMax; i++) {
			for (int j = unitRange.yMin; j < unitRange.yMax; j++) {
				RedirectForRule(squad, i, j, z, BlockType.Level);
				RedirectForRule(squad, i, j, z, BlockType.Background);
				RedirectForRule(squad, i, j, z, BlockType.Element);
			}
		}
	}


	/// <summary>
	/// Auto tile map block in given position and all tiles nearby
	/// </summary>
	/// <param name="squad">Source of the map blocks</param>
	/// <param name="unitX">Position in unit space</param>
	/// <param name="unitY">Position in unit space</param>
	/// <param name="z">Position z</param>
	/// <param name="type">Type of the auto tile block</param>
	public static void RedirectForRule (IBlockSquad squad, int unitX, int unitY, int z, BlockType type) {
		int id = squad.GetBlockAt(unitX, unitY, z, type);
		if (id == 0) return;
		int oldID = id;
		if (!Renderer.TryGetSprite(id, out var sprite) || sprite.Group == null || !sprite.Group.WithRule) return;
		var group = sprite.Group;
		int ruleIndex = GetRuleIndex(
			group.Sprites, group.ID,
			squad.GetBlockAt(unitX - 1, unitY + 1, z, type),
			squad.GetBlockAt(unitX + 0, unitY + 1, z, type),
			squad.GetBlockAt(unitX + 1, unitY + 1, z, type),
			squad.GetBlockAt(unitX - 1, unitY + 0, z, type),
			squad.GetBlockAt(unitX + 1, unitY + 0, z, type),
			squad.GetBlockAt(unitX - 1, unitY - 1, z, type),
			squad.GetBlockAt(unitX + 0, unitY - 1, z, type),
			squad.GetBlockAt(unitX + 1, unitY - 1, z, type)
		);
		if (ruleIndex < 0 || ruleIndex >= group.Count) return;
		int newID = group.Sprites[ruleIndex].ID;
		if (newID == oldID) return;
		squad.SetBlockAt(unitX, unitY, z, type, newID);
	}


	/// <summary>
	/// Find which tile in the given list conforms the auto tiling rule
	/// </summary>
	/// <param name="sprites">Source list</param>
	/// <param name="ruleGroupID">Int data of the tiling rule. Get this data with FrameworkUtil.BlockRuleToDigit</param>
	/// <param name="tl">ID for block at top-left</param>
	/// <param name="tm">ID for block at top</param>
	/// <param name="tr">ID for block at top-right</param>
	/// <param name="ml">ID for block at left</param>
	/// <param name="mr">ID for block at right</param>
	/// <param name="bl">ID for block at bottom-left</param>
	/// <param name="bm">ID for block at bottom</param>
	/// <param name="br">ID for block at bottom-right</param>
	/// <returns>Index of the founded sprite</returns>
	public static int GetRuleIndex (IList<AngeSprite> sprites, int ruleGroupID, int tl, int tm, int tr, int ml, int mr, int bl, int bm, int br) {
		int count = sprites.Count;
		for (int i = 0; i < count; i++) {
			var sprite = sprites[i];
			var rule = sprite.Rule;
			if (!CheckForTile(ruleGroupID, tl, rule.RuleTL)) continue;
			if (!CheckForTile(ruleGroupID, tm, rule.RuleT)) continue;
			if (!CheckForTile(ruleGroupID, tr, rule.RuleTR)) continue;
			if (!CheckForTile(ruleGroupID, ml, rule.RuleL)) continue;
			if (!CheckForTile(ruleGroupID, mr, rule.RuleR)) continue;
			if (!CheckForTile(ruleGroupID, bl, rule.RuleBL)) continue;
			if (!CheckForTile(ruleGroupID, bm, rule.RuleB)) continue;
			if (!CheckForTile(ruleGroupID, br, rule.RuleBR)) continue;
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


	/// <summary>
	/// Convert auto tiling rule digit into BlockRule struct
	/// </summary>
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


	/// <summary>
	/// Convert auto tiling rule digit into byte array
	/// </summary>
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


	/// <summary>
	/// Convert BlockRule struct into auto tiling rule digit
	/// </summary>
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


	/// <summary>
	/// Convert byte array into auto tiling rule digit
	/// </summary>
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


	// Misc
	/// <summary>
	/// Remove given block entity from map and invoke corresponding callback functions. (Do not handle item drops)
	/// </summary>
	public static bool BreakEntityBlock (IBlockEntity eBlock) {

		if (eBlock is not Entity target) return false;

		// Not from Map
		if (!target.MapUnitPos.HasValue) {
			InvokeObjectBreak(target.TypeID, target.Rect);
			return false;
		}

		// From Map
		target.Active = false;
		var mapPos = target.MapUnitPos.Value;
		int z = Stage.ViewZ;

		// Remove from Map
		WorldSquad.Front.SetBlockAt(mapPos.x, mapPos.y, z, BlockType.Entity, 0);

		// Event
		eBlock.OnEntityPicked();
		if (eBlock.EmbedEntityAsElement) {
			WorldSquad.Front.SetBlockAt(mapPos.x, mapPos.y, z, BlockType.Element, 0);
		}
		InvokeObjectBreak(target.TypeID, target.Rect);

		return true;
	}


	/// <summary>
	/// Perform a block pick
	/// </summary>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="allowPickBlockEntity">True if entity blocks will be picked</param>
	/// <param name="allowPickLevelBlock">True if level blocks will be picked</param>
	/// <param name="allowPickBackgroundBlock">True if background blocks will be picked</param>
	/// <param name="dropItemAfterPicked">True if spawn an ItemHolder with the block</param>
	/// <param name="allowMultiplePick">True if pick more than one block with one function call</param>
	/// <returns>True if any block is picked</returns>
	public static bool PickBlockAt (int unitX, int unitY, bool allowPickBlockEntity = true, bool allowPickLevelBlock = true, bool allowPickBackgroundBlock = true, bool dropItemAfterPicked = true, bool allowMultiplePick = false) {

		bool result = false;
		int z = Stage.ViewZ;

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
					WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, mapPos.Value.z, BlockType.Entity, 0);
					result = true;
					// Remove Embed Element
					if (eBlock.EmbedEntityAsElement) {
						WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, mapPos.Value.z, BlockType.Element, 0);
					}
				}

				// Event
				eBlock.OnEntityPicked();
				if (dropItemAfterPicked && e.FromWorld && ItemSystem.HasItem(e.TypeID)) {
					// Drop Block Building Item
					var rect = e.Rect;
					ItemSystem.SpawnItem(e.TypeID, rect.CenterX(), rect.CenterY(), jump: false);
					InvokeBlockPicked(e.TypeID, rect);
				} else {
					// Break
					InvokeObjectBreak(e.TypeID, new IRect(e.X, e.Y, Const.CEL, Const.CEL));
				}

				// Refresh Nearby
				IBlockEntity.RefreshBlockEntitiesNearby(e.XY.ToUnit(), e);

				// Mul Gate
				if (!allowMultiplePick) return true;

			}
		}

		// Try Pick Level Block
		if (allowPickLevelBlock) {
			int blockID = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level);
			if (
				blockID != 0 && (
				!Renderer.TryGetSprite(blockID, out AngeSprite sprite, false) ||
				!sprite.Tag.HasAll(Tag.Unbreackable)
			)) {

				int realBlockID = blockID;
				var blockRect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL);

				// Remove from Map
				WorldSquad.Front.SetBlockAt(unitX, unitY, z, BlockType.Level, 0);
				result = true;

				// Event
				if (sprite != null && sprite.Group != null) {
					blockID = sprite.Group.ID;
					// Rule
					if (sprite.Group.WithRule) {
						RedirectForRule(
							WorldSquad.Front, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}

				// Spawn Embedded Item
				int ele = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element);
				if (ele != 0 && ItemSystem.GetItem(ele) is Item _ele && _ele.EmbedIntoLevel) {
					ItemSystem.SpawnItem(ele, unitX.ToGlobal(), unitY.ToGlobal(), 1, false);
					WorldSquad.Front.SetBlockAt(unitX, unitY, z, BlockType.Element, 0);
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
			if (
				blockID != 0 && (
				!Renderer.TryGetSprite(blockID, out AngeSprite sprite, false) ||
				!sprite.Tag.HasAll(Tag.Unbreackable)
			)) {
				int realBlockID = blockID;
				var blockRect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL);

				// Remove from Map
				WorldSquad.Front.SetBlockAt(unitX, unitY, z, BlockType.Background, 0);
				result = true;

				// Rule
				if (sprite != null && sprite.Group != null) {
					blockID = sprite.Group.ID;
					if (sprite.Group.WithRule) {
						RedirectForRule(
							WorldSquad.Front, new IRect(unitX - 1, unitY - 1, 3, 3), Stage.ViewZ
						);
					}
				}

				// Spawn Embedded Item
				int ele = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Element);
				if (ele != 0 && ItemSystem.GetItem(ele) is Item _ele && _ele.EmbedIntoLevel) {
					ItemSystem.SpawnItem(ele, unitX.ToGlobal() + Const.HALF, unitY.ToGlobal() + Const.HALF, 1, false);
					WorldSquad.Front.SetBlockAt(unitX, unitY, z, BlockType.Element, 0);
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


	/// <summary>
	/// True if found any block can be pick at given position
	/// </summary>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="allowPickBlockEntity">True if entity blocks will be picked</param>
	/// <param name="allowPickLevelBlock">True if level blocks will be picked</param>
	/// <param name="allowPickBackgroundBlock">True if background blocks will be picked</param>
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
		if (allowPickLevelBlock) {
			int id = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Level);
			if (id != 0) {
				if (!Renderer.TryGetSprite(id, out var sprite, false) || !sprite.Tag.HasAll(Tag.Unbreackable)) {
					return true;
				}
			}
		}

		// Check for BG Block
		if (allowPickBackgroundBlock) {
			int id = WorldSquad.Front.GetBlockAt(unitX, unitY, BlockType.Background);
			if (id != 0) {
				if (!Renderer.TryGetSprite(id, out var sprite, false) || !sprite.Tag.HasAll(Tag.Unbreackable)) {
					return true;
				}
			}
		}

		return false;
	}


	/// <summary>
	/// Build a block into the map
	/// </summary>
	/// <param name="blockID">ID of the building block</param>
	/// <param name="blockType">Type of the building block</param>
	/// <param name="targetUnitX">Target position X in unit space</param>
	/// <param name="targetUnitY">Target position Y in unit space</param>
	/// <returns>True if the block is built</returns>
	public static bool PutBlockTo (int blockID, BlockType blockType, int targetUnitX, int targetUnitY) {

		bool success = false;
		var squad = WorldSquad.Front;
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
					IBlockEntity.RefreshBlockEntitiesNearby(e.XY.ToUnit(), e);
				}
				break;
			}

			case BlockType.Element: {

				// Embed as Element
				if (IBlockEntity.IsIgnoreEmbedAsElement(blockID)) {
					InvokeErrorHint(targetUnitX.ToGlobal() + Const.HALF, targetUnitY.ToGlobal(), blockID);
					break;
				}

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
					if (!bEntity.EmbedEntityAsElement) return false;
					bEntity.OnEntityRefresh();
				}

				break;
			}
		}

		return success;
	}


	/// <summary>
	/// Find an empty place on map nearby given position for placing an entity
	/// </summary>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="resultUnitX">Position founded in unit space</param>
	/// <param name="resultUnitY">Position founded in unit space</param>
	/// <param name="maxRange">Maximal searching range in unit space</param>
	/// <param name="preferNoSolidLevel">Set to true to put this entity into place without solid level blocks</param>
	/// <returns>True if the place is founded</returns>
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


	/// <summary>
	/// Remove target entity from world data. Only work when entity spawned from map.
	/// </summary>
	public static void RemoveFromWorldMemory (Entity entity) {
		var mapPos = entity.MapUnitPos;
		if (mapPos.HasValue) {
			WorldSquad.Front.SetBlockAt(mapPos.Value.x, mapPos.Value.y, mapPos.Value.z, BlockType.Entity, 0);
		}
	}


	/// <summary>
	/// True if any block founded inside given range. Search blocks with specific order to lower the CPU usage. 
	/// </summary>
	/// <param name="squad">Source of map blocks</param>
	/// <param name="startUnitPoint">Position to start searching in unit space</param>
	/// <param name="direction">Direction of the search operation facing. Set to null to make it search in circle range.</param>
	/// <param name="unitDistance">Maximal distance for the search in unit space</param>
	/// <param name="reverse">True if search blocks from far side first</param>
	public static bool SearchlightBlockCheck (IBlockSquad squad, Int3 startUnitPoint, Direction8? direction, int unitDistance = Const.MAP, bool reverse = false) {
		(int deltaX, int deltaY) = direction.HasValue ? direction.Value.Normal() : Int2.Zero;
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


	/// <summary>
	/// Get FittingPose for given position and entity ID from WorldSquad. 
	/// eg. 3 mushroom entities makes a tall mushroom, only the top one is the cap. So your entity renders as cap only when the fitting pose is FittingPose.Up.
	/// </summary>
	/// <param name="typeID">Type of the target entity</param>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="horizontal">True if the entity fits horizontaly</param>
	public static FittingPose GetEntityPose (int typeID, int unitX, int unitY, bool horizontal) {
		bool n = WorldSquad.Front.GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
		bool p = WorldSquad.Front.GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
		return
			n && p ? FittingPose.Mid :
			!n && p ? FittingPose.Left :
			n && !p ? FittingPose.Right :
			FittingPose.Single;
	}


	/// <summary>
	/// Get FittingPose for given entity type from WorldSquad and stage. 
	/// </summary>
	/// <param name="entity">Target entity</param>
	/// <param name="horizontal">True if the entity fits horizontaly</param>
	/// <param name="mask">Physics layers to get the entity instance</param>
	/// <param name="left_down">Nearby entity instance at left/down</param>
	/// <param name="right_up">Nearby entity instance at right/up</param>
	public static FittingPose GetEntityPose (Entity entity, bool horizontal, int mask, out Entity left_down, out Entity right_up) {
		left_down = null;
		right_up = null;
		int unitX = entity.X.ToUnit();
		int unitY = entity.Y.ToUnit();
		int typeID = entity.TypeID;
		bool n = WorldSquad.Front.GetBlockAt(horizontal ? unitX - 1 : unitX, horizontal ? unitY : unitY - 1, BlockType.Entity) == typeID;
		bool p = WorldSquad.Front.GetBlockAt(horizontal ? unitX + 1 : unitX, horizontal ? unitY : unitY + 1, BlockType.Entity) == typeID;
		if (n) {
			left_down = Physics.GetEntity(typeID, entity.Rect.EdgeOutside(horizontal ? Direction4.Left : Direction4.Down), mask, entity, OperationMode.ColliderAndTrigger);
		}
		if (p) {
			right_up = Physics.GetEntity(typeID, entity.Rect.EdgeOutside(horizontal ? Direction4.Right : Direction4.Up), mask, entity, OperationMode.ColliderAndTrigger);
		}
		return
			n && p ? FittingPose.Mid :
			!n && p ? FittingPose.Left :
			n && !p ? FittingPose.Right :
			FittingPose.Single;
	}


	/// <summary>
	/// Find all world position that overlap the given range. (256 results in maximal)
	/// </summary>
	/// <param name="overlapUnitRange">Target range in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="count">How many result is founded</param>
	/// <returns>Array that holds the result</returns>
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


	/// <summary>
	/// Find all existing world position that overlap the given range. (512 results in maximal)
	/// </summary>
	/// <param name="squad">Source of the world instance</param>
	/// <param name="overlapUnitRange">Target range in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="count">How many result is founded</param>
	/// <returns>Array that holds the result</returns>
	public static Int3[] ForAllExistsWorldInRange (IBlockSquad squad, IRect overlapUnitRange, int z, out int count) => ForAllExistsWorldInRangeLogic(squad, overlapUnitRange, z, out count, false, out _);
	public static Int3[] ForAllExistsWorldInRangeWithNotExistsInEnd (IBlockSquad squad, IRect overlapUnitRange, int z, out int count, out int notExistsCount) => ForAllExistsWorldInRangeLogic(squad, overlapUnitRange, z, out count, true, out notExistsCount);
	private static Int3[] ForAllExistsWorldInRangeLogic (IBlockSquad squad, IRect overlapUnitRange, int z, out int count, bool fillNotExistsToEnd, out int notExistsCount) {
		int left = overlapUnitRange.xMin.UDivide(Const.MAP);
		int right = (overlapUnitRange.xMax + 1).UDivide(Const.MAP);
		int down = overlapUnitRange.yMin.UDivide(Const.MAP);
		int up = (overlapUnitRange.yMax + 1).UDivide(Const.MAP);
		int index = 0;
		notExistsCount = 0;
		int endIndex = WorldPosInViewCache.Length - 1;
		int maxCount = WorldPosInViewCache.Length;
		for (int i = left; i <= right; i++) {
			for (int j = down; j <= up; j++) {
				var worldPos = new Int3(i, j, z);
				if (squad.WorldExists(worldPos)) {
					WorldPosInViewCache[index] = worldPos;
					index++;
					if (index >= maxCount) {
						goto _END_;
					}
				} else if (fillNotExistsToEnd) {
					WorldPosInViewCache[endIndex] = worldPos;
					notExistsCount++;
					endIndex--;
					maxCount--;
					if (index >= maxCount) {
						goto _END_;
					}
				}
			}
		}
		_END_:;
		count = index;
		return WorldPosInViewCache;
	}


	/// <summary>
	/// Paint the map block with BlockColoringSystem at given position
	/// </summary>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="blockColorID">ID of BlockColor's subclass as a map element</param>
	/// <param name="overrideExistingElement">True if existing map element at given position will be override</param>
	public static void PaintBlock (int unitX, int unitY, int blockColorID, bool overrideExistingElement = false) {
		var squad = WorldSquad.Front;
		var (lv, bg, _, ele) = squad.GetAllBlocksAt(unitX, unitY, Stage.ViewZ);
		if (lv == 0 && bg == 0) return;
		if (!overrideExistingElement && !BlockColoringSystem.TryGetColor(ele, out _) && ele != 0) return;
		squad.SetBlockAt(unitX, unitY, Stage.ViewZ, BlockType.Element, blockColorID);
	}


	/// <summary>
	/// Move the given rigidbody to closest empty space nearby
	/// </summary>
	/// <param name="rig"></param>
	/// <param name="collisionMask">Which physics layers should count as "Ground"</param>
	/// <param name="unitRange">How far can it move in unit space</param>
	/// <param name="speed">How fast will it move</param>
	public static void TryEjectOutsideGround (Rigidbody rig, int collisionMask = 0, int unitRange = 2, int speed = 32) {
		var centerPos = rig.Rect.CenterInt();
		var targetDir = Int2.Zero;
		collisionMask = collisionMask == 0 ? rig.CollisionMask : collisionMask;
		if (CheckForDir(centerPos, Int2.Up, unitRange, collisionMask)) {
			// Up
			targetDir = Int2.Up;
		} else if (CheckForDir(centerPos, Int2.Right, unitRange, collisionMask)) {
			// Right
			targetDir = Int2.Right;
		} else if (CheckForDir(centerPos, Int2.Left, unitRange, collisionMask)) {
			// Left
			targetDir = Int2.Left;
		} else if (CheckForDir(centerPos, Int2.Down, unitRange, collisionMask)) {
			// Down
			targetDir = Int2.Down;
		}
		// Perform Eject
		if (targetDir != Int2.Zero) {
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


	// Stage Reposition
	internal static int GetRepositionElementCode (int deltaGlobalX, int deltaGlobalY) {
		const int MAX_RANGE = 8 * Const.CEL;
		deltaGlobalX = deltaGlobalX.Clamp(-MAX_RANGE, MAX_RANGE) + MAX_RANGE;
		deltaGlobalY = deltaGlobalY.Clamp(-MAX_RANGE, MAX_RANGE) + MAX_RANGE;
		return REPOS_BASIC_ID | (deltaGlobalX << 12) | deltaGlobalY;
	}


	internal static bool TryGetRepositionElementDelta (int elementCode, out int deltaGlobalX, out int deltaGlobalY) {
		const int MAX_RANGE = 8 * Const.CEL;
		deltaGlobalX = 0;
		deltaGlobalY = 0;
		if ((REPOS_BASIC_ID >> 24) != (elementCode >> 24)) return false;
		deltaGlobalX = (int)(((uint)(elementCode << 8)) >> 20) - MAX_RANGE;
		deltaGlobalY = (int)(((uint)(elementCode << 20)) >> 20) - MAX_RANGE;
		return true;
	}


	internal static bool IsRepositionElementCode (int elementCode) => (REPOS_BASIC_ID >> 24) == (elementCode >> 24);


	// System Number
	/// <summary>
	/// Convert system number ID into the number it represents
	/// </summary>
	public static int SystemNumberID_to_Number (int id) => SYSTEM_NUMBER_POOL.TryGetValue(id, out int number) ? number : -1;


	/// <summary>
	/// Convert number into system number map element ID. 0 by default.
	/// </summary>
	public static int Number_to_SystemNumberID (int number) => SYSTEM_NUMBER_POOL_ALT.TryGetValue(number, out int id) ? id : 0;


	/// <summary>
	/// Get system number at given position from map. (Left-to-right then up-to-down)
	/// </summary>
	/// <param name="squad">Source of the map blocks</param>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="number">Result number</param>
	/// <returns>True if the number is founded</returns>
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


	/// <summary>
	/// Get system number at given position from map in specified direction
	/// </summary>
	/// <param name="squad">Source of the map blocks</param>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="direction">Which direction should it reads</param>
	/// <param name="number">Result number</param>
	/// <returns>True if the number is founded</returns>
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


	/// <summary>
	/// True if there is system number at given position
	/// </summary>
	/// <param name="squad">Source of the map blocks</param>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="z">Position Z</param>
	public static bool HasSystemNumber (IBlockSquad squad, int unitX, int unitY, int z) {
		int id = squad.GetBlockAt(unitX, unitY, z, BlockType.Element);
		return id != 0 && SystemNumberID_to_Number(id) != -1;
	}


	/// <summary>
	/// Get a single digit of system number at given position from map
	/// </summary>
	/// <param name="squad">Source of the map blocks</param>
	/// <param name="unitX">Target position X in unit space</param>
	/// <param name="unitY">Target position Y in unit space</param>
	/// <param name="z">Position Z</param>
	/// <param name="digitValue">Result of the digit</param>
	/// <returns>True if the digit is founded</returns>
	public static bool TryGetSingleSystemNumber (IBlockSquad squad, int unitX, int unitY, int z, out int digitValue) {
		int id = squad.GetBlockAt(unitX, unitY, z, BlockType.Element);
		digitValue = SystemNumberID_to_Number(id);
		return digitValue >= 0;
	}


	// Block Aiming
	/// <summary>
	/// Get aiming position for block building target with mouse
	/// </summary>
	/// <param name="holder">Character that using the tool</param>
	/// <param name="blockType">Type of building block</param>
	/// <param name="targetUnitX">Result position in unit space</param>
	/// <param name="targetUnitY">Result position in unit space</param>
	/// <param name="requireEmbedAsElement">True if this block can be embed into other block</param>
	/// <returns>True if the target is founded</returns>
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


	/// <summary>
	/// Get aiming position for block building target with keyboard keys
	/// </summary>
	/// <param name="holder">Character that using the tool</param>
	/// <param name="blockType">Type of building block</param>
	/// <param name="targetUnitX">Result position in unit space</param>
	/// <param name="targetUnitY">Result position in unit space</param>
	/// <param name="requireEmbedAsElement">True if this block can be embed into other block</param>
	/// <param name="ignoreValid">Set to true to skip block building validation</param>
	/// <returns>True if valid position is founded</returns>
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
		// Check Overlapping Level Block
		var rect = new IRect(unitX.ToGlobal(), unitY.ToGlobal(), Const.CEL, Const.CEL).Shrink(64);
		if (Physics.Overlap(
			PhysicsMask.LEVEL, rect, null, OperationMode.ColliderOnly
		)) return false;

		// Check Overlapping Entity
		var hits = Physics.OverlapAll(PhysicsMask.ENTITY, rect, out int count, null, OperationMode.ColliderAndTrigger);
		for (int i = 0; i < count; i++) {
			var hit = hits[i];
			if (hit.Entity is not IBlockEntity bEntity) return false;
			if (!bEntity.EmbedEntityAsElement) return false;
			requireEmbedAsElement = true;
		}

		return true;
	}


	/// <summary>
	/// Get aiming position for block picking target with mouse
	/// </summary>
	/// <param name="holder">Character that using the tool</param>
	/// <param name="unitRange">Range limitation in unit space</param>
	/// <param name="targetUnitX">Result position in unit space</param>
	/// <param name="targetUnitY">Result position in unit space</param>
	/// <param name="inRange">True if the current mouse cursor is in range</param>
	/// <param name="allowPickBlockEntity">True if the tool can pick entity blocks</param>
	/// <param name="allowPickLevelBlock">True if the tool can pick level blocks</param>
	/// <param name="allowPickBackgroundBlock">True if the tool can pick background blocks</param>
	/// <returns>True if valid position is founded</returns>
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


	/// <summary>
	/// Get aiming position for block picking target with keyboard keys
	/// </summary>
	/// <param name="pHolder">Character that using the tool</param>
	/// <param name="targetUnitX">Result position in unit space</param>
	/// <param name="targetUnitY">Result position in unit space</param>
	/// <param name="allowPickBlockEntity">True if the tool can pick entity blocks</param>
	/// <param name="allowPickLevelBlock">True if the tool can pick level blocks</param>
	/// <param name="allowPickBackgroundBlock">True if the tool can pick background blocks</param>
	/// <returns>True if valid position is founded</returns>
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


}
