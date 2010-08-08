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
    public class Tweet
    {
        public string id { get; set; }
        public string created_at { get; set; }
        public string text { get; set; }
        public User user { get; set; }
        public bool truncated { get; set; }
        public Tweet retweeted_status { get; set; }

        public string TweetText
        {
            get
            {
                if (truncated && retweeted_status != null)
                {
                    return retweeted_status.text;
                }
                else
                {
                    return text;
                }
            }
        }

        public string TweetUrl
        {
            get
            {
                return "http://twitter.com/" + user.screen_name + "/status/" + id;
            }
        }

        public override string ToString()
        {
            return user.name + ": " + text;
        }

        public string Username { get { return user.name; } }
        public string Userpic { get { return user.profile_image_url; } }
    }
}
