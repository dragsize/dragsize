namespace Dragsize.WPF.Interfaces
{
   public interface IConfig
   {
      bool DesktopMode { get; set; }
      System.Windows.Forms.Keys CaptureHotkey { get; set; }
      bool CaptureHotkeyShift { get; set; }
      bool CaptureHotkeyCtrl { get; set; }
      bool CaptureHotkeyAlt { get; set; }
      bool CaptureHotkeyWin { get; set; }

      void Save();
      void Load();
   }
}
