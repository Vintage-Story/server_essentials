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
                .RequiresPrivilege(Privilege.chat)
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
                .RequiresPrivilege(Privilege.chat)
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
                .RequiresPrivilege(Privilege.chat)
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
                .RequiresPrivilege(Privilege.chat)
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

            if (tpaCooldowns.TryGetValue(playerTeleporting.PlayerUID, out _))
                return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaRequesterOnCooldown, playerTeleporting.PlayerName).ToString(), "7");

            EntityPos playerLastPosition = playerTeleporting.Entity.Pos.Copy();
            float playerLastHealth = playerTeleporting.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
            if (playerLastHealth <= 0 && !Configuration.tpaCommandCanReceiveDamage)
                return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaRequesterHealthInvalid, playerTeleporting.PlayerName).ToString(), "3");

            if (tpaDelays.TryGetValue(player.PlayerUID, out _))
                if (!tpaDelays[player.PlayerUID].Contains(playerTeleporting.PlayerUID))
                    tpaDelays[player.PlayerUID].Add(playerTeleporting.PlayerUID);
                else
                    return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaAlreadyChanneling, playerTeleporting.PlayerName).ToString(), "14");
            else
                tpaDelays[player.PlayerUID] = [playerTeleporting.PlayerUID];

            long tickId = 0;
            long tickCooldownId = 0;

            uint ticksPassed = 0;

            void OnTpaCooldownTick(float obj)
            {
                if (tpaCooldowns.TryGetValue(playerTeleporting.PlayerUID, out _))
                {
                    tpaCooldowns[playerTeleporting.PlayerUID] -= 1;
                    if (tpaCooldowns[playerTeleporting.PlayerUID] <= 0)
                    {
                        tpaCooldowns.Remove(playerTeleporting.PlayerUID);
                        serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
                    }
                }
                else serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
            }
            void OnTpaAcceptTick(float obj)
            {
                void RemoveDelay()
                {
                    if (tpaDelays.TryGetValue(player.PlayerUID, out _))
                    {
                        tpaDelays[player.PlayerUID].Remove(playerTeleporting.PlayerUID);
                        if (tpaDelays[player.PlayerUID].Count == 0)
                            tpaDelays.Remove(player.PlayerUID);
                    }
                }
                void ResetCooldown()
                {
                    if (Configuration.tpaCommandResetCooldownOnCancellation)
                        tpaCooldowns.Remove(playerTeleporting.PlayerUID);
                }

                if (tpaDelays.TryGetValue(player.PlayerUID, out List<string> requests))
                {
                    bool stillInDelay = false;
                    foreach (string request in requests)
                    {
                        if (request == playerTeleporting.PlayerUID)
                        {
                            stillInDelay = true;
                            break;
                        }
                    }

                    if (!stillInDelay)
                    {
                        RemoveDelay();
                        ResetCooldown();
                        (playerTeleporting as IServerPlayer).SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaRequestCancelled, player.PlayerName).ToString(), EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);

                        if (Configuration.enableExtendedLogs)
                            Debug.Log($"{playerTeleporting.PlayerName} canceled due to not on tpaDelays");
                        return;
                    }
                }
                else
                {
                    RemoveDelay();
                    ResetCooldown();
                    (playerTeleporting as IServerPlayer).SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaRequestCancelled, player.PlayerName).ToString(), EnumChatType.CommandError);
                    serverAPI.Event.UnregisterGameTickListener(tickId);

                    if (Configuration.enableExtendedLogs)
                        Debug.Log($"{playerTeleporting.PlayerName} canceled due to {player.PlayerName} missing tpaDelays");
                    return;
                }

                EntityPos playerActualPosition = playerTeleporting.Entity.Pos.Copy();
                float playerActualHealth = playerTeleporting.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;

                if (Configuration.enableExtendedLogs)
                {
                    Debug.Log($"{playerTeleporting.PlayerName}: POS: {playerLastPosition.XYZ},{playerActualPosition.XYZ}");
                    Debug.Log($"{playerTeleporting.PlayerName}: Health: {playerLastHealth},{playerActualHealth}");
                }

                if (!Configuration.tpaCommandCanMove)
                {
                    if (playerActualPosition.XYZ != playerLastPosition.XYZ)
                    {
                        RemoveDelay();
                        (playerTeleporting as IServerPlayer).SendMessage(0, Configuration.translationTpaCancelledDueMoving, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);

                        if (Configuration.enableExtendedLogs)
                            Debug.Log($"{playerTeleporting.PlayerName} moved during tpa: {playerActualPosition.XYZ} : {playerLastPosition.XYZ}");
                        return;
                    }
                }

                if (!Configuration.tpaCommandCanReceiveDamage)
                {
                    // This is necessary because the health system keep changing between server ticks for some fucking reason
                    if (Math.Abs(playerLastHealth - playerActualHealth) > 0.1)
                    {
                        RemoveDelay();
                        ResetCooldown();
                        (playerTeleporting as IServerPlayer).SendMessage(0, Configuration.translationTpaCancelledDueDamage, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);

                        if (Configuration.enableExtendedLogs)
                            Debug.Log($"{playerTeleporting.PlayerName} received damage during tpa: {playerActualHealth} : {playerLastHealth}");
                        return;
                    }

                    playerLastHealth = playerActualHealth;
                }

                ticksPassed++;
                if (ticksPassed >= Configuration.tpaCommandDelay)
                {
                    if (tpaDelays.TryGetValue(player.PlayerUID, out _))
                        tpaDelays[player.PlayerUID].Remove(playerTeleporting.PlayerUID);
                    if (tpaDelays[player.PlayerUID].Count == 0)
                        tpaDelays.Remove(player.PlayerUID);

                    if (Configuration.enableBackForTpa)
                        Back.InvokePlayerTeleported(playerTeleporting as IServerPlayer, playerTeleporting.Entity.Pos.Copy());
                    playerTeleporting.Entity.TeleportTo(player.Entity.Pos);
                    serverAPI.Event.UnregisterGameTickListener(tickId);

                    if (Configuration.tpaCooldown > 0) {
                        tpaCooldowns[playerTeleporting.PlayerUID] = Configuration.tpaCooldown;
                        tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnTpaCooldownTick, 1000, 0);
                    }
                }
            }
            tickId = serverAPI.Event.RegisterGameTickListener(OnTpaAcceptTick, 1000, 0);

            tpaRequests[player.PlayerUID].Remove(playerTeleporting.PlayerUID);
            if (tpaRequests[player.PlayerUID].Count == 0)
                tpaRequests.Remove(player.PlayerUID);

            (playerTeleporting as IServerPlayer).SendMessage(0, new StringBuilder().AppendFormat(Configuration.translationTpaRequestAccepted, Configuration.tpaCommandDelay).ToString(), EnumChatType.Notification);
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationTpaAccepted, playerTeleporting.PlayerName).ToString(), "13");
        }
        return TextCommandResult.Success(Configuration.translationTpaNoRequests, "11");
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