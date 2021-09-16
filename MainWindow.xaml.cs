
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private ObservableCollection<Channel> channels = new();
        public MainWindow()
        {
            InitializeComponent();
            dgChannels.ItemsSource = channels;
        }

        private async void btnParse_Click(object sender, RoutedEventArgs e)
        {
            Channel channel;
            Uri uriResult;
            if (Uri.TryCreate(tbLink.Text, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp
                || uriResult.Scheme == Uri.UriSchemeHttps))
                channel = new Channel() { Link = tbLink.Text };
            else
                return;

            using (HttpClient client = new())
            {
                var result = await client.GetAsync(channel.Link);
                var body = await result.Content.ReadAsStringAsync();
                var name = body.Split("\"name\": \"")[1];
                channel.Name = name.Remove(name.IndexOf("\""));
                var subs = body.Split("subscriberCountText\":{\"accessibility\":{\"accessibilityData\":{\"label\":\"").Last();
                subs = subs.Substring(subs.IndexOf("\"simpleText\":\"") + 14);
                subs = subs.Remove(subs.IndexOf("subscribers")).ToUpper().Trim();
                channel.SubcribersCount = Convert.ToInt32(subs.Contains('K') ? subs.Replace("K", subs.Contains('.') ? Convert.ToString(Math.Pow(10, 5 - (subs.Length - subs.IndexOf('.')))).Substring(1) : "000").Replace(".", "") : subs.Contains('M') ? subs.Replace("M", subs.Contains('.') ? Convert.ToString(Math.Pow(10, 8 - (subs.Length - subs.IndexOf('.')))).Substring(1) : "000000").Replace(".", "") : subs);

                result = await client.GetAsync(channel.Link + "/about");
                body = await result.Content.ReadAsStringAsync();
                var country = body.Split("country\":{\"simpleText\":\"").Last();
                channel.Country = country.Remove(country.IndexOf('"'));
                var description = body.Split("property=\"og:description\" content=\"").Last();
                channel.Description = description.Remove(description.IndexOf("\"><meta"));
                var joined = body.Split("\"Joined \"},{\"text\":\"").Last();
                channel.RegistrationDate = joined.Remove(joined.IndexOf('"'));
                var viewCount = body.Split("\"viewCountText\":{\"simpleText\":\"").Last();
                channel.ViewsCount = Convert.ToInt32(viewCount.Remove(viewCount.IndexOf(" views")).Replace(",", ""));
            }
            channels.Add(channel);
        }
    }
}
