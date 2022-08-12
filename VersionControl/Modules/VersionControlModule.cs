/*
 * Copyright (c) Contributors, http://whitecore-sim.org/, http://aurora-sim.org
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the WhiteCore-Sim Project nor the
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
using System.Timers;
using Nini.Config;
using WhiteCore.Framework.ConsoleFramework;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.SceneInfo;
using WhiteCore.Framework.Utilities;

namespace WhiteCore.AddOn.VersionControl
{
    public class VersionControlModule : INonSharedRegionModule
    {
        bool m_Enabled;

        // Auto OAR configs
        bool m_autoOAREnabled;
        float m_autoOARTime = 1;    // Time in days
        Timer m_autoOARTimer;
        IScene m_Scene;

        int nextVersion = 1;

        #region INonSharedRegionModule Members

        public string Name {
            get { return "VersionControlModule"; }
        }

        public Type ReplaceableInterface {
            get { return null; }
        }

        public void Initialise(IConfigSource source)
        {
            if (source.Configs["VersionControl"] == null)
                return;
            IConfig config = source.Configs["VersionControl"];
            m_Enabled = config.GetBoolean("Enabled", false);

            // Auto OAR config
            m_autoOAREnabled = config.GetBoolean("AutoVersionEnabled", false);
            m_autoOARTime = config.GetFloat("AutoVersionTime", 1);
        }

        public void Close()
        {
        }

        public void AddRegion(IScene scene)
        {
        }

        public void RemoveRegion(IScene scene)
        {
            m_Scene = null;
        }

        public void RegionLoaded(IScene scene)
        {
            if (!m_Enabled)
                return;

            if (m_autoOAREnabled) {
                m_autoOARTimer = new Timer(m_autoOARTime * 1000 * 60 * 60 * 24); // Time in days
                m_autoOARTimer.Elapsed += SaveOAR;
                m_autoOARTimer.Enabled = true;
                m_Scene = scene;
            }

            MainConsole.Instance.Commands.AddCommand(
                "save version",
                "save version <description>",
                "Saves a region OAR with incremented version details.",
                SaveVersionCmd, true, false);
        }

        #endregion

        void SaveOAR(object sender, ElapsedEventArgs e)
        {
            SaveVersion("AutomaticBackup");
        }

        protected void SaveVersionCmd(IScene scene, string[] cmdparams)
        {
            string description = "";

            if (cmdparams.Length < 3) {
                description = MainConsole.Instance.Prompt("Enter a description for this version", description);
                if (description == "")
                    return;
            } else
                description = Util.CombineParams(cmdparams, 2);         // in case of spaces 

            cmdparams[0] = "";
            cmdparams[1] = "";
            SaveVersion(description);
        }

        public void SaveVersion(string Description)
        {
            string tag = "";
            tag += "Region." + m_Scene.RegionInfo.RegionName;
            tag += ".Desc." + Description;
            tag += ".Version." + nextVersion;
            tag += ".Date." + DateTime.Now.Year + "." + DateTime.Now.Month + "." + DateTime.Now.Day + "." +
            DateTime.Now.Hour;
            nextVersion++;
            MainConsole.Instance.RunCommand("save oar " + tag + ".vc.oar");
        }
    }
}
