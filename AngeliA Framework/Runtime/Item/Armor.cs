using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {



	public abstract class Helmet<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
		public sealed override EquipmentType EquipmentType => EquipmentType.Helmet;
		private int SpriteFront { get; init; } = 0;
		private int SpriteBack { get; init; } = 0;
		public Helmet () {
			string basicName = GetType().AngeName();
			SpriteFront = $"{basicName}.Main".AngeHash();
			SpriteBack = $"{basicName}.Back".AngeHash();
			if (!CellRenderer.HasSprite(SpriteFront)) SpriteFront = 0;
			if (!CellRenderer.HasSprite(SpriteBack)) SpriteBack = 0;
		}
		protected override void DrawArmor (Character character) {

			var head = character.Head;
			int spriteID = head.FrontSide ? SpriteFront : SpriteBack;
			if (spriteID == 0 || !CellRenderer.TryGetSprite(spriteID, out var sprite)) return;
			Cell[] cells;

			// Draw Helmet
			if (CellRenderer.TryGetMeta(spriteID, out var meta) && meta.IsTrigger) {
				// Attach
				cells = Cloth.AttachClothOn(
					head, sprite, 500, 1000, 34 - head.Z, Scale, Scale, defaultHideLimb: false
				);
			} else {
				// Cover
				cells = Cloth.CoverClothOn(
					head, spriteID, 34 - head.Z, Const.WHITE, false
				);
				// Grow Padding
				if (!sprite.GlobalBorder.IsZero && cells != null) {
					var center = head.GetGlobalCenter();
					int widthAbs = head.Width.Abs();
					int heightAbs = head.Height.Abs();
					float scaleX = (widthAbs + sprite.GlobalBorder.horizontal) / (float)widthAbs.GreaterOrEquel(1);
					float scaleY = (heightAbs + sprite.GlobalBorder.vertical) / (float)heightAbs.GreaterOrEquel(1);
					foreach (var cell in cells) {
						cell.ReturnPosition(center.x, center.y);
						cell.Width = (int)(cell.Width * scaleX);
						cell.Height = (int)(cell.Height * scaleY);
					}
				}
			}

			// Head Rotate
			if (cells != null && character.HeadRotation != 0) {
				int offsetY = character.Head.Height.Abs() * character.HeadRotation.Abs() / 360;
				foreach (var cell in cells) {
					cell.RotateAround(character.HeadRotation, character.Body.GlobalX, character.Body.GlobalY + character.Body.Height);
					cell.Y -= offsetY;
				}
			}

		}
	}



	public abstract class BodyArmor<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
		public sealed override EquipmentType EquipmentType => EquipmentType.BodyArmor;
		private int BodyId { get; init; } = 0;
		private int BodyIdAlt { get; init; } = 0;
		private int HipID { get; init; } = 0;
		private int ShoulderID { get; init; } = 0;
		private int UpperArmID { get; init; } = 0;
		private int LowerArmID { get; init; } = 0;
		private int UpperLegID { get; init; } = 0;
		private int LowerLegID { get; init; } = 0;
		private readonly bool IsSkirt = false;
		public BodyArmor () {
			string basicName = GetType().AngeName();
			IsSkirt = false;
			BodyId = BodyIdAlt = $"{basicName}.Body".AngeHash();
			HipID = $"{basicName}.Hip".AngeHash();
			ShoulderID = $"{basicName}.Shoulder".AngeHash();
			UpperArmID = $"{basicName}.UpperArm".AngeHash();
			LowerArmID = $"{basicName}.LowerArm".AngeHash();
			UpperLegID = $"{basicName}.UpperLeg".AngeHash();
			LowerLegID = $"{basicName}.LowerLeg".AngeHash();
			if (!CellRenderer.HasSpriteGroup(BodyId)) {
				BodyId = $"{basicName}.BodyL".AngeHash();
				BodyIdAlt = $"{basicName}.BodyR".AngeHash();
			}
			if (!CellRenderer.HasSpriteGroup(HipID) && !CellRenderer.HasSprite(HipID)) {
				HipID = $"{basicName}.Skirt".AngeHash();
				IsSkirt = true;
			}
			if (!CellRenderer.HasSprite(BodyId) && !CellRenderer.HasSpriteGroup(BodyId)) BodyId = 0;
			if (!CellRenderer.HasSprite(BodyIdAlt) && !CellRenderer.HasSpriteGroup(BodyIdAlt)) BodyIdAlt = 0;
			if (!CellRenderer.HasSprite(HipID)) HipID = 0;
			if (!CellRenderer.HasSprite(ShoulderID)) ShoulderID = 0;
			if (!CellRenderer.HasSprite(UpperArmID)) UpperArmID = 0;
			if (!CellRenderer.HasSprite(LowerArmID)) LowerArmID = 0;
			if (!CellRenderer.HasSprite(UpperLegID)) UpperLegID = 0;
			if (!CellRenderer.HasSprite(LowerLegID)) LowerLegID = 0;
		}
		protected override void DrawArmor (Character character) {

			// Body
			if (BodyId != 0 || BodyIdAlt != 0) {
				if (CellRenderer.TryGetMeta(BodyId, out var meta) && meta.IsTrigger) {
					if (CellRenderer.TryGetSprite(BodyId, out var bodySprite)) {
						Cloth.AttachClothOn(
							character.Body, bodySprite, 500, 1000, 8,
							widthAmount: character.Body.Width > 0 ? Scale : -Scale,
							heightAmount: Scale,
							defaultHideLimb: false
						);
					}
				} else {
					BodyCloth.DrawClothForBody(character, BodyId, BodyIdAlt, 8, 200);
				}
			}

			// Hip
			if (HipID != 0) {
				if (IsSkirt) {
					HipCloth.DrawClothForSkirt(character, HipID, 7);
				} else {
					HipCloth.DrawClothForHip(character, HipID, 2);
				}
			}

			// Cape
			if (BodyId != 0 && CellRenderer.TryGetSpriteFromGroup(
				BodyId, character.Body.FrontSide ? 2 : 3, out var capeSprite, false, false
			)) {
				BodyCloth.DrawCape(character, capeSprite);
			}

			// Shoulder
			if (ShoulderID != 0 && CellRenderer.TryGetSprite(ShoulderID, out var shoulderSprite)) {
				Cloth.AttachClothOn(character.ShoulderL, shoulderSprite, 1000, 1000, 16);
				Cloth.AttachClothOn(character.ShoulderR, shoulderSprite, 1000, 1000, 16);
			}

			// Arm
			BodyCloth.DrawClothForUpperArm(character, UpperArmID, 3);
			BodyCloth.DrawClothForLowerArm(character, LowerArmID, 3);

			// Leg
			HipCloth.DrawClothForUpperLeg(character, UpperLegID, 3);
			HipCloth.DrawClothForLowerLeg(character, LowerLegID, 3);

		}
	}



	public abstract class Gloves<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
		public sealed override EquipmentType EquipmentType => EquipmentType.Gloves;
		private int SpriteID { get; init; } = 0;
		public Gloves () {
			string basicName = GetType().AngeName();
			SpriteID = $"{basicName}.Main".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID)) SpriteID = 0;
		}
		protected override void DrawArmor (Character character) => HandCloth.DrawClothForHand(character, SpriteID, 2);
	}



	public abstract class Shoes<P, N> : Armor<P, N> where P : Equipment where N : Equipment {
		public sealed override EquipmentType EquipmentType => EquipmentType.Shoes;
		private int SpriteID { get; init; } = 0;
		public Shoes () {
			string basicName = GetType().AngeName();
			SpriteID = $"{basicName}.Main".AngeHash();
			if (!CellRenderer.HasSprite(SpriteID)) SpriteID = 0;
		}
		protected override void DrawArmor (Character character) => FootCloth.DrawClothForFoot(character, SpriteID, 2);
	}



	public abstract class Armor<P, N> : Equipment, IProgressiveItem where P : Equipment where N : Equipment {

		protected virtual System.Type[] RepairMaterials => null;
		protected virtual int Scale => 1000;
		protected virtual bool HideEar => false;
		protected virtual bool HideHorn => false;
		protected virtual bool HideHair => false;
		protected virtual bool HideTail => false;
		protected virtual bool HideFace => false;
		protected virtual bool HideWing => false;
		int IProgressiveItem.Progress { get; set; } = 0;
		int IProgressiveItem.TotalProgress { get; set; } = 1;
		int IProgressiveItem.PrevItemID { get; set; } = 0;
		int IProgressiveItem.NextItemID { get; set; } = 0;

		private readonly int[] RepairMaterialsID;


		public Armor () {
			var progItem = this as IProgressiveItem;
			progItem.PrevItemID = typeof(P).AngeHash();
			progItem.NextItemID = typeof(N).AngeHash();
			progItem.Progress = GetProgress(GetType(), out int totalProgressive);
			progItem.TotalProgress = totalProgressive;
			if (progItem.PrevItemID == TypeID) progItem.PrevItemID = 0;
			if (progItem.NextItemID == TypeID) progItem.NextItemID = 0;
			RepairMaterialsID = AngeUtil.GetAngeHashs(RepairMaterials);
			// Func
			static int GetProgress (System.Type armorType, out int totalProgress) {
				int progressive = 0;
				totalProgress = 1;
				// Backward
				var type = armorType;
				for (int safe = 0; safe < 1024; safe++) {
					var genericArgs = type.BaseType.GenericTypeArguments;
					if (genericArgs.Length < 2 || genericArgs[0] == type) break;
					type = genericArgs[0];
					totalProgress++;
					progressive++;
#if UNITY_EDITOR
					if (safe == 1023) Debug.LogWarning($"Armor {armorType} is having a progressive loop.");
#endif
				}
				// Forward
				type = armorType;
				for (int safe = 0; safe < 1024; safe++) {
					var genericArgs = type.BaseType.GenericTypeArguments;
					if (genericArgs.Length < 2 || genericArgs[1] == type) break;
					type = genericArgs[1];
					totalProgress++;
#if UNITY_EDITOR
					if (safe == 1023) Debug.LogWarning($"Armor {armorType} is having a progressive loop.");
#endif
				}
				return progressive;
			}
		}


		public override void PoseAnimationUpdate_FromEquipment (Entity holder) {
			base.PoseAnimationUpdate_FromEquipment(holder);

			// Gate
			if (
				holder is not Character character ||
				character.RenderWithSheet ||
				character.AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				character.AnimatedPoseType == CharacterPoseAnimationType.PassOut
			) return;

			// Draw
			DrawArmor(character);

			// Hide Gadget
			if (HideEar) character.IgnoreBodyGadget(BodyGadgetType.Ear);
			if (HideHorn) character.IgnoreBodyGadget(BodyGadgetType.Horn);
			if (HideHair) character.IgnoreBodyGadget(BodyGadgetType.Hair);
			if (HideTail) character.IgnoreBodyGadget(BodyGadgetType.Tail);
			if (HideFace) character.IgnoreBodyGadget(BodyGadgetType.Face);
			if (HideWing) character.IgnoreBodyGadget(BodyGadgetType.Wing);

		}


		protected abstract void DrawArmor (Character character);


		public override void OnTakeDamage_FromEquipment (Entity holder, Entity sender, ref int damage) {
			base.OnTakeDamage_FromEquipment(holder, sender, ref damage);
			var progItem = this as IProgressiveItem;
			if (progItem.PrevItemID != 0 && damage > 0) {
				Inventory.SetEquipment(holder.TypeID, EquipmentType, progItem.PrevItemID);
				SpawnEquipmentDamageParticle(TypeID, progItem.PrevItemID, holder.X, holder.Y);
				damage--;
			}
		}


		public override void OnSquat (Entity holder) {
			base.OnSquat(holder);
			if ((this as IProgressiveItem).NextItemID == 0) return;
			foreach (var materialID in RepairMaterialsID) {
				if (OnRepair(holder, materialID)) {
					break;
				}
			}
		}


		public virtual bool OnRepair (Entity holder, int materialID) {
			if (materialID == 0) return false;
			int tookCount = Inventory.FindAndTakeItem(holder.TypeID, materialID, 1);
			if (tookCount <= 0) return false;
			Inventory.SetEquipment(holder.TypeID, EquipmentType, (this as IProgressiveItem).NextItemID);
			SpawnItemLostParticle(materialID, holder.X, holder.Y);
			return true;
		}


	}
}