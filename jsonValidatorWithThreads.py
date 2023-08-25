import sys
import threading
import json
import csv
import re

experimentJson = "exp.json"
configurationCSV = "config.csv"
isValidJson = True

def checkRule(key, value, rule, errorMessage):
    isValid = re.match(rule,value)
    if not isValid:
        print ("Invalid value : \n" + key + ":" + value + "\nErrorMessage : \n" + errorMessage + "\n" )

def validateUsingConfigurationFile(key, value):
    threads = []
    with open(configurationCSV, "r") as fi:
        configurationData = csv.reader(fi)
        for row in configurationData:
            if key == row[0]:
                checkRuleThread = threading.Thread(target=checkRule, args=(key, value, row[1], row[2]))
                checkRuleThread.start()
                threads.append(checkRuleThread)
    for thread in threads:
        thread.join()

def printErrors(jsonData):
    threads = []
    if not isinstance(jsonData,list):
        for key, value in jsonData.items():
            if isinstance(value, dict) or isinstance(value,list):
                printErrorThread = threading.Thread(target=printErrors, args=(value,))
                printErrorThread.start()
                threads.append(printErrorThread)
            else:
                validationThread = threading.Thread(target=validateUsingConfigurationFile, args=(key, value))
                validationThread.start()
                threads.append(validationThread)
    else:
        for value in jsonData:
            if isinstance(value, dict) or isinstance(value,list):
                printErrorThread = threading.Thread(target=printErrors, args=(value,))
                printErrorThread.start()
                threads.append(printErrorThread)

    for thread in threads:
        thread.join()

with open(experimentJson, "r") as f:
    experimentData = json.load(f)
printErrors(experimentData)