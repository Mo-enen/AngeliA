using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;




namespace PixelJelly.Editor {
	public partial class PixelJellyWindow {


		// Const Readonly
		private const string TITLE = "Pixel Jelly";
		private const string ROOT_NAME = "Pixel Jelly";
		private const string PACKAGE_NAME = "com.moenengames.pixeljelly";
		private const string DEFAULT_GROUP_NAME = "Default";
		private const int BEHAVIOUR_PANEL_WIDTH = 228; //248/180;
		private const int INSPECTOR_PANEL_WIDTH = 285; //275
		private const int TITLE_HEIGHT = 20;
		private const int BEHAVIOUR_ITEM_HEIGHT = 28;
		private const int MAX_CANVAS_SIZE = 4096;
		private const int MAX_FRAME_SIZE = 256;
		private const int MAX_SPRITE_SIZE = 256;
		private static readonly AnimationCurve MAGICAL_GRID_CURVE = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0f, 0f, 1.5f, 1.5f, 0.3333333f, 0.3333333f), new Keyframe(0.5f, 0.75f, 0.9797839f, 0.9797839f, 0.3333333f, 0.3267745f), new Keyframe(1f, 1f, 0.3026721f, 0.3026721f, 0.3333333f, 0.1693605f), new Keyframe(2f, 1.5f, 0.5261937f, 0.5261937f, 0.3333333f, 0.3333333f), new Keyframe(4.040914f, 2.627375f, 0.5523874f, 0.5523874f, 0.3333333f, 0f), }, preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, };
		private static readonly AnimationCurve MAGICAL_GRID_CURVE_ALT = new AnimationCurve() { keys = new Keyframe[] { new Keyframe(0f, 0f, 1.731082f, 1.731082f, 0.3333333f, 0.3333333f), new Keyframe(0.1522044f, 0.2634782f, 1.651971f, 1.651971f, 0.3333333f, 0.3333333f), new Keyframe(0.4365595f, 0.7107289f, 1.040882f, 1.040882f, 0.3333333f, 0.3333333f), new Keyframe(0.6485097f, 0.8185913f, 0.4738718f, 0.4738718f, 0.3333333f, 0.3333333f), new Keyframe(1.316345f, 1.111664f, 0.5424764f, 0.5424764f, 0.3333333f, 0.3333333f), new Keyframe(3.028154f, 2.217686f, 0.7189047f, 0.7189047f, 0.3333333f, 0.3333333f), new Keyframe(5.014055f, 3.789916f, 0.7916959f, 0.7916959f, 0.3333333f, 0f), }, preWrapMode = WrapMode.Clamp, postWrapMode = WrapMode.Clamp, };
		private static readonly Dictionary<string, string> DEFAULT_SWAPER = new Dictionary<string, string>() { { "m_Seed", "." }, { "Seed", "." }, };

		// Short
		private static Color HIGHLIGHT_TINT => EditorGUIUtility.isProSkin ? new Color(44f / 255f, 93f / 255f, 135f / 255f) : new Color(58f / 255f, 114f / 255f, 176f / 255f);
		private static Color TITLE_TINT => EditorGUIUtility.isProSkin ? new Color(0.85f, 0.85f, 0.85f) : new Color(0.93f, 0.93f, 0.93f);
		private static string BehaviourAssetRoot {
			get {
				if (string.IsNullOrEmpty(_BehaviourAssetRoot)) {
					string root = EditorUtil.GetRootPath(ROOT_NAME, "");
					if (string.IsNullOrEmpty(root)) {
						root = "Assets/Pixel Jelly Data";
					}
					_BehaviourAssetRoot = Util.FixPath(
						Util.CombinePaths(root, "Data", "Behaviour")
					);
				}
				return _BehaviourAssetRoot;
			}
		}
		private static string _BehaviourAssetRoot = "";
		private static string BehaviourDefaultRoot {
			get {
				if (string.IsNullOrEmpty(_BehaviourDefaultRoot)) {
					string root = EditorUtil.GetRootPath(ROOT_NAME, "");
					if (string.IsNullOrEmpty(root)) {
						root = "Assets/Pixel Jelly Data";
					}
					_BehaviourDefaultRoot = Util.FixPath(
						Util.CombinePaths(root, "Data", "Behaviour Default")
					);
				}
				return _BehaviourDefaultRoot;
			}
		}
		private static string _BehaviourDefaultRoot = "";

		private static Texture2D WindowIcon => _WindowIcon ??= EditorUtil.GetImage(ROOT_NAME, PACKAGE_NAME, "Editor", "Image", $"Icon Small {(EditorGUIUtility.isProSkin ? "Pro" : "Per")}.png");
		private static Texture2D _WindowIcon = null;
		private static Texture2D WindowIconColorful => _WindowIconColorful ??= EditorUtil.GetImage(ROOT_NAME, PACKAGE_NAME, "Editor", "Image", $"Icon Small Colorful.png");
		private static Texture2D _WindowIconColorful = null;
		private static Texture2D TextureIcon => _TextureIcon ??= EditorGUIUtility.IconContent("Texture Icon").image as Texture2D;
		private static Texture2D _TextureIcon = null;
		private static Texture2D AnimationIcon => _AnimationIcon ??= EditorGUIUtility.IconContent("AnimationClip Icon").image as Texture2D;
		private static Texture2D _AnimationIcon = null;
		private static Texture2D DiceIcon => _DiceIcon ??= EditorUtil.GetImage(ROOT_NAME, PACKAGE_NAME, "Editor", "Image", $"Dice Icon {(EditorGUIUtility.isProSkin ? "Pro" : "Per")}.png");
		private static Texture2D _DiceIcon = null;
		private static Texture2D ScriptableIcon => _ScriptableIcon ??= EditorGUIUtility.IconContent("ScriptableObject Icon").image as Texture2D;
		private static Texture2D _ScriptableIcon = null;

		private static GUIStyle PanelStyle => _PanelStyle ??= new GUIStyle() {
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(0, 0, 0, 0),
		};
		private static GUIStyle _PanelStyle = null;
		private static GUIStyle ButtonPanelStyle => _ButtonPanelStyle ??= new GUIStyle() {
			margin = new RectOffset(0, 0, 0, 0),
			padding = new RectOffset(24, 24, 0, 0),
		};
		private static GUIStyle _ButtonPanelStyle = null;
		private static GUIStyle HighlightLabelStyle => _HighlightLabelStyle ??= new GUIStyle(GUI.skin.label) {
			normal = new GUIStyleState() { textColor = Color.white, },
			alignment = TextAnchor.MiddleLeft,
		};
		private static GUIStyle _HighlightLabelStyle = null;
		private static GUIStyle MaximizeButtonStyle => _MaximizeButtonStyle ??= new GUIStyle("IN LockButton") {
			normal = new GUIStyleState() {
				background = new GUIContent(MaximizedContent).image as Texture2D,
				scaledBackgrounds = new Texture2D[0],
				textColor = Color.white,
			},
			active = new GUIStyleState() {
				background = Texture2D.blackTexture,
				scaledBackgrounds = new Texture2D[0],
				textColor = Color.white,
			},
		};
		private static GUIStyle _MaximizeButtonStyle = null;
		private static GUIStyle CommentLabelStyle => _CommentLabelStyle ??= new GUIStyle(GUI.skin.label) {
			alignment = TextAnchor.MiddleCenter,
			normal = new GUIStyleState() {
				background = Texture2D.whiteTexture,
				textColor = Color.white,
			},
			padding = new RectOffset(6, 6, 6, 6),
		};
		private static GUIStyle _CommentLabelStyle = null;
		private static GUIStyle ArrowStyle => _ArrowStyle ??= new GUIStyle(GUI.skin.button) {
			alignment = TextAnchor.MiddleCenter,
			fontSize = 11,
		};
		private static GUIStyle _ArrowStyle = null;

		private static GUIContent CheckerFloorContent => _CheckerFloorContent ??= new GUIContent(EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_CheckerFloor" : "CheckerFloor")) { tooltip = "Draw checker floor or not [G]" };
		private static GUIContent _CheckerFloorContent = null;
		private static GUIContent ClearCacheContent => _ClearCacheContent ??= new GUIContent(EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_Profiler.Memory" : "Profiler.Memory")) { tooltip = "Clear Unused Behaviour Cache" };
		private static GUIContent _ClearCacheContent = null;
		private static GUIContent PlayContent => _PlayContent ??= new GUIContent(EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_PlayButton" : "PlayButton")) { tooltip = "Play Animation [P]" };
		private static GUIContent _PlayContent = null;
		private static GUIContent PauseContent => _PauseContent ??= new GUIContent(EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_PauseButton" : "PauseButton")) { tooltip = "Pause Animation [P]" };
		private static GUIContent _PauseContent = null;
		private static GUIContent MaximizedContent => _MaximizedContent ??= new GUIContent(EditorGUIUtility.IconContent(EditorGUIUtility.isProSkin ? "d_winbtn_win_max" : "winbtn_win_max"));
		private static GUIContent _MaximizedContent = null;
		private static GUIContent HelpContent => _HelpContent ??= new GUIContent(EditorGUIUtility.IconContent("_Help")) { tooltip = "Open help window" };
		private static GUIContent _HelpContent = null;
		private static GUIContent ShowInsMsgContent => _ShowInsMsgContent ??= new GUIContent(EditorGUIUtility.IconContent("scenevis_visible_hover@2x")) { tooltip = "Show Inspector Messages" };
		private static GUIContent _ShowInsMsgContent = null;
		private static GUIContent HideInsMsgContent => _HideInsMsgContent ??= new GUIContent(EditorGUIUtility.IconContent("scenevis_hidden_hover@2x")) { tooltip = "Hide Inspector Messages" };
		private static GUIContent _HideInsMsgContent = null;
		private static GUIContent NewJellyBehContent => _NewJellyBehContent ??= new GUIContent(EditorGUIUtility.IconContent("Toolbar Plus")) { tooltip = "Create New Jelly Behaviour" };
		private static GUIContent _NewJellyBehContent = null;
		private static GUIContent ArrowLeftContent => _ArrowLeftContent ??= EditorGUIUtility.IconContent("scrollleft");
		private static GUIContent _ArrowLeftContent = null;
		private static GUIContent ArrowRightContent => _ArrowRightContent ??= EditorGUIUtility.IconContent("scrollright");
		private static GUIContent _ArrowRightContent = null;
	}
}
