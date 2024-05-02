
dotnet publish "AngeliA Rigged.csproj" -c debug -p:OutputPath="Build" -p:PublishDir="../Engine-AngeliA/Universe/Runtime/Debug" -p:DebugType="none" -p:PublishSingleFile="true" -p:SelfContained="true" -p:PublishTrimmed="false" -p:RuntimeIdentifier="win-x64" -p:IncludeAllContentForSelfExtract="false" -p:PublishReadyToRun="false" -p:IncludeNativeLibrariesForSelfExtract="true" -p:EnableCompressionInSingleFile="true"

 @RD /S /Q "Build"
