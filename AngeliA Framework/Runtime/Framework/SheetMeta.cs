using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;


namespace AngeliaFramework {

	[System.Serializable]
	public class SpriteEditingMeta {

		[System.Serializable]
		public class Meta {
			public int GlobalID {
				get => I;
				set => I = value;
			}
			public string RealName {
				get => R;
				set => R = value;
			}
			public int SheetNameIndex {
				get => S;
				set => S = value;
			}
			public SheetType SheetType {
				get => T;
				set => T = value;
			}
			public GroupType GroupType {
				get => C;
				set => C = value;
			}

			[SerializeField] private int I;
			[SerializeField] private string R;
			[SerializeField] private int S;
			[SerializeField] private SheetType T;
			[SerializeField] private GroupType C;
		}

		public Meta[] Metas = null;
		public string[] SheetNames = null;

	}



}