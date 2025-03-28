using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

/// <summary>
/// Interface that makes the item break/repair into other item
/// </summary>
public interface IProgressiveItem {
	/// <summary>
	/// Current localtion inside the profress chain
	/// </summary>
	int Progress { get; set; }
	/// <summary>
	/// Total count of the profress chain
	/// </summary>
	int TotalProgress { get; set; }
	/// <summary>
	/// ID of the item that this item will become when it's broken
	/// </summary>
	int PrevItemID { get; set; }
	/// <summary>
	/// ID of the item that this item will become when it got repair
	/// </summary>
	int NextItemID { get; set; }
}