using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace DlibDotNet
{

    internal sealed partial class NativeMethods
    {

        #region Fields

        public const string EmotionClassificationNativeLibrary = "DlibDotNetNativeDnnEmotionClassification";

        public const CallingConvention EmotionClassificationCallingConvention = CallingConvention.Cdecl;

        #endregion

        [DllImport(EmotionClassificationNativeLibrary, CallingConvention = EmotionClassificationCallingConvention)]
        public static extern IntPtr LossMulticlassLog_emotion_train_type_create();

        [DllImport(EmotionClassificationNativeLibrary, CallingConvention = EmotionClassificationCallingConvention)]
        public static extern void LossMulticlassLog_emotion_train_type_delete(IntPtr @base);

        [DllImport(EmotionClassificationNativeLibrary, CallingConvention = EmotionClassificationCallingConvention)]
        public static extern void LossMulticlassLog_emotion_train_type_eval(IntPtr @base);


    }

}