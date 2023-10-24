using System;
using ONNX_detect;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.Serialization.Formatters;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using CsvHelper.Delegates;
using CsvHelper.TypeConversion;
using System.Globalization;
using System.Text;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;

public class Program
{
    static void CreateCsv(string filename, (List<Image<Rgb24>>, List<DataItem>) results)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            Delimiter = ";",
            Encoding = Encoding.UTF8,
        };

        var writer = new StreamWriter(filename);
        var csv_writer = new CsvWriter(writer, config);
        for (int i = 0; i < results.Item1.Count(); i++)
            results.Item1[i].Save($"{results.Item2[i].filename}.jpg");
        foreach (var dataitem in results.Item2)
        {
            csv_writer.WriteRecord(dataitem);
            csv_writer.NextRecord();
        }
        writer.Close();
    }

	static async Task Main(string[] args)
	{
        string filename = "detection results.csv";
        List<Image<Rgb24>> images = new List<Image<Rgb24>>();

        string[] filenames = (string[])args.Clone();
        if (args.Length == 0)
            filenames = new string[2] { "chair.jpg", "cat.jpg" };
        for(int i = 0; i < filenames.Length; i++)
        {
            try
            {
                var image = Image.Load<Rgb24>(filenames[i]);
                images.Add(image);
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); }
        }
        (List<Image<Rgb24>>, List<DataItem>) results;
        results = await ClassDetection.ObjectDetection(images, filenames);
        CreateCsv(filename, results);
    }
}
