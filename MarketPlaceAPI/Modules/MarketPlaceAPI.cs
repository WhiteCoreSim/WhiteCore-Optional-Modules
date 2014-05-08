/*
 * Copyright (c) Contributors, http://whitecore-sim.org/
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

using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Reflection;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.SceneInfo;
using WhiteCore.Framework.Services;
using WhiteCore.Framework.Servers.HttpServer;
using WhiteCore.Framework.Servers.HttpServer.Implementation;
using WhiteCore.Framework.Servers.HttpServer.Interfaces;
using WhiteCore.Framework.ConsoleFramework;

[assembly: AssemblyVersion("2014.5.8")]
[assembly: AssemblyFileVersion("2014.5.8")]

namespace WhiteCore.Modules.MarketPlaceAPI
{
    public class MarketPlaceAPIModule : IService
    {
        // TODO: This is the explanation what the MarketPlaceAPI will do
        //
        // * Register the user as a MarketPlace user
        //
        // * External MarketPlace can use the following calls
        // - GetBalance(uuid)
        // - Charge (uuid, amount, text)
        //
        
        #region Startup
        
        public string Name
        {
            get { return "MarketPlaceAPIModule"; }
        }        
        
        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            IConfig handlerConfig = config.Configs["MarketPlace"];
            if (handlerConfig.GetString("MarketPlaceHandler", "") != Name)
            {
                MainConsole.Instance.Info("[MarketPlaceAPI]: MarketPlaceAPI Handler not set");
                return;
            }
            MainConsole.Instance.Info("[MarketPlaceAPI]: MarketPlaceAPI has been started");
        }

        public void FinishedStartup()
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Money Regulators
        private void GetBalance(UUID agentID)
        {
            throw new NotImplementedException();
        }

        public bool Charge(UUID agentID, int amount, string text, TransactionType type)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}