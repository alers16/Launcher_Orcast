using Avalonia.Controls;
using Avalonia;
using Avalonia.Markup.Xaml;
using System;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using Avalonia.Platform;
using Avalonia.Media;
using Avalonia.Styling;
using System.IO;
using System.Linq;
using System.Net;
using Avalonia.Threading;
using System.IO.Compression;
using System.Diagnostics;
using Microsoft.Win32;
using System.Net.Http;
using System.Threading.Tasks;

namespace Launcher.Views
{
    public partial class MainWindow : Window
    {
        private enum EstadoJuego { Actualizar, Descargar, Jugar }
        private Grid? _GridPanel = null;
        private string path;
        EstadoJuego estadoJuego;
        int porcentaje = 0;
        bool isDownloading = false;
        string? version = null;

        int Porcentaje
        {
            get { return porcentaje; }
            set
            {
                porcentaje = value;
                if (porcentaje == 100)
                {
                    isDownloading = false;
                    Porcentaje = 0;
                    ZipFile.ExtractToDirectory("orcast-game.zip", path + "/Orcast");
                    File.Delete("orcast-game.zip");
                    StreamWriter sw = new StreamWriter(path + "/Orcast/Orcast/Orcast_Data/version.txt");
                    sw.Write(version);
                    sw.Close();
                    CreateOrcastPanel();
                    
                }
                else if (isDownloading)
                {

                    _GridPanel!.Children.OfType<StackPanel>().FirstOrDefault(child => child.Name == "Porcentaje")!.
                    Children.OfType<TextBlock>().FirstOrDefault(child => child.Name == "TextoPorcentaje")!.Text = "Descargando... Porcentaje descargado " + porcentaje + "%";
                   
                }



            }
        }
        public MainWindow()
        {
             InitializeComponent();
        }

        private void InitializeComponent()
        {

            var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Launcher");
            path = key.GetValue("Path") as string;

            if (string.IsNullOrEmpty(path))
            {
                estadoJuego = EstadoJuego.Descargar;
            }


            AvaloniaXamlLoader.Load(this);
            _GridPanel = this.FindControl<Grid>("Content");
            
            CreateOrcastPanel();

        }



        private void OrcastButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            CreateOrcastPanel();
        }

