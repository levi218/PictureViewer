using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

    public partial class MainWindow : Window, INotifyPropertyChanged
    {

        public ObservableCollection<ImageModel> list_images;
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
        private enum DisplayMode { IndividualPic, Folder };
        Storyboard story_main_layout;

        public double Width_Col1
        {
            get
            {
                switch (mode)
                {
                    case DisplayMode.IndividualPic:
                        return 0;
                    case DisplayMode.Folder:
                        {
                            double desired = (this.ActualWidth - 45 - SystemParameters.VerticalScrollBarWidth) * 0.25;
                            return desired < 0 ? 0 : desired;
                        }
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
                    case DisplayMode.IndividualPic:
                        {
                            double desired = (this.ActualWidth - 45 - SystemParameters.VerticalScrollBarWidth) * 0.25;
                            return desired < 0 ? 0 : desired;
                        }
                    case DisplayMode.Folder:
                        {
                            double desired = (this.ActualWidth - 45 - SystemParameters.VerticalScrollBarWidth) * 0.75;
                            return desired < 0 ? 0 : desired;
                        }
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
                    case DisplayMode.IndividualPic:
                        {
                            double desired = (this.ActualWidth - 45 - SystemParameters.VerticalScrollBarWidth) * 0.75;
                            return desired < 0 ? 0 : desired;
                        }
                    case DisplayMode.Folder:
                        return 0;
                    default:
                        return 0;
                }
            }
        }
        public double ThumbnailsWidth
        {
            get
            {
                double result;
                switch (mode)
                {
                    case DisplayMode.IndividualPic:
                        {
                            result = Width_Col2 - 22 - SystemParameters.VerticalScrollBarWidth;
                            break;
                        }
                    case DisplayMode.Folder:
                        {
                            result = (Width_Col2 - 66 - SystemParameters.VerticalScrollBarWidth) / 3;
                            break;
                        }
                    default:
                        result = 0;
                        break;
                }
                CoordinateConverter.ThumbnailsWidth = result;
                return result;
            }
        }
        public double ThumbnailsHeight
        {
            get
            {
                return ThumbnailsWidth * 9 / 16;
            }
        }

        private DisplayMode mode = DisplayMode.Folder;
        private object dummyNode = null;
        private String SelectedFolder;
        public ObservableCollection<String> FavoriteFolders { get; set; }
        public Brush BtnFavoriteBg
        {
            get
            {
                if (FavoriteFolders != null && FavoriteFolders.Contains(SelectedFolder))
                {
                    return new SolidColorBrush(Colors.LightGoldenrodYellow);
                }
                else return new SolidColorBrush(Colors.Gray);
            }
        }

        public ObservableCollection<String> People { get; set; }
        public ObservableCollection<RecognitionModel> PeopleImage { get; set; }
        private BackgroundWorker recognizerWorker { get; set; }
        public MainWindow()
        {
            People = new ObservableCollection<string>();
            InitRecognizer();
            //PV_Recognizer.Recognize(new UMat(@"C:\Users\theph\OneDrive\Pictures\Screenshots\Screenshot (109).png", ImreadModes.Color));
            list_images = new ObservableCollection<ImageModel>();
            FavoriteFolders = new ObservableCollection<String>();
            using (StreamReader reader = new StreamReader(new BufferedStream(new FileStream("favorite.txt", FileMode.OpenOrCreate, FileAccess.Read), 512)))
            {
                while (!reader.EndOfStream)
                {
                    FavoriteFolders.Add(reader.ReadLine());
                }
            }
            InitializeComponent();
            story_main_layout = this.FindResource("story_main_layout") as Storyboard;
            icImages.ItemsSource = list_images;

            //tvi_fav_folders.ItemsSource = FavoriteFolders;
        }
        private async void InitRecognizer()
        {
            PeopleImage = new ObservableCollection<RecognitionModel>(await PV_Recognizer.InitAsync());
            //People = new ObservableCollection<String>();
            List<string> names = PV_Recognizer.GetNames();
            foreach (string name in names) { People.Add(name); }
            recognizerWorker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = true
            };
            recognizerWorker.DoWork += Worker_DoWork;
            recognizerWorker.ProgressChanged += Worker_ProgressChanged;

            recognizerWorker.RunWorkerAsync();
            //Recognize();
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            List<string> checked_file = new List<string>();
            using (StreamReader reader = new StreamReader(new BufferedStream(new FileStream("checked_file.txt", FileMode.OpenOrCreate, FileAccess.Read), 512)))
            {
                while (!reader.EndOfStream)
                {
                    checked_file.Add(reader.ReadLine());
                }
            }


            using (StreamWriter writer = new StreamWriter(new BufferedStream(new FileStream("checked_file.txt", FileMode.Append, FileAccess.Write), 512)))
            {
                int i = PV_Recognizer.training_sets.Count;
                //while (!worker.CancellationPending)
                {
                    //System.Threading.Thread.Sleep(2000);
                    IEnumerable<FileInfo> files = FavoriteFolders.SelectMany(x => (new DirectoryInfo(x).GetFilesByExtensions(".jpg", ".bmp", ".png")))
                                                    .Where(x => !checked_file.Contains(x.FullName) && x.Length < 10000000);
                    foreach (FileInfo f in files)
                    {
                        List<RecognitionModel> result = (PV_Recognizer.Recognize(new UMat(f.FullName, ImreadModes.Color), f.FullName));
                        writer.WriteLine(f.FullName);
                        //foreach (RecognitionModel rm in result) PeopleImage.Add(rm);
                        worker.ReportProgress(0, result);
                        
                        while ((PV_Recognizer.training_sets.Where(x => (!x.Processed)).Count() > 15) && !worker.CancellationPending) System.Threading.Thread.Sleep(2000); ;
                        if(PV_Recognizer.training_sets.Count-i>30)
                        {
                            Console.WriteLine("Re-training recognizer....");
                            IEnumerable<RecognitionModel> arr = PV_Recognizer.training_sets.Where(x => x.Processed);
                            PV_Recognizer.Recognizer.Train<Gray, byte>(arr.Select(x => x.FaceImageCV).ToArray(), arr.Select(x => x.LabelInt).ToArray());
                            i = PV_Recognizer.training_sets.Count;
                            Console.WriteLine("Training completed!");

                        }
                    }
                }
            }
        }

        private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            List<RecognitionModel> result = (List<RecognitionModel>)e.UserState;
            foreach (RecognitionModel rm in result) PeopleImage.Add(rm);
            if(icImages.ItemsSource!=list_images)
            icImages.ItemsSource = PeopleImage.Where(x => x.Processed == false);
        }
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
            story_main_layout.Begin();
        }

        void folder_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
            if (item.Header.ToString() == "Unknown" && item.Tag.ToString() == "Unknown")
            {
                icImages.ItemsSource = PeopleImage.Where(x => x.Processed == false);
                return;
            }
            else
            {
                if (icImages.ItemsSource != list_images) icImages.ItemsSource = list_images;
            }
            list_images.Clear();
            IEnumerable<FileInfo> files = new DirectoryInfo(item.Tag.ToString()).GetFilesByExtensions(".jpg", ".bmp", ".png");
            SelectedFolder = item.Tag.ToString();
            int i = 0;
            foreach (FileInfo file in files)
            {
                ImageModel im = new ImageModel(file.FullName, ImageModel.LoadingMode.ThumbnailOnly);
                list_images.Add(im);
                i++;
            }
            RaiseEvent(new RoutedEventArgs(RoutedPropertyChangedEvent));
            Parallel.ForEach(list_images, new ParallelOptions { MaxDegreeOfParallelism = 5 }, im =>
             {
                 im.Fetch();
             });
            e.Handled = true;
        }

        private void Image_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Image source = sender as Image;
            mode = DisplayMode.IndividualPic;
            RaiseEvent(new RoutedEventArgs(RoutedPropertyChangedEvent));
            ImageModel im = new ImageModel(source.Tag.ToString());
            im.Fetch();
            img_large.DataContext = im;

        }

        void folder_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = (TreeViewItem)sender;
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
            e.Handled = true;

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ReEvaluateElements();
        }
        private void ReEvaluateElements()
        {
            PropertyChanged(this, new PropertyChangedEventArgs("Width_Col1"));
            PropertyChanged(this, new PropertyChangedEventArgs("Width_Col2"));
            PropertyChanged(this, new PropertyChangedEventArgs("Width_Col3"));
            PropertyChanged(this, new PropertyChangedEventArgs("ThumbnailsWidth"));
            PropertyChanged(this, new PropertyChangedEventArgs("ThumbnailsHeight"));
            PropertyChanged(this, new PropertyChangedEventArgs("BtnFavoriteBg"));
            PropertyChanged(this, new PropertyChangedEventArgs("FavoriteFolders"));
            PropertyChanged(this, new PropertyChangedEventArgs("PeopleImage"));
            PropertyChanged(this, new PropertyChangedEventArgs("People"));


        }
        private void _this_StateChanged(object sender, EventArgs e)
        {
            ReEvaluateElements();
        }

        private void btn_folder_Click(object sender, RoutedEventArgs e)
        {
            mode = DisplayMode.Folder;
            scroll_directories.Visibility = Visibility.Visible;
            scroll_favorites.Visibility = Visibility.Hidden;
            RaiseEvent(new RoutedEventArgs(RoutedPropertyChangedEvent));
        }

        private void _this_Activated(object sender, EventArgs e)
        {
            ReEvaluateElements();
        }

        private void btn_favorite_Click(object sender, RoutedEventArgs e)
        {
            mode = DisplayMode.Folder;
            scroll_directories.Visibility = Visibility.Hidden;
            scroll_favorites.Visibility = Visibility.Visible;

            RaiseEvent(new RoutedEventArgs(RoutedPropertyChangedEvent));
        }

        private void btn_Favorite_Click_1(object sender, RoutedEventArgs e)
        {
            if (FavoriteFolders.Contains(SelectedFolder)) FavoriteFolders.Remove(SelectedFolder);
            else FavoriteFolders.Add(SelectedFolder);
            RaiseEvent(new RoutedEventArgs(RoutedPropertyChangedEvent));
        }

        private void _this_Closing(object sender, CancelEventArgs e)
        {
            PV_Recognizer.Save();
            using (StreamWriter writer = new StreamWriter("favorite.txt", false))
            {
                foreach (String s in FavoriteFolders)
                {
                    writer.WriteLine(s);
                }
            }
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            Console.WriteLine("entered");
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            RecognitionModel model = button.Tag as RecognitionModel;
            TextBox textBox = (button.Parent as DockPanel).Children.OfType<TextBox>().FirstOrDefault();
            string name = textBox.Text;
            PV_Recognizer.Train(model, name);
            icImages.ItemsSource = PeopleImage.Where(x => x.Processed == false);
        }

        private void BtnSkip_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            RecognitionModel model = button.Tag as RecognitionModel;
            PV_Recognizer.Train(model, "Unknown");
            icImages.ItemsSource = PeopleImage.Where(x => x.Processed == false);
        }
    }
}
