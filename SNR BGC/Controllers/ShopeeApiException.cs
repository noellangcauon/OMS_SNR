using System;
using System.Net;
using System.Runtime.Serialization;

namespace Infrastructure.External.ShopeeWebApi
{

    [Serializable]
    public class ShopeeApiException : Exception
    {
        public string PayloadDescription { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string RequestMethod { get; set; } = string.Empty;
        public string ResponseDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public HttpStatusCode? HttpStatusCode { get; set; }


        public ShopeeApiException()
        {
        }

        public ShopeeApiException(string? message) : base(message)
        {
        }

        public ShopeeApiException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected ShopeeApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}