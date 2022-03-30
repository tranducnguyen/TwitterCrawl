using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace Pwpf
{
    class Controller
    {
        bool is_createClient = false;
        RestClient client;
        void createClient()
        {
            if (is_createClient)
            {
                return;
            }

            client = new RestClient();
            client.FollowRedirects = true;
            client.CookieContainer = new CookieContainer();
            is_createClient = true;
        }
        public void login(int vitri, Info account)
        {
            
        }
       
    }

}
