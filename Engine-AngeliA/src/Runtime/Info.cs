using AngeliA;
using AngeliaRuntime;

using System.Diagnostics.CodeAnalysis;
[assembly: AngeliaGameTitle("AngeliA Engine")]
[assembly: AngeliaGameDeveloper("Moenen", " ")]
[assembly: AngeliaVersion(0, 0, 1)]
[assembly: AngeliaProjectType(ProjectType.Application)]
[assembly: RequireEventWaiting]

[assembly: SuppressMessage(
	"Style",
	"IDE0063:使用简单的 \"using\" 语句",
	Justification = "<挂起>",
	Scope = "namespaceanddescendants",
	Target = "~N:AngeliaEngine")
]
