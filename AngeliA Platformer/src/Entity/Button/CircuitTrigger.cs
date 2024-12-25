using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AngeliA;

namespace AngeliA.Platformer;


[EntityAttribute.Layer(EntityLayer.ENVIRONMENT)]
public abstract class CircuitTrigger : Entity, IBlockEntity {

	// VAR
	public abstract bool TriggerOnLeft { get; }
	public abstract bool TriggerOnRight { get; }
	public abstract bool TriggerOnBottom { get; }
	public abstract bool TriggerOnTop { get; }

	// MSG
	public override void LateUpdate () {
		base.LateUpdate();
		// Circuit Gizmos
		const int SIZE = 46;
		var rect = new IRect(X, Y, Const.CEL, Const.CEL);
		if (TriggerOnLeft) {
			DrawCircle(rect.CornerOutside(Alignment.MidLeft, SIZE), new Color32(0, 120, 166, 255));
		}
		if (TriggerOnRight) {
			DrawCircle(rect.CornerOutside(Alignment.MidRight, SIZE), new Color32(0, 120, 166, 255));
		}
		if (TriggerOnBottom) {
			DrawCircle(rect.CornerOutside(Alignment.BottomMid, SIZE), new Color32(142, 55, 56, 255));
		}
		if (TriggerOnTop) {
			DrawCircle(rect.CornerOutside(Alignment.TopMid, SIZE), new Color32(142, 55, 56, 255));
		}
		static void DrawCircle (IRect rect, Color32 tint) {
			Renderer.Draw(BuiltInSprite.CIRCLE_32, rect.Expand(SIZE / 6), Color32.WHITE);
			Renderer.Draw(BuiltInSprite.CIRCLE_32, rect, tint);
		}
	}

	public virtual void TriggerCircuit () => IWire.TriggerCircuit(
		WorldSquad.Stream,
		(X + 1).ToUnit(), (Y + 1).ToUnit(), Stage.ViewZ,
		maxUnitDistance: Const.MAP,
		TriggerOnLeft, TriggerOnRight, TriggerOnBottom, TriggerOnTop
	);

}
