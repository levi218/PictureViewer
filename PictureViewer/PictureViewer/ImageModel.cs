using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PictureViewer
{
    public class ImageModel : INotifyPropertyChanged
    {
        public enum LoadingMode { ThumbnailOnly, FullImage }
        public LoadingMode Mode { get; set; }
        public ImageModel()
        {
            PropertyChanged += OnPropertyChanged;
        }
        public ImageModel(string file)
        {
            PropertyChanged += OnPropertyChanged;
            CurrentFile = file;
            Mode = LoadingMode.FullImage;
        }
        public ImageModel(string file, LoadingMode mode)
        {
            PropertyChanged += OnPropertyChanged;
            CurrentFile = file;
            Mode = mode;
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
        }

        private string _currentFile;

        public string CurrentFile
        {
            get { return _currentFile; }
            set
            {
                if (value == _currentFile) return;
                _currentFile = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentFile"));
                //UpdateImage();
            }
        }
        public void Fetch()
        {
            var file = this.CurrentFile;
            if (Mode == LoadingMode.ThumbnailOnly) { 
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(file,UriKind.RelativeOrAbsolute);
            image.DecodePixelWidth = 200;
            image.EndInit();
            image.Freeze(); // important
            this.CurrentImage = image;
            }
            else { UpdateImage(); }
        }
        private ImageSource _currentImage;

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource CurrentImage
        {
            get { return _currentImage; }
            set
            {
                if (Equals(value, _currentImage)) return;
                _currentImage = value;
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentImage"));
            }
        }
        private async void UpdateImage()
        {
            var file = this.CurrentFile;
            // this is asynchronous and won't block UI
            // first generate rough preview
            this.CurrentImage = await Generate(file, 200);
            // then generate quality preview
            if (Mode == LoadingMode.FullImage) {
                this.CurrentImage = await Generate(file, 1920);
            }
        }

        private Task<BitmapImage> Generate(string file, int scale)
        {
            return Task.Run(() =>
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(file);
                image.DecodePixelWidth = scale;
                image.EndInit();
                image.Freeze(); // important
                return image;
            });
        }
    }
}
