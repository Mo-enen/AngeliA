using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace AngeliaFramework {
	public class QuickSmokeSmallParticle : Particle {
		public override int Duration => 10;
		public override bool Loop => false;
		public override void DrawParticle () {
			base.DrawParticle();
			int smokeDuration = Duration - 4;
			int smokeFrame = (Game.GlobalFrame - SpawnFrame - 4).GreaterOrEquelThanZero();
			int _smokeFrame = smokeDuration * smokeDuration - (smokeDuration - smokeFrame) * (smokeDuration - smokeFrame);
			var tint = Tint;
			tint.a = (byte)Util.Remap(0, smokeDuration, 512, 0, smokeFrame);
			var cell = CellRenderer.Draw(
				TypeID, X, Y,
				500, 500, (smokeFrame + Game.GlobalFrame) * 12,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
				tint, int.MaxValue - 1
			);
			cell.X += (Width > 0 ? _smokeFrame : -_smokeFrame) * 6;
			cell.Y += cell.Height / 2;
			cell.Width = Util.Remap(0, smokeDuration, cell.Width, cell.Width * 2, smokeFrame);
			cell.Height = Util.Remap(0, smokeDuration, cell.Height, cell.Height * 2, smokeFrame);
		}
	}
	public class QuickSmokeBigParticle : Particle {
		public override int Duration => 10;
		public override bool Loop => false;
		public override void DrawParticle () {
			base.DrawParticle();
			int smokeDuration = Duration - 4;
			int smokeFrame = (Game.GlobalFrame - SpawnFrame - 4).GreaterOrEquelThanZero();
			int _smokeFrame = smokeDuration * smokeDuration - (smokeDuration - smokeFrame) * (smokeDuration - smokeFrame);
			var tint = Tint;
			tint.a = (byte)Util.Remap(0, smokeDuration, 512, 0, smokeFrame);
			var cell = CellRenderer.Draw(
				TypeID, X, Y,
				500, 500, (smokeFrame + Game.GlobalFrame) * 12,
				Const.ORIGINAL_SIZE, Const.ORIGINAL_SIZE,
				tint, int.MaxValue - 1
			);
			cell.X += (Width > 0 ? _smokeFrame : -_smokeFrame) * 6;
			cell.Y += cell.Height / 2;
			cell.Width = Util.Remap(0, smokeDuration, cell.Width, cell.Width * 2, smokeFrame);
			cell.Height = Util.Remap(0, smokeDuration, cell.Height, cell.Height * 2, smokeFrame);
		}
	}
}
