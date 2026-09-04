using System.Collections.Generic;
using Newtonsoft.Json;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace ServerEssentials;

public class CustomCommandEntry
{
    [JsonProperty("syntax")]
    public string Syntax { get; set; } = "";
    [JsonProperty("message")]
    public string Message { get; set; } = "";
    [JsonProperty("privilege")]
    public string Privilege { get; set; } = "chat";
}

public class CustomCommandsConfiguration
{
    public List<CustomCommandEntry> customCommands = [];
    public bool enableExtendedLog = true;
}

public static partial class Configuration
{
    public static CustomCommandsConfiguration CustomCommands = new();

    private static void LoadCustomCommands(ICoreAPI api)
        => CustomCommands = ConfigManager.LoadModConfig<CustomCommandsConfiguration>(api, "ServerEssentials", "base", ServerEssentialsModSystem.Logger, "serveressentials:config/base.json");
}
