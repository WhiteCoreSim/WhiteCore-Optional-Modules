﻿/*
 * This file's license:
 * 
 *  Copyright 2011 Matthew Beardmore
 *
 *  This file is part of Aurora.Addon.IRCChat.
 *  Aurora.Addon.IRCChat is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 *  Aurora.Addon.IRCChat is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 *  You should have received a copy of the GNU General Public License along with Aurora.Addon.IRCChat. If not, see http://www.gnu.org/licenses/.
 *
 * 
 * MetaBuilders.Irc.dll License:
 * 
 *  Microsoft Permissive License (Ms-PL)
 *  This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.
 *  1. Definitions
 *  The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.
 *  A "contribution" is the original software, or any additions or changes to the software.
 *  A "contributor" is any person that distributes its contribution under this license.
 *  "Licensed patents" are a contributor's patent claims that read directly on its contribution.
 *  2. Grant of Rights
 *  (A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.
 *  (B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.
 *  3. Conditions and Limitations
 *  (A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.
 *  (B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.
 *  (C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.
 *  (D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.
 *  (E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.

    Updated May 2016 for WhiteCore
    ..with a few more mods by Rowan Deppeler<greythane@gmail.com>
    Feb 2019 - included clean up nebo source to remove dll requirement

*/

using System;
using System.Collections.Generic;
using MetaBuilders.Irc;
using MetaBuilders.Irc.Messages;
using MetaBuilders.Irc.Network;
using Nini.Config;
using OpenMetaverse;
using WhiteCore.Framework.ClientInterfaces;
using WhiteCore.Framework.ConsoleFramework;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.PresenceInfo;
using WhiteCore.Framework.SceneInfo;
using WhiteCore.Framework.Servers;
using WhiteCore.Framework.Utilities;
using GridRegion = WhiteCore.Framework.Services.GridRegion;

namespace WhiteCore.Addon.IRCChat
{
    public class IRCParcelService : INonSharedRegionModule
    {
        Dictionary<UUID, string> m_network = new Dictionary<UUID, string>();
        Dictionary<UUID, string> m_channel = new Dictionary<UUID, string>();
        IScene m_scene;
        bool m_spamDebug = false;
        bool m_enabled = false;
        int m_chatToIRCChannel = 0;
        Dictionary<UUID, Client> clients = new Dictionary<UUID, Client>();
        IConfig m_config;

        public void Initialise(IConfigSource source)
        {
            IConfig ircConfig = source.Configs["IRCModule"];
            if (ircConfig != null) {
                string moduleEnabled = ircConfig.GetString("Module", "");
                m_spamDebug = ircConfig.GetBoolean("DebugMode", m_spamDebug);
                m_network[UUID.Zero] = ircConfig.GetString("Network", "");
                m_channel[UUID.Zero] = ircConfig.GetString("Channel", "");
                m_chatToIRCChannel = ircConfig.GetInt("ChatToIRCChannel", m_chatToIRCChannel);

                m_enabled = moduleEnabled == "Parcel";
                m_config = ircConfig;
            }
        }

        public void PostInitialise()
        {
        }

        public void AddRegion(IScene scene)
        {
            if (!m_enabled)
                return;

            m_scene = scene;
            scene.EventManager.OnMakeRootAgent += EventManager_OnMakeRootAgent;
            scene.EventManager.OnMakeChildAgent += EventManager_OnMakeChildAgent;
            scene.EventManager.OnRemovePresence += EventManager_OnRemovePresence;
            scene.EventManager.OnChatFromClient += EventManager_OnChatFromClient;
            scene.EventManager.OnAvatarEnteringNewParcel += EventManager_OnAvatarEnteringNewParcel;
        }

        public void RegionLoaded(IScene scene)
        {
        }

        public void RemoveRegion(IScene scene)
        {
            if (!m_enabled)
                return;

            scene.EventManager.OnMakeRootAgent -= EventManager_OnMakeRootAgent;
            scene.EventManager.OnMakeChildAgent -= EventManager_OnMakeChildAgent;
            scene.EventManager.OnRemovePresence -= EventManager_OnRemovePresence;
            scene.EventManager.OnChatFromClient -= EventManager_OnChatFromClient;
            scene.EventManager.OnAvatarEnteringNewParcel -= EventManager_OnAvatarEnteringNewParcel;
        }

        public void Close()
        {
        }

        public string Name {
            get { return "IRCParcelService"; }
        }

        public Type ReplaceableInterface {
            get { return null; }
        }

