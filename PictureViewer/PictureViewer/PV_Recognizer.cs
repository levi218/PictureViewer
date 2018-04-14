using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.Util;
using System.Drawing;
using Emgu.CV.Structure;
using System.Diagnostics;
using Emgu.CV.Face;
using Emgu.CV.CvEnum;
using System.IO;
using System.Collections;
using System.Windows.Media.Imaging;
#if !(__IOS__ || NETFX_CORE)
using Emgu.CV.Cuda;
#endif

namespace PictureViewer
{
    public class PV_Recognizer
    {
        private static EigenFaceRecognizer _recognizer = null;
        public static EigenFaceRecognizer Recognizer
        {
            get
            {
                if (_recognizer == null) _recognizer = new EigenFaceRecognizer();
                return _recognizer;
            }
        }
        private static bool Trained = false;

        public static Dictionary<int, String> dic_labels;
        //public static List<int> list_labels;
        //private static List<string> list_images;
        //private static List<string> checked_uri;
        public static List<RecognitionModel> training_sets;
        public static List<string> GetNames()
        {
            List<string> result = new List<string>();
            if (dic_labels != null) result.AddRange(dic_labels.Values.ToList());
            return result;
        }
        public static List<RecognitionModel> Init()
        {
            //List<RecognitionModel> models = new List<RecognitionModel>();
            training_sets = new List<RecognitionModel>();
            //checked_uri = new List<string>();
            //list_labels = new List<int>();
            //list_images = new List<string>();
            dic_labels = new Dictionary<int, string>();
            //List<Image<Gray, Byte>> trainingImages = new List<Image<Gray, Byte>>();
            //trainingImages[0] = new Image<Gray, byte>("QA.jpg").Resize(100, 100, Inter.Cubic);
            using (StreamReader reader = new StreamReader(new BufferedStream(new FileStream("labels.txt", FileMode.OpenOrCreate, FileAccess.Read), 512)))
            {
                while (!reader.EndOfStream)
                {
                    string[] vals = reader.ReadLine().Split('|');
                    dic_labels.Add(int.Parse(vals[0]), vals[1]);
                }
            }
            using (StreamReader reader = new StreamReader(new BufferedStream(new FileStream("training_sets.txt", FileMode.OpenOrCreate, FileAccess.Read), 512)))
            {
                while (!reader.EndOfStream)
                {
                    string[] vals = reader.ReadLine().Split('|');
                    //TrainingSet trainingSet = new TrainingSet { Label = int.Parse(vals[0]), FaceImageUri = vals[1], FullImageUri = vals[2], Processed = int.Parse(vals[3])==0?false:true };
                    RecognitionModel trainingSet = new RecognitionModel(int.Parse(vals[0]), vals[1], vals[2], int.Parse(vals[3]) == 0 ? false : true);
                    if (vals.Length > 4) trainingSet.Distance = double.Parse(vals[4]);
                    training_sets.Add(trainingSet);
                    //if (!trainingSet.Processed) models.Add(trainingSet);
                    // TODO: return everything, not just unprocessed, but need a people filter in UI
                    //models.Add(trainingSet);
                    //list_labels.Add(int.Parse(vals[0]));
                    //list_images.Add(vals[1]);
                    //checked_uri.Add(vals[2]);
                    //trainingImages.Add(new Image<Gray, byte>(vals[1]).Resize(100, 100, Inter.Cubic));
                }
            }
            //int[] labels = new int[] { 0, 1 };
            // string[] str_labels = new string[] { "QA,Huy" };
            //if (trainingImages.Count > 0 && dic_labels.Keys.Count > 0)
            if (training_sets.Count > 0)
            {
                //Recognizer.Train<Gray, Byte>(trainingImages.ToArray(), list_labels.ToArray());
                IEnumerable<RecognitionModel> arr = training_sets.Where(x =>  x.Processed);
                Recognizer.Train<Gray, byte>(arr.Select(x => x.FaceImageCV).ToArray(), arr.Select(x => x.LabelInt).ToArray());
                Trained = true;
            }

            Console.WriteLine(training_sets.Count);
            return training_sets;
        }
        public static Task<List<RecognitionModel>> InitAsync()
        {
            return Task.Run(() =>
            {
                return Init();
            });
        }
        public static void Save()
        {
            using (StreamWriter writer = new StreamWriter(new BufferedStream(new FileStream("labels.txt", FileMode.OpenOrCreate, FileAccess.Write), 512)))
            {
                foreach (int i in dic_labels.Keys)
                {
                    writer.WriteLine(i + "|" + dic_labels[i]);
                }
            }
            using (StreamWriter writer = new StreamWriter(new BufferedStream(new FileStream("training_sets.txt", FileMode.OpenOrCreate, FileAccess.Write), 512)))
            {
                //for (int i = 0; i < list_labels.Count; i++)
                foreach(RecognitionModel set in training_sets)
                {
                    //writer.WriteLine(list_labels[i] + "|" + list_images[i] + "|" + checked_uri[i]);
                    writer.WriteLine(set.ToString());
                }
            }

        }
        public static void Train(RecognitionModel model, String name)
        {
            //REWRITE
            //RecognitionModel ts = training_sets.FirstOrDefault(x => x.FaceImageUri == model.Face.CurrentFile);
            model.Processed = true;
            if (dic_labels.Values.Contains(name)) {
                model.LabelInt = dic_labels.FirstOrDefault(x=>x.Value==name).Key;
            }
            else
            {
                int num = dic_labels.Count;
                dic_labels.Add(num, name);
                model.LabelInt = num;
            }
            //Recognizer.Train<Gray, byte>(new[] { model.FaceImageCV }, new int[] { model.LabelInt});

            /*model.Label = name;
            int label;
            if (!dic_labels.ContainsValue(name))
            {
                label = dic_labels.Count + 1;
                dic_labels.Add(label, name);
            }
            else
            {
                label = dic_labels.First(x => x.Value == name).Key;
            }*/


            //return null;
        }

