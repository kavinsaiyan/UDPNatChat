if [ $1 == "r" ]
then
    dotnet build UDPConsoleCommonLib/UDPConsoleCommonLib.csproj
fi
dotnet run --project UDPServerConsole/UDPServerConsole.csproj