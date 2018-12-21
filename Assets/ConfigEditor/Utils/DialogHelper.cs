using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UGUIAssembler.Config
{
    public class DialogHelper
    {

        public static bool ShowDialog(string title, string info, string confer = "确认", string cansale = null)
        {
#if !RUNTIME_UI
            return UnityEditor.EditorUtility.DisplayDialog(title, info, confer, cansale);
#else
            if (string.IsNullOrEmpty(cansale))
        {
            return WinAPI.MessageBoxTimeout(System.IntPtr.Zero, info, title, 0, 0, 0) == 1;
        }
        else
        {
            return WinAPI.MessageBoxTimeout(System.IntPtr.Zero, info, title, 1, 0, 0) == 1;
        }
#endif
        }

        public static string OpenFolderDialog(string title, string defultFolder)
        {
#if !RUNTIME_UI
       return UnityEditor.EditorUtility.OpenFolderPanel(title, defultFolder, "");
#else
            return WinAPI.GetDirectory();
#endif
        }

        public static string OpenCSVFileDialog(string title, string path)
        {
#if !RUNTIME_UI
        return UnityEditor.EditorUtility.OpenFilePanel(title, path, "csv");
#else
            return WinAPI.OpenFileDialog(title, path, "CSV Files\0 *.csv\0\0");
#endif
        }
        public static string OpenPSDFileDialog(string title, string path)
        {
#if !RUNTIME_UI
        return UnityEditor.EditorUtility.OpenFilePanel(title, path, "psd");
#else
            return WinAPI.OpenFileDialog(title, path, "PSD Files\0 *.psd\0\0");
#endif
        }



        public static string OpenPictureFileDialog(string title, string path)
        {
#if !RUNTIME_UI
        return UnityEditor.EditorUtility.OpenFilePanel(title, path, "png");
#else
            return WinAPI.OpenFileDialog(title, path, "PNG Files\0 *.png\0\0");
#endif
        }

        public static string SaveCsvFileDialog(string title, string defultName, string path)
        {
#if !RUNTIME_UI
        return UnityEditor.EditorUtility.SaveFilePanel(title, path, defultName, "csv");
#else
            return WinAPI.SaveFileDialog(title, defultName, path, "CSV Files\0 *.csv\0\0");
#endif
        }
        /// <summary>
        /// 打开文件夹并选中文件
        /// </summary>
        /// <param name="fileName"></param>
        public static void OpenFolderAndSelectFile(string fileName)
        {
            if (!System.IO.File.Exists(fileName)) return;

            System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo("Explorer.exe");
            psi.Arguments = " /select," + System.IO.Path.GetFullPath(fileName);
            System.Diagnostics.Process.Start(psi);
        }

       public static bool ShowDialog(string title,string info,bool canSaleAble = false)
        {
#if !RUNTIME_UI
            return UnityEditor.EditorUtility.DisplayDialog(title, info, "确认", canSaleAble ? "取消" : null);
#else
            if(canSaleAble)
            {
                var selectID = WinAPI.MessageBoxTimeout(System.IntPtr.Zero, info, title, 1, 0, -1);
                var ok = selectID == 1;
                return ok;
            }
            else
            {
                WinAPI.MessageBoxTimeout(System.IntPtr.Zero, info, title, 0, 0, -1);
                return true;
            }
#endif
        }
    }
}