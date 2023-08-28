import csv
import json
import requests
import time
from datetime import date
from datetime import datetime


today = date.today()
d1 = today.strftime("%d/%m/%Y")
now = datetime.now()
current_time = now.strftime("%H:%M:%S")
timeStamp = datetime.now().timestamp()

# Replace the following variables with your information
sas = '<YOUR DEVICE SAS TOKEN>'
iotHub = '<YOUR IOT HUB NAME>'
deviceId = '<YOUR DEVICE NAME>'


jsondata ={}
vehicle ={}
trip ={}
position ={}


api = '2018-06-30'
restUri = "https://"+iotHub+".azure-devices.net/devices/"+deviceId+"/messages/events?api-version="+api
count = 0
with open('YOUR FILE PATH') as f:
    reader = csv.reader(f)
    next(reader, None)
    for row in reader:
        today = date.today()
        d1 = today.strftime("%d/%m/%Y")
        now = datetime.now()
        current_time = now.strftime("%H:%M:%S")
        timeStamp = datetime.now().timestamp()
        count = count + 1
        payload = {"id": row[7], "vehicle" : {"trip":{"tripId": row[8],"startTime":current_time ,"startDate":d1,"routeId":row[11]},"position":{"latitude":row[2],"longitude":row[3],"bearing":219,"speed": row[12]},"currentStopSequence": row[13],"currentStatus":row[14],"timestamp": timeStamp,"vehicle": {"id":row[7]},"occupancyStatus": row[16]}}
        r = requests.post(restUri, json=payload, headers = {'Authorization':sas})        
        print(payload)

        #json_data = json.dumps(payload)
        #r = requests.post(restUri, json=json_data, headers = {'Authorization':sas})
        #print(json_data)

        time.sleep(0.3)
