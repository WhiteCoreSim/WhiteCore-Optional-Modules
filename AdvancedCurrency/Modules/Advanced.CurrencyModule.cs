﻿using WhiteCore.Framework;
using WhiteCore.Framework.Modules;
using WhiteCore.Framework.PresenceInfo;
using WhiteCore.Framework.SceneInfo;
using WhiteCore.Framework.Servers;
using WhiteCore.Framework.Services;
using WhiteCore.Framework.Utilities;
using Nini.Config;
using OpenMetaverse;
using OpenMetaverse.StructuredData;
using System;
using System.Collections.Generic;

namespace WhiteCore.Addon.AdvancedCurrency
{
    public class AdvancedCurrencyModule : IMoneyModule, IService
    {
        #region Declares

        private AdvancedCurrencyConfig Config
        {
            get { return m_connector.GetConfig(); }
        }
        private List<IScene> m_scenes = new List<IScene>();
        private AdvancedCurrencyConnector m_connector;
        private IRegistryCore m_registry;

        #endregion

        #region IService Members

        public void Initialize(IConfigSource config, IRegistryCore registry)
        {
            if (config.Configs["Currency"] == null ||
                config.Configs["Currency"].GetString("Module", "") != "AdvancedCurrency")
                return;

            m_registry = registry;
            m_connector = DataManager.RequestPlugin<IAdvancedCurrencyConnector>() as AdvancedCurrencyConnector;

            registry.RegisterModuleInterface<IMoneyModule>(this);
        }

        public void Start(IConfigSource config, IRegistryCore registry)
        {
            if (m_registry == null)
                return;
            ISyncMessageRecievedService syncRecievedService =
                registry.RequestModuleInterface<ISyncMessageRecievedService>();
            if (syncRecievedService != null)
                syncRecievedService.OnMessageReceived += syncRecievedService_OnMessageReceived;
        }

        public void FinishedStartup()
        {
            if (m_registry == null)
                return;
            ISceneManager manager = m_registry.RequestModuleInterface<ISceneManager>();
            if (manager != null)
            {
                manager.OnAddedScene += (scene) =>
                {
                                                m_scenes.Add(scene);
                                                scene.EventManager.OnNewClient += OnNewClient;
                                                scene.EventManager.OnClosingClient += OnClosingClient;
                                                scene.EventManager.OnMakeRootAgent += OnMakeRootAgent;
                                                scene.EventManager.OnValidateBuyLand += EventManager_OnValidateBuyLand;
                                                scene.RegisterModuleInterface<IMoneyModule>(this);
                                            };
                manager.OnCloseScene += (scene) =>
                                            {
                                                scene.EventManager.OnNewClient -= OnNewClient;
                                                scene.EventManager.OnClosingClient -= OnClosingClient;
                                                scene.EventManager.OnMakeRootAgent -= OnMakeRootAgent;
                                                scene.EventManager.OnValidateBuyLand -= EventManager_OnValidateBuyLand;
                                                scene.RegisterModuleInterface<IMoneyModule>(this);
                                                m_scenes.Remove(scene);
                                            };
            }

            if (!m_connector.DoRemoteCalls)
            {
                if ((m_connector.GetConfig().GiveStipends) && (m_connector.GetConfig().Stipend > 0))
                    new GiveStipends(m_connector.GetConfig(), m_registry, m_connector);
            }
        }

        private bool EventManager_OnValidateBuyLand(EventManager.LandBuyArgs e)
        {
            IParcelManagementModule parcelManagement = GetSceneFor(e.agentId).RequestModuleInterface<IParcelManagementModule>();
            if (parcelManagement == null)
                return false;
            ILandObject lob = parcelManagement.GetLandObject(e.parcelLocalID);

            if (lob != null)
            {
                UUID AuthorizedID = lob.LandData.AuthBuyerID;
                int saleprice = lob.LandData.SalePrice;
                UUID pOwnerID = lob.LandData.OwnerID;

                bool landforsale = ((lob.LandData.Flags &
                                     (uint)
                                     (ParcelFlags.ForSale | ParcelFlags.ForSaleObjects |
                                      ParcelFlags.SellParcelObjects)) != 0);
                if ((AuthorizedID == UUID.Zero || AuthorizedID == e.agentId) && e.parcelPrice >= saleprice &&
                    landforsale)
                {
                    if (m_connector.UserCurrencyTransfer(lob.LandData.OwnerID, e.agentId,
                                                         (uint) saleprice, "Land Buy", TransactionType.LandSale,
                                                         UUID.Zero))
                    {
                        e.parcelOwnerID = pOwnerID;
                        e.landValidated = true;
                        return true;
                    }
                    else
                    {
                        e.landValidated = false;
                    }
                }
            }
            return false;
        }

