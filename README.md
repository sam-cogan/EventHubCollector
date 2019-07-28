# EventHubCollector

An Azure Function to transfer Event Hub Data from Event Hub Capture to Cosoms DB on regular schedule.

## Implementation

This Function is designed to collect the .avro files created by Event Hub Capture and push the content of the Body of the Avro file into Cosmos DB.
The collector runs on a schedule (configurable) and reads the files from the storage account associated with Event Hub Capture. These files are loaded and the body field is parsed and exported to Cosmos DB as a new record. This is intended for content where the body field is in a JSON format ready for Cosmos to consume.

## Deployment

The simplest way to deploy is to use the button below, which will create the following Azure resources for you and configure them all to work:
* Storage Account for your Event Hub Capture to use
* Azure Function
* Key Vault to store secrets used by the function
* Azure Container Instance - This is used to deploy and configure the code to the function, it is run once only

[![Deploy to Azure](https://azuredeploy.net/deploybutton.svg)](https://deploy.azure.com/?repository=https://github.com/sam-cogan/EventHubCollector)

Alternatively you can download the code from Github and deploy manually, making sure the required resources and configuration settings are present.