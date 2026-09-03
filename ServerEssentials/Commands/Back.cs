using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ServerEssentials.Commands;

public class Back
{
    private static ICoreServerAPI serverAPI;

    /// <summary>
    /// PlayerUID: [Player last position, seconds remaining]
    /// </summary>
    private static readonly Dictionary<string, KeyValuePair<EntityPos, int>> backData = [];

    /// <summary>
    /// { PlayerUID: secondsRemaining }
    /// </summary>
    private readonly Dictionary<string, int> backCooldowns = [];

    /// <summary>
    /// [ PlayerUid,PlayerUid ]
    /// </summary>
    private readonly List<string> backDelays = [];

    internal static void InvokePlayerTeleported(IServerPlayer player, EntityPos pos)
    {
        long tickId = 0;
        backData[player.PlayerUID] = new(pos, Configuration.Back.backCommandDuration);
        KeyValuePair<EntityPos, int> lastData = backData[player.PlayerUID];
        if (Configuration.Back.backCommandDuration <= -1) return;

        void OnBackTick(float obj)
        {
            if (backData.TryGetValue(player.PlayerUID, out KeyValuePair<EntityPos, int> actualData))
            {
                if (lastData.Value != actualData.Value || lastData.Key != actualData.Key)
                {

                    Initialization.Logger.LogDebug($"{player.PlayerName} has a new data removing the previously tick listener");
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                    return;
                }

                KeyValuePair<EntityPos, int> updatedData = new(actualData.Key, actualData.Value - 1);
                if (updatedData.Value <= 0)
                {
                    Initialization.Logger.LogDebug($"{player.PlayerName} back command has timeout removing it...");
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                    backData.Remove(player.PlayerUID);
                    return;
                }

                lastData = updatedData;
                backData[player.PlayerUID] = updatedData;
            }
            else serverAPI.Event.UnregisterGameTickListener(tickId);
        }
        tickId = serverAPI.Event.RegisterGameTickListener(OnBackTick, 1000, 1000);
    }

