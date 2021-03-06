using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EyeDropper
{
    /// <summary>
    /// Interaction logic for CustomCursor.xaml
    /// </summary>
    public partial class CustomCursor : UserControl
    {
        public CustomCursor()
        {
            InitializeComponent();
            this.Loaded += new RoutedEventHandler(CustomCursor_Loaded);
        }

        void CustomCursor_Loaded(object sender, RoutedEventArgs e)
        {
            Cursor = new Cursor(AppDomain.CurrentDomain.BaseDirectory + @"eyedropper.cur");
        }
    }
}
