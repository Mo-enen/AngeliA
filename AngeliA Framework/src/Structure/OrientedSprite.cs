namespace AngeliA;

/// <summary>
/// A set of AngeSprite ID that auto handles orientation logic
/// </summary>
public struct OrientedSprite {

	// VAR
	/// <summary>
	/// True if any sprite ID is not zero
	/// </summary>
	public readonly bool IsValid => (SpriteID_FL != 0 && SpriteID_FR != 0 && SpriteID_BL != 0 && SpriteID_BR != 0);

	/// <summary>
	/// Name of the source unit (eg. "Tail" "UpperArm" "Mouth")
	/// </summary>
	public string AttachmentName;

	/// <summary>
	/// Sprite ID for facing Front-Left
	/// </summary>
	public int SpriteID_FL;

	/// <summary>
	/// Sprite ID for facing Front-Right
	/// </summary>
	public int SpriteID_FR;

	/// <summary>
	/// Sprite ID for facing Back-Left
	/// </summary>
	public int SpriteID_BL;

	/// <summary>
	/// Sprite ID for facing Back-Right
	/// </summary>
	public int SpriteID_BR;

	// MSG
	public OrientedSprite () => SpriteID_FL = SpriteID_FR = SpriteID_BL = SpriteID_BR = 0;
	/// <summary>
	/// Create a new OSprite from current render sheet
	/// </summary>
	/// <param name="hostName">Which character own this OSprite</param>
	/// <param name="attachmentName">Which part of the character own this OSprite</param>
	public OrientedSprite (string hostName, string attachmentName) => LoadFromSheet(hostName, attachmentName);
	/// <summary>
	/// Create a new OSprite from current render sheet
	/// </summary>
	/// <param name="hostName">Which character own this OSprite</param>
	/// <param name="attachmentName">Which part of the character own this OSprite</param>
	/// <param name="attachmentNameAlt">Failback attachment name if "attachmentName" not found</param>
	public OrientedSprite (string hostName, string attachmentName, string attachmentNameAlt) {
		if (!LoadFromSheet(hostName, attachmentName)) {
			LoadFromSheet(hostName, attachmentNameAlt);
		}
	}
	/// <summary>
	/// Create a new OSprite from current render sheet
	/// </summary>
	/// <param name="hostName">Which character own this OSprite</param>
	/// <param name="attachmentName">Which part of the character own this OSprite</param>
	/// <param name="attachmentNameAltA">Failback attachment name if "attachmentName" not found</param>
	/// <param name="attachmentNameAltB">Failback attachment name if "attachmentNameA" not found</param>
	public OrientedSprite (string hostName, string attachmentName, string attachmentNameAltA, string attachmentNameAltB) {
		if (!LoadFromSheet(hostName, attachmentName) && !LoadFromSheet(hostName, attachmentNameAltA)) {
			LoadFromSheet(hostName, attachmentNameAltB);
		}
	}
	/// <summary>
	/// Create a new OSprite from current render sheet
	/// </summary>
	/// <param name="hostName">Which character own this OSprite</param>
	/// <param name="attachmentNames">Which part of the character own this OSprite. Will be load by the order of the array until one name is valid.</param>
	public OrientedSprite (string hostName, params string[] attachmentNames) {
		foreach (var attachmentName in attachmentNames) {
			if (LoadFromSheet(hostName, attachmentName)) break;
		}
	}

	// API
	/// <summary>
	/// Load this OSprite from current render sheet
	/// </summary>
	/// <param name="hostName">Which character own this OSprite</param>
	/// <param name="attachmentName">Which part of the character own this OSprite</param>
	/// <returns>True if successfuly loaded</returns>
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

	/// <summary>
	/// Get the sprite ID for rendering
	/// </summary>
	/// <param name="front">True if the sprite is for facing front</param>
	/// <param name="right">True if the sprite is for facing right</param>
	public readonly int GetID (bool front, bool right) => front ? (right ? SpriteID_FR : SpriteID_FL) : (right ? SpriteID_BR : SpriteID_BL);

	/// <summary>
	/// Get the sprite ID for rendering without auto loading animated sprites
	/// </summary>
	/// <param name="front">True if the sprite is for facing front</param>
	/// <param name="right">True if the sprite is for facing right</param>
	/// <param name="sprite">Result sprite</param>
	/// <returns>True if the sprite is founded</returns>
	public readonly bool TryGetSpriteWithoutAnimation (bool front, bool right, out AngeSprite sprite) => Renderer.TryGetSprite(GetID(front, right), out sprite, true);

	/// <summary>
	/// Get the sprite group for rendering
	/// </summary>
	/// <param name="front">True if the sprite is for facing front</param>
	/// <param name="right">True if the sprite is for facing right</param>
	/// <param name="group">Result group</param>
	/// <returns>True if the group is founded</returns>
	public readonly bool TryGetSpriteGroup (bool front, bool right, out SpriteGroup group) => Renderer.TryGetSpriteGroup(GetID(front, right), out group);

	/// <summary>
	/// Get the sprite for rendering
	/// </summary>
	/// <param name="front">True if the sprite is for facing front</param>
	/// <param name="right">True if the sprite is for facing right</param>
	/// <param name="animationFrame">Local frame for animation</param>
	/// <param name="sprite">Result sprite</param>
	/// <returns>True if the sprite is founded</returns>
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

	/// <summary>
	/// Get the sprite for rendering as gizmos
	/// </summary>
	/// <param name="sprite">Result sprite</param>
	/// <returns>True if the sprite is founded</returns>
	public readonly bool TryGetSpriteForGizmos (out AngeSprite sprite) => Renderer.TryGetSpriteForGizmos(GetID(true, true), out sprite);

}
