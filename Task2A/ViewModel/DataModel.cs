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
using ONNX_detect;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.Windows.Input;
using Microsoft.ML;
//using static ONNX_detect.ClassDetection;
using static ONNX_detect.ClassDetection;
namespace ViewModel
{
    public class ListData
    {
        public string ImagePath { get; set; }
        public Image<Rgb24> OriginalImage { get; set; }
        public BitmapSource DetectedImage { get; set; }
        public string Label { get; set; } 
        public double conf { get; set; }
        public ListData(string path, Image<Rgb24> image_init, string label, double confidence, BitmapSource detect)
        {
            this.ImagePath = (string)path.Clone();
            this.conf = confidence;
            this.OriginalImage = image_init.Clone();
            this.Label = (string)label.Clone();
            //if(detect == null)
            //    this.DetectedImage = null;
            //else
                this.DetectedImage = detect;
        }
    }
       
    public class DataModel: INotifyPropertyChanged //:// INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler PropertyChanged;
        //protected void RaisePropertyChanged([CallerMemberName] String propertyName = "") =>
        //       PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
        private string folderName { get; set; }
        /*public string FolderName
        {
            get
            {
                return folderName;
            }
            set
            {
                folderName = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("folderName"));
                }
            }
        }*/
        public bool isExecuting;
        public List<Image<Rgb24>> loaded_images { get; set; }
        public string[]? files { get; set; }
        public List<string> filenames { get; set; }
        // public Image<Rgb24>? ChosenImage;
        // public string return_create
        public ICommand ExecuteCommand { get; private set; }

        public (List<Image<Rgb24>>, List<DataItem>) results;
        //public ICommand StopExecCommand { get; private set; }

