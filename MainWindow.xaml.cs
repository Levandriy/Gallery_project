using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Gallery_TheSecondSon
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public int total_imgs = -1;
        public double direction = -1;
        public bool Check(string filename)
        {
            foreach (Image img in Gallery_wrap_panel.Children)
            {
                if (new Uri(filename).ToString() == img.Source.ToString())
                {
                    return false;
                }
            }
            return true;
        }
        public void Choose_img(int img_number)
        {
            MainImage.Source = ((Image)Gallery_wrap_panel.Children[img_number]).Source;
        }
        private int Find_current_img()
        {
            //Image local = Gallery_wrap_panel.Children.OfType<Image>().Where(img => img.Source == MainImage.Source).ToArray()[0]; // получить изображение, которое имеет одинаковый сурс с главным изображением
            //int current = Gallery_wrap_panel.Children.IndexOf(local); //получить индекс изображения в галерее
            //return current; //вернуть индекс

            return Gallery_wrap_panel.Children.IndexOf(Gallery_wrap_panel.Children.OfType<Image>().Where(img => img.Source == MainImage.Source).ToArray()[0]);
        }
        /// <summary>
        /// Для обработки спец случаев перехода между 0 и последнем элементом
        /// </summary>
        /// <param name="a">индекс изображения, куда перейти</param>
        /// <returns></returns>
        private int Changing_img(int a)
        {
            if (a < 0)
            {
                return total_imgs;
            }
            else
            {
                return a > total_imgs ? 0 : a;
            }
        }
        private void Add_image(BitmapImage file)
        {
            total_imgs++;
            Image local = new Image()
            {
                Source = file,
                Height = 60,
                Width = 60,
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5, 0, 5, 0),
            };
            local.MouseEnter += new MouseEventHandler(Mouse_image_enter);
            local.MouseLeave += new MouseEventHandler(Mouse_image_leave);
            local.MouseDown += new MouseButtonEventHandler(Mouse_image_click);
            Gallery_wrap_panel.Children.Add(local);
            MainImage.Source = local.Source;
            Bottom_grid.Width = 70 * total_imgs + 70;
        }
        private void Mouse_enter_Element(object sender, MouseEventArgs e)
        {
            Grid local = (Grid)sender;
            local.Background = new SolidColorBrush(Color.FromArgb(33, 0, 0, 0));
        }
        private void Mouse_leave_Element(object sender, MouseEventArgs e)
        {
            Grid local = (Grid)sender;
            local.Background = new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));
        }
        private void Mouse_image_enter(object sender, MouseEventArgs e)
        {
            Image local = (Image)sender;
            //local.Width = 120;
            //local.Height = 120;
        }
        private void Mouse_image_leave(object sender, MouseEventArgs e)
        {
            Image local = (Image)sender;
            local.Width = 60;
            local.Height = 60;
        }
        private void Mouse_image_click(object sender, MouseButtonEventArgs e)
        {
            Image local = (Image)sender;
            MainImage.Source = local.Source;
        }
        private void Add_image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog files = new OpenFileDialog()
            {
                Title = "Выберете фото",
                Filter = "Image Files(*.JPEG;*.JPG;*.PNG)|*.JPEG;*.JPG;*.PNG|JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg",
                Multiselect = true,
                RestoreDirectory = false
            };
            if (files.ShowDialog() == true)
            {
                foreach(string file in files.FileNames)
                {
                    if (Check(file))
                    {
                        Add_image(new BitmapImage(new Uri(file)));
                    }
                }
            }
        }

        private void Last_img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Choose_img(Changing_img(Find_current_img() - 1));
        }

        private void Next_img_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Choose_img(Changing_img(Find_current_img() + 1));
        }

        private void Delete_image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Gallery_wrap_panel.Children.Count > 0)
            {
                int current = Find_current_img();
                Gallery_wrap_panel.Children.Remove(Gallery_wrap_panel.Children[current]);
                //Debug.WriteLine("Первый пошёл");
                total_imgs--;
                Bottom_grid.Width = 70 * total_imgs + 70;
                if (Gallery_wrap_panel.Children.Count != 0)
                {
                    MainImage.Source = ((Image)Gallery_wrap_panel.Children[Changing_img(current - 1)]).Source;
                    //Debug.WriteLine("Второй пошёл");
                }
                else
                {
                    MainImage.Source = null;
                }
                
            }
            else
            {
                MessageBox.Show("Нечего удалять. Добавьте изображения.");
            }
        }

        private void Import_image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Gallery_wrap_panel.Children.Clear();
            OpenFileDialog mypath = new OpenFileDialog();
            mypath.Title = "Выберете файл галереи";
            mypath.Filter = "Gallery Files (*.GAL)|*.GAL|All files (*.*)|*.*";
            if (mypath.ShowDialog() == true)
            {
                if (File.Exists(mypath.FileName))
                {
                    FileInfo f = new FileInfo(mypath.FileName);
                    f.MoveTo(System.IO.Path.ChangeExtension(f.FullName, ".txt"));
                    using (StreamReader sr = new StreamReader(f.FullName))
                    {
                        for (int i = 0; i < File.ReadAllLines(f.FullName).Length; i++)
                        {
                            byte[] data = new byte[File.ReadAllLines(f.FullName).Length];
                            //BitmapImage img = Convert.FromBase64String(sr.ReadLine());
                            data = Convert.FromBase64String(sr.ReadLine());
                            using (MemoryStream ms = new MemoryStream(data))
                            {
                                BitmapImage img = new BitmapImage();
                                img.BeginInit();
                                img.CacheOption = BitmapCacheOption.OnLoad;
                                img.StreamSource = ms;
                                img.EndInit();
                                Add_image(img);
                            }
                        }
                    }
                    f.MoveTo(System.IO.Path.ChangeExtension(f.FullName, ".gal"));
                }
            }
        }

        private void Export_image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SaveFileDialog mypath = new SaveFileDialog();
            mypath.Title = "Выберете каталог для сохранения";
            mypath.Filter = "Gallery Files(*.GAL)|*.GAL";
            if (mypath.ShowDialog() == true)
            {
                if (!File.Exists(mypath.FileName))
                {
                    File.Create(mypath.FileName).Close();
                    FileInfo f = new FileInfo(mypath.FileName);
                    f.MoveTo(System.IO.Path.ChangeExtension(f.FullName, ".txt"));
                    using (StreamWriter sw = new StreamWriter(f.FullName))
                    {
                        foreach (Image img in Gallery_wrap_panel.Children)
                        {
                            byte[] data;
                            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create((BitmapImage)img.Source));
                            using (MemoryStream ms = new MemoryStream())
                            {
                                encoder.Save(ms);
                                data = ms.ToArray();
                            }
                            sw.WriteLine(Convert.ToBase64String(data));
                        }
                    }
                    f.MoveTo(System.IO.Path.ChangeExtension(f.FullName, ".gal"));
                }
            }
        }

        private void Gallery_wrap_panel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Thickness t = Gallery_wrap_panel.Margin;
            if ((Math.Abs(t.Left) <= (Bottom_grid.Width - Window_grid.ActualWidth) / 2 && Math.Abs(t.Right) <= (Bottom_grid.Width - Window_grid.ActualWidth) / 2) || (direction / (e.Delta / Math.Abs(e.Delta)) < 0))
            {
                t.Left += e.Delta;
                t.Right -= e.Delta;
                Debug.WriteLine($"Кручу!");
            }
            Gallery_wrap_panel.Margin = t;
            direction = e.Delta / Math.Abs(e.Delta);
            //Debug.WriteLine($"Крутанулось: {Window_grid.ActualWidth}");
        }
    }
}
