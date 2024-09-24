using System.Collections;
using System.Collections.Generic;

namespace AngeliA;

public interface IProgressiveItem {
	int Progress { get; set; }
	int TotalProgress { get; set; }
	int PrevItemID { get; set; }
	int NextItemID { get; set; }
}