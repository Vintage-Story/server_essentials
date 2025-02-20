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

    internal static void InvokePlayerTeleported(IServerPlayer player, EntityPos pos)
    {
        long tickId = 0;
        backData[player.PlayerUID] = new(pos, Configuration.backCommandDuration);
        KeyValuePair<EntityPos, int> lastData = backData[player.PlayerUID];
        if (Configuration.backCommandDuration <= -1) return;

        void OnBackTick(float obj)
        {
            if (backData.TryGetValue(player.PlayerUID, out KeyValuePair<EntityPos, int> actualData))
            {
                if (lastData.Value != actualData.Value || lastData.Key != actualData.Key)
                {
                    if (Configuration.enableExtendedLogs)
                        Debug.Log($"{player.PlayerName} has a new data removing the previously tick listener");
                    serverAPI.Event.UnregisterGameTickListener(tickId);
                    return;
                }

                KeyValuePair<EntityPos, int> updatedData = new(actualData.Key, actualData.Value - 1);
                if (updatedData.Value <= 0)
                {
                    if (Configuration.enableExtendedLogs)
                        Debug.Log($"{player.PlayerName} back command has timeout removing it...");
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

        if (Configuration.enableBackCommand)
        {
            foreach (string syntax in Configuration.backSyntaxes)
            {
                // Create back command
                api.ChatCommands.Create(syntax)
                // Description
                .WithDescription(Configuration.translationBackDescription)
                // Chat privilege
                .RequiresPrivilege(Privilege.chat)
                // Only if is a valid player
                .RequiresPlayer()
                // Function Handle
                .HandleWith(BackCommand);
                Debug.Log($"Command created: /{syntax}");
            }
        }
        if (Configuration.enableBackForDeath)
        {
            api.Event.PlayerDeath += BackPlayerDeath;
            Debug.Log("Death event created for /back");
        }
    }

    private TextCommandResult BackCommand(TextCommandCallingArgs args)
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;

        if (backCooldowns.TryGetValue(player.PlayerUID, out int secondsRemaining))
            return TextCommandResult.Success(new StringBuilder().AppendFormat(Configuration.translationBackCooldown, secondsRemaining).ToString(), "7");

        if (backData.TryGetValue(player.PlayerUID, out KeyValuePair<EntityPos, int> data))
        {
            EntityPos playerLastPosition = player.Entity.Pos.Copy();
            float playerLastHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
            if (playerLastHealth <= 0 && !Configuration.backCommandCanReceiveDamage)
                return TextCommandResult.Success(Configuration.translationBackHealthInvalid, "3");

            long tickId = 0;
            long tickCooldownId = 0;

            uint ticksPassed = 0;

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

                if (Configuration.enableExtendedLogs)
                {
                    Debug.Log($"{player.PlayerName}: POS: {playerLastPosition.XYZ},{playerActualPosition.XYZ}");
                    Debug.Log($"{player.PlayerName}: Health: {playerLastHealth},{playerActualHealth}");
                }

                if (!Configuration.backCommandCanMove)
                {
                    if (playerActualPosition.XYZ != playerLastPosition.XYZ)
                    {
                        player.SendMessage(0, Configuration.translationBackCancelledDueMoving, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        return;
                    }
                }

                if (!Configuration.backCommandCanReceiveDamage)
                {
                    // This is necessary because the health system keep changing between server ticks for some fucking reason
                    if (Math.Abs(playerLastHealth - playerActualHealth) > 0.1)
                    {
                        player.SendMessage(0, Configuration.translationBackCancelledDueDamage, EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        return;
                    }

                    playerLastHealth = playerActualHealth;
                }

                ticksPassed++;
                if (ticksPassed >= Configuration.backCommandDelay)
                {
                    if (Configuration.enableBackResycle)
                        InvokePlayerTeleported(player, player.Entity.Pos.Copy());
                    else
                        backData.Remove(player.PlayerUID);

                    player.Entity.TeleportTo(data.Key);
                    serverAPI.Event.UnregisterGameTickListener(tickId);

                    if (Configuration.backCooldown > 0)
                    {
                        backCooldowns[player.PlayerUID] = Configuration.backCooldown;
                        tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnBackCooldownTick, 1000, 0);
                    }
                }
            }

            tickId = serverAPI.Event.RegisterGameTickListener(OnBackTick, 1000, 1000);

            return TextCommandResult.Success(Configuration.translationBackTeleporting, "2");
        }
        else
            return TextCommandResult.Success(Configuration.translationBackNoBackAvailable, "2");
    }

    private void BackPlayerDeath(IServerPlayer byPlayer, DamageSource damageSource)
        => InvokePlayerTeleported(byPlayer, byPlayer.Entity.Pos.Copy());
}