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

	public class CheckLalynnA : eCheckPoint { }
	public class CheckMage : eCheckPoint { }
	public class CheckElf : eCheckPoint { }
	public class CheckDragon : eCheckPoint { }
	public class CheckTorch : eCheckPoint { }
	public class CheckSlime : eCheckPoint { }
	public class CheckInsect : eCheckPoint { }
	public class CheckOrc : eCheckPoint { }
	public class CheckTako : eCheckPoint { }
	public class CheckShark : eCheckPoint { }
	public class CheckBone : eCheckPoint { }
	public class CheckFootman : eCheckPoint { }
	public class CheckKnight : eCheckPoint { }
	public class CheckJesus : eCheckPoint { }
	public class CheckShield : eCheckPoint { }
	public class CheckGamble : eCheckPoint { }
	public class CheckScience : eCheckPoint { }
	public class CheckSpider : eCheckPoint { }
	public class CheckStalactite : eCheckPoint { }
	public class CheckSword : eCheckPoint { }
	public class CheckSpace : eCheckPoint { }
	public class CheckMachineGun : eCheckPoint { }
	public class CheckKnowledge : eCheckPoint { }
	public class CheckCat : eCheckPoint { }


	[EntityAttribute.Capacity(1)]
	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.MapEditorGroup("Check Point")]
	public abstract class eCheckAltar : Entity, IInitialize {


		// SUB
		[System.Serializable]
		public class CheckPointMeta {
			[System.Serializable]
			public struct Data {
				public int X; // Global Unit Pos
				public int Y; // Global Unit Pos
				public int Z; // Global Unit Pos
				public int EntityID;
			}
			public Data[] CPs = null;
		}


		// VAR
		public static Dictionary<int, Vector3Int> AltarPositionPool { get; } = new();
		private Int4 Border = default;


		// MSG
		public static void Initialize () {
			AltarPositionPool.Clear();
			var cpMeta = AngeUtil.LoadMeta<CheckPointMeta>();
			if (cpMeta != null) {
				foreach (var cpData in cpMeta.CPs) {
					AltarPositionPool.TryAdd(cpData.EntityID, new Vector3Int(cpData.X, cpData.Y, cpData.Z));
				}
			}
		}


		public static bool TryGetAltarPosition (int id, out Vector3Int globalPos) {
			if (AltarPositionPool.TryGetValue(id, out var unitPos)) {
				globalPos = new(unitPos.x * Const.CEL, unitPos.y * Const.CEL, unitPos.z);
				return true;
			} else {
				globalPos = default;
				return false;
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


	[EntityAttribute.Capacity(1)]
	[EntityAttribute.MapEditorGroup("Check Point")]
	public abstract class eCheckPoint : Entity {



		private Int4 Border = default;


		public override void OnActived () {
			base.OnActived();
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