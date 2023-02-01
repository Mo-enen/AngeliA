using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AngeliaFramework;


namespace Yaya {
	public abstract class eActionEntity : Entity {


		// Api
		public int HighlightFrame { get; set; } = int.MinValue;
		public virtual bool LockInput => false;
		public bool IsHighlighted => Game.GlobalFrame <= HighlightFrame + 1;
		public virtual bool AnimateOnHighlight => true;

		// Data
		private static readonly Dictionary<int, int> HintCodePool = new();
		protected int HighlightBlinkOffset = 0;


		// MSG
		[AfterGameInitialize]
		public static void Initialize () {
			HintCodePool.Clear();
			var BOTTOM_TYPE = typeof(eActionEntity);
			foreach (var type in typeof(eActionEntity).AllChildClass()) {
				var _type = type;
				while (_type != null && _type != BOTTOM_TYPE) {
					string name = _type.Name;
					if (name[0] == 'e') name = name[1..];
					int code = $"ActionHint.{name}".AngeHash();
					if (Language.Has(code)) {
						HintCodePool.TryAdd(type.AngeHash(), code);
						break;
					}
					_type = _type.BaseType;
				}
			}
		}


		// API
		public virtual void Highlight () {
			if (HighlightFrame < Game.GlobalFrame - 1) {
				HighlightBlinkOffset = -Game.GlobalFrame;
			}
			HighlightFrame = Game.GlobalFrame;
		}


		public abstract bool Invoke (Entity target);


		public virtual void CancelInvoke (Entity target) { }


		public virtual bool AllowInvoke (Entity target) => true;


		public static int GetHintLanguageCode (int typeID) => HintCodePool.TryGetValue(typeID, out int result) ? result : WORD.HINT_USE;


		// UTL
		protected void HighlightBlink (Cell cell) {
			if (!AnimateOnHighlight || !IsHighlighted) return;
			if ((Game.GlobalFrame + HighlightBlinkOffset) % 30 <= 15) {
				const int OFFSET = Const.CEL / 20;
				cell.Width += OFFSET * 2;
				cell.Height += OFFSET * 2;
			}
		}


		protected void HighlightBlink (Cell cell, Direction3 moduleType, FittingPose pose) {
			if (!AnimateOnHighlight || !IsHighlighted) return;
			// Highlight
			int offset = (Game.GlobalFrame + HighlightBlinkOffset) % 30 > 15 ? 0 : Const.CEL / 20;
			if (moduleType == Direction3.Horizontal) {
				// Horizontal
				if (pose == FittingPose.Left || pose == FittingPose.Single) {
					cell.X -= offset;
				}
				if (pose != FittingPose.Mid) {
					if (pose == FittingPose.Left) {
						cell.Width += offset;
					} else {
						cell.Width += offset * 2;
					}
				}
				cell.Y -= offset;
				cell.Height += offset * 2;
			} else {
				// Vertical
				if (pose == FittingPose.Down || pose == FittingPose.Single) {
					cell.Y -= offset;
				}
				if (pose != FittingPose.Mid) {
					if (pose == FittingPose.Down) {
						cell.Height += offset;
					} else {
						cell.Height += offset * 2;
					}
				}
				cell.X -= offset;
				cell.Width += offset * 2;
			}
		}


	}
}