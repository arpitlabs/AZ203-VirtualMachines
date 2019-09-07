$pass = ConvertTo-SecureString "W0rd@Pass2584" -AsPlainText -Force   
$cred = New-Object PSCredential("install", $pass) 

# Resource Group 'test' must already exist

New-AzVM -Name 'newvm'`
-ResourceGroupName 'test'`
-Location westus`
-VirtualNetworkName vnet1`
-SubnetName mysubnet`
-AddressPrefix '10.0.0.0/16'`
-SubnetAddressPrefix '10.0.0.0/24'`
-PublicIpAddressName 'publicip'`
-Image Win2016Datacenter`
-Size Standard_A2`
-Credential $cred  