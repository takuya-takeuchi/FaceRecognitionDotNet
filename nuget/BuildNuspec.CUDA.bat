@set target=101 100 92

for %%t in (%target%) do (
  nuget pack FaceRecognitionDotNet.CUDA%%t.nuspec
)