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
    ..with a few more mods by Rowan Deppeler <greythane@gmail.com>
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
using WhiteCore.Framework.DatabaseInterfaces;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.PresenceInfo;
using WhiteCore.Framework.SceneInfo;
using WhiteCore.Framework.Servers;
using WhiteCore.Framework.Services;
using WhiteCore.Framework.Utilities;
using GridRegion = WhiteCore.Framework.Services.GridRegion;

namespace WhiteCore.Addon.IRCChat
{
    public class IRCGroupService : INonSharedRegionModule
    {
        Dictionary<UUID, string> m_network = new Dictionary<UUID, string> ();
        Dictionary<UUID, string> m_channel = new Dictionary<UUID, string> ();
        Dictionary<UUID, string> m_gridName = new Dictionary<UUID, string> ();
        IScene m_scene;
        bool m_spamDebug = false;
        bool m_enabled = false;
        // m_GroupUser;
        Dictionary<UUID, Client> clients = new Dictionary<UUID, Client> ();
        IConfig m_config;

        public void Initialise (IConfigSource source)
        {
            IConfig ircConfig = source.Configs ["IRCModule"];
            if (ircConfig != null) {
                m_enabled = ircConfig.GetBoolean ("GroupsModule", m_enabled);
                m_spamDebug = ircConfig.GetBoolean ("DebugMode", m_spamDebug);
                //m_GroupUser = ircConfig.Get("AvatarID","");
                m_config = ircConfig;
            }
        }

        public void PostInitialise ()
        {
        }

        public void AddRegion (IScene scene)
        {
            if (!m_enabled)
                return;

            m_scene = scene;
            scene.EventManager.OnMakeRootAgent += EventManager_OnMakeRootAgent;
            scene.EventManager.OnMakeChildAgent += EventManager_OnMakeChildAgent;
            scene.EventManager.OnRemovePresence += EventManager_OnRemovePresence;
            scene.EventManager.OnIncomingInstantMessage += EventManager_OnIncomingInstantMessage;
            InitClients ();
        }

        void InitClients ()
        {
            // use this???     IGroupsServicesConnector conn = m_scene.RequestModuleInterface<IGroupsServicesConnector>();

            IGroupsServiceConnector conn = Framework.Utilities.DataManager.RequestPlugin<IGroupsServiceConnector> ();
            if (conn != null) {
                foreach (string s in m_config.GetKeys ()) {
                    if (s.EndsWith ("_Network", StringComparison.Ordinal)) {
                        string networkvalue = m_config.GetString (s);
                        string channelvalue = m_config.GetString (s.Replace ("_Network", "_Channel"));
                        string nickvalue = m_config.GetString (s.Replace ("_Network", "_Nick"));
                        string gridName = m_config.GetString (s.Replace ("_Network", "_GridName"), MainServer.Instance.ServerURI.Remove (0, 7));
                        GroupRecord g = conn.GetGroupRecord (UUID.Zero, UUID.Zero, s.Replace ("_Network", "").Replace ('_', ' '));
                        if (g != null) {
                            m_network [g.GroupID] = networkvalue;
                            m_channel [g.GroupID] = channelvalue;
                            m_gridName [g.GroupID] = gridName;
                            CreateIRCConnection (networkvalue, nickvalue, channelvalue, g.GroupID);
                        }
                    }
                }
            } else {
                MainConsole.Instance.TraceFormat ("[GroupIRC]: Exception initialising clients - Unable to locate GroupServiceConnector");
                m_enabled = false;
            }
        }

        public void RegionLoaded (IScene scene)
        {
        }

        public void RemoveRegion (IScene scene)
        {
            if (!m_enabled)
                return;

            scene.EventManager.OnMakeRootAgent -= EventManager_OnMakeRootAgent;
            scene.EventManager.OnMakeChildAgent -= EventManager_OnMakeChildAgent;
            scene.EventManager.OnRemovePresence -= EventManager_OnRemovePresence;
            scene.EventManager.OnIncomingInstantMessage -= EventManager_OnIncomingInstantMessage;
        }

        public void Close ()
        {
        }

        public string Name {
            get { return "IRCParcelService"; }
        }

        public Type ReplaceableInterface {
            get { return null; }
        }

        void EventManager_OnMakeRootAgent (IScenePresence presence)
        {
            presence.ControllingClient.OnPreSendInstantMessage += ControllingClient_OnPreSendInstantMessage;
        }

        void EventManager_OnRemovePresence (IScenePresence presence)
        {
            presence.ControllingClient.OnPreSendInstantMessage -= ControllingClient_OnPreSendInstantMessage;
        }

        void EventManager_OnMakeChildAgent (IScenePresence presence, GridRegion destination)
        {
            presence.ControllingClient.OnPreSendInstantMessage -= ControllingClient_OnPreSendInstantMessage;
        }

        void EventManager_OnIncomingInstantMessage (GridInstantMessage message)
        {
            ControllingClient_OnPreSendInstantMessage (null, message);
        }

