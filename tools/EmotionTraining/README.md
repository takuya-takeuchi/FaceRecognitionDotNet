# Emotion Training
 
This program aims to train human face images to estimate face emotion.

## How to estimate?

This program implements [Facial Expression Recognition using Facial Landmark Detection and Feature Extraction via Neural Networks](https://arxiv.org/pdf/1812.04510.pdf).
Please check this paper about algorithm.

##### :warning: Warning

This program does not reproduce paper's performance.

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
dotnet run -c Release -- train --dataset <dataset_dir> ^
                               --epoch 3000 ^
                               --lr 0.0005 ^
                               --min-lr 0.000000005 ^
                               --min-batchsize 512 ^
                               --validation-interval 30 ^
                               --output <dataset_dir>\result
            Dataset: <dataset_dir>
              Epoch: 3000
      Learning Rate: 0.0005
  Min Learning Rate: 5E-09
     Min Batch Size: 512
Validation Interval: 30
           Use Mean: False
             Output: <dataset_dir>\result

Start load train images
Use Cache <dataset_dir>\train_cache.dat
Load train images: 29568
Start load test images
Use Cache <dataset_dir>\test_cache.dat
Load test images: 3286

step#: 0     learning rate: 0.0005  average loss: 0            steps without apparent progress: 0
Epoch: 0, learning Rate: 0.0005, average loss: 4.11948018428823
Epoch: 1, learning Rate: 0.0005, average loss: 3.1305690457948
Epoch: 2, learning Rate: 0.0005, average loss: 2.69736696763001
Epoch: 3, learning Rate: 0.0005, average loss: 2.45950934526035
Epoch: 4, learning Rate: 0.0005, average loss: 2.30691530587521
Epoch: 5, learning Rate: 0.0005, average loss: 2.19935585556312
Epoch: 6, learning Rate: 0.0005, average loss: 2.11859311641417
Epoch: 7, learning Rate: 0.0005, average loss: 2.0555247708289
Epoch: 8, learning Rate: 0.0005, average loss: 2.00418632351514
Epoch: 9, learning Rate: 0.0005, average loss: 1.96132853099646
Epoch: 10, learning Rate: 0.0005, average loss: 1.92488743458344
Epoch: 11, learning Rate: 0.0005, average loss: 1.89341635029666
Epoch: 12, learning Rate: 0.0005, average loss: 1.86622101266595
Epoch: 13, learning Rate: 0.0005, average loss: 1.84257901432909
Epoch: 14, learning Rate: 0.0005, average loss: 1.82172028172718
Epoch: 15, learning Rate: 0.0005, average loss: 1.80331000603231
Epoch: 16, learning Rate: 0.0005, average loss: 1.78676114864211

Epoch: 384, learning Rate: 5E-09, average loss: 1.39896950052756
Saved state to Corrective_re-annotation_of_FER_CK+_KDEF-mlp_3000_0.0005_5E-09_512
Epoch: 385, learning Rate: 5E-09, average loss: 1.39896063521396
Epoch: 386, learning Rate: 5E-10, average loss: 1.39897608550845
Saved state to Corrective_re-annotation_of_FER_CK+_KDEF-mlp_3000_0.0005_5E-09_512_
done training
training num_right: 14257
training num_wrong: 15311
training accuracy:  0.482176677489177
testing num_right: 1703
testing num_wrong: 1583
testing accuracy:  0.518259281801582                    00:00:27
───────────────────────────────────────────────────────────────────
100.00% Step 4000 of 4000                                  00:00:00
───────────────────────────────────────────────────────────────────
training num_right: 4000
training num_wrong: 0
 training accuracy:  1.0000
100.00% Step 1000 of 1000                                  00:00:00
───────────────────────────────────────────────────────────────────
testing num_right: 1000
testing num_wrong: 0
 testing accuracy:  1.0000
train accuracy: 1.0000, test accuracy: 1.0000
````

## 6. Test (Option)

You must parepare `Models` directory of **FaceRecognitionDotNet** in <EmotionTraining_dir> before start test.

````cmd
cd <EmotionTraining_dir>
dotnet run -c Debug -- test --dataset <dataset_dir> ^
                            --model "Corrective_re-annotation_of_FER_CK+_KDEF-mlp_3000_5E-05_5E-09_512.dat"

Start load train images
Use Cache <dataset_dir>\train_cache.dat
Load train images: 29568
Start load test images
Use Cache <dataset_dir>\test_cache.dat
Load test images: 3286

training num_right: 14128
training num_wrong: 15440
training accuracy:  0.477813852813853
testing num_right: 1659
testing num_wrong: 1627
testing accuracy:  0.504869141813755
````

## 7. Evaluation

The following table is evaluation from author's data. 
The condition is here. 

* Training Data: 29568
* Test Data: 3286
* Epoch: 3000
* Learning Rate: 0.0005
* Min Learning Rate: 5E-09
* Min Batch Size: 512
* Accuracy: 0.524345709068777