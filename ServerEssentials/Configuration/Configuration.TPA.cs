using System.Collections.Generic;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace ServerEssentials;

public class TPAConfiguration
{
    public bool enableTpaCommand = true;
    public string tpaPrivilege = "chat";
    public List<string> tpaSyntaxes = ["tpa"];
    public int tpaCommandDelay = 5;
    public int tpaCooldown = 120;
    public string tpaCostItemId = "";
    public int tpaCostQuantity = 0;
    public int tpaTimeout = 10;
    public bool tpaCommandCanMove = false;
    public bool tpaCommandCanReceiveDamage = false;
    public bool tpaCommandResetCooldownOnCancellation = true;
    public bool tpaAutoAccept = false;
    public bool enableTpaAcceptCommand = true;
    public string tpaAcceptPrivilege = "chat";
    public List<string> tpaAcceptSyntaxes = ["tpaaccept", "tpaccept", "tpaa"];
    public bool enableTpaDenyCommand = true;
    public string tpaDenyPrivilege = "chat";
    public List<string> tpaDenySyntaxes = ["tpadeny", "tpad"];
    public bool enableTpaCancelCommand = true;
    public string tpaCancelPrivilege = "chat";
    public List<string> tpaCancelSyntaxes = ["tpacancel", "tpac"];
}

public static partial class Configuration
{
    public static TPAConfiguration TPA = new();

    private static void LoadTPA(ICoreAPI api)
        => TPA = ConfigManager.LoadModConfig<TPAConfiguration>(api, "ServerEssentials", "base", ServerEssentialsModSystem.Logger, "serveressentials:config/base.json");
}
