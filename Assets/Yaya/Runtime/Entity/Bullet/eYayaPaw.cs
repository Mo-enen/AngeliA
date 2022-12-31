using System.Collections;
using System.Collections.Generic;
using AngeliaFramework;
using UnityEngine;


namespace Yaya {
	[EntityAttribute.Capacity(12)]
	public class eYayaPaw : ePlayerBullet {


		// Const
		private static readonly int[] PAW_X = new int[] { -128, -96, 0, -16, -32, };
		private static readonly int[] PAW_Y = new int[] { 512, 480, 0, -64, -48, };
		private static readonly int[] PAW_SIZE = new int[] { 1200, 1600, 1800, 1400, 1000, };
		private static readonly int[] PAW_ROT = new int[] { -30, -15, 0, 15, 30, };

		// Api
		protected override bool DestroyOnCollide => false;
		protected override bool DestroyOnHit => false;
		protected override int Duration => 10;
		protected override int Speed => 0;

		// Data
		private bool Hitted = false;
		private bool FacingRight = false;
		private bool Grounded = true;
		private eCharacter Character = null;

		// MSG
		public override void Release (eCharacter character, Vector2Int direction, int combo, int chargeDuration) {
			base.Release(character, direction, combo, chargeDuration);
			Hitted = false;
			Width = Const.CEL;
			Height = Const.CEL * 2;
			Character = character;
			FacingRight = character.FacingRight;
			var characterRect = character.Rect;
			X = character.FacingRight ? characterRect.xMax : characterRect.xMin - Width;
			Y = character.Y - 1;
			Grounded =
				CellPhysics.Overlap(YayaConst.MASK_MAP, Rect.Edge(Direction4.Down, 4), this) ||
				CellPhysics.Overlap(YayaConst.MASK_MAP, Rect.Edge(Direction4.Down, 4), this, OperationMode.TriggerOnly, Const.ONEWAY_UP_TAG);
		}


		public override void FillPhysics () {
			if (!Hitted && Game.GlobalFrame - StartFrame >= 4) {
				YayaCellPhysics.FillEntity_Damage(this, false, Damage);
			}
		}


		public override void FrameUpdate () {

			var characterRect = Character.Rect;
			X = Character.FacingRight ? characterRect.xMax : characterRect.xMin - Width;
			Y = Character.Y - 1;


			int localFrame = (Game.GlobalFrame - StartFrame) / 2;
			localFrame = localFrame.Clamp(0, PAW_Y.Length - 1);
			int spriteFrame = localFrame;
			int rot = PAW_ROT[localFrame];
			if (!Grounded) {
				spriteFrame = spriteFrame.Clamp(0, 1);
			} else {
				rot = PAW_ROT[localFrame.Clamp(0, 2)];
			}
			if (CellRenderer.TryGetSpriteFromGroup(TypeID, spriteFrame, out var sprite, false, true)) {
				CellRenderer.Draw(
					sprite.GlobalID,
					X + Width / 2 + (FacingRight ? PAW_X[localFrame] : -PAW_X[localFrame]),
					Y + PAW_Y[localFrame],
					500, 0, FacingRight ? rot : -rot,
					(FacingRight ? sprite.GlobalWidth : -sprite.GlobalWidth) * PAW_SIZE[localFrame] / 1000,
					sprite.GlobalHeight * PAW_SIZE[localFrame] / 1000
				).Z = int.MaxValue;
			}
		}


		public override void OnHit (IDamageReceiver receiver) => Hitted = true;


	}
}