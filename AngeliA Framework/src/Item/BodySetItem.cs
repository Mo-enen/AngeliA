using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.ExcludeInMapEditor]
[NoItemCombination]
public sealed class BodySetItem : NonStackableItem {





	#region --- VAR ---


	// Api
	public int TargetCharacterID { get; init; }
	public string TargetCharacterName { get; init; }
	public CharacterRenderingConfig Data { get; init; }

	// Data
	private static readonly Dictionary<int, System.Type> Pool = [];


	#endregion




	#region --- MSG ---


	[OnGameInitialize(-129)]
	internal static void OnGameInitialize () {
		var tempConfig = new CharacterRenderingConfig();
		foreach (var type in typeof(Character).AllChildClass()) {
			tempConfig.LoadFromSheet(type, ignoreBodyGadget: true, ignoreCloth: true);
			if (tempConfig.AllBodyPartIsDefault()) continue;
			Pool.TryAdd(type.AngeHash(), type);
		}
	}


	public BodySetItem (System.Type characterType) {
		TargetCharacterName = characterType.AngeName();
		TargetCharacterID = TargetCharacterName.AngeHash();
		Data = new();
		Data.LoadFromSheet(characterType, ignoreBodyGadget: true, ignoreCloth: true);
	}


	public override void DrawItem (Entity holder, IRect rect, Color32 tint, int z) {
		




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


	public static IEnumerable<KeyValuePair<int, System.Type>> ForAllBodySetCharacterType () {
		foreach (var pair in Pool) yield return pair;
	}


	public string GetDisplayName (string typeName, out int languageID) {
		string name = GetType().AngeName();
		languageID = $"{name}.{typeName}".AngeHash();
		return Language.Get(languageID, Util.GetDisplayName(name));
	}


	#endregion




}
