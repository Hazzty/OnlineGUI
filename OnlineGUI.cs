using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using Oxide.Core.Plugins;
using Oxide.Core;
using Oxide.Game.Rust.Cui;
namespace Oxide.Plugins
{
    [Info("OnlineGUI", "Bamabo", "1.0.0")]
    [Description("Adds a discrete player counter to the HUD")]

    class OnlineGUI : RustPlugin
    {
        private Dictionary<ulong, bool> players = new Dictionary<ulong, bool>();

        private CuiElementContainer elements;
        private string container;
        private CuiLabel playerCounter;
        void Init()
        {
            permission.RegisterPermission("onlinegui.onlinegui", this);
            players = Interface.Oxide.DataFileSystem.ReadObject<Dictionary<ulong, bool>>("OnlineGUI");

            elements = new CuiElementContainer();
            container = elements.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0.1 0.1 0.1 0"
                },
                RectTransform =
                {
                    AnchorMin = "0.9 0.97",
                    AnchorMax = "0.997 1"
                }
            });
            playerCounter = new CuiLabel
            { 
                Text =
                {
                    Text = String.Format("{0}/{1}" , BasePlayer.activePlayerList.Count, ConVar.Server.maxplayers) ,
                    FontSize = Convert.ToInt32(Config["fontsize"]) > 22 ? 22 : Convert.ToInt32(Config["fontsize"]),
                    Color = Config["color"].ToString(),
                    Align = TextAnchor.MiddleRight
               },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "1 1"
              }
            };
            elements.Add(playerCounter, container);

            foreach (var player in BasePlayer.activePlayerList)
                if (players[player.userID] == true && (permission.UserHasPermission(player.UserIDString, "onlinegui.onlinegui") || player.IsAdmin())) 
                    CuiHelper.AddUi(player, elements);
        }
        void Loaded()
        {
            timer.Repeat(Convert.ToSingle(Config["rate"]), 0, () =>
            {
                UpdateCounter();
            });
        }

        void OnPlayerInit(BasePlayer player)
        {
            if(!players.ContainsKey(player.userID))
            {
                players.Add(player.userID, true);
            }
        }

        void Unload()
        {
            Interface.Oxide.DataFileSystem.WriteObject("OnlineGUI", players);

            foreach (var player in BasePlayer.activePlayerList)
                CuiHelper.DestroyUi(player, container);
        }
	   
        void UpdateCounter()
        {
            foreach (var player in BasePlayer.activePlayerList)
                CuiHelper.DestroyUi(player, container);

            elements = new CuiElementContainer();
            container = elements.Add(new CuiPanel
            {
                Image =
                {
                    Color = "0 0 0 0"
                },
                RectTransform =
                {
                    AnchorMin = "0.9 0.97",
                    AnchorMax = "0.997 1"
                }
            });
            playerCounter = new CuiLabel
            {
                Text =
                {
                    Text = String.Format("{0}/{1}" , BasePlayer.activePlayerList.Count, ConVar.Server.maxplayers) ,
                    FontSize = Convert.ToInt32(Config["fontsize"]) > 22 ? 22 : Convert.ToInt32(Config["fontsize"]),
                    Color = Config["color"].ToString(),
                    Align = TextAnchor.MiddleRight
               },
                RectTransform =
                {
                    AnchorMin = "0 0",
                    AnchorMax = "1 1"
              }
            };
            elements.Add(playerCounter, container);

            foreach (var player in BasePlayer.activePlayerList)
                if (players[player.userID] == true && (permission.UserHasPermission(player.UserIDString, "onlinegui.onlinegui") || player.IsAdmin()))
                    CuiHelper.AddUi(player, elements);
        }

        [ChatCommand("OnlineGUI")]
        void cmdOnline(BasePlayer sender, string command, String[] args)
        {
            if (permission.UserHasPermission(sender.UserIDString, "onlinegui.onlinegui") || sender.IsAdmin())
            {
                if (players.ContainsKey(sender.userID))
                {
                    if (players[sender.userID] == true)
                    {
                        players[sender.userID] = false;
                        CuiHelper.DestroyUi(sender, container);
                        rust.SendChatMessage(sender, "Toggled OnlineGUI Off", null, Config["serverIconID"].ToString());
                    }
                    else
                    {
                        players[sender.userID] = true;
                        CuiHelper.AddUi(sender, elements);
                        rust.SendChatMessage(sender, "Toggled OnlineGUI On", null, Config["serverIconID"].ToString());
                    }
                }
            }
            else
            {
                rust.SendChatMessage(sender, "<color=red>You do not have access to that command.</color>", null, Config["serverIconID"].ToString());
            }
        }

        protected override void LoadDefaultConfig()
        {
            PrintWarning("Creating a new configuration file for OnlineGUI");
            Config.Clear();
            Config["color"] = "1 1 1 0.5";
            Config["fontsize"] = 18;
            Config["rate"] = 2.0;
            Config["serverIconID"] = "00000000000000000";
            SaveConfig();
        }
    }   
}