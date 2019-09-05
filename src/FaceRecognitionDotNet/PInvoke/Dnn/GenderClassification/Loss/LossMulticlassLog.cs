using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace DlibDotNet
{

    internal sealed class NativeMethods
    {

        #region Fields

        public const string NativeLibrary = "DlibDotNetNativeDnnGenderClassification";

        public const CallingConvention CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
        
        #endregion

        [DllImport(NativeLibrary, CallingConvention = CallingConvention)]
        public static extern IntPtr LossMulticlassLog_gender_train_type_create();

    }

}