
copy "..\AngeliA Engine\Universe\Info.json" "..\AngeliA Engine\Universe\Obsolete Info.json"

dotnet publish "../AngeliA Framework/" -c release -p:OutputPath="../AngeliA Framework/Publish Temp" -p:PublishDir="../AngeliA Engine/Universe/ProjectTemplate/lib/Release" -p:DebugType="none" 
dotnet publish "../AngeliA Framework/" -c release -p:OutputPath="../AngeliA Framework/Publish Temp" -p:PublishDir="../AngeliA Engine/Universe/Runtime/Release" -p:DebugType="none" 
dotnet publish "../AngeliA Raylib/" -c release -p:OutputPath="../AngeliA Raylib/Publish Temp" -p:PublishDir="../AngeliA Engine/Universe/Runtime/Release" -p:DebugType="none" 

@rd /S /Q "../AngeliA Entry/Publish Temp"
@rd /S /Q "../AngeliA Framework/Publish Temp"
@rd /S /Q "../AngeliA Raylib/Publish Temp"

pause