import os
import sys
import string
#from gensim import corpora, models, similarities
from collections import defaultdict
import nltk
import csv
import numpy as np
import itertools
import matplotlib.pyplot as plt
from sklearn.multiclass import OneVsOneClassifier
from sklearn import cross_validation
from sklearn.cluster import KMeans
from sklearn.feature_extraction.text import TfidfVectorizer
from nltk.stem.porter import PorterStemmer
from nltk.parse import stanford
from sklearn import svm
from sklearn.svm import LinearSVC
from sklearn.metrics import f1_score
from sklearn.metrics import accuracy_score
from sklearn.feature_selection import SelectKBest, chi2
from sklearn.linear_model import RidgeClassifier
from sklearn.svm import LinearSVC
from sklearn.linear_model import SGDClassifier
from sklearn.linear_model import Perceptron
from sklearn.linear_model import PassiveAggressiveClassifier
from sklearn.naive_bayes import BernoulliNB, MultinomialNB
from sklearn.neighbors import KNeighborsClassifier
from sklearn.neighbors import NearestCentroid
from sklearn.decomposition import PCA
import random
# used to persist the model without having to retrain
# using joblib instead of python's built-in pickle more efficient for
# certain cases
from sklearn.externals import joblib
from sklearn.discriminant_analysis import LinearDiscriminantAnalysis
from sklearn.discriminant_analysis import QuadraticDiscriminantAnalysis
from sklearn.neighbors import KNeighborsClassifier
from sklearn.ensemble import RandomForestClassifier, AdaBoostClassifier
from sklearn.svm import SVC

import os


