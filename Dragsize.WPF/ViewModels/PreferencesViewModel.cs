using System;
using System.Windows.Forms;
using Dragsize.WPF.Interfaces;

namespace Dragsize.WPF.ViewModels
{
   public class PreferencesViewModel : ViewModelBase
   {
      private readonly IConfig _config;
      private readonly Action _closeAction;

      public void RecordKey(Keys key)
      {
         _hotkey = key;
         NotifyPropertyChanged(nameof(HotkeyDisplayName));
      }

      public PreferencesViewModel(IConfig config, Action closeAction)
      {
         _config = config;
         _closeAction = closeAction;

         _config.Load();

         _hotkey = _config.CaptureHotkey;
         HotkeyShift = _config.CaptureHotkeyShift;
         HotkeyCtrl = _config.CaptureHotkeyCtrl;
         HotkeyAlt = _config.CaptureHotkeyAlt;
         HotkeyWin = _config.CaptureHotkeyWin;
      }


      private Keys _hotkey;

      public string HotkeyDisplayName => _hotkey.ToString();

      private bool _HotkeyShift;
      public bool HotkeyShift
      {
         get { return _HotkeyShift; }
         set { _HotkeyShift = value; NotifyPropertyChanged(nameof(HotkeyShift)); }
      }

      private bool _HotkeyCtrl;
      public bool HotkeyCtrl
      {
         get { return _HotkeyCtrl; }
         set { _HotkeyCtrl = value; NotifyPropertyChanged(nameof(HotkeyCtrl)); }
      }

      private bool _HotkeyAlt;
      public bool HotkeyAlt
      {
         get { return _HotkeyAlt; }
         set { _HotkeyAlt = value; NotifyPropertyChanged(nameof(HotkeyAlt)); }
      }

      private bool _HotkeyWin;
      public bool HotkeyWin
      {
         get { return _HotkeyWin; }
         set { _HotkeyWin = value; NotifyPropertyChanged(nameof(HotkeyWin)); }
      }

      public RelayCommand CmdSave { get { if (_CmdSave == null) { _CmdSave = new RelayCommand(param => this.Save()); } return _CmdSave; } }
      private RelayCommand _CmdSave;
      /// <summary>
      /// Called by the CmdSave command.
      /// </summary>
      private void Save()
      {
         _config.CaptureHotkey = _hotkey;
         _config.CaptureHotkeyShift = HotkeyShift;
         _config.CaptureHotkeyCtrl = HotkeyCtrl;
         _config.CaptureHotkeyAlt = HotkeyAlt;
         _config.CaptureHotkeyWin = HotkeyWin;

         _config.Save();
         _closeAction();
      }

      public RelayCommand CmdCancel { get { if (_CmdCancel == null) { _CmdCancel = new RelayCommand(param => this.Cancel()); } return _CmdCancel; } }
      private RelayCommand _CmdCancel;
      /// <summary>
      /// Called by the CmdCancel command.
      /// </summary>
      private void Cancel()
      {
         _closeAction();
      }
   }
}
