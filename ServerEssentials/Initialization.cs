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
        Debug.Log($"Running on Version: {Mod.Info.Version}");

        homeCommands = new(api);
        tpaCommands = new(api);
        backCommands = new(api);
    }

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        Debug.LoadLogger(api.Logger);
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        Configuration.UpdateBaseConfigurations(api);
        Configuration.UpdateTranslationsConfigurations(api);
        Debug.Log("Configurations Loaded");
    }
}

public class Debug
{
    static private ILogger logger;

    static public void LoadLogger(ILogger _logger) => logger = _logger;
    static public void Log(string message)
    {
        logger?.Log(EnumLogType.Notification, $"[LevelUP] {message}");
    }
    static public void LogDebug(string message)
    {
        if (Configuration.enableExtendedLog)
            logger?.Log(EnumLogType.Debug, $"[LevelUP] {message}");
    }
    static public void LogWarn(string message)
    {
        logger?.Log(EnumLogType.Warning, $"[LevelUP] {message}");
    }
    static public void LogError(string message)
    {
        logger?.Log(EnumLogType.Error, $"[LevelUP] {message}");
    }
}