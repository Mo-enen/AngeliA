
dotnet publish -c release -p:OutputPath="Build" -p:PublishDir="Publish" -p:DebugType="none" -p:PublishSingleFile="true" -p:SelfContained="true" -p:PublishTrimmed="false" -p:RuntimeIdentifier="win-x64" -p:IncludeAllContentForSelfExtract="false" -p:PublishReadyToRun="false" -p:IncludeNativeLibrariesForSelfExtract="true" -p:EnableCompressionInSingleFile="true"
