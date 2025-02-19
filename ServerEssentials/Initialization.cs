using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ServerEssentials;

public class Initialization : ModSystem
{
    private ICoreServerAPI serverAPI;

    private readonly Dictionary<string, int> homeCooldowns = [];
    private readonly Dictionary<string, int> tpaCooldowns = [];
    private readonly Dictionary<string, List<string>> tpaRequests = [];
    private readonly Dictionary<string, List<string>> tpaDelays = [];

    public override void StartServerSide(ICoreServerAPI api)
    {
        serverAPI = api;
        base.StartServerSide(api);
        Debug.LoadLogger(api.Logger);

        Debug.Log($"Running on Version: {Mod.Info.Version}");

        if (Configuration.enableSetHomeCommand)
        {
            // Create sethome command
            api.ChatCommands.Create("sethome")
            // Description
            .WithDescription("Set a home using /sethome homename")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Need a argument called home name or not
            .WithArgs(new StringArgParser("homename", false))
            // Function Handle
            .HandleWith(SetHomeCommand);
        }
        if (Configuration.enableHomeCommand)
        {
            // Create home command
            api.ChatCommands.Create("home")
            // Description
            .WithDescription("Teleport to a home using /home homename")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Need a argument called home name or not
            .WithArgs(new StringArgParser("homename", false))
            // Function Handle
            .HandleWith(HomeCommand);
        }
        if (Configuration.enableDelHomeCommand)
        {
            // Create delhome command
            api.ChatCommands.Create("delhome")
            // Description
            .WithDescription("Delete a home /delhome homename")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Need a argument called home name or not
            .WithArgs(new StringArgParser("homename", false))
            // Function Handle
            .HandleWith(DelHomeCommand);
        }
        if (Configuration.enableListHomeCommand)
        {
            // Create listhome command
            api.ChatCommands.Create("listhome")
            // Description
            .WithDescription("View the home lists")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Function Handle
            .HandleWith(ListHomeCommand);
        }

        if (Configuration.enableTpaCommand)
        {
            // Create tpa command
            api.ChatCommands.Create("tpa")
            // Description
            .WithDescription("Teleport to a player using /tpa playername")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Need a argument called home name or not
            .WithArgs(new StringArgParser("playername", false))
            // Function Handle
            .HandleWith(TpaCommand);
        }
        if (Configuration.enableTpaAcceptCommand)
        {
            // Create tpaaccept command
            api.ChatCommands.Create("tpaaccept")
            // Description
            .WithDescription("Teleport to a player using /tpaaccept playername")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Need a argument called home name or not
            .WithArgs(new StringArgParser("playername", false))
            // Function Handle
            .HandleWith(TpaAcceptCommand);
        }
        if (Configuration.enableTpaDenyCommand)
        {
            // Create tpadeny command
            api.ChatCommands.Create("tpadeny")
            // Description
            .WithDescription("Deny a teleport request /tpadeny playername")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Need a argument called home name or not
            .WithArgs(new StringArgParser("playername", false))
            // Function Handle
            .HandleWith(TpaDenyCommand);
        }
        if (Configuration.enableTpaCancelCommand)
        {
            // Create tpacancel command
            api.ChatCommands.Create("tpacancel")
            // Description
            .WithDescription("Cancel a channeling teleport request /tpacancel playername")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Need a argument called home name or not
            .WithArgs(new StringArgParser("playername", false))
            // Function Handle
            .HandleWith(TpaCancelCommand);
        }
    }

    public override void AssetsLoaded(ICoreAPI api)
    {
        base.AssetsLoaded(api);
        Configuration.UpdateBaseConfigurations(api);
    }

    #region home
    private TextCommandResult SetHomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        if (playerHomes.Count >= Configuration.maxHomes)
            return TextCommandResult.Success("Max homes reached", "0");

        string homeName = "home";
        if (!args.Parsers[0].IsMissing)
            homeName = args[0] as string;

        playerHomes[homeName] = $"{player.Entity.Pos.X},{player.Entity.Pos.Y},{player.Entity.Pos.Z}";

        serverAPI.WorldManager.SaveGame.StoreData($"ServerEssentials_homes_{player.PlayerUID}", SerializerUtil.Serialize(playerHomes));

