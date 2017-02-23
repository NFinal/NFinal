﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace System
{
    public struct  NameVarCollection : IEnumerable<KeyValuePair<string, StringContainer>>
    {
        private IDictionary<string, StringContainer> collection;


        public NameVarCollection(Dictionary<string, StringContainer> collection)
        {
            if (collection == null)
            {
                this.collection = new Dictionary<string, StringContainer>(StringComparer.Ordinal);
            }
            else
            {
                this.collection = collection;
            }
        }
        public StringContainer this[string key]
        {
            get
            {
                if (collection.ContainsKey(key))
                {
                    return collection[key];
                }
                else
                {
                    return new StringContainer();
                }
            }
            set
            {
                if (value == null)
                {
                    if (collection.ContainsKey(key))
                    {
                        collection.Remove(key);
                    }
                }
                else
                {
                    if (collection.ContainsKey(key))
                    {
                        collection[key] = value;
                    }
                    else
                    {
                        collection.Add(key, value);
                    }
                }
            }
        }
        public void Add(string key, string value)
        {
            collection[key] = value;
        }
        public int Count
        {
            get
            {
                return collection.Count;
            }
        }

        public override string ToString()
        {
            StringWriter sw = new StringWriter();
            bool firstChild = true;
            foreach (var item in collection)
            {
                if (firstChild)
                {
                    firstChild = false;
                }
                else
                {
                    sw.Write("&");
                }
                sw.Write(item.Key);
                sw.Write("=");
                sw.Write(NFinal.Utility.UrlEncode(item.Value));
            }
            return sw.ToString();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, StringContainer>> IEnumerable<KeyValuePair<string, StringContainer>>.GetEnumerator()
        {
            return collection.GetEnumerator();
        }
    }
}