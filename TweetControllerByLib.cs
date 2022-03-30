using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pwpf
{
    class TweetControllerByLib
    {
        public void Test()
        {
            string BearToken = "AAAAAAAAAAAAAAAAAAAAANRILgAAAAAAnNwIzUejRCOuH5E6I8xnZz4puTs%3D1Zv7ttfk8LF81IUq16cHjhLTvJu4FA33AGWWjCpTnA";
            var client = new TwitterSharp.Client.TwitterClient(BearToken);

            
        }
    }
}
