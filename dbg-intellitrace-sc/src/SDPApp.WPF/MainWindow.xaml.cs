using SDPApp.Shared;
using System;
using System.Collections.Generic;
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

namespace SDPApp.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool _loadingDone;

        private async void LoadSpeakers(object sender, RoutedEventArgs e)
        {
            _loadingDone = false;
            var speakers = await ConferenceService.GetSpeakers();
            var speakerViewModels = from speaker in speakers
                                    select new SpeakerViewModel(speaker);
            lstSpeakers.ItemsSource = speakerViewModels.ToArray();
            _loadingDone = true;
        }

        private void lstSpeakers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_loadingDone)
                MessageBox.Show("Invalid Operation Attempted");
        }
    }
}
