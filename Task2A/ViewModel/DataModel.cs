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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp.Formats.Jpeg;
using ONNX_detect;
namespace ViewModel
{
    public class ListData
    { 
        public string ImagePath { get; set; }
        public Image<Rgb24> image { get; set; }
        public string Label { get; set; }
        public float conf { get; set; }
        public ListData(string path, Image<Rgb24> image_init, string label, float confidence)
        {
            this.ImagePath = (string)path.Clone();
            this.conf = confidence;
            this.image = image_init.Clone();
            this.Label = (string)label.Clone();
        }
    }
    public class DataModel: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] String propertyName = "") =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        public List<Image<Rgb24>> loaded_images { get; set; }
        public string[]? files { get; set; }
        public List<string> filenames { get; set; }// = new List<string>(arr);
        //string[]? filenames { get; set; }
        public List<string> FileNames => filenames;
        public List<ListData> items { get; set; }
        public Image<Rgb24>? ChosenImage;

        public DataModel()//List<Image<Rgb24>> images, List<string> filenames, )
        {
            files = null;
            filenames = new List<string>();
            loaded_images = new List<Image<Rgb24>>();
            items = new List<ListData>();
            ChosenImage = null;
        }
        public void LoadFiles(string[] files)
        {
            if (files != null)
            {
                this.files = (string[])files.Clone();
                filenames = new List<string>(files);
            }
        }
        public void LoadImages()
        {
            if(filenames.Count > 0)
            {
                //filenames = (string[])files.Clone();
                for (int i = 0; i < filenames.Count(); i++)
                {
                    try
                    {
                        var image = SixLabors.ImageSharp.Image.Load<Rgb24>(filenames[i]);
                        loaded_images.Add(image);
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }
                }
            }
        }
        public void ImagesToList()
        {
            for(int i = 0; i < loaded_images.Count(); ++i)
            {
                items.Add(new ListData(filenames[i], loaded_images[i], "label", (float)0.1));
            }
        }
        public async Task CreateList()
        {
            (List<Image<Rgb24>>, List<DataItem>) results;

            //results = await ClassDetection.ObjectDetection(loaded_images, files);
            /*for(int i = 0; i < results.Item1.Count; ++i)
            {
                double x = (double)results.Item2[i].x;
                var y = results.Item2[i].y;
                var w = results.Item2[i].w;
                var h = results.Item2[i].h;
                var name = results.Item2[i].filename;
                var label = results.Item2[i].label;
                //var rec = new Rectangle((int)x, (int)y, (int)w, (int)h);
                var segment = results.Item1[i].Clone(j => j.Crop(rec));
                items.Add(new ListData(name, loaded_images[i], label, (float)1.0));
            }*/
            items.Add(new ListData(filenames[0], loaded_images[0], "label", (float)0.1));
            items.Add(new ListData(filenames[1], loaded_images[1], "label", (float)0.1));
        }
    }
}