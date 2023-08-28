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
