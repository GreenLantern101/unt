# -*- coding: utf-8 -*-
# Code source: Gaël Varoquaux
#              Andreas Müller
# Modified for documentation by Jaques Grobler
# License: BSD 3 clause

import numpy as np
import matplotlib.pyplot as plt
from matplotlib.colors import ListedColormap
from sklearn.cross_validation import train_test_split
from sklearn.preprocessing import StandardScaler
from sklearn.datasets import make_moons, make_circles, make_classification
from sklearn.neighbors import KNeighborsClassifier
from sklearn.svm import SVC
from sklearn.tree import DecisionTreeClassifier
from sklearn.ensemble import RandomForestClassifier, AdaBoostClassifier
from sklearn.naive_bayes import GaussianNB
from sklearn.discriminant_analysis import LinearDiscriminantAnalysis
from sklearn.discriminant_analysis import QuadraticDiscriminantAnalysis
from MLClass import MLClass

import os

h = .02  # step size in the mesh

names = ["Nearest Neighbors", "Linear SVM", "RBF SVM", "Decision Tree",
         "Random Forest", "AdaBoost", "Naive Bayes", "Linear Discriminant Analysis",
         "Quadratic Discriminant Analysis"]

# play around with params?
'''
SVMs: A low C makes the decision surface smooth,
while a high C aims at classifying all training examples correctly.
gamma defines how much influence a single training example has.
The larger gamma is, the closer other examples must be to be affected.
'''
classifiers = [
    KNeighborsClassifier(n_neighbors = 3),
    SVC(kernel="linear", C=0.025),
    SVC(gamma=2, C=1),
    DecisionTreeClassifier(max_depth=5),
    RandomForestClassifier(max_depth=5, n_estimators=10, max_features=1),
    AdaBoostClassifier(),
    GaussianNB(),
    LinearDiscriminantAnalysis(),
    QuadraticDiscriminantAnalysis()]

# get the data
InsML = MLClass()
TrainfileName = os.getcwd() + '/Collaborative_Game.txt'
InsML.createMLmodel(TrainfileName)

for i in range(0, 10):
    # preprocess dataset, split into training and test part
    X = InsML.featureList
    y = InsML.actionArray
    X = StandardScaler().fit_transform(X)
    # http://scikit-learn.org/stable/modules/cross_validation.html
    X_train, X_test, y_train, y_test = train_test_split(
        X, y, test_size=.3, random_state=42)

    # iterate over classifiers
    for name, clf in zip(names, classifiers):
        clf.fit(X_train, y_train)
        score = clf.score(X_test, y_test)
        print str(name) + ": " + str(score)
