import sys
import json
import csv
import re

experimentJson = sys.argv[1]
configurationCSV = sys.argv[2]
flag = True

def printErrors(jsonData):
    if not isinstance(jsonData,list):
        for key, value in jsonData.items():
            if isinstance(value, dict) or isinstance(value,list):
                printErrors(value)
            else:
                with open(configurationCSV, "r") as fi:
                    configurationData = csv.reader(fi)
                    for row in configurationData:
                        if key == row[0]:
                            isValid = re.match(row[1], value)
                            if not isValid:
                                print ("Invalid value : \n" + key + ":" + value )
                                print ("ErrorMessage : \n" + row[2] + "\n")
                                global flag
                                flag = False
    else:
        for value in jsonData:
            if isinstance(value, dict) or isinstance(value,list):
                printErrors(value)

with open(experimentJson, "r") as f:
    experimentData = json.load(f)
printErrors(experimentData)
if flag == True:
    print ("It is valid json file!")