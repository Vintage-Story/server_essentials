using System.Collections.Generic;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace ServerEssentials;

public class BackConfiguration
{
    public bool enableBackCommand = true;
    public string backPrivilege = "chat";
    public List<string> backSyntaxes = ["back"];
    public int backCooldown = 120;
    public string backCostItemId = "";
    public int backCostQuantity = 0;
    public int backCommandDelay = 5;
    public int backCommandDuration = 300;
    public bool backCommandCanMove = false;
    public bool backCommandCanReceiveDamage = false;
    public bool enableBackForHome = true;
    public bool enableBackForTpa = true;
    public bool enableBackForDeath = true;
    public bool enableBackResycle = false;
}

public static partial class Configuration
{
    public static BackConfiguration Back = new();

    private static void LoadBack(ICoreAPI api)
        => Back = ConfigManager.LoadModConfig<BackConfiguration>(api, "ServerEssentials", "back", ServerEssentialsModSystem.Logger);
}
