$rg = 'resourcegroupname'   
$location = 'westus'

New-AzResourceGroup -Name $rg -Location $location 

New-AzResourceGroupDeployment -Name 'Deployment1' -ResourceGroupName $rg -Mode Complete -TemplateFile .\virtualmachine.json  -TemplateParameterFile .\virtualmachine-param.json 