using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PictureViewer
{
    public class RecognitionModel
    {

        public RecognitionModel(int label, string face, string full, bool processed)
        {
            LabelInt = label;
            FaceImageUri = face;
            FullImageUri = full;
            Processed = processed;
            Distance = 0;
        }
        public int LabelInt { get; set; }
        public String Label { get { return PV_Recognizer.dic_labels[LabelInt]; } }

        private ImageModel _Face;
        public ImageModel Face { get { _Face.Fetch(); return _Face; } private set { _Face = value; } }

        private ImageModel _FullImage;
        public ImageModel FullImage { get { _FullImage.Fetch(); return _FullImage; } private set { _FullImage = value; } }

        private String _FaceImageUri;
        public String FaceImageUri
        {
            get { return _FaceImageUri; }
            set
            {
                _FaceImageUri = value;
                if (_Face != null)
                    _Face.CurrentFile = value;
                else _Face = new ImageModel(value, ImageModel.LoadingMode.ThumbnailOnly);

                FaceImageCV = new Image<Gray, byte>(value).Resize(100, 100, Inter.Cubic).Mat;
                
            }
        }
        private String _FullImageUri;
        public String FullImageUri
        {
            get
            {
                return _FullImageUri;
            }
            set
            {
                _FullImageUri = value;
                if (_FullImage != null)
                    _FullImage.CurrentFile = value;
                else _FullImage = new ImageModel(value, ImageModel.LoadingMode.ThumbnailOnly);
            }
        }
        public Mat FaceImageCV { get; private set; }
        public bool Processed { get; set; }

        public double Distance { get; set; }
        public String StrDistance { get { return "Distance: " + Distance; } }
        public override string ToString()
        {
            return LabelInt + "|" + FaceImageUri + "|" + FullImageUri + "|" + (Processed ? 1 : 0)+"|"+Distance;
        }
    }
}
