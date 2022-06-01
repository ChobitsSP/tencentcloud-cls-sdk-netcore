using Serilog.Events;
using Serilog.Parsing;
using Serilog.Sinks.TencentCloud.Sinks.Http.BatchFormatters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Tencent.Cls.Sdk.Models;
using Tencent.Cls.Sdk.Utils;

namespace Tencent.Cls.Sdk
{
    public class TxClsClient
    {
        public string HostPath { get; set; } = "https://ap-guangzhou.cls.tencentcs.com";
        public string HostName
        {
            get
            {
                if (string.IsNullOrEmpty(this.HostPath)) return null;
                return this.HostPath.Split('/')[2];
            }
        }

        public string SecretId { get; set; }
        public string SecretKey { get; set; }
        public string TopicId { get; set; }
        public string SourceIp { get; set; }
        public int RetryTimes { get; set; } = 1;

        public async Task<PutLogsResponse> PutLogs(PutLogsRequest request)
        {
            var logBytes = request.getLogGroupBytes(this.SourceIp);
            if (logBytes.Length > Constants.CONST_MAX_PUT_SIZE)
            {
                throw new Exception($"InvalidLogSize.logItems' size exceeds maximum limitation : { Constants.CONST_MAX_PUT_SIZE} bytes, logBytes={logBytes.Length}, topic={request.Topic}");
            }

            var headParameter = this.getCommonHeadPara();
            request[Constants.TOPIC_ID] = request.Topic;
            Dictionary<string, string> urlParameter = request;

            for (var retryTimes = 0; retryTimes < this.RetryTimes; retryTimes++)
            {
                try
                {
                    var res = await this.sendLogs(Constants.CONST_HTTP_METHOD_POST, Constants.UPLOAD_LOG_RESOURCE_URI, urlParameter, headParameter, logBytes, request.Topic);
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        return res;
                    }
                    if (retryTimes + 1 >= this.RetryTimes)
                    {
                        throw new Exception("send log failed and exceed retry times");
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return null;
        }

        public Task<PutLogsResponse> EmitBatchAsync(params LogEvent[] logEvents)
        {
            return EmitBatchAsync(this.TopicId, logEvents);
        }

        public async Task<PutLogsResponse> SendLog(Dictionary<string, string> body)
        {
            var logBytes = new ClsFormatter().Format(body);

            var topic = this.TopicId;

            if (logBytes.Length > Constants.CONST_MAX_PUT_SIZE)
            {
                throw new Exception($"InvalidLogSize.logItems' size exceeds maximum limitation : { Constants.CONST_MAX_PUT_SIZE} bytes, logBytes={logBytes.Length}, topic={topic}");
            }

            var headParameter = this.getCommonHeadPara();
            var urlParameter = new Dictionary<string, string>();
            urlParameter[Constants.TOPIC_ID] = topic;

            for (var retryTimes = 0; retryTimes < this.RetryTimes; retryTimes++)
            {
                try
                {
                    var res = await this.sendLogs(Constants.CONST_HTTP_METHOD_POST, Constants.UPLOAD_LOG_RESOURCE_URI, urlParameter, headParameter, logBytes, topic);
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        return res;
                    }
                    if (retryTimes + 1 >= this.RetryTimes)
                    {
                        throw new Exception(res.errormessage);
                        // throw new Exception("send log failed and exceed retry times");
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return null;
        }

        /// <summary>
        /// Emit a batch of log events, running asynchronously.
        /// </summary>
        public async Task<PutLogsResponse> EmitBatchAsync(string topic, params LogEvent[] logEvents)
        {
            var logBytes = new ClsFormatter().Format(logEvents);

            if (logBytes.Length > Constants.CONST_MAX_PUT_SIZE)
            {
                throw new Exception($"InvalidLogSize.logItems' size exceeds maximum limitation : { Constants.CONST_MAX_PUT_SIZE} bytes, logBytes={logBytes.Length}, topic={topic}");
            }

            var headParameter = this.getCommonHeadPara();
            var urlParameter = new Dictionary<string, string>();
            urlParameter[Constants.TOPIC_ID] = topic;

            for (var retryTimes = 0; retryTimes < this.RetryTimes; retryTimes++)
            {
                try
                {
                    var res = await this.sendLogs(Constants.CONST_HTTP_METHOD_POST, Constants.UPLOAD_LOG_RESOURCE_URI, urlParameter, headParameter, logBytes, topic);
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        return res;
                    }
                    if (retryTimes + 1 >= this.RetryTimes)
                    {
                        throw new Exception(res.errormessage);
                        // throw new Exception("send log failed and exceed retry times");
                    }
                }
                catch (Exception ex)
                {
                    throw;
                }
            }

            return null;
        }

        public Task<PutLogsResponse> sendLogs(string method, string resourceUri, IDictionary<string, string> urlParameter, IDictionary<string, string> headParameter, byte[] body, string topic)
        {
            headParameter[Constants.CONST_CONTENT_LENGTH] = body.Length.ToString();
            var signature_str = TopUtils.GetSign(this.SecretId, this.SecretKey, method, resourceUri, urlParameter, headParameter, 300000);
            headParameter[Constants.CONST_AUTHORIZATION] = signature_str;

            var webUtils = new WebUtils();

            var url = WebUtils.BuildGetUrl(this.HostPath + resourceUri, new Dictionary<string, string>()
            {
               { Constants.TOPIC_ID, topic },
            });

            return webUtils.DoPostWithHeadersAsync(url, headParameter, body);
        }

        private Dictionary<string, string> getCommonHeadPara()
        {
            var headParameter = new Dictionary<string, string>();
            headParameter.Add(Constants.CONST_CONTENT_LENGTH, "0");
            headParameter.Add(Constants.CONST_CONTENT_TYPE, Constants.CONST_PROTO_BUF);
            headParameter.Add(Constants.CONST_HOST, this.HostName);
            return headParameter;
        }
    }
}