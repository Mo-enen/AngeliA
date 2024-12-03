namespace AngeliA;

public struct OrientedSprite {

	public readonly bool IsValid => SpriteID_FL != 0 && SpriteID_FR != 0 && SpriteID_BL != 0 && SpriteID_BR != 0;

	public string AttachmentName;
	public int GroupID;
	public int SpriteID_FL;
	public int SpriteID_FR;
	public int SpriteID_BL;
	public int SpriteID_BR;

	public OrientedSprite () => SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = GroupID = 0;
	public OrientedSprite (string hostName, string attachmentName) => LoadFromSheet(hostName, attachmentName);
	public OrientedSprite (string hostName, string attachmentName, string attachmentNameAlt) {
		if (!LoadFromSheet(hostName, attachmentName)) {
			LoadFromSheet(hostName, attachmentNameAlt);
		}
	}
	public OrientedSprite (string hostName, string attachmentName, string attachmentNameAltA, string attachmentNameAltB) {
		if (!LoadFromSheet(hostName, attachmentName) && !LoadFromSheet(hostName, attachmentNameAltA)) {
			LoadFromSheet(hostName, attachmentNameAltB);
		}
	}
	public OrientedSprite (string hostName, params string[] attachmentNames) {
		foreach (var attachmentName in attachmentNames) {
			if (LoadFromSheet(hostName, attachmentName)) break;
		}
	}

	public bool LoadFromSheet (string hostName, string attachmentName) {

		AttachmentName = attachmentName;

		SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = $"{hostName}.{attachmentName}".AngeHash();
		if (Renderer.HasSprite(SpriteID_FL)) return true;
		if (Renderer.HasSpriteGroup(SpriteID_FL)) {
			GroupID = SpriteID_FL;
			SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = 0;
			return true;
		}

		SpriteID_FL = SpriteID_FR = $"{hostName}.{attachmentName}.F".AngeHash();
		SpriteID_BL = SpriteID_BR = $"{hostName}.{attachmentName}.B".AngeHash();
		if (!Renderer.HasSprite(SpriteID_BL)) SpriteID_BL = SpriteID_BR = SpriteID_FL;
		if (Renderer.HasSprite(SpriteID_FL)) return true;

		SpriteID_FL = SpriteID_BL = $"{hostName}.{attachmentName}.L".AngeHash();
		SpriteID_FR = SpriteID_BR = $"{hostName}.{attachmentName}.R".AngeHash();
		if (Renderer.HasSprite(SpriteID_FL) && Renderer.HasSprite(SpriteID_FR)) return true;

		SpriteID_FL = $"{hostName}.{attachmentName}.FL".AngeHash();
		SpriteID_FR = $"{hostName}.{attachmentName}.FR".AngeHash();
		SpriteID_BL = $"{hostName}.{attachmentName}.BL".AngeHash();
		SpriteID_BR = $"{hostName}.{attachmentName}.BR".AngeHash();
		if (!Renderer.HasSprite(SpriteID_BL)) SpriteID_BL = SpriteID_FL;
		if (!Renderer.HasSprite(SpriteID_BR)) SpriteID_BR = SpriteID_FR;
		if (Renderer.HasSprite(SpriteID_FL) && Renderer.HasSprite(SpriteID_FR)) return true;

		SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = 0;
		AttachmentName = "";

		return false;
	}
	public readonly int GetSpriteID (bool front, bool right) => front ? (right ? SpriteID_FR : SpriteID_FL) : (right ? SpriteID_BR : SpriteID_BL);
	public readonly bool TryGetSprite (bool front, bool right, out AngeSprite sprite) => Renderer.TryGetSprite(GetSpriteID(front, right), out sprite, true);

}
