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
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using WhiteCore.Framework.ClientInterfaces;
using WhiteCore.Framework.ConsoleFramework;
using WhiteCore.Framework.DatabaseInterfaces;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.SceneInfo;
using WhiteCore.Framework.Servers;
using WhiteCore.Framework.Services;

[assembly: AssemblyVersion("2014.5.17")]
[assembly: AssemblyFileVersion("2014.5.17")]

namespace WhiteCore.Addon.SystemUsers
{
    public class SystemUsersModule : IService
    {
        public const string RealEstateOwnerUUID = "3d6181b0-6a4b-97ef-18d8-722652995cf1";
        public const string RealEstateOwnerName = "RealEstate Owner";
        public const string RealEstateGroupUUID = "dc7b21cd-3c89-fcaa-31c8-25f9ffd224cd";
        public const string RealEstateGroupName = "Maintenance";

        private IGroupsServiceConnector m_groupData;
        IRegistryCore m_registry;

        public string Name
        {
            get { return "SystemUsers"; }
        }

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
            MainConsole.Instance.Commands.AddCommand("create system users", "create system users", "Creates all the neccessary System users/groups for Auctions, Land and other tasks", CreateSystemUser, false, true);
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            throw new NotImplementedException();
        }

        public void FinishedStartup()
        {
            throw new NotImplementedException();
        }

        private void CreateSystemUser(IScene scene, string[] cmd)
        {
            IScene m_MockScene = null;
            RegionInfo regInfo = new RegionInfo();

            if (m_registry is IScene)
                m_MockScene = (IScene)m_registry;
            else
            {
                m_MockScene = new Scene();
                m_MockScene.Initialize(regInfo);
                m_MockScene.AddModuleInterfaces(m_registry.GetInterfaces());
            }
            UserAccount uinfo = m_MockScene.UserAccountService.GetUserAccount(null, RealEstateOwnerUUID);
            if (uinfo == null)
            {
                MainConsole.Instance.Warn("Creating System User " + RealEstateOwnerName);
                m_MockScene.UserAccountService.CreateUser((UUID)RealEstateOwnerUUID, UUID.Zero, RealEstateOwnerName,
                                                          "", "");
                uinfo = m_MockScene.UserAccountService.GetUserAccount(null, RealEstateOwnerUUID);
                m_MockScene.InventoryService.CreateUserInventory((UUID)RealEstateOwnerUUID, true);
                bool result = CreateSystemGroup();
                if (result)
                {
                    MainConsole.Instance.Warn("All System Users and Groups have been created");
                }
                else
                {
                    MainConsole.Instance.Error("Not all operations could be finished, please check the console for errors");
                    }
            }
            else
            {
                MainConsole.Instance.Warn("System user " + RealEstateOwnerName + " already exists");
            }
        }

        private bool CreateSystemGroup()
        {
            if (m_groupData.GetGroupRecord(UUID.Zero, UUID.Zero, "Maintenance") != null)
            {
                MainConsole.Instance.Warn("System Group " + RealEstateGroupName + " already exists");
                return false;
            }
            else
            {
                MainConsole.Instance.Warn("Creating System Group " + RealEstateGroupName);
                m_groupData.CreateGroup((UUID)RealEstateGroupUUID, "Maintenance", "This group is for the RealEstate Maintenance", false, UUID.Zero, 0, false, false, true, (UUID)RealEstateGroupUUID, UUID.Random());
                return true;
            }
        }
    }
}
