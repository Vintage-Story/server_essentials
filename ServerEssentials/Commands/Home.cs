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

    /// <summary>
    /// [ PlayerUid,PlayerUid ]
    /// </summary>
    private readonly List<string> homeDelays = [];

    public Home(ICoreServerAPI api)
    {
        serverAPI = api;

        if (Configuration.Home.enableSetHomeCommand)
        {
            foreach (string syntax in Configuration.Home.setHomeSyntaxes)
            {
                // Create sethome command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.Translations.translationHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.Home.setHomePrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("homename", false))
                // Function Handle
                .HandleWith(SetHomeCommand);

                ServerEssentialsModSystem.Logger.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.Home.enableHomeCommand)
        {
            foreach (string syntax in Configuration.Home.homeSyntaxes)
            {
                // Create home command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.Translations.translationHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.Home.homePrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("homename", false))
                // Function Handle
                .HandleWith(HomeCommand);
                ServerEssentialsModSystem.Logger.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.Home.enableDelHomeCommand)
        {
            foreach (string syntax in Configuration.Home.delHomeSyntaxes)
            {
                // Create delhome command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.Translations.translationDelHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.Home.delHomePrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Need a argument called home name or not
                .WithArgs(new StringArgParser("homename", false))
                // Function Handle
                .HandleWith(DelHomeCommand);
                ServerEssentialsModSystem.Logger.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.Home.enableListHomeCommand)
        {
            foreach (string syntax in Configuration.Home.listHomeSyntaxes)
            {
                // Create listhome command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.Translations.translationListHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.Home.listHomePrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Function Handle
                .HandleWith(ListHomeCommand);
                ServerEssentialsModSystem.Logger.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.Home.enableBuyHomeCommand)
        {
            foreach (string syntax in Configuration.Home.buyHomeSyntaxes)
            {
                // Create buyhome command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.Translations.translationBuyHomeDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.Home.buyHomePrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Function Handle
                .HandleWith(BuyHomeCommand);
                ServerEssentialsModSystem.Logger.Log($"Command created: /{syntax}");
            }
        }
    }

    private TextCommandResult SetHomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (!Utils.CheckPlayerInventoryForCommandCost(player, Configuration.Home.homeCostItemId, Configuration.Home.homeCostQuantity))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.Translations.translationNotEnoughItems, Utils.GetItemName(Configuration.Home.homeCostItemId), Configuration.Home.homeCostQuantity).ToString(), "7");

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        string homeName = "home";
        if (!args.Parsers[0].IsMissing)
            homeName = args[0] as string;

        int playerMaxHomes = Configuration.Home.maxHomes + GetPlayerExtraHomes(player.PlayerUID);
        if (playerHomes.Count >= playerMaxHomes && !playerHomes.ContainsKey(homeName))
            return TextCommandResult.Success(Configuration.Translations.translationHomeMaxHomesReached, "0");

        if (!Utils.ConsumeItemsForCommandCost(player, Configuration.Home.homeCostItemId, Configuration.Home.homeCostQuantity))
            return TextCommandResult.Success(string.Empty, "7");

        playerHomes[homeName] = $"{player.Entity.Pos.X},{player.Entity.Pos.Y},{player.Entity.Pos.Z}";

        serverAPI.WorldManager.SaveGame.StoreData($"ServerEssentials_homes_{player.PlayerUID}", SerializerUtil.Serialize(playerHomes));

        return TextCommandResult.Success(HomeSetMessage(), "1");
    }

    private TextCommandResult HomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (homeCooldowns.TryGetValue(player.PlayerUID, out int secondsRemaing))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.Translations.translationHomeCooldown, secondsRemaing).ToString(), "7");

        if (homeDelays.Contains(player.PlayerUID))
            return TextCommandResult.Success(Configuration.Translations.translationHomeAlreadySent, "7");

        if (!Utils.CheckPlayerInventoryForCommandCost(player, Configuration.Home.homeCostItemId, Configuration.Home.homeCostQuantity))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.Translations.translationNotEnoughItems, Utils.GetItemName(Configuration.Home.homeCostItemId), Configuration.Home.homeCostQuantity).ToString(), "7");

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        string homeName = "home";
        if (!args.Parsers[0].IsMissing)
            homeName = args[0] as string;

        if (playerHomes.TryGetValue(homeName, out string position))
        {
            double[] coordinates = [.. position.Split(',').Select(double.Parse)];

            EntityPos playerLastPosition;
            float playerLastHealth;

            long tickId = 0;
            long tickCooldownId = 0;

            uint ticksPassed = 0;

            void OnHomeCooldownTick(float obj)
            {
                if (homeCooldowns.TryGetValue(player.PlayerUID, out _))
                {
                    homeCooldowns[player.PlayerUID] -= 1;
                    if (homeCooldowns[player.PlayerUID] <= 0)
                    {
                        homeCooldowns.Remove(player.PlayerUID);
                        serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
                    }
                }
                else serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
            }
            void OnHomeTick(float obj)
            {
                EntityPos playerActualPosition = player.Entity.Pos.Copy();
                float playerActualHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;

                ServerEssentialsModSystem.Logger.LogDebug($"{player.PlayerName}: POS: {playerLastPosition.XYZ},{playerActualPosition.XYZ}");
                ServerEssentialsModSystem.Logger.LogDebug($"{player.PlayerName}: Health: {playerLastHealth},{playerActualHealth}");

                if (!Configuration.Home.homeCommandCanMove)
                {
                    if (playerActualPosition.XYZ != playerLastPosition.XYZ)
                    {
                        player.SendMessage(0, Configuration.Translations.translationHomeCancelledDueMoving, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        homeDelays.Remove(player.PlayerUID);
                        return;
                    }
                }

                if (!Configuration.Home.homeCommandCanReceiveDamage)
                {
                    if (playerActualHealth < playerLastHealth && (playerLastHealth - playerActualHealth) > 0.1f)
                    {
                        player.SendMessage(0, Configuration.Translations.translationHomeCancelledDueDamage, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        homeDelays.Remove(player.PlayerUID);
                        return;
                    }

                    playerLastHealth = playerActualHealth;
                }

                ticksPassed++;
                if (ticksPassed >= Configuration.Home.homeCommandDelay)
                {
                    if (!Utils.ConsumeItemsForCommandCost(player, Configuration.Home.homeCostItemId, Configuration.Home.homeCostQuantity))
                    {
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        homeDelays.Remove(player.PlayerUID);
                        return;
                    }

                    if (Configuration.Back.enableBackForHome)
                        Back.InvokePlayerTeleported(player, player.Entity.Pos.Copy());
                    player.Entity.TeleportTo(new Vec3d(coordinates[0], coordinates[1], coordinates[2]));
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                    homeDelays.Remove(player.PlayerUID);
                    if (Configuration.Home.homeCooldown > 0)
                    {
                        homeCooldowns[player.PlayerUID] = Configuration.Home.homeCooldown;
                        tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnHomeCooldownTick, 1000, 0);
                    }
                }
            }

            if (Configuration.Home.homeCommandDelay <= 0)
            {
                if (!Utils.ConsumeItemsForCommandCost(player, Configuration.Home.homeCostItemId, Configuration.Home.homeCostQuantity))
                    return TextCommandResult.Success(string.Empty, "7");

                if (Configuration.Back.enableBackForHome)
                    Back.InvokePlayerTeleported(player, player.Entity.Pos.Copy());

                player.Entity.TeleportTo(new Vec3d(coordinates[0], coordinates[1], coordinates[2]));

                if (Configuration.Home.homeCooldown > 0)
                {
                    homeCooldowns[player.PlayerUID] = Configuration.Home.homeCooldown;
                    tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnHomeCooldownTick, 1000, 0);
                }

                return TextCommandResult.Success(HomeTeleportingMessage(homeName), "2");
            }

            playerLastPosition = player.Entity.Pos.Copy();
            playerLastHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
            if (playerLastHealth <= 0 && !Configuration.Home.homeCommandCanReceiveDamage)
                return TextCommandResult.Success(Configuration.Translations.translationHomeHealthInvalid, "3");

            homeDelays.Add(player.PlayerUID);
            tickId = serverAPI.Event.RegisterGameTickListener(OnHomeTick, 1000, 1000);

            return TextCommandResult.Success(HomeTeleportingMessage(homeName), "2");
        }
        else
            return TextCommandResult.Success(Configuration.Translations.translationHomeHomeNotSet, "2");
    }

    private int GetPlayerExtraHomes(string playerUID)
    {
        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_extraHomes_{playerUID}");
        return data == null ? 0 : SerializerUtil.Deserialize<int>(data);
    }

    private TextCommandResult BuyHomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        int currentExtra = GetPlayerExtraHomes(player.PlayerUID);

        if (Configuration.Home.buyHomeMaxSlots > 0 && currentExtra >= Configuration.Home.buyHomeMaxSlots)
            return TextCommandResult.Success(Configuration.Translations.translationBuyHomeMaxSlotsReached, "0");

        int actualCost = Configuration.Home.buyHomeCostQuantity + (currentExtra * Configuration.Home.buyHomeCostIncrement);

        if (!Utils.CheckPlayerInventoryForCommandCost(player, Configuration.Home.buyHomeCostItemId, actualCost))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.Translations.translationNotEnoughItems, Utils.GetItemName(Configuration.Home.buyHomeCostItemId), actualCost).ToString(), "7");

        if (!Utils.ConsumeItemsForCommandCost(player, Configuration.Home.buyHomeCostItemId, actualCost))
            return TextCommandResult.Success(string.Empty, "7");

        int newExtra = currentExtra + 1;
        serverAPI.WorldManager.SaveGame.StoreData($"ServerEssentials_extraHomes_{player.PlayerUID}", SerializerUtil.Serialize(newExtra));

        int newMax = Configuration.Home.maxHomes + newExtra;
        return TextCommandResult.Success(BuyHomeMessage(actualCost, newMax), "1");
    }

    private static string BuyHomeMessage(int actualCost, int newMax)
    {
        if (!string.IsNullOrEmpty(Configuration.Home.buyHomeCostItemId) && actualCost > 0)
            return new StringBuilder().AppendFormat(Configuration.Translations.translationBuyHomePurchasedCost, actualCost, Utils.GetItemName(Configuration.Home.buyHomeCostItemId), newMax).ToString();
        return new StringBuilder().AppendFormat(Configuration.Translations.translationBuyHomePurchased, newMax).ToString();
    }

    private static string HomeSetMessage()
    {
        if (!string.IsNullOrEmpty(Configuration.Home.homeCostItemId) && Configuration.Home.homeCostQuantity > 0)
            return new StringBuilder().AppendFormat(Configuration.Translations.translationHomeHomeSetCost, Configuration.Home.homeCostQuantity, Utils.GetItemName(Configuration.Home.homeCostItemId)).ToString();
        return Configuration.Translations.translationHomeHomeSet;
    }

    private static string HomeTeleportingMessage(string homeName)
    {
        if (!string.IsNullOrEmpty(Configuration.Home.homeCostItemId) && Configuration.Home.homeCostQuantity > 0)
            return new StringBuilder().AppendFormat(Configuration.Translations.translationHomeTeleportingCost, homeName, Configuration.Home.homeCostQuantity, Utils.GetItemName(Configuration.Home.homeCostItemId)).ToString();
        return new StringBuilder().AppendFormat(Configuration.Translations.translationHomeTeleporting, homeName).ToString();
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
            return TextCommandResult.Success(Configuration.Translations.translationHomeHomeRemoved, "3");
        }
        else
            return TextCommandResult.Success(Configuration.Translations.translationHomeHomeInvalid, "2");
    }

    private TextCommandResult ListHomeCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        byte[] data = serverAPI.WorldManager.SaveGame.GetData($"ServerEssentials_homes_{player.PlayerUID}");
        Dictionary<string, string> playerHomes = data == null ? [] : SerializerUtil.Deserialize<Dictionary<string, string>>(data);

        if (playerHomes.Count == 0)
            return TextCommandResult.Success(Configuration.Translations.translationHomeNoHomes, "5");

        string homes = Configuration.Translations.translationHomeHomesList;
        foreach (KeyValuePair<string, string> keyValuePair in playerHomes)
        {
            homes += Environment.NewLine + keyValuePair.Key;
            if (Configuration.Home.ListHomeCommandShowCoords)
            {
                double[] coordinates = [.. keyValuePair.Value.Split(',').Select(double.Parse)];
                coordinates[0] = Math.Round(coordinates[0] - serverAPI.World.DefaultSpawnPosition.X);
                coordinates[1] = Math.Round(coordinates[1] - serverAPI.World.DefaultSpawnPosition.Y);
                coordinates[2] = Math.Round(coordinates[2] - serverAPI.World.DefaultSpawnPosition.Z);

                homes += $" : X:{coordinates[0]} Y:{coordinates[1]} Z{coordinates[2]}";
            }
        }

        return TextCommandResult.Success(homes, "6");
    }
}