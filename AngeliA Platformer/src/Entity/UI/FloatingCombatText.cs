using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// Displays text information with flowing up movement. 
/// </summary>
[EntityAttribute.Capacity(128, 0)]
[EntityAttribute.DontDespawnOutOfRange]
[EntityAttribute.DontDestroyOnZChanged]
[EntityAttribute.UpdateOutOfRange]
[EntityAttribute.ExcludeInMapEditor]
[EntityAttribute.Layer(EntityLayer.UI)]
public class FloatingCombatText : Entity {




	#region --- VAR ---


	// Const
	public static readonly int TYPE_ID = typeof(FloatingCombatText).AngeHash();

	// Api
	/// <summary>
	/// Total existing time in frame
	/// </summary>
	public int Duration = 48;
	/// <summary>
	/// Which font does it use
	/// </summary>
	public int FontID = 0;
	/// <summary>
	/// How far does it flow up in one frame
	/// </summary>
	public int FloatSpeed = 6;
	public int FloatAirDrag = 0;
	public int GlobalHeight = Const.CEL;
	public Color32 BackgroundColor;
	public Color32 TextColor;
	public GUIStyle Style;

	// Data
	private readonly char[] Chars = new char[32];


	#endregion




	#region --- MSG ---


	public override void OnActivated () {
		base.OnActivated();
		Duration = 48;
		FontID = 0;
		FloatSpeed = 5;
		FloatAirDrag = 0;
		GlobalHeight = Const.CEL;
		BackgroundColor = default;
		TextColor = Color32.WHITE;
		Style = null;
		System.Array.Clear(Chars);
	}


	public override void BeforeUpdate () {
		base.BeforeUpdate();
		// Life-time Check
		if (Game.GlobalFrame > SpawnFrame + Duration) {
			Active = false;
			return;
		}
		// Float Movement
		Y += FloatSpeed;
		if (FloatAirDrag >= 1000) {
			FloatSpeed = FloatSpeed.MoveTowards(0, FloatAirDrag / 1000);
		}
		int subDrag = FloatAirDrag % 1000;
		if (subDrag > 0 && Game.GlobalFrame % (1000 / subDrag) == 500 / subDrag) {
			FloatSpeed = FloatSpeed.MoveTowards(0, 1);
		}
	}


	public override void LateUpdate () {
		base.LateUpdate();
		if (!Active) return;
		// Rendering
		TextColor.a = (byte)(Util.Remap(SpawnFrame, SpawnFrame + Duration, 512, 0, Game.GlobalFrame).Clamp(0, 255));
		using var ___ = new GUIContentColorScope(TextColor);
		using var __ = new UILayerScope();
		using var _ = new FontScope(FontID);
		var rect = new IRect(X, Y, 1, GlobalHeight);
		if (BackgroundColor.a > 0) {
			// With BG
			GUI.BackgroundLabel(rect, Chars, BackgroundColor.WithNewA(TextColor.a), Const.QUARTER / 3, style: Style);
		} else {
			// No BG
			GUI.Label(rect, Chars, Style);
		}
	}


	#endregion




	#region --- API ---

	/// <summary>
	/// Spawn a FCT with given infomation
	/// </summary>
	/// <param name="x">Start position in global space</param>
	/// <param name="y">Start position in global space</param>
	/// <param name="text">Text string content</param>
	/// <param name="chars">Text string content</param>
	/// <param name="fontID">Which font does it use</param>
	/// <param name="color">Text color</param>
	/// <param name="backgroundColor"></param>
	/// <param name="style"></param>
	/// <returns>Instance of the spawned FCT entity. Null if failed.</returns>
	public static FloatingCombatText Spawn (int x, int y, string text, int fontID = 0, Color32? color = null, Color32? backgroundColor = null, GUIStyle style = null) => SpawnLogic(x, y, text, null, fontID, style, color, backgroundColor);
	/// <inheritdoc cref="Spawn(int, int, string, int, Color32?, Color32?, GUIStyle)"/>
	public static FloatingCombatText Spawn (int x, int y, char[] chars, int fontID = 0, Color32? color = null, Color32? backgroundColor = null, GUIStyle style = null) => SpawnLogic(x, y, null, chars, fontID, style, color, backgroundColor);


	#endregion




	#region --- LGC ---


	private static FloatingCombatText SpawnLogic (int x, int y, string text, char[] chars, int fontID, GUIStyle style, Color32? color, Color32? backgroundColor) {
		if (Stage.SpawnEntity(TYPE_ID, x, y) is not FloatingCombatText fct) return null;
		fct.FontID = fontID;
		fct.Style = style ?? GUI.Skin.AutoCenterLabel;
		fct.TextColor = color ?? Color32.WHITE;
		fct.BackgroundColor = backgroundColor ?? Color32.CLEAR;
		if (text != null) {
			int len = Util.Min(text.Length, fct.Chars.Length);
			for (int i = 0; i < len; i++) {
				fct.Chars[i] = text[i];
			}
			if (len < fct.Chars.Length) {
				fct.Chars[len] = '\0';
			}
		} else if (chars != null) {
			int len = Util.Min(chars.Length, fct.Chars.Length);
			System.Array.Copy(chars, fct.Chars, len);
			if (len < fct.Chars.Length) {
				fct.Chars[len] = '\0';
			}
		}
		return fct;
	}


	#endregion





}
