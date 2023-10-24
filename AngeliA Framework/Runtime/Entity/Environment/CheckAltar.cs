using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


namespace AngeliaFramework {



	[System.AttributeUsage(System.AttributeTargets.Class)]
	public class LinkedCheckPointAttribute : System.Attribute {
		public System.Type LinkedCheckPoint;
		public LinkedCheckPointAttribute (System.Type linkedCheckPoint) => LinkedCheckPoint = linkedCheckPoint;
	}



	[EntityAttribute.Bounds(0, 0, Const.CEL, Const.CEL * 2)]
	[EntityAttribute.Capacity(1, 1)]
	public abstract class CheckAltar : CheckPoint, IGlobalPosition {




		#region --- VAR ---


		// Api
		protected sealed override bool OnlySpawnWhenUnlocked => false;

		// Data
		private static Vector3Int? _TurnBackUnitPosition = default;
		private int LinkedCheckPointID { get; init; } = 0;


		#endregion




		#region --- MSG ---


		public CheckAltar () {
			var linkedAtt = GetType().GetCustomAttribute<LinkedCheckPointAttribute>();
			LinkedCheckPointID = linkedAtt != null ? linkedAtt.LinkedCheckPoint.AngeHash() : 0;
		}


		[OnGameInitialize(-64)]
		public static void BeforeGameInitializeLater () {
			foreach (var type in typeof(CheckAltar).AllChildClass()) {
				if (System.Activator.CreateInstance(type) is not CheckAltar altar) continue;
				Link(type.AngeHash(), altar.LinkedCheckPointID);
			}
		}


		public override void OnActivated () {
			base.OnActivated();
			Height = Const.CEL * 2;
		}


		protected override void OnPlayerTouched (Vector3Int unitPos) {
			_TurnBackUnitPosition = Player.RespawnCpUnitPosition;
			base.OnPlayerTouched(unitPos);
			Unlock(LinkedCheckPointID);
		}


		protected override bool TryGetTurnBackUnitPosition (out Vector3Int unitPos) {
			unitPos = _TurnBackUnitPosition ?? default;
			return _TurnBackUnitPosition.HasValue;
		}


		#endregion




	}
}