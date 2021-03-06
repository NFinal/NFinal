﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NFinal;

namespace NFinalServerSample.Controllers
{
    public class IndexController:BaseController
    {
        [Code.UserCheck]
        public void Default()
        {
            this.ViewBag.Message = "Hello World!";
            this.ViewBag.Title = "Title";
            this.Render();
        }
        public void Ajax()
        {
            this.ViewBag.Message = "Hello Json!";
            this.ViewBag.id = 2;
            
            this.ViewBag.time = DateTime.Now;
            this.AjaxReturn();
        }
    }
}
