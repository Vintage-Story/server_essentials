using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace ServerEssentials.Commands;

public class CustomCommands
{
    public CustomCommands(ICoreServerAPI api)
    {
        foreach (CustomCommandEntry entry in Configuration.customCommands)
        {
            string message = entry.Message;

            api.ChatCommands.Create(entry.Syntax)
                .WithDescription(entry.Message)
                .RequiresPrivilege(entry.Privilege)
                .HandleWith(_ => TextCommandResult.Success(message, "1"));

            Debug.Log($"Command created: /{entry.Syntax}");
        }
    }
}