        bool ControllingClient_OnPreSendInstantMessage (IClientAPI remoteclient, GridInstantMessage im)
        {
            string name = remoteclient == null ? im.FromAgentName : remoteclient.Name;
            if (im.Dialog == (byte)InstantMessageDialog.SessionSend) {
                Client client;
                if (clients.TryGetValue (im.SessionID, out client)) {
                    try {
                        if (client.Connection.Status == ConnectionStatus.Connected)
                            client.SendChat ("(grid:" + m_gridName [im.SessionID] + ") " + name + ": " + im.Message, m_channel [im.SessionID]);
                    } catch (Exception ex) {
                        MainConsole.Instance.TraceFormat ("[GroupIRC]: Exception sending chat message ({0})", ex);
                    }
                }
            }
            return false;
        }

        /*  previous stuff... not correct??
                private void chatting (Object sender, IrcMessageEventArgs<TextMessage> e, UUID groupID)
                {
                    IGroupsServicesConnector conn = m_scene.RequestModuleInterface<IGroupsServicesConnector>();
                    IGroupsMessagingModule gMessaging = m_scene.RequestModuleInterface<IGroupsMessagingModule>();

                    gMessaging.EnsureGroupChatIsStarted(groupID);
                    gMessaging.SendMessageToGroup(new GridInstantMessage(null, UUID.Random(), e.Message.Sender.Nick,
                        UUID.Zero, (byte)InstantMessageDialog.SessionSend, e.Message.Text, false, Vector3.Zero), groupID);
                }
        */
        void chatting (object sender, IrcMessageEventArgs<TextMessage> e, UUID groupID)
        {
            IInstantMessagingService gMessaging = m_scene.RequestModuleInterface<IInstantMessagingService> ();

            if (gMessaging != null) {
                gMessaging.EnsureSessionIsStarted (groupID);
                gMessaging.SendChatToSession (UUID.Zero, new GridInstantMessage () {
                    FromAgentID = (UUID)"4fec5721-6980-40ca-815c-aba0264b175a",
                    //FromAgentID = UUID.Random(),
                    //FromAgentName = "VN_Irc",
                    FromAgentName = e.Message.Sender.Nick,
                    //ToAgentID = groupID,
                    ToAgentID = UUID.Zero,
                    Dialog = (byte)InstantMessageDialog.SessionSend,
                    Message = e.Message.Text,
                    FromGroup = false,
                    SessionID = UUID.Zero,
                    Offline = 0,
                    BinaryBucket = new byte [0],
                    Timestamp = (uint)Util.UnixTimeSinceEpoch ()
                });


                MainConsole.Instance.InfoFormat ("Sending " + e.Message.Text + " to Group " + groupID + " From " + e.Message.Sender.Nick);
            }

        }

        void CreateIRCConnection (string network, string nick, string channel, UUID groupID)
        {
            // Create a new client to the given address with the given nick
            Client client = new Client (network, nick);
            Ident.Service.User = client.User;
            HookUpClientEvents (channel, groupID, client);
            client.EnableAutoIdent = false;
            client.Connection.Connect ();
            clients [groupID] = client;
        }

        void HookUpClientEvents (string channel, UUID groupID, Client client)
        {
            // Once I'm welcomed, I can start joining channels
            client.Messages.Welcome += delegate (object sender, IrcMessageEventArgs<WelcomeMessage> e) {
                welcomed (sender, e, client, channel);
            };

            // People are chatting, pay attention so I can be a lame echobot :)
            client.Messages.Chat += delegate (object sender, IrcMessageEventArgs<TextMessage> e) {

                UUID mystupiduuid = (UUID)"4fec5721-6980-40ca-815c-aba0264b175a"; // debug only??
                chatting (mystupiduuid, e, groupID);
                MainConsole.Instance.InfoFormat ("got " + groupID + " and " + e.Message.Text + " From: " + sender);
            };

            client.Messages.TimeRequest += delegate (object sender, IrcMessageEventArgs<TimeRequestMessage> e) {
                timeRequested (sender, e, client);
            };

            client.DataReceived += dataGot;
            client.DataSent += dataSent;

            client.Connection.Disconnected += logDisconnected;
        }

        void CloseClient (IScenePresence sp)
        {
            if (clients.ContainsKey (sp.UUID)) {
                Client client = clients [sp.UUID];
                clients.Remove (sp.UUID);
                Util.FireAndForget (delegate (object o) {
                    client.SendQuit ("Left the region");
                });
            }
        }

        void logDisconnected (object sender, ConnectionDataEventArgs e)
        {
            if (m_spamDebug) {
                string data = "*** Disconnected: " + e.Data;
                MainConsole.Instance.Warn ("[GroupIRC]: " + data);
            }
        }

        void dataGot (object sender, ConnectionDataEventArgs e)
        {
            if (m_spamDebug) {
                string data = "*** Got: " + e.Data;
                MainConsole.Instance.Warn ("[GroupIRC]: " + data);
            }
        }

        void dataSent (object sender, ConnectionDataEventArgs e)
        {
            if (m_spamDebug) {
                string data = "*** Sent: " + e.Data;
                MainConsole.Instance.Warn ("[GroupIRC]: " + data);
            }
        }

        void timeRequested (object sender, IrcMessageEventArgs<TimeRequestMessage> e, Client client)
        {
            TimeReplyMessage reply = new TimeReplyMessage ();
            reply.CurrentTime = DateTime.Now.ToLongTimeString ();
            reply.Target = e.Message.Sender.Nick;
            client.Send (reply);
        }

        void welcomed (object sender, IrcMessageEventArgs<WelcomeMessage> e, Client client, string channel)
        {
            client.SendJoin (channel);
        }
    }
}
