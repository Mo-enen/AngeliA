using System.Collections;
using System.Collections.Generic;


namespace AngeliaFramework {
	public class ArrowBullet : MovableBullet {

		public int ArrowItemID { get; set; } = 0;
		public int ArrowArtworkID { get; set; } = 0;
		protected override int ArtworkID => ArrowArtworkID;

		public override void OnActivated () {
			base.OnActivated();
			ArrowItemID = 0;
			ArrowArtworkID = 0;
		}

		protected override void SpawnResidue (IDamageReceiver receiver) {
			base.SpawnResidue(receiver);
			if (ArrowItemID != 0) {
				ItemSystem.SpawnItem(ArrowItemID, X, Y, 1);
			}
		}

	}
}