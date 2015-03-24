/*
 * Copyright (c) Contributors, http://whitecore-sim.org, http://aurora-sim.org/
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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using Nini.Config;
using OpenMetaverse;
using WhiteCore.Framework.ConsoleFramework;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.SceneInfo;
using WhiteCore.Framework.Services;
using WhiteCore.Framework.Services.ClassHelpers.Assets;
using WhiteCore.Framework.Services.ClassHelpers.Inventory;
using WhiteCore.Modules.Archivers;
using WhiteCore.Region;

[assembly: AssemblyVersion("2014.12.19")]
[assembly: AssemblyFileVersion("2014.12.19")]
[assembly: AssemblyTitle("DefaultInventoryToIAR")]
[assembly: AssemblyCompany("WhiteCore-Sim.org")]
[assembly: AssemblyDescription("WhiteCore Default inventory export module")]

namespace WhiteCore.Addon.DefaultInventoryToIARConverter
{
    /// <summary>
    /// This plugin saves the default asset and inventory folders over into an IAR file so they can be loaded easier.
    /// </summary>
    public class DefaultInventoryToIARConverter : IService
    {
		IConfig libConfig;
		string IARName = "./DefaultInventory/DefaultInventory.iar";
		bool m_enabled = true;
		bool m_busy;
		IRegistryCore m_registry;


        protected ILibraryService m_service;

		/// <value>The name of the module</value>
		/// <summary>
		/// Gets the name.
		/// </summary>
		public string Name
		{
			get { return "DefaultAssetsIARCreator"; }
		}

		/// <summary>
		/// Set up and register the module
		/// </summary>
		/// <param name="config">Config file</param>
		/// <param name="registry">Place to register the modules into</param>
        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
            m_registry = registry;
			libConfig = config.Configs["DefaultAssetsIARCreator"];

			if (libConfig == null)
			{
				var iniSource = new IniConfigSource ("DefaultInventory/Inventory.ini", Nini.Ini.IniFileType.AuroraStyle);
				libConfig = iniSource.Configs["DefaultAssetsIARCreator"];
			}

            if (libConfig != null)
            {
                m_enabled = libConfig.GetBoolean ("Enabled", false);
                IARName = libConfig.GetString("NameOfIAR", IARName);
            }

			if ( m_enabled )
			{
				AddConsoleCommands();
			}
        }

        public void PostInitialize(IConfigSource config, IRegistryCore registry)
        {
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
        }

        public void FinishedStartup()
        {
        }


		/// <summary>
		/// Handles the default inventory save.
		/// </summary>
		/// <param name="scene">Scene.</param>
		/// <param name="cmd">Cmd.</param>
		void HandleDefInvSave( IScene scene, string[] cmd )
		{
			if (!m_enabled)
				return;

            if (m_busy)
                return;

            string fileName = IARName;

			// optional filename
			int el = cmd.Length;
			if (el >= 4)
			{
				fileName = cmd[3];

				// some file sanity checks
				string extension = Path.GetExtension (fileName);

				if (extension == string.Empty)
				{
					fileName = fileName + ".iar";
				}
                    
			}

            string fileDir = Path.GetDirectoryName (fileName);
            if (fileDir == "")
            {
                fileDir = "./DefaultInventory";
                fileName = fileDir + '/' + fileName;
            }
            if (!Directory.Exists (fileDir))
            {
                MainConsole.Instance.Info ("[LIBDEF]: The folder specified, '" + fileDir + "' does not exist!");
                return;
            }

            // don't try and write to an existing file
            if (File.Exists (fileName))
			{
                if (MainConsole.Instance.Prompt ("[LIBDEF]: The inventory file '" + fileName + "' exists. Overwrite?", "yes") != "yes")
					return;

                File.Delete (fileName);
			}

			// good to go... do it...
			m_busy = true;
			m_service = m_registry.RequestModuleInterface<ILibraryService>();

			RegionInfo regInfo = new RegionInfo();
			IScene m_MockScene = null;

			//Make the scene for the IAR loader
			if (m_registry is IScene)
				m_MockScene = (IScene)m_registry;
			else
			{
				m_MockScene = new Scene();
				m_MockScene.Initialize(regInfo);
				m_MockScene.AddModuleInterfaces(m_registry.GetInterfaces());
			}

			UserAccount uinfo = m_MockScene.UserAccountService.GetUserAccount(null, m_service.LibraryOwner);
			//Make the user account for the default IAR
			if (uinfo == null)
			{
				uinfo = new UserAccount(m_service.LibraryOwner);
				uinfo.Name = m_service.LibraryOwnerName;
				m_MockScene.InventoryService.CreateUserInventory(m_service.LibraryOwner, false);
			}

			List<AssetBase> assets = new List<AssetBase> ();
			if (m_MockScene.InventoryService != null)
			{
				//Add the folders to the user's inventory
				InventoryCollection i = m_MockScene.InventoryService.GetFolderContent (m_service.LibraryOwner, UUID.Zero);
				if (i != null)
				{
					foreach (InventoryItemBase item in i.Items)
					{
						AssetBase asset = m_MockScene.RequestModuleInterface<IAssetService> ().Get (item.AssetID.ToString ());
						if (asset != null)
							assets.Add (asset);
					}
				}
			}
			InventoryFolderBase rootFolder = null;
			List<InventoryFolderBase> rootFolders = m_MockScene.InventoryService.GetRootFolders (m_service.LibraryOwner);
			foreach (InventoryFolderBase folder in rootFolders)
			{
				if (folder.Name == "My Inventory")
					continue;

				rootFolder = folder;
				break;
			}
			if (rootFolder != null)
			{
				//Save the IAR of the default assets
                MainConsole.Instance.Info ("[LIBDEF]: Saving default inventory to " + fileName);
				InventoryArchiveWriteRequest write = new InventoryArchiveWriteRequest (Guid.NewGuid (), null, m_MockScene,
                    uinfo, "/", new GZipStream (new FileStream (fileName, FileMode.Create), CompressionMode.Compress), true, rootFolder, assets, null);
				write.Execute ();
			}

            m_busy = false;

		}

		/// <summary>
		/// Handles the help command.
		/// </summary>
		/// <param name="scene">Not used</param>
		/// <param name="cmd">Not used</param>
		void HandleDefInvHelp( IScene scene, string[] cmd )
		{

			MainConsole.Instance.Info (
				"save default inventory [IAR Filename]\n" +
				"Save the current default inventory to an IAR file for later\n" +
				"[IAR Filename] : Optional, defaults to 'DefaultInventory.iar'");
		}

		/// <summary>
		/// Adds the console commands.
		/// </summary>
		void AddConsoleCommands()
		{
			if (MainConsole.Instance != null)
			{
				MainConsole.Instance.Commands.AddCommand (
					"save default inventory",
					"save default inventory [IAR Filename]", 
					"Save the current default inventory to an IAR file for later reloadingte>: The required activity state",
					HandleDefInvSave,
					false,
                    true);
                
				MainConsole.Instance.Commands.AddCommand (
                    "save default inventory help",
                    "save default inventory help",
                    "Help about the save default inventory command.",
					HandleDefInvHelp,
					false,
                    true);
			}
		}
    }
}
