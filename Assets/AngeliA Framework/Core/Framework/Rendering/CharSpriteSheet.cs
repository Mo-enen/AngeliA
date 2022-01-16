using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework.Rendering {
	[CreateAssetMenu(fileName = "New Char Sheet", menuName = "AngeliA/Char Sheet", order = 99)]
	public class CharSpriteSheet : SpriteSheet {


		[System.Serializable]
		public class CharSprite {
			public Rect UvOffset;
			public bool FullWidth;
		}


		public CharSprite[] CharSprites => m_CharSprites;

		[SerializeField] CharSprite[] m_CharSprites = null;


#if UNITY_EDITOR
		public void SetCharSprites (CharSprite[] sprites) => m_CharSprites = sprites;
#endif


	}
}
