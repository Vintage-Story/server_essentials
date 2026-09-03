using OpenConfiguration;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ServerEssentials;

public class Initialization : ModSystem
{
    internal static ModLogger Logger = ModLogger.None;

    #region Commands
    private Commands.Home homeCommands;
    private Commands.TPA tpaCommands;
    private Commands.Back backCommands;
    private Commands.CustomCommands customCommands;
    #endregion

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        Logger = new ModLogger(api.Logger, "ServerEssentials");
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        Logger.Log($"Running on Version: {Mod.Info.Version}");

        homeCommands = new(api);
        tpaCommands = new(api);
        backCommands = new(api);
        customCommands = new(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        Configuration.Load(api);
        Logger.ExtendedLoggingEnabled = Configuration.CustomCommands.enableExtendedLog;
        Logger.Log("Configurations Loaded");
    }
}
