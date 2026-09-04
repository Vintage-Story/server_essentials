using OpenConfiguration;
using Vintagestory.API.Common;

namespace ServerEssentials;

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

public static partial class Configuration
{
    public static TranslationsConfiguration Translations = new();

    private static void LoadTranslations(ICoreAPI api)
        => Translations = ConfigManager.LoadModConfig<TranslationsConfiguration>(api, "ServerEssentials", "translations", ServerEssentialsModSystem.Logger, "serveressentials:config/translations.json");
}
