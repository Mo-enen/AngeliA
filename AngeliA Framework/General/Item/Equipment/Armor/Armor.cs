using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	public abstract class Armor<P, N> : Equipment, IProgressiveItem where P : Equipment where N : Equipment {


		// VAR
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

		private readonly int[] SpritesID = new int[8];
		private readonly int[] RepairMaterialsID;
		private readonly bool IsSkirt = false;


		// MSG
		public Armor () {
			var progItem = this as IProgressiveItem;
			progItem.PrevItemID = typeof(P).AngeHash();
			progItem.NextItemID = typeof(N).AngeHash();
			progItem.Progress = GetProgress(GetType(), out int totalProgressive);
			progItem.TotalProgress = totalProgressive;
			if (progItem.PrevItemID == TypeID) progItem.PrevItemID = 0;
			if (progItem.NextItemID == TypeID) progItem.NextItemID = 0;
			RepairMaterialsID = AngeUtil.GetAngeHashs(RepairMaterials);
			string basicName = GetType().AngeName();
			switch (EquipmentType) {
				case EquipmentType.BodyArmor:
					IsSkirt = false;
					SpritesID[0] = SpritesID[7] = $"{basicName}.Body".AngeHash();
					SpritesID[1] = $"{basicName}.Hip".AngeHash();
					SpritesID[2] = $"{basicName}.Shoulder".AngeHash();
					SpritesID[3] = $"{basicName}.UpperArm".AngeHash();
					SpritesID[4] = $"{basicName}.LowerArm".AngeHash();
					SpritesID[5] = $"{basicName}.UpperLeg".AngeHash();
					SpritesID[6] = $"{basicName}.LowerLeg".AngeHash();
					if (!CellRenderer.HasSpriteGroup(SpritesID[0])) {
						SpritesID[0] = $"{basicName}.BodyL".AngeHash();
						SpritesID[7] = $"{basicName}.BodyR".AngeHash();
					}
					if (!CellRenderer.HasSpriteGroup(SpritesID[1]) && !CellRenderer.HasSprite(SpritesID[1])) {
						SpritesID[1] = $"{basicName}.Skirt".AngeHash();
						IsSkirt = true;
					}
					break;
				case EquipmentType.Helmet:
					SpritesID[0] = $"{basicName}.Main".AngeHash();
					SpritesID[1] = $"{basicName}.Back".AngeHash();
					break;
				case EquipmentType.Shoes:
					SpritesID[0] = $"{basicName}.Main".AngeHash();
					break;
				case EquipmentType.Gloves:
					SpritesID[0] = $"{basicName}.Main".AngeHash();
					break;
			}
			for (int i = 0; i < SpritesID.Length; i++) {
				int id = SpritesID[i];
				if (id != 0 && !CellRenderer.HasSprite(id) && !CellRenderer.HasSpriteGroup(id)) SpritesID[i] = 0;
			}
		}


		public override void PoseAnimationUpdate_FromEquipment (Entity holder) {
			base.PoseAnimationUpdate_FromEquipment(holder);

			if (
				holder is not Character character ||
				character.RenderWithSheet ||
				character.AnimatedPoseType == CharacterPoseAnimationType.Sleep ||
				character.AnimatedPoseType == CharacterPoseAnimationType.PassOut
			) return;

			// Draw Armor
			switch (EquipmentType) {
				case EquipmentType.BodyArmor:
					DrawBodyArmor(character);
					break;
				case EquipmentType.Helmet:
					DrawHelmet(character);
					break;
				case EquipmentType.Shoes:
					DrawShoes(character);
					break;
				case EquipmentType.Gloves:
					DrawGloves(character);
					break;
			}

			// Hide Gadget
			if (HideEar) character.IgnoreBodyGadget(BodyGadgetType.Ear);
			if (HideHorn) character.IgnoreBodyGadget(BodyGadgetType.Horn);
			if (HideHair) character.IgnoreBodyGadget(BodyGadgetType.Hair);
			if (HideTail) character.IgnoreBodyGadget(BodyGadgetType.Tail);
			if (HideFace) character.IgnoreBodyGadget(BodyGadgetType.Face);
			if (HideWing) character.IgnoreBodyGadget(BodyGadgetType.Wing);

		}


		private void DrawBodyArmor (Character character) {

			int bodyId = SpritesID[0];
			int bodyIdAlt = SpritesID[7];
			int hipID = SpritesID[1];
			int shoulderID = SpritesID[2];
			int upperArmID = SpritesID[3];
			int lowerArmID = SpritesID[4];
			int upperLegID = SpritesID[5];
			int lowerLegID = SpritesID[6];

			// Body
			if (bodyId != 0 || bodyIdAlt != 0) {
				if (CellRenderer.TryGetMeta(bodyId, out var meta) && meta.IsTrigger) {
					if (CellRenderer.TryGetSprite(bodyId, out var bodySprite)) {
						Cloth.AttachClothOn(
							character.Body, bodySprite, 500, 1000, 8,
							widthAmount: character.Body.Width > 0 ? Scale : -Scale,
							heightAmount: Scale,
							defaultHideLimb: false
						);
					}
				} else {
					BodyCloth.DrawClothForBody(character, bodyId, bodyIdAlt, 8, 200);
				}
			}

			// Hip
			if (hipID != 0) {
				if (IsSkirt) {
					HipCloth.DrawClothForSkirt(character, hipID, 7);
				} else {
					HipCloth.DrawClothForHip(character, hipID, 2);
				}
			}

			// Cape
			if (bodyId != 0 && CellRenderer.TryGetSpriteFromGroup(
				bodyId, character.Body.FrontSide ? 2 : 3, out var capeSprite, false, false
			)) {
				BodyCloth.DrawCape(character, capeSprite);
			}

			// Shoulder
			if (shoulderID != 0 && CellRenderer.TryGetSprite(shoulderID, out var shoulderSprite)) {
				Cloth.AttachClothOn(character.ShoulderL, shoulderSprite, 1000, 1000, 16);
				Cloth.AttachClothOn(character.ShoulderR, shoulderSprite, 1000, 1000, 16);
			}

			// Arm
			BodyCloth.DrawClothForUpperArm(character, upperArmID, 3);
			BodyCloth.DrawClothForLowerArm(character, lowerArmID, 3);

			// Leg
			HipCloth.DrawClothForUpperLeg(character, upperLegID, 3);
			HipCloth.DrawClothForLowerLeg(character, lowerLegID, 3);

		}


		private void DrawHelmet (Character character) {

			var head = character.Head;
			int spriteID = SpritesID[head.FrontSide ? 0 : 1];
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


		private void DrawShoes (Character character) => FootCloth.DrawClothForFoot(character, SpritesID[0], 2);


		private void DrawGloves (Character character) => HandCloth.DrawClothForHand(character, SpritesID[0], 2);


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


		// LGC
		private static int GetProgress (System.Type armorType, out int totalProgress) {
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
}