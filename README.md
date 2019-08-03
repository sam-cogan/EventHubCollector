# EventHubCollector

An Azure Function to transfer Event Hub Data from Event Hub Capture to Cosoms DB on regular schedule.

## Implementation

This Function is designed to collect the .avro files created by Event Hub Capture and push the content of the Body of the Avro file into Cosmos DB.
The collector runs on a schedule (configurable) and reads the files from the storage account associated with Event Hub Capture. These files are loaded and exported to Cosmos DB as a new record.

### Options
There are a few options you can set at the time of deploying the function
1. FunctionIntervalInMinutes - Defines how often the function checks for records, and how far back it looks for them. Default is 15 minutes
2. OutputType- Determines whether to output the whole of the EventHub message to Cosmos DB, or just he body part. Default is all

## Deployment

The simplest way to deploy is to use the button below, which will create the following Azure resources for you and configure them all to work:
* Storage Account for your Event Hub Capture to use
* Azure Function
* Key Vault to store secrets used by the function
* Azure Container Instance - This is used to deploy and configure the code to the function, it is run once only

This assumes that the account your are deploying as has access to retrieve your Cosmos DB keys

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://deploy.azure.com/?repository=https://github.com/sam-cogan/EventHubCollector)

Alternatively you can download the code from Github and deploy manually, making sure the required resources and configuration settings are present.