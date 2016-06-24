cd /d "C:\Users\ihey\Documents\HeatSource\HeatSource" &msbuild "HeatSource.csproj" /t:sdvViewer /p:configuration="Debug" /p:platform=Any CPU
exit %errorlevel% 