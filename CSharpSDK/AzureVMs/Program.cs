using System;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.Network.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace AzureVMs
{
    internal class Program
    {
        private static IAzure azureClient;
        private static readonly string resourceGroupName = "rg-03";
        private static readonly Region region = Region.USWest;

        private static void Main(string[] args)
        {
            // Get the credentials from azureauth.json (see readme.txt)
            var credentials = SdkContext.AzureCredentialsFactory
                .FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION", EnvironmentVariableTarget.User));

            // Create Azure management client
            azureClient = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithDefaultSubscription();

            var avSetName = "HA01";
            var publicIpName = "publicIp";
            var vNetName = "vNet";

            // Create a resource group
            CreateResourceGroup().Wait(); ;

            // Create availability set
            var avSet = CreateAvailabilitySet(avSetName).Result;
            
            // Create Virtual Network
            var vNet = CreateVirtualNetwork(vNetName, "10.0.0.0/16", "subnet", "10.0.0.0/24").Result;

            // Create public IP Address
            var publicIp1 = CreatePublicIPAddress("publicIp1").Result;
            // Create Network Interface
            var nic1 = CreateNetworkInterface("nic1", vNet, "subnet", publicIp1).Result;
            // Create VM
            var vm1 = CreateVirtualMachine("DevVm01", nic1, avSet);

            // Create public IP Address
            var publicIp2 = CreatePublicIPAddress("publicIp2").Result;
            // Create Network Interface
            var nic2 = CreateNetworkInterface("nic2", vNet, "subnet", publicIp2).Result;
            // Create VM
            var vm2 = CreateVirtualMachine("DevVm02", nic2, avSet);

            Console.ReadLine();
        }

        private static async Task CreateResourceGroup()
        {
            Console.WriteLine($"Creating resource group {resourceGroupName}");

            var resourceGroup = await azureClient.ResourceGroups.Define(resourceGroupName)
                 .WithRegion(region)
                 .CreateAsync();

            Console.WriteLine($"Resource group {resourceGroup.Name} created.");
        }

        private static async Task<IAvailabilitySet> CreateAvailabilitySet(string availabilitySetName)
        {
            Console.WriteLine($"Creating Availability Set {availabilitySetName}");

            var avSet = await azureClient.AvailabilitySets.Define(availabilitySetName)
                 .WithRegion(region)
                 .WithExistingResourceGroup(resourceGroupName)
                 .WithSku(AvailabilitySetSkuTypes.Aligned)
                 .CreateAsync();

            Console.WriteLine($"Availability Set {availabilitySetName} created.");

            return avSet;
        }

        private static async Task<IPublicIPAddress> CreatePublicIPAddress(string publicIpAddressName)
        {
            Console.WriteLine($"Creating Public IP Address {publicIpAddressName}");

            var publicIp = await azureClient.PublicIPAddresses.Define(publicIpAddressName)
                    .WithRegion(region)
                    .WithExistingResourceGroup(resourceGroupName)
                    .WithDynamicIP()
                    .CreateAsync();

            Console.WriteLine($"Public IP Address  {publicIpAddressName} created.");

            return publicIp;
        }

        private static async Task<INetwork> CreateVirtualNetwork(string name, string addressSpace, string subnetName, string subnetAddressSpace)
        {
            Console.WriteLine($"Creating Virtual Network {name}");

            var network = await azureClient.Networks.Define(name)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroupName)
                .WithAddressSpace(addressSpace)
                .WithSubnet(subnetName, subnetAddressSpace)
                .CreateAsync();

            Console.WriteLine($"Virtual Network {name} created.");

            return network;
        }

        private static async Task<INetworkInterface> CreateNetworkInterface(string name, INetwork network, string subnetName, IPublicIPAddress publicIPAddress)
        {
            Console.WriteLine($"Creating Network Inteface {name}");

            var nic = await azureClient.NetworkInterfaces.Define(name)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroupName)
                .WithExistingPrimaryNetwork(network)
                .WithSubnet(subnetName)
                .WithPrimaryPrivateIPAddressDynamic()
                .WithExistingPrimaryPublicIPAddress(publicIPAddress)
                .CreateAsync();

            Console.WriteLine($"Network Inteface  {name} created.");

            return nic;
        }

        static async Task<IVirtualMachine> CreateVirtualMachine(string vmName, INetworkInterface nic, IAvailabilitySet avSet)
        {
            Console.WriteLine($"Creating VM {vmName}");

            var vm = await azureClient.VirtualMachines.Define(vmName)
                .WithRegion(region)
                .WithExistingResourceGroup(resourceGroupName)
                .WithExistingPrimaryNetworkInterface(nic)
                .WithLatestWindowsImage("MicrosoftWindowsServer", "WindowsServer", "2016-Datacenter")
                .WithAdminUsername("install")
                .WithAdminPassword("Y0urL0c@l@dmini$tr@t0r")
                .WithComputerName(vmName)
                .WithExistingAvailabilitySet(avSet)
                .WithSize(VirtualMachineSizeTypes.StandardA1V2)
                .CreateAsync();

            Console.WriteLine($"VM  {vmName} created.");

            return vm;
        }
    }
}
