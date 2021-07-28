using RootTools_Vision;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RooRoot_WindII_Optiont_WIND2
{
    public class KlarfFileTransfer
    {
        
        public static void UplaodKlarfFiles(string dstPath, string backupPath = "")
        {
            if(Directory.Exists(dstPath) == false)
            {
                MessageBox.Show("지정 경로가 잘못되었습니다.\n" + dstPath);
                return;
            }

            Settings settings = new Settings();
            SettingItem_SetupFrontside settings_frontside = settings.GetItem<SettingItem_SetupFrontside>();
            SettingItem_SetupBackside settings_backside = settings.GetItem<SettingItem_SetupBackside>();
            SettingItem_SetupEdgeside settings_Edgeside = settings.GetItem<SettingItem_SetupEdgeside>();
            SettingItem_SetupEBR settings_EBR = settings.GetItem<SettingItem_SetupEBR>();


            string klarfPath = settings_frontside.KlarfSavePath;
            if (Directory.Exists(klarfPath) == true)
            {
                string[] files = Directory.GetFiles(klarfPath);
                foreach(string file in files)
                {
                    string movePath = Path.Combine(dstPath, Path.GetFileName(file));
                    if (Directory.Exists(backupPath) == true)
                        File.Copy(file, Path.Combine(backupPath, Path.GetFileName(file)));

                    File.Move(file, movePath);
                }
            }

            klarfPath = settings_backside.KlarfSavePath;
            if (Directory.Exists(klarfPath) == true)
            {
                string[] files = Directory.GetFiles(klarfPath);
                foreach (string file in files)
                {
                    string newPath = Path.Combine(dstPath, Path.GetFileName(file));
                    if (Directory.Exists(backupPath) == true)
                        File.Copy(file, Path.Combine(backupPath, Path.GetFileName(file)));
                    File.Move(file, newPath);
                }
            }

            klarfPath = settings_Edgeside.KlarfSavePath;
            if (Directory.Exists(klarfPath) == true)
            {
                string[] files = Directory.GetFiles(klarfPath);
                foreach (string file in files)
                {
                    string newPath = Path.Combine(dstPath, Path.GetFileName(file));
                    if (Directory.Exists(backupPath) == true)
                        File.Copy(file, Path.Combine(backupPath, Path.GetFileName(file)));
                    File.Move(file, newPath);
                }
            }

            klarfPath = settings_EBR.KlarfSavePath;
            if (Directory.Exists(klarfPath) == true)
            {
                string[] files = Directory.GetFiles(klarfPath);
                foreach (string file in files)
                {
                    string newPath = Path.Combine(dstPath, Path.GetFileName(file));
                    if (Directory.Exists(backupPath) == true)
                        File.Copy(file, Path.Combine(backupPath, Path.GetFileName(file)));
                    File.Move(file, newPath);
                }
            }
        }
    }
}
