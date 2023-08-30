# Suivi logistic en temps réel

Dans cet article, je vais vous présenter les étapes pour mettre en place, de bout en bout, une solution de suivie d'une flotte de véhicules en temps réel. 

![Architecture](pictures/000.png)

## Prérequis

- Une [souscription](https://azure.microsoft.com/en-ca/free/) Azure
- [Visual Studio Code](https://code.visualstudio.com/download)
- [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/install-azure-cli)
- [IoT Explorer](https://github.com/Azure/azure-iot-explorer/releases)




## Ingestion

La première étape consiste à ingérer les données dans notre solution. IoT Hub est le service utilisé pour recevoir des événements et permettre à d'autres services Azure de s’abonner et recevoir ces événements. Un événement est simplement un ensemble de données, qui dans notre cas est représenté les informations des véhicules (id, emplacement, occupation, état du transit,....)

### IoT Hub

Depuis le [portail Azure](https://portal.azure.com), dans la place de marché, recherchez le service Azure IoT Hub, puis cliquez sur "*Create*" :

![IoTHub](pictures/001.png)

Renseignez les informations. Le tier basic sera suffisant ici. Il pourra cependant être augmenté ultérieurement en fonction des besoins.

Cliquez sur "*Review + Create*" puis validez la création du service :

![IoTHub](pictures/002.png)

Une fois le service créé, rendez-vous sur sa page "*Overview*" et mettez tout de suite à jour le certificat. Cliquez sur le lien "*What do I need to do?*" :

![IoTHub](pictures/003.png)

CLiquez sur le bouton "*Migrate to DigiCert Global G2*" :

![IoTHub](pictures/004.png)

Puis cochez les 4 cases avant de cliquez sur le bouton "Update" :

![IoTHub](pictures/005.png).

## Creation d'un appareil

Au niveau de votre service Azure IoT Hub, sur la gauche, cliquez sur "*Devices*" puis sur "*Add Device*"

![IoTHub](pictures/006.png)

Donnez un nom à l áppareil, choisissez "*Symetric key*", cochez la case "*Auto-generate keys*" et vérifiez que l'option "*Enable*" est bien sélectionnée.

Cliquez sur "*Save*"

![IoTHub](pictures/007.png)

Cliquez sur "*Refresh*" pour voir votre appareil dans la liste


![IoTHub](pictures/008.png)

## Création des groupes de consommateurs

Les groupes de consommateurs sont une vue d’État du hub. Ils permettent à plusieurs applications consommatrices d’avoir chacune leur propre vue du flux d’événements et de lire le flux indépendamment. Cela signifie que lorsqu’une application cesse de lire à partir d’un flux d’événements, elle peut continuer là où elle s’est arrêté. Il est préférable pour chaque application d’avoir son propre groupe de consommateurs.

Sur la gauche, cliquez sur "Built-in endpoints", puis créez vos groupes de consommateurs.

Copiez la chaîne de connection "*Event Hub-compatible endpoint*" et conservez là dans un tableau. On s'en servira un peu plus tard.

![IoTHub](pictures/009.png)



## Creation d'un "SAS TOKEN"



Pour se connecter à notre IoT Hub avec IoT Explorer nous avons besoin d'une chaîne de connexion.

Sur la gauche cliquez sur "*Shared access policies*", "*iothubowner*", puis copiez une des chaînes de connexion.


![IoTHub](pictures/010.png)


Pour la création du jeton de connection, nous allons utiliser [IoT Explorer](https://github.com/Azure/azure-iot-explorer/releases).

Exécutez IoT Explorer et cliquez sur "Add connection" :

![IoTHub](pictures/011.png)

Collez la chaîne de connexion puis cliquez sur "Save"

![IoTHub](pictures/012.png)

Vous devez alors voir les appareils présents dans votre IoT Hub. Cliquez sur un des appareils :

![IoTHub](pictures/013.png)

Dans la section "Connection string with SAS token". choisissez "Primary key", définisez le nombre de minutes avec l'expiration du jeton puis cliquez sur "Generate".

Vous allez obtenir un jeton ressemblant à celui ci-dessous :

*HostName=IoTHub-Logictic-AEffacer.azure-devices.net;DeviceId=FranmerBuses;SharedAccessSignature=SharedAccessSignature sr=IoTHub-Logictic-AEffacer.azure-devices.net%2Fdevices%2FFranmerBuses&sig=Qq0GFga6kzgwHp0DldvpJbR1mwAwXMOAm6Lmc9UFBhQ%3D&se=2392799555*


Conservez uniquement la portion commençant par "*SharedAccessSignature sr=*" comme illustré ci-dessous :

*SharedAccessSignature sr=IoTHub-Logictic-AEffacer.azure-devices.net%2Fdevices%2FFranmerBuses&sig=Qq0GFga6kzgwHp0DldvpJbR1mwAwXMOAm6Lmc9UFBhQ%3D&se=2392799555*



Copiez la portion ainsi genérée puis collez-là. Nous allons en avoir besoin plus tard.

![IoTHub](pictures/014.png)

## Envoyer des évènements à Azure IoT Hub

Depuis Visual Studio code, ouvrez le fichier "*SendVehicleEvent.py*" puis renseignez les champs requis.

![IoTHub](pictures/015.png)

Pour la valeur "*YOUR FILE PATH*", allez dans le "*data*", puis faîtes un clique droit sur le fichier "BusPositionV2.csv" et cliquez sur "*Copy Path*" :

![IoTHub](pictures/016.png)