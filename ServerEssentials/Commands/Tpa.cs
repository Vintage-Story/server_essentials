using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ServerEssentials.Commands;

public class TPA
{
    private readonly ICoreServerAPI serverAPI;

    /// <summary>
    /// { PlayerUID: secondsRemaining }
    /// </summary>
    private readonly Dictionary<string, int> tpaCooldowns = [];
    /// <summary>
    /// The key is the player who received the request, and the value is the players list who sended the requests
    /// { PlayerUID: [PlayerUID,PlayerUID,PlayerUID] }
    /// </summary>
    private readonly Dictionary<string, List<string>> tpaRequests = [];
    /// <summary>
    /// The key is the player who received the request, and the value is the players list who is channeling to teleport to the player (already accepted the request)
    /// { PlayerUID: [PlayerUID,PlayerUID,PlayerUID] }
    /// </summary>
    private readonly Dictionary<string, List<string>> tpaDelays = [];

    public TPA(ICoreServerAPI api)
    {
        serverAPI = api;

        if (Configuration.enableTpaCommand)
        {
            foreach (string syntax in Configuration.tpaSyntaxes)
            {
                // Create tpa command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationTpaDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.tpaPrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("playername", false))
                // Function Handle
                .HandleWith(TpaCommand);
                Debug.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.enableTpaAcceptCommand)
        {
            foreach (string syntax in Configuration.tpaAcceptSyntaxes)
            {
                // Create tpaaccept command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationTpaAcceptDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.tpaAcceptPrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("playername", false))
                // Function Handle
                .HandleWith(TpaAcceptCommand);
                Debug.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.enableTpaDenyCommand)
        {
            foreach (string syntax in Configuration.tpaDenySyntaxes)
            {
                // Create tpadeny command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationTpaDenyDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.tpaDenyPrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("playername", false))
                // Function Handle
                .HandleWith(TpaDenyCommand);
                Debug.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.enableTpaCancelCommand)
        {
            foreach (string syntax in Configuration.tpaCancelSyntaxes)
            {
                // Create tpacancel command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationTpaCancelDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.tpaCancelPrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("playername", false))
                // Function Handle
                .HandleWith(TpaCancelCommand);
                Debug.Log($"Command created: /{syntax}");
            }
        }
    }

    private TextCommandResult TpaCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (tpaCooldowns.TryGetValue(player.PlayerUID, out int secondsRemaing))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaCooldown, secondsRemaing).ToString(), "7");

        if (!Utils.CheckPlayerInventoryForCommandCost(player, Configuration.tpaCostItemId, Configuration.tpaCostQuantity))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationNotEnoughItems, Utils.GetItemName(Configuration.tpaCostItemId), Configuration.tpaCostQuantity).ToString(), "7");

        if (args.Parsers[0].IsMissing)
            return TextCommandResult.Success(Configuration.translationTpaMissingPlayer, "8");

        IPlayer playerToTeleport = null;
        foreach (IPlayer teleportPlayer in serverAPI.World.AllOnlinePlayers)
        {
            if (teleportPlayer.PlayerName.ToLower() == (args[0] as string).ToLower())
            {
                playerToTeleport = teleportPlayer;
                break;
            }
        }
        if (playerToTeleport is null)
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaNotFound, args[0] as string).ToString(), "9");

        if (tpaRequests.TryGetValue(playerToTeleport.PlayerUID, out List<string> requests))
        {
            if (!requests.Contains(player.PlayerUID))
                tpaRequests[playerToTeleport.PlayerUID].Add(player.PlayerUID);
            else return TextCommandResult.Success(Configuration.translationTpaAlreadySent, "9");
        }
        else
            tpaRequests[playerToTeleport.PlayerUID] = [player.PlayerUID];

        if (Configuration.tpaAutoAccept)
        {
            (playerToTeleport as IServerPlayer).SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaAutoAcceptNotification, player.PlayerName).ToString(), EnumChatType.Notification);
            return ExecuteTpaAccept(playerToTeleport as IServerPlayer, player);
        }

        (playerToTeleport as IServerPlayer).SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaOutRequestNotification, player.PlayerName).ToString(), EnumChatType.Notification);

        long tickid = 0;
        int timeout = Configuration.tpaTimeout;

        void OnTpaTick(float obj)
        {
            timeout--;
            if (tpaRequests.TryGetValue(playerToTeleport.PlayerUID, out _))
            {
                // Accepted
                if (!tpaRequests[playerToTeleport.PlayerUID].Contains(player.PlayerUID))
                {
                    if (tpaRequests[playerToTeleport.PlayerUID].Count == 0)
                        tpaRequests.Remove(playerToTeleport.PlayerUID);

                    serverAPI.Event.UnregisterGameTickListener(tickid);
                    return;
                }

                // Expired
                if (timeout <= 0)
                {
                    tpaRequests[playerToTeleport.PlayerUID].Remove(player.PlayerUID);
                    if (tpaRequests[playerToTeleport.PlayerUID].Count == 0)
                        tpaRequests.Remove(playerToTeleport.PlayerUID);

                    serverAPI.Event.UnregisterGameTickListener(tickid);
                    player.SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaRequestExpired, playerToTeleport.PlayerName).ToString(), EnumChatType.Notification);
                    (playerToTeleport as IServerPlayer).SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaRequestExpired, player.PlayerName).ToString(), EnumChatType.Notification);
                }
            }
            // Expired
            else
                serverAPI.Event.UnregisterGameTickListener(tickid);

        }

        tickid = serverAPI.Event.RegisterGameTickListener(OnTpaTick, 1000, 1000);

        return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaSent, playerToTeleport.PlayerName).ToString(), "10");
    }

    private TextCommandResult TpaAcceptCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (tpaRequests.TryGetValue(player.PlayerUID, out List<string> playerRequests))
        {
            IPlayer playerTeleporting = null;
            foreach (string playerRequestUid in playerRequests)
            {
                foreach (IPlayer selectedPlayer in serverAPI.World.AllOnlinePlayers)
                {
                    if (playerRequestUid == selectedPlayer.PlayerUID)
                    {
                        playerTeleporting = selectedPlayer;
                        break;
                    }
                }
                if (playerTeleporting is not null)
                    if (!args.Parsers[0].IsMissing)
                        if (playerTeleporting.PlayerName == args[0] as string)
                            break;
            }

            if (playerTeleporting is null)
                return TextCommandResult.Success(Configuration.translationTpaRequestNotFound, "12");

            return ExecuteTpaAccept(player, playerTeleporting as IServerPlayer);
        }
        return TextCommandResult.Success(Configuration.translationTpaNoRequests, "11");
    }

    private TextCommandResult ExecuteTpaAccept(IServerPlayer receiver, IServerPlayer teleporting)
    {
        if (tpaCooldowns.TryGetValue(teleporting.PlayerUID, out _))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaRequesterOnCooldown, teleporting.PlayerName).ToString(), "7");

        long tickId = 0;
        long tickCooldownId = 0;
        uint ticksPassed = 0;
        EntityPos playerLastPosition;
        float playerLastHealth;

        void OnTpaCooldownTick(float obj)
        {
            if (tpaCooldowns.TryGetValue(teleporting.PlayerUID, out _))
            {
                tpaCooldowns[teleporting.PlayerUID] -= 1;
                if (tpaCooldowns[teleporting.PlayerUID] <= 0)
                {
                    tpaCooldowns.Remove(teleporting.PlayerUID);
                    serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
                }
            }
            else serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
        }
        void OnTpaAcceptTick(float obj)
        {
            void RemoveDelay()
            {
                if (tpaDelays.TryGetValue(receiver.PlayerUID, out _))
                {
                    tpaDelays[receiver.PlayerUID].Remove(teleporting.PlayerUID);
                    if (tpaDelays[receiver.PlayerUID].Count == 0)
                        tpaDelays.Remove(receiver.PlayerUID);
                }
            }
            void ResetCooldown()
            {
                if (Configuration.tpaCommandResetCooldownOnCancellation)
                    tpaCooldowns.Remove(teleporting.PlayerUID);
            }

            if (tpaDelays.TryGetValue(receiver.PlayerUID, out List<string> requests))
            {
                bool stillInDelay = false;
                foreach (string request in requests)
                {
                    if (request == teleporting.PlayerUID)
                    {
                        stillInDelay = true;
                        break;
                    }
                }

                if (!stillInDelay)
                {
                    RemoveDelay();
                    ResetCooldown();
                    teleporting.SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaRequestCancelled, receiver.PlayerName).ToString(), EnumChatType.CommandError);
                    serverAPI.Event.UnregisterGameTickListener(tickId);

                    Debug.LogDebug($"{teleporting.PlayerName} canceled due to not on tpaDelays");
                    return;
                }
            }
            else
            {
                RemoveDelay();
                ResetCooldown();
                teleporting.SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaRequestCancelled, receiver.PlayerName).ToString(), EnumChatType.CommandError);
                serverAPI.Event.UnregisterGameTickListener(tickId);

                Debug.LogDebug($"{teleporting.PlayerName} canceled due to {receiver.PlayerName} missing tpaDelays");
                return;
            }

            EntityPos playerActualPosition = teleporting.Entity.Pos.Copy();
            float playerActualHealth = teleporting.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;

            Debug.LogDebug($"{teleporting.PlayerName}: POS: {playerLastPosition.XYZ},{playerActualPosition.XYZ}");
            Debug.LogDebug($"{teleporting.PlayerName}: Health: {playerLastHealth},{playerActualHealth}");

            if (!Configuration.tpaCommandCanMove)
            {
                if (playerActualPosition.XYZ != playerLastPosition.XYZ)
                {
                    RemoveDelay();
                    teleporting.SendMessage(0, Configuration.translationTpaCancelledDueMoving, EnumChatType.CommandError);
                    serverAPI.Event.UnregisterGameTickListener(tickId);

                    Debug.LogDebug($"{teleporting.PlayerName} moved during tpa: {playerActualPosition.XYZ} : {playerLastPosition.XYZ}");
                    return;
                }
            }

            if (!Configuration.tpaCommandCanReceiveDamage)
            {
                if (playerActualHealth < playerLastHealth && (playerLastHealth - playerActualHealth) > 0.1f)
                {
                    RemoveDelay();
                    ResetCooldown();
                    teleporting.SendMessage(0, Configuration.translationTpaCancelledDueDamage, EnumChatType.CommandError);
                    serverAPI.Event.UnregisterGameTickListener(tickId);

                    Debug.LogDebug($"{teleporting.PlayerName} received damage during tpa: {playerActualHealth} : {playerLastHealth}");
                    return;
                }

                playerLastHealth = playerActualHealth;
            }

            ticksPassed++;
            if (ticksPassed >= Configuration.tpaCommandDelay)
            {
                if (!Utils.ConsumeItemsForCommandCost(teleporting, Configuration.tpaCostItemId, Configuration.tpaCostQuantity))
                {
                    RemoveDelay();
                    ResetCooldown();
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                    return;
                }

                if (tpaDelays.TryGetValue(receiver.PlayerUID, out _))
                    tpaDelays[receiver.PlayerUID].Remove(teleporting.PlayerUID);
                if (tpaDelays[receiver.PlayerUID].Count == 0)
                    tpaDelays.Remove(receiver.PlayerUID);

                if (Configuration.enableBackForTpa)
                    Back.InvokePlayerTeleported(teleporting, teleporting.Entity.Pos.Copy());
                teleporting.Entity.TeleportTo(receiver.Entity.Pos);
                serverAPI.Event.UnregisterGameTickListener(tickId);

                if (Configuration.tpaCooldown > 0)
                {
                    tpaCooldowns[teleporting.PlayerUID] = Configuration.tpaCooldown;
                    tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnTpaCooldownTick, 1000, 0);
                }
            }
        }

        if (Configuration.tpaCommandDelay <= 0)
        {
            if (!Utils.ConsumeItemsForCommandCost(teleporting, Configuration.tpaCostItemId, Configuration.tpaCostQuantity))
                return TextCommandResult.Success(string.Empty, "7");

            if (Configuration.enableBackForTpa)
                Back.InvokePlayerTeleported(teleporting, teleporting.Entity.Pos.Copy());

            teleporting.Entity.TeleportTo(receiver.Entity.Pos);

            if (Configuration.tpaCooldown > 0)
            {
                tpaCooldowns[teleporting.PlayerUID] = Configuration.tpaCooldown;
                tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnTpaCooldownTick, 1000, 0);
            }

            if (tpaRequests.TryGetValue(receiver.PlayerUID, out _))
            {
                tpaRequests[receiver.PlayerUID].Remove(teleporting.PlayerUID);
                if (tpaRequests[receiver.PlayerUID].Count == 0)
                    tpaRequests.Remove(receiver.PlayerUID);
            }

            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaAccepted, teleporting.PlayerName).ToString(), "13");
        }

        if (!Utils.CheckPlayerInventoryForCommandCost(teleporting, Configuration.tpaCostItemId, Configuration.tpaCostQuantity))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationNotEnoughItems, Utils.GetItemName(Configuration.tpaCostItemId), Configuration.tpaCostQuantity).ToString(), "7");

        if (tpaDelays.TryGetValue(receiver.PlayerUID, out _))
            if (!tpaDelays[receiver.PlayerUID].Contains(teleporting.PlayerUID))
                tpaDelays[receiver.PlayerUID].Add(teleporting.PlayerUID);
            else
                return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaAlreadyChanneling, teleporting.PlayerName).ToString(), "14");
        else
            tpaDelays[receiver.PlayerUID] = [teleporting.PlayerUID];

        playerLastPosition = teleporting.Entity.Pos.Copy();
        playerLastHealth = teleporting.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
        if (playerLastHealth <= 0 && !Configuration.tpaCommandCanReceiveDamage)
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaRequesterHealthInvalid, teleporting.PlayerName).ToString(), "3");

        tickId = serverAPI.Event.RegisterGameTickListener(OnTpaAcceptTick, 1000, 0);

        tpaRequests[receiver.PlayerUID].Remove(teleporting.PlayerUID);
        if (tpaRequests[receiver.PlayerUID].Count == 0)
            tpaRequests.Remove(receiver.PlayerUID);

        teleporting.SendMessage(0, TpaRequestAcceptedMessage(), EnumChatType.Notification);
        return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaAccepted, teleporting.PlayerName).ToString(), "13");
    }

    private static string TpaRequestAcceptedMessage()
    {
        if (!string.IsNullOrEmpty(Configuration.tpaCostItemId) && Configuration.tpaCostQuantity > 0)
            return new StringBuilder().AppendFormat(Configuration.translationTpaRequestAcceptedCost, Configuration.tpaCommandDelay, Configuration.tpaCostQuantity, Utils.GetItemName(Configuration.tpaCostItemId)).ToString();
        return new StringBuilder().AppendFormat(Configuration.translationTpaRequestAccepted, Configuration.tpaCommandDelay).ToString();
    }

    private TextCommandResult TpaDenyCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (tpaRequests.TryGetValue(player.PlayerUID, out List<string> playerRequests))
        {
            IPlayer playerTeleporting = null;
            foreach (string playerRequestUid in playerRequests)
            {
                foreach (IPlayer selectedPlayer in serverAPI.World.AllOnlinePlayers)
                {
                    if (playerRequestUid == selectedPlayer.PlayerUID)
                    {
                        playerTeleporting = selectedPlayer;
                        break;
                    }
                }
                if (playerTeleporting is not null)
                    if (!args.Parsers[0].IsMissing)
                        if (playerTeleporting.PlayerName == args[0] as string)
                            break;
            }

            string nameRemoved = null;
            if (playerTeleporting is null)
            {
                string requestUid = tpaRequests[player.PlayerUID][^1];
                foreach (IPlayer selectedPlayer in serverAPI.World.AllOnlinePlayers)
                {
                    if (requestUid == selectedPlayer.PlayerUID)
                    {
                        nameRemoved = selectedPlayer.PlayerName;
                        break;
                    }
                }
                tpaRequests[player.PlayerUID].RemoveAt(tpaRequests[player.PlayerUID].Count - 1);
            }
            else
            {
                foreach (string requestUid in tpaRequests[player.PlayerUID])
                {
                    if (requestUid == playerTeleporting.PlayerUID)
                    {
                        tpaRequests[player.PlayerUID].Remove(requestUid);
                        nameRemoved = playerTeleporting.PlayerName;
                        break;
                    }
                }
            }

            if (nameRemoved is null)
                return TextCommandResult.Success(Configuration.translationTpaRequestNotFound, "14");
            else
                return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaRequestDenied, playerTeleporting.PlayerName).ToString(), "15");
        }

        return TextCommandResult.Success(Configuration.translationTpaNoRequests, "11");
    }

    private TextCommandResult TpaCancelCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (tpaDelays.TryGetValue(player.PlayerUID, out List<string> playerRequests))
        {
            IPlayer playerTeleporting = null;
            foreach (string playerRequestUid in playerRequests)
            {
                foreach (IPlayer selectedPlayer in serverAPI.World.AllOnlinePlayers)
                {
                    if (playerRequestUid == selectedPlayer.PlayerUID)
                    {
                        playerTeleporting = selectedPlayer;
                        break;
                    }
                }
                if (playerTeleporting is not null)
                    if (!args.Parsers[0].IsMissing)
                        if (playerTeleporting.PlayerName == args[0] as string)
                            break;
            }

            if (playerTeleporting is null)
                return TextCommandResult.Success(Configuration.translationTpaRequestNotFound, "12");

            tpaDelays[player.PlayerUID].Remove(playerTeleporting.PlayerUID);
            if (tpaDelays[player.PlayerUID].Count == 0)
                tpaDelays.Remove(player.PlayerUID);

            return TextCommandResult.Success(new StringBuilder().AppendFormat(playerTeleporting.PlayerName).ToString(), "16");
        }
        else
            return TextCommandResult.Success(Configuration.translationTpaNoRequestToCancel, "17");
    }
}