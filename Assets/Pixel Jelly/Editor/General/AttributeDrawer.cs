using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;



namespace PixelJelly.Editor {



	[CustomPropertyDrawer(typeof(NullAlertAttribute))]
	public class NullAlertDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.PropertyField(position, property, label);
			if (property.objectReferenceValue == null) {
				var oldC = GUI.color;
				GUI.color = new Color(1f, 0f, 0f, 1f);
				GUI.Box(new Rect(0, position.y, position.width + position.x, position.height), GUIContent.none);
				GUI.color = oldC;
			}
		}
	}



	[CustomPropertyDrawer(typeof(RandomButtonAttribute))]
	public class RandomButtonDrawer : PropertyDrawer {
		private static GUIContent DiceContent => _DiceContent ??= new GUIContent(EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_PreMatCube" : "PreMatCube")) { tooltip = "Random" };
		private static GUIContent _DiceContent = null;
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			float BUTTON_WIDTH = position.height + 6;
			position.width -= BUTTON_WIDTH + 2;
			EditorGUI.PropertyField(position, property, label);
			position.x += position.width + 2;
			position.width = BUTTON_WIDTH;
			var att = attribute as RandomButtonAttribute;
			if (Layout.QuickButton(position, DiceContent)) {
				switch (property.propertyType) {
					case SerializedPropertyType.Integer:
						property.intValue = new System.Random().Next(att.MinInt, att.MaxInt);
						break;
					case SerializedPropertyType.Float:
						property.floatValue = Random.Range(att.Min, att.Max);
						break;
				}
			}

		}
	}



	[CustomPropertyDrawer(typeof(ArrowNumberAttribute))]
	public class ArrowNumberDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			int enumLength = 0;
			if (
				property.propertyType == SerializedPropertyType.Integer ||
				property.propertyType == SerializedPropertyType.Float ||
				property.propertyType == SerializedPropertyType.Enum
			) {
				if (property.propertyType == SerializedPropertyType.Enum) {
					enumLength = property.enumNames.Length;
				}
				int oldI = EditorGUI.indentLevel;
				bool oldE = GUI.enabled;
				var att = attribute as ArrowNumberAttribute;
				// Label
				EditorGUI.PrefixLabel(position, label);
				GUI.Button(new Rect(0, 0, 0, 0), "");
				var rect = position;
				rect.x += EditorGUIUtility.labelWidth;
				rect.width -= EditorGUIUtility.labelWidth;
				float buttonWidth = position.height + 6f;
				float fieldWidth = rect.width - buttonWidth * 2f;
				rect.width = buttonWidth;
				GUI.enabled = oldE && (att.Loop || !ReachMin(att));
				// Button L
				if (Layout.LeftQuickButton(rect)) {
					if (EditorGUIUtility.editingTextField) {
						EditorGUI.FocusTextInControl(null);
					}
					SetValue(false, att);
				}
				rect.x += rect.width;
				rect.width = fieldWidth;
				EditorGUI.indentLevel = 0;
				GUI.enabled = oldE && att.Enabled;
				// Field
				var fieldRect = rect.Expand(-2, -2, 0, 0);
				switch (property.propertyType) {
					default:
						EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
						break;
					case SerializedPropertyType.Integer:
						property.intValue = Mathf.Clamp(
							EditorGUI.DelayedIntField(fieldRect, property.intValue),
							att.MinInt, att.MaxInt
						);
						break;
					case SerializedPropertyType.Float:
						property.floatValue = Mathf.Clamp(
							EditorGUI.DelayedFloatField(fieldRect, property.floatValue),
							att.Min, att.Max
						);
						break;
					case SerializedPropertyType.Enum:
						EditorGUI.PropertyField(fieldRect, property, GUIContent.none);
						break;
				}
				EditorGUI.indentLevel = oldI;
				rect.x += rect.width;
				rect.width = buttonWidth;
				GUI.enabled = oldE && (att.Loop || !ReachMax(att));
				// Button R
				if (Layout.RightQuickButton(rect)) {
					if (EditorGUIUtility.editingTextField) {
						EditorGUI.FocusTextInControl(null);
					}
					SetValue(true, att);
				}
				GUI.enabled = oldE;
			} else {
				base.OnGUI(position, property, label);
			}
			// Func
			void SetValue (bool grow, ArrowNumberAttribute att) {
				int id; // 0:min, 1:-. 2:+, 3:max
				if (att.Loop && grow && ReachMax(att)) {
					id = 0;
				} else if (att.Loop && !grow && ReachMin(att)) {
					id = 3;
				} else {
					id = grow ? 2 : 1;
				}
				switch (property.propertyType) {
					case SerializedPropertyType.Integer:
						property.intValue = Mathf.Clamp(
							id == 0 ? att.MinInt :
							id == 1 ? property.intValue - att.StepInt :
							id == 2 ? property.intValue + att.StepInt :
							att.MaxInt, att.MinInt, att.MaxInt
						);
						break;
					case SerializedPropertyType.Float:
						float newValue = Mathf.Clamp(
							id == 0 ? att.Min :
							id == 1 ? property.floatValue - att.Step :
							id == 2 ? property.floatValue + att.Step :
							att.Max, att.Min, att.Max
						);
						newValue = Mathf.Round(newValue / att.Step) * att.Step;
						property.floatValue = newValue;
						break;
					case SerializedPropertyType.Enum:
						property.enumValueIndex = Mathf.Clamp(
							id == 0 ? 0 :
							id == 1 ? property.enumValueIndex - 1 :
							id == 2 ? property.enumValueIndex + 1 :
							enumLength - 1,
							0, enumLength - 1
						);
						break;
				}
				GUI.changed = true;
			}
			bool ReachMin (ArrowNumberAttribute att) => property.propertyType switch {
				SerializedPropertyType.Integer => property.intValue <= att.MinInt,
				SerializedPropertyType.Float => property.floatValue <= att.Min,
				SerializedPropertyType.Enum => property.enumValueIndex <= 0,
				_ => false,
			};
			bool ReachMax (ArrowNumberAttribute att) => property.propertyType switch {
				SerializedPropertyType.Integer => property.intValue >= att.MaxInt,
				SerializedPropertyType.Float => property.floatValue >= att.Max,
				SerializedPropertyType.Enum => property.enumValueIndex >= enumLength - 1,
				_ => false,
			};
		}
	}



	[CustomPropertyDrawer(typeof(ClampAttribute))]
	public class ClampDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			var att = attribute as ClampAttribute;
			switch (property.propertyType) {
				case SerializedPropertyType.Integer:
					property.intValue = Mathf.Clamp(property.intValue, att.MinInt, att.MaxInt);
					break;
				case SerializedPropertyType.Float:
					property.floatValue = Mathf.Clamp(property.floatValue, att.Min, att.Max);
					break;
			}
			EditorGUI.PropertyField(position, property, label);
		}
	}


	[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
	public class MinMaxSliderDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			var att = attribute as MinMaxSliderAttribute;
			switch (property.propertyType) {
				case SerializedPropertyType.Vector2:
					float min = property.vector2Value.x;
					float max = property.vector2Value.y;
					EditorGUI.MinMaxSlider(position, label, ref min, ref max, att.Min, att.Max);
					property.vector2Value = new Vector2(min, max);
					break;
			}
		}
	}



	[CustomPropertyDrawer(typeof(EnumSwicherAttribute))]
	public class EnumSwicherDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.Enum) {
				base.OnGUI(position, property, label);
				return;
			}
			position = EditorGUI.IndentedRect(position);
			var names = property.enumDisplayNames;
			float buttonSize = position.width / names.Length;
			var oldE = GUI.enabled;
			for (int i = 0; i < names.Length; i++) {
				GUI.enabled = i != property.enumValueIndex;
				if (Layout.QuickButton(new Rect(position.x + i * buttonSize, position.y, buttonSize, position.height), names[i])) {
					property.enumValueIndex = i;
				}
			}
			GUI.enabled = oldE;
		}
	}



	[CustomPropertyDrawer(typeof(ColorGradientAttribute))]
	public class ColorGradientDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

			var p_UseColor = property.FindPropertyRelative("UseColor");
			var p_Color = property.FindPropertyRelative("Color");
			var p_Gradient = property.FindPropertyRelative("Gradient");
			if (p_UseColor == null || p_Color == null || p_Gradient == null) { return; }

			var att = attribute as ColorGradientAttribute;
			if (att.ThumbnailOnly && position.width < att.ThumbnailWidth) {
				int _oldI = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				if (p_UseColor.boolValue) {
					p_Color.colorValue = EditorGUI.ColorField(position, GUIContent.none, p_Color.colorValue, false, false, false);
				} else {
					EditorGUI.PropertyField(position, p_Gradient, GUIContent.none);
				}
				EditorGUI.indentLevel = _oldI;
				return;
			}

			// Label
			if (label != null && !string.IsNullOrEmpty(label.text)) {
				EditorGUI.PrefixLabel(position, label);
			}

			var rect = position;
			rect.x += EditorGUIUtility.labelWidth + 2;
			rect.width -= EditorGUIUtility.labelWidth + 2;
			rect.height = 18;
			float buttonWidth = position.height;
			float fieldWidth = rect.width - buttonWidth;

			// Field
			rect.width = fieldWidth;
			int oldI = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			if (p_UseColor.boolValue) {
				EditorGUI.PropertyField(rect, p_Color, GUIContent.none);
			} else {
				EditorGUI.PropertyField(rect, p_Gradient, GUIContent.none);
			}
			EditorGUI.indentLevel = oldI;

			// Use Color
			rect.x += rect.width;
			rect.width = buttonWidth;
			if (GUI.Button(rect, p_UseColor.boolValue ? "C" : "G", EditorStyles.miniButtonRight)) {
				p_UseColor.boolValue = !p_UseColor.boolValue;
			}

		}
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, false);
	}


	[CustomPropertyDrawer(typeof(PixelGalleryAttribute))]
	public class PixelGalleryDrawer : PropertyDrawer {


		// Const
		private const int GAPPERY_HEIGHT = 36;

		// Short
		private GUIStyle RightLabel => _RightLabel ??= new GUIStyle(GUI.skin.label) {
			alignment = TextAnchor.LowerRight,
		};
		private GUIStyle _RightLabel = null;
		private GUIContent MenuContent => _MenuContent ??= EditorGUIUtility.IconContent("_Menu@2x");
		private GUIContent _MenuContent = null;

		// Data
		private SerializedProperty p_Sprites = null;
		private bool GalleryMode = true;


		// MSG
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

			var p_sprites = GetSpritesProp(property);
			var p_index = property.FindPropertyRelative("m_Selecting");
			float LABEL_WIDTH = EditorGUIUtility.labelWidth;
			var oldE = GUI.enabled;

			// Label
			EditorGUI.PrefixLabel(position, label);

			// Menu Button
			var menuRect = new Rect(position.x + LABEL_WIDTH - 20, position.y, 18, 18);
			if (GUI.Button(menuRect, MenuContent, GUI.skin.label)) {
				GalleryMode = !GalleryMode;
			}
			EditorGUIUtility.AddCursorRect(menuRect, MouseCursor.Link);

			if (GalleryMode) {
				// Gallery Mode
				var gRect = position.Expand(-LABEL_WIDTH - 26, -26, 0, 0);
				var rect = new Rect(
					position.x, position.y,
					position.width, 18
				);
				// Index Label
				if (p_sprites.arraySize > 0) {
					EditorGUI.LabelField(
						new Rect(position.x, position.y, LABEL_WIDTH - 6, gRect.height),
						new GUIContent($"{p_index.intValue + 1}/{ p_sprites.arraySize}"),
						RightLabel
					);
				} else {
					EditorGUI.LabelField(
						new Rect(position.x, position.y, LABEL_WIDTH - 6, gRect.height),
						new GUIContent("0/0"),
						RightLabel
					);
				}
				rect = EditorGUI.IndentedRect(rect);
				EditorGUI.indentLevel++;
				// Switcher
				rect.height = position.yMax - rect.y - 12;
				GUI.enabled = p_sprites.arraySize > 0 && p_index.intValue > 0;
				if (Layout.LeftQuickButton(new Rect(gRect.xMin - 26, gRect.y, 24, gRect.height))) {
					p_index.intValue = Mathf.Clamp(p_index.intValue - 1, 0, p_sprites.arraySize - 1);
				}
				GUI.enabled = p_sprites.arraySize > 0 && p_index.intValue < p_sprites.arraySize - 1;
				if (Layout.RightQuickButton(new Rect(gRect.xMax + 2, gRect.y, 24, gRect.height))) {
					p_index.intValue = Mathf.Clamp(p_index.intValue + 1, 0, p_sprites.arraySize - 1);
				}
				// Gallery
				GUI.enabled = oldE;
				p_index.intValue = Mathf.Clamp(p_index.intValue, 0, p_sprites.arraySize - 1);
				if (p_sprites.arraySize > 0) {
					// Pixel Editor
					EditorGUI.PropertyField(gRect.Expand(0, 0, 0, 2), p_sprites.GetArrayElementAtIndex(p_index.intValue), GUIContent.none, false);
				} else {
					// No Sprite Warning
					GUI.Label(gRect, GUIContent.none, EditorStyles.textField);
					GUI.Label(gRect, "No Sprite", EditorStyles.centeredGreyMiniLabel);
				}
				EditorGUI.indentLevel--;
			} else {
				// Debug Mode
				var rect = EditorGUI.IndentedRect(new Rect(
					position.x, position.y + 20,
					position.width, 18
				));
				EditorGUI.PropertyField(rect, p_index, new GUIContent("Selecting"), true);
				rect.y += 20;
				rect.height = position.height - 18 - 18 - 4;
				EditorGUI.PropertyField(rect, p_sprites, new GUIContent("Sprites"), true);
			}
			GUI.enabled = oldE;
		}


		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) =>
			GalleryMode ? GAPPERY_HEIGHT :
			EditorGUI.GetPropertyHeight(GetSpritesProp(property), label, true) + 48;


		private SerializedProperty GetSpritesProp (SerializedProperty property) {
			if (p_Sprites == null) {
				p_Sprites = property.FindPropertyRelative("m_Sprites");
			}
			return p_Sprites;
		}


	}



	[CustomPropertyDrawer(typeof(GradientGalleryAttribute))]
	public class GradientGalleryDrawer : PropertyDrawer {


		// Const
		private const int ITEM_SIZE = 22;
		private const int ITEM_GAP = 1;

		// Short
		private GUIContent MenuContent => _MenuContent ??= EditorGUIUtility.IconContent("_Menu@2x");
		private GUIContent _MenuContent = null;
		private GUIStyle RightLabel => _RightLabel ??= new GUIStyle(GUI.skin.label) {
			alignment = TextAnchor.LowerRight,
		};
		private GUIStyle _RightLabel = null;

		// Data
		private bool GalleryMode = true;
		private SerializedProperty p_Gradients = null;


		// MSG
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

			var oldE = GUI.enabled;
			var p_gradients = GetGradientsProp(property);
			var p_index = property.FindPropertyRelative("m_Selecting");
			float LABEL_WIDTH = EditorGUIUtility.labelWidth;
			var att = attribute as GradientGalleryAttribute;
			int row = GetGalleryRow(property);

			// Label
			EditorGUI.PrefixLabel(position, label);

			// Menu Button
			var menuRect = new Rect(position.x + LABEL_WIDTH - 20, position.y, 18, 18);
			if (GUI.Button(menuRect, MenuContent, GUI.skin.label)) {
				GalleryMode = !GalleryMode;
			}
			EditorGUIUtility.AddCursorRect(menuRect, MouseCursor.Link);

			if (GalleryMode) {
				// Index Label
				if (row >= 2) {
					if (p_gradients.arraySize > 0) {
						EditorGUI.LabelField(
							new Rect(position.x, position.y, LABEL_WIDTH - 6, ITEM_SIZE * 2),
							new GUIContent($"{p_index.intValue + 1}/{ p_gradients.arraySize}"),
							RightLabel
						);
					} else {
						EditorGUI.LabelField(
							new Rect(position.x, position.y, LABEL_WIDTH - 6, ITEM_SIZE * 2),
							new GUIContent("0/0"),
							RightLabel
						);
					}
				}
				// Gallery Mode
				if (p_gradients.arraySize > 0) {
					// Gallery
					var oldC = GUI.color;
					int index = 0;
					float posX = position.x + LABEL_WIDTH;
					float posY = position.y;
					posX += (position.width - LABEL_WIDTH - ITEM_SIZE * att.Column) / 2;
					for (int y = 0; y < row; y++) {
						for (int x = 0; x < att.Column; x++, index++) {
							var rect = new Rect(posX + x * ITEM_SIZE, posY + y * ITEM_SIZE, ITEM_SIZE, ITEM_SIZE);
							rect = rect.Expand(-ITEM_GAP);
							if (index < p_gradients.arraySize) {
								var p_ele = p_gradients.GetArrayElementAtIndex(index);
								GUI.enabled = false;
								GUI.color = oldC;
								EditorGUI.PropertyField(rect, p_ele, GUIContent.none, false);
								GUI.enabled = oldE;
								GUI.color = Color.clear;
								if (Layout.QuickButton(rect, GUIContent.none)) {
									p_index.intValue = index;
								}
								if (p_index.intValue == index) {
									GUI.enabled = oldE;
									GUI.color = oldC;
									Layout.FrameGUI(rect, 1f, Color.white);
									Layout.FrameGUI(rect.Expand(-1), 1f, Color.black);
								}
							} else {
								break;
							}
						}
					}
					GUI.color = oldC;
				} else {
					// No Gradient


				}
			} else {
				// Debug Mode
				var rect = EditorGUI.IndentedRect(new Rect(
					position.x, position.y + 20,
					position.width, 18
				));
				EditorGUI.PropertyField(rect, p_index, new GUIContent("Selecting"), true);
				rect.y += 20;
				rect.height = position.height - 18 - 18 - 4;
				EditorGUI.PropertyField(rect, p_gradients, new GUIContent("Gradients"), true);
			}
			GUI.enabled = oldE;
		}


		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) => GalleryMode ?
			Mathf.Max(GetGalleryRow(property), 1) * ITEM_SIZE :
			EditorGUI.GetPropertyHeight(GetGradientsProp(property), label, true) + 48;


		// LGC
		private SerializedProperty GetGradientsProp (SerializedProperty property) {
			if (p_Gradients == null) {
				p_Gradients = property.FindPropertyRelative("m_Gradients");
			}
			return p_Gradients;
		}


		private int GetGalleryRow (SerializedProperty property) => Mathf.CeilToInt(
			(float)GetGradientsProp(property).arraySize / (attribute as GradientGalleryAttribute).Column
		);


	}


	[CustomPropertyDrawer(typeof(RectOffsetAttribute))]
	public class RectOffsetDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.PrefixLabel(position, label);
			var rect = position;
			rect.x += EditorGUIUtility.labelWidth + 2;
			rect.width -= EditorGUIUtility.labelWidth + 2;
			float labelWidth = 16;
			float fieldWidth = rect.width / 4 - labelWidth;
			int oldI = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			var p_L = property.FindPropertyRelative("m_Left");
			var p_R = property.FindPropertyRelative("m_Right");
			var p_T = property.FindPropertyRelative("m_Top");
			var p_B = property.FindPropertyRelative("m_Bottom");

			// L
			rect.width = labelWidth;
			GUI.Label(rect, "L");
			rect.x += rect.width;
			rect.width = fieldWidth - 2;
			p_L.intValue = EditorGUI.DelayedIntField(rect, GUIContent.none, p_L.intValue);

			// R
			rect.x += fieldWidth;
			rect.width = labelWidth;
			GUI.Label(rect, "R");
			rect.x += rect.width;
			rect.width = fieldWidth - 2;
			p_R.intValue = EditorGUI.DelayedIntField(rect, GUIContent.none, p_R.intValue);

			// T
			rect.x += fieldWidth;
			rect.width = labelWidth;
			GUI.Label(rect, "T");
			rect.x += rect.width;
			rect.width = fieldWidth - 2;
			p_T.intValue = EditorGUI.DelayedIntField(rect, GUIContent.none, p_T.intValue);

			// B
			rect.x += fieldWidth;
			rect.width = labelWidth;
			GUI.Label(rect, "B");
			rect.x += rect.width;
			rect.width = fieldWidth - 2;
			p_B.intValue = EditorGUI.DelayedIntField(rect, GUIContent.none, p_B.intValue);

			EditorGUI.indentLevel = oldI;
		}
		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) => EditorGUI.GetPropertyHeight(property, label, false);
	}



	[CustomPropertyDrawer(typeof(PixelEditorAttribute))]
	public class PixelEditorDrawer : PropertyDrawer {


		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

			var att = attribute as PixelEditorAttribute;
			var p_Colors = property.FindPropertyRelative("Colors");
			var p_Width = property.FindPropertyRelative("Width");
			var p_Height = property.FindPropertyRelative("Height");
			int width = Mathf.Max(p_Width.intValue, 1);
			int height = Mathf.Max(p_Height.intValue, 1);
			int arrSize = p_Colors.arraySize;
			position.height -= 2;

			// Label
			var pRect = position;
			if (att.UseLabel) {
				float oldLabelWidth = EditorGUIUtility.labelWidth;
				float LABEL_WIDTH = Mathf.Max(EditorGUIUtility.labelWidth, 64);
				EditorGUIUtility.labelWidth = LABEL_WIDTH;
				EditorGUI.PrefixLabel(position, label);
				pRect.x += EditorGUIUtility.labelWidth + 2;
				pRect.width -= EditorGUIUtility.labelWidth + 2;
				EditorGUIUtility.labelWidth = oldLabelWidth;
			}

			// BG
			if (att.FitBackground) {
				float oldWidth = pRect.width;
				pRect = pRect.Fit((float)width / height);
				if (pRect.width.NotAlmost(oldWidth)) {
					pRect.x += (pRect.width - oldWidth) / 2f + 2;
				}
			}

			// Center
			if (att.Center) {
				pRect.x = position.x + (position.width - pRect.width) / 2;
			}

			// Open Window Button
			if (GUI.enabled) {
				EditorGUIUtility.AddCursorRect(pRect, MouseCursor.Link);
			}
			if (GUI.Button(pRect, GUIContent.none, GUI.skin.textField)) {
				var sourceColors = new Color32[width * height];
				for (int i = 0; i < arrSize; i++) {
					sourceColors[i] = p_Colors.GetArrayElementAtIndex(i).colorValue;
				}
				var p_PivotX = property.FindPropertyRelative("PivotX");
				var p_PivotY = property.FindPropertyRelative("PivotY");
				var p_BorderL = property.FindPropertyRelative("BorderL");
				var p_BorderR = property.FindPropertyRelative("BorderR");
				var p_BorderD = property.FindPropertyRelative("BorderD");
				var p_BorderU = property.FindPropertyRelative("BorderU");
				PixelEditorWindow.OpenWindow(
					new PixelSprite(
						sourceColors, width, height,
						p_PivotX.intValue, p_PivotY.intValue,
						p_BorderL.intValue, p_BorderR.intValue, p_BorderD.intValue, p_BorderU.intValue
					),
					(_sprite) => SpriteToProperty(_sprite, property)
				);
			}

			if (!att.FitBackground) {
				float oldWidth = pRect.width;
				pRect = pRect.Fit((float)width / height);
				if (pRect.width.NotAlmost(oldWidth)) {
					pRect.x += (pRect.width - oldWidth) / 2f + 2;
				}
				if (att.Center) {
					pRect.x = position.x + (position.width - pRect.width) / 2;
				}
			}

			// Pixels
			const int MAX_SIZE = 16;
			int aimAxis = width > height ? 0 : 1;
			int aimSize = Mathf.Max(width, height);
			while (aimSize > MAX_SIZE) {
				aimSize /= 2;
			}
			int pixelWidth, pixelHeight;
			if (aimAxis == 0) {
				pixelWidth = aimSize;
				pixelHeight = aimSize > 0 ? pixelWidth * height / width : 0;
			} else {
				pixelHeight = aimSize;
				pixelWidth = aimSize > 0 ? pixelHeight * width / height : 0;
			}
			pRect = pRect.Expand(-6);

			if (width > 0 && height > 0) {
				var pixelSize = new Vector2(pRect.width / pixelWidth, pRect.height / pixelHeight);
				var rect = new Rect(0, 0, pixelSize.x + 1f, pixelSize.y + 1f);
				for (int j = 0; j < pixelHeight; j++) {
					rect.y = pRect.y + (pixelHeight - j - 1) * pixelSize.y;
					for (int i = 0; i < pixelWidth; i++) {
						rect.x = pRect.x + i * pixelSize.x;
						EditorGUI.DrawRect(rect, GetColor(i, j));
					}
				}
			} else {
				GUI.Label(pRect, "(No Pixels)", EditorStyles.centeredGreyMiniLabel);
			}
			// Func
			Color32 GetColor (int i, int j) {
				int index =
					Util.RemapRounded(0, pixelHeight - 1, 0, height - 1, j) * width +
					Util.RemapRounded(0, pixelWidth - 1, 0, width - 1, i);
				return index >= 0 && index < arrSize ?
					(Color32)p_Colors.GetArrayElementAtIndex(index).colorValue :
					new Color32(0, 0, 0, 0);
			}
		}


		public override float GetPropertyHeight (SerializedProperty property, GUIContent label) => 18 * 3;


		private void SpriteToProperty (PixelSprite source, SerializedProperty property) {
			if (source == null || property == null) { return; }
			property.serializedObject.Update();
			var p_NewColors = property.FindPropertyRelative("Colors");
			var p_NewWidth = property.FindPropertyRelative("Width");
			var p_NewHeight = property.FindPropertyRelative("Height");
			var p_NewPivotX = property.FindPropertyRelative("PivotX");
			var p_NewPivotY = property.FindPropertyRelative("PivotY");
			var p_NewBorderL = property.FindPropertyRelative("BorderL");
			var p_NewBorderR = property.FindPropertyRelative("BorderR");
			var p_NewBorderD = property.FindPropertyRelative("BorderD");
			var p_NewBorderU = property.FindPropertyRelative("BorderU");
			p_NewColors.ClearArray();
			if (source.Colors != null) {
				for (int i = 0; i < source.Colors.Length; i++) {
					p_NewColors.InsertArrayElementAtIndex(i);
					p_NewColors.GetArrayElementAtIndex(i).colorValue = source.Colors[i];
				}
			}
			p_NewWidth.intValue = source.Width;
			p_NewHeight.intValue = source.Height;
			p_NewPivotX.intValue = source.PivotX;
			p_NewPivotY.intValue = source.PivotY;
			p_NewBorderL.intValue = source.BorderL;
			p_NewBorderR.intValue = source.BorderR;
			p_NewBorderD.intValue = source.BorderD;
			p_NewBorderU.intValue = source.BorderU;
			property.serializedObject.ApplyModifiedProperties();
		}


	}



	[CustomPropertyDrawer(typeof(MinMaxNumberAttribute))]
	public class MinMaxNumberDrawer : PropertyDrawer {

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			if (property.propertyType != SerializedPropertyType.Vector2 && property.propertyType != SerializedPropertyType.Vector2Int) {
				EditorGUI.PropertyField(position, property, label);
				return;
			}
			bool isInt = property.propertyType == SerializedPropertyType.Vector2Int;
			var att = attribute as MinMaxNumberAttribute;
			EditorGUI.PrefixLabel(position, label);
			GUI.Button(default, GUIContent.none);
			const int GAP = 2;
			const int BUTTON_WIDTH = 24;
			var rect = position;
			rect.x += EditorGUIUtility.labelWidth;
			rect.width -= EditorGUIUtility.labelWidth;
			float fieldWidth = rect.width / 2f - GAP;
			bool oldE = GUI.enabled;
			int oldI = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			if (isInt) {
				// Int
				var value = property.vector2IntValue;
				int newValueX = value.x;
				int newValueY = value.y;
				// Min
				rect.width = fieldWidth;
				GUI.enabled = newValueX > att.MinInt;
				if (Layout.LeftQuickButton(rect.Expand(0, BUTTON_WIDTH - rect.width, 0, 0))) {
					newValueX = Mathf.Clamp(newValueX - att.GapInt, att.MinInt, att.MaxInt);
				}
				GUI.enabled = oldE;
				newValueX = EditorGUI.DelayedIntField(rect.Expand(-BUTTON_WIDTH - 2, -BUTTON_WIDTH - 2, 0, 0), GUIContent.none, newValueX);
				GUI.enabled = newValueX < att.MaxInt;
				if (Layout.RightQuickButton(rect.Expand(BUTTON_WIDTH - rect.width, 0, 0, 0))) {
					newValueX = Mathf.Clamp(newValueX + att.GapInt, att.MinInt, att.MaxInt);
				}
				// Label
				rect.x += rect.width;
				rect.width = GAP * 2;
				GUI.enabled = oldE;
				// Max
				rect.width = fieldWidth;
				rect.x += GAP * 2;
				GUI.enabled = newValueY > att.MinInt;
				if (Layout.LeftQuickButton(rect.Expand(0, BUTTON_WIDTH - rect.width, 0, 0))) {
					newValueY = Mathf.Clamp(newValueY - att.GapInt, att.MinInt, att.MaxInt);
				}
				GUI.enabled = oldE;
				newValueY = EditorGUI.DelayedIntField(rect.Expand(-BUTTON_WIDTH - 2, -BUTTON_WIDTH - 2, 0, 0), GUIContent.none, newValueY);
				GUI.enabled = newValueY < att.MaxInt;
				if (Layout.RightQuickButton(rect.Expand(BUTTON_WIDTH - rect.width, 0, 0, 0))) {
					newValueY = Mathf.Clamp(newValueY + att.GapInt, att.MinInt, att.MaxInt);
				}
				if (att.IgnoreMinMaxCompare) {
					if (newValueX != value.x) {
						value.x = Mathf.Clamp(newValueX, att.MinInt, att.MaxInt);
					} else if (newValueY != value.y) {
						value.y = Mathf.Clamp(newValueY, att.MinInt, att.MaxInt);
					}
				} else {
					if (newValueX != value.x) {
						value.x = Mathf.Clamp(newValueX, att.MinInt, att.MaxInt);
						value.y = Mathf.Max(value.x, value.y);
					} else if (newValueY != value.y) {
						value.y = Mathf.Clamp(newValueY, att.MinInt, att.MaxInt);
						value.x = Mathf.Min(value.x, value.y);
					}
				}
				property.vector2IntValue = value;
			} else {
				// Float
				var value = property.vector2Value;
				float newValueX = value.x;
				float newValueY = value.y;
				bool snapX = false;
				bool snapY = false;
				// Min
				rect.width = fieldWidth;
				GUI.enabled = newValueX > att.Min;
				if (Layout.LeftQuickButton(rect.Expand(0, BUTTON_WIDTH - rect.width, 0, 0))) {
					newValueX = Mathf.Clamp(newValueX - att.Gap, att.Min, att.Max);
					snapX = true;
				}
				GUI.enabled = oldE;
				newValueX = EditorGUI.DelayedFloatField(rect.Expand(-BUTTON_WIDTH - 2, -BUTTON_WIDTH - 2, 0, 0), GUIContent.none, newValueX);
				GUI.enabled = newValueX < att.Max;
				if (Layout.RightQuickButton(rect.Expand(BUTTON_WIDTH - rect.width, 0, 0, 0))) {
					newValueX = Mathf.Clamp(newValueX + att.Gap, att.Min, att.Max);
					snapX = true;
				}
				// Label
				rect.x += rect.width;
				rect.width = GAP * 2;
				GUI.enabled = oldE;
				// Max
				rect.width = fieldWidth;
				rect.x += GAP * 2;
				GUI.enabled = newValueY > att.Min;
				if (Layout.LeftQuickButton(rect.Expand(0, BUTTON_WIDTH - rect.width, 0, 0))) {
					newValueY = Mathf.Clamp(newValueY - att.Gap, att.Min, att.Max);
					snapY = true;
				}
				GUI.enabled = oldE;
				newValueY = EditorGUI.DelayedFloatField(rect.Expand(-BUTTON_WIDTH - 2, -BUTTON_WIDTH - 2, 0, 0), GUIContent.none, newValueY);
				GUI.enabled = newValueY < att.Max;
				if (Layout.RightQuickButton(rect.Expand(BUTTON_WIDTH - rect.width, 0, 0, 0))) {
					newValueY = Mathf.Clamp(newValueY + att.Gap, att.Min, att.Max);
					snapY = true;
				}
				if (att.IgnoreMinMaxCompare) {
					if (newValueX != value.x) {
						value.x = Mathf.Clamp(newValueX, att.Min, att.Max);
					} else if (newValueY != value.y) {
						value.y = Mathf.Clamp(newValueY, att.Min, att.Max);
					}
					if (snapX && att.Gap.NotAlmostZero()) {
						value.x = Mathf.Round(value.x / att.Gap) * att.Gap;
					}
					if (snapY && att.Gap.NotAlmostZero()) {
						value.y = Mathf.Round(value.y / att.Gap) * att.Gap;
					}
				} else {
					if (newValueX != value.x) {
						value.x = Mathf.Clamp(newValueX, att.Min, att.Max);
						value.y = Mathf.Max(value.x, value.y);
					} else if (newValueY != value.y) {
						value.y = Mathf.Clamp(newValueY, att.Min, att.Max);
						value.x = Mathf.Min(value.x, value.y);
					}
					if (snapX && att.Gap.NotAlmostZero()) {
						value.x = Mathf.Round(value.x / att.Gap) * att.Gap;
					}
					if (snapY && att.Gap.NotAlmostZero()) {
						value.y = Mathf.Round(value.y / att.Gap) * att.Gap;
					}
				}
				property.vector2Value = value;
			}
			GUI.enabled = oldE;
			EditorGUI.indentLevel = oldI;
		}


	}



	[CustomPropertyDrawer(typeof(LabelOnlyAttribute))]
	public class LabelOnlyDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) =>
			EditorGUI.LabelField(position, label);
	}


	[CustomPropertyDrawer(typeof(LabelAttribute))]
	public class LabelDrawer : PropertyDrawer {
		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) =>
			EditorGUI.PropertyField(position, property, new GUIContent((attribute as LabelAttribute).Label));
	}


}