    public Back(ICoreServerAPI api)
    {
        serverAPI = api;

        if (Configuration.Back.enableBackCommand)
        {
            foreach (string syntax in Configuration.Back.backSyntaxes)
            {
                // Create back command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.Translations.translationBackDescription)
                // Chat privilege
                .RequiresPrivilege(Configuration.Back.backPrivilege)
                // Only if is a valid player
                .RequiresPlayer()
                // Function Handle
                .HandleWith(BackCommand);
                Initialization.Logger.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.Back.enableBackForDeath)
        {
            api.Event.PlayerDeath += BackPlayerDeath;
            Initialization.Logger.Log("Death event created for /back");
        }
    }

    private TextCommandResult BackCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (backCooldowns.TryGetValue(player.PlayerUID, out int secondsRemaining))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.Translations.translationBackCooldown, secondsRemaining).ToString(), "7");

        if (backDelays.Contains(player.PlayerUID))
            return TextCommandResult.Success(Configuration.Translations.translationBackAlreadySent, "7");

        if (!Utils.CheckPlayerInventoryForCommandCost(player, Configuration.Back.backCostItemId, Configuration.Back.backCostQuantity))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.Translations.translationNotEnoughItems, Utils.GetItemName(Configuration.Back.backCostItemId), Configuration.Back.backCostQuantity).ToString(), "7");

        if (backData.TryGetValue(player.PlayerUID, out KeyValuePair<EntityPos, int> data))
        {
            EntityPos playerLastPosition;
            float playerLastHealth;

            long tickId = 0;
            long tickCooldownId = 0;

            uint ticksPassed = 0;
            float playerLastMaxHealth;

            void OnBackCooldownTick(float obj)
            {
                if (backCooldowns.TryGetValue(player.PlayerUID, out _))
                {
                    backCooldowns[player.PlayerUID] -= 1;
                    if (backCooldowns[player.PlayerUID] <= 0)
                    {
                        backCooldowns.Remove(player.PlayerUID);
                        serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
                    }
                }
                else serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
            }
            void OnBackTick(float obj)
            {
                EntityPos playerActualPosition = player.Entity.Pos.Copy();
                float playerActualHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;

                Initialization.Logger.LogDebug($"{player.PlayerName}: POS: {playerLastPosition.XYZ},{playerActualPosition.XYZ}");
                Initialization.Logger.LogDebug($"{player.PlayerName}: Health: {playerLastHealth},{playerActualHealth}");

                if (!Configuration.Back.backCommandCanMove)
                {
                    if (playerActualPosition.XYZ != playerLastPosition.XYZ)
                    {
                        player.SendMessage(0, Configuration.Translations.translationBackCancelledDueMoving, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        backDelays.Remove(player.PlayerUID);
                        return;
                    }
                }

                if (!Configuration.Back.backCommandCanReceiveDamage)
                {
                    float playerActualMaxHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.MaxHealth ?? 0;
                    float healthDiff = playerLastHealth - playerActualHealth;
                    float maxHealthDiff = playerLastMaxHealth - playerActualMaxHealth;

                    if ((healthDiff - maxHealthDiff) > 0.1f)
                    {
                        player.SendMessage(0, Configuration.Translations.translationBackCancelledDueDamage, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        backDelays.Remove(player.PlayerUID);
                        return;
                    }

                    playerLastHealth = playerActualHealth;
                    playerLastMaxHealth = playerActualMaxHealth;
                }

                ticksPassed++;
                if (ticksPassed >= Configuration.Back.backCommandDelay)
                {
                    if (!Utils.ConsumeItemsForCommandCost(player, Configuration.Back.backCostItemId, Configuration.Back.backCostQuantity))
                    {
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        backDelays.Remove(player.PlayerUID);
                        return;
                    }

                    if (Configuration.Back.enableBackResycle)
                        InvokePlayerTeleported(player, player.Entity.Pos.Copy());
                    else
                        backData.Remove(player.PlayerUID);

                    player.Entity.TeleportTo(data.Key);
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                    backDelays.Remove(player.PlayerUID);

                    if (Configuration.Back.backCooldown > 0)
                    {
                        backCooldowns[player.PlayerUID] = Configuration.Back.backCooldown;
                        tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnBackCooldownTick, 1000, 0);
                    }
                }
            }

            if (Configuration.Back.backCommandDelay <= 0)
            {
                if (!Utils.ConsumeItemsForCommandCost(player, Configuration.Back.backCostItemId, Configuration.Back.backCostQuantity))
                    return TextCommandResult.Success(string.Empty, "7");

                if (Configuration.Back.enableBackResycle)
                    InvokePlayerTeleported(player, player.Entity.Pos.Copy());
                else
                    backData.Remove(player.PlayerUID);

                player.Entity.TeleportTo(data.Key);

                if (Configuration.Back.backCooldown > 0)
                {
                    backCooldowns[player.PlayerUID] = Configuration.Back.backCooldown;
                    tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnBackCooldownTick, 1000, 0);
                }
                return TextCommandResult.Success(BackTeleportingMessage(), "3");
            }

            playerLastPosition = player.Entity.Pos.Copy();
            playerLastHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
            playerLastMaxHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.MaxHealth ?? 0;
            if (playerLastHealth <= 0 && !Configuration.Back.backCommandCanReceiveDamage)
                return TextCommandResult.Success(Configuration.Translations.translationBackHealthInvalid, "3");

            backDelays.Add(player.PlayerUID);
            tickId = serverAPI.Event.RegisterGameTickListener(OnBackTick, 1000, 1000);

            return TextCommandResult.Success(BackTeleportingMessage(), "2");
        }
        else
            return TextCommandResult.Success(Configuration.Translations.translationBackNoBackAvailable, "2");
    }

    private static string BackTeleportingMessage()
    {
        if (!string.IsNullOrEmpty(Configuration.Back.backCostItemId) && Configuration.Back.backCostQuantity > 0)
            return new StringBuilder().AppendFormat(Configuration.Translations.translationBackTeleportingCost, Configuration.Back.backCostQuantity, Utils.GetItemName(Configuration.Back.backCostItemId)).ToString();
        return Configuration.Translations.translationBackTeleporting;
    }

    private void BackPlayerDeath(IServerPlayer byPlayer, DamageSource damageSource)
        => InvokePlayerTeleported(byPlayer, byPlayer.Entity.Pos.Copy());
}