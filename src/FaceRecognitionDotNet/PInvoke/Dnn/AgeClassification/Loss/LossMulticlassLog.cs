using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace DlibDotNet
{

    internal sealed partial class NativeMethods
    {

        #region Fields

        public const string AgeClassificationNativeLibrary = "DlibDotNetNativeDnnAgeClassification";

        public const CallingConvention AgeClassificationCallingConvention = CallingConvention.Cdecl;

        #endregion

        [DllImport(AgeClassificationNativeLibrary, CallingConvention = AgeClassificationCallingConvention)]
        public static extern IntPtr LossMulticlassLog_age_train_type_create();

        [DllImport(AgeClassificationNativeLibrary, CallingConvention = AgeClassificationCallingConvention)]
        public static extern void LossMulticlassLog_age_train_type_delete(IntPtr @base);

        [DllImport(AgeClassificationNativeLibrary, CallingConvention = AgeClassificationCallingConvention)]
        public static extern void LossMulticlassLog_age_train_type_eval(IntPtr @base);


    }

}