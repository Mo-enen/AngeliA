using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Represent an item that holds a whole set of bodypart for pose-style characters to apply
/// </summary>
[EntityAttribute.ExcludeInMapEditor]
[NoItemCombination]
public sealed class BodySetItem : NonStackableItem {




	#region --- VAR ---


	// Api
	/// <summary>
	/// ID of the target character for the bodypart it holds
	/// </summary>
	public int TargetCharacterID { get; init; }
	/// <summary>
	/// Name of the target character for the bodypart it holds
	/// </summary>
	public string TargetCharacterName { get; init; }
	/// <summary>
	/// Rendering config data for the bodypart it holds
	/// </summary>
	public CharacterRenderingConfig Data { get; init; }

	// Data
	private static readonly Dictionary<int, (System.Type type, string name)> Pool = [];


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-129)]
	internal static void OnGameInitialize () {
		// From Character
		var tempConfig = new CharacterRenderingConfig();
		foreach (var type in typeof(Character).AllChildClass()) {
			tempConfig.LoadFromSheet(type, ignoreBodyGadget: true, ignoreCloth: true);
			if (tempConfig.AllBodyPartIsDefault()) continue;
			Pool.TryAdd(type.AngeHash(), (type, type.AngeName()));
		}
		// From Attribute
		foreach (var (_, att) in Util.ForAllAssemblyWithAttribute<BodySetAttribute>()) {
			Pool.TryAdd(att.Name.AngeHash(), (null, att.Name));
		}
	}


	public BodySetItem (System.Type characterType) {
		TargetCharacterName = characterType.AngeName();
		TargetCharacterID = TargetCharacterName.AngeHash();
		Data = new();
		Data.LoadFromSheet(characterType, ignoreBodyGadget: true, ignoreCloth: true);
	}


	public BodySetItem (string basicName) {
		TargetCharacterName = basicName;
		TargetCharacterID = TargetCharacterName.AngeHash();
		Data = new() {
			Head = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[0]}".AngeHash(),
			Body = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[1]}".AngeHash(),
			Hip = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[2]}".AngeHash(),
			Shoulder = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[3]}".AngeHash(),
			UpperArm = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[5]}".AngeHash(),
			LowerArm = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[7]}".AngeHash(),
			Hand = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[9]}".AngeHash(),
			UpperLeg = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[11]}".AngeHash(),
			LowerLeg = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[13]}".AngeHash(),
			Foot = $"{basicName}.{PoseCharacterRenderer.BODY_PART_NAME[15]}".AngeHash(),
		};
	}


	public override void DrawItem (Entity holder, IRect rect, Color32 tint, int z) {

		// Icon
		if (Renderer.TryGetSpriteForGizmos(TargetCharacterID, out var iconSP)) {
			Renderer.Draw(iconSP, rect.Fit(iconSP), tint, z);
		} else {
			Renderer.Draw(BuiltInSprite.ICON_ENTITY, rect, tint, z);
		}

		// Mark
		if (holder is Character ch && ch.Rendering is PoseCharacterRenderer rendering) {
			var bodyparts = rendering.BodyParts;
			bool mark = true;
			for (int i = 0; i < bodyparts.Length; i++) {
				if (Data.GetBodyPartID(i) != bodyparts[i].ID) {
					mark = false;
					break;
				}
			}
			if (mark) {
				Renderer.Draw(
					BuiltInSprite.CHECK_MARK_32,
					rect.Shrink(rect.height / 8).Shift(rect.width / 3, -rect.height / 3),
					Color32.GREEN
				);
			}
		}
	}


	public override bool Use (Character holder, int inventoryID, int itemIndex, out bool consume) {

		consume = false;
		if (holder.Rendering is not PoseCharacterRenderer rendering) return false;

		var bodyparts = rendering.BodyParts;

		bool apply = false;
		for (int i = 0; i < bodyparts.Length; i++) {
			if (Data.GetBodyPartID(i) != bodyparts[i].ID) {
				apply = true;
				break;
			}
		}

		if (apply) {
			// Apply
			for (int i = 0; i < bodyparts.Length; i++) {
				bodyparts[i].SetData(Data.GetBodyPartID(i));
			}
			rendering.SaveCharacterToConfig(saveToFile: true);
		} else if (PoseCharacterRenderer.TryGetConfigFromPool(holder.TypeID, out var config)) {
			// Back to Default
			config.LoadFromSheet(holder.GetType(), ignoreBodyGadget: true, ignoreCloth: true);
			for (int i = 0; i < bodyparts.Length; i++) {
				bodyparts[i].SetData(config.GetBodyPartID(i));
			}
		}

		return true;
	}


	public override bool CanUse (Character holder) => holder.Rendering is PoseCharacterRenderer;


	#endregion




	#region --- API ---


	/// <summary>
	/// Iterate through all body-set the current project have
	/// </summary>
	/// <example><code>
	/// using AngeliA;
	/// 
	/// namespace AngeliaGame;
	/// 
	/// public class Example {
	/// 
	/// 	[OnGameInitializeLater(4096)]
	/// 	internal static void OnGameUpdate () {
	/// 		Debug.Log("All body-set inside this game:");
	/// 		foreach (var (id, (type, typeName)) in BodySetItem.ForAllBodySetCharacterType()) {
	/// 			string setName = ItemSystem.GetItemDisplayName(id);
	/// 			Debug.Log($"{setName} - {typeName}");
	/// 		}
	/// 	}
	/// 
	/// }
	/// </code></example>
	public static IEnumerable<KeyValuePair<int, (System.Type, string)>> ForAllBodySetCharacterType () {
		foreach (var pair in Pool) yield return pair;
	}


	/// <summary>
	/// Get display name for bodyset from language system
	/// </summary>
	public string GetDisplayName (string typeName, out int languageID) {
		string basicName = $"{typeName}.BodySet";
		languageID = basicName.AngeHash();
		return Language.Get(languageID, basicName);
	}


	#endregion




}
