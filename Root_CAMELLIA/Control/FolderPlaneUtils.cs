using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Media.Imaging;

namespace WpfApplication1.Model
{
    public static class FolderPlaneUtils
    {
        public static bool IsFolder(string path)
        {
            bool isFolder;
            isFolder = System.IO.Directory.Exists(path);
            return isFolder;
        }

        public static bool IsDrive(string path)
        {
            bool isDrive = false;
            // path here X: ; str X:// 
            foreach (string str in Directory.GetLogicalDrives())
            {
                if (str.Contains(path)) { isDrive = true; }
            }
            return isDrive;
        }

        public static bool IsLink(string path)
        {
            bool isLink = false;

            string ext = Path.GetExtension(path);
            ext.ToLower();

            isLink = (ext == ".lnk");
            return isLink;
        }

        public static string ResolveShortCut(string path)
        {
            if (FolderPlaneUtils.IsLink(path))
            {
                // First working solution chosen to resolve link
                // For using IWshRuntimeLibrary add reference, Com tab, choose Microsoft Shell Controls and Automation
                // Question: must we dispose shell, I assume this is a Managed wrapper around com

                IWshRuntimeLibrary.WshShell shell = new IWshRuntimeLibrary.WshShell();
                IWshRuntimeLibrary.IWshShortcut link = (IWshRuntimeLibrary.IWshShortcut)shell.CreateShortcut(path);

                string target = link.TargetPath;
                return target;
            }

            // If not found, keep original path
            return path;
        }

        public static string FolderUp(string path)
        {
            string[] folders = path.Split('\\');

            if (folders.Length <= 1)
            {
                // the sky is the limit
                return path;
            }
            else
            {
                // Remove from path tailer = "//" + folders[folders.Length - 1]
                // for (int i = 0; i <= folders.Length - 3; i++) { result = result + folders[i] + "\\"; }
                // result = result + folders[folders.Length - 2];

                string result = path.Remove(path.Length - (2 + folders[folders.Length - 1].Length) + 1);
                return result;
            }
        }
    }

}
