using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Dragsize.WPF.Implementations;
using Dragsize.WPF.Interfaces;
using Dragsize.WPF.ViewModels;
using Dragsize.WPF.Windows;
using Application = System.Windows.Application;
using KeyEventArgs = System.Windows.Forms.KeyEventArgs;
using Point = System.Drawing.Point;

namespace Dragsize.WPF
{
   /// <summary>
   /// Interaction logic for MainWindow.xaml
   /// </summary>
   public partial class MainWindow : Window
   {
      private Point? _mouseDownPosition;

      private CaptureState _captureState = CaptureState.Idle;

      private WindowToResize _windowToResize;

      private readonly CaptureForm _captureForm = new CaptureForm();
      private readonly SelectedWindowIndicator _selectedWindowIndicator = new SelectedWindowIndicator();

      private readonly UserActivityHook _hook;

      private readonly IConfig _config;

      public MainWindow()
      {
         InitializeComponent();

         _config = new ConfigFileConfig();
         _config.Load();
         UpdateModeBasedOnConfig();

         this.Loaded += MainWindow_Loaded;

         this.Hide();

         _hook = new UserActivityHook(true, true);
         _hook.KeyDown += Hook_KeyDown;
         _hook.OnMouseActivity += Hook_OnMouseActivity;
      }

      private void UpdateModeBasedOnConfig()
      {
         if (_config.DesktopMode)
         {
            cmDesktop.IsChecked = true;
            cmTouchpad.IsChecked = false;
         }
         else
         {
            cmDesktop.IsChecked = false;
            cmTouchpad.IsChecked = true;
         }
      }

      private void ForgetCaptureAndReturnToIdle()
      {
         _mouseDownPosition = null;
         _captureForm.Height = 0;
         _captureForm.Width = 0;
         _captureForm.Hide();
         _captureState = CaptureState.Idle;
      }

      private void ShowCaptureForm(Point startPosition)
      {
         _mouseDownPosition = startPosition;
         _captureState = CaptureState.Desktop_MouseDownAwaitingMouseUp;

         _captureForm.Left = _mouseDownPosition.Value.X;
         _captureForm.Top = _mouseDownPosition.Value.Y;

         _captureForm.lblWindowInformation.Content = $"{_windowToResize.Title}";

         _captureForm.ShowInTaskbar = false;
         _captureForm.Show();
         _captureForm.Activate(); //this is supposed to take OS focus, but I'm not sure if it actually does anything for us
      }

      private void UpdateCaptureForm(Point endPosition)
      {
         var mousePosition = endPosition;

         if (mousePosition.X >= _mouseDownPosition.Value.X)
         {
            _captureForm.Left = _mouseDownPosition.Value.X;
            _captureForm.Width = mousePosition.X - _mouseDownPosition.Value.X;
         }
         else
         {
            _captureForm.Left = mousePosition.X;
            _captureForm.Width = _mouseDownPosition.Value.X - mousePosition.X;
         }

         if (mousePosition.Y >= _mouseDownPosition.Value.Y)
         {
            _captureForm.Top = _mouseDownPosition.Value.Y;
            _captureForm.Height = mousePosition.Y - _mouseDownPosition.Value.Y;
         }
         else
         {
            _captureForm.Top = mousePosition.Y;
            _captureForm.Height = _mouseDownPosition.Value.Y - mousePosition.Y;
         }
      }

      private void Hook_OnMouseActivity(object sender, CaptureMouseEventArgs e)
      {
         if (_captureState == CaptureState.Desktop_HotkeyPressedAwaitingMouseDown && (e.Button & MouseButtons.Left) != 0)
         {
            ShowCaptureForm(e.Location);
         }
         else if (_captureState == CaptureState.Touchpad_HotkeyPressed)
         {
            ShowCaptureForm(e.Location);
            _captureState = CaptureState.Touchpad_Dragging;
            _hook.SuppressLeftMouseDownOnce = true;
         }
         else if (_captureState == CaptureState.Touchpad_Dragging)
         {
            UpdateCaptureForm(e.Location);

            if (e.LeftButtonUp)
            {
               CommitCaptureForm();
               _selectedWindowIndicator.Hide();
            }
         }
         else if (_captureState == CaptureState.Desktop_MouseDownAwaitingMouseUp)
         {
            UpdateCaptureForm(e.Location);

            if (e.LeftButtonUp)
            {
               CommitCaptureForm();
            }
         }
      }

      private void CommitCaptureForm()
      {
         if (_windowToResize != null)
         {
            _windowToResize.MoveTo((int)_captureForm.Left, (int)_captureForm.Top, (int)_captureForm.Width, (int)_captureForm.Height);
         }

         ForgetCaptureAndReturnToIdle();
         _selectedWindowIndicator.Hide();
      }


