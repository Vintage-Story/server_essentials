using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ServerEssentials;

public class Initialization : ModSystem
{
    #region Commands
    private Commands.Home homeCommands;
    private Commands.TPA tpaCommands;
    private Commands.Back backCommands;
    #endregion

    public override void StartServerSide(ICoreServerAPI api)
    {
        base.StartServerSide(api);
        Debug.LoadLogger(api.Logger);

        Debug.Log($"Running on Version: {Mod.Info.Version}");

        homeCommands = new(api);
        tpaCommands = new(api);
        backCommands = new(api);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        Configuration.UpdateBaseConfigurations(api);
    }
}

public class Debug
{
    static private ILogger loggerForNonTerminalUsers;

    static public void LoadLogger(ILogger logger) => loggerForNonTerminalUsers = logger;
    static public void Log(string message)
        => loggerForNonTerminalUsers?.Log(EnumLogType.Notification, $"[ServerEssentials] {message}");
}