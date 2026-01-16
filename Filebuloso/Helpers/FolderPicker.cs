using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Filebuloso.Helpers;

public sealed class FolderPicker
{
    public string? PickFolder(Window owner)
    {
        var dialogType = Type.GetTypeFromCLSID(CLSID_FileOpenDialog)
            ?? throw new InvalidOperationException("File open dialog COM type not available.");
        var dialog = (IFileDialog)Activator.CreateInstance(dialogType)!;
        try
        {
            dialog.GetOptions(out var options);
            options |= FOS_PICKFOLDERS | FOS_FORCEFILESYSTEM;
            dialog.SetOptions(options);

            var handle = owner is null ? IntPtr.Zero : new WindowInteropHelper(owner).Handle;
            var hr = dialog.Show(handle);
            if (hr == HRESULT_CANCELLED)
            {
                return null;
            }

            Marshal.ThrowExceptionForHR(hr);
            dialog.GetResult(out var item);
            item.GetDisplayName(SIGDN_FILESYSPATH, out var path);
            return path;
        }
        finally
        {
            Marshal.ReleaseComObject(dialog);
        }
    }

    private const uint FOS_PICKFOLDERS = 0x00000020;
    private const uint FOS_FORCEFILESYSTEM = 0x00000040;
    private const int HRESULT_CANCELLED = unchecked((int)0x800704C7);
    private const int SIGDN_FILESYSPATH = unchecked((int)0x80058000);

    private static readonly Guid CLSID_FileOpenDialog = new("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEF7");

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("42F85136-DB7E-439C-85F1-E4075D135FC8")]
    private interface IFileDialog
    {
        [PreserveSig]
        int Show(IntPtr parent);
        void SetFileTypes(uint count, IntPtr filterSpec);
        void SetFileTypeIndex(uint index);
        void GetFileTypeIndex(out uint index);
        void Advise(IntPtr events, out uint cookie);
        void Unadvise(uint cookie);
        void SetOptions(uint options);
        void GetOptions(out uint options);
        void SetDefaultFolder(IShellItem folder);
        void SetFolder(IShellItem folder);
        void GetFolder(out IShellItem folder);
        void GetCurrentSelection(out IShellItem item);
        void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string name);
        void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string name);
        void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string title);
        void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
        void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string text);
        void GetResult(out IShellItem item);
        void AddPlace(IShellItem item, int fileDialogCustomize);
        void SetDefaultExtension([MarshalAs(UnmanagedType.LPWStr)] string extension);
        void Close(int hr);
        void SetClientGuid(ref Guid guid);
        void ClearClientData();
        void SetFilter(IntPtr filter);
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
    private interface IShellItem
    {
        void BindToHandler(IntPtr pbc, ref Guid bhid, ref Guid riid, out IntPtr ppv);
        void GetParent(out IShellItem ppsi);
        void GetDisplayName(int sigdnName, [MarshalAs(UnmanagedType.LPWStr)] out string ppszName);
        void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);
        void Compare(IShellItem psi, uint hint, out int piOrder);
    }
}
