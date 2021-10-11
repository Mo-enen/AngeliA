using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AngeliaFramework {
	[CreateAssetMenu(fileName = "New Prefab", menuName = "AngeliA/New Prefab", order = 99)]
	public class Prefab : ScriptableObject {




		#region --- VAR ---


		// Api
		public ushort GlobalID => m_GlobalID;

		// Ser
		[SerializeField, Disable] ushort m_GlobalID = 0;
		[SerializeField] Sprite[] m_Sprites = null;


		#endregion




		#region --- MSG ---




		#endregion




		#region --- API ---





		#endregion




		#region --- LGC ---




		#endregion




		#region --- EDT ---
#if UNITY_EDITOR
		public void SetGlobalID (ushort id) {
			if (UnityEditor.EditorApplication.isPlaying) {
				Debug.LogError("Can not set global id at runtime.");
				return;
			}
			m_GlobalID = id;
		}
#endif
		#endregion




	}
}


#if UNITY_EDITOR
namespace AngeliaFramework.Editor {
	using UnityEngine;
	using UnityEditor;
	[CustomEditor(typeof(Prefab))]
	public class Prefab_Inspector : Editor {
		private int SpriteWarning = -1;
		private void OnEnable () => CheckSprites();
		public override void OnInspectorGUI () {
			serializedObject.Update();
			DrawPropertiesExcluding(serializedObject, "m_Script");
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed) {
				CheckSprites();
			}
			if (SpriteWarning >= 0) {
				EditorGUILayout.HelpBox($"Element {SpriteWarning} has different texture.", MessageType.Error, true);
			}
		}
		private void CheckSprites () {
			SpriteWarning = -1;
			serializedObject.Update();
			var p_Sprites = serializedObject.FindProperty("m_Sprites");
			int len = p_Sprites.arraySize;
			if (len > 1) {
				Texture2D texture = null;
				for (int i = 0; i < len; i++) {
					var sp = p_Sprites.GetArrayElementAtIndex(i).objectReferenceValue as Sprite;
					if (sp == null) { continue; }
					if (texture == null) {
						texture = sp.texture;
					} else if (sp.texture != texture) {
						SpriteWarning = i;
						break;
					}
				}
			}
		}
	}
}
#endif
