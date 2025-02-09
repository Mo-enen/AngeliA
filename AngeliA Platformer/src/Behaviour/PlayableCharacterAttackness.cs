using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

public class PlayableCharacterAttackness (PlayableCharacter character) : CharacterAttackness(character) {

	// VAR
	public int CombatLevel { get; set; } = 0;
	private static readonly Dictionary<int, PlayableAttacknessConfig> ConfigPool = [];
	private PlayableCharacter TargetPlayableCharacter => TargetCharacter as PlayableCharacter;

	// MSG
	[OnSavingSlotChanged]
	[OnGameInitialize]
	internal static void OnGameInitialize () => LoadConfigPoolFromFile();

	[OnGameQuitting]
	internal static void OnGameQuitting () {
		foreach (var type in typeof(PlayableCharacter).AllChildClass()) {
			int id = type.AngeHash();
			if (Stage.PeekOrGetEntity(id) is not PlayableCharacter pCh) continue;
			(pCh.Attackness as PlayableCharacterAttackness)?.SaveConfigToPool();
		}
		SaveConfigPoolToFile();
	}

	public override void OnActivated () {
		base.OnActivated();
		LoadConfigFromPool();
	}

	// LGC
	private void LoadConfigFromPool () {
		if (!ConfigPool.TryGetValue(TargetPlayableCharacter.TypeID, out var config)) return;
		config.LoadConfigIntoCharacter(TargetPlayableCharacter);
	}

	private void SaveConfigToPool () {
		if (!ConfigPool.TryGetValue(TargetCharacter.TypeID, out var config)) return;
		config.SaveConfigFromCharacter(TargetPlayableCharacter);
	}

	private static void LoadConfigPoolFromFile () {
		string rootPath = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "Character Attack");
		Util.CreateFolder(rootPath);
		foreach (var type in typeof(PlayableCharacter).AllChildClass()) {
			string name = type.AngeName();
			string path = Util.CombinePaths(rootPath, $"{name}.json");
			var config = JsonUtil.LoadOrCreateJsonFromPath<PlayableAttacknessConfig>(path);
			config.Name = name;
			ConfigPool.TryAdd(type.AngeHash(), config);
		}
	}

	private static void SaveConfigPoolToFile () {
		string rootPath = Util.CombinePaths(Universe.BuiltIn.SlotMetaRoot, "Character Attack");
		foreach (var (_, config) in ConfigPool) {
			string path = Util.CombinePaths(rootPath, $"{config.Name}.json");
			JsonUtil.SaveJsonToPath(config, path, prettyPrint: true);
		}
	}

}
