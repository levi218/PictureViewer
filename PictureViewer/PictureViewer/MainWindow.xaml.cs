using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PictureViewer
{
    /*public class Img : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private String _image;
        public String image
        {
            get { return _image; }
            set
            {
                _image = value;
                PropertyChanged(this, new PropertyChangedEventArgs("image"));
            }
        }
        public Img(String url) {
            _image = url;
        }
    }*/
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public static readonly RoutedEvent RoutedPropertyChangedEvent = EventManager.RegisterRoutedEvent("RoutedPropertyChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(MainWindow));

        public event RoutedEventHandler RoutedPropertyChanged
        {
            add
            {
                this.AddHandler(RoutedPropertyChangedEvent, value);
            }

            remove
            {
                this.RemoveHandler(RoutedPropertyChangedEvent, value);
            }
        }
        //public ObservableCollection<Img> MyList
        //{
        //    get;set;
        //}
        private const int MODE_INDIVIDUAL_PIC = 0;
        private const int MODE_FOLDER = 1;
        Storyboard story_main_layout;
        public double Width_Col1
        {
            get
            {
                switch (mode)
                {
                    case MODE_INDIVIDUAL_PIC:
                        return 0;
                    case MODE_FOLDER:
                        return (this.ActualWidth - 40) * 0.3<0?150: (this.ActualWidth - 40) * 0.3;
                    default:
                        return 0;
                }

            }
        }
        public double Width_Col2
        {
            get
            {
                switch (mode)
                {
                    case MODE_INDIVIDUAL_PIC:
                        return (this.ActualWidth - 40) *0.3;
                    case MODE_FOLDER:
                        return (this.ActualWidth - 40) * 0.7;
                    default:
                        return 0;
                }
            }
        }
        public double Width_Col3
        {
            get
            {
                switch (mode)
                {
                    case MODE_INDIVIDUAL_PIC:
                        return (this.ActualWidth - 40) * 0.7;
                    case MODE_FOLDER:
                        return 0;
                    default:
                        return 0;
                }
            }
        }
        private int mode = 1;
        
        public MainWindow()
        {
            InitializeComponent();
            story_main_layout = this.FindResource("story_main_layout") as Storyboard;
            Uri uri = new Uri(@"C:\Users\theph\OneDrive\Pictures\Screenshots\Screenshot (125).png");
            BitmapImage bitmapImage = new BitmapImage(uri);
            img_large.Source = bitmapImage;

        }
        private object dummyNode = null;
        
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            foreach (string s in Directory.GetLogicalDrives())
            {
                TreeViewItem item = new TreeViewItem();
                item.Header = s;
                item.Tag = s;
                item.FontWeight = FontWeights.Normal;
                if (Directory.EnumerateDirectories(s).Where(f => (new FileInfo(f).Attributes & FileAttributes.Hidden & FileAttributes.System) == 0).Count() > 0)
                    item.Items.Add(dummyNode);
                item.Expanded += new RoutedEventHandler(folder_Expanded);
                item.Selected += new RoutedEventHandler(folder_Selected);
                treev_directories.Items.Add(item);
            }
            RoutedPropertyChanged += MainWindow_RoutedPropertyChanged;
        }

        private void MainWindow_RoutedPropertyChanged(object sender, RoutedEventArgs e)
        {
            ReEvaluateElements();
        }

        void folder_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            wrapp_images.Children.Clear();
            IEnumerable<FileInfo> files = new DirectoryInfo(item.Tag.ToString()).GetFilesByExtensions(".jpg", ".bmp", ".png");
            Console.WriteLine(item.Tag.ToString() + files.Count());
            wrapp_images.BeginInit();
            foreach (FileInfo file in files)
            {
                Console.WriteLine(file.FullName);
                Border border = new Border { BorderBrush = Brushes.DarkCyan, BorderThickness = new Thickness(1) };
                Uri uri = new Uri(file.FullName);
                BitmapImage bitmapImage = new BitmapImage(uri);
                Image img = new Image { Width = 120, Height = 96, Stretch = Stretch.Uniform, Margin = new Thickness(10), Source = bitmapImage };
                border.BeginInit();
                border.Child = img;
                border.EndInit();
                wrapp_images.Children.Add(border);
            }
            wrapp_images.EndInit();
            e.Handled = true;
        }
        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Items[0] == dummyNode)
            {
                item.Items.Clear();
                try
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(item.Tag.ToString());
                    foreach (DirectoryInfo dir in dirInfo.EnumerateDirectories().Where(d => (d.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0))
                    {
                        TreeViewItem subitem = new TreeViewItem();
                        subitem.Header = dir.Name;
                        subitem.Tag = dir.FullName;
                        subitem.FontWeight = FontWeights.Normal;
                        if (dir.EnumerateDirectories().Where(f => (f.Attributes & (FileAttributes.Hidden | FileAttributes.System)) == 0).Count() > 0)
                            subitem.Items.Add(dummyNode);
                        subitem.Expanded += new RoutedEventHandler(folder_Expanded);
                        subitem.Selected += new RoutedEventHandler(folder_Selected);
                        item.Items.Add(subitem);
                    }
                }
                catch (Exception)
                {
                }
            }
            e.Handled = true;

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Console.WriteLine("Triggered");
            ReEvaluateElements();
        }
        private void ReEvaluateElements()
        {
            PropertyChanged(this, new PropertyChangedEventArgs("Width_Col1"));
            PropertyChanged(this, new PropertyChangedEventArgs("Width_Col2"));
            PropertyChanged(this, new PropertyChangedEventArgs("Width_Col3"));
        }
        private void _this_StateChanged(object sender, EventArgs e)
        {
            ReEvaluateElements();
        }

        private void btn_folder_Click(object sender, RoutedEventArgs e)
        {
            mode = (mode==MODE_FOLDER ? MODE_INDIVIDUAL_PIC : MODE_FOLDER);
            Console.WriteLine(mode);
            Console.WriteLine(Width_Col1);
            RaiseEvent(new RoutedEventArgs(RoutedPropertyChangedEvent));
            //story_main_layout.Begin();
            //ReEvaluateElements();
        }

        private void _this_Activated(object sender, EventArgs e)
        {
            ReEvaluateElements();
        }
    }
}
