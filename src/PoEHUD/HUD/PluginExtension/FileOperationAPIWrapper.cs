using System;
using System.Runtime.InteropServices;

namespace PoEHUD.HUD.PluginExtension
{
    public class FileOperationApiWrapper
    {
        /// <summary>
        /// Possible flags for the SHFileOperation method.
        /// </summary>
        [Flags]
        public enum FileOperationFlags : ushort
        {
            /// <summary>
            /// Do not show a dialog during the process
            /// </summary>
            Silent = 0x0004,

            /// <summary>
            /// Do not ask the user to confirm selection
            /// </summary>
            NoConfirmation = 0x0010,

            /// <summary>
            /// Delete the file to the recycle bin.  (Required flag to send a file to the bin
            /// </summary>
            AllowUndo = 0x0040,

            /// <summary>
            /// Do not show the names of the files or folders that are being recycled.
            /// </summary>
            SimpleProgress = 0x0100,

            /// <summary>
            /// Surpress errors, if any occur during the process.
            /// </summary>
            NoErrorUI = 0x0400,

            /// <summary>
            /// Warn if files are too big to fit in the recycle bin and will need
            /// to be deleted completely.
            /// </summary>
            WantNukeWarning = 0x4000
        }

        /// <summary>
        /// File Operation Function Type for SHFileOperation
        /// </summary>
        public enum FileOperationType : uint
        {
            /// <summary>
            /// Move the objects
            /// </summary>
            Move = 0x0001,

            /// <summary>
            /// Copy the objects
            /// </summary>
            Copy = 0x0002,

            /// <summary>
            /// Delete (or recycle) the objects
            /// </summary>
            Delete = 0x0003,

            /// <summary>
            /// Rename the object(s)
            /// </summary>
            Rename = 0x0004
        }

        /// <summary>
        /// Send file to recycle bin
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        /// <param name="flags">FileOperationFlags to add in addition to FOF_ALLOWUNDO</param>
        public static bool Send(string path, FileOperationFlags flags)
        {
            try
            {
                var fs = new SHFILEOPSTRUCT
                {
                    wFunc = FileOperationType.Delete,
                    pFrom = path + '\0' + '\0',
                    fFlags = FileOperationFlags.AllowUndo | flags
                };
                SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Send file to recycle bin.  Display dialog, display warning if files are too big to fit (FOF_WANTNUKEWARNING)
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool Send(string path)
        {
            return Send(path, FileOperationFlags.NoConfirmation | FileOperationFlags.WantNukeWarning);
        }

        /// <summary>
        /// Send file silently to recycle bin.  Surpress dialog, surpress errors, delete if too large.
        /// </summary>
        /// <param name="path">Location of directory or file to recycle</param>
        public static bool MoveToRecycleBin(string path)
        {
            return Send(path, FileOperationFlags.NoConfirmation | FileOperationFlags.NoErrorUI | FileOperationFlags.Silent);
        }

        public static bool DeleteCompletelySilent(string path)
        {
            return DeleteFile(path, FileOperationFlags.NoConfirmation | FileOperationFlags.NoErrorUI | FileOperationFlags.Silent);
        }

        private static bool DeleteFile(string path, FileOperationFlags flags)
        {
            try
            {
                var fs = new SHFILEOPSTRUCT
                {
                    wFunc = FileOperationType.Delete,
                    pFrom = path + '\0' + '\0',
                    fFlags = flags
                };
                SHFileOperation(ref fs);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

        /// <summary>
        /// SHFILEOPSTRUCT for SHFileOperation from COM
        /// </summary>
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEOPSTRUCT
        {
            public IntPtr hwnd;
            [MarshalAs(UnmanagedType.U4)]
            public FileOperationType wFunc;
            public string pFrom;
            public string pTo;
            public FileOperationFlags fFlags;
            [MarshalAs(UnmanagedType.Bool)]
            public bool fAnyOperationsAborted;
            public IntPtr hNameMappings;
            public string lpszProgressTitle;
        }
    }
}