        private void NovedadesButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            _GridPanel!.Children.Clear();
            _GridPanel!.Children.Add(new TextBlock
            {
                Text = "Proximamente",
                FontFamily = (FontFamily)Application.Current.FindResource("GrapeSoda"),
                Foreground = Brushes.White,
                FontSize = 130,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = new Avalonia.Thickness(150, 200, 0, 0),
                Effect = new DropShadowDirectionEffect { Direction = 100, ShadowDepth = 7 }
            });
        }

        public async Task GetVersion()
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Token", "nomeseasgitano.es");
            HttpResponseMessage response = await client.GetAsync("https://yellowbeavers.com/api/version");
            HttpContent content = response.Content;
            string data = await content.ReadAsStringAsync();
            version = data;
        }

        private async void CreateOrcastPanel()
        {
            if(version == null) await GetVersion();
            if(!string.IsNullOrEmpty(path) && version != File.ReadAllText(path + "/Orcast/Orcast/Orcast_Data/version.txt"))
            {
                estadoJuego = EstadoJuego.Actualizar;
            }
            else if(!string.IsNullOrEmpty(path))
            {
                estadoJuego = EstadoJuego.Jugar;
            }
            ;
            var button = new Button
            {
                Width = 300,
                Height = 50,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                Background = Brushes.AntiqueWhite,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Black,
                BorderThickness = new Avalonia.Thickness(1),
                CornerRadius = new Avalonia.CornerRadius(5),
                Cursor = new Avalonia.Input.Cursor(Avalonia.Input.StandardCursorType.Hand),

                Content = new TextBlock
                {
                    Text = estadoJuego == EstadoJuego.Actualizar ? "Actualizar" : estadoJuego == EstadoJuego.Descargar ? "Descargar" : "Iniciar",
                    FontSize = 30,
                    FontFamily = (FontFamily)Application.Current.FindResource("GrapeSoda"),
                }
            };

            button.Click += estadoJuego == EstadoJuego.Actualizar ? ActualizarButton_Click : estadoJuego == EstadoJuego.Descargar ? DescargarButton_Click : JugarButton_Click;

            _GridPanel!.Children.Clear();
            _GridPanel!.Children.Add(new Image
            {
                Source = new Bitmap(AssetLoader.Open(new Uri("avares://Launcher/Assets/background_main.jpeg"))),
                Width = 1100,
                Effect = new BlurEffect { Radius = 1 }
            });
            _GridPanel!.Children.Add(new TextBlock
            {
                Text = "Orcast",
                FontFamily = (FontFamily)Application.Current.FindResource("GrapeSoda"),
                Foreground = Brushes.Red,
                FontSize = 130,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                Margin = new Avalonia.Thickness(-60, 150, 0, 0),
                Effect = new DropShadowDirectionEffect { Direction = 100, ShadowDepth = 7 }
            });

            _GridPanel!.Children.Add(new StackPanel
            {
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                Margin = new Avalonia.Thickness(370, 0, 0, 80),
                Orientation = Avalonia.Layout.Orientation.Horizontal,
                Children =
                {
                    button
                }
            });

            if (!string.IsNullOrEmpty(path))
            {
                var des = new MenuItem
                {
                    Header = "Desinstalar",
                };

                var ubi = new MenuItem
                {
                    Header = "Ubicación del archivo",
                };

                des.Click += DesinstallButton_Click;
                ubi.Click += OpenFolderButton_Click;

                _GridPanel!.Children.Add(new Button
                {
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
                    Background = Brushes.Transparent,
                    Width = 50,
                    Height = 50,
                    Margin = new Avalonia.Thickness(850, 0, 0, 0),
                    BorderThickness = new Avalonia.Thickness(0),
                    Content = new Image
                    {
                        Source = new Bitmap(AssetLoader.Open(new Uri("avares://Launcher/Assets/9035884_settings_sharp_icon.png"))),
                        RenderTransform = new ScaleTransform(1, 1)
                    },
                    Flyout = new MenuFlyout
                    {
                        Placement = PlacementMode.LeftEdgeAlignedBottom,
                        Items =
                        {
                            des,
                            ubi
                        }
                    }

                });
            }


        }

        private void DesinstallButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var di = new DirectoryInfo(path + "/Orcast");
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
            estadoJuego = EstadoJuego.Descargar;
            Directory.Delete(path + "/Orcast");
            var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Launcher");
            key.DeleteValue("Path");
            path = null;
            CreateOrcastPanel();
        }

        private void OpenFolderButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start("explorer.exe", path + "\\Orcast");
        }

        private async void DescargarButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (!isDownloading)
            {
                var dialog = new OpenFolderDialog();
                var result = await dialog.ShowAsync(this);
                if (result != null)
                {
                    path = result;
                    var key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Launcher");
                    key.SetValue("Path", path);
                }
                Dispatcher.UIThread.Post(DownloadFile, DispatcherPriority.Send);

            }

        }

        private void ActualizarButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (estadoJuego == EstadoJuego.Actualizar)
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                WebClient webClient = new WebClient();
                webClient.DownloadFile("https://yellowbeavers.com/download/?type=zip", "orcast-game.zip");

            }
        }

        private void JugarButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Process.Start(path + "/Orcast/Orcast/Orcast.exe");
        }

        public void Updater()
        {
            try
            {
                _GridPanel!.Children.Add(new StackPanel
                {
                    Name = "Porcentaje",
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom,
                    Margin = new Avalonia.Thickness(370, 0, 0, 60),
                    Orientation = Avalonia.Layout.Orientation.Horizontal,
                    Children =
                    {
                        new TextBlock
                        {
                            Name = "TextoPorcentaje",
                            Text = "Descargando... Porcentaje descargado " + porcentaje + "%",
                        }
                    }
                });
            }
            catch
            {

            }
        }

        public void DownloadFile()
        {

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            isDownloading = true;
            WebClient webClient = new WebClient();
            webClient.DownloadProgressChanged += (s, e) =>
            {
                Porcentaje = e.ProgressPercentage;
            };
            webClient.DownloadFileAsync(new Uri("https://yellowbeavers.com/download/?type=zip"), "orcast-game.zip");
            Updater();
        }
    }
}