        bool TryGetNetwork(IScenePresence avatar, out string network)
        {
            network = "";
            if (avatar.CurrentParcel == null)
                return false;

            if (m_network.TryGetValue(avatar.CurrentParcel.LandData.GlobalID, out network)) {
                string channel = "";
                if (m_channel.TryGetValue(avatar.CurrentParcel.LandData.GlobalID, out channel))
                    return true;

                channel = m_config.GetString(avatar.CurrentParcel.LandData.Name.Replace(' ', '_') + "_Channel", "");
                if (channel == "") {
                    if (m_channel.TryGetValue(UUID.Zero, out channel) && channel != "") {
                    } else {
                        return false;
                    }
                }

                m_channel[avatar.CurrentParcel.LandData.GlobalID] = channel;
                return true;

            } else {
                network = m_config.GetString(avatar.CurrentParcel.LandData.Name.Replace(' ', '_') + "_Network", "");
                if (network == "") {
                    if (m_network.TryGetValue(UUID.Zero, out network) && network != "") {
                    } else {
                        return false;
                    }
                }
                m_network[avatar.CurrentParcel.LandData.GlobalID] = network;
                string channel = m_config.GetString(avatar.CurrentParcel.LandData.Name.Replace(' ', '_') + "_Channel", "");
                if (channel == "") {
                    if (m_channel.TryGetValue(UUID.Zero, out channel) && channel != "") {
                    } else {
                        return false;
                    }
                }

                m_channel[avatar.CurrentParcel.LandData.GlobalID] = channel;
                return true;
            }
        }

        void EventManager_OnAvatarEnteringNewParcel(IScenePresence presence, ILandObject oldParcel)
        {
            string network;
            if (TryGetNetwork(presence, out network)) {
                if (clients.ContainsKey(presence.UUID)) {
                    Client client = clients[presence.UUID];
                    if (client.Connection.Address == network)
                        SwitchChannels(presence, oldParcel, client);

                } else {
                    CloseClient(presence);
                    CreateIRCConnection(presence, network);
                }
            } else {
                CloseClient(presence);
            }
        }

        void SwitchChannels(IScenePresence presence, ILandObject oldParcel, Client client)
        {
            string channel;
            string oldchannel;
            m_channel.TryGetValue(presence.CurrentParcel.LandData.GlobalID, out channel);
            m_channel.TryGetValue(oldParcel.LandData.GlobalID, out oldchannel);
            JoinChannel(client, channel, presence);
            client.SendPart(oldchannel);
        }

        void EventManager_OnMakeRootAgent(IScenePresence presence)
        {
            presence.ControllingClient.OnPreSendInstantMessage += ControllingClient_OnInstantMessage;
        }

        void EventManager_OnRemovePresence(IScenePresence presence)
        {
            CloseClient(presence);
            presence.ControllingClient.OnPreSendInstantMessage -= ControllingClient_OnInstantMessage;
        }

        void EventManager_OnMakeChildAgent(IScenePresence presence, GridRegion destination)
        {
            CloseClient(presence);
            presence.ControllingClient.OnPreSendInstantMessage -= ControllingClient_OnInstantMessage;
        }

        Dictionary<string, UUID> m_ircUsersToFakeUUIDs = new Dictionary<string, UUID>();
        bool ControllingClient_OnInstantMessage(IClientAPI remoteclient, GridInstantMessage im)
        {
            foreach (KeyValuePair<string, UUID> fakeID in m_ircUsersToFakeUUIDs) {
                if (im.ToAgentID == fakeID.Value) {
                    Client client;
                    if (TryGetClient(remoteclient.AgentId, out client)) {
                        User user = client.Peers.Find(delegate (User u) {
                            if (u.UserName == fakeID.Key)
                                return true;

                            return false;
                        });

                        if (user != null && im.Message != "" && im.Dialog == (byte)InstantMessageDialog.MessageFromAgent) {
                            client.SendChat(im.Message, user.Nick);
                        }

                        return true;
                    }
                }
            }
            return false;
        }

        void Chatting(object sender, IrcMessageEventArgs<TextMessage> e, IScenePresence presence)
        {
            IChatModule chatModule = m_scene.RequestModuleInterface<IChatModule>();
            if (chatModule != null) {
                if (e.Message.Targets.Count > 0 && e.Message.Targets[0] == clients[presence.UUID].User.Nick) {
                    UUID fakeUUID;
                    if (!m_ircUsersToFakeUUIDs.TryGetValue(e.Message.Sender.UserName, out fakeUUID)) {
                        fakeUUID = UUID.Random();
                        m_ircUsersToFakeUUIDs[e.Message.Sender.UserName] = fakeUUID;
                    }

                    presence.ControllingClient.SendInstantMessage(new GridInstantMessage() {
                        FromAgentID = fakeUUID,
                        FromAgentName = e.Message.Sender.Nick,
                        ToAgentID = presence.UUID,
                        Dialog = (byte)InstantMessageDialog.MessageFromAgent,
                        Message = e.Message.Text,
                        FromGroup = false,
                        SessionID = UUID.Zero,
                        Offline = 0,
                        BinaryBucket = new byte[0],
                        Timestamp = (uint)Util.UnixTimeSinceEpoch()
                    });
                } else
                    chatModule.TrySendChatMessage(presence, presence.AbsolutePosition, UUID.Zero,
                        e.Message.Targets[0] + " - " + e.Message.Sender.Nick, ChatTypeEnum.Say, e.Message.Text, ChatSourceType.Agent, 20);
            }
        }

