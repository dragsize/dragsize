using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Dragsize.WPF.ViewModels;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace Dragsize.WPF.Windows
{
   /// <summary>
   /// Interaction logic for Preferences.xaml
   /// </summary>
   public partial class Preferences : Window
   {
      public Preferences()
      {
         InitializeComponent();
      }

      private void TxtChangeKey_OnPreviewKeyDown(object sender, KeyEventArgs e)
      {
         var vm = this.DataContext as PreferencesViewModel;
         vm.RecordKey((Keys)KeyInterop.VirtualKeyFromKey(e.Key));

         txtChangeKey.Text = string.Empty;
      }
   }
}