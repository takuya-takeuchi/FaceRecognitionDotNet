namespace FaceRecognitionDotNet
{

    internal sealed class FaceRecognitionModels
    {

        public static string GetPosePredictorModelLocation()
        {
            return "shape_predictor_68_face_landmarks.dat";
        }

        public static string GetPosePredictorFivePointModelLocation()
        {
            return "shape_predictor_5_face_landmarks.dat";
        }

        public static string GetFaceRecognitionModelLocation()
        {
            return "dlib_face_recognition_resnet_model_v1.dat";
        }

        public static string GetCnnFaceDetectorModelLocation()
        {
            return "mmod_human_face_detector.dat";
        }

        public static string GetPosePredictor194PointModelLocation()
        {
            return "helen-dataset.dat";
        }

        public static string GetAgeNetworkModelLocation()
        {
            return "adience-age-network.dat";
        }

        public static string GetGenderNetworkModelLocation()
        {
            return "utkface-gender-network.dat";
        }

        public static string GetEmotionNetworkModelLocation()
        {
            return "corrective-reannotation-of-fer-ck-kdef-emotion-network_test_best.dat";
        }

    }

}
