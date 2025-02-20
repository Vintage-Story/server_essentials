using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.API.Util;
using Vintagestory.GameContent;

namespace ServerEssentials.Commands;

public class Home
{
    private readonly ICoreServerAPI serverAPI;

    /// <summary>
    /// { PlayerUID: secondsRemaining }
    /// </summary>
    private readonly Dictionary<string, int> homeCooldowns = [];

    public Home(ICoreServerAPI api)
    {
        serverAPI = api;

        if (Configuration.enableSetHomeCommand)
        {
            foreach (string syntax in Configuration.setHomeSyntaxes)
            {
                // Create sethome command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Privilege.chat)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("homename", false))
                // Function Handle
                .HandleWith(SetHomeCommand);

                Debug.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.enableHomeCommand)
        {
            foreach (string syntax in Configuration.homeSyntaxes)
            {
                // Create home command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Privilege.chat)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("homename", false))
                // Function Handle
                .HandleWith(HomeCommand);
                Debug.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.enableDelHomeCommand)
        {
            foreach (string syntax in Configuration.delHomeSyntaxes)
            {
                // Create delhome command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationDelHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Privilege.chat)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("homename", false))
                // Function Handle
                .HandleWith(DelHomeCommand);
                Debug.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.enableListHomeCommand)
        {
            foreach (string syntax in Configuration.listHomeSyntaxes)
            {
                // Create listhome command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationListHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Privilege.chat)
                // Only if is a valid player
                .RequiresPlayer()
                // Function Handle
                .HandleWith(ListHomeCommand);
                Debug.Log($"Command created: /{syntax}");
            }
        }
    }

    private TextCommandResult SetHomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        if (playerHomes.Count >= Configuration.maxHomes)
            return TextCommandResult.Success(Configuration.translationHomeMaxHomesReached, "0");

        string homeName = "home";
        if (!args.Parsers[0].IsMissing)
            homeName = args[0] as string;

        playerHomes[homeName] = $"{player.Entity.Pos.X},{player.Entity.Pos.Y},{player.Entity.Pos.Z}";

        serverAPI.WorldManager.SaveGame.StoreData($"ServerEssentials_homes_{player.PlayerUID}", SerializerUtil.Serialize(playerHomes));

        return TextCommandResult.Success(Configuration.translationHomeHomeSet, "1");
    }

    private TextCommandResult HomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (homeCooldowns.TryGetValue(player.PlayerUID, out int secondsRemaing))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationHomeCooldown, secondsRemaing).ToString(), "7");

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        string homeName = "home";
        if (!args.Parsers[0].IsMissing)
            homeName = args[0] as string;

        if (playerHomes.TryGetValue(homeName, out string position))
        {
            double[] coordinates = [.. position.Split(',').Select(double.Parse)];

            if (Configuration.homeCommandDelay == 0)
            {
                if (Configuration.enableBackForHome)
                    Back.InvokePlayerTeleported(player, player.Entity.Pos.Copy());
                player.Entity.TeleportTo(new Vec3d(coordinates[0], coordinates[1], coordinates[2]));
                return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationHomeTeleporting, homeName).ToString(), "2");
            }

            EntityPos playerLastPosition = player.Entity.Pos.Copy();
            float playerLastHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
            if (playerLastHealth <= 0 && !Configuration.homeCommandCanReceiveDamage)
                return TextCommandResult.Success(Configuration.translationHomeHealthInvalid, "3");

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
                        player.SendMessage(0, Configuration.translationHomeCancelledDueMoving, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        return;
                    }
                }

                if (!Configuration.homeCommandCanReceiveDamage)
                {
                    // This is necessary because the health system keep changing between server ticks for some fucking reason
                    if (Math.Abs(playerLastHealth - playerActualHealth) > 0.1)
                    {
                        player.SendMessage(0, Configuration.translationHomeCancelledDueDamage, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        return;
                    }

                    playerLastHealth = playerActualHealth;
                }

                ticksPassed++;
                if (ticksPassed >= Configuration.homeCommandDelay)
                {
                    if (Configuration.enableBackForHome)
                        Back.InvokePlayerTeleported(player, player.Entity.Pos.Copy());
                    player.Entity.TeleportTo(new Vec3d(coordinates[0], coordinates[1], coordinates[2]));
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                }
            }

            tickId = serverAPI.Event.RegisterGameTickListener(OnHomeTick, 1000, 1000);
            if (Configuration.homeCooldown > 0)
                tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnHomeCooldownTick, 1000, 0);

            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationHomeTeleporting, homeName).ToString(), "2");
        }
        else
            return TextCommandResult.Success(Configuration.translationHomeHomeNotSet, "2");
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
            return TextCommandResult.Success(Configuration.translationHomeHomeRemoved, "3");
        }
        else
            return TextCommandResult.Success(Configuration.translationHomeHomeInvalid, "2");
    }

    private TextCommandResult ListHomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        if (playerHomes.Count == 0)
            return TextCommandResult.Success(Configuration.translationHomeNoHomes, "5");

        string homes = Configuration.translationHomeHomesList;
        foreach (KeyValuePair<string, string> keyValuePair in playerHomes)
        {
            homes += Environment.NewLine + keyValuePair.Key;
            if (Configuration.ListHomeCommandShowCoords)
            {
                double[] coordinates = [.. keyValuePair.Value.Split(',').Select(double.Parse)];
                coordinates[0] = coordinates[0] - serverAPI.World.DefaultSpawnPosition.X;
                coordinates[1] = coordinates[1] - serverAPI.World.DefaultSpawnPosition.Y;
                coordinates[2] = coordinates[2] - serverAPI.World.DefaultSpawnPosition.Z;

                homes += $" : X:{coordinates[0]} Y:{coordinates[1]} Z{coordinates[2]}";
            }
        }

        return TextCommandResult.Success(homes, "6");
    }
}