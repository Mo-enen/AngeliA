using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaGame {

	// Tail
	public class FubukiTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int FrameLen => 419;
	}
	public class MioTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int FrameLen => 619;
	}
	public class ChocoTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class TowaTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class OkayuTail : AutoSpriteTail { }
	public class KoroneTail : AutoSpriteTail { }
	public class PekoraTail : AutoSpriteTail {
		protected override void DrawTail (Character character) {
			if (!CellRenderer.TryGetSpriteFromGroup(SpriteGroupID, 0, out var sprite)) return;
			int w = sprite.GlobalWidth;
			int h = sprite.GlobalHeight;
			int x = character.Body.GlobalX - w / 2;
			int y = character.Body.GlobalY;
			int z = character.Body.FrontSide ? -33 : 33;
			CellRenderer.Draw(sprite.GlobalID, x, y, 500, 500, 0, w, h, z);
		}
	}
	public class CocoTail : AutoSpriteTail {
		protected override int LimbGrow => 200;
		protected override int AngleAmountRoot => 100;
		protected override int AngleAmountSubsequent => 300;
		protected override int AngleOffset => 45;
	}
	public class BotanTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class PolkaTail : AutoSpriteTail { }
	public class RisuTail : AutoSpriteTail { }
	public class GuraTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 200;
		protected override int AngleAmountSubsequent => 100;
		protected override int AngleOffset => 45;
	}
	public class BaeTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class LaplusTail : AutoSpriteTail {
		protected override int LimbGrow => 500;
		protected override int AngleAmountRoot => 800;
		protected override int AngleAmountSubsequent => 500;
	}
	public class KoyoriTail : AutoSpriteTail { }
	public class FuwaMocoTail : AutoSpriteTail { }
	public class AloeTail : AutoSpriteTail { }


}
