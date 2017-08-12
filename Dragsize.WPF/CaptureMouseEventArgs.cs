using System.Windows.Forms;

namespace Dragsize.WPF
{
   public delegate void CaptureMouseEventHandler(object sender, CaptureMouseEventArgs e);

   public class CaptureMouseEventArgs : MouseEventArgs
   {
      public bool LeftButtonUp { get; }

      public CaptureMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta) : base(button, clicks, x, y, delta)
      {
      }

      public CaptureMouseEventArgs(MouseButtons button, int clicks, int x, int y, int delta, bool leftButtonUp) : base(button, clicks, x, y, delta)
      {
         LeftButtonUp = leftButtonUp;
      }

   }
}