        public static List<RecognitionModel> Recognize(IInputArray image, String uri)
        {
            if (training_sets.Exists(x=>x.FullImageUri==uri)) return new List<RecognitionModel>();
            List<Rectangle> faces = new List<Rectangle>();
            //List<Rectangle> eyes = new List<Rectangle>();
            long detectionTime;
            Detect(image, "haarcascade_frontalface_default.xml", /*"haarcascade_eye.xml",*/ faces, /*eyes,*/ out detectionTime);
            List<RecognitionModel> results = new List<RecognitionModel>();
            foreach (Rectangle r in faces)
            {
                IImage face = new UMat((UMat)image, r);
                String path = "training_set\\" + Path.GetRandomFileName() + ".jpg";
                new Image<Bgr,byte>(face.Bitmap).Resize(100,100,Inter.Cubic).Save(path);
                //TrainingSet training_set = new TrainingSet { FaceImageUri = path, FullImageUri = uri, Processed = false };
                RecognitionModel rm = new RecognitionModel(0, path, uri, false) ;// { Face = new ImageModel(path, ImageModel.LoadingMode.ThumbnailOnly), FullImage = new ImageModel(uri, ImageModel.LoadingMode.ThumbnailOnly) };
                if (Trained)
                {
                    using (Image<Gray, Byte> f = new Image<Gray, byte>(face.Bitmap))
                    {

                        FaceRecognizer.PredictionResult predictionResults = Recognizer.Predict(f.Resize(100, 100, Inter.Cubic));
                        Console.WriteLine(predictionResults.Distance + "     " + predictionResults.Label);
                        rm.Distance = predictionResults.Distance;
                        if (predictionResults.Distance < 6000 && dic_labels.ContainsKey(predictionResults.Label))
                        {
                            //list_labels.Add(predictionResults.Label);
                            rm.LabelInt = predictionResults.Label;
                            //rm.Label = dic_labels[predictionResults.Label];
                        }
                        else
                        {
                            //list_labels.Add(0);
                            rm.LabelInt = 0;
                            //rm.Label = "Unknown";
                        }
                    }
                    //Console.WriteLine("Recognition result: " + predictionResults.Label + "   " + predictionResults.Distance);
                    //(face.Bitmap).Save(i + ".jpg");
                }
                else
                {
                    //list_labels.Add(0);
                    //training_set.Label = 0;
                    rm.LabelInt = 0;// "Unknown";
                }
                
                results.Add(rm);
                training_sets.Add(rm);

                //foreach (Rectangle e in eyes)
                //{
                //    IImage eye = new UMat((UMat)image, e);
                //    (eye.Bitmap).Save("e" + i + ".jpg");
                //    i++;
                //}
            }
            return results;
        }
        public static Task<List<RecognitionModel>> RecognizeAsync(IInputArray image, String uri)
        {
            return Task.Run(() =>
            {
                return Recognize(image, uri);
            });
        }
        public static CascadeClassifier face = null;
        public static CascadeClassifier eye = null;

