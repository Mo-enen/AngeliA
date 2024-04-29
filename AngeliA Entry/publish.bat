
dotnet publish -c debug -p:OutputPath="Build" -p:PublishDir="../Engine-AngeliA/Universe/Runtime/Debug" -p:DebugType="none" -p:PublishSingleFile="true" -p:SelfContained="true" -p:PublishTrimmed="false" -p:RuntimeIdentifier="win-x64" -p:IncludeAllContentForSelfExtract="false" -p:PublishReadyToRun="false" -p:IncludeNativeLibrariesForSelfExtract="true" -p:EnableCompressionInSingleFile="true"

dotnet publish "../AngeliA Framework/" -c debug -p:OutputPath="../Engine-AngeliA/Universe/ProjectTemplate/lib/Debug" -p:DebugType="none" 
dotnet publish "../AngeliA Framework/" -c release -p:OutputPath="../Engine-AngeliA/Universe/ProjectTemplate/lib/Release" -p:DebugType="none" 
dotnet publish "../AngeliA Framework/" -c release -p:OutputPath="../Engine-AngeliA/Universe/Runtime/Release" -p:DebugType="none" 
dotnet publish "../AngeliA Raylib/" -c release -p:OutputPath="../Engine-AngeliA/Universe/Runtime/Release" -p:DebugType="none" 

@RD /S /Q "Build"
@RD /S /Q "../AngeliA Framework/Build"
@RD /S /Q "../AngeliA Raylib/Build"