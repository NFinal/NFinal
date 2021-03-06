﻿//======================================================================
//
//        Copyright : Zhengzhou Strawberry Computer Technology Co.,LTD.
//        All rights reserved
//        
//        Application:NFinal MVC framework
//        Filename : Int32Extension.cs
//        Description :Int32扩展函数
//
//        created by Lucas at  2015-5-31
//     
//        WebSite:http://www.nfinal.com
//
//======================================================================
using System;
using System.Collections.Generic;
using System.Text;

namespace NFinal
{
    /// <summary>
    /// Int32扩展函数
    /// </summary>
    public static class Int32Extension
    {
        private static readonly int[] sigment = new int[32] {
            1,2,4,8,16,32,64,128,256,512,
            1024,2048,4096,8192,16384,32768,65536,131072,262144,524288,
            1048576,2097152,4194304,8388608,16777216,33554432,67108864,134217728,268435456,536870912,
            1073741824,-2147483648
        };
        /// <summary>
        /// 是否具有某个值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPosition"></param>
        /// <returns></returns>
        public static bool HasValue(this int value, int bitPosition)
        {
            return (value & sigment[bitPosition]) != 0;
        }
        /// <summary>
        /// 是否具有某个值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPositions"></param>
        /// <returns></returns>
        public static bool HasValue(this int value, params int[] bitPositions)
        {
            int bitNumber = 0;
            foreach (int bp in bitPositions)
            {
                bitNumber |= sigment[bp];
            }
            return (value & bitNumber) != 0;
        }
        /// <summary>
        /// 是否具有所有的值
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bitPositions"></param>
        /// <returns></returns>
        public static bool HasAllValue(this int value, params int[] bitPositions)
        {
            int bitNumber = 0;
            foreach (int bp in bitPositions)
            {
                bitNumber |= sigment[bp];
            }
            return (value & bitNumber) == bitNumber;
        }
    }
}
