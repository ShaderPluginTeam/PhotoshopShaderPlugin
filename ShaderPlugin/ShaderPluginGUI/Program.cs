using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;

using PS_Structures;
using ShaderPluginGUI.PS_Structures;

namespace ShaderPluginGUI
{
    public enum PSPluginErrorCodes : short
    {
        NoError = 0,
        UserCanceledError = -128,
        CoercedParamError = 2,
        ReadError = -19,
        WriteError = -20,
        OpenError = -23,
        DiskFullError = -34,
        IOError = -36,
        eofErr = -39, // Also - end of descriptor error.
        fnfErr = -43,
        vLckdErr = -46,
        fLckdErr = -45,
        ParamError = -50,
        MemoryFullError = -108,
        NullHandleErr = -109,
        memWZErr = -111
    }

    public static class Program
    {
        public static PSPluginErrorCodes Result = PSPluginErrorCodes.UserCanceledError; // Canceled by User
        public static String StartupPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PhotoshopShaderPlugin");
        public static String ShadersFolderPath = Path.Combine(StartupPath, "Shaders");
        public static FilterRecordM filterRecord;
        public static IntPtr PhotoshopWindowPointer;
        public static IntPtr LastParamsPtr;

        public static short Main(IntPtr PhotoshopWindowHandle, IntPtr FilterRecordPtr, IntPtr LastParamsPointer)
        {
            Result = PSPluginErrorCodes.UserCanceledError;

            if (!Directory.Exists(StartupPath))
            {
                try
                {
                    Directory.CreateDirectory(StartupPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageBox.Show("Try to run Photoshop with Admin rights.", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
                    return (int)PSPluginErrorCodes.WriteError;
                }
            }

            if (!Directory.Exists(ShadersFolderPath))
                ShadersFolderPath = StartupPath;

            PhotoshopWindowPointer = PhotoshopWindowHandle;
            filterRecord = FilterRecordM.Load(FilterRecordPtr);
            LastParamsPtr = LastParamsPointer;

            SplashWindow splashWindow = new SplashWindow();
            WindowInteropHelper windowInteropHelper = new WindowInteropHelper(splashWindow)
            {
                Owner = PhotoshopWindowHandle
            };
            splashWindow.ShowDialog();

            return (short)Result;
        }
    }
}