        #endregion

        #region IMoneyModule Members

        public int UploadCharge
        {
            get { return Config.PriceUpload; }
        }

        public int GroupCreationCharge
        {
            get { return Config.PriceGroupCreate; }
        }

        public int DirectoryFeeCharge
        {
            get { return Config.PriceDirectoryFee; }
        }

        public int ClientPort 
        {
            get  { return Config.ClientPort; }
        }

        public bool ObjectGiveMoney(UUID objectID, string objectName, UUID fromID, UUID toID, int amount)
        {
            return m_connector.UserCurrencyTransfer(toID, fromID, UUID.Zero, "", objectID, objectName, (uint) amount, "Object payment",
                                                    TransactionType.ObjectPays, UUID.Zero);
        }

        public int Balance(UUID agentID)
        {
            return (int) m_connector.GetUserCurrency(agentID).Amount;
        }

        public bool Charge(UUID agentID, int amount, string text, TransactionType type)
        {
            return m_connector.UserCurrencyTransfer(UUID.Zero, agentID, (uint)amount, text,
                                                    type, UUID.Zero);
        }

        public event ObjectPaid OnObjectPaid;

        public void FireObjectPaid(UUID objectID, UUID agentID, int amount)
        {
            if (OnObjectPaid != null)
                OnObjectPaid(objectID, agentID, amount);
        }

        public bool Transfer(UUID toID, UUID fromID, int amount, string description, TransactionType type)
        {
            return m_connector.UserCurrencyTransfer(toID, fromID, (uint) amount, description, type,
                                                    UUID.Zero);
        }

        public bool Transfer(UUID toID, UUID fromID, UUID toObjectID, string toObjectName, UUID fromObjectID, 
            string fromObjectName, int amount, string description, TransactionType type)
        {
            bool result = m_connector.UserCurrencyTransfer(toID, fromID, toObjectID, toObjectName, 
                fromObjectID, fromObjectName, (uint)amount, description, type, UUID.Zero);
            if (toObjectID != UUID.Zero)
            {
                ISceneManager manager = m_registry.RequestModuleInterface<ISceneManager>();
                if (manager != null)
                {
                    foreach (IScene scene in manager.Scenes)
                    {
                        ISceneChildEntity ent = scene.GetSceneObjectPart(toObjectID);
                        if (ent != null)
                            FireObjectPaid(toObjectID, fromID, amount);
                    }
                }
            }
            return result;
        }

        public List<GroupAccountHistory> GetTransactions(UUID groupID, UUID agentID, int currentInterval,
                                                         int intervalDays)
        {
            return new List<GroupAccountHistory>();
        }

        public GroupBalance GetGroupBalance(UUID groupID)
        {
            return m_connector.GetGroupBalance(groupID);
        }

        #endregion

        #region Client Members

        private void OnNewClient(IClientAPI client)
        {
            client.OnEconomyDataRequest += EconomyDataRequestHandler;
            client.OnMoneyBalanceRequest += SendMoneyBalance;
            client.OnMoneyTransferRequest += ProcessMoneyTransferRequest;
        }

        private void OnMakeRootAgent(IScenePresence presence)
        {
            presence.ControllingClient.SendMoneyBalance(UUID.Zero, true, new byte[0],
                                                        (int) m_connector.GetUserCurrency(presence.UUID).Amount);
        }

        protected void OnClosingClient(IClientAPI client)
        {
            client.OnEconomyDataRequest -= EconomyDataRequestHandler;
            client.OnMoneyBalanceRequest -= SendMoneyBalance;
            client.OnMoneyTransferRequest -= ProcessMoneyTransferRequest;
        }

        private void ProcessMoneyTransferRequest(UUID fromID, UUID toID, int amount, int type, string description)
        {
            if (toID != UUID.Zero)
            {
                ISceneManager manager = m_registry.RequestModuleInterface<ISceneManager>();
                if (manager != null)
                {
                    bool paid = false;
                    foreach (IScene scene in manager.Scenes)
                    {
                        ISceneChildEntity ent = scene.GetSceneObjectPart(toID);
                        if (ent != null)
                        {
                            bool success = m_connector.UserCurrencyTransfer(ent.OwnerID, fromID, ent.UUID, ent.Name, UUID.Zero, "",
                                (uint)amount, description, (TransactionType)type, UUID.Random());
                            if (success)
                                FireObjectPaid(toID, fromID, amount);
                            paid = true;
                            break;
                        }
                    }
                    if(!paid)
                    {
                        m_connector.UserCurrencyTransfer(toID, fromID, (uint)amount, description,
                                                    (TransactionType)type, UUID.Random());
                    }
                }
            }
        }

