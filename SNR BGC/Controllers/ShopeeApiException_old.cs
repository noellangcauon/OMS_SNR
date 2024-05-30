using System;
using System.Runtime.Serialization;

namespace SNR_BGC.Controllers
{
    [Serializable]
    internal class ShopeeApiException : Exception
    {
        public ShopeeApiException()
        {
        }

        public ShopeeApiException(string message) : base(message)
        {
        }

        public ShopeeApiException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ShopeeApiException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public string PayloadDescription { get; set; } = string.Empty;
        public string RequestUrl { get; set; } = string.Empty;
        public string RequestMethod { get; set; } = string.Empty;
        public string ResponseDescription { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public object HttpStatusCode { get; set; }
    }
}