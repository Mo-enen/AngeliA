using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

[EntityAttribute.MapEditorGroup("ItemHandTool")]
public abstract class HandTool : Equipment {

	// VAR
	public int SpriteID { get; protected set; }
	public sealed override EquipmentType EquipmentType => EquipmentType.HandTool;
	public int PerformDelayFrame => Duration * PerformDelayRate / 1000;
	public virtual int PerformDelayRate => 0;
	public string TypeName { get; init; }
	public virtual int Duration => 12;
	public virtual int Cooldown => 2;
	public virtual int HoldPunish => 4;
	public virtual int ChargeDuration => int.MaxValue;
	public virtual int? DefaultMovementSpeedRateOnUse => null;
	public virtual int? WalkingMovementSpeedRateOnUse => null;
	public virtual int? RunningMovementSpeedRateOnUse => null;
	public virtual bool UseStackAsUsage => false;
	public virtual bool RepeatWhenHolding => false;
	public virtual bool CancelUseWhenRelease => false;
	public virtual bool LockFacingOnUse => false;
	public virtual bool IgnoreGrabTwist => false;
	public virtual bool AvailableInAir => true;
	public virtual bool AvailableInWater => true;
	public virtual bool AvailableWhenWalking => true;
	public virtual bool AvailableWhenRunning => true;
	public virtual bool AvailableWhenClimbing => false;
	public virtual bool AvailableWhenFlying => false;
	public virtual bool AvailableWhenRolling => false;
	public virtual bool AvailableWhenSquatting => false;
	public virtual bool AvailableWhenDashing => false;
	public virtual bool AvailableWhenSliding => false;
	public virtual bool AvailableWhenGrabbing => false;
	public virtual bool AvailableWhenRushing => false;
	public virtual bool AvailableWhenPounding => false;

	// MSG
	public HandTool () : this(true) { }

	public HandTool (bool loadArtwork) {
		TypeName = GetType().AngeName();
		if (loadArtwork) LoadFromSheet();
	}

	public void LoadFromSheet () {
		SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
		if (!Renderer.HasSprite(SpriteID)) SpriteID = TypeID;
	}

	// API
	public virtual Cell OnToolSpriteRendered (PoseCharacterRenderer renderer, int x, int y, int width, int height, int grabRotation, int grabScale, AngeSprite sprite, int z) {
		return Renderer.Draw(
			sprite,
			x, y,
			sprite.PivotX, sprite.PivotY, grabRotation,
			width * grabScale / 1000,
			height * grabScale.Abs() / 1000,
			z
		);
	}

	public virtual bool AllowingUse (Character character) => true;

	public virtual void OnToolPerform (Character user) { }

	public virtual int GetHandheldPoseAnimationID (Character character) => PoseHandheld_Tool.TYPE_ID;

	public virtual int GetPerformPoseAnimationID (Character character) => PosePerform_Tool.TYPE_ID;

}
