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

    # Import data
    train = os.path.join(args.dataset, "train.csv")
    test = os.path.join(args.dataset, "test.csv")
    train_df = pd.read_csv(train)
    train_x1 = train_df.loc[train_df.Gender=='Male',   'Age']
    train_x2 = train_df.loc[train_df.Gender=='Female', 'Age']
    test_df = pd.read_csv(test)
    test_x1 = test_df.loc[test_df.Gender=='Male',   'Age']
    test_x2 = test_df.loc[test_df.Gender=='Female', 'Age']
    
    kwargs = dict(hist_kws={'alpha':.6}, kde_kws={'linewidth':2})

    # Plot
    fig, (axL, axR) = plt.subplots(ncols=2, figsize=(12,4), dpi= 80, sharex=True, sharey=True)

    sns.distplot(train_x1, kde=False, color="dodgerblue", label="Male", **kwargs,   ax=axL)
    sns.distplot(train_x2, kde=False, color="deeppink",   label="Female", **kwargs, ax=axL)
    sns.distplot(test_x1,  kde=False, color="dodgerblue", label="Male", **kwargs,   ax=axR)
    sns.distplot(test_x2,  kde=False, color="deeppink",   label="Female", **kwargs, ax=axR)

    axL.set_xlim(0,116)
    axL.legend()
    axR.legend()
    axL.set_title('Face count by age and gender (Train)')
    axR.set_title('Face count by age and gender (Test)')

    plt.subplots_adjust(wspace=0.05, hspace=0)

    fig.savefig("age-by-gender-hist.png", bbox_inches='tight')
    plt.show()

if __name__ == '__main__':
    main()