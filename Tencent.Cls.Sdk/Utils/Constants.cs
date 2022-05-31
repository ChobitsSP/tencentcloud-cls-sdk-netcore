namespace Tencent.Cls.Sdk
{
    public static class Constants
    {
        public const string CONST_CONTENT_TYPE = "Content-Type";
        public const string CONST_PROTO_BUF = "application/x-protobuf";
        public const string CONST_CONTENT_LENGTH = "Content-Length";
        public const string CONST_AUTHORIZATION = "Authorization";
        public const string CONST_GZIP_ENCODING = "deflate";
        public const string CONST_HTTP_METHOD_POST = "POST";
        public const string CONST_X_SLS_REQUESTID = "x-log-requestid";
        public const string CONST_HOST = "Host";
        public const string CONST_MD5 = "MD5";
        public const string UTF_8_ENCODING = "UTF-8";
        public const string CONST_LOCAL_IP = "127.0.0.1";
        public const int HTTP_CONNECT_TIME_OUT = 60 * 1000;
        public const int HTTP_SEND_TIME_OUT = 60 * 1000;
        public const string TOPIC_ID = "topic_id";
        public const string UPLOAD_LOG_RESOURCE_URI = "/structuredlog";
        public const int CONST_MAX_PUT_SIZE = 1 * 1024 * 1024;
        public const string CONST_X_SLS_COMPRESSTYPE = "x-cls-compress-type";
        public const string CONST_LZ4 = "lz4";
    }
}