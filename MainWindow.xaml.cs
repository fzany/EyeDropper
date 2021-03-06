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
using System.Windows.Threading;
using Microsoft.Win32;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

namespace EyeDropper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            if (timer == null)
            {
                timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 50);
                timer.Tick += new EventHandler(timer_Tick);
            }
            this.Closing += new System.ComponentModel.CancelEventHandler(MainWindow_Closing);
        }

        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ResetCursorToDefault();
        }


        private BitmapSource screenimage;

        private byte[] pixels;

        private DispatcherTimer timer;

        private Point? previousposition = null;

        private Brush backBrush;

        private bool ineyedropmode;

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                rectcolor.Fill = backBrush;
                txt.Text = InteropHelper.ConvertToString(rectcolor.Fill);
                ResetCursorToDefault();
            }
            base.OnPreviewKeyDown(e);
        }

        private void ResetCursorToDefault()
        {
            timer.Stop();

            RegistryKey pRegKey = Registry.CurrentUser;
            pRegKey = pRegKey.OpenSubKey(@"Control Panel\Cursors");
            foreach (string key in paths.Keys)
            {
                string path = paths[key];
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors", key, path);
            }
            InteropHelper.SystemParametersInfo(InteropHelper.SPI_SETCURSORS, 0, null, InteropHelper.SPIF_UPDATEINIFILE | InteropHelper.SPIF_SENDCHANGE);
        }

        private List<string> AllColors = new List<string>();
        void timer_Tick(object sender, EventArgs e)
        {
            System.Drawing.Point _point = System.Windows.Forms.Control.MousePosition;
            Point point = new Point(_point.X, _point.Y);
            if (previousposition == null || previousposition != point)
            {
                if (screenimage != null)
                {
                    int stride = (screenimage.PixelWidth * screenimage.Format.BitsPerPixel + 7) / 8;
                    pixels = new byte[screenimage.PixelHeight * stride];
                    Int32Rect rect = new Int32Rect((int)point.X, (int)point.Y, 1, 1);
                    screenimage.CopyPixels(rect, pixels, stride, 0);
                    rectcolor.Fill = new SolidColorBrush(Color.FromRgb(pixels[2], pixels[1], pixels[0]));
                    var new_Hex = InteropHelper.ConvertToString(rectcolor.Fill);
                    txt.Text = new_Hex;
                    if (!AllColors.Contains(new_Hex))
                        AllColors.Add(new_Hex);
                }
            }
            if (System.Windows.Forms.Control.MouseButtons == System.Windows.Forms.MouseButtons.Left)
            {
                timer.Stop();
                ResetCursorToDefault();

                //Save Color to a file. 
                System.IO.File.WriteAllText(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "golders.txt"), JsonConvert.SerializeObject(AllColors));
            }
            previousposition = point;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            screenimage = InteropHelper.CaptureRegion(InteropHelper.GetDesktopWindow(),
                                                                       (int)SystemParameters.VirtualScreenLeft,
                                                                       (int)SystemParameters.VirtualScreenTop,
                                                                       (int)SystemParameters.PrimaryScreenWidth,
                                                                       (int)SystemParameters.PrimaryScreenHeight);
            backBrush = rectcolor.Fill;
            if (paths.Count() > 0)
            {
                ResetCursorToDefault();
            }
            else
            {
                timer.Start();
                ineyedropmode = true;
                ChangeCursor();
            }
        }

        private Dictionary<string, string> paths = new Dictionary<string, string>();

        private void ChangeCursor()
        {
            RegistryKey pRegKey = Registry.CurrentUser;
            pRegKey = pRegKey.OpenSubKey(@"Control Panel\Cursors");
            paths.Clear();
            foreach (var key in pRegKey.GetValueNames())
            {
                Object _key = pRegKey.GetValue(key);
                //Take a backup.
                paths.Add(key, _key.ToString());
                Object val = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors", key, null);
                Registry.SetValue(@"HKEY_CURRENT_USER\Control Panel\Cursors", key, AppDomain.CurrentDomain.BaseDirectory + @"eyedropper.cur");
            }

            InteropHelper.SystemParametersInfo(InteropHelper.SPI_SETCURSORS, 0, null, InteropHelper.SPIF_UPDATEINIFILE | InteropHelper.SPIF_SENDCHANGE);
        }
    }
}
