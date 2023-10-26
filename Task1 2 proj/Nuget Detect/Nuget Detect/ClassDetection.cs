using System;
using System.Net;
using System.Net.Http;
using SixLabors.ImageSharp; // Из одноимённого пакета NuGet
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using SixLabors.Fonts;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Polly;

namespace ONNX_detect
{
    public class DataItem
    {
        public DataItem(string filename, string label, float x, float y, float w, float h)
        {
            this.filename = filename;
            this.label = label;
            this.x = x;
            this.y = y;
            this.w = w;
            this.h = h;
        }
        public string filename { get; set; }
        public string label { get; set; }
        public float x { get; set; }
        public float y { get; set; }
        public float w { get; set; }
        public float h { get; set; }
    }
    public class ClassDetection
    {
        /// Вспомогательные функции ///
        
        private static float Sigmoid(float value)
    	{
            var e = (float)Math.Exp(value);
            return e / (1.0f + e);
        } 

        private static float[] Softmax(float[] values)
        {
            var exps = values.Select(v => Math.Exp(v));
            var sum = exps.Sum();
            return exps.Select(e => (float)(e / sum)).ToArray();
        }
        private static int IndexOfMax(float[] values)
        {
            int idx = 0;
            for(int i = 1;i<values.Length;i++)
                if(values[i] > values[idx])
                    idx = i;
            return idx;
        }

        static CancellationTokenSource cts = new CancellationTokenSource();
        static SemaphoreSlim hasMessages = new SemaphoreSlim(0, 1);
        static SemaphoreSlim boxLock = new SemaphoreSlim(1, 1);
        static Queue<((Image<Rgb24>, string) Input, TaskCompletionSource<(Image<Rgb24>, DataItem)> Result)> mailbox = new();

        /////Константы/////

        static async Task<(Image<Rgb24>, DataItem)> EnqueueT(Image<Rgb24> im, string st)
        {
            await boxLock.WaitAsync();
            if (mailbox.Count == 0)
                hasMessages.Release();
            var r = new TaskCompletionSource<(Image<Rgb24>, DataItem)>();
            mailbox.Enqueue(((im, st), r));
            boxLock.Release();
            return await r.Task;
        }

        static async Task Process()
        {
            while (!cts.Token.IsCancellationRequested)
            {
                await hasMessages.WaitAsync();
                await boxLock.WaitAsync();

                var m = new Queue<((Image<Rgb24>, string) Input, TaskCompletionSource<(Image<Rgb24>, DataItem)> Result)>();
                while (mailbox.Count > 0)
                    m.Enqueue(mailbox.Dequeue());
                boxLock.Release();
                while (m.Count > 0)
                {
                    var item = m.Dequeue();

                    // Выполняем полезные вычисления
                    var session = new InferenceSession(TinyYOLOPath);
                    var result = Prediction(item.Input, session);

                    item.Result.SetResult(result);
                }
            }
        }

        public static async Task<(List<Image<Rgb24>>, List<DataItem>)> ObjectDetection(List<Image<Rgb24>> images, string[] filenames)
        {
            await DownLoadNN();
            (List<Image<Rgb24>>, List<DataItem>) detections = (new List<Image<Rgb24>>(), new List<DataItem>());
            List<Task> tasks = new List<Task>();
            Queue<int> queue = new Queue<int>();
            for (var i = 0; i < images.Count(); i++)
            {
                Console.WriteLine(i);
                queue.Enqueue(i);
                var task = Task.Run(async () =>
                {
                    int j = 0;
                    lock (queue)
                    {
                        j = queue.Dequeue();
                    }
                    (Image<Rgb24>, DataItem) prediction = await EnqueueT(images[j], filenames[j]);
                    lock (detections.Item1)
                    {
                        detections.Item1.Add(prediction.Item1);
                    }
                    lock (detections.Item2)
                    {
                        detections.Item2.Add(prediction.Item2);
                    }
                });
                tasks.Add(task);
            }
            var processTask = Task.Run(Process);
            foreach (var task in tasks)
            {
                await Task.WhenAll(task);
            }
            cts.Cancel();
            await processTask;
            return detections;
        }

        private static readonly string TinyYOLOPath = "tinyyolov2-8.onnx";

        // Размер изображения
        const int TargetSize = 416;
        
        const int CellCount = 13; // 13x13 ячеек
        const int BoxCount = 5; // 5 прямоугольников в каждой ячейке
        const int ClassCount = 20; // 20 классов

        private static readonly string[] labels = new string[]
        {
            "aeroplane", "bicycle", "bird", "boat", "bottle",
            "bus", "car", "cat", "chair", "cow",
            "diningtable", "dog", "horse", "motorbike", "person",
            "pottedplant", "sheep", "sofa", "train", "tvmonitor"
        };

        private static int cellSize = TargetSize / CellCount;

        private static (double, double)[] anchors = new (double, double)[]
        {
            (1.08, 1.19), 
            (3.42, 4.41), 
            (6.63, 11.38), 
            (9.42, 5.11), 
            (16.62, 10.52)
        };

