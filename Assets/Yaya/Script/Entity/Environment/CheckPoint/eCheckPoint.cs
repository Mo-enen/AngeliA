using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eCheckPoint : Entity, IInitialize {



		// Api
		public override int Layer => (int)EntityLayer.Environment;
		protected abstract int ArtCode { get; }


		public override void OnCreate (int frame) {
			base.OnCreate(frame);
			if (CellRenderer.GetUVRect(ArtCode, out var rect)) {
				Width = rect.Width;
				Height = rect.Height;
			}
		}


		public override void FrameUpdate (int frame) {
			base.FrameUpdate(frame);
			CellRenderer.Draw(ArtCode, Rect);
		}


	}
}
