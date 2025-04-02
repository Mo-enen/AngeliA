using System.Collections;
using System.Collections.Generic;
using AngeliA;

namespace AngeliA.Platformer;

/// <summary>
/// A type of bullet that shoot into straight line
/// </summary>
public abstract class BeamBullet : MovableBullet {

	// Const
	private const int BEAM_SPEED = Const.CEL * 42;

	// Api
	public sealed override int Duration => 1;
	public sealed override int SpeedForward => BEAM_SPEED;
	public override int MaxRange => Const.CEL * 24;
	/// <summary>
	/// Artwork sprite ID for the beam body
	/// </summary>
	protected abstract int BeamSpriteID { get; }
	/// <summary>
	/// Artwork sprite ID for the beam spark effect
	/// </summary>
	protected abstract int SparkSpriteID { get; }
	/// <summary>
	/// Artwork sprite ID for the beam burst effect in the end-side
	/// </summary>
	protected abstract int BurstSpriteID { get; }
	/// <summary>
	/// Artwork sprite ID for the beam burst effect in the hand-side
	/// </summary>
	protected abstract int HandBurstSpriteID { get; }
	protected virtual int BurstRotateSpeed => 0;
	protected virtual int HandBurstRotateSpeed => 0;
	/// <summary>
	/// Thickness of the beam in global space
	/// </summary>
	protected virtual int BeamSize => 128;
	/// <summary>
	/// Size of the spark effect in global space
	/// </summary>
	protected virtual int SparkSize => 196;
	/// <summary>
	/// Size of the end burst effect in global space
	/// </summary>
	protected virtual int BurstSize => Const.CEL;
	/// <summary>
	/// Size of the hand burst effect in global space
	/// </summary>
	protected virtual int HandBurstSize => 196;
	protected virtual int RenderingLayer => RenderLayer.ADD;
	protected virtual Color32 BeamTint => Color32.WHITE;
	protected virtual Color32 SparkTint => Color32.WHITE_128;
	protected virtual Color32 BurstTint => Color32.WHITE;
	protected virtual Color32 HandBurstTint => Color32.WHITE;
	/// <summary>
	/// Radius in unit space for the LightingSystem
	/// </summary>
	protected virtual int IllumanteUnitRadius => 3;
	/// <summary>
	/// How bright of the illumante (0 means no illumante. 1000 means general amount)
	/// </summary>
	protected virtual int IllumanteAmount => 300;
	/// <summary>
	/// True if show burst effect when hit a IDamageReceiver, not just hit the environment
	/// </summary>
	protected virtual bool OnlyShowBurstWhenHitReceiver => true;

	// Data
	private bool BeamRendered = false;


	// MSG
	public override void OnActivated () {
		base.OnActivated();
		BeamRendered = false;
		Height = Const.HALF * 2 / 3;
	}

	public override void LateUpdate () {

		if (BeamRendered) return;
		BeamRendered = true;

		// Render Beam
		var (x, y, endX, endY, h, rot1000, hitReceiver) = GetLastUpdatedTramsform();

		// Beam
		if (BeamSpriteID != 0) {
			GroupAnimationHolder.Spawn(
				BeamSpriteID, x, y, BeamSize, h,
				Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT,
				rot1000, 0, -1, 1, false, BeamTint, renderLayer: RenderingLayer
			);
		}

		// Spark 
		if (SparkSpriteID != 0) {
			GroupAnimationHolder.Spawn(
				SparkSpriteID, x, y,
				SparkSize, h,
				Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT,
				rot1000, 0, -1, 1, false, SparkTint, renderLayer: RenderingLayer
			);
		}

		// Burst 
		if ((!OnlyShowBurstWhenHitReceiver || hitReceiver) && BurstSpriteID != 0) {
			GroupAnimationHolder.Spawn(
				BurstSpriteID, endX, endY, BurstSize, BurstSize,
				Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT,
				0, BurstRotateSpeed,
				-1, 1, true, BurstTint, renderLayer: RenderingLayer
			);
		}

		// Hand Burst
		if (HandBurstSpriteID != 0) {
			GroupAnimationHolder.Spawn(
				HandBurstSpriteID, x, y, HandBurstSize, HandBurstSize,
				Const.ORIGINAL_PIVOT, Const.ORIGINAL_PIVOT,
				rot1000, HandBurstRotateSpeed,
				-1, 1, true, HandBurstTint, renderLayer: RenderingLayer
			);
		}

		// Illu
		int illuAmount = IllumanteAmount;
		if (illuAmount > 0) {
			int len = h / Const.CEL;
			for (int i = 0; i < len; i++) {
				LightingSystem.Illuminate(
					x + i * (endX - x) / len,
					y + i * (endY - y) / len,
					IllumanteUnitRadius * Const.CEL, illuAmount
				);
			}
		}
	}

	protected override void BeforeDespawn (IDamageReceiver receiver) { }

}

