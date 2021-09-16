
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Windows;

using YouTubeParser.Models;
using YouTubeParser.Utils;

namespace YouTubeParser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Channel> channels = new();
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void btnParse_Click(object sender, RoutedEventArgs e)
        {
            foreach (var url in tbLinks.Text.Split('\n'))
            {
                Uri uriResult;
                if (Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp
                    || uriResult.Scheme == Uri.UriSchemeHttps))
                    channels.Add(new Channel() { Link = url });

            }
            foreach (var channel in channels)
            {
                using (HttpClient client = new())
                {
                    var result = await client.GetAsync(channel.Link);
                    var body = await result.Content.ReadAsStringAsync();
                    var name = body.Split("\"name\": \"")[1];
                    channel.Name = name.Remove(name.IndexOf("\""));
                }
            }
        }
    }
}
