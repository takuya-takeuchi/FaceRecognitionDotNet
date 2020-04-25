# Age Training
 
This program aims to train human face images to classify age.
Age classes are **(0, 2)**, **(4, 6)**, **(8, 13)**, **(15, 20)**, **(25, 32)**, **(38, 43)**, **(48, 53)** and **(60, 100)**.

## How to use?

## 1. Build

1. Open command prompt and change to &lt;AgeTraining_dir&gt;
1. Type the following command
````
$ dotnet remove reference ../../src/FaceRecognitionDotNet\FaceRecognitionDotNet.csproj
$ dotnet add package FaceRecognitionDotNet
$ dotnet build -c Release
````
2. Copy ***DlibDotNetNative.dll***, ***DlibDotNetNativeDnn.dll*** and ***DlibDotNetNativeDnnAgeClassification.dll*** to output directory; &lt;AgeTraining_dir&gt;\bin\Release\netcoreapp2.0.

**NOTE**  
- You should build ***DlibDotNetNative.dll***, ***DlibDotNetNativeDnn.dll*** and ***DlibDotNetNativeDnnAgeClassification.dll*** with CUDA.
- If you want to run at Linux and MacOS, you should build the **DlibDotNet** at first.  
Please refer the [Tutorial for Linux](https://github.com/takuya-takeuchi/DlibDotNet/wiki/Tutorial-for-Linux) or [Tutorial for MacOS](https://github.com/takuya-takeuchi/DlibDotNet/wiki/Tutorial-for-MacOS).

## 2. Download train and test data

#### Dataset

Download data from the following url. You must sign up by using your name and mail address.

- https://talhassner.github.io/home/projects/Adience/Adience-data.html
  - aligned.tar.gz
  - faces.tar.gz
  - fold_0_data.txt
  - fold_1_data.txt
  - fold_2_data.txt
  - fold_3_data.txt
  - fold_4_data.txt
  - fold_frontal_0_data.txt
  - fold_frontal_1_data.txt
  - fold_frontal_2_data.txt
  - fold_frontal_3_data.txt
  - fold_frontal_4_data.txt

And extract them and copy to extracted files to directory in &lt;AgeTraining_dir&gt;.

#### Model file

Download test data from the following urls.

- http://dlib.net/files/shape_predictor_5_face_landmarks.dat.bz2

And extract them and copy to extracted files to &lt;AgeTraining_dir&gt;.

## 3. Create dataset

Create dataset from Adience directory by using ***tools/CreateDataset.ps1***.
The following command divides images to train and test randomly according to ***TrainRate***.
***TrainRate 8*** means that training data is 80% and test data is 20%.

````
pwsh tools\CreateDataset.ps1 -InputDirectory Adience -TrainRate 8 -OutputDirectory AdienceDataset -Max 0
````

## 4. Preprocess

Created Dataset contains bad data. **preprocess** command cleans them by the following steps.

1. Face Detection. Skip it if fail to detect face.
1. Crop face area by detected rectangle. Use max size if there is some areas.
1. Save file

````
cd <AgeTraining_dir>
dotnet run -c Release -- preprocess --dataset=AdienceDataset --output=AdienceDataset_preprocessed
````

## 5. Run

You can train dataset by using the following comannd.

#### :bulb: NOTE

Dataset include invalida class. So train and test data will be decrease than you expect.

````
cd <AgeTraining_dir>
dotnet run -c Release -- train --dataset=AdienceDataset ^
                               --epoch=600 ^
                               --lr=0.001 ^
                               --min-lr=0.00001 ^
                               --min-batchsize=384 ^
                               --validation-interval=20
              Epoch: 600
      Learning Rate: 0.001
  Min Learning Rate: 1E-05
     Min Batch Size: 384
Validation Interval: 20
           Use Mean: False

Start load train images
Load train images: 11481
Start load test images
Load test images: 2861
step#: 0     learning rate: 0.001  average loss: 0            steps without apparent progress: 0
Epoch: 0, learning Rate: 0.001, average loss: 1.95084826823971
Epoch: 1, learning Rate: 0.001, average loss: 1.8563975835607
Epoch: 2, learning Rate: 0.001, average loss: 1.79714517301013
step#: 115   learning rate: 0.001  average loss: 1.75747      steps without apparent progress: 14
Epoch: 3, learning Rate: 0.001, average loss: 1.58082807022997
Epoch: 4, learning Rate: 0.001, average loss: 1.55499436315802
Epoch: 5, learning Rate: 0.001, average loss: 1.5295710878622
Epoch: 6, learning Rate: 0.001, average loss: 1.50453946884442
step#: 234   learning rate: 0.001  average loss: 1.48705      steps without apparent progress: 30
Epoch: 7, learning Rate: 0.001, average loss: 1.39775350564064
...
...
Epoch: 596, learning Rate: 0.001, average loss: 0.00659288855763023
step#: 17920  learning rate: 0.001  average loss: 0.00652122   steps without apparent progress: 769
Epoch: 597, learning Rate: 0.001, average loss: 0.00633556920725426
Saved state to adience-age-network_600_0.001_1E-05_384_False
Epoch: 598, learning Rate: 0.001, average loss: 0.00605806328235235
Epoch: 599, learning Rate: 0.001, average loss: 0.00648398713911098
Saved state to adience-age-network_600_0.001_1E-05_384_False_
done training
training num_right: 11473
training num_wrong: 8
training accuracy:  0.999303196585663
testing num_right: 2183
testing num_wrong: 678
testing accuracy:  0.76301992310381
````

## 6. Check training (Option)

You can check training log as graph.

````
python tools\visualize-log.py adience-age-network_600_0.001_1E-05_384_False.log
````

<img src="images/visualize-log.png"/>