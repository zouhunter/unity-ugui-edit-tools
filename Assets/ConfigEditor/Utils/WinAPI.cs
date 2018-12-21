using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;
using System.Linq;
using System.Text;

public class WinAPI
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct FileStruct
    {
        public int structSize;
        public IntPtr dlgOwner;
        public IntPtr instance;
        public String filter;
        public String customFilter;
        public int maxCustFilter;
        public int filterIndex;
        public String file;
        public int maxFile;
        public String fileTitle;
        public int maxFileTitle;
        public String initialDir;
        public String title;
        public int flags;
        public short fileOffset;
        public short fileExtension;
        public String defExt;
        public IntPtr custData;
        public IntPtr hook;
        public String templateName;
        public IntPtr reservedPtr;
        public int reservedInt;
        public int flagsEx;
    }
    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int MessageBoxTimeout(IntPtr hwnd, String text, String title, uint type, Int16 wLanguageId, Int32 milliseconds);

    [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
    public static extern bool GetOpenFileName([In, Out] FileStruct ofn);

    [DllImport("Comdlg32.dll", CharSet = CharSet.Auto, ThrowOnUnmappableChar = true, SetLastError = true)]
    private static extern bool GetSaveFileName([In, Out] FileStruct ofn);
    /// <summary>
    /// 运行时调用（忽删除）
    /// </summary>
    /// <param name="title"></param>
    /// <param name="defultName"></param>
    /// <param name="path"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static string SaveFileDialog(string title, string defultName, string path, string filter)
    {
        FileStruct ofn = new FileStruct();

        ofn.structSize = Marshal.SizeOf(ofn);

        ofn.filter = filter;

        ofn.file = defultName + new string(new char[256]);

        ofn.maxFile = ofn.file.Length;

        ofn.fileTitle = new string(new char[64]);

        ofn.maxFileTitle = ofn.fileTitle.Length;

        if (!string.IsNullOrEmpty(path))
            path = path.Replace('/', '\\');

        ofn.initialDir = path;

        ofn.title = title;

        ofn.defExt = "";//显示文件的类型

        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;

        try
        {
            if (GetSaveFileName(ofn))
            {
                var filePath = ofn.file.Replace("\0", "");
                return filePath;
            }
            else
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }

    }
    /// <summary>
    /// 运行时调用（忽删除）
    /// </summary>
    /// <param name="title"></param>
    /// <param name="defultName"></param>
    /// <param name="path"></param>
    /// <param name="filter"></param>
    /// <returns></returns>
    public static string OpenFileDialog(string title, string path, string filter)
    {
        FileStruct ofn = new FileStruct();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = filter;
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        if (!string.IsNullOrEmpty(path))
            path = path.Replace('/', '\\');
        //默认路径  
        ofn.initialDir = path;
        ofn.title = title;
        //显示文件的类型  
        ofn.defExt = "";
        //注意 一下项目不一定要全选 但是0x00000008项不要缺少  
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR  
        try
        {
            if (GetOpenFileName(ofn))
            {
                var filePath = ofn.file.Replace("\0", "");
                return filePath;
            }
            else
            {
                return null;
            }
        }
        catch (Exception)
        {
            return null;
        }
    }

    [DllImport("Shell32.Dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHBrowseForFolder(ref BROWSEINFO lpbi);
    [DllImport("Shell32.DLL")]
    public static extern int SHGetPathFromIDList(IntPtr pidl, StringBuilder Path);

    private static readonly int MAX_PATH = 260;
    private enum BIF_Flags
    {
        BIF_RETURNONLYFSDIRS = 0x0001,
        BIF_DONTGOBELOWDOMAIN = 0x0002,
        BIF_STATUSTEXT = 0x0004,
        BIF_RETURNFSANCESTORS = 0x0008,
        BIF_EDITBOX = 0x0010,
        BIF_VALIDATE = 0x0020,
        BIF_NEWDIALOGSTYLE = 0x0040,
        BIF_USENEWUI = (BIF_NEWDIALOGSTYLE | BIF_EDITBOX),
        BIF_BROWSEINCLUDEURLS = 0x0080,
        BIF_UAHINT = 0x0100,
        BIF_NONEWFOLDERBUTTON = 0x0200,
        BIF_NOTRANSLATETARGETS = 0x0400,
        BIF_BROWSEFORCOMPUTER = 0x1000,
        BIF_BROWSEFORPRINTER = 0x2000,
        BIF_BROWSEINCLUDEFILES = 0x4000,
        BIF_SHAREABLE = 0x8000
    };
    private delegate int BrowseCallbackProc(IntPtr hwnd, UInt32 uMsg, UInt32
    lParam, UInt32 lpData);
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential,
    CharSet = CharSet.Auto)]
    private struct BROWSEINFO
    {
        public IntPtr hwndOwner;
        public UIntPtr pidlRoot;
        [MarshalAs(UnmanagedType.LPTStr)]
        public String pszDisplayName;
        [MarshalAs(UnmanagedType.LPTStr)]
        public String lpszTitle;
        [MarshalAs(UnmanagedType.U4)]
        public BIF_Flags ulFlags;
        [MarshalAs(UnmanagedType.FunctionPtr)]
        public BrowseCallbackProc lpfn;
        [MarshalAs(UnmanagedType.U4)]
        public UInt32 lParam;
        public Int32 iImage;
    }
    /// <summary>
    /// 选择目录对话框
    /// </summary>
    /// <returns>返回选择的目录路径</returns>
    public static string GetDirectory()
    {
        BROWSEINFO bi = new BROWSEINFO();
        IntPtr pidlRet = IntPtr.Zero;
        pidlRet = SHBrowseForFolder(ref bi);
        if (pidlRet == IntPtr.Zero)
        {
            return string.Empty;
        }
        StringBuilder sb = new StringBuilder(MAX_PATH);
        if (0 == SHGetPathFromIDList(pidlRet, sb))
        {
            return string.Empty;
        }
        return sb.ToString();
    }

}