        private bool ValidateLandBuy(EventManager.LandBuyArgs e)
        {
            return m_connector.UserCurrencyTransfer(e.parcelOwnerID, e.agentId,
                                                    (uint) e.parcelPrice, "Land Purchase", TransactionType.LandSale,
                                                    UUID.Random());
        }

        private void EconomyDataRequestHandler(IClientAPI remoteClient)
        {
            if (Config == null)
            {
                remoteClient.SendEconomyData(0, remoteClient.Scene.RegionInfo.ObjectCapacity,
                                             remoteClient.Scene.RegionInfo.ObjectCapacity,
                                             0, 0,
                                             0, 0,
                                             0, 0,
                                             0,
                                             0, 0,
                                             0, 0,
                                             0,
                                             0, 0);
            }
            else
                remoteClient.SendEconomyData(0, remoteClient.Scene.RegionInfo.ObjectCapacity,
                                             remoteClient.Scene.RegionInfo.ObjectCapacity,
                                             0, Config.PriceGroupCreate,
                                             0, 0,
                                             0, 0,
                                             0,
                                             0, 0,
                                             0, 0,
                                             Config.PriceUpload,
                                             0, 0);
        }

        private void SendMoneyBalance(IClientAPI client, UUID agentId, UUID sessionId, UUID transactionId)
        {
            if (client.AgentId == agentId && client.SessionId == sessionId)
                client.SendMoneyBalance(transactionId, true, new byte[0],
                                        (int) m_connector.GetUserCurrency(client.AgentId).Amount);
            else
                client.SendAlertMessage("Unable to send your money balance to you!");
        }

        #endregion

        #region Service Members

        private OSDMap syncRecievedService_OnMessageReceived(OSDMap message)
        {
            string method = message["Method"];
            if (method == "UpdateMoneyBalance")
            {
                UUID agentID = message["AgentID"];
                int Amount = message["Amount"];
                string Message = message["Message"];
                UUID TransactionID = message["TransactionID"];
                IDialogModule dialogModule = GetSceneFor(agentID).RequestModuleInterface<IDialogModule>();
                IScenePresence sp = GetSceneFor(agentID).GetScenePresence(agentID);
                if (sp != null)
                {
                    if (dialogModule != null && !string.IsNullOrEmpty(Message))
                    {
                        dialogModule.SendAlertToUser(agentID, Message);
                    }
                    sp.ControllingClient.SendMoneyBalance(TransactionID, true, Utils.StringToBytes(Message), Amount);
                }
            }
            else if (method == "GetLandData")
            {
                UUID agentID = message["AgentID"];
                IParcelManagementModule parcelManagement = GetSceneFor(agentID).RequestModuleInterface<IParcelManagementModule>();
                if (parcelManagement != null)
                {
                    IScenePresence sp = GetSceneFor(agentID).GetScenePresence(agentID);
                    if (sp != null)
                    {
                        ILandObject lo = sp.CurrentParcel;
                        if ((lo.LandData.Flags & (uint) ParcelFlags.ForSale) == (uint) ParcelFlags.ForSale)
                        {
                            if (lo.LandData.AuthBuyerID != UUID.Zero && lo.LandData.AuthBuyerID != agentID)
                                return new OSDMap() {new KeyValuePair<string, OSD>("Success", false)};
                            OSDMap map = lo.LandData.ToOSD();
                            map["Success"] = true;
                            return map;
                        }
                    }
                }
                return new OSDMap() {new KeyValuePair<string, OSD>("Success", false)};
            }
            return null;
        }

        private IScene GetSceneFor(UUID userID)
        {
            foreach (IScene scene in m_scenes)
                if (scene.GetScenePresence(userID) != null)
                    return scene;
            if (m_scenes.Count == 0)
                return null;
            return m_scenes[0];
        }

        /// <summary>
        ///     All message for money actually go through this function. Which also update the balance
        /// </summary>
        /// <param name="toId"></param>
        /// <param name="message"></param>
        /// <param name="transactionId"></param>
        /// <returns></returns>
        public bool SendGridMessage(UUID toId, string message, UUID transactionId)
        {
            IDialogModule dialogModule = GetSceneFor(toId).RequestModuleInterface<IDialogModule>();
            if (dialogModule != null)
            {
                IScenePresence icapiTo = GetSceneFor(toId).GetScenePresence(toId);
                if (icapiTo != null)
                {
                    icapiTo.ControllingClient.SendMoneyBalance(transactionId, true, Utils.StringToBytes(message),
                                                               (int) m_connector.GetUserCurrency(icapiTo.UUID).Amount);
                    dialogModule.SendAlertToUser(toId, message);
                }

                return true;
            }
            return false;
        }

        #endregion
    }
}