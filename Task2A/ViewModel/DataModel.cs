using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System.Collections.Generic;
using SixLabors.Fonts;
using System.Globalization;
using static System.Net.Mime.MediaTypeNames;
using System.Runtime.CompilerServices;
using System;
using System.Windows;
using System.ComponentModel;
using System.Linq;
// using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SixLabors.ImageSharp.Formats.Jpeg;
//using ONNX_detect;
using ONNX_detect1;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Input;
using Microsoft.ML;
//using static ONNX_detect.ClassDetection;
using static ONNX_detect1.ClassDetection;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using System.Reflection.Emit;
namespace ViewModel
{
    public class ListData
    {
        public string ImagePath { get; set; }
        public Image<Rgb24> OriginalImage { get; set; }
        public BitmapSource miniImage { get; set; }
        public BitmapSource DetectedImage { get; set; }
        public string Label { get; set; } 
        public double conf { get; set; }
        public ListData(string path, Image<Rgb24> image_init, string label, double confidence, BitmapSource detect, BitmapSource MiniImage)
        {
            this.ImagePath = (string)path.Clone();
            this.conf = confidence;
            this.OriginalImage = image_init.Clone();
            this.Label = (string)label.Clone();
            this.DetectedImage = detect;
            this.miniImage = MiniImage;
        }
    }
       
    public class DataModel: INotifyPropertyChanged //:// INotifyPropertyChanged
    {
        public string testing;
        public string testing_event
        {
            get
            {
                return testing;
            }
            set
            {
                testing = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("testing"));
                }
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public List<ListData> items { get; set; }
        public bool isExecuting;
        public List<Image<Rgb24>> loaded_images { get; set; }
        public List<BitmapSource> loaded_miniImages { get; set; }
        public string[]? files { get; set; }
        public List<string> filenames { get; set; }
        //public ICommand ExecuteCommand { get; private set; }

        public (List<Image<Rgb24>>, List<DataItem>) results;

