using NETCore.Encrypt.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Tencent.Cls.Sdk.Utils
{
    public static class TopUtils
    {
        static readonly string[] HEADER_KEYS = new string[] { "content-type", "content-md5", "host", "x" };

        static string CalSha1sum(string msg)
        {
            return msg.SHA1();
            //var enc = Encoding.UTF8;
            //var hasher = new HMACSHA1();
            //var baText2BeHashed = enc.GetBytes(msg);
            //byte[] baHashedText = hasher.ComputeHash(baText2BeHashed);
            //return string.Concat(baHashedText.Select(b => b.ToString("x2")));
        }

        static string CalSha1HMACDigest(string key, string msg)
        {
            return msg.HMACSHA1(key).ToLower();
            //var enc = Encoding.UTF8;
            //var baSalt = enc.GetBytes(key);
            //var hasher = new HMACSHA1(baSalt);
            //var baText2BeHashed = enc.GetBytes(msg);
            //byte[] baHashedText = hasher.ComputeHash(baText2BeHashed);
            //return string.Concat(baHashedText.Select(b => b.ToString("x2")));
        }

        static IEnumerable<string> getHeaderKeylist(IDictionary<string, string> dic)
        {
            return dic.Keys.Select(t => t.ToLower()).Where(t => HEADER_KEYS.Contains(t)).OrderBy(t => t);
        }

        public static string GetTencentCloudTimeStamp(this DateTimeOffset offset)
        {
            var endOffset = offset.AddHours(1);
            return $"{offset.ToUnixTimeSeconds()};{endOffset.ToUnixTimeSeconds()}";
        }

        public static string GetSign(string secretId, string secretKey, string method, string path, IDictionary<string, string> dic, IDictionary<string, string> headers, long expire)
        {

            var timeStr = DateTimeOffset.Now.GetTencentCloudTimeStamp();
            var time = timeStr.Split(';');
            var nowStartTime = Convert.ToInt64(time[0]);

            var signature = GetSignature("post", path, timeStr, timeStr, secretKey);
            var authorizationString = $"q-sign-algorithm=sha1&q-ak={secretId}&q-sign-time={timeStr}&q-key-time={timeStr}&q-header-list=&q-url-param-list=&q-signature={signature}";
            return authorizationString;




            // 签名有效起止时间
            var now = DateTimeOffset.Now.ToUnixTimeSeconds();
            var exp = now + expire;
            now = now - 60;

            // 要用到的 Authorization 参数列表
            var qSignAlgorithm = "sha1";
            var qAk = secretId;

            var qSignTime = DateTimeOffset.Now.GetTencentCloudTimeStamp();

            // var qSignTime = now + ';' + exp;
            var qpathTime = qSignTime;

            var keys1 = getHeaderKeylist(headers).ToArray();
            var keys2 = dic.Keys.OrderBy(t => t).ToArray();

            var qHeaderList = string.Join(";", keys1);
            var qUrlParamList = string.Join(";", keys2);

            // 步骤一：计算 Signpath
            var signpath = CalSha1HMACDigest(secretKey, qpathTime.ToString());

            // 步骤二：构成 FormatString
            var formatString = string.Join("\n", method.ToLower(), path, DicToStr(dic, keys2), DicToStr(headers, keys1), string.Empty);

            // 步骤三：计算 StringToSign
            var res = CalSha1sum(formatString);
            var stringToSign = string.Join("\n", "sha1", qSignTime, res, string.Empty);

            // 步骤四：计算 Signature
            var qSignature = CalSha1HMACDigest(signpath, stringToSign);

            // 步骤五：构造 Authorization
            var authorization = string.Join("&", new string[] {
                "q-sign-algorithm=" + qSignAlgorithm,
                "q-ak=" + qAk,
                "q-sign-time=" + qSignTime,
                "q-key-time=" + qpathTime,
                "q-header-list=" + qHeaderList,
                "q-url-param-list=" + qUrlParamList,
                "q-signature=" + qSignature
            });

            return authorization;
        }

        public static string CamSafeUrlEncode(string str)
        {
            return HttpUtility.UrlEncode(str, Encoding.UTF8)
                .Replace("!", "%21")
                .Replace("'", "%27")
                .Replace("(", "%28")
                .Replace(")", "%29")
                .Replace("*", "%2A")
                ;
        }

        static string DicToStr(IDictionary<string, string> dic, string[] keyList)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < keyList.Length; i++)
            {
                var key = keyList[i];

                var val = dic.ContainsKey(key) ? dic[key] : string.Empty;

                key = CamSafeUrlEncode(key.ToLower());
                val = CamSafeUrlEncode(val);

                if (i > 0) sb.Append("&");
                sb.AppendFormat("{0}={1}", key, val);
            }

            return sb.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method">HTTP 请求使用的方法，小写字母，如 get、post等</param>
        /// <param name="uri">HTTP 请求的资源名称，不包含 query string 部分，如 /logset</param>
        /// <returns>HttpRequestInfo</returns>
        private static string GetHttpRequestInfo(string method, string uri)
        {
            return $"{method}\n{uri}\n\n\n";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q_sign_algorithm">签名算法，目前仅支持 sha1</param>
        /// <param name="q_sign_time">签名有效起止时间，Unix时间戳，以秒为单位，;分隔</param>
        /// <param name="httpRequestInfo">HttpRequestInfo</param>
        /// <returns></returns>
        private static string GetStringToSign(string q_sign_algorithm, string q_sign_time, string httpRequestInfo)
        {
            return $"{q_sign_algorithm}\n{q_sign_time}\n{httpRequestInfo.SHA1()}\n".ToLower();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="q_key_time">Unix时间戳，以秒为单位，;分隔</param>
        /// <param name="secretKey">腾讯云API的SecretKey</param>
        /// <returns></returns>
        private static string GetSignKey(string q_key_time, string secretKey)
        {
            return q_key_time.HMACSHA1(secretKey).ToLower();
        }
        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="stringToSign">StringToSign</param>
        /// <param name="signKey">SignKey</param>
        /// <returns></returns>
        private static string GetSignature(string stringToSign, string signKey)
        {
            return stringToSign.HMACSHA1(signKey).ToLower();
        }

        /// <summary>
        /// 获取签名字符串
        /// </summary>
        /// <param name="method">HTTP 请求使用的方法，小写字母，如 get、post等</param>
        /// <param name="uri">HTTP 请求的资源名称，不包含 query string 部分，如 /logset</param>
        /// <param name="q_sign_algorithm">签名算法，目前仅支持 sha1</param>
        /// <param name="q_sign_time">签名有效起止时间，Unix时间戳，以秒为单位，;分隔</param>
        /// <param name="q_key_time">Unix时间戳，以秒为单位，;分隔</param>
        /// <param name="secretKey">腾讯云API的SecretKey</param>
        /// <returns>Signature签名字符串</returns>
        public static string GetSignature(string method, string uri, string q_sign_time, string q_key_time, string secretKey)
        {
            var httpRequestInfo = GetHttpRequestInfo(method, uri);
            var stringToSign = GetStringToSign("sha1", q_sign_time, httpRequestInfo);
            var signKey = GetSignKey(q_key_time, secretKey);

            return GetSignature(stringToSign, signKey);
        }
    }
}