      private void ShowIndicatorWindow()
      {
         Debug.Assert(_windowToResize != null); //there should be a null check on somewhere window before this is called
         var rect = _windowToResize.GetPosition();
         _selectedWindowIndicator.Top = rect.Top;
         _selectedWindowIndicator.Left = rect.Left;
         _selectedWindowIndicator.Height = rect.Bottom - rect.Top;
         _selectedWindowIndicator.Width = rect.Right - rect.Left;
         _selectedWindowIndicator.ShowInTaskbar = false;
         _selectedWindowIndicator.Show();
      }

      private void Hook_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
      {         
         if (_captureState == CaptureState.Idle && HotkeyPressed(e))
         {
            _windowToResize = WindowToResize.FromActiveWindow();

            if (_windowToResize == null)
            {
              //no window to work with 
            }
            else
            {
               if (_config.DesktopMode)
               {
                  _captureState = CaptureState.Desktop_HotkeyPressedAwaitingMouseDown;
                  ShowIndicatorWindow();
               }
               else //touchpad mode
               {
                  _captureState = CaptureState.Touchpad_HotkeyPressed;
                  ShowIndicatorWindow();
               }
            }

            e.Handled = true;
         }
         else if (_captureState == CaptureState.Desktop_HotkeyPressedAwaitingMouseDown && e.KeyCode == Keys.Escape)
         {
            _windowToResize = null;
            _captureState = CaptureState.Idle;
            _selectedWindowIndicator.Hide();

            e.Handled = true;
         }
         else if (_captureState == CaptureState.Desktop_MouseDownAwaitingMouseUp && e.KeyCode == Keys.Escape)
         {
            _windowToResize = null;
            ForgetCaptureAndReturnToIdle();
            _selectedWindowIndicator.Hide();
            e.Handled = true;
         }
         else if (_captureState == CaptureState.Touchpad_Dragging && e.KeyCode == Keys.Escape)
         {
            _windowToResize = null;
            ForgetCaptureAndReturnToIdle();
            _selectedWindowIndicator.Hide();
            e.Handled = true;
         }
      }

      private bool HotkeyPressed(KeyEventArgs e)
      {
         if (e.KeyCode != _config.CaptureHotkey) return false;

         bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
         bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
         bool alt = Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt);
         bool win = Keyboard.IsKeyDown(Key.LWin) || Keyboard.IsKeyDown(Key.RWin);

         bool modifierMatch = true;
         if (_config.CaptureHotkeyShift && !shift) modifierMatch = false;
         if (_config.CaptureHotkeyCtrl && !ctrl) modifierMatch = false;
         if (_config.CaptureHotkeyAlt && !alt) modifierMatch = false;
         if (_config.CaptureHotkeyWin && !win) modifierMatch = false;

         return modifierMatch;
      }

      private void MainWindow_Loaded(object sender, RoutedEventArgs e)
      {
      }

      private void CmExit_OnClick(object sender, RoutedEventArgs e)
      {
         Application.Current.Shutdown();
      }

      private bool _showingPreferences;

      private void CmPreferences_OnClick(object sender, RoutedEventArgs e)
      {
         if (_showingPreferences) return;

         var preferences = new Preferences();
         preferences.DataContext = new PreferencesViewModel(_config, () => preferences.Close());
         preferences.WindowStartupLocation = WindowStartupLocation.CenterScreen;
         _showingPreferences = true;
         preferences.Show();

         preferences.Closed += (s, ev) => { _showingPreferences = false; };
      }

      private bool _showingAbout;

      private void CmAbout_OnClick(object sender, RoutedEventArgs e)
      {
         if (_showingAbout) return;

         var about = new About();
         about.DataContext = new AboutViewModel();
         _showingAbout = true;
         about.Show();
         about.Closed += (s, ev) => { _showingAbout = false; };
      }

      private void CmDesktop_OnClick(object sender, RoutedEventArgs e)
      {
         _config.DesktopMode = true;
         _config.Save();
         UpdateModeBasedOnConfig();
      }

      private void CmTouchpad_OnClick(object sender, RoutedEventArgs e)
      {
         _config.DesktopMode = false;
         _config.Save();
         UpdateModeBasedOnConfig();
      }
   }

   public enum CaptureState
   {
      Idle,
      Desktop_HotkeyPressedAwaitingMouseDown,
      Desktop_MouseDownAwaitingMouseUp,
      Touchpad_HotkeyPressed,
      Touchpad_Dragging
   }
}
