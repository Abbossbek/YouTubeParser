
using Microsoft.Win32;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

using YouTubeParser.Models;

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
            channels.Add(await GetChannelFromLink(tbLink.Text));
        }
        private async void btnAddMany_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new();
            dialog.Filter = "Text file (*.txt)| *.txt";

            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                string[] links = File.ReadAllLines(dialog.FileName);
                foreach (var link in links)
                {
                    var channel = await GetChannelFromLink(link);
                    if(channel is not null)
                    channels.Add(channel);
                }
            }
        }
        private async Task<Channel> GetChannelFromLink(string link)
        {
            link = link.Trim();
            Channel channel;
            Uri uriResult;
            if (Uri.TryCreate(link, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp
                || uriResult.Scheme == Uri.UriSchemeHttps))
                channel = new Channel() { Link = link };
            else
                return null;
            tbLink.Text = "";
            using (HttpClient client = new())
            {
                var result = await client.GetAsync(channel.Link + "?hl=en");
                var body = await result.Content.ReadAsStringAsync();
                try
                {
                    var name = body.Split("\"name\": \"")[1];
                    channel.Name = name.Remove(name.IndexOf("\""));
                }
                catch { }
                try
                {
                    var subs = body.Split("subscriberCountText\":{\"accessibility\":{\"accessibilityData\":{\"label\":\"").Last();
                    subs = subs.Substring(subs.IndexOf("\"simpleText\":\"") + 14);
                    subs = subs.Remove(subs.IndexOf("subscribers")).ToUpper().Trim();
                    channel.SubcribersCount = Convert.ToInt32(subs.Contains('K') ? subs.Replace("K", subs.Contains('.') ? Convert.ToString(Math.Pow(10, 5 - (subs.Length - subs.IndexOf('.')))).Substring(1) : "000").Replace(".", "") : subs.Contains('M') ? subs.Replace("M", subs.Contains('.') ? Convert.ToString(Math.Pow(10, 8 - (subs.Length - subs.IndexOf('.')))).Substring(1) : "000000").Replace(".", "") : subs);
                }
                catch { }
                result = await client.GetAsync(channel.Link + "/about?hl=en");
                body = await result.Content.ReadAsStringAsync();
                try
                {
                    var country = body.Split("country\":{\"simpleText\":\"").Last();
                    channel.Country = country.Remove(country.IndexOf('"'));
                }
                catch { }
                try
                {
                    var description = body.Split("property=\"og:description\" content=\"").Last();
                    channel.Description = HttpUtility.HtmlDecode(description.Remove(description.IndexOf("\"><meta")));
                }
                catch { }
                try
                {
                    var joined = body.Split("\"Joined \"},{\"text\":\"").Last();
                    channel.RegistrationDate = joined.Remove(joined.IndexOf('"'));
                }
                catch { }
                try
                {
                    var viewCount = body.Split("\"viewCountText\":{\"simpleText\":\"").Last();
                    channel.ViewsCount = Convert.ToInt32(viewCount.Remove(viewCount.IndexOf(" views")).Replace(",", ""));
                }
                catch { }
            }
            channel.Number = channels.Count+1;
            return channel;
        }
        public static DataTable ToDataTable<T>(List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            //Get all the properties
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var prop in properties)
            {
                try
                {
                    //Defining type of data column gives proper data table 
                    var type = (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>) ? Nullable.GetUnderlyingType(prop.PropertyType) : prop.PropertyType);
                    //Setting column names as Property names
                    dataTable.Columns.Add(prop.Name, type);
                }
                catch { }
            }
            foreach (var item in items)
            {
                try
                {
                    var values = new object[properties.Length];
                    for (var i = 0; i < properties.Length; i++)
                    {
                        //inserting property values to data table rows
                        values[i] = properties[i].GetValue(item, null);
                    }
                    dataTable.Rows.Add(values);
                }
                catch { }
            }
            //put a breakpoint here and check data table
            return dataTable;
        }

        private void btnExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new();
            dialog.Filter = "Excel file (*.xlsx)| *.xlsx";

            bool? result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                if (File.Exists(dialog.FileName))
                    File.Delete(dialog.FileName);

                using (var connection = new OleDbConnection())
                {
                    connection.ConnectionString = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={dialog.FileName};" +
                                                  "Extended Properties='Excel 12.0 Xml;HDR=YES;'";
                    connection.Open();
                    using (var command = new OleDbCommand())
                    {
                        command.Connection = connection;
                        var dataTable = ToDataTable(channels.ToList());
                        var columnNames = (from DataColumn dataColumn in dataTable.Columns select dataColumn.ColumnName).ToList();
                        var tableName = !string.IsNullOrWhiteSpace(dataTable.TableName) ? dataTable.TableName : Guid.NewGuid().ToString();
                        command.CommandText = $"CREATE TABLE [{tableName}] ({string.Join(",", columnNames.Select(c => $"[{c}] MEMO").ToArray())});";
                        command.ExecuteNonQuery();
                        foreach (DataRow row in dataTable.Rows)
                        {
                            try
                            {
                                var rowValues = (from DataColumn column in dataTable.Columns select (row[column] != null && row[column] != DBNull.Value) ? row[column].ToString() : string.Empty).ToList();
                                command.CommandText = $"INSERT INTO [{tableName}]({string.Join(",", columnNames.Select(c => $"[{c}]"))}) VALUES ({string.Join(",", rowValues.Select(r => $"'{r}'").ToArray())});";
                                command.ExecuteNonQuery();
                            }
                            catch { }
                        }
                    }

                    connection.Close();
                }
            }
        }

    }
}
