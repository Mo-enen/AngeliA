using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class LeafMaple : Leaf { }


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class LeafPine : Leaf { }


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class LeafPoplar : Leaf { }


	[EntityAttribute.Bounds(-Const.CEL, -Const.CEL, Const.CEL * 3, Const.CEL * 3)]
	public class LeafPalm : Leaf {

		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillBlock(PhysicsLayer.ENVIRONMENT, TypeID, Rect, true, Const.ONEWAY_UP_TAG);
		}

		public override void FrameUpdate () {
			CellRenderer.Draw(LeafArtworkCode, base.Rect.Shift(0, GetLeafShiftY(-24)));
		}

	}


	public class LeafWillow : Leaf {

		public override void FillPhysics () {
			base.FillPhysics();
			CellPhysics.FillBlock(
				PhysicsLayer.ENVIRONMENT, TypeID,
				Rect.Shrink(0, 0, 0, Height / 2),
				true, Const.CLIMB_TAG
			);
		}

		public override void FrameUpdate () => CellRenderer.Draw(LeafArtworkCode, base.Rect.Shift(GetLeafShiftY(Y, 120, 12), 0));

	}


	[EntityAttribute.MapEditorGroup("Vegetation")]
	[EntityAttribute.Capacity(1024)]
	public abstract class Leaf : EnvironmentEntity, ICombustible, IDamageReceiver {




		#region --- VAR ---


		// Const
		private static readonly int[] LEAF_OFFSET_SEEDS = new int[] { 0, 6, 2, 8, 3, 7, 2, 3, 5, 2, 2, 6, 9, 3, 6, 1, 9, 0, 1, 7, 4, 2, 8, 4, 6, 5, 2, 4, 8, 7, };
		private const byte LEAF_HIDE_ALPHA = 42;

		// Virtual
		protected virtual int LeafCount => 3;
		protected virtual int LeafExpand => Const.CEL / 3;
		protected int LeafArtworkCode { get; private set; } = 0;
		int ICombustible.BurnStartFrame { get; set; }
		int IDamageReceiver.Team => Const.TEAM_ENVIRONMENT;

		// Data
		private Pixel32 LeafTint = new(255, 255, 255, 255);
		private bool CharacterNearby = false;


		#endregion




		#region --- MSG ---


		public override void OnActivated () {
			base.OnActivated();
			Width = Const.CEL;
			Height = Const.CEL;
			// Leaf
			LeafArtworkCode = CellRenderer.TryGetSpriteFromGroup(
				TypeID, (X * 5 + Y * 7) / Const.CEL, out var lSprite
			) ? lSprite.GlobalID : TypeID;
		}


		public override void FillPhysics () => CellPhysics.FillEntity(PhysicsLayer.ENVIRONMENT, this, true);


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			CharacterNearby = CellPhysics.HasEntity<Character>(Rect.Expand(Const.CEL), PhysicsMask.CHARACTER, null);
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			// Leaf
			LeafTint.a = (byte)Mathf.Lerp(LeafTint.a, CharacterNearby ? LEAF_HIDE_ALPHA : 255, 0.1f);
			int sLen = LEAF_OFFSET_SEEDS.Length;
			for (int i = 0; i < LeafCount; i++) {
				int seedX = LEAF_OFFSET_SEEDS[(i + X / Const.CEL).UMod(sLen)];
				int seedY = LEAF_OFFSET_SEEDS[(i + Y / Const.CEL).UMod(sLen)];
				var offset = new Int2(
					((X * 137 * seedX + Y * 327 * seedY) / Const.CEL).UMod(Const.CEL) - Const.HALF,
					((X * 149 * seedX + Y * 177 * seedY) / Const.CEL).UMod(Const.CEL) - Const.HALF
				);
				DrawLeaf(offset, 12 * i, LeafExpand, offset.x % 2 == 0);
			}
			// Func
			void DrawLeaf (Int2 offset, int frameOffset, int expand, bool flipX = false) {
				var rect = Rect.Shift(offset.x, GetLeafShiftY(frameOffset) + offset.y).Expand(expand);
				if (flipX) {
					rect.x += rect.width;
					rect.width = -rect.width;
				}
				CellRenderer.Draw(LeafArtworkCode, rect, LeafTint);
			}
		}


		protected virtual void OnBreak () { }


		#endregion




		#region --- API ---


		protected int GetLeafShiftY (int frameOffset = 0, int duration = 60, int amount = 12) {
			return ((Game.GlobalFrame + (X / Const.CEL) + frameOffset).PingPong(duration) * amount / duration) - (amount / 2);
		}


		public void TakeDamage (int damage, Entity sender) {
			if (damage <= 0) return;
			// Particle
			int id = TypeID;
			var rect = Rect;
			if (CellRenderer.TryGetSprite(LeafArtworkCode, out var sprite)) {
				id = sprite.GlobalID;
				rect.height = sprite.GlobalHeight;
				if (rect.width != sprite.GlobalWidth) {
					rect.x -= (sprite.GlobalWidth - rect.width) / 2;
					rect.width = sprite.GlobalWidth;
				}
			}
			BreakingParticle.SpawnParticles(id, rect, true);
			Stage.MarkAsLocalAntiSpawn(this);
			// Disable
			Active = false;
			OnBreak();
		}


		#endregion




	}
}