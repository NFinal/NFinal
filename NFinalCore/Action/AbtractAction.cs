﻿//======================================================================
//
//        Copyright : Zhengzhou Strawberry Computer Technology Co.,LTD.
//        All rights reserved
//        
//        Application:NFinal MVC framework
//        Filename :MagicViewBag.cs
//        Description :控制器父类
//
//        created by Lucas at  2015-6-30`
//     
//        WebSite:http://www.nfinal.com
//
//======================================================================
using System;
using System.Collections.Generic;
using System.IO;
using NFinal.Owin;

namespace NFinal
{
    /// <summary>
    /// 用户实体类
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    public interface IUser<TUser>
    {
        TUser user { get; set; }
    }
    /// <summary>
    /// NFinal的控制器基类，核心类，类似于asp.net中的page类
    /// </summary>
    /// <typeparam name="TContext">上下文IOwinContext,Enviroment,Context</typeparam>
    /// <typeparam name="TRequest">请求信息</typeparam>
    /// <typeparam name="TUser">用户相关数据类型</typeparam>
    /// <typeparam name="TMasterPage">母页模板</typeparam>
    /// <typeparam name="TViewBag">ViewBag视图,不需要实例化</typeparam>
    public abstract class AbstractAction<TContext,TRequest,TUser,TMasterPage> :NFinal.IO.Writer, IAction<TContext, TRequest> ,IUser<TUser> where TMasterPage : NFinal.MasterPageModel
    {
        /// <summary>
        /// 常用系统变量
        /// </summary>
        [ViewBagMember]
        [Newtonsoft.Json.JsonIgnore]
        public static NFinal.Collections.FastDictionary<StringContainer> systemConfig = null;
        /// <summary>
        /// 请求参数信息
        /// </summary>
        public NameValueCollection parameters;
        public Owin.HttpMultipart.HttpFile[] files=null;
        public string contentType = "text/html; charset=utf-8";
        protected Stream writeStream;
        public virtual TMasterPage MasterPage { get; set; }
        public ServerType _serverType = ServerType.UnKnown;
        public ICookie Cookie;
        public ISession Session;
        private TUser _user;
        private string _methodName;
        public string methodName { get { return _methodName; } }
        public TUser user
        {
            get
            {
                return Session.Get<TUser>("user");
            }

            set
            {
                _user = value;
                Session.Set<TUser>("user",_user);
            }
        }
        public dynamic ViewBag=null;
        public TContext context { get; set; }

        public TRequest request { get; set; }

        public Response response { get; set; }
        public Stream outputStream { get; set; }

        public CompressMode compressMode { get; set; }
        #region 子类必须实现的方法
        public virtual void BaseInitialization(TContext context, string methodName)
        {
            this.context = context;
            this._methodName = methodName;
            SetResponse(CompressMode.Deflate);
        }

        public virtual void Initialization(TContext context, string methodName, Stream outputStream, TRequest request, CompressMode compressMode)
        {
            this.context = context;
            this._methodName = methodName;
            this.outputStream = outputStream;
            this.request = request;
            SetResponse(compressMode);
        }
        /// <summary>
        /// 设置压缩方式
        /// </summary>
        /// <param name="compressMode"></param>
        public void SetResponse(CompressMode compressMode)
        {
            this.response = new Owin.Response();
            this.response.headers = new Dictionary<string, string[]>(StringComparer.Ordinal);
            this.response.statusCode = 200;
            this.response.stream = new MemoryStream();
            this.compressMode = compressMode;
            if (compressMode == CompressMode.None)
            {
                this.writeStream = this.response.stream;
            }
            else
            {
                if (compressMode == CompressMode.GZip)
                {
                    this.writeStream = new System.IO.Compression.GZipStream(this.response.stream, System.IO.Compression.CompressionMode.Compress, true);
                    this.response.headers.Add(NFinal.Constant.HeaderContentEncoding, NFinal.Constant.HeaderContentEncodingGzip);
                }
                else if (compressMode == CompressMode.Deflate)
                {
                    this.writeStream = new System.IO.Compression.DeflateStream(this.response.stream, System.IO.Compression.CompressionMode.Compress, true);
                    this.response.headers.Add(NFinal.Constant.HeaderContentEncoding, NFinal.Constant.HeaderContentEncodingDeflate);
                }
            }
        }
        public abstract string GetRequestPath();

        public abstract string GetRemoteIpAddress();

        public abstract string GetRequestHeader(string key);

