﻿using System.Collections.Generic;
using System.Linq;
using AElf.Common;
using AElf.Kernel;
using AElf.Sdk.CSharp;
using AElf.Sdk.CSharp.Types;
using Google.Protobuf.WellKnownTypes;
using Api = AElf.Sdk.CSharp.Api;
using Enum = System.Enum;

namespace AElf.Contracts.Resource
{
    public class ResourceContract : CSharpSmartContract
    {
        #region static

        internal static List<string> ResourceTypes;

        static ResourceContract()
        {
            ResourceTypes = Enum.GetValues(typeof(ResourceType))
                .Cast<ResourceType>().Select(x => x.ToString()).ToList();
        }

        internal static void AssertCorrectResourceType(string resourceType)
        {
            Api.Assert(ResourceTypes.Contains(resourceType), "Incorrect resource type.");
        }

        #endregion static 

        #region Fields

        internal Map<StringValue, ConnectorPair> ConnectorPairs = new Map<StringValue, ConnectorPair>("ConnectorPairs");
        internal MapToUInt64<UserResourceKey> UserResources = new MapToUInt64<UserResourceKey>("UserResources");
        internal MapToUInt64<UserResourceKey> LockedUserResources = new MapToUInt64<UserResourceKey>("LockedUserResources");
        internal BoolField Initialized = new BoolField("Initialized");
        internal PbField<Address> ElfTokenAddress = new PbField<Address>("ElfTokenAddress");

        #endregion Fields

        #region Helpers

        private ElfTokenShim ElfToken => new ElfTokenShim(ElfTokenAddress);

        #endregion Helpers

        #region Views

        [View]
        public byte[] GetElfTokenAddress()
        {
            return ElfTokenAddress.GetValue().DumpByteArray();
        }

        [View]
        public ulong GetResourceBalance(Address address, string resourceType)
        {
            AssertCorrectResourceType(resourceType);
            var urk = new UserResourceKey()
            {
                Address = address,
                Type = (ResourceType) Enum.Parse(typeof(ResourceType),
                    resourceType)
            };
            return UserResources[urk];
        }

        #endregion Views

        #region Actions

        public void Initialize(Address elfTokenAddress)
        {
            var i = Initialized.GetValue();
            Api.Assert(!i, $"Already initialized {i}.");
            ElfTokenAddress.SetValue(elfTokenAddress);
            foreach (var resourceType in ResourceTypes)
            {
                var rt = new StringValue() {Value = resourceType};
                var c = ConnectorPairs[rt];
                c.ElfBalance = 1000000;
                ConnectorPairs[rt] = c;
            }

            Initialized.SetValue(true);
        }

        public void AdjustResourceCap(string resourceType, ulong newCap)
        {
            // TODO: Limit the permission to delegate nodes' multisig
            AssertCorrectResourceType(resourceType);
            var rt = new StringValue() {Value = resourceType};
            var connector = ConnectorPairs[rt];
            connector.ResBalance = newCap;
            ConnectorPairs[rt] = connector;
        }

        public void BuyResource(string resourceType, ulong paidElf)
        {
            AssertCorrectResourceType(resourceType);
            var payout = this.BuyResourceFromExchange(resourceType, paidElf);
            var urk = new UserResourceKey()
            {
                Address = Api.GetFromAddress(),
                Type = (ResourceType) Enum.Parse(typeof(ResourceType),
                    resourceType)
            };
            UserResources[urk] = UserResources[urk].Add(payout);
            ElfToken.TransferByUser(Api.GetContractAddress(), paidElf);
        }

        public void SellResource(string resourceType, ulong resToSell)
        {
            AssertCorrectResourceType(resourceType);
            var elfToReceive = this.SellResourceToExchange(resourceType, resToSell);
            var urk = new UserResourceKey()
            {
                Address = Api.GetFromAddress(),
                Type = (ResourceType) Enum.Parse(typeof(ResourceType),
                    resourceType)
            };
            UserResources[urk] = UserResources[urk].Sub(resToSell);
            ElfToken.TransferByContract(Api.GetFromAddress(), elfToReceive);
        }

        public void LockResource(Address to, ulong amount, string resourceType)
        {
            AssertCorrectResourceType(resourceType);
            var urkFrom = new UserResourceKey
            {
                Address = Api.GetFromAddress(),
                Type = (ResourceType) Enum.Parse(typeof(ResourceType),
                    resourceType)
            };
            UserResources[urkFrom] = UserResources[urkFrom].Sub(amount);
            var lurkTo = new UserResourceKey
            {
                Address = to,
                Type = (ResourceType) Enum.Parse(typeof(ResourceType),
                    resourceType)
            };
            LockedUserResources[lurkTo] = UserResources[lurkTo].Add(amount);
        }
        
        public void WithdrawResource(Address to, ulong amount, string resourceType)
        {
            AssertCorrectResourceType(resourceType);
            var urkFrom = new UserResourceKey
            {
                Address = Api.GetFromAddress(),
                Type = (ResourceType) Enum.Parse(typeof(ResourceType),
                    resourceType)
            };
            UserResources[urkFrom] = UserResources[urkFrom].Add(amount);
            var lurkTo = new UserResourceKey
            {
                Address = to,
                Type = (ResourceType) Enum.Parse(typeof(ResourceType),
                    resourceType)
            };
            LockedUserResources[lurkTo] = UserResources[lurkTo].Sub(amount);
        }
        
        #endregion Actions
    }
}