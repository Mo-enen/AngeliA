namespace AngeliA;

public struct OrientedSprite {

	// VAR
	public readonly bool IsValid => (SpriteID_FL != 0 && SpriteID_FR != 0 && SpriteID_BL != 0 && SpriteID_BR != 0);

	public string AttachmentName;
	public int SpriteID_FL;
	public int SpriteID_FR;
	public int SpriteID_BL;
	public int SpriteID_BR;

	// MSG
	public OrientedSprite () => SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = 0;
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

	// API
	public bool LoadFromSheet (string hostName, string attachmentName) {

		SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = 0;

		// Basic
		int basicID = $"{hostName}.{attachmentName}".AngeHash();
		if (IsValid(basicID)) SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = basicID;

		// .L
		int lID = $"{hostName}.{attachmentName}.L".AngeHash();
		if (IsValid(lID)) SpriteID_FL = SpriteID_BL = lID;

		// .R
		int rID = $"{hostName}.{attachmentName}.R".AngeHash();
		if (IsValid(rID)) SpriteID_FR = SpriteID_BR = rID;

		// .F
		int fID = $"{hostName}.{attachmentName}.F".AngeHash();
		if (IsValid(fID)) SpriteID_FL = SpriteID_FR = fID;

		// .B
		int bID = $"{hostName}.{attachmentName}.B".AngeHash();
		if (IsValid(bID)) SpriteID_BL = SpriteID_BR = bID;

		// .FL
		int flID = $"{hostName}.{attachmentName}.FL".AngeHash();
		if (IsValid(flID)) SpriteID_FL = flID;

		// .FR
		int frID = $"{hostName}.{attachmentName}.FR".AngeHash();
		if (IsValid(frID)) SpriteID_FR = frID;

		// .BL
		int blID = $"{hostName}.{attachmentName}.BL".AngeHash();
		if (IsValid(blID)) SpriteID_BL = blID;

		// .BR
		int brID = $"{hostName}.{attachmentName}.BR".AngeHash();
		if (IsValid(brID)) SpriteID_BR = brID;

		// Final
		if (SpriteID_FL == 0 && SpriteID_FR == 0 && SpriteID_BL == 0 && SpriteID_BR == 0) {
			AttachmentName = "";
			return false;
		} else {
			AttachmentName = attachmentName;
			return true;
		}

		// Func
		static bool IsValid (int id) => Renderer.HasSprite(id) || Renderer.HasSpriteGroup(id);
	}
	public readonly int GetID (bool front, bool right) => front ? (right ? SpriteID_FR : SpriteID_FL) : (right ? SpriteID_BR : SpriteID_BL);
	public readonly bool TryGetSpriteWithoutAnimation (bool front, bool right, out AngeSprite sprite) => Renderer.TryGetSprite(GetID(front, right), out sprite, true);
	public readonly bool TryGetSpriteGroup (bool front, bool right, out SpriteGroup group) => Renderer.TryGetSpriteGroup(GetID(front, right), out group);
	public readonly bool TryGetSprite (bool front, bool right, int animationFrame, out AngeSprite sprite) {
		int id = GetID(front, right);
		if (Renderer.TryGetSprite(id, out sprite, true)) {
			return true;
		} else if (Renderer.TryGetAnimationGroup(id, out var aniGroup)) {
			return Renderer.CurrentSheet.TryGetSpriteFromAnimationFrame(aniGroup, animationFrame, out sprite);
		} else {
			sprite = null;
			return false;
		}
	}

}
