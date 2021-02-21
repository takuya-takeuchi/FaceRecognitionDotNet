using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace DlibDotNet
{

    internal sealed partial class NativeMethods
    {

        #region Fields

        public const string GenderClassificationNativeLibrary = "DlibDotNetNativeDnnGenderClassification";

        public const CallingConvention GenderClassificationCallingConvention = CallingConvention.Cdecl;
        
        #endregion

        [DllImport(GenderClassificationNativeLibrary, CallingConvention = GenderClassificationCallingConvention)]
        public static extern IntPtr LossMulticlassLog_gender_train_type_create();

        [DllImport(GenderClassificationNativeLibrary, CallingConvention = GenderClassificationCallingConvention)]
        public static extern void LossMulticlassLog_gender_train_type_delete(IntPtr @base);

        [DllImport(GenderClassificationNativeLibrary, CallingConvention = GenderClassificationCallingConvention)]
        public static extern void LossMulticlassLog_gender_train_type_eval(IntPtr @base);

    }

}