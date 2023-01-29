using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	[EntityAttribute.UpdateOutOfRange]
	[EntityAttribute.MapEditorGroup("Zone")]
	public abstract class eZone : Entity {


		// Api
		protected abstract RectInt? Range { get; set; }

		// MSG
		public override void BeforePhysicsUpdate () {
			base.BeforePhysicsUpdate();
			Range = null;
		}


		public override void PhysicsUpdate () {
			base.PhysicsUpdate();
			if (!Range.HasValue) {
				Range = Rect;
			} else {
				var range = Range.Value;
				var rect = Rect;
				range.SetMinMax(
					Mathf.Min(range.xMin, rect.xMin),
					Mathf.Max(range.xMax, rect.xMax),
					Mathf.Min(range.yMin, rect.yMin),
					Mathf.Max(range.yMax, rect.yMax)
				);
				Range = range;
			}
		}


		public override void FrameUpdate () {
			base.FrameUpdate();
			if (Range.HasValue) {
				PerformRange(Range.Value);
			}
		}


		protected abstract void PerformRange (RectInt range);


	}
}