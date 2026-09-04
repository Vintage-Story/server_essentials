using Vintagestory.API.Common;

namespace ServerEssentials;

#pragma warning disable CA2211
public static partial class Configuration
{
    internal static void Load(ICoreAPI api)
    {
        LoadHome(api);
        LoadTPA(api);
        LoadBack(api);
        LoadCustomCommands(api);
        LoadTranslations(api);
    }
}
