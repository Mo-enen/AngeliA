using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eCheckPoint : Entity, IInitialize {



		// Api
		public override int Layer => (int)EntityLayer.Environment;
		protected abstract int ArtCode { get; }

		// Data
		private int ArtworkCode = 0;


		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			ArtworkCode = ArtCode;
			if (CellRenderer.GetUVRect(ArtworkCode, out var rect)) {
				Width = rect.Width;
				Height = rect.Height;
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(ArtworkCode, Rect);
		}


	}
}
