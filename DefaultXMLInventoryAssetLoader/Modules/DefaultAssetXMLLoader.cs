/*
 * Copyright (c) Contributors, http://whitecore-sim.org/ and http://aurora-sim.org/
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
using System.Xml;
using Nini.Config;
using OpenMetaverse;
using WhiteCore.Framework.ConsoleFramework;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.Services;
using WhiteCore.Framework.Services.ClassHelpers.Assets;


namespace WhiteCore.Addon.DefaultAssetXMLLoader
{
    /// <summary>
    /// Loads assets from the file system location.
    /// </summary>
    public class DefaultAssetXMLLoader : IDefaultLibraryLoader
    {
        protected ILibraryService m_service;

        protected AssetBase CreateAsset (string assetIdStr, string name, string path, AssetType type)
        {
            var assetBase = new AssetBase (new UUID (assetIdStr), name, type, m_service.LibraryOwnerUUID);

            if (!string.IsNullOrEmpty (path)) {
                //MainConsole.Instance.InfoFormat("[AssetsXMLLoader]: Loading: [{0}][{1}]", name, path);
                LoadAsset (assetBase, path);
            } else
                MainConsole.Instance.InfoFormat ("[AssetsXMLLoader]: Instantiated: [{0}]", name);

            return assetBase;
        }

        protected static void LoadAsset (AssetBase info, string path)
        {
            //            bool image =
            //               (info.Type == (sbyte)AssetType.Texture ||
            //                info.Type == (sbyte)AssetType.TextureTGA ||
            //                info.Type == (sbyte)AssetType.ImageJPEG ||
            //                info.Type == (sbyte)AssetType.ImageTGA);

            var fInfo = new FileInfo (path);
            long numBytes = fInfo.Length;
            if (fInfo.Exists) {
                byte [] idata;
                var fStream = new FileStream (path, FileMode.Open, FileAccess.Read);
                var br = new BinaryReader (fStream);

                idata = br.ReadBytes ((int)numBytes);
                br.Close ();
                fStream.Close ();
                info.Data = idata;
                //info.loaded=true;
            } else
                MainConsole.Instance.ErrorFormat ("[AssetsXMLLoader]: file: [{0}] not found !", path);
        }

        protected void ForEachDefaultXmlAsset (string assetSetFilename, Action<AssetBase> action)
        {
            var assets = new List<AssetBase> ();
            if (File.Exists (assetSetFilename)) {
                string assetSetPath = "ERROR";
                string assetRootPath;
                try {
                    DateTime start = DateTime.Now;
                    var xmlSource = new XmlConfigSource (assetSetFilename);
                    assetRootPath = Path.GetFullPath (xmlSource.SavePath);
                    assetRootPath = Path.GetDirectoryName (assetRootPath);

                    for (int i = 0; i < xmlSource.Configs.Count; i++) {
                        assetSetPath = xmlSource.Configs [i].GetString ("file", string.Empty);

                        LoadXmlAssetSet (Path.Combine (assetRootPath, assetSetPath), assets);
                    }
                    MainConsole.Instance.Warn ((DateTime.Now - start).Milliseconds);
                } catch (XmlException e) {
                    MainConsole.Instance.ErrorFormat ("[AssetsXMLLoader]: Error loading {0} : {1}", assetSetPath, e);
                }
            } else
                MainConsole.Instance.ErrorFormat ("[AssetsXMLLoader]: Asset set control file {0} does not exist!  No assets loaded.", assetSetFilename);

            DateTime start2 = DateTime.Now;
            assets.ForEach (action);
            MainConsole.Instance.Warn ((DateTime.Now - start2).Milliseconds);
        }

        /// <summary>
        /// Use the asset set information at path to load assets
        /// </summary>
        /// <param name="assetSetPath"></param>
        /// <param name="assets"></param>
        protected void LoadXmlAssetSet (string assetSetPath, List<AssetBase> assets)
        {
            //MainConsole.Instance.InfoFormat("[AssetsXMLLoader]: Loading asset set {0}", assetSetPath);

            if (File.Exists (assetSetPath)) {
                try {
                    var xmlSource = new XmlConfigSource (assetSetPath);
                    string dir = Path.GetDirectoryName (assetSetPath);

                    for (int i = 0; i < xmlSource.Configs.Count; i++) {
                        string assetIdStr = xmlSource.Configs [i].GetString ("assetID", UUID.Random ().ToString ());
                        string name = xmlSource.Configs [i].GetString ("name", string.Empty);
                        var assetType = (AssetType)xmlSource.Configs [i].GetInt ("assetType", 0);
                        string assetPath = Path.Combine (dir, xmlSource.Configs [i].GetString ("fileName", string.Empty));

                        AssetBase newAsset = CreateAsset (assetIdStr, name, assetPath, assetType);

                        newAsset.Type = (int)assetType;
                        assets.Add (newAsset);
                    }
                } catch (XmlException e) {
                    MainConsole.Instance.ErrorFormat ("[AssetsXMLLoader]: Error loading {0} : {1}", assetSetPath, e);
                }
            } else
                MainConsole.Instance.ErrorFormat ("[AssetsXMLLoader]: Asset set file {0} does not exist!", assetSetPath);
        }

        #region IDefaultLibraryLoader Members

        public void LoadLibrary (ILibraryService service, IConfigSource source, IRegistryCore registry)
        {
            m_service = service;

            IConfig assetConfig = source.Configs ["AssetsXMLLoader"];
            if (assetConfig == null)
                return;

            string loaderArgs = assetConfig.GetString ("AssetLoaderArgs", string.Empty);
            bool assetLoaderEnabled = !assetConfig.GetBoolean ("PreviouslyLoaded", false);

            if (!assetLoaderEnabled)
                return;

            registry.RegisterModuleInterface<DefaultAssetXMLLoader> (this);

            MainConsole.Instance.InfoFormat ("[AssetsXMLLoader]: Loading default asset set from {0}", loaderArgs);
            IAssetService assetService = registry.RequestModuleInterface<IAssetService> ();
            ForEachDefaultXmlAsset (loaderArgs,
                    delegate (AssetBase a) {
                        if (!assetLoaderEnabled && assetService.GetExists (a.IDString))
                            return;
                        assetService.Store (a);
                    });
        }

        #endregion
    }
}
