using System;
using System.IO;
using System.Windows.Forms;
using Dragsize.WPF.Interfaces;
using Dragsize.WPF.LitJson;

namespace Dragsize.WPF.Implementations
{
   public class ConfigFileConfig : IConfig
   {
      public bool DesktopMode { get; set; }
      public Keys CaptureHotkey { get; set; }
      public bool CaptureHotkeyShift { get; set; }
      public bool CaptureHotkeyCtrl { get; set; }
      public bool CaptureHotkeyAlt { get; set; }
      public bool CaptureHotkeyWin { get; set; }

      private void SetDefaults()
      {
         DesktopMode = true;
         CaptureHotkey = Keys.Pause;
         CaptureHotkeyShift = false;
         CaptureHotkeyCtrl = false;
         CaptureHotkeyAlt = false;
         CaptureHotkeyWin = false;
      }

      public void Save()
      {
         var jsonObject = new ConfigJsonFormat()
         {
            DesktopMode = this.DesktopMode,
            CaptureHotkey = (int)this.CaptureHotkey,
            CaptureHotkeyAlt = this.CaptureHotkeyAlt,
            CaptureHotkeyCtrl = this.CaptureHotkeyCtrl,
            CaptureHotkeyShift = this.CaptureHotkeyShift,
            CaptureHotkeyWin = this.CaptureHotkeyWin,
         };

         string json = JsonMapper.ToJson(jsonObject);

         Directory.CreateDirectory(ConfigFileFolder);
         File.WriteAllText(ConfigFileLocation, json);
      }

      public void Load()
      {
         if (File.Exists(ConfigFileLocation))
         {
            string json = File.ReadAllText(ConfigFileLocation);
            var jsonObject = JsonMapper.ToObject<ConfigJsonFormat>(json);
            DesktopMode = jsonObject.DesktopMode;
            CaptureHotkey = (Keys) jsonObject.CaptureHotkey;
            CaptureHotkeyShift = jsonObject.CaptureHotkeyShift;
            CaptureHotkeyAlt = jsonObject.CaptureHotkeyAlt;
            CaptureHotkeyCtrl = jsonObject.CaptureHotkeyCtrl;
            CaptureHotkeyWin = jsonObject.CaptureHotkeyWin;
         }
         else
         {
            SetDefaults();
         }
      }

      private string ConfigFileFolder => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DragSize");

      private string ConfigFileLocation => Path.Combine(ConfigFileFolder, "Dragsize.config");
   }

   public class ConfigJsonFormat
   {
      public bool DesktopMode { get; set; }
      public int CaptureHotkey { get; set; }
      public bool CaptureHotkeyShift { get; set; }
      public bool CaptureHotkeyCtrl { get; set; }
      public bool CaptureHotkeyAlt { get; set; }
      public bool CaptureHotkeyWin { get; set; }
   }
}
