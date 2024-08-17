using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public abstract class CharacterMovementOverride {

	public virtual void PhysicsUpdateGamePlay (CharacterMovement movement) => movement.PhysicsUpdateGamePlay();

}
