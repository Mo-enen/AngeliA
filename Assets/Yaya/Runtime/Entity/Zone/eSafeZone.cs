using UnityEngine;
namespace Yaya {
	public class eSafeZone : eZone {
		protected override RectInt? Range { get => _Range; set => _Range = value; }
		private static RectInt? _Range = null;
		protected override void PerformRange (RectInt range) {
			var player = ePlayer.Selecting;
			if (player == null) return;
			if (range.Overlaps(player.Rect)) player.MakeSafe(1);
		}
	}
}