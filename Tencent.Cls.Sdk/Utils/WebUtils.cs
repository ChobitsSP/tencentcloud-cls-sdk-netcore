﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Tencent.Cls.Sdk.Models;

namespace Tencent.Cls.Sdk
{
    /// <summary>
    /// 网络工具类。
    /// </summary>
    internal sealed class WebUtils
    {
        /// <summary>
        /// 请求与响应的超时时间
        /// </summary>
        public int Timeout { get; set; } = 100000;

        static Encoding GetEncoding(string CharacterSet)
        {
            if (string.IsNullOrEmpty(CharacterSet))
            {
                return Encoding.UTF8;
            }

            return Encoding.GetEncoding(CharacterSet) ?? Encoding.UTF8;
        }

        public PutLogsResponse DoPostWithHeaders(string url, IDictionary<string, string> headers, byte[] postData)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");

            foreach (var header in headers)
            {
                req.Headers.Set(header.Key, header.Value);
            }

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                // var rsp = (HttpWebResponse)await Task.Factory.FromAsync<WebResponse>(req.BeginGetResponse, req.EndGetResponse, null);

                var rsp = (HttpWebResponse)req.GetResponse();

                var res = new PutLogsResponse();
                res.Headers = new Dictionary<string, string>();
                res.StatusCode = rsp.StatusCode;

                foreach (var key in rsp.Headers.AllKeys)
                {
                    res.Headers[key] = rsp.Headers.GetValues(key).FirstOrDefault();
                }

                return res;
            }
            catch (WebException ex)
            {
                if (ex.Response == null) throw;
                var res = GetJsonRsp<PutLogsResponse>(ex.Response);
                res.StatusCode = (ex.Response as HttpWebResponse).StatusCode;
                return res;
            }
        }


        public async Task<PutLogsResponse> DoPostWithHeadersAsync(string url, IDictionary<string, string> headers, byte[] postData)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");

            foreach (var header in headers)
            {
                req.Headers.Set(header.Key, header.Value);
            }

            Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                // var rsp = await req.GetRequestStreamAsync();

                var rsp = (HttpWebResponse)req.GetResponse();

                var res = new PutLogsResponse();
                res.Headers = new Dictionary<string, string>();
                res.StatusCode = HttpStatusCode.OK;

                //foreach (var key in rsp.Headers.AllKeys)
                //{
                //    res.Headers[key] = rsp.Headers.GetValues(key).FirstOrDefault();
                //}

                return res;
            }
            catch (WebException ex)
            {
                if (ex.Response == null) throw;
                var res = GetJsonRsp<PutLogsResponse>(ex.Response);
                res.StatusCode = (ex.Response as HttpWebResponse).StatusCode;
                return res;
            }
        }

        static T GetJsonRsp<T>(WebResponse rsp)
        {
            using (var ms = new MemoryStream())
            {
                rsp.GetResponseStream().CopyTo(ms);
                byte[] buf = ms.ToArray();
                ms.Flush();
                ms.Dispose();
                var errjson = Encoding.UTF8.GetString(buf, 0, buf.Length);
                return JsonConvert.DeserializeObject<T>(errjson);
            }
        }

        /// <summary>
        /// 执行HTTP GET请求。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>HTTP响应</returns>
        public string DoGet(string url, IDictionary<string, string> parameters)
        {
            url = BuildGetUrl(url, parameters);
            HttpWebRequest req = GetWebRequest(url, "GET");
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

            Encoding encoding = GetEncoding(rsp.CharacterSet);

            return GetResponseAsString(rsp, encoding);
        }

        public HttpWebRequest GetWebRequest(string url, string method)
        {
            HttpWebRequest req = null;
            if (url.Contains("https"))
            {
                //直接确认，否则打不开
                ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                req = (HttpWebRequest)WebRequest.CreateDefault(new Uri(url));
            }
            else
            {
                req = (HttpWebRequest)WebRequest.Create(url);
            }

            req.ServicePoint.Expect100Continue = false;
            req.Method = method;
            req.KeepAlive = true;
            req.Timeout = this.Timeout;

            return req;
        }

        /// <summary>
        /// 把响应流转换为文本。
        /// </summary>
        /// <param name="rsp">响应流对象</param>
        /// <param name="encoding">编码方式</param>
        /// <returns>响应文本</returns>
        public static string GetResponseAsString(HttpWebResponse rsp, Encoding encoding)
        {
            System.IO.Stream stream = null;
            StreamReader reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                stream = rsp.GetResponseStream();
                reader = new StreamReader(stream, encoding);
                return reader.ReadToEnd();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }

        public static byte[] GetResponseBytes(HttpWebResponse rsp)
        {
            System.IO.Stream stream = null;
            MemoryStream reader = null;

            try
            {
                // 以字符流的方式读取HTTP响应
                reader = new MemoryStream();
                rsp.GetResponseStream().CopyTo(reader);
                return reader.ToArray();
            }
            finally
            {
                // 释放资源
                if (reader != null) reader.Close();
                if (stream != null) stream.Close();
                if (rsp != null) rsp.Close();
            }
        }

        /// <summary>
        /// 组装GET请求URL。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>带参数的GET请求URL</returns>
        public static string BuildGetUrl(string url, IDictionary<string, string> parameters)
        {
            if (parameters != null && parameters.Count > 0)
            {
                if (url.Contains("?"))
                {
                    url = url + "&" + BuildQuery(parameters);
                }
                else
                {
                    url = url + "?" + BuildQuery(parameters);
                }
            }
            return url;
        }

        /// <summary>
        /// 组装普通文本请求参数。
        /// </summary>
        /// <param name="parameters">Key-Value形式请求参数字典</param>
        /// <returns>URL编码后的请求数据</returns>
        public static string BuildQuery(IDictionary<string, string> parameters)
        {
            StringBuilder postData = new StringBuilder();
            bool hasParam = false;

            IEnumerator<KeyValuePair<string, string>> dem = parameters.GetEnumerator();
            while (dem.MoveNext())
            {
                string name = dem.Current.Key;
                string value = dem.Current.Value;
                // 忽略参数名或参数值为空的参数
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
                {
                    if (hasParam)
                    {
                        postData.Append("&");
                    }

                    postData.Append(name);
                    postData.Append("=");
                    postData.Append(HttpUtility.UrlEncode(value, Encoding.UTF8));
                    hasParam = true;
                }
            }

            return postData.ToString();
        }
    }
}