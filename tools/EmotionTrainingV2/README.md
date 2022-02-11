# Emotion Training
 
This program aims to train human face images to estimate face emotion.

## How to estimate?

This program uses ResNet-10. Input is 227x227 grayscale images.

## How to use?

## 1. Build

1. Open command prompt and change to &lt;EmotionTraining_dir&gt;
1. Type the following command
````
dotnet build -c Release
````
2. Copy ***DlibDotNetNative.dll*** and ***DlibDotNetNativeDnn.dll*** to output directory; &lt;EmotionTraining_dir&gt;\bin\Release\netcoreapp2.0.

## 2. Download train and test data

Download data from the following url.

- https://www.kaggle.com/sudarshanvaidya/corrective-reannotation-of-fer-ck-kdef
  - Corrective re-annotation of FER - CK+ - KDEF

And extract them and copy extracted files to directory where you want to.

## 3. Create dataset

Create your dataset from 300W-LP directory by using ***tools/GenerateTrainTestList.ps1***.
The following command generates train and test image list files into <dataset_dir>.

````cmd
pwsh tools\GenerateTrainTestList.ps1 -rootDirectory <dataset_dir> ^
                                     -trainingRatio 0.9
````

***-trainingRatio 0.9*** means that training data is 90% and test data is 10%.

## 4. Train

You must parepare `Models` directory of **FaceRecognitionDotNet** in <EmotionTraining_dir> before start training.

````cmd
cd <EmotionTraining_dir>
2022-02-06 17:27:34.0383 [INFO ]             Dataset: <EmotionTraining_dir> 
2022-02-06 17:27:34.2450 [INFO ]               Epoch: 3000 
2022-02-06 17:27:34.2450 [INFO ]       Learning Rate: 0.001 
2022-02-06 17:27:34.2450 [INFO ]   Min Learning Rate: 5E-09 
2022-02-06 17:27:34.2450 [INFO ]      Min Batch Size: 192 
2022-02-06 17:27:34.2450 [INFO ] Validation Interval: 1 
2022-02-06 17:27:34.2450 [INFO ]            Use Mean: False 
2022-02-06 17:27:34.2450 [INFO ]              Output: <EmotionTraining_dir>\result 
2022-02-06 17:27:34.2450 [INFO ]  
2022-02-06 17:27:34.2527 [INFO ] Start load train images 
2022-02-06 17:32:22.9455 [INFO ]             Dataset: <EmotionTraining_dir> 
2022-02-06 17:32:23.1567 [INFO ]               Epoch: 3000 
2022-02-06 17:32:23.1581 [INFO ]       Learning Rate: 0.001 
2022-02-06 17:32:23.1581 [INFO ]   Min Learning Rate: 5E-09 
2022-02-06 17:32:23.1581 [INFO ]      Min Batch Size: 192 
2022-02-06 17:32:23.1581 [INFO ] Validation Interval: 1 
2022-02-06 17:32:23.1581 [INFO ]            Use Mean: False 
2022-02-06 17:32:23.1581 [INFO ]              Output: <EmotionTraining_dir>\result 
2022-02-06 17:32:23.1581 [INFO ]  
2022-02-06 17:32:23.1581 [INFO ] Start load train images 
2022-02-06 17:32:49.4177 [INFO ] Load train images: 29568 
2022-02-06 17:32:49.4177 [INFO ] Start load test images 
2022-02-06 17:32:52.1883 [INFO ] Load test images: 3286 
2022-02-06 17:32:52.1883 [INFO ]  
2022-02-06 17:37:33.2620 [INFO ] Epoch: 0, learning Rate: 0.001, average loss: 1.83685714839136 
2022-02-06 17:38:13.9779 [INFO ] training num_right: 8214 
2022-02-06 17:38:13.9779 [INFO ] training num_wrong: 21354 

...

2022-02-06 23:22:20.0030 [INFO ] Epoch: 55, train accuracy: 0.996617965367965, test accuracy: 0.674071819841753 
2022-02-06 23:22:30.4702 [INFO ] Best Accuracy Model file is saved for train [0.996617965367965] 
2022-02-06 23:27:11.5971 [INFO ] Epoch: 56, learning Rate: 0.0001, average loss: 0.0236947398526919 
2022-02-06 23:27:52.3024 [INFO ] training num_right: 29460 
2022-02-06 23:27:52.3024 [INFO ] training num_wrong: 108 
2022-02-06 23:27:52.3024 [INFO ] training accuracy:  0.996347402597403 
2022-02-06 23:27:56.4852 [INFO ] testing num_right: 2231 
2022-02-06 23:27:56.4852 [INFO ] testing num_wrong: 1055 
2022-02-06 23:27:56.4852 [INFO ] testing accuracy:  0.678940961655508 
2022-02-06 23:28:25.0470 [INFO ]              Output: <EmotionTraining_dir>\result 
2022-02-06 23:28:25.0945 [INFO ]  
````

## 6. Test (Option)

You must parepare `Models` directory of **FaceRecognitionDotNet** in <EmotionTraining_dir> before start test.

````cmd
cd <EmotionTraining_dir>
dotnet run -c Debug -- test --dataset <dataset_dir> ^
                            --model "Corrective_re-annotation_of_FER_CK+_KDEF-mlp_3000_5E-05_5E-09_512.dat"

2022-02-08 00:40:45.9466 [INFO ] Dataset: <EmotionTraining_dir>
2022-02-08 00:40:45.9976 [INFO ]   Model: <EmotionTraining_dir>\result\Corrective_re-annotation_of_FER_CK+_KDEF-cnn_3000_0.001_5E-09_192_test_best_0.681679853925746.dat
2022-02-08 00:40:45.9976 [INFO ]
2022-02-08 00:40:45.9976 [INFO ] Start load train images
2022-02-08 00:41:09.5241 [INFO ] Load train images: 29568
2022-02-08 00:41:09.5241 [INFO ] Start load test images
2022-02-08 00:41:12.0637 [INFO ] Load test images: 3286
2022-02-08 00:41:12.0637 [INFO ]
2022-02-08 00:41:51.9729 [INFO ] training num_right: 23754
2022-02-08 00:41:51.9729 [INFO ] training num_wrong: 5814
2022-02-08 00:41:51.9729 [INFO ] training accuracy:  0.803368506493506
2022-02-08 00:41:55.9163 [INFO ] testing num_right: 2240
2022-02-08 00:41:55.9181 [INFO ] testing num_wrong: 1046
2022-02-08 00:41:55.9181 [INFO ] testing accuracy:  0.681679853925746
````

## 7. Evaluation

### Best accuracy model for training data

* Training Data: 29568
* Test Data: 3286
* Epoch: 55
* Learning Rate: 0.001
* Min Learning Rate: 5E-09
* Min Batch Size: 192
* training accuracy: 0.996617965367965
* testing accuracy: 0.674071819841753

### Best accuracy model for test data

* Training Data: 29568
* Test Data: 3286
* Epoch: 55
* Learning Rate: 0.001
* Min Learning Rate: 5E-09
* Min Batch Size: 192
* training accuracy: 0.803368506493506
* testing accuracy: 0.681679853925746