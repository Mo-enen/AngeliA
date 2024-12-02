namespace AngeliA;

public struct ClothSprite {

	public readonly bool IsValid => SpriteID_FL != 0 && SpriteID_FR != 0 && SpriteID_BL != 0 && SpriteID_BR != 0;

	public string SuitName;
	public int GroupID;
	public int SpriteID_FL;
	public int SpriteID_FR;
	public int SpriteID_BL;
	public int SpriteID_BR;

	public ClothSprite () => SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = GroupID = 0;
	public ClothSprite (string name, string suitName) => LoadFromSheet(name, suitName);
	public ClothSprite (string name, string suitName, string suitNameAlt) {
		if (!LoadFromSheet(name, suitName)) {
			LoadFromSheet(name, suitNameAlt);
		}
	}
	public ClothSprite (string name, string suitName, string suitNameAltA, string suitNameAltB) {
		if (!LoadFromSheet(name, suitName) && !LoadFromSheet(name, suitNameAltA)) {
			LoadFromSheet(name, suitNameAltB);
		}
	}
	public ClothSprite (string name, params string[] suitNames) {
		foreach (var suitName in suitNames) {
			if (LoadFromSheet(name, suitName)) break;
		}
	}

	public bool LoadFromSheet (string name, string suitName) {

		SuitName = suitName;

		SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = $"{name}.{suitName}".AngeHash();
		if (Renderer.HasSprite(SpriteID_FL)) return true;
		if (Renderer.HasSpriteGroup(SpriteID_FL)) {
			GroupID = SpriteID_FL;
			SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = 0;
			return true;
		}

		SpriteID_FL = SpriteID_FR = $"{name}.{suitName}.F".AngeHash();
		SpriteID_BL = SpriteID_BR = $"{name}.{suitName}.B".AngeHash();
		if (!Renderer.HasSprite(SpriteID_BL)) SpriteID_BL = SpriteID_BR = SpriteID_FL;
		if (Renderer.HasSprite(SpriteID_FL)) return true;

		SpriteID_FL = SpriteID_BL = $"{name}.{suitName}.L".AngeHash();
		SpriteID_FR = SpriteID_BR = $"{name}.{suitName}.R".AngeHash();
		if (Renderer.HasSprite(SpriteID_FL) && Renderer.HasSprite(SpriteID_FR)) return true;

		SpriteID_FL = $"{name}.{suitName}.FL".AngeHash();
		SpriteID_FR = $"{name}.{suitName}.FR".AngeHash();
		SpriteID_BL = $"{name}.{suitName}.BL".AngeHash();
		SpriteID_BR = $"{name}.{suitName}.BR".AngeHash();
		if (!Renderer.HasSprite(SpriteID_BL)) SpriteID_BL = SpriteID_FL;
		if (!Renderer.HasSprite(SpriteID_BR)) SpriteID_BR = SpriteID_FR;
		if (Renderer.HasSprite(SpriteID_FL) && Renderer.HasSprite(SpriteID_FR)) return true;

		SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = 0;
		SuitName = "";

		return false;
	}
	public readonly int GetSpriteID (bool front, bool right) => front ? (right ? SpriteID_FR : SpriteID_FL) : (right ? SpriteID_BR : SpriteID_BL);
	public readonly bool TryGetSprite (bool front, bool right, out AngeSprite sprite) => Renderer.TryGetSprite(GetSpriteID(front, right), out sprite, true);

}
