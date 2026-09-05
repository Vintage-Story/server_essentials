using System.Collections.Generic;
using OpenConfiguration;
using Vintagestory.API.Common;

namespace ServerEssentials;

public class HomeConfiguration
{
    public bool enableSetHomeCommand = true;
    public string setHomePrivilege = "chat";
    public List<string> setHomeSyntaxes = ["sethome"];
    public int maxHomes = 5;
    public bool enableHomeCommand = true;
    public string homePrivilege = "chat";
    public List<string> homeSyntaxes = ["home"];
    public int homeCommandDelay = 5;
    public int homeCooldown = 120;
    public string homeCostItemId = "";
    public int homeCostQuantity = 0;
    public bool homeCommandCanMove = false;
    public bool homeCommandCanReceiveDamage = false;
    public bool enableDelHomeCommand = true;
    public string delHomePrivilege = "chat";
    public List<string> delHomeSyntaxes = ["delhome"];
    public bool enableListHomeCommand = true;
    public string listHomePrivilege = "chat";
    public List<string> listHomeSyntaxes = ["listhome"];
    public bool ListHomeCommandShowCoords = true;
    public bool enableBuyHomeCommand = true;
    public string buyHomePrivilege = "chat";
    public List<string> buyHomeSyntaxes = ["buyhome"];
    public string buyHomeCostItemId = "game:gear-temporal";
    public int buyHomeCostQuantity = 1;
    public int buyHomeMaxSlots = 0;
    public int buyHomeCostIncrement = 0;
}

public static partial class Configuration
{
    public static HomeConfiguration Home = new();

    private static void LoadHome(ICoreAPI api)
        => Home = ConfigManager.LoadModConfig<HomeConfiguration>(api, "ServerEssentials", "home", ServerEssentialsModSystem.Logger);
}
