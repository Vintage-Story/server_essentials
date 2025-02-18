dotnet run --project ./CakeBuild/CakeBuild.csproj -- "$@"
rm -rf "$VINTAGE_STORY/Mods/serveressentials"
cp -r ./Releases/serveressentials "$VINTAGE_STORY/Mods/serveressentials"
