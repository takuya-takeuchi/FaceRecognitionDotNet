#!/usr/bin/env python
# -*- coding: utf-8 -*-

import argparse
import os
import re
import numpy as np
import matplotlib.pyplot as plt
import pandas as pd
import seaborn as sns

def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument('log')
    return parser.parse_args()

def main():
    args = get_args()

    sns.set_style("white")

    # Import data
    log = args.log

    trainx = []
    trainy_lr = []
    trainy_loss = []
    valx = []    
    valy_train = []    
    valy_test = []

    trainPattern = re.compile('Epoch: ([0-9]+), learning Rate: ([0-9\.]+), average loss: ([0-9\.]+)')
    valPattern = re.compile('Epoch: ([0-9]+), train accuracy: ([0-9\.]+), test accuracy: ([0-9\.]+)')
    with open(log, "r", encoding="utf-8") as f:
        for line in f:
            ret = trainPattern.match(line)
            if ret:
                trainx.append(int(ret.group(1)))
                trainy_lr.append(float(ret.group(2)))
                trainy_loss.append(float(ret.group(3)))
            ret = valPattern.match(line)
            if ret:
                valx.append(int(ret.group(1)))
                valy_train.append(float(ret.group(2)))
                valy_test.append(float(ret.group(3)))
    
    kwargs = dict(kde_kws={'linewidth':2})

    # Plot
    fig, (axL, axR) = plt.subplots(ncols=2, figsize=(12,4), dpi= 80, sharex=True, sharey=True)
    
    trainx = np.array(trainx)
    trainy_lr = np.array(trainy_lr)
    trainy_loss = np.array(trainy_loss)
    valx = np.array(valx)
    valy_train = np.array(valy_train)
    valy_test = np.array(valy_test)

    #sns.lineplot(x=trainx, y=trainy_lr,   color="dodgerblue", label="Learning Rate",  alpha=0.6, ax=axL)
    sns.lineplot(x=trainx, y=trainy_loss, color="orange",     label="Average Loss",   alpha=0.6, ax=axL)
    sns.lineplot(x=valx,   y=valy_train,  color="crimson",    label="Train Accuracy", alpha=0.6, ax=axR)
    sns.lineplot(x=valx,   y=valy_test,   color="dodgerblue", label="Test Accuracy",  alpha=0.6, ax=axR)

    axL.legend()
    axR.legend()
    axL.set_title('Training')
    axR.set_title('Validation')    
    axL.set(xlabel='Epoch', ylabel='Loss')
    axR.set(xlabel='Epoch', ylabel='Accuracy')

    plt.subplots_adjust(wspace=0.1, hspace=0)

    fig.savefig("visualize-log.png", bbox_inches='tight')
    plt.show()

if __name__ == '__main__':
    main()