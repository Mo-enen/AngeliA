using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace UIGadget {
	public class JellyImage : Image {




		#region --- VAR ---


		// Api
		public override Texture mainTexture {
			get {

				if (sprite != null && sprite.texture != null)
					return sprite.texture;

				if (m_JellySprite != null && m_JellySprite.texture != null)
					return m_JellySprite.texture;

				if (m_Material != null)
					return m_Material.mainTexture;

				return base.mainTexture;
			}
		}
		public Vector2 Point {
			get => m_Point;
			set {
				m_Point = value;
				SetVerticesDirty();
			}
		}
		public Vector2 Size {
			get => m_Size;
			set {
				m_Size = value;
				SetVerticesDirty();
			}
		}
		public bool UseJelly {
			get => m_UseJelly;
			set {
				m_UseJelly = value;
				SetVerticesDirty();
			}
		}


		// Ser
		[SerializeField] bool m_UseJelly = true;
		[SerializeField] Sprite m_JellySprite = null;
		[SerializeField] Vector2 m_Point = default;
		[SerializeField] Vector2 m_Size = new Vector2(64, 64);
		[SerializeField, Clamp(0, 128)] int m_Detail = 6;

		// Data
		private UIVertex[] VCache = new UIVertex[4] { new UIVertex(), new UIVertex(), new UIVertex(), new UIVertex(), };


		#endregion




		#region --- MSG ---


		protected override void OnPopulateMesh (VertexHelper toFill) {

			toFill.Clear();

			var rectStart = GetPixelAdjustedRect();
			var rectEnd = new Rect(
				rectStart.position + rectStart.size / 2f + m_Point - m_Size / 2f,
				m_Size
			);
			SetColor(color);

			// Basic
			SetPos(
				new Vector2(rectStart.xMax, rectStart.yMin),
				new Vector2(rectStart.xMax, rectStart.yMax),
				new Vector2(rectStart.xMin, rectStart.yMax),
				new Vector2(rectStart.xMin, rectStart.yMin)
			);
			if (sprite != null) {
				var sUV = sprite.uv;
				SetUV(sUV[3], sUV[1], sUV[0], sUV[2]);
			}
			toFill.AddUIVertexQuad(VCache);

			// Jelly
			if (m_UseJelly) {
				if (m_JellySprite != null) {
					var sUV = m_JellySprite.uv;
					SetUV(sUV[3], sUV[1], sUV[0], sUV[2]);
				}
				// Up
				FillBending(
					new Vector2(rectStart.xMax, rectStart.yMax),
					new Vector2(rectStart.xMin, rectStart.yMax),
					new Vector2(rectEnd.xMax, rectEnd.yMax),
					new Vector2(rectEnd.xMin, rectEnd.yMax),
					Vector2.up, 2, false
				);
				// Down
				FillBending(
					new Vector2(rectStart.xMax, rectStart.yMin),
					new Vector2(rectStart.xMin, rectStart.yMin),
					new Vector2(rectEnd.xMax, rectEnd.yMin),
					new Vector2(rectEnd.xMin, rectEnd.yMin),
					Vector2.down, 0, true
				);
				// Left
				FillBending(
					new Vector2(rectStart.xMin, rectStart.yMin),
					new Vector2(rectStart.xMin, rectStart.yMax),
					new Vector2(rectEnd.xMin, rectEnd.yMin),
					new Vector2(rectEnd.xMin, rectEnd.yMax),
					Vector2.left, 0, true
				);
				// Right
				FillBending(
					new Vector2(rectStart.xMax, rectStart.yMin),
					new Vector2(rectStart.xMax, rectStart.yMax),
					new Vector2(rectEnd.xMax, rectEnd.yMin),
					new Vector2(rectEnd.xMax, rectEnd.yMax),
					Vector2.right, 2, false
				);
			}

			// Func
			void FillBending (
				Vector2 startA, Vector2 startB, Vector2 endA, Vector2 endB, Vector2 dir, int shift, bool flip
			) {
				// Check
				if (Vector2.Angle(endA - startA, dir) > 90f) { return; }
				
				// Fill
				int detail = m_Detail + 2;
				for (int i = 0; i < detail; i++) {
					float u01 = i / (detail - 1f);
					float v01 = (i + 1f) / (detail - 1f);
					SetPos(
						Vector2.Lerp(startA, endA, u01),
						Vector2.Lerp(startA, endA, v01),
						Vector2.Lerp(startB, endB, v01),
						Vector2.Lerp(startB, endB, u01),
						shift, flip
					);
					toFill.AddUIVertexQuad(VCache);
				}
			}
		}


		#endregion




		#region --- UTL ---


		private void SetPos (Vector2 a, Vector2 b, Vector2 c, Vector2 d, int shift = 0, bool flip = false) {
			if (flip) {
				VCache[(3 + shift) % 4].position = b;
				VCache[(2 + shift) % 4].position = a;
				VCache[(1 + shift) % 4].position = d;
				VCache[(0 + shift) % 4].position = c;
			} else {
				VCache[(3 + shift) % 4].position = a;
				VCache[(2 + shift) % 4].position = b;
				VCache[(1 + shift) % 4].position = c;
				VCache[(0 + shift) % 4].position = d;
			}
		}


		private void SetUV (Vector2 a, Vector2 b, Vector2 c, Vector2 d) {
			VCache[3].uv0 = a;
			VCache[2].uv0 = b;
			VCache[1].uv0 = c;
			VCache[0].uv0 = d;
		}


		private void SetColor (Color32 _color) {
			VCache[0].color = _color;
			VCache[1].color = _color;
			VCache[2].color = _color;
			VCache[3].color = _color;
		}


		#endregion




	}
}



