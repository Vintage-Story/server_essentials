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

public class HomeConfiguration
{
    public bool enableSetHomeCommand = true;
    public string setHomePrivilege = "chat";
    public List<string> setHomeSyntaxes = ["sethome"];
    public int maxHomes = 5;
    public bool enableHomeCommand = true;
    public string homePrivilege = "chat";
    public List<string> homeSyntaxes = ["home"];
    public int homeCommandDelay = 5;
    public int homeCooldown = 120;
    public string homeCostItemId = "";
    public int homeCostQuantity = 0;
    public bool homeCommandCanMove = false;
    public bool homeCommandCanReceiveDamage = false;
    public bool enableDelHomeCommand = true;
    public string delHomePrivilege = "chat";
    public List<string> delHomeSyntaxes = ["delhome"];
    public bool enableListHomeCommand = true;
    public string listHomePrivilege = "chat";
    public List<string> listHomeSyntaxes = ["listhome"];
    public bool ListHomeCommandShowCoords = true;
    public bool enableBuyHomeCommand = true;
    public string buyHomePrivilege = "chat";
    public List<string> buyHomeSyntaxes = ["buyhome"];
    public string buyHomeCostItemId = "game:gear-temporal";
    public int buyHomeCostQuantity = 1;
    public int buyHomeMaxSlots = 0;
    public int buyHomeCostIncrement = 0;
}

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

public class CustomCommandsConfiguration
{
    public List<CustomCommandEntry> customCommands = [];
    public bool enableExtendedLog = true;
}

public class TranslationsConfiguration
{
    #region back
    public string translationBackCancelledDueMoving = "Teleport canceled, because you moved";
    public string translationBackCancelledDueDamage = "Teleport canceled, because you received damage";
    public string translationBackHealthInvalid = "Cannot teleport, your health is invalid";
    public string translationBackTeleporting = "Teleporting to previously position...";
    public string translationBackTeleportingCost = "Teleporting to previously position... will cost {0} {1}";
    public string translationBackNoBackAvailable = "No previously position to go back!";
    public string translationBackCooldown = "Back command is still on cooldown: {0} seconds remaining...";
    public string translationBackDescription = "Returns to your previous position before teleporting using /back";
    public string translationBackAlreadySent = "Already Sent";
    #endregion
    #region home
    public string translationHomeCancelledDueMoving = "Teleport canceled, because you moved";
    public string translationHomeCancelledDueDamage = "Teleport canceled, because you received damage";
    public string translationHomeMaxHomesReached = "Max homes reached";
    public string translationHomeHomeSet = "Home Set!";
    public string translationHomeHomeSetCost = "Home Set! Cost {0} {1}";
    public string translationHomeHomeNotSet = "Home not set!";
    public string translationHomeHomeRemoved = "Home removed!";
    public string translationHomeHomeInvalid = "Invalid home!";
    public string translationHomeNoHomes = "You don't have any home set!";
    public string translationHomeHomesList = "Your homes:";
    public string translationHomeCooldown = "Home command is still on cooldown: {0} seconds remaining...";
    public string translationHomeTeleporting = "Teleporting to {0}...";
    public string translationHomeTeleportingCost = "Teleporting to {0}... will cost {1} {2}";
    public string translationHomeHealthInvalid = "Cannot teleport, your health is invalid";
    public string translationSetHomeDescription = "Set a home using /sethome homename";
    public string translationHomeDescription = "Teleport to a home using /home homename";
    public string translationDelHomeDescription = "Delete a home /delhome homename";
    public string translationListHomeDescription = "View the home lists";
    public string translationHomeAlreadySent = "Already Sent";
    public string translationBuyHomeDescription = "Buy an extra home slot using /buyhome";
    public string translationBuyHomePurchased = "Home slot purchased! Max homes: {0}";
    public string translationBuyHomePurchasedCost = "Home slot purchased! Cost {0} {1}. Max homes: {2}";
    public string translationBuyHomeMaxSlotsReached = "Maximum purchasable home slots reached!";
    #endregion
    #region tpa
    public string translationTpaCancelledDueMoving = "Teleport canceled, because you moved";
    public string translationTpaCancelledDueDamage = "Teleport canceled, because you received damage";
    public string translationTpaOutRequestNotification = "{0} send you a tpa request, /tpaaccept or /tpadeny";
    public string translationTpaAutoAcceptNotification = "{0} is being teleported to you";
    public string translationTpaRequestExpired = "{0} Tpa has expired";
    public string translationTpaRequestCancelled = "Teleport cancelled, by {0}";
    public string translationTpaRequestAccepted = "Request accepted don't move for {0} seconds";
    public string translationTpaRequestAcceptedCost = "Request accepted don't move for {0} seconds. Will cost {1} {2}";
    public string translationTpaCooldown = "Tpa command is still on cooldown: {0} seconds remaining...";
    public string translationTpaMissingPlayer = "Missing player name";
    public string translationTpaAlreadySent = "Already sent";
    public string translationTpaNotFound = "{0} not found";
    public string translationTpaSent = "Tpa request send to {0}";
    public string translationTpaRequestNotFound = "Request not found";
    public string translationTpaRequesterOnCooldown = "Tpa command is still on cooldown for {0}";
    public string translationTpaRequesterHealthInvalid = "Cannot teleport, {0} health is invalid";
    public string translationTpaAlreadyChanneling = "The request already exists for {0}";
    public string translationTpaAccepted = "Request accepted: {0}";
    public string translationTpaNoRequests = "No requests";
    public string translationTpaRequestDenied = "Request denied: {0}";
    public string translationTpaNoRequestToCancel = "No teleport to cancel";
    public string translationTpaCancelled = "{0} teleport cancelled";
    public string translationTpaDescription = "Teleport to a player using /tpa playername";
    public string translationTpaAcceptDescription = "A requested player will teleport to you using /tpaaccept playername";
    public string translationTpaDenyDescription = "Deny a teleport request /tpadeny playername";
    public string translationTpaCancelDescription = "Cancel a channeling teleport request /tpacancel playername";
    #endregion
    public string translationNotEnoughItems = "You don't have the required items: {1}x {0}";
}

#pragma warning disable CA2211
public static class Configuration
{
    public static HomeConfiguration Home = new();
    public static TPAConfiguration TPA = new();
    public static BackConfiguration Back = new();
    public static CustomCommandsConfiguration CustomCommands = new();
    public static TranslationsConfiguration Translations = new();

    internal static void Load(ICoreAPI api)
    {
        Home = ConfigManager.LoadModConfig<HomeConfiguration>(api, "ServerEssentials", "base", Initialization.Logger, "serveressentials:config/base.json");
        TPA = ConfigManager.LoadModConfig<TPAConfiguration>(api, "ServerEssentials", "base", Initialization.Logger, "serveressentials:config/base.json");
        Back = ConfigManager.LoadModConfig<BackConfiguration>(api, "ServerEssentials", "base", Initialization.Logger, "serveressentials:config/base.json");
        CustomCommands = ConfigManager.LoadModConfig<CustomCommandsConfiguration>(api, "ServerEssentials", "base", Initialization.Logger, "serveressentials:config/base.json");
        Translations = ConfigManager.LoadModConfig<TranslationsConfiguration>(api, "ServerEssentials", "translations", Initialization.Logger, "serveressentials:config/translations.json");
    }
}
