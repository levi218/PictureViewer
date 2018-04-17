using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PictureViewer
{
    public class Setting : INotifyPropertyChanged
    {
        private static Setting _Setting = null;

        public event PropertyChangedEventHandler PropertyChanged;

        public static Setting Current
        {
            get
            {
                if (_Setting == null)
                {
                    _Setting = new Setting();
                }
                return _Setting;
            }
        }
        public void Init()
        {
            using (StreamReader reader = new StreamReader(new BufferedStream(new FileStream("settings.txt", FileMode.OpenOrCreate, FileAccess.Read), 512)))
            {
                try { 
                _IsRecognitionEnabled = bool.Parse(reader.ReadLine());
                }
                catch { }
            }
        }
        public void Save() {
            using (StreamWriter writer = new StreamWriter(new BufferedStream(new FileStream("settings.txt", FileMode.OpenOrCreate, FileAccess.Write), 512)))
            {
                writer.Write(_IsRecognitionEnabled);
                writer.Flush();
            }
        }
        private bool _IsRecognitionEnabled;
        public bool IsRecognitionEnabled
        {
            get
            {
                return _IsRecognitionEnabled;
            }
            set
            {
                _IsRecognitionEnabled = value;
                PropertyChanged(this, new PropertyChangedEventArgs("IsRecognitionEnabled"));
            }
        }
    }
}