        return TextCommandResult.Success("Home Set!", "1");
    }

    private TextCommandResult HomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (homeCooldowns.TryGetValue(player.PlayerUID, out int secondsRemaing))
            return TextCommandResult.Success($"Home command is still on cooldown: {secondsRemaing} seconds remaining...", "7");

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        string homeName = "home";
        if (!args.Parsers[0].IsMissing)
            homeName = args[0] as string;

        if (playerHomes.TryGetValue(homeName, out string position))
        {
            double[] coordinates = position.Split(',').Select(double.Parse).ToArray();

            if (Configuration.homeCommandDelay == 0)
            {
                player.Entity.TeleportTo(new Vec3d(coordinates[0], coordinates[1], coordinates[2]));
                return TextCommandResult.Success($"Teleporting to {homeName}...", "2");
            }

            EntityPos playerLastPosition = player.Entity.Pos.Copy();
            float playerLastHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
            if (playerLastHealth <= 0 && !Configuration.homeCommandCanReceiveDamage)
                return TextCommandResult.Success($"Cannot teleport, your health is invalid", "3");

            long tickId = 0;
            long tickCooldownId = 0;

            uint ticksPassed = 0;

            void OnHomeCooldownTick(float obj)
            {
                if (homeCooldowns.TryGetValue(player.PlayerUID, out _))
                {
                    homeCooldowns[player.PlayerUID] -= 1;
                    if (homeCooldowns[player.PlayerUID] <= 0) homeCooldowns.Remove(player.PlayerUID);
                    serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
                }
                else homeCooldowns[player.PlayerUID] = Configuration.homeCooldown;
            }
            void OnHomeTick(float obj)
            {
                EntityPos playerActualPosition = player.Entity.Pos.Copy();
                float playerActualHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;

                if (Configuration.enableExtendedLogs)
                {
                    Debug.Log($"{player.PlayerName}: POS: {playerLastPosition.XYZ},{playerActualPosition.XYZ}");
                    Debug.Log($"{player.PlayerName}: Health: {playerLastHealth},{playerActualHealth}");
                }

                if (!Configuration.homeCommandCanMove)
                {
                    if (playerActualPosition.XYZ != playerLastPosition.XYZ)
                    {
                        player.SendMessage(0, "Teleport canceled, because you moved", EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        return;
                    }
                }

                if (!Configuration.homeCommandCanReceiveDamage)
                {
                    if (playerActualHealth < playerLastHealth)
                    {
                        player.SendMessage(0, "Teleport canceled, because you received damage", EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        return;
                    }

                    playerLastHealth = playerActualHealth;
                }

                ticksPassed++;
                if (ticksPassed >= Configuration.homeCommandDelay)
                {
                    player.Entity.TeleportTo(new Vec3d(coordinates[0], coordinates[1], coordinates[2]));
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                }
            }

            tickId = serverAPI.Event.RegisterGameTickListener(OnHomeTick, 1000, 1000);
            if (Configuration.homeCooldown > 0)
                tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnHomeCooldownTick, 1000, 0);

            return TextCommandResult.Success($"Teleporting to {homeName}...", "2");
        }
        else
            return TextCommandResult.Success("Home not set!", "2");
    }

    private TextCommandResult DelHomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        string homeName = "home";
        if (!args.Parsers[0].IsMissing)
            homeName = args[0] as string;

        if (playerHomes.TryGetValue(homeName, out _))
        {
            playerHomes.Remove(homeName);
            serverAPI.WorldManager.SaveGame.StoreData($"ServerEssentials_homes_{player.PlayerUID}", SerializerUtil.Serialize(playerHomes));
            return TextCommandResult.Success("Home removed!", "3");
        }
        else
            return TextCommandResult.Success("Invalid home!", "2");
    }

    private TextCommandResult ListHomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        if (playerHomes.Count == 0)
            return TextCommandResult.Success("You don't have any home set!", "5");

        string homes = "Your homes:";
        foreach (KeyValuePair<string, string> keyValuePair in playerHomes)
        {
            homes += Environment.NewLine + keyValuePair.Key;
        }

        return TextCommandResult.Success(homes, "6");
    }
    #endregion

    #region tpa
    private TextCommandResult TpaCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (tpaCooldowns.TryGetValue(player.PlayerUID, out int secondsRemaing))
            return TextCommandResult.Success($"Tpa command is still on cooldown: {secondsRemaing} seconds remaining...", "7");

        if (args.Parsers[0].IsMissing)
            return TextCommandResult.Success($"Missing player name", "8");

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
            return TextCommandResult.Success($"{args[0] as string} not found", "9");

        (playerToTeleport as IServerPlayer).SendMessage(0, $"{player.PlayerName} send you a tpa request, /tpaaccept or /tpadeny", EnumChatType.Notification);
        if (tpaRequests.TryGetValue(playerToTeleport.PlayerUID, out _))
            tpaRequests[playerToTeleport.PlayerUID].Add(player.PlayerUID);
        else
            tpaRequests[playerToTeleport.PlayerUID] = [player.PlayerUID];

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
                    player.SendMessage(0, $"{playerToTeleport.PlayerName} Tpa has expired", EnumChatType.Notification);
                    (playerToTeleport as IServerPlayer).SendMessage(0, $"{player.PlayerName} Tpa has expired", EnumChatType.Notification);
                }
            }
            // Expired
            else
                serverAPI.Event.UnregisterGameTickListener(tickid);

        }

        tickid = serverAPI.Event.RegisterGameTickListener(OnTpaTick, 1000, 1000);

        return TextCommandResult.Success($"Tpa request send to {playerToTeleport.PlayerName}", "10");
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
                return TextCommandResult.Success($"Request not found", "12");

            if (tpaCooldowns.TryGetValue(playerTeleporting.PlayerUID, out int secondsRemaing))
                return TextCommandResult.Success($"Tpa command is still on cooldown for {playerTeleporting.PlayerName}", "7");

            EntityPos playerLastPosition = playerTeleporting.Entity.Pos.Copy();
            float playerLastHealth = playerTeleporting.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
            if (playerLastHealth <= 0 && !Configuration.tpaCommandCanReceiveDamage)
                return TextCommandResult.Success($"Cannot teleport, {playerTeleporting.PlayerName} health is invalid", "3");

            if (tpaDelays.TryGetValue(player.PlayerUID, out _))
                if (!tpaDelays[player.PlayerUID].Contains(playerTeleporting.PlayerUID))
                    tpaDelays[player.PlayerUID].Add(playerTeleporting.PlayerUID);
                else
                    return TextCommandResult.Success($"The request already exists for {playerTeleporting.PlayerUID}", "14");
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
                else tpaCooldowns[playerTeleporting.PlayerUID] = Configuration.tpaCooldown;
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
                        (playerTeleporting as IServerPlayer).SendMessage(0, $"Teleport cancelled, by {player.PlayerName}", EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        return;
                    }
                }
                else
                {
                    RemoveDelay();
                    ResetCooldown();
                    (playerTeleporting as IServerPlayer).SendMessage(0, $"Teleport cancelled, by {player.PlayerName}", EnumChatType.CommandError);
                    serverAPI.Event.UnregisterGameTickListener(tickId);
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
                        (playerTeleporting as IServerPlayer).SendMessage(0, "Teleport canceled, because you moved", EnumChatType.CommandError);
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
                        (playerTeleporting as IServerPlayer).SendMessage(0, "Teleport canceled, because you received damage", EnumChatType.CommandError);
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
                    playerTeleporting.Entity.TeleportTo(player.Entity.Pos);
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                }
            }
            tickId = serverAPI.Event.RegisterGameTickListener(OnTpaAcceptTick, 1000, 0);

            tpaRequests[player.PlayerUID].Remove(playerTeleporting.PlayerUID);
            if (tpaRequests[player.PlayerUID].Count == 0)
                tpaRequests.Remove(player.PlayerUID);

            if (Configuration.tpaCooldown > 0)
                tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnTpaCooldownTick, 1000, 0);

            (playerTeleporting as IServerPlayer).SendMessage(0, $"Request accepted don't move for {Configuration.tpaCommandDelay} seconds", EnumChatType.Notification);
            return TextCommandResult.Success($"Request accepted: {playerTeleporting.PlayerName}", "13");
        }
        return TextCommandResult.Success($"No requests", "11");
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
                return TextCommandResult.Success($"Request cannot be found", "14");
            else
                return TextCommandResult.Success($"Request denied: {playerTeleporting.PlayerName}", "15");
        }

        return TextCommandResult.Success($"No requests", "11");
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
                return TextCommandResult.Success($"Teleport not found", "12");

            tpaDelays[player.PlayerUID].Remove(playerTeleporting.PlayerUID);
            if (tpaDelays[player.PlayerUID].Count == 0)
                tpaDelays.Remove(player.PlayerUID);

            return TextCommandResult.Success($"{playerTeleporting.PlayerName} teleport cancelled", "16");
        }
        else
            return TextCommandResult.Success($"No teleport to cancel", "17");
    }
    #endregion
}

public class Debug
{
    static private ILogger loggerForNonTerminalUsers;

    static public void LoadLogger(ILogger logger) => loggerForNonTerminalUsers = logger;
    static public void Log(string message)
        => loggerForNonTerminalUsers?.Log(EnumLogType.Notification, $"[ServerEssentials] {message}");
}