using System;
using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace ServerEssentials.Commands;

public class Back
{
    private static readonly ICoreServerAPI serverAPI;

    /// <summary>
    /// PlayerUID: [Player last position, seconds remaining]
    /// </summary>
    private static readonly Dictionary<string, KeyValuePair<EntityPos, int>> backData = [];

    /// <summary>
    /// { PlayerUID: secondsRemaining }
    /// </summary>
    private readonly Dictionary<string, int> backCooldowns = [];

    public static event EventHandler PlayerTeleported;
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
        if (Configuration.enableBackCommand)
        {
            // Create tpa command
            api.ChatCommands.Create("back")
            // Description
            .WithDescription("Returns to your previous position before teleporting using /back")
            // Chat privilege
            .RequiresPrivilege(Privilege.chat)
            // Only if is a valid player
            .RequiresPlayer()
            // Need a argument called home name or not
            .WithArgs(new StringArgParser("playername", false))
            // Function Handle
            .HandleWith(BackCommand);
            Debug.Log("Command created: /back");
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

        if (backData.TryGetValue(player.PlayerUID, out KeyValuePair<EntityPos, int> data))
        {
            EntityPos playerLastPosition = player.Entity.Pos.Copy();
            float playerLastHealth = player.Entity.GetBehavior<EntityBehaviorHealth>()?.Health ?? 0;
            if (playerLastHealth <= 0 && !Configuration.homeCommandCanReceiveDamage)
                return TextCommandResult.Success($"Cannot teleport, your health is invalid", "3");

            long tickId = 0;
            long tickCooldownId = 0;

            uint ticksPassed = 0;

            void OnBackCooldownTick(float obj)
            {
                if (backCooldowns.TryGetValue(player.PlayerUID, out _))
                {
                    backCooldowns[player.PlayerUID] -= 1;
                    if (backCooldowns[player.PlayerUID] <= 0) backCooldowns.Remove(player.PlayerUID);
                    serverAPI.Event.UnregisterGameTickListener(tickCooldownId);
                }
                else backCooldowns[player.PlayerUID] = Configuration.backCooldown;
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
                        player.SendMessage(0, "Teleport canceled, because you moved", EnumChatType.CommandError);
                        serverAPI.Event.UnregisterGameTickListener(tickId);
                        return;
                    }
                }

                if (!Configuration.backCommandCanReceiveDamage)
                {
                    // This is necessary because the health system keep changing between server ticks for some fucking reason
                    if (Math.Abs(playerLastHealth - playerActualHealth) > 0.1)
                    {
                        player.SendMessage(0, "Teleport canceled, because you received damage", EnumChatType.CommandError);
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
                }
            }

            tickId = serverAPI.Event.RegisterGameTickListener(OnBackTick, 1000, 1000);
            if (Configuration.backCooldown > 0)
                tickCooldownId = serverAPI.Event.RegisterGameTickListener(OnBackCooldownTick, 1000, 0);

            return TextCommandResult.Success($"Teleporting to previously position...", "2");
        }
        else
            return TextCommandResult.Success("No previously position to go back!", "2");
    }

    private void BackPlayerDeath(IServerPlayer byPlayer, DamageSource damageSource)
        => InvokePlayerTeleported(byPlayer, byPlayer.Entity.Pos.Copy());
}