using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AngeliaFramework {
	public abstract class BodyGadget {


		protected abstract string BaseTypeName { get; }

		public string GetDisplayName () {
			string baseName = BaseTypeName;
			string smartTypeName = GetType().AngeName();
			if (smartTypeName.EndsWith(baseName) && smartTypeName.Length > baseName.Length) {
				smartTypeName = smartTypeName[..^baseName.Length];
			}
			int smartID = $"Pat.{smartTypeName}".AngeHash();
			string suffix = Language.Get($"UI.GadgetSuffix.{baseName}".AngeHash(), $" {baseName}");
			return $"{Language.Get(smartID, Util.GetDisplayName(smartTypeName))}{suffix}";
		}

	}
}
