#!/usr/bin/env python
# -*- coding: utf-8 -*-

import argparse
import os
import matplotlib.pyplot as plt
import pandas as pd
import seaborn as sns

def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument('dataset')
    return parser.parse_args()

def main():
    args = get_args()

    sns.set_style("white")

    #df['Gender'].value_counts().plot(kind="bar", color=['dodgerblue', 'deeppink'])
    #plt.title('Face count by gender')

    # Import data
    train = os.path.join(args.dataset, "train.csv")
    test = os.path.join(args.dataset, "test.csv")
    train_df = pd.read_csv(train)
    test_df = pd.read_csv(test)

    # Plot
    fig, (axL, axR) = plt.subplots(ncols=2, figsize=(12,4), dpi= 80, sharex=True, sharey=True)
    
    train_df['Gender'].value_counts().plot(kind="bar", color=['dodgerblue', 'deeppink'], ax=axL, alpha=0.6)
    test_df['Gender'].value_counts().plot(kind="bar", color=['dodgerblue', 'deeppink'],  ax=axR, alpha=0.6)

    axL.set_title('Face count by gender (Train)')
    axR.set_title('Face count by gender (Test)')

    plt.subplots_adjust(wspace=0.05, hspace=0)

    fig.savefig("gender-hist.png", bbox_inches='tight')
    plt.show()

if __name__ == '__main__':
    main()