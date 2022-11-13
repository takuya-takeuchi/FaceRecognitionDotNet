using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace FaceRecognitionDotNet
{

    internal sealed partial class NativeMethods
    {

        #region Fields

        public const string AnnoySearchLibrary = "FaceRecognitionDotNetNativeAnnoySearch";

        public const CallingConvention AnnoySearchConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
        
        #endregion

        [DllImport(AnnoySearchLibrary, CallingConvention = AnnoySearchConvention)]
        public static extern IntPtr AnnoySearch_AnnoyIndex_new(int f);

        [DllImport(AnnoySearchLibrary, CallingConvention = AnnoySearchConvention)]
        public static extern void AnnoySearch_AnnoyIndex_delete(IntPtr index);

        [DllImport(AnnoySearchLibrary, CallingConvention = AnnoySearchConvention)]
        public static extern void AnnoySearch_AnnoyIndex_add_item(IntPtr index,
                                                                  int item,
                                                                  double[] vector);

        [DllImport(AnnoySearchLibrary, CallingConvention = AnnoySearchConvention)]
        public static extern void AnnoySearch_AnnoyIndex_build(IntPtr index,
                                                               int q,
                                                               int n_threads=-1);

        [DllImport(AnnoySearchLibrary, CallingConvention = AnnoySearchConvention)]
        public static extern void AnnoySearch_AnnoyIndex_get_nns_by_vector(IntPtr index,
                                                                           double[] query,
                                                                           ulong n,
                                                                           int search_k,
                                                                           IntPtr toplist,
                                                                           IntPtr distances);

    }

}