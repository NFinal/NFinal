﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace NFinal.Validation.Attribute
{
    public class UrlAttribute:ValidateAttribte
    {
        public override bool Validate {
            get {
                Regex reg = new Regex(Pattern.url);
                return reg.IsMatch(value.value);
            }
        }
        public override string ErrorMessage
        {
            get {
                return $"{name}必须为Url地址";
            }
        }
    }
}