        public abstract void SetResponseHeader(string key, string value);

        public abstract void SetResponseHeader(string key, string[] value);

        public abstract void SetResponseStatusCode(int statusCode);

        public abstract bool Before();

        public abstract void After();

        public abstract void Close();

        //public abstract void Write(byte[] buffer, int offset, int count);

        //public abstract void Write(string value);

        public abstract void Dispose();
        #endregion
        #region 扩展方法
        /// <summary>
        /// 输出字节流，用于输出二进制流，如图象，文件等。
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            this.writeStream.Write(buffer, 0, count);
        }
        /// <summary>
        /// 输出文本内容
        /// </summary>
        /// <param name="value">文本</param>
        public override void Write(string value)
        {
            if (value != null && value.Length != 0)
            {
                byte[] buffer = NFinal.Constant.encoding.GetBytes(value);
                this.writeStream.Write(buffer, 0, buffer.Length);
            }
        }
        /// <summary>
        /// 页面重定向
        /// </summary>
        /// <param name="url"></param>
        public void Redirect(string url)
        {
            this.SetResponseStatusCode(302);
            this.SetResponseHeader(NFinal.Constant.HeaderContentType, NFinal.Constant.ResponseContentType_Text_html);
            this.SetResponseHeader(NFinal.Constant.HeaderLocation, url);
        }
        public void AjaxReturn()
        {
            this.contentType = "application/json; charset=utf-8";
            this.Write(Newtonsoft.Json.JsonConvert.SerializeObject(this.ViewBag, new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter()));
        }
        /// <summary>
        /// 返回json{code:1,msg:"",result:[json字符串]}
        /// </summary>
        /// <param name="json">json字符串</param>
        public void AjaxReturn(string json)
        {
            this.SetResponseHeader(NFinal.Constant.HeaderContentType, NFinal.Constant.ResponseContentType_Application_json);
            this.Write(json);
        }
        /// <summary>
        /// 返回true或false
        /// </summary>
        /// <param name="obj">bool变量</param>
        public  void AjaxReturn(bool obj)
        {
            this.SetResponseHeader(NFinal.Constant.HeaderContentType, NFinal.Constant.ResponseContentType_Application_json);
            this.Write(obj ? NFinal.Constant.trueString : NFinal.Constant.falseString);
        }
        public TModel GetModel<TModel>()
        {
            return NFinal.Model.ModelHelper.GetModel<TModel>(this.parameters);
        }
        /// <summary>
        /// 返回数据库实体类对应的json对象{code:1,msg:"",result:[json对象]}
        /// </summary>
        /// <param name="str">数据库实体类</param>
        public string GetModelJson<TModel>(TModel model)
        {
            if (model == null)
            {
                return Constant.nullString;
            }
            else
            {
                NFinal.IO.StringWriter sw = new IO.StringWriter();
                NFinal.Json.JsonHelper.GetJson(model, sw, Json.DateTimeFormat.LocalTimeNumber);
                return sw.ToString();
            }
        }
        /// <summary>
        /// 返回数据库实体类对应的json对象{code:[code状态码],msg:[msg消息],result:[json对象]}
        /// </summary>
        /// <param name="str">数据库实体类</param>
        /// <param name="code">状态码</param>
        /// <param name="msg">消息</param>
        public string GetJson<T>(T t)
        {
            if (t == null)
            {
                return Constant.nullString;
            }
            else
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(t);
            }
        }
        /// <summary>
        /// 返回json字符串{code:[code状态码],msg:[msg消息],result:[json字符串]}
        /// </summary>
        /// <param name="json">json字符串</param>
        /// <param name="code">状态码</param>
        /// <param name="msg">消息</param>
        public void AjaxReturn(string json, int code, string msg)
        {
            this.SetResponseHeader(Constant.HeaderContentType, NFinal.Constant.ResponseContentType_Application_json);
            this.Write(Constant.AjaxReturnJson_Code);
            this.Write(code);
            this.Write(Constant.AjaxReturnJson_Msg);
            this.Write(msg);
            this.Write(Constant.AjaxReturnJson_Json);
            this.Write(json);
            this.Write(Constant.AjaxReturnJson_End);
        }
        public abstract string GetSubDomain(TContext context);
        /// <summary>
        /// 是否是手机端
        /// </summary>
        /// <param name="userAgent"></param>
        /// <returns></returns>
        public bool IsMobile(string userAgent)
        {
            string agent = userAgent;
            bool isMobile = false;
            //排除 苹果桌面系统  
            if (!userAgent.Contains(NFinal.Constant.UserAgnet_Windows_NT) && !userAgent.Contains(NFinal.Constant.UserAgent_Macintosh))
            {
                for (int i = 0; i < NFinal.Constant.UserAgent_MobileKeyWords.Length; i++)
                {
                    if (agent.Contains(NFinal.Constant.UserAgent_MobileKeyWords[i]))
                    {
                        isMobile = true;
                        break;
                    }
                }
            }
            return isMobile;
        }
        /// <summary>
        /// 返回服务器路径
        /// </summary>
        /// <param name="path">相对路径</param>
        /// <returns></returns>
        public static string MapPath(string path)
        {
            return NFinal.Utility.MapPath(path);
        }
        /// <summary>
        /// 获取Url
        /// </summary>
        /// <param name="controllerType">控制器名称</param>
        /// <param name="methodName">方法名</param>
        /// <param name="urlParameters">Url参数</param>
        /// <returns></returns>
        public static string Url<TController>(string methodName, params StringContainer[] urlParameters)
        {
            return NFinal.Middleware.ActionUrlHelper.Format(Middleware.ActionUrlHelper.formatControllerDictionary[typeof(TController)][methodName].formatUrl, urlParameters);
        }
        /// <summary>
        /// 获取Url
        /// </summary>
        /// <param name="methodName">方法名</param>
        /// <param name="urlParameters">Url参数</param>
        /// <returns></returns>
        public string Url(string methodName, params StringContainer[] urlParameters)
        {
            return NFinal.Middleware.ActionUrlHelper.Format(Middleware.ActionUrlHelper.formatControllerDictionary[this.GetType()][methodName].formatUrl, urlParameters);
        }
        /// <summary>
        /// 模板渲染函数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        /// <param name="url"></param>
        /// <param name="t"></param>
        public bool RenderModel<T>(string url, T t)
        {
            NFinal.ViewDelegateData dele;
            if (NFinal.ViewHelper.viewFastDic != null)
            {
                if (NFinal.ViewHelper.viewFastDic.TryGetValue(url, out dele))
                {
                    if (dele.renderMethod == null)
                    {
                        dele.renderMethod = NFinal.ViewHelper.GetRenderDelegate<T>(url,dele.renderMethodInfo);
                        NFinal.ViewHelper.viewFastDic[url] = dele;
                    }
                    var render = (NFinal.RenderMethod<T>)dele.renderMethod;
                    render(this, t);
                    return true;
                }
                else
                {
                    //模板未找到异常
                    throw new NFinal.Exceptions.ViewNotFoundException(url);
                }
            }
            else
            {
                throw new NFinal.Exceptions.ViewNotFoundException(url);
            }
        }
        /// <summary>
        /// 按照默认路径渲染模板
        /// </summary>
        public void Render()
        {
            //CoreWebTest/Index1.cshtml
            Type controllerType= this.GetType();
            string[] nameSpace= controllerType.Namespace.Split('.');
            using (StringWriter sw = new StringWriter())
            {
                for (int i = 0; i < nameSpace.Length; i++)
                {
                    sw.Write("/");
                    if (nameSpace[i] == "Controllers")
                    {
                        sw.Write("Views");
                    }
                    else
                    {
                        sw.Write(nameSpace[i]);
                    }
                }
                sw.Write("/");
                sw.Write(controllerType.Name);
                sw.Write("/");
                sw.Write(this.methodName);
                sw.Write(".cshtml"); sw.Dispose();
                this.RenderModel(sw.ToString(), ViewBag);
                sw.Dispose();
            }
        }
        /// <summary>
        /// 按照指定路径渲染模板
        /// </summary>
        /// <param name="templateUrl"></param>
        public void Render(string templateUrl)
        {
            this.RenderModel(templateUrl, ViewBag);
        }
        /// <summary>
        /// 根据母模板路径和当前路径渲染模板
        /// </summary>
        /// <param name="masterPagePath"></param>
        /// <param name="templateUrl"></param>
        public void Render(string masterPagePath, string templateUrl)
        {
            if (this.MasterPage.GetType() == typeof(object))
            {
                this.Write("MasterPage必须为非dynamic以及object类型");
            }
            else
            {
                this.MasterPage.ViewData = new ViewData(templateUrl, this.ViewBag);
                this.RenderModel(masterPagePath,this.MasterPage);
            }
        }
        #endregion
    }
}