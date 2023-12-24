using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Globalization;
using System.Security.Cryptography;
using System.Windows.Markup;
using System.Windows.Forms;
using ViewModel;
using static System.Net.WebRequestMethods;
using SixLabors.ImageSharp.PixelFormats;
using AsyncCommand;
using System.Reflection.Emit;
using SixLabors.ImageSharp;
using System.Reflection.Metadata;
using System.Data.Common;
using System.Threading;
using System.Xml.Linq;

namespace Task2A
{
    public class CustomCommands
    {
        public static RoutedCommand ChooseFromFileCommand =
                new RoutedCommand("Get directory", typeof(CustomCommands));
        public static RoutedCommand CalculateCommand =
                new RoutedCommand("Images from directory", typeof(CustomCommands));
        //public static RoutedCommand StopCommand =
          //      new RoutedCommand("Stop calculating", typeof(CustomCommands));
        //<CommandBinding Command = "{x:Static local:CustomCommands.StopCommand}"
        //        CanExecute = "IsExecuting" Executed="StopExecuting"/>
    }
    public partial class MainWindow : Window
    {
        DataModel viewData = new DataModel();
        private readonly ICommand calcCommand;
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = viewData;
        }
        private void CorrectDirectoryName(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                if (viewData.isExecuting)
                {
                    e.CanExecute = false;
                }
                else
                {
                    e.CanExecute = true;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                e.CanExecute = false;
            }
        }

        private void FromDirectoryClick(object sender, ExecutedRoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowser = new FolderBrowserDialog();

            DialogResult result = folderBrowser.ShowDialog();

            string[]? files = null;
            if (!string.IsNullOrWhiteSpace(folderBrowser.SelectedPath))
            {
                var foldername = folderBrowser.SelectedPath;
                files = Directory.GetFiles(foldername);
                viewData.LoadFiles(files, foldername);
                DataContext = null;
                DataContext = viewData;
            }
        }
        private void CorrectImagesDirectory(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var files = viewData.filenames;
                bool can_execute = false;
                /*if (isExecuting)
                {
                    can_execute = false;
                }*/
                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        can_execute = file.EndsWith(".jpg") ? true : can_execute;
                    }
                    if (!can_execute)
                    {
                        System.Windows.MessageBox.Show("no .jpg in directory");
                    }
                }
                else
                {
                    //System.Windows.MessageBox.Show("no files");
                    can_execute = false;
                }
                e.CanExecute = can_execute;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                e.CanExecute = false;
            }
        }

        //private bool isExecuting;

        ///*public event EventHandler? CanExecuteChanged
        //{
        //    add { CommandManager.RequerySuggested += value; }
        //    remove { CommandManager.RequerySuggested -= value; }
        //}*/


        private async void FromDirectoryExecute(object sender, ExecutedRoutedEventArgs e)
        {
            viewData.LoadImages();
            //System.Windows.MessageBox.Show("have items");
            await viewData.PredictForImages();
            //System.Windows.MessageBox.Show(viewData.items.Count.ToString());
            //viewData.ChosenImage = (ListData)itemlist.SelectedItem;
            this.DataContext = null;
            this.DataContext = viewData;
        }
        private void listbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            viewData.ChosenImage = (ListData)itemlist.SelectedItem;
            this.DataContext = null;
            this.DataContext = viewData;
        }


        //    //if (!isExecuting)
        //    //{
        //    //    isExecuting = true;
        //        //await Task.Delay(1000);
        //        //Func<object, Task> executeAsync = async _ => await Task.Delay(1000);//viewData.CreateList();//Task.Delay(1000);
        //        //await executeAsync(1).ContinueWith(_ =>
        //        //{

        //    //await
        //    //viewData.CreateList();
        //    //int n1 = 4, n2 = 5;
        //    //Task<int> sumTask = new Task<int>(() => Sum(n1, n2));
        //    //sumTask.Start();
        //    //var outer = Task.Factory.StartNew(() =>      // внешняя задача
        //    //{
        //        //System.Windows.MessageBox.Show("outer task");
        //        //Console.WriteLine("Outer task starting...");



        //    //viewData.CreateList();


        //    //});
        //    //outer.Wait();
        //    //int result = sumTask.Result;
        //    //Console.WriteLine($"{n1} + {n2} = {result}"); // 4 + 5 = 9

        //    //int Sum(int a, int b) => a + b;
        //    //   isExecuting = false;
        //    //    CommandManager.InvalidateRequerySuggested();
        //    //}, scheduler: TaskScheduler.FromCurrentSynchronizationContext());
        //    //}


        //    /*.ContinueWith(_ =>
        //        {
        //            isExecuting = false;
        //            CommandManager.InvalidateRequerySuggested();
        //        }, scheduler: TaskScheduler.FromCurrentSynchronizationContext());
        //    //}
        //    DataContext = null;
        //    DataContext = viewData;
        //    //viewData.ImagesToList();
        //    //await viewData.ProccessImages();
        //    /*startCommand = new AsyncRelayCommand(async _ =>
        //    {
        //        await viewData.ProcessImages();
        //    });*/
        //    System.Windows.MessageBox.Show(viewData.return_create);
        //    if (viewData.items.Count > 0)
        //    {
        //        System.Windows.MessageBox.Show("have items");
        //    }
        //    directory_declaration.Text = directory_declaration.Text + "!";
        //private void usersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        //{
        //    //var item = sender as ListData;// System.Windows.Forms.ListViewItem;
        //    /*if (ListImages.SelectedItem is ListData)// && item. IsSelected())
        //    {
        //        viewData.ChosenImage = ((ListData)ListImages.SelectedItem).image.Clone();
        //        //System.Windows.Forms.MessageBox.Show("changed!");
        //    }
        //    DataContext = null;
        //    DataContext = viewData;*/
        //}
        /*private void IsExecuting(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var files = viewData.filenames;
                bool can_execute = false;
                if (files.Count > 0)
                {
                    can_execute = true;
                }
                can_execute = can_execute && !viewData.isExecuting;
                e.CanExecute = can_execute;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
                e.CanExecute = false;
            }
        }
        // "IsExecuting" Executed="StopExecuting"
        private void StopExecuting(object sender, ExecutedRoutedEventArgs e)
        {
            
        }*/
    }
}
