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
    public static bool tpaCommandResetCooldownOnCancellation = true;
    public static bool enableTpaAcceptCommand = true;
    public static bool enableTpaDenyCommand = true;
    public static bool enableTpaCancelCommand = true;
    #endregion
    #region back
    public static bool enableBackCommand = true;
    public static int backCooldown = 120;
    public static int backCommandDelay = 5;
    public static int backCommandDuration = 30;
    public static bool backCommandCanMove = false;
    public static bool backCommandCanReceiveDamage = false;
    public static bool enableBackForHome = true;
    public static bool enableBackForTpa = true;
    public static bool enableBackForDeath = true;
    public static bool enableBackResycle = false;
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
        { //tpaCommandResetCooldownOnCancellation
            if (baseConfigs.TryGetValue("tpaCommandResetCooldownOnCancellation", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaCommandResetCooldownOnCancellation is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: tpaCommandResetCooldownOnCancellation is not boolean is {value.GetType()}");
                else tpaCommandResetCooldownOnCancellation = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: tpaCommandResetCooldownOnCancellation not set");
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
        { //enableTpaCancelCommand
            if (baseConfigs.TryGetValue("enableTpaCancelCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableTpaCancelCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableTpaCancelCommand is not boolean is {value.GetType()}");
                else enableTpaCancelCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableTpaCancelCommand not set");
        }
        { //enableBackCommand
            if (baseConfigs.TryGetValue("enableBackCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableBackCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableBackCommand is not boolean is {value.GetType()}");
                else enableBackCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableBackCommand not set");
        }
        { //backCooldown
            if (baseConfigs.TryGetValue("backCooldown", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: backCooldown is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: backCooldown is not int is {value.GetType()}");
                else backCooldown = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: backCooldown not set");
        }
        { //backCommandDelay
            if (baseConfigs.TryGetValue("backCommandDelay", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: backCommandDelay is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: backCommandDelay is not int is {value.GetType()}");
                else backCommandDelay = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: backCommandDelay not set");
        }
        { //backCommandDuration
            if (baseConfigs.TryGetValue("backCommandDuration", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: backCommandDuration is null");
                else if (value is not long) Debug.Log($"CONFIGURATION ERROR: backCommandDuration is not int is {value.GetType()}");
                else backCommandDuration = (int)(long)value;
            else Debug.Log("CONFIGURATION ERROR: backCommandDuration not set");
        }
        { //backCommandCanMove
            if (baseConfigs.TryGetValue("backCommandCanMove", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: backCommandCanMove is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: backCommandCanMove is not boolean is {value.GetType()}");
                else backCommandCanMove = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: backCommandCanMove not set");
        }
        { //backCommandCanReceiveDamage
            if (baseConfigs.TryGetValue("backCommandCanReceiveDamage", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: backCommandCanReceiveDamage is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: backCommandCanReceiveDamage is not boolean is {value.GetType()}");
                else backCommandCanReceiveDamage = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: backCommandCanReceiveDamage not set");
        }
        { //enableBackForHome
            if (baseConfigs.TryGetValue("enableBackForHome", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableBackForHome is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableBackForHome is not boolean is {value.GetType()}");
                else enableBackForHome = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableBackForHome not set");
        }
        { //enableBackForTpa
            if (baseConfigs.TryGetValue("enableBackForTpa", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableBackForTpa is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableBackForTpa is not boolean is {value.GetType()}");
                else enableBackForTpa = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableBackForTpa not set");
        }
        { //enableBackForDeath
            if (baseConfigs.TryGetValue("enableBackForDeath", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableBackForDeath is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableBackForDeath is not boolean is {value.GetType()}");
                else enableBackForDeath = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableBackForDeath not set");
        }
        { //enableBackResycle
            if (baseConfigs.TryGetValue("enableBackResycle", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableBackResycle is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableBackResycle is not boolean is {value.GetType()}");
                else enableBackResycle = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableBackResycle not set");
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

    #region translations
    #region back
    public static string translationBackCancelledDueMoving = "Teleport canceled, because you moved";
    public static string translationBackCancelledDueDamage = "Teleport canceled, because you received damage";
    public static string translationBackHealthInvalid = "Cannot teleport, your health is invalid";
    public static string translationBackTeleporting = "Teleporting to previously position...";
    public static string translationBackNoBackAvailable = "No previously position to go back!";
    #endregion
    #region home
    public static string translationHomeCancelledDueMoving = "Teleport canceled, because you moved";
    public static string translationHomeCancelledDueDamage = "Teleport canceled, because you received damage";
    public static string translationHomeMaxHomesReached = "Max homes reached";
    public static string translationHomeHomeSet = "Home Set!";
    public static string translationHomeHomeNotSet = "Home not set!";
    public static string translationHomeHomeRemoved = "Home removed!";
    public static string translationHomeHomeInvalid = "Invalid home!";
    public static string translationHomeNoHomes = "You don't have any home set!";
    public static string translationHomeHomesList = "Your homes:";
    public static string translationHomeCooldown = "Home command is still on cooldown: {0} seconds remaining...";
    public static string translationHomeTeleporting = "Teleporting to {0}...";
    public static string translationHomeHealthInvalid = "Cannot teleport, your health is invalid";
    #endregion
    #region tpa
    public static string translationTpaCancelledDueMoving = "Teleport canceled, because you moved";
    public static string translationTpaCancelledDueDamage = "Teleport canceled, because you received damage";
    public static string translationTpaOutRequestNotification = "{0} send you a tpa request, /tpaaccept or /tpadeny";
    public static string translationTpaRequestExpired = "{0} Tpa has expired";
    public static string translationTpaRequestCancelled = "Teleport cancelled, by {0}";
    public static string translationTpaRequestAccepted = "Request accepted don't move for {0} seconds";
    public static string translationTpaCooldown = "Tpa command is still on cooldown: {0} seconds remaining...";
    public static string translationTpaMissingPlayer = "Missing player name";
    public static string translationTpaNotFound = "{0} not found";
    public static string translationTpaSent = "Tpa request send to {0}";
    public static string translationTpaReqiestNotFound = "Request not found";
    public static string translationTpaRequesterOnCooldown = "Tpa command is still on cooldown for {}";
    public static string translationTpaRequesterHealthInvalid = "Cannot teleport, {0} health is invalid";
    public static string translationTpaAlreadyChanneling = "The request already exists for {0}";
    public static string translationTpaAccepted = "Request accepted: {0}";
    public static string translationTpaNoRequests = "No requests";
    public static string translationTpaRequestNotFound = "Request cannot be found";
    public static string translationTpaRequestDenied = "Request denied: {0}";
    public static string translationTpaNoRequestToCancel = "No teleport to cancel";
    public static string translationTpaCancelled = "{0} teleport cancelled";
    #endregion
    public static void UpdateTranslationsConfigurations(ICoreAPI api)
    {
        Dictionary<string, object> baseConfigs = LoadConfigurationByDirectoryAndName(
            api,
            "ModConfig/ServerEssentials/config",
            "translations",
            "serveressentials:config/translations.json"
        );
        { //translationBackCancelledDueMoving
            if (baseConfigs.TryGetValue("translationBackCancelledDueMoving", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: translationBackCancelledDueMoving is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: translationBackCancelledDueMoving is not string is {value.GetType()}");
                else translationBackCancelledDueMoving = (string)value;
            else Debug.Log("CONFIGURATION ERROR: translationBackCancelledDueMoving not set");
        }
        { //translationBackCancelledDueDamage
            if (baseConfigs.TryGetValue("translationBackCancelledDueDamage", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: translationBackCancelledDueDamage is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: translationBackCancelledDueDamage is not string is {value.GetType()}");
                else translationBackCancelledDueDamage = (string)value;
            else Debug.Log("CONFIGURATION ERROR: translationBackCancelledDueDamage not set");
        }
        { //translationHomeCancelledDueMoving
            if (baseConfigs.TryGetValue("translationHomeCancelledDueMoving", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: translationHomeCancelledDueMoving is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: translationHomeCancelledDueMoving is not string is {value.GetType()}");
                else translationHomeCancelledDueMoving = (string)value;
            else Debug.Log("CONFIGURATION ERROR: translationHomeCancelledDueMoving not set");
        }
        { //translationHomeCancelledDueDamage
            if (baseConfigs.TryGetValue("translationHomeCancelledDueDamage", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: translationHomeCancelledDueDamage is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: translationHomeCancelledDueDamage is not string is {value.GetType()}");
                else translationHomeCancelledDueDamage = (string)value;
            else Debug.Log("CONFIGURATION ERROR: translationHomeCancelledDueDamage not set");
        }
    }
    #endregion
}