        //public ICommand ExecuteCommand => calcCommand;
        public DataModel()//List<Image<Rgb24>> images, List<string> filenames, )
        {
            testing = "default";
            isExecuting = false;
            files = null;
            filenames = new List<string>();
            loaded_images = new List<Image<Rgb24>>();
            items = new List<ListData>();
            //ExecuteCommand = new AsyncCommand.AsyncRelayCommand(Detecting, CanDetect);// x => FolderName != string.Empty && !isExecuting);
            // ChosenImage = null;
            // return_create = "constructed";
        }
        public async Task Detection()
        {
            try
            {
                results = await ClassDetection.ObjectDetection(loaded_images, files);
                //int j = 0;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error in work of neural network");
            }

            //return results;
        }
        public static Bitmap BitmapFromSource(BitmapSource bitmapsource)
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
        }
        public async Task PredictForImages()
        {
            items.Clear();
            //RaisePropertyChanged(nameof(DetectedImages));
            try
            {
                isExecuting = true;
                //this.LoadImages();
                //(List<Image<Rgb24>>, List<DataItem>) results;
                testing = "sit1";
                //results =
                await Detection(); //await ClassDetection.ObjectDetection(loaded_images, files);
                testing = "sit2";
                //Console.WriteLine(results.Item2.Count);
                for (int i = 0; i < loaded_images.Count(); i++)
                {
                    var image = loaded_images[i];
                    var label = results.Item2[i].label;

                    //var label = "label";
                    var detected = results.Item1[i];
                    var bitmap_image = ImageToBitmap(detected);// detected);
                    var conf = results.Item2[i].conf;
                    var h = results.Item2[i].h;
                    var w = results.Item2[i].w;
                    var x = results.Item2[i].x;
                    var y = results.Item2[i].y;
                    //private static Image cropImage(Image img, Rectangle cropArea)
                    //{
                    System.Drawing.Rectangle cropArea = new System.Drawing.Rectangle((int)x, (int)y, (int)w, (int)h);
                    //Bitmap bmpImage = BitmapFromSource(bitmap_image());
                    //Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
                    //public ListData(string path, Image<Rgb24> image_init, string label, double confidence, Bitmap detect)
                    this.items.Add(new ListData(filenames[i], image, label, conf, bitmap_image));
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
        private  ListData chosenImage;
        public ListData ChosenImage
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
        /*public async Task Detecting(object obj)
        {
            items.Clear();
            //RaisePropertyChanged(nameof(DetectedImages));
            try
            {
                isExecuting = true;
                LoadImages();
                (List<Image<Rgb24>>, List<DataItem>) results;
                results = await ClassDetection.ObjectDetection(loaded_images, files);
                for (int i = 0; i < loaded_images.Count(); i++)
                {
                    var image = SixLabors.ImageSharp.Image.Load<Rgb24>(filenames[i]);
                    this.loaded_images.Add(image);
                    var label = results.Item2[i].label;
                    var detected = results.Item1[i];
                    var bitmap_image = ImageToBitmap(detected);
                    //                            public ListData(string path, Image<Rgb24> image_init, string label, double confidence, Bitmap detect)
                    this.items.Add(new ListData(filenames[i], image, label, 0.0, bitmap_image));

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
        public bool CanDetect(object obj)
        {
            try
            {
                //var files = viewData.filenames;
                bool can_execute = false;
                if (filenames.Count > 0)
                {
                    foreach (var file in filenames)
                    {
                        can_execute = file.EndsWith(".jpg") ? true : can_execute;
                    }
                    if (!can_execute)
                    {
                        throw new Exception("No .jpg images");
                    }
                }
                else
                {
                    //System.Windows.MessageBox.Show("no files");
                    can_execute = false;
                }
                return can_execute;
            }
            catch (Exception ex)
            {
                throw new Exception("Errors in image reader");
            }
        }*/
        public void LoadFiles(string[] files, string foldername)
        {
            isExecuting = true;
            if (files != null)
            {
                //FolderName = (string)foldername.Clone();
                this.files = (string[])files.Clone();
                //this.filenames = new List<string>();
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
                            //var bitmap_image = ImageToBitmap(image);
//                            public ListData(string path, Image<Rgb24> image_init, string label, double confidence, Bitmap detect)
                            //this.items.Add(new ListData(filenames[i], image, filenames[i], 0.0, bitmap_image));
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
        /*private static Image<Rgb24> CutImage(Image<Rgb24> original, ObjectBox bbox)
        {
            int x = (int)bbox.XMin, y = (int)bbox.YMin;
            int width = (int)(bbox.XMax - bbox.XMin), height = (int)(bbox.YMax - bbox.YMin);
            if (x < 0)
            {
                width += x;
                x = 0;
            }
            if (y < 0)
            {
                height += y;
                y = 0;
            }
            if (x + width > TargetSize)
            {
                width = TargetSize - x;
            }
            if (y + height > TargetSize)
            {
                height = TargetSize - y;
            }
            if (x > TargetSize || y > TargetSize)
            {
                return original.Clone(
                    i => i.Resize(resizeOptions)
                );
            }

            return original.Clone(
                i => i.Resize(resizeOptions).Crop(new Rectangle(x, y, width, height))
            );
        }*/

        /*public static Image<Rgb24> Annotate(Image<Rgb24> target, ObjectBox bbox)
        {
            int maxDimension = Math.Max(target.Width, target.Height);
            float scale = (float)maxDimension / TargetSize;
            return target.Clone(ctx =>
            {
                ctx.Resize(new ResizeOptions { Size = new Size(maxDimension, maxDimension), Mode = ResizeMode.Pad }).DrawPolygon(
                    Pens.Solid(Color.Red, 1 + maxDimension / TargetSize),
                    new PointF[] {
                        new PointF((float)bbox.XMin * scale, (float) bbox.YMin * scale),
                        new PointF((float)bbox.XMin * scale, (float) bbox.YMax * scale),
                        new PointF((float)bbox.XMax * scale, (float) bbox.YMax * scale),
                        new PointF((float) bbox.XMax * scale, (float) bbox.YMin * scale)
                    });
            });
        }*/
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

        /*public async void PredictForImages()
        {
            (List<Image<Rgb24>>, List<DataItem>) results;
            if (this.files != null && this.loaded_images.Count > 0)
            {
                //(List<Image<Rgb24>>, List<DataItem>) 
                results = await ClassDetection.ObjectDetection(this.loaded_images, this.files);
                for (int i = 0; i < filenames.Count(); ++i)
                {
                    var detectedImage = CutImage(results.Item1[i], results.Item2[i]);
                    //(string path, Image<Rgb24> image_init, string label, double confidence, Bitmap detect)
                    //results.Item2[i].
                    //this.items.Add(new ListData(filenames[i], loaded_images[i], results.Item2[i].label, results.Item2[i]))
                }
            }
        }*/
        //    for(int i = 0; i < loaded_images.Count(); ++i)
        //    {
        //        //public ListData(string path, Image<Rgb24> image_init, string label, float confidence, Image<Rgb24>? detect)
        //        items.Add(new ListData(filenames[i], loaded_images[i], "label", (float)0.1, null));
        //    }
        //}
        //public async Task CreateList()
        //{
        //    (List<Image<Rgb24>>, List<DataItem>) results;


        //    //await
        //    //Task.Factory.StartNew(() =>  // вложенная задача
        //    //{
        //    //Console.WriteLine("Inner task starting...");
        //    //Thread.Sleep(2000);
        //    //Console.WriteLine("Inner task finished.");


        //    this.return_create = "not detected";

        //    /*var task = ClassDetection.ObjectDetection(loaded_images, files);
        //    results = 


        //    Task<Test> task2 = Task<Test>.Factory.StartNew(() =>
        //    {
        //        string s = ".NET";
        //        double d = 4.0;
        //        return new Test { Name = s, Number = d };
        //    });
        //    Test test = task2.Result;*/
        //    //int n1 = 4, n2 = 5;
        //    //Task<(List<Image<Rgb24>>, List<DataItem>)> DetectTask = new Task<(List<Image<Rgb24>>, List<DataItem>)>(() => ClassDetection.ObjectDetection(loaded_images, files));// Sum(n1, n2));

        //    Task<(List<Image<Rgb24>>, List<DataItem>)> DetectTask = ClassDetection.ObjectDetection(loaded_images, files);
        //    DetectTask.Start();

        //    this.return_create = "detected";
        //    DetectTask.Wait();
        //    //results = DetectTask.Result;
        //    //Console.WriteLine($"{n1} + {n2} = {result}"); // 4 + 5 = 9

        //    //int Sum(int a, int b) => a + b;
        //    //}, TaskCreationOptions.AttachedToParent);

        //    //await Task.Delay(1000);


        //    //if (String.Compare(this.return_create, "contructed") == 0)
        //    //{
        //    //}
        //    //this.return_create = $"{results.Item1.Count.ToString()}";
        //    /*for(int i = 0; i < results.Item1.Count; ++i)
        //    {
        //        double x = (double)results.Item2[i].x;
        //        var y = results.Item2[i].y;
        //        var w = results.Item2[i].w;
        //        var h = results.Item2[i].h;
        //        var name = results.Item2[i].filename;
        //        var label = results.Item2[i].label;
        //        var rec = new SixLabors.ImageSharp.Rectangle((int)x, (int)y, (int)w, (int)h);
        //        var segment = results.Item1[i].Clone(j => j.Crop(rec));
        //        items.Add(new ListData(name, loaded_images[i], label, (float)1.0, segment));
        //        //if(this.return_create == "contructed")
        //        //{
        //        this.return_create = "created";
        //        //}
        //    }*/
        //    /*items.Add(new ListData(filenames[0], loaded_images[0], "label", (float)0.1, null));
        //    items.Add(new ListData(filenames[1], loaded_images[1], "label", (float)0.1, null));*/
    }
}