        public static void Detect(
         IInputArray image, String faceFileName, /*String eyeFileName,*/
         List<Rectangle> faces, /*List<Rectangle> eyes,*/
         out long detectionTime)
        {
            Stopwatch watch;
            if (face == null) face = new CascadeClassifier(faceFileName);
            //if (eye == null) eye = new CascadeClassifier(eyeFileName);

            using (InputArray iaImage = image.GetInputArray())
            {

/*#if !(__IOS__ || NETFX_CORE)
                if (iaImage.Kind == InputArray.Type.CudaGpuMat && CudaInvoke.HasCuda)
                {
                    Console.WriteLine("CUDA MODE");
                    using (CudaCascadeClassifier face = new CudaCascadeClassifier(faceFileName))
                    using (CudaCascadeClassifier eye = new CudaCascadeClassifier(eyeFileName))
                    {
                        face.ScaleFactor = 1.1;
                        face.MinNeighbors = 10;
                        face.MinObjectSize = Size.Empty;
                        eye.ScaleFactor = 1.1;
                        eye.MinNeighbors = 10;
                        eye.MinObjectSize = Size.Empty;
                        watch = Stopwatch.StartNew();
                        using (CudaImage<Bgr, Byte> gpuImage = new CudaImage<Bgr, byte>(image))
                        using (CudaImage<Gray, Byte> gpuGray = gpuImage.Convert<Gray, Byte>())
                        using (GpuMat region = new GpuMat())
                        {
                            face.DetectMultiScale(gpuGray, region);
                            Rectangle[] faceRegion = face.Convert(region);
                            //faces.AddRange(faceRegion);
                            Console.WriteLine(faceRegion.Length);

                            bool[] checking_arr = new bool[faceRegion.Length];
                            for (int i = 0; i < faceRegion.Length; i++)
                            {
                                checking_arr[i] = true;
                            }
                            for (int i = 0; i < faceRegion.Length; i++)
                            {
                                for (int j = 0; j < faceRegion.Length; j++)
                                    if (i != j)
                                    {
                                        Rectangle f1 = faceRegion[i];
                                        Rectangle f2 = faceRegion[j];
                                        Console.WriteLine(f1.ToString());
                                        Console.WriteLine(f2.ToString());

                                        if (f1.IntersectsWith(f2))
                                        {
                                            Rectangle f_inter = Rectangle.Intersect(f1, f2);
                                            Console.WriteLine(f_inter.Height * f_inter.Width + "   " + f1.Height * f1.Width + "   " + f2.Height * f2.Width);
                                            if ((f1.Height * f1.Width > f2.Height * f2.Width)
                                                && (f1.Height * f1.Width * 0.1 < f_inter.Height * f_inter.Width))
                                            {
                                                checking_arr[j] = false;
                                            }
                                        }
                                    }
                            }
                            for (int i = 0; i < faceRegion.Length; i++)
                            {
                                if (!checking_arr[i]) continue;
                                Rectangle f = faceRegion[i];
                                using (CudaImage<Gray, Byte> faceImg = gpuGray.GetSubRect(f))
                                {

                                    //For some reason a clone is required.
                                    //Might be a bug of CudaCascadeClassifier in opencv
                                    using (CudaImage<Gray, Byte> clone = faceImg.Clone(null))
                                    using (GpuMat eyeRegionMat = new GpuMat())
                                    {
                                        eye.DetectMultiScale(clone, eyeRegionMat);
                                        Rectangle[] eyeRegion = eye.Convert(eyeRegionMat);
                                        if (eyeRegion.Length > 0) faces.Add(f);
                                        foreach (Rectangle e in eyeRegion)
                                        {
                                            Rectangle eyeRect = e;
                                            eyeRect.Offset(f.X, f.Y);
                                            eyes.Add(eyeRect);
                                        }
                                    }
                                }
                            }
                        }
                        watch.Stop();
                    }
                }
                else
#endif*/
                {
                    Console.WriteLine("Normal MODE");
                    //Read the HaarCascade objects
                    // using (CascadeClassifier face = new CascadeClassifier(faceFileName))
                    // using (CascadeClassifier eye = new CascadeClassifier(eyeFileName))
                    {
                        watch = Stopwatch.StartNew();

                        using (UMat ugray = new UMat())
                        {
                            CvInvoke.CvtColor(image, ugray, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);

                            //normalizes brightness and increases contrast of the image
                            CvInvoke.EqualizeHist(ugray, ugray);

                            //Detect the faces  from the gray scale image and store the locations as rectangle
                            //The first dimensional is the channel
                            //The second dimension is the index of the rectangle in the specific channel                     
                            Rectangle[] facesDetected = face.DetectMultiScale(
                               ugray,
                               1.1,
                               10,
                               new Size(20, 20));

                            //faces.AddRange(facesDetected);
                            Console.WriteLine(facesDetected.Length);

                            bool[] checking_arr = new bool[facesDetected.Length];
                            for (int i = 0; i < facesDetected.Length; i++)
                            {
                                checking_arr[i] = true;
                            }

                            for (int i = 0; i < facesDetected.Length; i++)
                            {
                                for (int j = 0; j < facesDetected.Length; j++)
                                    if (i != j)
                                    {
                                        Rectangle f1 = facesDetected[i];
                                        Rectangle f2 = facesDetected[j];
                                        // Console.WriteLine(f1.ToString());
                                        // Console.WriteLine(f2.ToString());

                                        if (f1.IntersectsWith(f2))
                                        {
                                            Rectangle f_inter = Rectangle.Intersect(f1, f2);
                                            //Console.WriteLine(f_inter.Height * f_inter.Width + "   " + f1.Height * f1.Width + "   " + f2.Height * f2.Width);
                                            if ((f1.Height * f1.Width > f2.Height * f2.Width)
                                                && (f1.Height * f1.Width * 0.1 < f_inter.Height * f_inter.Width))
                                            {
                                                checking_arr[j] = false;
                                            }
                                        }
                                    }
                            }
                            //for (int i = 0; i < facesDetected.Length; i++) Console.WriteLine(checking_arr[i]);
                            for (int i = 0; i < facesDetected.Length; i++)
                            {
                                if (!checking_arr[i]) continue;

                                //        foreach (Rectangle f in facesDetected)
                                //  {
                                //Get the region of interest on the faces
                                Rectangle f = facesDetected[i];
                                faces.Add(f);
                                /*using (UMat faceRegion = new UMat(ugray, f))
                                {
                                    Rectangle[] eyesDetected = eye.DetectMultiScale(
                                       faceRegion,
                                       1.1,
                                       10,
                                       new Size(20, 20));
                                    if (eyesDetected.Length > 0)
                                    {
                                        
                                    }

                                    foreach (Rectangle e in eyesDetected)
                                    {
                                        Rectangle eyeRect = e;
                                        eyeRect.Offset(f.X, f.Y);
                                        eyes.Add(eyeRect);
                                    }
                                }*/
                            }
                        }
                        watch.Stop();
                    }
                }
                detectionTime = watch.ElapsedMilliseconds;
            }
        }
    }
}