        static async Task DownLoadNN()
        {
            if (!System.IO.File.Exists(TinyYOLOPath))
            {
                var jitterer = new Random();
                var retryPolicy = Policy.Handle<HttpRequestException>()
                    .WaitAndRetryAsync(5,
                        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))
                                      + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)));
                using (var httpClient = new HttpClient())
                {
                    var buffer = await retryPolicy.ExecuteAsync(async () =>
                    {
                        return await httpClient.GetByteArrayAsync("https://storage.yandexcloud.net/dotnet4/tinyyolov2-8.onnx", cts.Token);
                    });
                    await File.WriteAllBytesAsync("tinyyolov2-8.onnx", buffer, cts.Token);
                }
            }
        }

        /*static async Task ConnectSession()
        {
            if (!cts.Token.IsCancellationRequested)
            {
                Console.WriteLine("in connection");
                await DownLoadNN();
                try
                {
                    session = new InferenceSession(TinyYOLOPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }*/

        static (Image<Rgb24>, DataItem) Prediction((Image<Rgb24>, string) image_filename, InferenceSession session)
        {
            Image<Rgb24> image = image_filename.Item1;
            string filename = image_filename.Item2;
            var resized = image.Clone(x =>
            {
                x.Resize(new ResizeOptions
                {
                    Size = new Size(TargetSize, TargetSize),
                    Mode = ResizeMode.Pad // Дополнить изображение до указанного размера с сохранением пропорций
                });
            });
            var input = new DenseTensor<float>(new[] { 1, 3, TargetSize, TargetSize });
            resized.ProcessPixelRows(pa =>
            {
                for (int y = 0; y < TargetSize; y++)
                {
                    Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                    for (int x = 0; x < TargetSize; x++)
                    {
                        input[0, 0, y, x] = pixelSpan[x].R;
                        input[0, 1, y, x] = pixelSpan[x].G;
                        input[0, 2, y, x] = pixelSpan[x].B;
                    }
                }
            });
            
            // Подготавливаем входные данные нейросети. Имя input задано в файле модели
            var inputs = new List<NamedOnnxValue>
            {
               NamedOnnxValue.CreateFromTensor("image", input),
            };

            IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results;
            lock (session)
            {
                results = session.Run(inputs);
            }
            //IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);
            // Получаем результаты
            var outputs = results.First().AsTensor<float>();
            
            var boundingBoxes = resized.Clone();

            List<ObjectBox> objects = new();

            Single max_conf = 0;
            int max_row = 0;
            int max_col = 0;
            int max_box = 0;

            for(var row = 0;row < CellCount;row++)
            {
                for(var col = 0;col < CellCount;col++)
                {
                    for(var box = 0;box<BoxCount;box++)
                    {
                        var conf = Sigmoid(outputs[0, (5 + ClassCount) * box + 4, row, col]);

                        if(conf > max_conf)
                        {
                            max_conf = conf;
                            max_row = row;
                            max_col = col;
                            max_box = box;
                        }
                    }
                }
            }
            var classes = Enumerable.Range(0, ClassCount).Select(i => outputs[0, (5 + ClassCount) * max_box + 5 + i, max_row, max_col]).ToArray();
            var max_ind = IndexOfMax(classes);

            var rawX = outputs[0, (5 + ClassCount) * max_box, max_row, max_col];
            var rawY = outputs[0, (5 + ClassCount) * max_box + 1, max_row, max_col];

            var rawW = outputs[0, (5 + ClassCount) * max_box + 2, max_row, max_col];
            var rawH = outputs[0, (5 + ClassCount) * max_box + 3, max_row, max_col];

            var x = (float)((max_col + Sigmoid(rawX)) * cellSize);
            var y = (float)((max_row + Sigmoid(rawY)) * cellSize);

            var w = (float)(Math.Exp(rawW) * anchors[max_box].Item1 * cellSize);
            var h = (float)(Math.Exp(rawH) * anchors[max_box].Item2 * cellSize);
            //objects.Add(new ObjectBox(x - w / 2, y - h / 2, x + w / 2, y + h / 2, conf, IndexOfMax(Softmax(classes))));
            //boundingBoxes.Save("boundingboxes.jpg");
            resized.Mutate(ctx =>
            {
                ctx.DrawPolygon(
                    Pens.Solid(Color.Green, 1),
                    new PointF[] {
                        new PointF(x - w / 2, y - h / 2),
                        new PointF(x + w / 2, y - h / 2),
                        new PointF(x + w / 2, y + h / 2),
                        new PointF(x - w / 2, y + h / 2)
                    });
            });
            return (resized, new DataItem($"detected_{filename}", labels[max_ind], x, y, w, h));
        }

        public record ObjectBox(double XMin, double YMin, double XMax, double YMax, double Confidence, int Class) 
        {
            public double IoU(ObjectBox b2) =>
                (Math.Min(XMax, b2.XMax) - Math.Max(XMin, b2.XMin)) * (Math.Min(YMax, b2.YMax) - Math.Max(YMin, b2.YMin)) /
                ((Math.Max(XMax, b2.XMax) - Math.Min(XMin, b2.XMin)) * (Math.Max(YMax, b2.YMax) - Math.Min(YMin, b2.YMin)));
        }
    }
}