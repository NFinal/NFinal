﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NFinal.View.Razor
{
    public class BasePage<T>
    {
        public NFinal.IO.IWriter writer { get; }
        public T Model { get; set; }
        public virtual void Execute()
        {
            throw new NotImplementedException();
        }
    }
}