using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Vintagestory.API.Common;

namespace ServerEssentials;

#pragma warning disable CA2211
public static class Configuration
{
    private static Dictionary<string, object> LoadConfigurationByDirectoryAndName(ICoreAPI api, string directory, string name, string defaultDirectory)
    {
        string directoryPath = Path.Combine(api.DataBasePath, directory);
        string configPath = Path.Combine(api.DataBasePath, directory, $"{name}.json");
        Dictionary<string, object> loadedConfig;
        try
        {
            // Load server configurations
            string jsonConfig = File.ReadAllText(configPath);
            loadedConfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonConfig);
        }
        catch (DirectoryNotFoundException)
        {
            Debug.Log($"WARNING: Server configurations directory does not exist creating {name}.json and directory...");
            try
            {
                Directory.CreateDirectory(directoryPath);
            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR: Cannot create directory: {ex.Message}");
            }
            Debug.Log("Loading default configurations...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();

            Debug.Log($"Configurations loaded, saving configs in: {configPath}");
            try
            {
                // Saving default configurations
                string defaultJson = JsonConvert.SerializeObject(loadedConfig, Formatting.Indented);
                File.WriteAllText(configPath, defaultJson);
            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR: Cannot save default files to {configPath}, reason: {ex.Message}");
            }
        }
        catch (FileNotFoundException)
        {
            Debug.Log($"WARNING: Server configurations {name}.json cannot be found, recreating file from default");
            Debug.Log("Loading default configurations...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();

            Debug.Log($"Configurations loaded, saving configs in: {configPath}");
            try
            {
                // Saving default configurations
                string defaultJson = JsonConvert.SerializeObject(loadedConfig, Formatting.Indented);
                File.WriteAllText(configPath, defaultJson);
            }
            catch (Exception ex)
            {
                Debug.Log($"ERROR: Cannot save default files to {configPath}, reason: {ex.Message}");
            }

        }
        catch (Exception ex)
        {
            Debug.Log($"ERROR: Cannot read the server configurations: {ex.Message}");
            Debug.Log("Loading default values from mod assets...");
            // Load default configurations
            loadedConfig = api.Assets.Get(new AssetLocation(defaultDirectory)).ToObject<Dictionary<string, object>>();
        }
        return loadedConfig;
    }


    #region baseconfigs

    #region homes
    public static bool enableSetHomeCommand = true;
    public static int maxHomes = 5;
    public static bool enableHomeCommand = true;
    public static int homeCommandDelay = 5;
    public static int homeCooldown = 120;
    public static bool homeCommandCanMove = false;
    public static bool homeCommandCanReceiveDamage = false;
    public static bool enableDelHomeCommand = true;
    public static bool enableListHomeCommand = true;
    #endregion
    #region tpa
    public static bool enableTpaCommand = true;
    public static int tpaCommandDelay = 5;
    public static int tpaCooldown = 120;
    public static int tpaTimeout = 10;
    public static bool tpaCommandCanMove = false;
    public static bool tpaCommandCanReceiveDamage = false;
    public static bool enableTpaAcceptCommand = true;
    public static bool enableTpaDenyCommand = true;
    #endregion
    public static bool enableExtendedLogs = true;

    public static void UpdateBaseConfigurations(ICoreAPI api)
    {
        Dictionary<string, object> baseConfigs = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/ServerEssentials/config",
            "base",
            "serveressentials:config/base.json"
        );
        { //enableSetHomeCommand
            if (baseConfigs.TryGetValue("enableSetHomeCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableSetHomeCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableSetHomeCommand is not boolean is {value.GetType()}");
                else enableSetHomeCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableSetHomeCommand not set");
        }
        { //maxHomes
            if (baseConfigs.TryGetValue("maxHomes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: maxHomes is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: maxHomes is not int is {value.GetType()}");
                else maxHomes = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: maxHomes not set");
        }
        { //enableHomeCommand
            if (baseConfigs.TryGetValue("enableHomeCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableHomeCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableHomeCommand is not boolean is {value.GetType()}");
                else enableHomeCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableHomeCommand not set");
        }
        { //homeCommandDelay
            if (baseConfigs.TryGetValue("homeCommandDelay", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: homeCommandDelay is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: homeCommandDelay is not int is {value.GetType()}");
                else homeCommandDelay = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: homeCommandDelay not set");
        }
        { //homeCooldown
            if (baseConfigs.TryGetValue("homeCooldown", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: homeCooldown is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: homeCooldown is not int is {value.GetType()}");
                else homeCooldown = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: homeCooldown not set");
        }
        { //homeCommandCanMove
            if (baseConfigs.TryGetValue("homeCommandCanMove", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: homeCommandCanMove is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: homeCommandCanMove is not boolean is {value.GetType()}");
                else homeCommandCanMove = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: homeCommandCanMove not set");
        }
        { //homeCommandCanReceiveDamage
            if (baseConfigs.TryGetValue("homeCommandCanReceiveDamage", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: homeCommandCanReceiveDamage is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: homeCommandCanReceiveDamage is not boolean is {value.GetType()}");
                else homeCommandCanReceiveDamage = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: homeCommandCanReceiveDamage not set");
        }
        { //enableDelHomeCommand
            if (baseConfigs.TryGetValue("enableDelHomeCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableDelHomeCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableDelHomeCommand is not boolean is {value.GetType()}");
                else enableDelHomeCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableDelHomeCommand not set");
        }
        { //enableListHomeCommand
            if (baseConfigs.TryGetValue("enableListHomeCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableListHomeCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableListHomeCommand is not boolean is {value.GetType()}");
                else enableListHomeCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableListHomeCommand not set");
        }
        { //enableTpaCommand
            if (baseConfigs.TryGetValue("enableTpaCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableTpaCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableTpaCommand is not boolean is {value.GetType()}");
                else enableTpaCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableTpaCommand not set");
        }
        { //tpaCommandDelay
            if (baseConfigs.TryGetValue("tpaCommandDelay", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaCommandDelay is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: tpaCommandDelay is not int is {value.GetType()}");
                else tpaCommandDelay = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: tpaCommandDelay not set");
        }
        { //tpaCooldown
            if (baseConfigs.TryGetValue("tpaCooldown", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaCooldown is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: tpaCooldown is not int is {value.GetType()}");
                else tpaCooldown = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: tpaCooldown not set");
        }
        { //tpaTimeout
            if (baseConfigs.TryGetValue("tpaTimeout", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaTimeout is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: tpaTimeout is not int is {value.GetType()}");
                else tpaTimeout = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: tpaTimeout not set");
        }
        { //tpaCommandCanMove
            if (baseConfigs.TryGetValue("tpaCommandCanMove", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaCommandCanMove is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: tpaCommandCanMove is not boolean is {value.GetType()}");
                else tpaCommandCanMove = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: tpaCommandCanMove not set");
        }
        { //tpaCommandCanReceiveDamage
            if (baseConfigs.TryGetValue("tpaCommandCanReceiveDamage", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaCommandCanReceiveDamage is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: tpaCommandCanReceiveDamage is not boolean is {value.GetType()}");
                else tpaCommandCanReceiveDamage = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: tpaCommandCanReceiveDamage not set");
        }
        { //enableTpaAcceptCommand
            if (baseConfigs.TryGetValue("enableTpaAcceptCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableTpaAcceptCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableTpaAcceptCommand is not boolean is {value.GetType()}");
                else enableTpaAcceptCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableTpaAcceptCommand not set");
        }
        { //enableTpaDenyCommand
            if (baseConfigs.TryGetValue("enableTpaDenyCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableTpaDenyCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableTpaDenyCommand is not boolean is {value.GetType()}");
                else enableTpaDenyCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableTpaDenyCommand not set");
        }
        { //enableExtendedLogs
            if (baseConfigs.TryGetValue("enableExtendedLogs", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableExtendedLogs is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableExtendedLogs is not boolean is {value.GetType()}");
                else enableExtendedLogs = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableExtendedLogs not set");
        }
    }
    #endregion
}