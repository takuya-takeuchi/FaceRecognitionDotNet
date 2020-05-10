#!/usr/bin/env python
# -*- coding: utf-8 -*-

import argparse
import os
import matplotlib.pyplot as plt
import pandas as pd
import seaborn as sns
import scipy.io as sio
import numpy as np

def get_args():
    parser = argparse.ArgumentParser()
    parser.add_argument('dataset')
    return parser.parse_args()

def find_all_mat_files(directory):
    for root, _, files in os.walk(directory):
        for file in files:
            base, ext = os.path.splitext(file)
            if ext == ".mat" and not base.endswith("_pts"):
                yield os.path.join(root, file)

# https://github.com/natanielruiz/deep-head-pose/blob/master/code/utils.py
def get_pose_params_from_mat(mat_path):
    # This functions gets the pose parameters from the .mat
    # Annotations that come with the Pose_300W_LP dataset.
    mat = sio.loadmat(mat_path)
    # [pitch yaw roll tdx tdy tdz scale_factor]
    pre_pose_params = mat['Pose_Para'][0]
    # Get [pitch, yaw, roll, tdx, tdy]
    pose_params = pre_pose_params[:5]
    return pose_params

def get_ypr_from_mat(mat_path):
    # Get yaw, pitch, roll from .mat annotation.
    # They are in radians
    mat = sio.loadmat(mat_path)
    # [pitch yaw roll tdx tdy tdz scale_factor]
    pre_pose_params = mat['Pose_Para'][0]
    # Get [pitch, yaw, roll]
    pose_params = pre_pose_params[:3]
    return pose_params

def main():
    args = get_args()

    # Import data
    hist = dict()
    hist["train"] = dict()
    hist["test"] = dict()
    hist["train"]["pitch"] = list()
    hist["train"]["yaw"] = list()
    hist["train"]["roll"] = list()
    hist["test"]["pitch"] = list()
    hist["test"]["yaw"] = list()
    hist["test"]["roll"] = list()

    bins = list()
    for n in range(1, 68):
        v = -99 + (n - 1) * 3
        bins.append(v)

    for t in ["train", "test"]:
        path = os.path.join(args.dataset, t)
        for f in find_all_mat_files(path):
            # pose_params: [pitch, yaw, roll]
            pose_params = get_ypr_from_mat(f)

            # https://github.com/natanielruiz/deep-head-pose/blob/master/code/datasets.py
            pitch = pose_params[0] * 180 / np.pi
            yaw = pose_params[1] * 180 / np.pi
            roll = pose_params[2] * 180 / np.pi

            for k, v in {"pitch":pitch, "yaw":yaw, "roll":roll}.items():
                label = -1
                for i in range(1, len(bins)):
                    if (bins[i - 1] <= v and v < bins[i]):
                        label = i - 1
                        break

                if label == -1:
                    print("{} of {} is out of range!!".format(k, f))
                    continue

                hist[t][k].append(bins[label])

    # Plot
    fig, ((trainP, trainY, trainR), (testP, testY, testR)) = plt.subplots(ncols=3, nrows=2, figsize=(18,8), dpi= 80, sharex=False, sharey=True)

    kwargs = dict(hist_kws={'alpha':.6}, kde_kws={'linewidth':2})
    
    sns.distplot(hist["train"]["pitch"], kde=False, color="crimson",    label="Pitch", **kwargs, ax=trainP)
    sns.distplot(hist["train"]["yaw"],   kde=False, color="dodgerblue", label="Yaw",   **kwargs, ax=trainY)
    sns.distplot(hist["train"]["roll"],  kde=False, color="lawngreen",  label="Roll",  **kwargs, ax=trainR)
    sns.distplot(hist["test"]["pitch"],  kde=False, color="crimson",    label="Pitch", **kwargs, ax=testP)
    sns.distplot(hist["test"]["yaw"],    kde=False, color="dodgerblue", label="Yaw",   **kwargs, ax=testY)
    sns.distplot(hist["test"]["roll"],   kde=False, color="lawngreen",  label="Roll",  **kwargs, ax=testR)

    trainP.set_xlim(-100,100)
    trainY.set_xlim(-100,100)
    trainR.set_xlim(-100,100)
    testP.set_xlim(-100,100)
    testY.set_xlim(-100,100)
    testR.set_xlim(-100,100)
    trainP.legend()
    trainY.legend()
    trainR.legend()
    testP.legend()
    testY.legend()
    testR.legend()
    trainP.set_title('Angle count by pitch (Train)')
    trainY.set_title('Angle count by yaw (Train)')
    trainR.set_title('Angle count by roll (Train)')
    testP.set_title('Angle count by pitch (Test)')
    testY.set_title('Angle count by yaw (Test)')
    testR.set_title('Angle count by roll (Test)')

    plt.subplots_adjust(wspace=0.15, hspace=0.2)

    fig.savefig("pose-hist.png", bbox_inches='tight')
    plt.show()

if __name__ == '__main__':
    main()