using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Snapshot
{

    public partial class MainWindow : Window
    {

        private string downloadLocation = "/";
        private bool IsPlaying = false;
        private bool IsDragging = false;

        public MainWindow()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Tick;
            timer.Start();
        }

        private void Tick(object sender, EventArgs e)
        {
            if ((mediaPlayer.Source != null) && (mediaPlayer.NaturalDuration.HasTimeSpan) && (!IsDragging))
            {
                slider.Minimum = 0;
                slider.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
                slider.Value = mediaPlayer.Position.TotalSeconds;
            }
        }

        private void Upload_Executed(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Media files (*.mp3;*.mp4;*.mov)|*.mp3;*.mp4;*.mov|All files (*.*)|*.*";
            if (dialog.ShowDialog() == true)
            {
                mediaPlayer.Source = new Uri(dialog.FileName);
                mediaPlayer.Position = TimeSpan.Zero;
                mediaPlayer.Stop(); // Makes the video appear on screen
            }
        }

        private void Upload_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void Download_Executed(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.InitialDirectory = "/";
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                textDownloadFolder.Text = dialog.FileName;
                downloadLocation = dialog.FileName;
            }
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = (mediaPlayer != null) && (mediaPlayer.Source != null);
        }

        private void Play_Executed(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Play();
            IsPlaying = true;
        }

        private void Pause_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsPlaying;
        }

        private void Pause_Executed(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Pause();
        }

        private void Stop_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = IsPlaying;
        }

        private void Stop_Executed(object sender, RoutedEventArgs e)
        {
            mediaPlayer.Stop();
            IsPlaying = false;
        }

        private void Grid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            mediaPlayer.Volume += (e.Delta > 0) ? 0.1 : -0.1;
        }

        private void Slider_ValueChanged(object sender, RoutedEventArgs e)
        {
            labelProgress.Text = TimeSpan.FromSeconds(slider.Value).ToString(@"hh\:mm\:ss");
        }

        private void Slider_DragStarted(object sender, RoutedEventArgs e)
        {
            IsDragging = true;
        }

        private void Slider_DragCompleted(object sender, RoutedEventArgs e)
        {
            IsDragging = false;
            mediaPlayer.Position = TimeSpan.FromSeconds(slider.Value);
        }

        private void Capture_Executed(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer == null || mediaPlayer.Source == null) return;
            SaveSnapshot();
        }

        private void Help_Executed(object sender, RoutedEventArgs e)
        {
            string caption = "Snapshot v1.0 by Daniel Rickman";
            string text = "This program allows you to upload video files (.mp3/.mp4/.mov) and capture images of the footage.\n\nControls:\n SPACE - Capture an image \n Scroll Wheel - Volume Control \n , - Volume Down \n . - Volume Up \n M - Mute \n\nIcon credit - https://www.flaticon.com/packs/video-production-51 by Hilmy Abiyyu A";
            MessageBoxButton button = MessageBoxButton.OK;
            MessageBoxImage icon = MessageBoxImage.Asterisk;
            MessageBox.Show(text, caption, button, icon, MessageBoxResult.None);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {

            switch(e.Key)
            {
                case Key.Space:
                    SaveSnapshot();
                    break;
                case Key.M:
                    mediaPlayer.Volume = 0.0f;
                    break;
                case Key.OemComma:
                    mediaPlayer.Volume -= 0.1;
                    break;
                case Key.OemPeriod:
                    mediaPlayer.Volume += 0.1f;
                    break;
            }

            e.Handled = true;
        }

        private void SaveSnapshot()
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)mediaPlayer.RenderSize.Width, (int)mediaPlayer.RenderSize.Height, 96, 96, PixelFormats.Default);
            VisualBrush sourceBrush = new VisualBrush(mediaPlayer);
            DrawingVisual drawingVisual = new DrawingVisual();
            DrawingContext drawingContext = drawingVisual.RenderOpen();
            using (drawingContext)
            {
                drawingContext.DrawRectangle(sourceBrush, null, new Rect(new Point(0, 0), new Point(mediaPlayer.RenderSize.Width, mediaPlayer.RenderSize.Height)));
            }
            renderTargetBitmap.Render(drawingVisual);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            string fileName = Guid.NewGuid().ToString() + ".png";
            FileStream fs = new FileStream(downloadLocation + "/" + fileName, FileMode.Create);
            encoder.Save(fs);
            fs.Close();
        }
    }
}
