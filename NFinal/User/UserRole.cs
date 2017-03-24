﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NFinal.User
{
    public abstract class BaseRole
    {
        /// <summary>
        /// 高级管理员
        /// </summary>
        public const int Administrator = 1;
        /// <summary>
        /// 普通管理员
        /// </summary>
        public const int Manager = 1 << 2;
        /// <summary>
        /// 一般用户
        /// </summary>
        public const int User = 1 << 3;
        /// <summary>
        /// 商家
        /// </summary>
        public const int Business = 1 << 4;
        /// <summary>
        /// 客户
        /// </summary>
        public const int Customer = 1 << 5;
        /// <summary>
        /// 会计
        /// </summary>
        public const int Accountant = 1 << 6;
        /// <summary>
        /// 销售人员
        /// </summary>
        public const int Salesperson = 1 << 7;
        /// <summary>
        /// 来宾
        /// </summary>
        public const int Guest = 1 << 8;
    }
    public class UserRole : BaseRole
    {
        /// <summary>
        /// 所有人
        /// </summary>
        public const int EveryOne = 1 << 9;
    }
}
