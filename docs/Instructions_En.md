# Real-time logistics monitoring

In this article, I will present the steps to set up, from start to finish, a solution for monitoring a fleet of vehicles in real time. 

![Architecture](pictures/000.png)

After following this article, you will have a complete solution as illustrated in the animated gif below:

![Solution](pictures/Bus4GifAnime2.gif)

## Prerequisites

- An Azure [subscription](https://azure.microsoft.com/en-ca/free/)
- [Visual Studio Code](https://code.visualstudio.com/download)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [IoT Explorer](https://github.com/Azure/azure-iot-explorer/releases)

## Ingestion

The first step is to ingest the data into our solution. IoT Hub is the service used to receive events and enable other Azure services to subscribe and receive those events. An event is simply a set of data, which in our case is represented vehicle information (id, location, occupancy, transit status,....)

### IoT Hub

From the [Azure portal](https://portal.azure.com), in the marketplace, search for the Azure IoT Hub service, and then click "*Create*":

![IoTHub](pictures/001.png)

Fill in the information. The basic tier will be sufficient here. However, it may be increased at a later date as required.

Click on "*Review + Create*" and validate the creation of the service:

![IoTHub](pictures/002.png)

Once the service is created, go to its "*Overview*" page and update the certificate right away. Click on the link "*What do I need to do?*":

![IoTHub](pictures/003.png)

Click on the "*Migrate to DigiCert Global G2*" button:

![IoTHub](pictures/004.png)

Then check the 4 boxes before clicking on the "*Update*" button:

![IoTHub](pictures/005.png).

#### Creating a device

At your Azure IoT Hub service level, on the left, click "*Devices*" and then "*Add Device*"

![IoTHub](pictures/006.png)

Give the device a name, choose "*Symetric key*", check the "*Auto-generate keys*" box and check that the "*Enable*" option is selected.

Click on "*Save*"

![IoTHub](pictures/007.png)

Click "*Refresh*" to see your device in the list

![IoTHub](pictures/008.png)

#### Creation of consumer groups

Consumer groups are a status view of the hub. They allow multiple consuming applications to each have their own view of the event stream and read the stream independently. This means that when an application stops reading from an event stream, it can continue where it left off. It is best for each app to have its own group of consumers.

On the left, click on "Built-in endpoints", then create your consumer groups.

Copy the connection string "*Event Hub-compatible endpoint*" and keep it in an array. It will be used a little later.

![IoTHub](pictures/009.png)

#### Creating a "*SAS TOKEN*"

To connect to our IoT Hub with IoT Explorer we need a connection string.

On the left click on "*Shared access policies*", "*iothubowner*", then copy one of the connection strings.

![IoTHub](pictures/010.png)

For the creation of the login token, we will use [IoT Explorer](https://github.com/Azure/azure-iot-explorer/releases).

Run IoT Explorer and click "*Add connection*":

![IoTHub](pictures/011.png)

Paste the connection string and click "*Save*":

![IoTHub](pictures/012.png)

You should then see the devices present in your IoT Hub. Click on one of the devices:

![IoTHub](pictures/013.png)

In the "*Connection string with SAS token*" section, choose "*Primary key*", set the number of minutes (525,600 minutes = 1 year) for the token to expire, and then click "*Generate*".

You'll get a token similar to the one below:

*HostName=IoTHub-Logictic-AEffacer.azure-devices.net;DeviceId=FranmerBuses; SharedAccessSignature=SharedAccessSignature sr=IoTHub-Logictic-AEffacer.azure-devices.net%2Fdevices%2FFranmerBuses&sig=Qq0GFga6kzgwHp0DldvpJbR1mwAwXMOAm6Lmc9UFBhQ%3D&se=2392799555*

**Keep only the portion beginning with "*SharedAccessSignature sr=*" as shown below:**

*SharedAccessSignature sr=IoTHub-Logictic-AEffacer.azure-devices.net%2Fdevices%2FFranmerBuses&sig=Qq0GFga6kzgwHp0DldvpJbR1mwAwXMOAm6Lmc9UFBhQ%3D&se=2392799555*


Copy the generated portion and paste it. We're going to need it later.

![IoTHub](pictures/014.png)

#### Send events to Azure IoT Hub

From Visual Studio code, open the "*SendVehicleEvent.py*" file and fill in the required fields.

![IoTHub](pictures/015.png)

For the value "*YOUR FILE PATH*", go to the "*data*", then right-click on the file "*BusPositionV2.csv*" and click on "*Copy Path*":

![IoTHub](pictures/016.png)

After editing, you should get something as shown below:

![IoTHub](pictures/017.png)

Once the changes are made, run the code. If all goes well you should see the events in the terminal:

![IoTHub](pictures/018.png)

#### Checking sent events

We'll verify that events happen in our Azure IoT Hub

##### With Azure CLI

Open a "Command prompt" window and connect to the correct Azure tenant with the following command:

az login --tenant <Your Tenant ID>

![IoTHub](pictures/019.png)

Once connected to the correct tenant, run the following command (Here we will use one of the consumer groups we created earlier):

az IoT hub monitor-events -n {iothub_name} -d {device_id} -g {resource_group} --cg {consumer_group_name}

![IoTHub](pictures/020.png)

If all goes well, once the Azure CLI command is executed you should see the events that enter Azure IoT Hub:

![IoTHub](pictures/021.png)

#### Azure IoT Explorer

You can also monitor incoming events with Azure IoT Explorer:

![IoTHub](pictures/022.png)

## Event Processing

Now that the events are ingested into Azure IoT hub, we'll retrieve them for action.

### SignalR

The Azure SignalR service simplifies the process of adding real-time web capabilities to applications over HTTP. This real-time feature allows the service to send content updates to connected clients, such as a web or mobile app. Therefore, clients are updated without having to query the server or send new HTTP update requests.

From the Azure portal, click "*Create a resource*"

![SignalR](pictures/023.png)

Then search for the SignalR service:

![SignalR](pictures/024.png)

Fill in the information to create your service. Choose the resource group where you want to deploy the service.
To try, you can choose the "*Free*" tier.

Choose "***Serverless***" for the service mode.

Click on "*Review + create*"

![SignalR](pictures/025.png)

Validate the creation of the service by clicking on the "*Create*" button

![SignalR](pictures/026.png)

You should now have 2 services in your resource group:

![SignalR](pictures/027.png)

Click your SignalR service, then click "*Connection strings*" to retrieve the connection string. Copy it and paste it into a file. We will need it a little later.

![SignalR](pictures/SignalR_928.png)


### Azure Function

We will now create an "*Azure Function App*" to retrieve events from Azure IoT hub and ensure real-time communication with Azure Maps (which we will deploy a little later)

From the Azure portal, create a new resource and search for "*Function App*":

![Function](pictures/028.png)

Set the options as shown in the screenshot below and click on "*Review + create*", then validate the creation of the "*Function App*":

![Function](pictures/029.png)

From the Azure portal, go to your resource group and click on the newly deployed "*Function App*":

![Function](pictures/030.png)

Then click on "Configuration. In "application settings" click on "New application setting"

![Function](pictures/031.png)

Add the values:

- AzureIOTHubConnectionString
- AzureSignalRConnectionString

For the value "*AzureIOTHubConnectionString*" fill in the value of the connection string for IoT Hub (*Event Hub-compatible endpoint*):

![Function](pictures/032.png)

For the value "*AzureSignalRConnectionString*" specify the connection string of the SignalR service: 

![Function](pictures/033.png)

Don't forget to save your changes by clicking on the "*Save*" button:

![Function](pictures/034.png)


#### Code deployment

We will deploy the function code from Visual Studio Code. First, make sure you're signed in to Azure:

![Function](pictures/035.png)

Once signed in, you should see your Azure resources:

![Function](pictures/036.png)

the code is located in the "*Functions*" folder. This is the file "*FranmerRealTimeLogistic.cs*". Click the file, and then from the command palette (Ctrl + Shift + P), select "*Azure Functions: Deploy to Function App... *" 

![Function](pictures/037.png)

Then select your "*Function App*":

![Function](pictures/038.png)

Validate the update:

![Function](pictures/039.png)

If all goes well, you should get this message at the end of the deployment:

![Function](pictures/040.png)

And you should see your 2 functions deployed in Azure:

![Function](pictures/041.png)

## Serve and present

Now we will deploy the services to serve and present the information

### Azure Maps

Azure Maps is a collection of geospatial services and SDKs that use up-to-date map data to provide accurate geographic context for web and mobile applications. Azure Maps provides the following services:

- REST API to render vector and raster maps in multiple styles and satellite imagery.
- Creator services to create and display maps based on private interior map data.
- Search services to locate addresses, places and points of interest worldwide.
- Various routing options: point-to-point, multipoint, multipoint optimization, isochronous, electric vehicle, commercial vehicle, influenced traffic and matrix routing.
- Traffic flow view and incident view for applications that need real-time traffic information.
- Time zone (*Time zone*) and geolocation (*Geolocation*) services.
- *geofencing* services and map data storage, with location information hosted in Azure.
- Geographic intelligence via geospatial analytics.

The Azure Maps solution shown here includes clustering and dynamic pop-ups.


#### Deploying Azure Maps

From the Azure portal, create a new resource and search for Azure Maps. Click on "*Create*":

![Maps](pictures/042.png)

Fill in the necessary information. For the price tier, choose "*Gen2 (Maps and Location Insights)*": 

![Maps](pictures/043.png)

Once the service is deployed, from the Azure portal, click on your Azure Maps service, and then click "*Authentication*". Copy the primary key of the service and copy it to a file. We will need it a little later:

![Maps](pictures/044.png)

### Azure Web App

Azure App Service is an HTTP service for hosting web apps, REST APIs, and mobile backends. You can develop in your preferred language, namely .NET, .NET Core, Java, Node.js, PHP and Python. Applications run and scale easily in Windows and Linux environments.

#### Azure Web App deployment

From the Azure portal, create a new resource and search for Azure Web App:

![WebApp](pictures/045.png)

Fill in the necessary information. For the following fields, set the values as shown below:

- Publish: "Code"
- Runtime stack: "PHP 8.2"
- Pricing plan: "Free"

Click on "Review + Create" and validate the creation of the service:

![WebApp](pictures/046.png)

Once the service is deployed, go to the "Overview" page of your Azure Web App service and copy the "*Default Domain*" value. Copy this value to a file, we will need it a little later:

![WebApp](pictures/047.png)

### Deployment

After all Azure services are deployed, you should have the following services in your resource group:

![Solution](pictures/048.png)

#### Azure Function CORS definition

In order to allow the Azure Maps Service to communicate with Azure functions, there is one last configuration to be made at the Azure level "*Function App*".

At your "*Function App*", click on "*CORS*" ("Cross-Origin Resource Sharing") and add the URL of your web application that you copied earlier:

![Solution](pictures/049.png)

#### Modifying the web application code

From Visual Studio Code, verify that you have the "*Azure App Service*" add-in:

![Solution](pictures/050.png)

Go back to the "*Explorer*" part of Visual Studio Code, in the "*Web*" folder click the "*Index.html*" file. Replace the following values:

- baseurl (the url of your Azure Function App)
- subscriptionKey (the key to your Azure Maps service)

![Solution](pictures/051.png)

For example you should get something like below after modifications:

![Solution](pictures/052.png)

In addition, change the references, with the url of your web application, to images or services, where appropriate, as shown below:

For images:

![Solution](pictures/053.png)

For features:

![Solution](pictures/054.png)



#### Web Application Deployment

From Visual Studio Code, in the Azure part, verify that you have access to your web application.

![Solution](pictures/055.png)

Right-click on your web application and click on "*Deploy to Web App... *" :

![Solution](pictures/056.png)

The command palette will open at the top of the screen. Click "*Browse...*" to select the folder you want to deploy:

![Solution](pictures/057.png)

In our case we will deploy the "*Web*" folder. **Double-click** on the "*Web*" folder and click "*Select*":

![Solution](pictures/058.png)

Click on "*Deploy*":

![Solution](pictures/059.png)

If all goes well, you should have the message below at the bottom right of your screen. Click on "*Browse Website*":

![Solution](pictures/060.png)

You should have a new tab opening in your web browser displaying a map focused on Montreal:

![Solution](pictures/061.png)

#### Follow buses in real time

We will now send events to IoT Hub to display them in near real-time on our Azure Maps.

In the Visual Studio Code File Explorer, click "*SendVehicleEvents.py*" which is located in the "*producer*" folder. Click on the "*play*" button at the top right of the screen:

![Solution](pictures/062.png)

You should see events in the console:

![Solution](pictures/063.png)

You should see the buses start moving on the map:

![Solution](pictures/064.png)

Below is a small animated Gif to illustrate the solution with clustering, event-based color management and dynamic pop-ups:

![Solution](pictures/Bus4GifAnime.gif)

## How to debug

After publishing your web page, in your web browser, press the **F12** key and scan for returned errors.

Below is an example of common errors with poor image reference and misconfiguration of "*applications settings*" at the Azure "*Function App*" level:

![Solution](pictures/065.png)