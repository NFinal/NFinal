﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace $safeprojectname$
{
    class Program
    {
        static void Main(string[] args)
        {
            bool debug = true;
            string url = null;
            if (debug)
            {
                url = "http://localhost:8083";
            }
            else
            {
                url = "http://localhost:80";
            }
            using (Microsoft.Owin.Hosting.WebApp.Start<$safeprojectname$.Startup>(url))
            {
                Console.WriteLine("服务器已经启动");
                Console.ReadKey();
            }
        }
    }
}
