using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
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

        public T DoPostJson<T>(string url, object data, IDictionary<string, string> headers)
        {
            var body = JsonConvert.SerializeObject(data);
            var rspJson = DoPostJsonWithHeaders(url, headers, body);
            return JsonConvert.DeserializeObject<T>(rspJson);
        }

        public byte[] DoPostJsonBytes(string url, object data)
        {
            var body = JsonConvert.SerializeObject(data);
            return DoPostJsonBytes(url, body);
        }

        public byte[] DoPostJsonBytes(string url, string data)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "application/json;charset=utf-8";

            byte[] postData = Encoding.UTF8.GetBytes(data);
            System.IO.Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
                return GetResponseBytes(rsp);
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    Stream stream = ex.Response.GetResponseStream();
                    byte[] buf = new byte[unchecked((int)stream.Length)];
                    stream.Read(buf, 0, buf.Length);
                    stream.Close();
                    return buf;
                }
                else
                {
                    throw (ex);
                }
            }
        }

        public async Task<string> DoPostJsonAsync(string url, string data)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "application/json;charset=utf-8";

            byte[] postData = Encoding.UTF8.GetBytes(data);
            Stream reqStream = await req.GetRequestStreamAsync();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                HttpWebResponse rsp = (HttpWebResponse)await req.GetResponseAsync();
                // Encoding encoding = GetEncoding(rsp.CharacterSet);
                Encoding encoding = Encoding.UTF8;

                return GetResponseAsString(rsp, encoding);
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    Stream stream = ex.Response.GetResponseStream();
                    byte[] buf = new byte[unchecked((int)stream.Length)];
                    stream.Read(buf, 0, buf.Length);
                    stream.Close();
                    return Encoding.UTF8.GetString(buf, 0, buf.Length);
                }
                else
                {
                    throw (ex);
                }
            }
        }

        public string DoPostJsonWithHeaders(string url, IDictionary<string, string> headrs, string data)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "application/json;charset=utf-8";

            foreach (var header in headrs)
            {
                req.Headers.Add(header.Key, header.Value);
            }

            byte[] postData = Encoding.UTF8.GetBytes(data);
            Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
                // Encoding encoding = GetEncoding(rsp.CharacterSet);
                Encoding encoding = Encoding.UTF8;

                return GetResponseAsString(rsp, encoding);
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    Stream stream = ex.Response.GetResponseStream();
                    byte[] buf = new byte[unchecked((int)stream.Length)];
                    stream.Read(buf, 0, buf.Length);
                    stream.Close();
                    return Encoding.UTF8.GetString(buf, 0, buf.Length);
                }
                else
                {
                    throw (ex);
                }
            }
        }

        public string DoPostJson(string url, string data)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "application/json;charset=utf-8";

            byte[] postData = Encoding.UTF8.GetBytes(data);
            System.IO.Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
                // Encoding encoding = GetEncoding(rsp.CharacterSet);
                Encoding encoding = Encoding.UTF8;

                return GetResponseAsString(rsp, encoding);
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    Stream stream = ex.Response.GetResponseStream();
                    byte[] buf = new byte[unchecked((int)stream.Length)];
                    stream.Read(buf, 0, buf.Length);
                    stream.Close();
                    return Encoding.UTF8.GetString(buf, 0, buf.Length);
                }
                else
                {
                    throw (ex);
                }
            }
        }

        static Encoding GetEncoding(string CharacterSet)
        {
            if (string.IsNullOrEmpty(CharacterSet))
            {
                return Encoding.UTF8;
            }

            return Encoding.GetEncoding(CharacterSet) ?? Encoding.UTF8;
        }

        /// <summary>
        /// 执行带文件上传的HTTP POST请求。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="textParams">请求文本参数</param>
        /// <param name="fileParams">请求文件参数</param>
        /// <returns>HTTP响应</returns>
        public string DoPost(string url, IDictionary<string, string> textParams, IDictionary<string, FileItem> fileParams)
        {
            // 如果没有文件参数，则走普通POST请求
            if (fileParams == null || fileParams.Count == 0)
            {
                return DoPost(url, textParams);
            }

            string boundary = DateTime.Now.Ticks.ToString("X"); // 随机分隔线

            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;

            System.IO.Stream reqStream = req.GetRequestStream();
            byte[] itemBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundaryBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            // 组装文本请求参数
            string textTemplate = "Content-Disposition:form-data;name=\"{0}\"\r\nContent-Type:text/plain\r\n\r\n{1}";
            IEnumerator<KeyValuePair<string, string>> textEnum = textParams.GetEnumerator();
            while (textEnum.MoveNext())
            {
                string textEntry = string.Format(textTemplate, textEnum.Current.Key, textEnum.Current.Value);
                byte[] itemBytes = Encoding.UTF8.GetBytes(textEntry);
                reqStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                reqStream.Write(itemBytes, 0, itemBytes.Length);
            }

            // 组装文件请求参数
            string fileTemplate = "Content-Disposition:form-data;name=\"{0}\";filename=\"{1}\"\r\nContent-Type:{2}\r\n\r\n";
            IEnumerator<KeyValuePair<string, FileItem>> fileEnum = fileParams.GetEnumerator();
            while (fileEnum.MoveNext())
            {
                string key = fileEnum.Current.Key;
                FileItem fileItem = fileEnum.Current.Value;
                string fileEntry = string.Format(fileTemplate, key, fileItem.GetFileName(), fileItem.GetMimeType());
                byte[] itemBytes = Encoding.UTF8.GetBytes(fileEntry);
                reqStream.Write(itemBoundaryBytes, 0, itemBoundaryBytes.Length);
                reqStream.Write(itemBytes, 0, itemBytes.Length);

                byte[] fileBytes = fileItem.GetContent();
                reqStream.Write(fileBytes, 0, fileBytes.Length);
            }

            reqStream.Write(endBoundaryBytes, 0, endBoundaryBytes.Length);
            reqStream.Close();

            HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();
            Encoding encoding = Encoding.GetEncoding(rsp.CharacterSet);
            return GetResponseAsString(rsp, encoding);
        }

        /// <summary>
        /// 执行HTTP POST请求。
        /// </summary>
        /// <param name="url">请求地址</param>
        /// <param name="parameters">请求参数</param>
        /// <returns>HTTP响应</returns>
        public string DoPost(string url, IDictionary<string, string> parameters)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

            byte[] postData = Encoding.UTF8.GetBytes(BuildQuery(parameters));
            System.IO.Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

                Encoding encoding = GetEncoding(rsp.CharacterSet);
                //Encoding encoding =  Encoding.UTF8;
                return GetResponseAsString(rsp, encoding);
            }
            catch (WebException ex)
            {
                //if (ex.Response == null) return PushUtils.CreateErrorJson(1, ex.Message);
                if (ex.Response != null)
                {
                    Stream stream = ex.Response.GetResponseStream();
                    byte[] buf = new byte[unchecked((int)stream.Length)];
                    stream.Read(buf, 0, buf.Length);
                    stream.Close();
                    return Encoding.UTF8.GetString(buf, 0, buf.Length);
                }
                else
                {
                    throw (ex);
                }
            }
        }

        public string DoPostWithHeaders(string url, IDictionary<string, string> headrs, IDictionary<string, string> parameters)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

            foreach (var header in headrs)
            {
                req.Headers.Add(header.Key, header.Value);
            }

            byte[] postData = Encoding.UTF8.GetBytes(BuildQuery(parameters));
            System.IO.Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

                Encoding encoding = GetEncoding(rsp.CharacterSet);
                //Encoding encoding =  Encoding.UTF8;
                return GetResponseAsString(rsp, encoding);
            }
            catch (WebException ex)
            {
                //if (ex.Response == null) return PushUtils.CreateErrorJson(1, ex.Message);
                if (ex.Response != null)
                {
                    Stream stream = ex.Response.GetResponseStream();
                    byte[] buf = new byte[unchecked((int)stream.Length)];
                    stream.Read(buf, 0, buf.Length);
                    stream.Close();
                    return Encoding.UTF8.GetString(buf, 0, buf.Length);
                }
                else
                {
                    throw (ex);
                }
            }
        }

        public async Task<PutLogsResponse> DoPostWithHeaders(string url, IDictionary<string, string> headers, byte[] postData)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "application/json;charset=utf-8";

            foreach (var header in headers)
            {
                req.Headers.Set(header.Key, header.Value);
            }

            System.IO.Stream reqStream = req.GetRequestStream();
            reqStream.Write(postData, 0, postData.Length);
            reqStream.Close();

            try
            {
                HttpWebResponse rsp = (HttpWebResponse)(await req.GetResponseAsync());
                // Encoding encoding = GetEncoding(rsp.CharacterSet);
                Encoding encoding = Encoding.UTF8;

                var res = new PutLogsResponse();
                res.StatusCode = rsp.StatusCode;
                return res;
                // return GetResponseAsString(rsp, encoding);
            }
            catch (WebException ex)
            {
                throw (ex);
            }
        }

        public string DoPost(string url)
        {
            HttpWebRequest req = GetWebRequest(url, "POST");
            req.ContentType = "application/x-www-form-urlencoded;charset=utf-8";

            System.IO.Stream reqStream = req.GetRequestStream();
            reqStream.Close();

            try
            {
                HttpWebResponse rsp = (HttpWebResponse)req.GetResponse();

                Encoding encoding = GetEncoding(rsp.CharacterSet);
                //Encoding encoding =  Encoding.UTF8;
                return GetResponseAsString(rsp, encoding);
            }
            catch (WebException ex)
            {
                //if (ex.Response == null) return PushUtils.CreateErrorJson(1, ex.Message);
                if (ex.Response != null)
                {
                    Stream stream = ex.Response.GetResponseStream();
                    byte[] buf = new byte[unchecked((int)stream.Length)];
                    stream.Read(buf, 0, buf.Length);
                    stream.Close();
                    return Encoding.UTF8.GetString(buf, 0, buf.Length);
                }
                else
                {
                    throw (ex);
                }
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

            req.Accept = "application/json";

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

    /// <summary>
    /// 文件元数据。
    /// 可以使用以下几种构造方法：
    /// 本地路径：new FileItem("C:/temp.jpg");
    /// 本地文件：new FileItem(new FileInfo("C:/temp.jpg"));
    /// 字节流：new FileItem("abc.jpg", bytes);
    /// </summary>
    public class FileItem
    {
        private string fileName;
        private string mimeType;
        private byte[] content;
        private FileInfo fileInfo;

        /// <summary>
        /// 基于本地文件的构造器。
        /// </summary>
        /// <param name="fileInfo">本地文件</param>
        public FileItem(FileInfo fileInfo)
        {
            if (fileInfo == null || !fileInfo.Exists)
            {
                throw new ArgumentException("fileInfo is null or not exists!");
            }
            this.fileInfo = fileInfo;
        }

        /// <summary>
        /// 基于本地文件全路径的构造器。
        /// </summary>
        /// <param name="filePath">本地文件全路径</param>
        public FileItem(string filePath)
            : this(new FileInfo(filePath))
        { }

        /// <summary>
        /// 基于文件名和字节流的构造器。
        /// </summary>
        /// <param name="fileName">文件名称（服务端持久化字节流到磁盘时的文件名）</param>
        /// <param name="content">文件字节流</param>
        public FileItem(string fileName, byte[] content)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException("fileName");
            if (content == null || content.Length == 0) throw new ArgumentNullException("content");

            this.fileName = fileName;
            this.content = content;
        }

        /// <summary>
        /// 基于文件名、字节流和媒体类型的构造器。
        /// </summary>
        /// <param name="fileName">文件名（服务端持久化字节流到磁盘时的文件名）</param>
        /// <param name="content">文件字节流</param>
        /// <param name="mimeType">媒体类型</param>
        public FileItem(string fileName, byte[] content, string mimeType)
            : this(fileName, content)
        {
            if (string.IsNullOrEmpty(mimeType)) throw new ArgumentNullException("mimeType");
            this.mimeType = mimeType;
        }

        public string GetFileName()
        {
            if (this.fileName == null && this.fileInfo != null && this.fileInfo.Exists)
            {
                this.fileName = this.fileInfo.FullName;
            }
            return this.fileName;
        }

        public string GetMimeType()
        {
            if (this.mimeType == null)
            {
                this.mimeType = "application/octet-stream";
            }
            return this.mimeType;
        }

        public byte[] GetContent()
        {
            if (this.content == null && this.fileInfo != null && this.fileInfo.Exists)
            {
                using (System.IO.Stream fileStream = this.fileInfo.OpenRead())
                {
                    this.content = new byte[fileStream.Length];
                    fileStream.Read(content, 0, content.Length);
                }
            }

            return this.content;
        }
    }
}
