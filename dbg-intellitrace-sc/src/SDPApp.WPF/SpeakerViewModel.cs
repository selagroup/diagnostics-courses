using SDPApp.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SDPApp.WPF
{
    public class SpeakerViewModel : INotifyPropertyChanged
    {
        public SpeakerViewModel(Speaker speaker)
        {
            Name = speaker.Name;
            LoadPhotoAsync(speaker.PhotoURL);
        }

        private async void LoadPhotoAsync(string photoURL)
        {
            HttpClient http = new HttpClient();
            Stream s = await http.GetStreamAsync(photoURL);
            BitmapImage image = new BitmapImage();
            image.CreateOptions = BitmapCreateOptions.None;
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.BeginInit();
            image.StreamSource = s;
            image.EndInit();
            Photo = image;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("Photo"));
        }

        public string Name { get; set; }
        public ImageSource Photo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public override string ToString()
        {
            return Name;
        }
    }
}