#if UNITY_EDITOR
namespace UIGadget.Editor {
	using UnityEditor;
	[CustomEditor(typeof(JellyImage))]
	public class JellyImage_Inspector : Editor {


		private readonly string[] EXCULDE = new string[] {
			"m_Script", "m_RaycastPadding", "m_OnCullStateChanged",
			"m_Type", "m_PreserveAspect", "m_FillCenter", "m_FillMethod",
			"m_FillAmount", "m_FillClockwise", "m_FillOrigin",
			"m_UseSpriteMesh", "m_Sprite", "m_Material", "m_Color",
			"m_RaycastTarget", "m_Maskable", "m_PixelsPerUnitMultiplier"
		};
		private SerializedProperty p_Sprite = null;
		private SerializedProperty p_Material = null;
		private SerializedProperty p_Color = null;
		private SerializedProperty p_RaycastTarget = null;
		private SerializedProperty p_Maskable = null;
		private SerializedProperty p_JellySprite = null;


		private void OnEnable () {
			p_Sprite = serializedObject.FindProperty("m_Sprite");
			p_Material = serializedObject.FindProperty("m_Material");
			p_Color = serializedObject.FindProperty("m_Color");
			p_RaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
			p_Maskable = serializedObject.FindProperty("m_Maskable");
			p_JellySprite = serializedObject.FindProperty("m_JellySprite");
		}


		public override void OnInspectorGUI () {
			serializedObject.Update();
			EditorGUILayout.PropertyField(p_Sprite);
			EditorGUILayout.PropertyField(p_Material);
			EditorGUILayout.PropertyField(p_Color);
			EditorGUILayout.PropertyField(p_RaycastTarget);
			EditorGUILayout.PropertyField(p_Maskable);
			DrawPropertiesExcluding(serializedObject, EXCULDE);
			serializedObject.ApplyModifiedProperties();
			var sprite = p_Sprite.objectReferenceValue as Sprite;
			var spriteJelly = p_JellySprite.objectReferenceValue as Sprite;
			if (sprite != null && spriteJelly != null && sprite.texture != spriteJelly.texture) {
				EditorGUILayout.HelpBox("sprite and jellySprite must have save texture.", MessageType.Warning, true);
			}
		}


	}
}
#endif
