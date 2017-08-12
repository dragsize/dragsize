using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Dragsize.WPF
{
   public class WindowToResize
   {
      [DllImport("user32.dll")]
      static extern IntPtr GetForegroundWindow();

      [DllImport("user32.dll")]
      static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

      [DllImport("user32.dll", SetLastError = true)]
      internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

      [DllImport("user32.dll", SetLastError = true)]
      internal static extern bool GetWindowRect(IntPtr hWnd, ref Rect rectangle);

      private const int SW_SHOWNORMAL = 1;
      private const int SW_SHOWMINIMIZED = 2;
      private const int SW_SHOWMAXIMIZED = 3;

      [DllImport("user32.dll")]
      private static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

      public struct Rect
      {
         public int Left { get; set; }
         public int Top { get; set; }
         public int Right { get; set; }
         public int Bottom { get; set; }
      }

      private WindowToResize()
      {
      }

      private static string GetWindowTitle(IntPtr intPtr)
      {
         const int nChars = 256;
         StringBuilder sb = new StringBuilder(nChars);

         if (GetWindowText(intPtr, sb, nChars) > 0)
         {
            return sb.ToString();
         }
         return null;
      }

      public IntPtr IntPtr;
      public string Title;

      public static WindowToResize FromActiveWindow()
      {
         IntPtr foregroundIntPtr = GetForegroundWindow();
         if (foregroundIntPtr.ToString() == "0") return null; //this can happen if you just closed a window in the task bar and there is no foreground window

         string title = GetWindowTitle(foregroundIntPtr);
         if (title == "Program Manager") return null; //desktop was selected - might be a better way to detect this


         return new WindowToResize()
         {
            IntPtr = foregroundIntPtr,
            Title = title
         };
      }

      public void MoveTo(int x, int y, int width, int height)
      {
         //the shownormal line is mostly for Chrome - when Chrome is maximized, we can't resize it. so we break windows unto an "unmaximized" state before trying to resize them
         ShowWindowAsync(IntPtr, SW_SHOWNORMAL);

         MoveWindow(IntPtr, x, y, width, height, true);
      }

      public Rect GetPosition()
      {
         Rect rect = new Rect();
         GetWindowRect(IntPtr, ref rect);
         return rect;
      }


   }
}
