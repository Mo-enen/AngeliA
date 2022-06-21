using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public class Attackness : EntityBehaviour<Entity>, ITxtMeta {




		#region --- VAR ---


		// Api
		public int CurrentBulletID { get; set; } = 0;

		// Ser
		[SerializeField] BuffString DefaultBulletName = new("Bullet");
		[SerializeField] BuffInt AttackColldown = new(12);

		// Data
		private int LastAttackFrame = int.MinValue;


		#endregion




		#region --- MSG ---


		public override void Initialize (Entity source) {
			base.Initialize(source);
			CurrentBulletID = DefaultBulletName.Value.AngeHash();
			LastAttackFrame = int.MinValue;
		}


		#endregion




		#region --- API ---


		// Meta
		public void LoadFromText (string text) => BuffValue.LoadBuffMetaFromText(this, text);


		public string SaveToText () => BuffValue.SaveBuffMetaToText(this);


		// Attackness
		public bool Attack () {
			int frame = Game.GlobalFrame;
			if (frame <= LastAttackFrame + AttackColldown) return false;
			// IAttackReceiver




			LastAttackFrame = frame;
			return true;
		}


		#endregion




		#region --- LGC ---




		#endregion



	}
}
