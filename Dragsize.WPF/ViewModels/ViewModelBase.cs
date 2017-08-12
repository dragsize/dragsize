using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace Dragsize.WPF.ViewModels
{
   public abstract class ViewModelBase : DependencyObject, INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;
      public void NotifyPropertyChanged(string property)
      {
         this.VerifyPropertyName(property);

         if (PropertyChanged != null)
         {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
         }
      }

      [Conditional("DEBUG")]
      [DebuggerStepThrough]
      public void VerifyPropertyName(string propertyName)
      {
         // Verify that the property name matches a real,  
         // public, instance property on this object.
         if (TypeDescriptor.GetProperties(this)[propertyName] == null)
         {
            string msg = "Invalid property name: " + propertyName + ".";
            throw new Exception(msg);
         }
      }
   }
}
