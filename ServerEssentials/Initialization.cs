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
    }

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
            if (playerLastHealth <= 0)
                return TextCommandResult.Success($"Cannot teleport, your health is invalid", "3");

            long tickId = 0;

            uint ticksPassed = 0;
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

        return TextCommandResult.Success(homes, "5");
    }
}

public class Debug
{
    static private ILogger loggerForNonTerminalUsers;

    static public void LoadLogger(ILogger logger) => loggerForNonTerminalUsers = logger;
    static public void Log(string message)
        => loggerForNonTerminalUsers?.Log(EnumLogType.Notification, $"[ServerEssentials] {message}");
}