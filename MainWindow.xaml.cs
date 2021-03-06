﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json;
using TweetCrawler.Models;

namespace TweetCrawler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            lstTweets.ItemsSource = tweets;
            
            //ThreadPool.QueueUserWorkItem((a) => startStreaming());
            dt.Interval = TimeSpan.FromSeconds(1);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();
            tweets.Add(new Tweet() { user = new User() });

            var settings = TweetCrawler.Properties.Settings.Default;
            Filter.Text = settings.SearchTerm;
            Username.Text = settings.Username;

        }

        void dt_Tick(object sender, EventArgs e)
        {
            if (pendingTweets.Count > 0)
            {
                dt.Interval = TimeSpan.FromMilliseconds(Math.Max(2000 / pendingTweets.Count, 250));

                if (updating)
                    tweets.Insert(0, pendingTweets.Dequeue());

                if (pendingTweets.Count > 0)
                    tweetCount.Text = pendingTweets.Count + " pending tweets";
                else
                    tweetCount.Text = string.Empty;
                
                while (tweets.Count > 100)
                    tweets.RemoveAt(tweets.Count - 1);

            }
            else
            {
                tweetCount.Text = string.Empty;
            }
        }

        DispatcherTimer dt = new DispatcherTimer();
        ObservableCollection<Tweet> tweets = new ObservableCollection<Tweet>();
        Queue<Tweet> pendingTweets = new Queue<Tweet>();

        void startStreaming()
        {
            try
            {
                AsyncCallback cb = new AsyncCallback(this.streamCallBack);
                var req = HttpWebRequest.Create("https://stream.twitter.com/1/statuses/filter.json?track=" + Filter.Text);
                req.Credentials = new NetworkCredential(Username.Text, Password.Password);
                req.Method = "POST";
                req.BeginGetResponse(cb, req);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        void streamCallBack(IAsyncResult a)
        {
            try
            {
                HttpWebRequest reqResp = a.AsyncState as HttpWebRequest;
                var resp = reqResp.EndGetResponse(a);
                using (var str = resp.GetResponseStream())
                {
                    using (var stream = new StreamReader(str))
                    {
                        using (var debugStream = new StreamWriter(File.OpenWrite("debug.json")))
                        {
                            while (downloading)
                            {
                                var json = stream.ReadLine();
                                debugStream.WriteLine(json);
                                if (!string.IsNullOrEmpty(json))
                                {
                                    var tweet = JsonConvert.DeserializeObject<Tweet>(json);
                                    if (!string.IsNullOrEmpty(tweet.id))
                                    {
                                        lock (pendingTweets)
                                        {
                                            pendingTweets.Enqueue(tweet);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke((Action)delegate
                {
                    btnStartDownloading.Visibility = Visibility.Visible;
                    btnStopDownloading.Visibility = Visibility.Collapsed;
                });
                MessageBox.Show(ex.Message);
            }
        }

        bool downloading = true;
        bool updating = true;
        
        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            updating = !updating;
        }

        private void StopDownloading_Click(object sender, RoutedEventArgs e)
        {
            downloading = false;
            btnStartDownloading.Visibility = Visibility.Visible;
            btnStopDownloading.Visibility = Visibility.Collapsed;
        }

        private void StartDownloading_Click(object sender, RoutedEventArgs e)
        {
            downloading = true;
            btnStopDownloading.Visibility = Visibility.Visible;
            btnStartDownloading.Visibility = Visibility.Collapsed;
            startStreaming();

            var settings = TweetCrawler.Properties.Settings.Default;
            settings.SearchTerm = Filter.Text;
            settings.Username = Username.Text;
            settings.Save();
        }

        private void ClearBacklog_Click(object sender, EventArgs e)
        {
            pendingTweets = new Queue<Tweet>();
        }
    }
}
