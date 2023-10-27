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

namespace Task2A
{
    public class CustomCommands
    {
        public static RoutedCommand ChooseFromFileCommand =
                new RoutedCommand("Get directory", typeof(CustomCommands));
        public static RoutedCommand CalculateCommand =
                new RoutedCommand("Images from directory", typeof(CustomCommands));
        /*<CommandBinding Command="{x:Static local:CustomCommands.ChooseFromFileCommand}"
                CanExecute = "CorrectDirectoryName" Executed="FromDirectoryClick"/>
        <CommandBinding Command="{x:Static local:CustomCommands.CalculateCommand}"
                CanExecute = "CorrectImagesDirectory" Executed="FromDirectoryExecute"/>*/
    }
    public partial class MainWindow : Window
    {
        DataModel viewData = new DataModel();
        //private ICommand startCommand;
        public MainWindow()
        {
            InitializeComponent();
            /*startCommand = new AsyncRelayCommand(async _ =>
            {
                System.Windows.Forms.MessageBox.Show("in command");
                viewData.CreateList();

            });*/
            this.DataContext = viewData;
            //function_list.ItemsSource = Enum.GetValues(typeof(ClassLibrary1.FRawEnum));
        }
        //public ICommand StartCommand => startCommand;
        private void CorrectDirectoryName(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                e.CanExecute = true;
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
                files = Directory.GetFiles(folderBrowser.SelectedPath);
                viewData.LoadFiles(files);
                DataContext = null;
                DataContext = viewData;
            }
        }
        private void CorrectImagesDirectory(object sender, CanExecuteRoutedEventArgs e)
        {
            try
            {
                var files = viewData.filenames;
                bool can_execute = true;
                if (files.Count > 0)
                {
                    foreach (var file in files)
                    {
                        can_execute = file.EndsWith(".jpg") ? can_execute : false;
                    }
                }
                else
                {
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

        private bool isExecuting;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }


        //private ICommand startCommand;
        private async void FromDirectoryExecute(object sender, ExecutedRoutedEventArgs e)
        {
            viewData.LoadImages();
            //viewData.ImagesToList();

            //System.Windows.Forms.MessageBox.Show("in executing");
            //if (!isExecuting)
            //{
            //    isExecuting = true;
            //await Task.Delay(1000);
            await viewData.CreateList();/*.ContinueWith(_ =>
                {
                    isExecuting = false;
                    CommandManager.InvalidateRequerySuggested();
                }, scheduler: TaskScheduler.FromCurrentSynchronizationContext());
            //}*/
            DataContext = null;
            DataContext = viewData;
            //viewData.ImagesToList();
            //await viewData.ProccessImages();
            /*startCommand = new AsyncRelayCommand(async _ =>
            {
                await viewData.ProcessImages();
            });
            public ICommand StartCommand => startCommand;
            //await StartCommand;
            DataContext = null;
            DataContext = viewData;
            */
        }
        private void usersList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var item = sender as ListData;// System.Windows.Forms.ListViewItem;
            if (ListImages.SelectedItem is ListData)// && item. IsSelected())
            {
                viewData.ChosenImage = ((ListData)ListImages.SelectedItem).image.Clone();
                //System.Windows.Forms.MessageBox.Show("changed!");
            }
            DataContext = null;
            DataContext = viewData;
        }
    }
}