class MLClass:

    slotList = []
    stoplist = set('for a of the and to in'.split())
    slotName = []
    actionList = ['reqcolor', 'provide',
                  'directmove', 'acknowledge', 'reqobject']
    number1List = ['two', 'three', 'four', 'five', 'six', 'seven', 'eight']
    number2List = ['2', '3', '4', '5', '6', '7', '8']
    wordArray = []
    posArray = []
    bigramArray = []
    responseList = []
    dialogueActs = len(slotList) * ['']  # the detailed information
    currentState = -1  # the current state, request, provide, direct, acknowledge
    SlotfileName = os.getcwd() + '/SlotValues.txt'
    responseFileName = os.getcwd() + '/SysResponsesNew.txt'

    # download nltk data as necessary
    nltk.download('punkt')
    nltk.download('averaged_perceptron_tagger')

    # for NLTK 3.1 and above, 'STANFORD_PARSER' env var no longer used?
    os.environ['STANFORD_MODELS'] = 'C:/NLTK/stanford-jars'
    os.environ['STANFORD_PARSER'] = 'C:/NLTK/stanford-jars'
    os.environ['JAVA_HOME'] = 'C:/Program Files/Java/jre1.8.0_102/bin'
    parser = stanford.StanfordParser(
        model_path='C:/NLTK/stanford-jars/englishPCFG.ser.gz')
    thred = 3
    dependent_node = []
    stemmer = PorterStemmer()
    sentenceArray = []
    featureList = []
    actionArray = []
    depArray = []

    def stem_tokens(self, tokens, stemmer):
        stemmed = []
        for item in tokens:
            # stemmed.append(stemmer.stem(item))
            stemmed.append(item)
        return stemmed

    # returns stems of tokenized, lowercase words
    def tokenize(self, text):
        filtered = []
        tokens = nltk.word_tokenize(text)
        # stopwords = nltk.corpus.stopwords.words('english')     #what, which,
        # can are stopwords
        stopwords = ['the', 'a', 'and', 'but',
                     'because', 'as', 'with', 'to', 'out']
        filtered = [w.lower() for w in tokens if not w.lower() in stopwords]
        stems = self.stem_tokens(filtered, self.stemmer)
        return stems

    def __init__(self):
        self.getSlotValues()
        self.getResponseActions()

    def getSlotValues(self):
        with open(self.SlotfileName) as fo:
            lines = fo.readlines()
            for line in lines:
                self.slotList.append(self.readOneSlot(line))

    def readOneSlot(self, string):
        slotVa = []
        if len(string) is 0:
            return slotVa
        startIdx = string.index(':')
        self.slotName.append(string[1:startIdx])
        if startIdx > 0:
            _str = string[startIdx + 2: len(string) - 2]
            slotVa = [slot for slot in _str.split('|')]
        return slotVa

    def slotValue(self, word):
        for slot in self.slotList:
            if word in slot:
                return self.slotList.index(slot)
        return -1

    def getResponseActions(self):
        with open(self.responseFileName) as fo:
            lines = fo.readlines()
            for line in lines:
                self.responseList.append(self.readOneResponse(line))

    def readOneResponse(self, string):
        RespVa = []
        if len(string) is 0:
            return RespVa
        startIdx = string.index(':')
        if startIdx > 0:
            _str = string[startIdx + 1: len(string) - 1]
            RespVa = [slot for slot in _str.split('|')]
        return RespVa

    def createMLmodel(self, fileName):

        # read the file to store the initial information
        # process sentence by sentence
        for line in open(fileName):
            line = str(line)
            # remove "Child ..."
            temp1 = [word for word in line.lower().split(':')]
            if(len(temp1) < 2):
                continue
            # split action from sentence
            temp2 = [word for word in temp1[1].split(']')]
            temp3 = [word for word in temp2[0].split('[')]
            if(len(temp3) < 2):
                continue
            temp4 = [word for word in temp3[1].split('(')]
            # get and record the action
            action = self.actionList.index(temp4[0])
            self.actionArray.append(action)
            # get words of a sentence
            sentence = temp2[1]
            sentence.translate(None, string.punctuation)
            #sentence = unicode(sentence, 'utf-8')

            # add sentence to array
            self.sentenceArray.append(sentence)

            # step1: extract word features into array, no duplicates
            words = self.tokenize(sentence)
            for word in words:
                if word not in self.wordArray:
                    self.wordArray.append(word)

            # step2: extract bigram features into array, no duplicates
            bigrams = self.find_bigrams(sentence)
            for bis in bigrams:
                if bis not in self.bigramArray:
                    self.bigramArray.append(bis)

            # step3: extract pos(part of speech) features into array
            # no duplicates
            parserF = self.parserFeature(sentence)
            for pars in parserF:
                if pars not in self.posArray:
                    self.posArray.append(pars)

            '''
            #step4: get the dependence feature
            depFs = self.dependentFeature(sentence)
            for item in depFs:
                if item not in self.depArray:
                    self.depArray.append(item)
            '''
        # write all data to output files ----------------------
        wordFile = open(os.getcwd() + '/wordArray.txt', 'w')
        for item in self.wordArray:
            wordFile.write("%s\n" % item)
        wordFile.close()
        biFile = open(os.getcwd() + '/bigramArray.txt', 'w')
        for item in self.bigramArray:
            biFile.write("%s\n" % item)
        biFile.close()
        posFile = open(os.getcwd() + '/posArray.txt', 'w')
        for item in self.posArray:
            posFile.write("%s\n" % item)
        posFile.close()
        '''
        depFile = open(os.getcwd() + '/depArray.txt', 'w')
        for item in self.depArray:
            depFile.write("%s\n" % item)
        depFile.close()
        '''
        # -----------------------------------------------------
        fList = []
        for sentence in self.sentenceArray:
            fList.append(self.convert2Feature(sentence))

        # NOTE: .pkl files are python pickled files.

        # reduce the feature dimension
        pca = PCA(n_components=5)
        pca.fit(fList)
        self.featureList = pca.transform(fList)
        # save
        joblib.dump(pca, os.getcwd() + '/pcaModel.pkl')

        # classification methods
        clf = SVC(gamma=2, C=1)
        clf.fit(self.featureList, self.actionArray)
        # save
        joblib.dump(clf, os.getcwd() + '/clasModel.pkl')

    def loadclassModel(self):
        clf = joblib.load(os.getcwd() + '/clasModel.pkl')
        return clf

    def loadpcaModel(self):
        pca = joblib.load(os.getcwd() + '/pcaModel.pkl')
        return pca

    def loadOtherInfor(self):
        self.wordArray = [line.rstrip('\n') for line in open(
            os.getcwd() + '/wordArray.txt')]
        self.bigramArray = [line.rstrip('\n') for line in open(
            os.getcwd() + '/bigramArray.txt')]
        self.posArray = [line.rstrip('\n')
                         for line in open(os.getcwd() + '/posArray.txt')]
        #self.depArray = [line.rstrip('\n') for line in open(os.getcwd() + '/depArray.txt')]

    # all_node includes all the nodes will be used
    # node_list includes the nodes with highest depth
    def traverse_deep(self, node_list):
        child_list = []
        # step1: record all the child nodes
        for node in node_list:
            for child in node:
                if isinstance(child, unicode) or isinstance(child, str):
                    return
                child_list.append(child)
        # step2: record the child nodes if not enough
        for node in child_list:
            self.dependent_node.append(node.label())
            if len(self.dependent_node) == self.thred:
                return
        self.traverse_deep(child_list)

    def parserFeature(self, sentence):
        # get the tag of pos
        # tags: https://catalog.ldc.upenn.edu/docs/LDC99T42/tagguid1.pdf
        sent_pos_tagger = nltk.pos_tag(sentence)
        results = []
        for pos in sent_pos_tagger:
            results.append(pos[1])
        return results

    def dependentFeature(self, sentence):
        tree_structure = self.parser.raw_parse(sentence)
        tree_list = list(tree_structure)
        self.dependent_node = []
        self.traverse_deep(tree_list)
        return self.dependent_node

    def MLPredict(self, model, x_test):
        #np.array(x_test).reshape(1, -1)
        y_result = model.predict(x_test)
        return y_result

    def find_bigrams(self, sentence):
        words = self.tokenize(sentence)
        bigram_list = []
        for i in range(len(words) - 1):
            bigram_list.append((words[i] + ' ' + words[i + 1]))
        return bigram_list

    def convert2Feature(self, sentence):
        # replace, convert numbers to words
        for i in range(0, len(self.number2List)):
            sentence = sentence.replace(
                self.number2List[i], self.number1List[i])

        # QUESTION: didn't we already call tokenize() in createMLmodel()??
        words = self.tokenize(sentence)
        # step1: get the word feature
        state1 = [0] * len(self.wordArray)
        # assign sentence presentation
        for word in words:
            if word in self.wordArray:
                idx = self.wordArray.index(word)
                state1[idx] = 1

        # step1.5: get the bi-grams feature
        bigrams = self.find_bigrams(sentence)
        state15 = [0] * len(self.bigramArray)
        # assign sentence presentation
        for word in bigrams:
            if word in self.bigramArray:
                idx = self.bigramArray.index(word)
                state15[idx] = 1

        # step2: get the pos feature
        pars = self.parserFeature(sentence)
        state2 = [0] * len(self.posArray)
        for parFeature in pars:
            if parFeature in self.posArray:
                idx = self.posArray.index(parFeature)
                state2[idx] = 1

        '''
        #step3: get the dependence feature
        depFs = self.dependentFeature(sentence)
        state3 = [0] * len(self.depArray)
        for depFeature in depFs:
            if depFeature in self.depArray:
                idx = self.depArray.index(depFeature)
                state3[idx] = 1
        '''
        results = []
        for feature in state1:
            results.append(feature)
        for feature in state15:
            results.append(feature)
        for feature in state2:
            results.append(feature)
        # for feature in state3:
        #    results.append(feature)

        return results

    def processFeature(self, pcaModel, feature):
        #np.array(feature).reshape(1, -1)
        newF = pcaModel.transform(feature)
        return newF

    def resetState(self):
        self.currentState = -1
        self.dialogueActs = len(self.slotList) * ['']

    def getResponse(self, _act, sentence):
        responseUtterance = 'act:' + str(_act) + ','

        inforSlot = ["" for x in range(len(self.slotList))]
        words = self.tokenize(sentence)
        # words = [word for word in sentence.split() if word not in self.stoplist]
        # replace word with slot
        for word in words:
            # in slot ?
            slotNum = self.slotValue(word)
            if(slotNum != -1):
                inforSlot[slotNum] = word
        # replace bi-word with slot
        for i in range(len(words) - 1):
            str1 = words[i] + ' ' + words[i + 1]
            slotNum = self.slotValue(str1)
            if(slotNum != -1):
                inforSlot[slotNum] = str1

        if self.slotName[0] not in responseUtterance:
            for i in range(0, len(inforSlot)):
                responseUtterance += self.slotName[i] + ':'
                if inforSlot[i] is '':
                    responseUtterance += 'Requ' + ','
                else:
                    responseUtterance += inforSlot[i] + ','

        print "response Utterance is: "
        print responseUtterance
        return responseUtterance
