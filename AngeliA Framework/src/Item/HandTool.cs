using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Tool equipment that can be equip in hand slot of a character
/// </summary>
[EntityAttribute.MapEditorGroup("ItemHandTool")]
public abstract class HandTool : Equipment {

	// VAR
	/// <summary>
	/// Artwork sprite ID for render this tool
	/// </summary>
	public int SpriteID { get; protected set; }
	public sealed override EquipmentType EquipmentType => EquipmentType.HandTool;
	/// <summary>
	/// How many frame does the internal tool logic invoke after character start to use the tool
	/// </summary>
	public int PerformDelayFrame => Duration * PerformDelayRate / 1000;
	/// <summary>
	/// How many frame based on "duration" does the internal tool logic invoke after character start to use the tool. (0 means immediately invoke. 1000 means invoke after "duration" frames)
	/// </summary>
	public virtual int PerformDelayRate => 0;
	/// <summary>
	/// Name of the class type 
	/// </summary>
	public string TypeName { get; init; }
	/// <summary>
	/// How long does the tool perform it's function for once
	/// </summary>
	public virtual int Duration => 12;
	/// <summary>
	/// How long have to wait to use again after the tool being used for once
	/// </summary>
	public virtual int Cooldown => 2;
	/// <summary>
	/// How many extra frames have to wait if the user hold the action button to use the tool multiple time
	/// </summary>
	public virtual int HoldPunish => 4;
	/// <summary>
	/// How many frames does it have to charge to perform the tool as charged. Set to int.MaxValue to disable charge feature.
	/// </summary>
	public virtual int ChargeDuration => int.MaxValue;
	/// <summary>
	/// How fast can the character move when using this tool. (0 means do not move. 1000 means move as normal speed. null means do not effect movement speed.)
	/// </summary>
	public virtual int? DefaultMovementSpeedRateOnUse => null;
	/// <summary>
	/// How fast can the character move when using this tool when walking. (0 means do not move. 1000 means move as normal speed. null means do not effect movement speed.)
	/// </summary>
	public virtual int? WalkingMovementSpeedRateOnUse => null;
	/// <summary>
	/// How fast can the character move when using this tool when running. (0 means do not move. 1000 means move as normal speed. null means do not effect movement speed.)
	/// </summary>
	public virtual int? RunningMovementSpeedRateOnUse => null;
	/// <summary>
	/// True if this tool treat it's inventory stack count as durability bar
	/// </summary>
	public virtual bool UseStackAsUsage => false;
	/// <summary>
	/// True if this tool can be use repeatedly when holding the action button
	/// </summary>
	public virtual bool RepeatWhenHolding => false;
	/// <summary>
	/// True if usage of this tool should be stop when user relase the action button
	/// </summary>
	public virtual bool CancelUseWhenRelease => false;
	/// <summary>
	/// Do not change character facing when the tool is being use
	/// </summary>
	public virtual bool LockFacingOnUse => false;
	/// <summary>
	/// Do not read grab twist data from character when rendering this tool
	/// </summary>
	public virtual bool IgnoreGrabTwist => false;
	/// <summary>
	/// True if character can use this tool when not touching ground
	/// </summary>
	public virtual bool AvailableInAir => true;
	/// <summary>
	/// True if character can use this tool when inside water
	/// </summary>
	public virtual bool AvailableInWater => true;
	/// <summary>
	/// True if character can use this tool when walking
	/// </summary>
	public virtual bool AvailableWhenWalking => true;
	/// <summary>
	/// True if character can use this tool when running
	/// </summary>
	public virtual bool AvailableWhenRunning => true;
	/// <summary>
	/// True if character can use this tool when climbing
	/// </summary>
	public virtual bool AvailableWhenClimbing => false;
	/// <summary>
	/// True if character can use this tool when flying
	/// </summary>
	public virtual bool AvailableWhenFlying => false;
	/// <summary>
	/// True if character can use this tool when rolling
	/// </summary>
	public virtual bool AvailableWhenRolling => false;
	/// <summary>
	/// True if character can use this tool when squatting
	/// </summary>
	public virtual bool AvailableWhenSquatting => false;
	/// <summary>
	/// True if character can use this tool when dashing
	/// </summary>
	public virtual bool AvailableWhenDashing => false;
	/// <summary>
	/// True if character can use this tool when sliding on wall
	/// </summary>
	public virtual bool AvailableWhenSliding => false;
	/// <summary>
	/// True if character can use this tool when grabbing
	/// </summary>
	public virtual bool AvailableWhenGrabbing => false;
	/// <summary>
	/// True if character can use this tool when rushing
	/// </summary>
	public virtual bool AvailableWhenRushing => false;
	/// <summary>
	/// True if character can use this tool when ground pounding
	/// </summary>
	public virtual bool AvailableWhenPounding => false;

	// MSG
	public HandTool () : this(true) { }

	public HandTool (bool loadArtwork) {
		TypeName = GetType().AngeName();
		if (loadArtwork) LoadFromSheet();
	}

	/// <summary>
	/// Load artwork sprite from current rendering sheet
	/// </summary>
	public void LoadFromSheet () {
		SpriteID = $"{GetType().AngeName()}.Main".AngeHash();
		if (!Renderer.HasSprite(SpriteID)) SpriteID = TypeID;
	}

	// API
	/// <summary>
	/// This function is called when the tool is rendering by pose-style character
	/// </summary>
	/// <param name="renderer">Target character</param>
	/// <param name="x">Position X of the sprite in global space</param>
	/// <param name="y">Position Y of the sprite in global space</param>
	/// <param name="width">Width of the sprite in global space</param>
	/// <param name="height">Height of the sprite in global space</param>
	/// <param name="grabRotation">Rotation of the sprite</param>
	/// <param name="grabScale">Size scale of the sprite (0 means 0%, 1000 means 100%)</param>
	/// <param name="sprite">Artwork sprite</param>
	/// <param name="z">Z value for sort rendering cells</param>
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

	/// <summary>
	/// True if the tool can be use by the character
	/// </summary>
	public virtual bool AllowingUse (Character character) => true;

	/// <summary>
	/// This function is called when this tool is used for once by the given character
	/// </summary>
	public virtual void OnToolPerform (Character user) { }

	/// <summary>
	/// Get the ID of the pose animation for handheld
	/// </summary>
	public virtual int GetHandheldPoseAnimationID (Character character) => PoseHandheld_Tool.TYPE_ID;

	/// <summary>
	/// Get the ID of the pose animation for using the tool
	/// </summary>
	public virtual int GetPerformPoseAnimationID (Character character) => PosePerform_Tool.TYPE_ID;

}
