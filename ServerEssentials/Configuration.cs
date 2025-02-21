using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
    public static string setHomePrivilege = "chat";
    public static List<string> setHomeSyntaxes = ["sethome"];
    public static int maxHomes = 5;
    public static bool enableHomeCommand = true;
    public static string homePrivilege = "chat";
    public static List<string> homeSyntaxes = ["home"];
    public static int homeCommandDelay = 5;
    public static int homeCooldown = 120;
    public static bool homeCommandCanMove = false;
    public static bool homeCommandCanReceiveDamage = false;
    public static bool enableDelHomeCommand = true;
    public static string delHomePrivilege = "chat";
    public static List<string> delHomeSyntaxes = ["delhome"];
    public static bool enableListHomeCommand = true;
    public static string listHomePrivilege = "chat";
    public static List<string> listHomeSyntaxes = ["listhome"];
    public static bool ListHomeCommandShowCoords = true;
    #endregion
    #region tpa
    public static bool enableTpaCommand = true;
    public static string tpaPrivilege = "chat";
    public static List<string> tpaSyntaxes = ["tpa"];
    public static int tpaCommandDelay = 5;
    public static int tpaCooldown = 120;
    public static int tpaTimeout = 10;
    public static bool tpaCommandCanMove = false;
    public static bool tpaCommandCanReceiveDamage = false;
    public static bool tpaCommandResetCooldownOnCancellation = true;
    public static bool enableTpaAcceptCommand = true;
    public static string tpaAcceptPrivilege = "chat";
    public static List<string> tpaAcceptSyntaxes = ["tpaaccept", "tpaccept", "tpaa"];
    public static bool enableTpaDenyCommand = true;
    public static string tpaDenyPrivilege = "chat";
    public static List<string> tpaDenySyntaxes = ["tpadeny", "tpad"];
    public static bool enableTpaCancelCommand = true;
    public static string tpaCancelPrivilege = "chat";
    public static List<string> tpaCancelSyntaxes = ["tpacancel", "tpac"];
    #endregion
    #region back
    public static bool enableBackCommand = true;
    public static string backPrivilege = "chat";
    public static List<string> backSyntaxes = ["back"];
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
        { //setHomePrivilege
            if (baseConfigs.TryGetValue("setHomePrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: setHomePrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: setHomePrivilege is not string is {value.GetType()}");
                else setHomePrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: setHomePrivilege not set");
        }
        { //setHomeSyntaxes
            if (baseConfigs.TryGetValue("setHomeSyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: setHomeSyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: setHomeSyntaxes is not array is {value.GetType()}");
                else setHomeSyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: setHomeSyntaxes not set");
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
        { //homePrivilege
            if (baseConfigs.TryGetValue("homePrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: homePrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: homePrivilege is not string is {value.GetType()}");
                else homePrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: homePrivilege not set");
        }
        { //homeSyntaxes
            if (baseConfigs.TryGetValue("homeSyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: homeSyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: homeSyntaxes is not array is {value.GetType()}");
                else homeSyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: homeSyntaxes not set");
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
        { //delHomePrivilege
            if (baseConfigs.TryGetValue("delHomePrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: delHomePrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: delHomePrivilege is not string is {value.GetType()}");
                else delHomePrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: delHomePrivilege not set");
        }
        { //delHomeSyntaxes
            if (baseConfigs.TryGetValue("delHomeSyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: delHomeSyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: delHomeSyntaxes is not array is {value.GetType()}");
                else delHomeSyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: delHomeSyntaxes not set");
        }
        { //enableListHomeCommand
            if (baseConfigs.TryGetValue("enableListHomeCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableListHomeCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableListHomeCommand is not boolean is {value.GetType()}");
                else enableListHomeCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableListHomeCommand not set");
        }
        { //listHomePrivilege
            if (baseConfigs.TryGetValue("listHomePrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: listHomePrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: listHomePrivilege is not string is {value.GetType()}");
                else listHomePrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: listHomePrivilege not set");
        }
        { //listHomeSyntaxes
            if (baseConfigs.TryGetValue("listHomeSyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: listHomeSyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: listHomeSyntaxes is not array is {value.GetType()}");
                else listHomeSyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: listHomeSyntaxes not set");
        }
        { //ListHomeCommandShowCoords
            if (baseConfigs.TryGetValue("ListHomeCommandShowCoords", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: ListHomeCommandShowCoords is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: ListHomeCommandShowCoords is not boolean is {value.GetType()}");
                else ListHomeCommandShowCoords = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: ListHomeCommandShowCoords not set");
        }
        { //enableTpaCommand
            if (baseConfigs.TryGetValue("enableTpaCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableTpaCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableTpaCommand is not boolean is {value.GetType()}");
                else enableTpaCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableTpaCommand not set");
        }
        { //tpaPrivilege
            if (baseConfigs.TryGetValue("tpaPrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaPrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: tpaPrivilege is not string is {value.GetType()}");
                else tpaPrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: tpaPrivilege not set");
        }
        { //tpaSyntaxes
            if (baseConfigs.TryGetValue("tpaSyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaSyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: tpaSyntaxes is not array is {value.GetType()}");
                else tpaSyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: tpaSyntaxes not set");
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
        { //tpaAcceptPrivilege
            if (baseConfigs.TryGetValue("tpaAcceptPrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaAcceptPrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: tpaAcceptPrivilege is not string is {value.GetType()}");
                else tpaAcceptPrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: tpaAcceptPrivilege not set");
        }
        { //tpaAcceptSyntaxes
            if (baseConfigs.TryGetValue("tpaAcceptSyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaAcceptSyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: tpaAcceptSyntaxes is not array is {value.GetType()}");
                else tpaAcceptSyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: tpaAcceptSyntaxes not set");
        }
        { //enableTpaDenyCommand
            if (baseConfigs.TryGetValue("enableTpaDenyCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableTpaDenyCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableTpaDenyCommand is not boolean is {value.GetType()}");
                else enableTpaDenyCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableTpaDenyCommand not set");
        }
        { //tpaDenyPrivilege
            if (baseConfigs.TryGetValue("tpaDenyPrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaDenyPrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: tpaDenyPrivilege is not string is {value.GetType()}");
                else tpaDenyPrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: tpaDenyPrivilege not set");
        }
        { //tpaDenySyntaxes
            if (baseConfigs.TryGetValue("tpaDenySyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaDenySyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: tpaDenySyntaxes is not array is {value.GetType()}");
                else tpaDenySyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: tpaDenySyntaxes not set");
        }
        { //enableTpaCancelCommand
            if (baseConfigs.TryGetValue("enableTpaCancelCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableTpaCancelCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableTpaCancelCommand is not boolean is {value.GetType()}");
                else enableTpaCancelCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableTpaCancelCommand not set");
        }
        { //tpaCancelPrivilege
            if (baseConfigs.TryGetValue("tpaCancelPrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaCancelPrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: tpaCancelPrivilege is not string is {value.GetType()}");
                else tpaCancelPrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: tpaCancelPrivilege not set");
        }
        { //tpaCancelSyntaxes
            if (baseConfigs.TryGetValue("tpaCancelSyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: tpaCancelSyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: tpaCancelSyntaxes is not array is {value.GetType()}");
                else tpaCancelSyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: tpaCancelSyntaxes not set");
        }
        { //enableBackCommand
            if (baseConfigs.TryGetValue("enableBackCommand", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: enableBackCommand is null");
                else if (value is not bool) Debug.Log($"CONFIGURATION ERROR: enableBackCommand is not boolean is {value.GetType()}");
                else enableBackCommand = (bool)value;
            else Debug.Log("CONFIGURATION ERROR: enableBackCommand not set");
        }
        { //backPrivilege
            if (baseConfigs.TryGetValue("backPrivilege", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: backPrivilege is null");
                else if (value is not string) Debug.Log($"CONFIGURATION ERROR: backPrivilege is not string is {value.GetType()}");
                else backPrivilege = (string)value;
            else Debug.Log("CONFIGURATION ERROR: backPrivilege not set");
        }
        { //backSyntaxes
            if (baseConfigs.TryGetValue("backSyntaxes", out object value))
                if (value is null) Debug.Log("CONFIGURATION ERROR: backSyntaxes is null");
                else if (value is not JArray) Debug.Log($"CONFIGURATION ERROR: backSyntaxes is not array is {value.GetType()}");
                else backSyntaxes = (value as JArray).ToObject<List<string>>();
            else Debug.Log("CONFIGURATION ERROR: backSyntaxes not set");
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
    public static string translationBackCooldown = "Back command is still on cooldown: {0} seconds remaining...";
    public static string translationBackDescription = "Returns to your previous position before teleporting using /back";
    public static string translationBackAlreadySent = "Already Sent";
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
    public static string translationSetHomeDescription = "Set a home using /sethome homename";
    public static string translationHomeDescription = "Teleport to a home using /home homename";
    public static string translationDelHomeDescription = "Delete a home /delhome homename";
    public static string translationListHomeDescription = "View the home lists";
    public static string translationHomeAlreadySent = "Already Sent";
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
    public static string translationTpaAlreadySent = "Already sent";
    public static string translationTpaNotFound = "{0} not found";
    public static string translationTpaSent = "Tpa request send to {0}";
    public static string translationTpaRequestNotFound = "Request not found";
    public static string translationTpaRequesterOnCooldown = "Tpa command is still on cooldown for {0}";
    public static string translationTpaRequesterHealthInvalid = "Cannot teleport, {0} health is invalid";
    public static string translationTpaAlreadyChanneling = "The request already exists for {0}";
    public static string translationTpaAccepted = "Request accepted: {0}";
    public static string translationTpaNoRequests = "No requests";
    public static string translationTpaRequestDenied = "Request denied: {0}";
    public static string translationTpaNoRequestToCancel = "No teleport to cancel";
    public static string translationTpaCancelled = "{0} teleport cancelled";
    public static string translationTpaDescription = "Teleport to a player using /tpa playername";
    public static string translationTpaAcceptDescription = "A requested player will teleport to you using /tpaaccept playername";
    public static string translationTpaDenyDescription = "Deny a teleport request /tpadeny playername";
    public static string translationTpaCancelDescription = "Cancel a channeling teleport request /tpacancel playername";
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
                if (value is null) Debug.Log("TRANSLATION ERROR: translationBackCancelledDueMoving is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationBackCancelledDueMoving is not string is {value.GetType()}");
                else translationBackCancelledDueMoving = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationBackCancelledDueMoving not set");
        }
        { //translationBackCancelledDueDamage
            if (baseConfigs.TryGetValue("translationBackCancelledDueDamage", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationBackCancelledDueDamage is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationBackCancelledDueDamage is not string is {value.GetType()}");
                else translationBackCancelledDueDamage = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationBackCancelledDueDamage not set");
        }
        { //translationBackHealthInvalid
            if (baseConfigs.TryGetValue("translationBackHealthInvalid", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationBackHealthInvalid is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationBackHealthInvalid is not string is {value.GetType()}");
                else translationBackHealthInvalid = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationBackHealthInvalid not set");
        }
        { //translationBackTeleporting
            if (baseConfigs.TryGetValue("translationBackTeleporting", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationBackTeleporting is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationBackTeleporting is not string is {value.GetType()}");
                else translationBackTeleporting = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationBackTeleporting not set");
        }
        { //translationBackNoBackAvailable
            if (baseConfigs.TryGetValue("translationBackNoBackAvailable", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationBackNoBackAvailable is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationBackNoBackAvailable is not string is {value.GetType()}");
                else translationBackNoBackAvailable = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationBackNoBackAvailable not set");
        }
        { //translationBackCooldown
            if (baseConfigs.TryGetValue("translationBackCooldown", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationBackCooldown is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationBackCooldown is not string is {value.GetType()}");
                else translationBackCooldown = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationBackCooldown not set");
        }
        { //translationBackDescription
            if (baseConfigs.TryGetValue("translationBackDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationBackDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationBackDescription is not string is {value.GetType()}");
                else translationBackDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationBackDescription not set");
        }
        { //translationBackAlreadySent
            if (baseConfigs.TryGetValue("translationBackAlreadySent", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationBackAlreadySent is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationBackAlreadySent is not string is {value.GetType()}");
                else translationBackAlreadySent = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationBackAlreadySent not set");
        }
        { //translationHomeCancelledDueMoving
            if (baseConfigs.TryGetValue("translationHomeCancelledDueMoving", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeCancelledDueMoving is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeCancelledDueMoving is not string is {value.GetType()}");
                else translationHomeCancelledDueMoving = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeCancelledDueMoving not set");
        }
        { //translationHomeCancelledDueDamage
            if (baseConfigs.TryGetValue("translationHomeCancelledDueDamage", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeCancelledDueDamage is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeCancelledDueDamage is not string is {value.GetType()}");
                else translationHomeCancelledDueDamage = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeCancelledDueDamage not set");
        }
        { //translationHomeMaxHomesReached
            if (baseConfigs.TryGetValue("translationHomeMaxHomesReached", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeMaxHomesReached is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeMaxHomesReached is not string is {value.GetType()}");
                else translationHomeMaxHomesReached = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeMaxHomesReached not set");
        }
        { //translationHomeHomeSet
            if (baseConfigs.TryGetValue("translationHomeHomeSet", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeHomeSet is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeHomeSet is not string is {value.GetType()}");
                else translationHomeHomeSet = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeHomeSet not set");
        }
        { //translationHomeHomeNotSet
            if (baseConfigs.TryGetValue("translationHomeHomeNotSet", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeHomeNotSet is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeHomeNotSet is not string is {value.GetType()}");
                else translationHomeHomeNotSet = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeHomeNotSet not set");
        }
        { //translationHomeHomeRemoved
            if (baseConfigs.TryGetValue("translationHomeHomeRemoved", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeHomeRemoved is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeHomeRemoved is not string is {value.GetType()}");
                else translationHomeHomeRemoved = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeHomeRemoved not set");
        }
        { //translationHomeHomeInvalid
            if (baseConfigs.TryGetValue("translationHomeHomeInvalid", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeHomeInvalid is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeHomeInvalid is not string is {value.GetType()}");
                else translationHomeHomeInvalid = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeHomeInvalid not set");
        }
        { //translationHomeNoHomes
            if (baseConfigs.TryGetValue("translationHomeNoHomes", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeNoHomes is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeNoHomes is not string is {value.GetType()}");
                else translationHomeNoHomes = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeNoHomes not set");
        }
        { //translationHomeHomesList
            if (baseConfigs.TryGetValue("translationHomeHomesList", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeHomesList is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeHomesList is not string is {value.GetType()}");
                else translationHomeHomesList = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeHomesList not set");
        }
        { //translationHomeCooldown
            if (baseConfigs.TryGetValue("translationHomeCooldown", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeCooldown is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeCooldown is not string is {value.GetType()}");
                else translationHomeCooldown = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeCooldown not set");
        }
        { //translationHomeTeleporting
            if (baseConfigs.TryGetValue("translationHomeTeleporting", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeTeleporting is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeTeleporting is not string is {value.GetType()}");
                else translationHomeTeleporting = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeTeleporting not set");
        }
        { //translationHomeHealthInvalid
            if (baseConfigs.TryGetValue("translationHomeHealthInvalid", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeHealthInvalid is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeHealthInvalid is not string is {value.GetType()}");
                else translationHomeHealthInvalid = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeHealthInvalid not set");
        }
        { //translationSetHomeDescription
            if (baseConfigs.TryGetValue("translationSetHomeDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationSetHomeDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationSetHomeDescription is not string is {value.GetType()}");
                else translationSetHomeDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationSetHomeDescription not set");
        }
        { //translationHomeDescription
            if (baseConfigs.TryGetValue("translationHomeDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeDescription is not string is {value.GetType()}");
                else translationHomeDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeDescription not set");
        }
        { //translationDelHomeDescription
            if (baseConfigs.TryGetValue("translationDelHomeDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationDelHomeDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationDelHomeDescription is not string is {value.GetType()}");
                else translationDelHomeDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationDelHomeDescription not set");
        }
        { //translationListHomeDescription
            if (baseConfigs.TryGetValue("translationListHomeDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationListHomeDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationListHomeDescription is not string is {value.GetType()}");
                else translationListHomeDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationListHomeDescription not set");
        }
        { //translationHomeAlreadySent
            if (baseConfigs.TryGetValue("translationHomeAlreadySent", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationHomeAlreadySent is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationHomeAlreadySent is not string is {value.GetType()}");
                else translationHomeAlreadySent = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationHomeAlreadySent not set");
        }
        { //translationTpaCancelledDueMoving
            if (baseConfigs.TryGetValue("translationTpaCancelledDueMoving", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaCancelledDueMoving is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaCancelledDueMoving is not string is {value.GetType()}");
                else translationTpaCancelledDueMoving = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaCancelledDueMoving not set");
        }
        { //translationTpaCancelledDueDamage
            if (baseConfigs.TryGetValue("translationTpaCancelledDueDamage", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaCancelledDueDamage is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaCancelledDueDamage is not string is {value.GetType()}");
                else translationTpaCancelledDueDamage = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaCancelledDueDamage not set");
        }
        { //translationTpaOutRequestNotification
            if (baseConfigs.TryGetValue("translationTpaOutRequestNotification", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaOutRequestNotification is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaOutRequestNotification is not string is {value.GetType()}");
                else translationTpaOutRequestNotification = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaOutRequestNotification not set");
        }
        { //translationTpaRequestExpired
            if (baseConfigs.TryGetValue("translationTpaRequestExpired", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaRequestExpired is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaRequestExpired is not string is {value.GetType()}");
                else translationTpaRequestExpired = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaRequestExpired not set");
        }
        { //translationTpaRequestCancelled
            if (baseConfigs.TryGetValue("translationTpaRequestCancelled", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaRequestCancelled is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaRequestCancelled is not string is {value.GetType()}");
                else translationTpaRequestCancelled = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaRequestCancelled not set");
        }
        { //translationTpaRequestAccepted
            if (baseConfigs.TryGetValue("translationTpaRequestAccepted", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaRequestAccepted is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaRequestAccepted is not string is {value.GetType()}");
                else translationTpaRequestAccepted = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaRequestAccepted not set");
        }
        { //translationTpaCooldown
            if (baseConfigs.TryGetValue("translationTpaCooldown", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaCooldown is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaCooldown is not string is {value.GetType()}");
                else translationTpaCooldown = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaCooldown not set");
        }
        { //translationTpaMissingPlayer
            if (baseConfigs.TryGetValue("translationTpaMissingPlayer", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaMissingPlayer is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaMissingPlayer is not string is {value.GetType()}");
                else translationTpaMissingPlayer = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaMissingPlayer not set");
        }
        { //translationTpaAlreadySent
            if (baseConfigs.TryGetValue("translationTpaAlreadySent", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaAlreadySent is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaAlreadySent is not string is {value.GetType()}");
                else translationTpaAlreadySent = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaAlreadySent not set");
        }
        { //translationTpaNotFound
            if (baseConfigs.TryGetValue("translationTpaNotFound", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaNotFound is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaNotFound is not string is {value.GetType()}");
                else translationTpaNotFound = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaNotFound not set");
        }
        { //translationTpaSent
            if (baseConfigs.TryGetValue("translationTpaSent", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaSent is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaSent is not string is {value.GetType()}");
                else translationTpaSent = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaSent not set");
        }
        { //translationTpaRequestNotFound
            if (baseConfigs.TryGetValue("translationTpaRequestNotFound", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaRequestNotFound is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaRequestNotFound is not string is {value.GetType()}");
                else translationTpaRequestNotFound = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaRequestNotFound not set");
        }
        { //translationTpaRequesterOnCooldown
            if (baseConfigs.TryGetValue("translationTpaRequesterOnCooldown", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaRequesterOnCooldown is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaRequesterOnCooldown is not string is {value.GetType()}");
                else translationTpaRequesterOnCooldown = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaRequesterOnCooldown not set");
        }
        { //translationTpaRequesterHealthInvalid
            if (baseConfigs.TryGetValue("translationTpaRequesterHealthInvalid", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaRequesterHealthInvalid is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaRequesterHealthInvalid is not string is {value.GetType()}");
                else translationTpaRequesterHealthInvalid = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaRequesterHealthInvalid not set");
        }
        { //translationTpaAlreadyChanneling
            if (baseConfigs.TryGetValue("translationTpaAlreadyChanneling", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaAlreadyChanneling is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaAlreadyChanneling is not string is {value.GetType()}");
                else translationTpaAlreadyChanneling = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaAlreadyChanneling not set");
        }
        { //translationTpaAccepted
            if (baseConfigs.TryGetValue("translationTpaAccepted", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaAccepted is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaAccepted is not string is {value.GetType()}");
                else translationTpaAccepted = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaAccepted not set");
        }
        { //translationTpaNoRequests
            if (baseConfigs.TryGetValue("translationTpaNoRequests", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaNoRequests is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaNoRequests is not string is {value.GetType()}");
                else translationTpaNoRequests = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaNoRequests not set");
        }
        { //translationTpaRequestNotFound
            if (baseConfigs.TryGetValue("translationTpaRequestNotFound", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaRequestNotFound is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaRequestNotFound is not string is {value.GetType()}");
                else translationTpaRequestNotFound = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaRequestNotFound not set");
        }
        { //translationTpaRequestDenied
            if (baseConfigs.TryGetValue("translationTpaRequestDenied", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaRequestDenied is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaRequestDenied is not string is {value.GetType()}");
                else translationTpaRequestDenied = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaRequestDenied not set");
        }
        { //translationTpaNoRequestToCancel
            if (baseConfigs.TryGetValue("translationTpaNoRequestToCancel", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaNoRequestToCancel is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaNoRequestToCancel is not string is {value.GetType()}");
                else translationTpaNoRequestToCancel = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaNoRequestToCancel not set");
        }
        { //translationTpaCancelled
            if (baseConfigs.TryGetValue("translationTpaCancelled", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaCancelled is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaCancelled is not string is {value.GetType()}");
                else translationTpaCancelled = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaCancelled not set");
        }
        { //translationTpaDescription
            if (baseConfigs.TryGetValue("translationTpaDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaDescription is not string is {value.GetType()}");
                else translationTpaDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaDescription not set");
        }
        { //translationTpaAcceptDescription
            if (baseConfigs.TryGetValue("translationTpaAcceptDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaAcceptDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaAcceptDescription is not string is {value.GetType()}");
                else translationTpaAcceptDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaAcceptDescription not set");
        }
        { //translationTpaDenyDescription
            if (baseConfigs.TryGetValue("translationTpaDenyDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaDenyDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaDenyDescription is not string is {value.GetType()}");
                else translationTpaDenyDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaDenyDescription not set");
        }
        { //translationTpaCancelDescription
            if (baseConfigs.TryGetValue("translationTpaCancelDescription", out object value))
                if (value is null) Debug.Log("TRANSLATION ERROR: translationTpaCancelDescription is null");
                else if (value is not string) Debug.Log($"TRANSLATION ERROR: translationTpaCancelDescription is not string is {value.GetType()}");
                else translationTpaCancelDescription = (string)value;
            else Debug.Log("TRANSLATION ERROR: translationTpaCancelDescription not set");
        }
    }
    #endregion
}