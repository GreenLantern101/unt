from MyNetClass import MyNetClass
from MLClass import MLClass
import sys
import unicodedata
import datetime
import string

import os

# used for actual game-play
class setup:

    TrainfileName = os.getcwd() + '/Collaborative_Game1.txt'
    logFileName = os.getcwd() + 'logFile.txt'
    # train the model

    def __init__(self):
        self.MLInstance = MLClass()
        self.classModel = self.MLInstance.loadclassModel()
        self.pcaModel = self.MLInstance.loadpcaModel()
        self.MLInstance.loadOtherInfor()
        self.netInstance = MyNetClass()
        self.netInstance.createConnection()
        self.Update()

    def Update(self):
        LoopFlag = True
        speechData = ''
        while LoopFlag:
            # get a string from speech recognition
            #speechData = self.netInstance.receiveSpeechData()
            speechData = input()
            if len(speechData) > 0:
                fp = open(self.logFileName, 'a')
                speechDataAscii = speechData.decode('utf-8')
                speechDataAscii = string.replace(speechData, 'to', 'two')
                fp.write('time; ' + str(datetime.datetime.now().time()
                                        ) + ', received data; ' + speechDataAscii + '\n')
                featureV = self.MLInstance.convert2Feature(speechDataAscii)
                featureVector = self.MLInstance.processFeature(
                    self.pcaModel, featureV)
                actCategory = self.MLInstance.MLPredict(
                    self.classModel, featureVector)
                actList = actCategory.tolist()  # change the type from numpy.ndarray to list
                act = actList[0]
                sDataInstance = self.MLInstance.getResponse(
                    act, speechDataAscii)
                print sDataInstance
                # responseData = sDataInstance.encode("utf-16") #need to
                # convert to utf16 before send to C#
                fp.write('time; ' + str(datetime.datetime.now().time()
                                        ) + ', send data; ' + sDataInstance + '\n')
                self.netInstance.sendGameData(sDataInstance)
                self.MLInstance.resetState()
                fp.close()


instance = setup()