        private BitmapSource chosenImage;
        public BitmapSource ChosenImage
        {
            get => chosenImage;
            set
            {
                chosenImage = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("ChosenImage"));
                }
            }
        }
        public DataModel()
        {
            testing = "default";
            isExecuting = false;
            files = null;
            filenames = new List<string>();
            loaded_images = new List<Image<Rgb24>>();
            items = new List<ListData>();
            loaded_miniImages = new List<BitmapSource>();
            //ExecuteCommand = new AsyncCommand.AsyncRelayCommand(Detecting, CanDetect);// x => FolderName != string.Empty && !isExecuting);
            // ChosenImage = null;
            // return_create = "constructed";
        }
        public async Task Detection()
        {
            try
            {
                results = await ClassDetection.ObjectDetection(loaded_images, files);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error in work of neural network");
            }
        }
        /*public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            Bitmap bitmap;
            using (var outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }*/
        /*public Bitmap GetBitmap(BitmapSource source)
        {
            Bitmap bmp = new Bitmap
            (
              source.PixelWidth,
              source.PixelHeight,
              System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            BitmapData data = bmp.LockBits
            (
                new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size),
                ImageLockMode.WriteOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppPArgb
            );

            source.CopyPixels
            (
              Int32Rect.Empty,
              data.Scan0,
              data.Height * data.Stride,
              data.Stride
            );

            bmp.UnlockBits(data);

            return bmp;
        }*/
        /*public BitmapSource GetBitmapSource(Bitmap bitmap)
        {
            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHBitmap
            (
                bitmap.GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );

            return bitmapSource;
        }*/
        public async Task PredictForImages()
        {
            items.Clear();
            try
            {
                isExecuting = true;
                testing = "sit1";
                await Detection();
                testing = "sit2";
                for (int i = 0; i < loaded_images.Count(); i++)
                {
                    var image = loaded_images[i];
                    var label = results.Item2[i].label;

                    //var label = "label";
                    var detected = results.Item1[i];
                    var bitmap_image = ImageToBitmap(detected);// detected);
                    var conf = results.Item2[i].conf;
                    /*var h = (int)results.Item2[i].h;
                    var w = (int)results.Item2[i].w;
                    var x = (int)results.Item2[i].x;
                    var y = (int)results.Item2[i].y;
                    //var temp = detected.Clone(i => i.Crop(new SixLabors.ImageSharp.Rectangle(x, y, w, h)));
                    var temp = detected.Crop(new SixLabors.ImageSharp.Rectangle(x, y, w, h));
                    byte[] pixels = new byte[detected.Width * detected.Height * 3];
                    detected.CopyPixelDataTo(pixels);*/
                    /*byte[] new_pixels = new byte[w * h * 3];
                    for(int j = 0; j < w; j++)
                    {
                        for (int k = 0; k < h; k++)
                        {
                            for (int l = 0; l < 3; l++)
                            {
                                //int i1 = l + k * 3 + j * h * 3;
                                int i1 = l + j * 3 + k * w * 3;
                                //int i1 = k + l * h + j * h * 3;
                                //int i1 = k + j * h + l * h * w;
                                //int i1 = j + k * w + l * h * w;
                                //int i1 = j + l * w + k * w * 3;
                                //int j1 = l + (k + y) * 3 + (j + x) * h * 3;
                                //int j1 = l + (j + x) * 3 + (k + y) * w * 3;
                                //int j1 = (k + y) + l * h + (j + x) * h * 3;
                                //int j1 = (k + y) + (j + x) * h + l * h * w;
                                //int j1 = (j + x) + (k + y) * w + l * h * w;
                                //int j1 = (j + x) + l * w + (k + y) * w * 3;
                                new_pixels[i1] = pixels[j1];
                            }
                        }
                    }*/
                    //var crop_image = BitmapFrame.Create(w, h, w, h, PixelFormats.Rgb24, null, pixels, 3 * w);
                    //foreach 
                    //private static Image cropImage(Image img, Rectangle cropArea)
                    //{
                    //System.Drawing.Rectangle cropArea = new System.Drawing.Rectangle((int)x, (int)y, (int)w, (int)h);

                    //Bitmap target = new Bitmap((int)cropArea.Width, (int)cropArea.Height);
                    //using (Graphics g = Graphics.FromImage(target))
                    //{
                    //    g.DrawImage(GetBitmap(bitmap_image),
                    //        new System.Drawing.Rectangle(0, 0, target.Width, target.Height),
                    //        cropArea,
                    //        GraphicsUnit.Pixel);
                    //}
                    //var miniImage = GetBitmapSource(target);
                    //Bitmap bmpImage = BitmapFromSource(bitmap_image());
                    //Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
                    //public ListData(string path, Image<Rgb24> image_init, string label, double confidence, Bitmap detect)
                    this.items.Add(new ListData(filenames[i], image, label, conf, bitmap_image, loaded_miniImages[i]));
                }
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error in work of neural network");
            }
            finally
            {
                isExecuting = false;
            }
        }

        public void LoadFiles(string[] files, string foldername)
        {
            isExecuting = true;
            if (files != null)
            {
                this.files = (string[])files.Clone();
                foreach (var file in this.files)
                {
                    if (file.EndsWith(".jpg"))
                    {
                        this.filenames.Add(file);
                    }
                }
                
            }
            isExecuting = false;
        }
        public void LoadImages()
        {
            try
            {
                isExecuting = true;
                if(filenames.Count > 0)
                {
                    for (int i = 0; i < filenames.Count(); i++)
                    {
                        if (filenames[i].EndsWith(".jpg"))
                        {
                            var image = SixLabors.ImageSharp.Image.Load<Rgb24>(filenames[i]);
                            this.loaded_images.Add(image);
                            var miniimage = ImageToBitmap(image);
                            this.loaded_miniImages.Add(miniimage);
                        }
                    }
                }
                isExecuting = false;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }
        private BitmapSource ImageToBitmap(Image<Rgb24> image)
        {
            byte[] pixels = new byte[image.Width * image.Height * 3];
            image.CopyPixelDataTo(pixels);
            var bitmap_image = BitmapFrame.Create(image.Width, image.Height, 100, 100, PixelFormats.Rgb24, null, pixels, 3 * image.Width);
            return bitmap_image;
        }
        public record ObjectBox(double XMin, double YMin, double XMax, double YMax, double Confidence, int Class)
        {
            public double IoU(ObjectBox b2) =>
                (Math.Min(XMax, b2.XMax) - Math.Max(XMin, b2.XMin)) * (Math.Min(YMax, b2.YMax) - Math.Max(YMin, b2.YMin)) /
                ((Math.Max(XMax, b2.XMax) - Math.Min(XMin, b2.XMin)) * (Math.Max(YMax, b2.YMax) - Math.Min(YMin, b2.YMin)));
        }
        /*private Bitmap CutImage(Image<Rgb24> image, DataItem data)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle((int)data.x, (int)data.y, (int)data.w, (int)data.h);
            byte[] pixels = new byte[image.Width * image.Height * 3];
            image.CopyPixelDataTo(pixels);
            int width = image.Width;
            int height = image.Height;
            Bitmap? detected;
            using (var ms = new MemoryStream(pixels))
            {
                var bmp = new Bitmap(ms);
                detected = bmp;//.Clone(rectangle, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            }
            return detected;
            //var bmp = new Bitmap(width, height); //создаем битмап
            //Graphics g = Graphics.FromImage(bmp);
            //g.DrawImage( (pic, 0, 0, rectangle, GraphicsUnit.Pixel); //перерисовываем с источника по координатам
            //return bmp;
            //return detected;
        }*/

    }
}