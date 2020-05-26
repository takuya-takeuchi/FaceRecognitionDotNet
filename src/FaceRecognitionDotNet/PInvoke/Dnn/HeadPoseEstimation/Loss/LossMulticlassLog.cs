using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace DlibDotNet
{

    internal sealed partial class NativeMethods
    {

        #region Fields

        public const string HeadPoseEstimationNativeLibrary = "DlibDotNetNativeDnnHeadPoseEstimation";

        public const CallingConvention HeadPoseEstimationCallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
        
        #endregion

        [DllImport(HeadPoseEstimationNativeLibrary, CallingConvention = HeadPoseEstimationCallingConvention)]
        public static extern IntPtr LossMulticlassLog_headpose_train_type_create();

        [DllImport(HeadPoseEstimationNativeLibrary, CallingConvention = HeadPoseEstimationCallingConvention)]
        public static extern void LossMulticlassLog_headpose_train_type_delete(IntPtr @base);

    }

}