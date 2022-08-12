/*
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
using WhiteCore.Framework.Services;
using WhiteCore.Framework.Utilities;
using GridRegion = WhiteCore.Framework.Services.GridRegion;

namespace WhiteCore.Addon.IRCChat
{
    public class IRCRegionService : INonSharedRegionModule
    {
        string gridname = "";
        string m_network = "";
        string m_channel = "";
        IScene m_scene;
        bool m_spamDebug = false;
        bool m_enabled = false;
        int m_chatToIRCChannel = 0;
        Dictionary<UUID, Client> clients = new Dictionary<UUID, Client>();

        public void Initialise(IConfigSource source)
        {
            IConfig ircConfig = source.Configs["IRCModule"];
            if (ircConfig != null) {
                string moduleEnabled = ircConfig.GetString("Module", "");
                m_spamDebug = ircConfig.GetBoolean("DebugMode", m_spamDebug);
                m_network = ircConfig.GetString("Network", m_network);
                m_channel = ircConfig.GetString("Channel", m_channel);
                m_chatToIRCChannel = ircConfig.GetInt("ChatToIRCChannel", m_chatToIRCChannel);

                m_enabled = moduleEnabled == "Region" && m_network != "" && m_channel != "";
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

            IGridInfo gridInfo = scene.RequestModuleInterface<IGridInfo>();
            if (gridInfo != null)
                gridname = gridInfo.GridName;
            else
                gridname = "Unknown";

            MainConsole.Instance.InfoFormat("[IRCModule]: Chat enabled for {0} on {1} to channel {2}",
                scene.RegionInfo.RegionName, m_network, m_channel);

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
        }

        public void Close()
        {
        }

        public string Name {
            get { return "IRCRegionService"; }
        }

        public Type ReplaceableInterface {
            get { return null; }
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

        void EventManager_OnMakeRootAgent(IScenePresence presence)
        {
            CreateIRCConnection(presence);
            presence.ControllingClient.OnPreSendInstantMessage += ControllingClient_OnInstantMessage;
        }

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
                        if (user != null && im.Message != "" && im.Dialog == (byte)InstantMessageDialog.MessageFromAgent)
                            client.SendChat(im.Message, user.Nick);
                        return true;
                    }
                }
            }
            return false;
        }

        void EventManager_OnChatFromClient(IClientAPI sender, OSChatMessage chat)
        {
            if (chat.Message == "" || sender == null || chat.Channel != m_chatToIRCChannel)
                return;

            Client client;
            if (TryGetClient(sender.AgentId, out client)) {
                Util.FireAndForget((object o) => client.SendChat("[" + gridname + "]: " + chat.Message, m_channel));
            }
        }

        bool TryGetClient(UUID uUID, out Client client)
        {
            return clients.TryGetValue(uUID, out client);
        }

        void CreateIRCConnection(IScenePresence presence)
        {
            // Create a new client to the given address with the given nick
            Client client = new Client(m_network, presence.Name.Replace(' ', '_'));
            Ident.Service.User = client.User;
            HookUpClientEvents(client, presence);
            client.EnableAutoIdent = false;
            client.Connection.Connect();
            clients[presence.UUID] = client;
        }

        void HookUpClientEvents(Client client, IScenePresence sp)
        {
            // Once I'm welcomed, I can start joining channels
            client.Messages.Welcome += delegate (object sender, IrcMessageEventArgs<WelcomeMessage> e) {
                Welcomed(sender, e, client);
            };

            // People are chatting, pay attention so I can be a lame echobot :)
            client.Messages.Chat += delegate (object sender, IrcMessageEventArgs<TextMessage> e) {
                Chatting(sender, e, sp);
                //MainConsole.Instance.RunCommand("alert user general "+sender+ " : " +e);
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

        Dictionary<string, UUID> m_ircUsersToFakeUUIDs = new Dictionary<string, UUID>();
        void Chatting(object sender, IrcMessageEventArgs<TextMessage> e, IScenePresence sp)
        {
            IChatModule chatModule = m_scene.RequestModuleInterface<IChatModule>();
            if (chatModule != null) {
                if (e.Message.Targets.Count > 0 && e.Message.Targets[0] == clients[sp.UUID].User.Nick) {
                    UUID fakeUUID;
                    if (!m_ircUsersToFakeUUIDs.TryGetValue(e.Message.Sender.UserName, out fakeUUID)) {
                        fakeUUID = UUID.Random();
                        m_ircUsersToFakeUUIDs[e.Message.Sender.UserName] = fakeUUID;
                    }
                    sp.ControllingClient.SendInstantMessage(new GridInstantMessage() {
                        FromAgentID = fakeUUID,
                        FromAgentName = e.Message.Sender.Nick,
                        ToAgentID = sp.UUID,
                        Dialog = (byte)InstantMessageDialog.MessageFromAgent,
                        Message = e.Message.Text,
                        FromGroup = false,
                        SessionID = UUID.Zero,
                        Offline = 0,
                        BinaryBucket = new byte[0],
                        RegionID = sp.Scene.RegionInfo.RegionID,
                        Timestamp = (uint)Util.UnixTimeSinceEpoch()
                    });
                    /*                   sp.ControllingClient.SendChatMessage(
                                            e.Message.Text, 1, sp.AbsolutePosition, e.Message.Sender.Nick, fakeUUID,
                                            (byte) ChatSourceType.Agent,
                                            (byte) ChatAudibleLevel.Fully);
                    */
                } else
                    chatModule.TrySendChatMessage(sp, sp.AbsolutePosition, UUID.Zero,
                        e.Message.Targets[0] + " - " + e.Message.Sender.Nick, ChatTypeEnum.Say, e.Message.Text, ChatSourceType.Agent, 20);
            }
        }

        void TimeRequested(object sender, IrcMessageEventArgs<TimeRequestMessage> e, Client client)
        {
            TimeReplyMessage reply = new TimeReplyMessage();
            reply.CurrentTime = DateTime.Now.ToLongTimeString();
            reply.Target = e.Message.Sender.Nick;
            client.Send(reply);
        }

        void Welcomed(object sender, IrcMessageEventArgs<WelcomeMessage> e, Client client)
        {
            client.SendJoin(m_channel);
        }
    }
}