        void EventManager_OnChatFromClient(IClientAPI sender, OSChatMessage chat)
        {
            if ((chat.Message == "") || (sender == null) || (chat.Channel != m_chatToIRCChannel))
                return;

            Client client;
            if (TryGetClient(sender.AgentId, out client)) {
                Util.FireAndForget(delegate (object o) {
                    IScenePresence sp = sender.Scene.GetScenePresence(sender.AgentId);
                    if (sp != null) {
                        string channel;
                        if (m_channel.TryGetValue(sp.CurrentParcel.LandData.GlobalID, out channel)) {
                            client.SendChat("(grid: " +
                                MainServer.Instance.ServerURI.Remove(0, 7) + ") - " +
                                chat.Message, channel);
                        }
                    }
                });
            }
        }

        bool TryGetClient(UUID uUID, out Client client)
        {
            return clients.TryGetValue(uUID, out client);
        }

        void CreateIRCConnection(IScenePresence presence, string network)
        {
            // Create a new client to the given address with the given nick
            Client client = new Client(network, presence.Name.Replace(' ', '_'));
            Ident.Service.User = client.User;
            HookUpClientEvents(presence, client);
            client.EnableAutoIdent = false;
            client.Connection.Connect();
            clients[presence.UUID] = client;
        }

        void HookUpClientEvents(IScenePresence sp, Client client)
        {
            // Once I'm welcomed, I can start joining channels
            client.Messages.Welcome += delegate (object sender, IrcMessageEventArgs<WelcomeMessage> e) {
                Welcomed(sender, e, client, sp);
            };

            // People are chatting, pay attention so I can be a lame echobot :)
            client.Messages.Chat += delegate (object sender, IrcMessageEventArgs<TextMessage> e) {
                Chatting(sender, e, sp);
            };

            client.Messages.TimeRequest += delegate (object sender, IrcMessageEventArgs<TimeRequestMessage> e) {
                TimeRequested(sender, e, client);
            };

            client.DataReceived += DataGot;
            client.DataSent += DataSent;

            client.Connection.Disconnected += LogDisconnected;
        }

        void CloseClient(IScenePresence sp)
        {
            if (clients.ContainsKey(sp.UUID)) {
                Client client = clients[sp.UUID];
                clients.Remove(sp.UUID);
                Util.FireAndForget(delegate (object o) {
                    client.SendQuit("Left the region");
                });
            }
        }

        void LogDisconnected(object sender, ConnectionDataEventArgs e)
        {
            if (m_spamDebug) {
                string data = "*** Disconnected: " + e.Data;
                MainConsole.Instance.Warn("[RegionIRC]: " + data);
            }
        }

        void DataGot(object sender, ConnectionDataEventArgs e)
        {
            if (m_spamDebug) {
                string data = "*** Got: " + e.Data;
                MainConsole.Instance.Warn("[RegionIRC]: " + data);
            }
        }

        void DataSent(object sender, ConnectionDataEventArgs e)
        {
            if (m_spamDebug) {
                string data = "*** Sent: " + e.Data;
                MainConsole.Instance.Warn("[RegionIRC]: " + data);
            }
        }

        void TimeRequested(object sender, IrcMessageEventArgs<TimeRequestMessage> e, Client client)
        {
            TimeReplyMessage reply = new TimeReplyMessage();
            reply.CurrentTime = DateTime.Now.ToLongTimeString();
            reply.Target = e.Message.Sender.Nick;
            client.Send(reply);
        }

        void Welcomed(object sender, IrcMessageEventArgs<WelcomeMessage> e, Client client, IScenePresence sp)
        {
            string channel;
            if (m_channel.TryGetValue(sp.CurrentParcel.LandData.GlobalID, out channel))
                JoinChannel(client, channel, sp);
        }

        static void JoinChannel(Client client, string channel, IScenePresence presence)
        {
            client.SendJoin(channel);
            IChatModule chatModule = presence.Scene.RequestModuleInterface<IChatModule>();
            if (chatModule != null) {
                chatModule.TrySendChatMessage(presence, presence.AbsolutePosition, UUID.Zero,
                    "System", ChatTypeEnum.Say, "You joined " + channel, ChatSourceType.Agent, 20);
            }
        }
    }
}
