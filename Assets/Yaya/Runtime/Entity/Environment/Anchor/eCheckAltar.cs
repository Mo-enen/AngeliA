using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {


	public class AltarLalynnA : eCheckAltar { }
	public class AltarMage : eCheckAltar { }
	public class AltarElf : eCheckAltar { }
	public class AltarDragon : eCheckAltar { }
	public class AltarTorch : eCheckAltar { }
	public class AltarSlime : eCheckAltar { }
	public class AltarInsect : eCheckAltar { }
	public class AltarOrc : eCheckAltar { }
	public class AltarTako : eCheckAltar { }
	public class AltarShark : eCheckAltar { }
	public class AltarBone : eCheckAltar { }
	public class AltarFootman : eCheckAltar { }
	public class AltarKnight : eCheckAltar { }
	public class AltarJesus : eCheckAltar { }
	public class AltarShield : eCheckAltar { }
	public class AltarGamble : eCheckAltar { }
	public class AltarScience : eCheckAltar { }
	public class AltarSpider : eCheckAltar { }
	public class AltarStalactite : eCheckAltar { }
	public class AltarSword : eCheckAltar { }
	public class AltarSpace : eCheckAltar { }
	public class AltarMachineGun : eCheckAltar { }
	public class AltarKnowledge : eCheckAltar { }
	public class AltarCat : eCheckAltar { }





	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.MapEditorGroup("Altar")]
	public abstract class eCheckAltar : Entity {



		private Int4 Border = default;



		// SUB
		[System.Serializable]
		public class AltarMeta {
			[System.Serializable]
			public struct Position {
				public int X; // Global Unit Pos
				public int Y; // Global Unit Pos
				public int Z; // Global Unit Pos
				public int EntityID;
			}
			public Position[] Positions = null;
		}


		// VAR
		public static readonly Dictionary<int, Vector3Int> PositionPool = new();


		// API
		public static bool TryGetGlobalPosition (int id, out Vector3Int globalPos) {
			if (PositionPool.TryGetValue(id, out var unitPos)) {
				globalPos = new(unitPos.x * Const.CEL, unitPos.y * Const.CEL, unitPos.z);
				return true;
			} else {
				globalPos = default;
				return false;
			}
		}


		// MSG
		[AfterGameInitialize]
		public static void Initialize () {
			PositionPool.Clear();
			var meta = AngeUtil.LoadJson<AltarMeta>(Const.MetaRoot);
			if (meta == null) return;
			foreach (var cpData in meta.Positions) {
				PositionPool.TryAdd(
					cpData.EntityID,
					new Vector3Int(cpData.X, cpData.Y, cpData.Z)
				);
			}
		}


		public override void OnActived () {
			base.OnActived();
			Height = Const.CEL * 2;
			Border = default;
			if (CellRenderer.TryGetSprite(TypeID, out var sprite)) {
				Border = sprite.GlobalBorder;
			}
		}


		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillEntity(YayaConst.LAYER_ENVIRONMENT, this, true);
			CellPhysics.FillBlock(YayaConst.LAYER_ENVIRONMENT, TypeID, Rect.Shrink(Border), true, Const.ONEWAY_UP_TAG);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			CellRenderer.Draw(TypeID, Rect);
		}


	}
}