using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace TweetCrawler.Models
{
    public class User
    {
        public string name { get; set; }
        public string profile_image_url { get; set; }
        public string screen_name { get; set; }
    }
}
