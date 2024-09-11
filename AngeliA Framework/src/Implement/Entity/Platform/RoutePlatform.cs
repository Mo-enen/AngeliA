using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class RoutinePlatform : StepTriggerPlatform {


	// Data
	private bool Moving = false;
	private RoutinePlatform Leader = null;
	private Int2 LeaderOffset;

	// MSG
	public override void OnActivated () {
		base.OnActivated();
		Moving = false;
		Leader = null;
	}


	public override void FirstUpdate () {
		base.FirstUpdate();
		// Self Leader Check
		if (Leader == null) {

			//Leader
			//LeaderOffset




		}
	}


	protected override void Move () {
		if (!Moving || Leader == null) return;
		if (Leader == this) {
			// Lead


		} else {
			// Follow


		}
	}


	protected override void OnTriggered (object data) => Moving = true;


}
