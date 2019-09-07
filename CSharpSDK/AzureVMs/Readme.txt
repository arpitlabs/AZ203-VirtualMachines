# Create sdk-auth file using Azure CLI
az ad sp create-for-rbac --sdk-auth

# Save the output in json file azureauth.json

# Save the location of azureauth.json in environment variable AZURE_AUTH_LOCATION
# [Environment]::SetEnvironmentVariable("AZURE_AUTH_LOCATION", "C:\src\azureauth.json", "User")

# Use azureauth.json in SDK
# // pull in the location of the authentication properties file from the environment 
# // var credentials = SdkContext.AzureCredentialsFactory
# //    .FromFile(Environment.GetEnvironmentVariable("AZURE_AUTH_LOCATION"));
