/*
 * Copyright (c) Contributors, http://whitecore-sim.org/, http://aurora-sim.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Aurora-Sim Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Nini.Config;
using Nwc.XmlRpc;
using OpenMetaverse;
using WhiteCore.Framework.ClientInterfaces;
using WhiteCore.Framework.ConsoleFramework;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.PresenceInfo;
using WhiteCore.Framework.SceneInfo;
using WhiteCore.Framework.Servers;
using WhiteCore.Framework.Utilities;
using WhiteCore.Modules.Chat;


namespace WhiteCore.Addon.Concierge
{
    public class ConciergeModule : ChatModule
    {
        const int DEBUG_CHANNEL = 2147483647;

        // List<IScene> m_scenes = new List<IScene>();
        List<IScene> m_conciergedScenes = new List<IScene> ();

        bool m_replacingChatModule;

        IConfigSource m_config;

        string m_whoami = "jeeves";
        Regex m_regions;
        string m_welcome = "This is your concierge, Hello {0} and welcome to {1}";
        string m_welcomeinfo;       // additional information file template
        int m_conciergeChannel = 42;
        string m_announceEntering = "{0} enters {1} (now {2} visitors in this region)";
        string m_announceLeaving = "{0} leaves {1} (back to {2} visitors in this region)";
        string m_xmlRpcPassword = string.Empty;
        string m_brokerURI = string.Empty;
        int m_brokerUpdateTimeout = 300;

        string m_regionName;
        bool m_enableLog;
        protected TextWriter m_logFile;
        DateTime m_logDate;

        internal object m_syncy = new object ();

        internal bool m_enabled;

        #region ISharedRegionModule Members
        public override void Initialise (IConfigSource config)
        {
            m_config = config;
            var c_config = config.Configs ["Concierge"];

            if (c_config == null) {
                MainConsole.Instance.Debug ("[Concierge]: no config found, plugin disabled");
                return;
            }

            if (!c_config.GetBoolean ("enabled", false)) {
                MainConsole.Instance.Debug ("[Concierge]: plugin disabled by configuration");
                return;
            }

            // take note of concierge channel and of identity
            m_conciergeChannel = c_config.GetInt ("concierge_channel", m_conciergeChannel);
            m_whoami = c_config.GetString ("whoami", "conferencier");
            m_welcome = c_config.GetString ("welcome", m_welcome);
            m_welcomeinfo = c_config.GetString ("welcomeinfo", m_welcomeinfo);
            m_announceEntering = c_config.GetString ("announce_entering", m_announceEntering);
            m_announceLeaving = c_config.GetString ("announce_leaving", m_announceLeaving);
            m_xmlRpcPassword = c_config.GetString ("password", m_xmlRpcPassword);
            m_brokerURI = c_config.GetString ("broker", m_brokerURI);
            m_brokerUpdateTimeout = c_config.GetInt ("broker_timeout", m_brokerUpdateTimeout);

            m_enabled = c_config.GetBoolean ("enabled", false);
            m_enableLog = c_config.GetBoolean ("enable_log", false);

            CheckForChatReplacement (config);

            MainConsole.Instance.InfoFormat ("[Concierge] reporting as \"{0}\" to our users", m_whoami);

            // calculate regions Regex
            if (m_regions == null) {
                string regions = c_config.GetString ("regions", string.Empty);
                if (!string.IsNullOrEmpty (regions)) {
                    m_regions = new Regex (@regions, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                }
            }

            if (m_enabled)
                AddConsoleCommands ();
        }

        void CheckForChatReplacement (IConfigSource config)
        {
            // check whether the main ChatModule has been disabled: if yes,
            // then we'll "stand in"
            m_replacingChatModule = false;

            if (config.Configs ["Chat"] != null) {
                // if Chat module has not been configured it's
                // enabled by default, so don't replace it unless it is disabled.
                m_replacingChatModule = !config.Configs ["Chat"].GetBoolean ("enabled", true);
            }
            MainConsole.Instance.InfoFormat ("[Concierge] {0} ChatModule", m_replacingChatModule ? "replacing" : "not replacing");
        }

        /// <summary>
        /// Adds the console commands.
        /// </summary>
        void AddConsoleCommands ()
        {
            if (MainConsole.Instance != null) {
                MainConsole.Instance.Commands.AddCommand (
                    "conceierge enable",
                    "concierge enable",
                    "Announce avatar entries and exits to other users in the region",
                    HandleConciergeEnable,
                    false, true);

                MainConsole.Instance.Commands.AddCommand (
                    "concierge disable",
                    "concierge disable [all]",
                    "Stop announcing avatar entries and exits. Optionally, disable all concierge activity",
                    HandleConciergeDisable,
                    false, true);

                MainConsole.Instance.Commands.AddCommand (
                    "concierge whoami",
                    "concierge whoami [name]",
                    "The name that the concierge uses to identify messages.",
                    HandleConciergeWhoAmI,
                    false, true);

                MainConsole.Instance.Commands.AddCommand (
                    "concierge welcome",
                    "concierge welcome [welcome string]",
                    "Set the welcome template used when the concierge announces avatar entries.",
                    HandleConciergeWelcome,
                    false, true);
            }

        }

        public override void AddRegion (IScene scene)
        {
            if (!m_enabled) return;

            m_regionName = scene.RegionInfo.RegionName;
            if (m_regions == null || m_regions.IsMatch (scene.RegionInfo.RegionName)) {
                // This one is one our list...
                //TODO: Replace with API call that makes sense
                MainServer.Instance.AddXmlRPCHandler ("concierge_update_welcome", XmlRpcUpdateWelcomeMethod);

                lock (m_syncy) {
                    // m_scenes.Add (scene);                   // TODO: this is probably not really required (used in XMLRPC call)
                    m_conciergedScenes.Add (scene);

                    SubscribeRegionEvents (scene);
                }

                if (m_enableLog)
                    OpenLog ();

            } else
                m_enabled = false;          // disable for this region
        }

        public override void RemoveRegion (IScene scene)
        {
            if (!m_enabled) return;

            UnsubscribeRegionEvents (scene);
        }

        void SubscribeRegionEvents (IScene scene)
        {
            // subscribe to NewClient events
            scene.EventManager.OnNewClient += OnNewClient;
            scene.EventManager.OnClosingClient += OnClientLoggedOut;

            // subscribe to *Chat events
            scene.EventManager.OnChatFromWorld += OnChatFromWorld;
            if (!m_replacingChatModule)
                scene.EventManager.OnChatFromClient += OnChatFromClient;
            scene.EventManager.OnChatBroadcast += OnChatBroadcast;

            // subscribe to agent change events
            scene.EventManager.OnMakeRootAgent += OnMakeRootAgent;
            scene.EventManager.OnMakeChildAgent += OnMakeChildAgent;

            MainConsole.Instance.InfoFormat ("[Concierge]: initialized for {0}", scene.RegionInfo.RegionName);
        }

        void UnsubscribeRegionEvents (IScene scene)
        {
            lock (m_syncy) {
                // unsubscribe from NewClient events
                scene.EventManager.OnNewClient -= OnNewClient;
                scene.EventManager.OnClosingClient -= OnClientLoggedOut;

                // unsubscribe from *Chat events
                scene.EventManager.OnChatFromWorld -= OnChatFromWorld;
                if (!m_replacingChatModule)
                    scene.EventManager.OnChatFromClient -= OnChatFromClient;
                scene.EventManager.OnChatBroadcast -= OnChatBroadcast;

                // unsubscribe from agent change events
                scene.EventManager.OnMakeRootAgent -= OnMakeRootAgent;
                scene.EventManager.OnMakeChildAgent -= OnMakeChildAgent;

                // m_scenes.Remove(scene);
                m_conciergedScenes.Remove (scene);
            }
            MainConsole.Instance.InfoFormat ("[Concierge]: removed {0}", scene.RegionInfo.RegionName);
        }

        //public override void PostInitialise()
        // {
        // }

        public override void Close ()
        {
            if (m_logFile != null)
                m_logFile.Close ();
        }

        new public Type ReplaceableInterface {
            get { return null; }
        }

        public override string Name {
            get { return "ConciergeModule"; }
        }
        #endregion

        #region ISimChat Members
        protected void OpenLog ()
        {
            if (m_logFile == null) {
                var logtime = DateTime.Now.AddMinutes (5);          // just a bit of leeway for rotation
                string timestamp = logtime.ToString ("yyyyMMdd");

                var logpath = MainConsole.Instance.LogPath;
                var fName = logpath + "concierge_chat_" + m_regionName + "_" + timestamp + ".log";
                m_logFile = StreamWriter.Synchronized (new StreamWriter (fName, true));
                m_logDate = logtime.Date;
            }
        }

        protected void RotateLog ()
        {
            m_logFile.Close ();      // close the current log
            OpenLog ();              // start a new one
        }

        protected override void OnChatBroadcast (object sender, OSChatMessage c)
        {
            if (m_replacingChatModule) {
                // distribute chat message to each and every avatar in
                // the region
                base.OnChatBroadcast (sender, c);
            }

            // capture chat if required
            if (m_logFile != null) {
                if (DateTime.Now.Date != m_logDate)
                    RotateLog ();

                m_logFile.WriteLine (c.From + ": " + c.Message);
                m_logFile.Flush ();
            }

            return;
        }

        protected override void OnChatFromClient (IClientAPI sender, OSChatMessage c)
        {
            if (m_replacingChatModule) {
                // replacing ChatModule: need to redistribute
                // ChatFromClient to interested subscribers
                c = FixPositionOfChatMessage (c);

                IScene scene = c.Scene;
                scene.EventManager.TriggerOnChatFromClient (sender, c);

                if (m_conciergedScenes.Contains (c.Scene)) {
                    // when we are replacing ChatModule, we treat
                    // OnChatFromClient like OnChatBroadcast for
                    // concierged regions, effectively extending the
                    // range of chat to cover the whole
                    // region. however, we don't do this for whisper
                    // (got to have some privacy)
                    if (c.Type != ChatTypeEnum.Whisper) {
                        base.OnChatBroadcast (sender, c);
                        return;
                    }
                }

                // redistribution will be done by base class
                base.OnChatFromClient (sender, c);
            }

            // capture chat if required
            if (m_logFile != null) {
                if (DateTime.Now.Date != m_logDate)
                    RotateLog ();

                m_logFile.WriteLine (c.From + ": " + c.Message);
                m_logFile.Flush ();
            }

            return;
        }

        public override void OnChatFromWorld (object sender, OSChatMessage c)
        {
            if (m_replacingChatModule) {
                if (m_conciergedScenes.Contains (c.Scene)) {
                    // when we are replacing ChatModule, we treat
                    // OnChatFromClient like OnChatBroadcast for
                    // concierged regions, effectively extending the
                    // range of chat to cover the whole
                    // region. however, we don't do this for whisper
                    // (got to have some privacy)
                    if (c.Type != ChatTypeEnum.Whisper) {
                        base.OnChatBroadcast (sender, c);
                        return;
                    }
                }

                base.OnChatFromWorld (sender, c);
            }
            return;
        }
        #endregion


        public override void OnNewClient (IClientAPI client)
        {
            client.OnLogout += OnClientLoggedOut;

            if (m_replacingChatModule)
                client.OnChatFromClient += OnChatFromClient;
        }

        public void OnClientLoggedOut (IClientAPI client)
        {
            if (m_conciergedScenes.Contains (client.Scene)) {
                IScene scene = client.Scene;
                IEntityCountModule entityCountModule = scene.RequestModuleInterface<IEntityCountModule> ();

                if (entityCountModule != null) {
                    MainConsole.Instance.DebugFormat ("[Concierge]: {0} logs off from {1}", client.Name, scene.RegionInfo.RegionName);
                    AnnounceToAgentsRegion (scene, string.Format (m_announceLeaving, client.Name, scene.RegionInfo.RegionName, entityCountModule.RootAgents));
                    UpdateBroker (scene);
                }
            }

            if (m_replacingChatModule)
                client.OnChatFromClient -= OnChatFromClient;

            client.OnLogout -= OnClientLoggedOut;
            client.OnConnectionClosed -= OnClientLoggedOut;
        }

        public void OnMakeRootAgent (IScenePresence agent)
        {
            if (m_conciergedScenes.Contains (agent.Scene)) {
                IScene scene = agent.Scene;
                MainConsole.Instance.DebugFormat ("[Concierge]: {0} enters {1}", agent.Name, scene.RegionInfo.RegionName);
                WelcomeAvatar (agent, scene);
                IEntityCountModule entityCountModule = scene.RequestModuleInterface<IEntityCountModule> ();
                if (entityCountModule != null) {
                    AnnounceToAgentsRegion (scene, string.Format (m_announceEntering, agent.Name,
                                                                scene.RegionInfo.RegionName, entityCountModule.RootAgents));
                    UpdateBroker (scene);
                }
            }
        }


        public void OnMakeChildAgent (IScenePresence presence, Framework.Services.GridRegion destination)
        {
            if (m_conciergedScenes.Contains (presence.Scene)) {
                IScene scene = presence.Scene;
                MainConsole.Instance.DebugFormat ("[Concierge]: {0} leaves {1}", presence.Name, scene.RegionInfo.RegionName);
                IEntityCountModule entityCountModule = scene.RequestModuleInterface<IEntityCountModule> ();
                if (entityCountModule != null) {
                    AnnounceToAgentsRegion (scene, string.Format (m_announceLeaving, presence.Name,
                                                               scene.RegionInfo.RegionName, entityCountModule.RootAgents));
                    UpdateBroker (scene);
                }
            }
        }

        internal class BrokerState
        {
            public string Uri;
            public string Payload;
            public HttpWebRequest Poster;
            public Timer Timer;

            public BrokerState (string uri, string payload, HttpWebRequest poster)
            {
                Uri = uri;
                Payload = payload;
                Poster = poster;
            }
        }

        protected void UpdateBroker (IScene scene)
        {
            if (string.IsNullOrEmpty (m_brokerURI))
                return;

            string uri = string.Format (m_brokerURI, scene.RegionInfo.RegionName, scene.RegionInfo.RegionID);

            // create XML sniplet
            StringBuilder list = new StringBuilder ();
            IEntityCountModule entityCountModule = scene.RequestModuleInterface<IEntityCountModule> ();
            if (entityCountModule != null) {
                list.Append (string.Format ("<avatars count=\"{0}\" region_name=\"{1}\" region_uuid=\"{2}\" timestamp=\"{3}\">\n",
                                         entityCountModule.RootAgents, scene.RegionInfo.RegionName,
                                         scene.RegionInfo.RegionID,
                                         DateTime.UtcNow.ToString ("s")));
            }
            scene.ForEachScenePresence (delegate (IScenePresence sp) {
                if (!sp.IsChildAgent) {
                    list.Append (string.Format ("    <avatar name=\"{0}\" uuid=\"{1}\" />\n", sp.Name, sp.UUID));
                    list.Append ("</avatars>");
                }
            });
            string payload = list.ToString ();

            // post via REST to broker
            HttpWebRequest updatePost = WebRequest.Create (uri) as HttpWebRequest;
            updatePost.Method = "POST";
            updatePost.ContentType = "text/xml";
            updatePost.ContentLength = payload.Length;
            updatePost.UserAgent = "WhiteCore.Concierge";


            BrokerState bs = new BrokerState (uri, payload, updatePost);
            bs.Timer = new Timer (delegate (object state) {
                BrokerState b = state as BrokerState;
                b.Poster.Abort ();
                b.Timer.Dispose ();
                MainConsole.Instance.Debug ("[Concierge]: async broker POST abort due to timeout");
            }, bs, m_brokerUpdateTimeout * 1000, Timeout.Infinite);

            try {
                updatePost.BeginGetRequestStream (UpdateBrokerSend, bs);
                MainConsole.Instance.DebugFormat ("[Concierge] async broker POST to {0} started", uri);
            } catch (WebException we) {
                MainConsole.Instance.ErrorFormat ("[Concierge] async broker POST to {0} failed: {1}", uri, we.Status);
            }
        }

        void UpdateBrokerSend (IAsyncResult result)
        {
            BrokerState bs = null;
            try {
                bs = result.AsyncState as BrokerState;
                string payload = bs.Payload;
                HttpWebRequest updatePost = bs.Poster;

                using (StreamWriter payloadStream = new StreamWriter (updatePost.EndGetRequestStream (result))) {
                    payloadStream.Write (payload);
                    payloadStream.Close ();
                }
                updatePost.BeginGetResponse (UpdateBrokerDone, bs);
            } catch (WebException we) {
                MainConsole.Instance.DebugFormat ("[Concierge]: async broker POST to {0} failed: {1}", bs.Uri, we.Status);
            } catch (Exception) {
                MainConsole.Instance.DebugFormat ("[Concierge]: async broker POST to {0} failed", bs.Uri);
            }
        }

        void UpdateBrokerDone (IAsyncResult result)
        {
            BrokerState bs = null;
            try {
                bs = result.AsyncState as BrokerState;
                HttpWebRequest updatePost = bs.Poster;
                HttpWebResponse response = (HttpWebResponse)updatePost.EndGetResponse (result);
                MainConsole.Instance.DebugFormat ("[Concierge] broker update: status {0}", response.StatusCode);
                response.Close ();
                bs.Timer.Dispose ();
            } catch (WebException we) {
                MainConsole.Instance.ErrorFormat ("[Concierge] broker update to {0} failed with status {1}", bs.Uri, we.Status);
                if (null != we.Response) {
                    using (HttpWebResponse resp = we.Response as HttpWebResponse) {
                        MainConsole.Instance.ErrorFormat ("[Concierge] response from {0} status code: {1}", bs.Uri, resp.StatusCode);
                        MainConsole.Instance.ErrorFormat ("[Concierge] response from {0} status desc: {1}", bs.Uri, resp.StatusDescription);
                        MainConsole.Instance.ErrorFormat ("[Concierge] response from {0} server:      {1}", bs.Uri, resp.Server);

                        if (resp.ContentLength > 0) {
                            StreamReader content = new StreamReader (resp.GetResponseStream ());
                            MainConsole.Instance.ErrorFormat ("[Concierge] response from {0} content:     {1}", bs.Uri, content.ReadToEnd ());
                            content.Close ();
                        }
                    }
                }
            }
        }

        protected void WelcomeAvatar (IScenePresence agent, IScene scene)
        {
            if (!string.IsNullOrEmpty (m_welcome))
                AnnounceToAgent (agent, string.Format (m_welcome, agent.Name, scene.RegionInfo.RegionName, m_whoami));

            // welcome mechanics: check whether we have a welcomes
            // directory set and wether there is a region specific
            // welcome file there: if yes, send it to the agent
            if (!string.IsNullOrEmpty (m_welcomeinfo)) {
                string [] welcomes = {
                        Path.Combine(m_welcomeinfo, agent.Scene.RegionInfo.RegionName),
                        Path.Combine(m_welcomeinfo, "DEFAULT")
                };

                foreach (string welcome in welcomes) {
                    if (File.Exists (welcome)) {
                        try {
                            string [] welcomeLines = File.ReadAllLines (welcome);
                            foreach (string l in welcomeLines) {
                                AnnounceToAgent (agent, string.Format (l, agent.Name, scene.RegionInfo.RegionName, m_whoami));
                            }
                        } catch (IOException ioe) {
                            MainConsole.Instance.ErrorFormat ("[Concierge]: run into trouble reading welcome file {0} for region {1} for avatar {2}: {3}",
                                             welcome, scene.RegionInfo.RegionName, agent.Name, ioe);
                        } catch (FormatException fe) {
                            MainConsole.Instance.ErrorFormat ("[Concierge]: welcome file {0} is malformed: {1}", welcome, fe);
                        }
                    }
                    return;
                }
                MainConsole.Instance.DebugFormat ("[Concierge]: no welcome message for region {0}", scene.RegionInfo.RegionName);
            }
        }

        static Vector3 PosOfGod = new Vector3 (128, 128, 9999);

        // protected void AnnounceToAgentsRegion(Scene scene, string msg)
        // {
        //     ScenePresence agent = null;
        //     if ((client.Scene is Scene) && (client.Scene as Scene).TryGetScenePresence(client.AgentId, out agent)) 
        //         AnnounceToAgentsRegion(agent, msg);
        //     else
        //         m_log.DebugFormat("[Concierge]: could not find an agent for client {0}", client.Name);
        // }

        protected void AnnounceToAgentsRegion (IScene scene, string msg)
        {
            OSChatMessage c = new OSChatMessage ();
            c.Message = msg;
            c.Type = ChatTypeEnum.Say;
            c.Channel = 0;
            c.Position = PosOfGod;
            c.From = m_whoami;
            c.Sender = null;
            c.SenderUUID = UUID.Zero;
            c.Scene = scene;

            scene.EventManager.TriggerOnChatBroadcast (this, c);
        }

        protected void AnnounceToAgent (IScenePresence agent, string msg)
        {
            OSChatMessage c = new OSChatMessage ();
            c.Message = msg;
            c.Type = ChatTypeEnum.Say;
            c.Channel = 0;
            c.Position = PosOfGod;
            c.From = m_whoami;
            c.Sender = null;
            c.SenderUUID = UUID.Zero;
            c.Scene = agent.Scene;

            agent.ControllingClient.SendChatMessage (msg, (byte)ChatTypeEnum.Say, PosOfGod, m_whoami, UUID.Zero,
                                                    (byte)ChatSourceType.Object, (byte)ChatAudibleLevel.Fully);
        }

        static void CheckStringParameters (XmlRpcRequest request, string [] param)
        {
            Hashtable requestData = (Hashtable)request.Params [0];
            foreach (string p in param) {
                if (!requestData.Contains (p))
                    throw new Exception (string.Format ("missing string parameter {0}", p));
                if (string.IsNullOrEmpty ((string)requestData [p]))
                    throw new Exception (string.Format ("parameter {0} is empty", p));
            }
        }

        public XmlRpcResponse XmlRpcUpdateWelcomeMethod (XmlRpcRequest request, IPEndPoint remoteClient)
        {
            MainConsole.Instance.Info ("[Concierge]: processing UpdateWelcome request");
            XmlRpcResponse response = new XmlRpcResponse ();
            Hashtable responseData = new Hashtable ();

            try {
                Hashtable requestData = (Hashtable)request.Params [0];
                CheckStringParameters (request, new string [] { "password", "region", "welcome" });

                // check password
                if (!string.IsNullOrEmpty (m_xmlRpcPassword) &&
                    (string)requestData ["password"] != m_xmlRpcPassword) {
                    // throw new Exception("wrong password");
                    responseData ["success"] = "false";
                    response.Value = "invalid XMLRPC access";
                    return response;
                }

                if (string.IsNullOrEmpty (m_welcomeinfo)) {
                    //throw new Exception("welcome templates are not enabled, ask your WhiteCore operator to set the \"welcomes\" option in the [Concierge] section of WhiteCore.ini");
                    responseData ["success"] = "false";
                    response.Value = "welcome templates have not been enabled";
                    return response;
                }

                string msg = (string)requestData ["welcome"];
                if (string.IsNullOrEmpty (msg)) {
                    responseData ["success"] = "false";
                    response.Value = "empty parameter 'welcome'";
                    return response;
                }

                string regionName = (string)requestData ["region"];
                IScene scene = m_conciergedScenes.Find (delegate (IScene s) { return s.RegionInfo.RegionName == regionName; });
                if (scene == null) {
                    responseData ["success"] = "false";
                    response.Value = string.Format ("region \"{0}\" is not a concierged region.", regionName);
                    return response;
                }

                /* 
                 IScene scene = m_scenes.Find(delegate(IScene s) { return s.RegionInfo.RegionName == regionName; });
                 if (scene == null) 
                     throw new Exception(string.Format("unknown region \"{0}\"", regionName));
                 if (!m_conciergedScenes.Contains(scene))
                     throw new Exception(string.Format("region \"{0}\" is not a concierged region.", regionName));
                */
                string welcome = Path.Combine (m_welcomeinfo, regionName);
                if (File.Exists (welcome)) {
                    MainConsole.Instance.InfoFormat ("[Concierge]: UpdateWelcome: updating existing template \"{0}\"", welcome);
                    string welcomeBackup = string.Format ("{0}~", welcome);
                    if (File.Exists (welcomeBackup))
                        File.Delete (welcomeBackup);
                    File.Move (welcome, welcomeBackup);
                }
                File.WriteAllText (welcome, msg);

                responseData ["success"] = "true";
                response.Value = responseData;
            } catch (Exception e) {
                MainConsole.Instance.InfoFormat ("[Concierge]: UpdateWelcome failed: {0}", e.Message);

                responseData ["success"] = "false";
                responseData ["error"] = e.Message;

                response.Value = responseData;
            }
            MainConsole.Instance.Debug ("[Concierge]: done processing UpdateWelcome request");
            return response;
        }

        #region Console Commands

        void HandleConciergeEnable (IScene scene, string [] cmd)
        {

            if (!m_enabled)                             // previously disabled so we need to check for chat replacement
                CheckForChatReplacement (m_config);

            m_enabled = true;

            lock (m_syncy) {
                // m_scenes.Add (scene);
                m_conciergedScenes.Add (scene);         // add this region to the concierged ones

                SubscribeRegionEvents (scene);

            }
        }


        void HandleConciergeDisable (IScene scene, string [] cmd)
        {
            bool allregions = true;
            if (cmd.Length > 2)
                allregions = (cmd [2].ToLower () == "all");

            if (allregions)
                m_enabled = false;

            UnsubscribeRegionEvents (scene);
        }

        void HandleConciergeWhoAmI (IScene scene, string [] cmd)
        {
            string newName;
            if (cmd.Length > 2)
                newName = Util.CombineParams (cmd, 2);       // in case of spaces i.e. "The Butler"
            else
                newName = MainConsole.Instance.Prompt ("The name of your concierge?", m_whoami);

            if (newName != "") {
                m_whoami = newName;
                MainConsole.Instance.Info ("[Concierge]: Your concierge's name is " + m_whoami);
            }
        }

        void HandleConciergeWelcome (IScene scene, string [] cmd)
        {
            string newWelcome = m_welcome;
            if (cmd.Length > 2)
                newWelcome = Util.CombineParams (cmd, 2);       // in case of spaces i.e. "The Butler"
            else {
                MainConsole.Instance.CleanInfo ("The welcome string allows substitution for the the following...");
                MainConsole.Instance.CleanInfo ("  {0} will be replaced with the avatar's name");
                MainConsole.Instance.CleanInfo ("  {1} will be replaced with the region name");
                MainConsole.Instance.CleanInfo ("  {2} will be replaced with the concierge name");

                newWelcome = MainConsole.Instance.Prompt ("Concierge welcome string?", newWelcome);
            }

            if (newWelcome != "") {
                m_welcome = newWelcome;
                MainConsole.Instance.Info ("[Concierge]: Your welcome string is " + m_welcome);
            }

        }


        #endregion
    }
}
