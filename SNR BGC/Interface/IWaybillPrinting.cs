using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SNR_BGC.Interface
{
    public interface IWaybillPrinting
    {
        Task<string> GetShippingDocumentParameter(string access_token, string orderId, string packageNumber);
        Task<string> CreateShippingDocument(string access_token, string orderId, string packageNumber, string trackingNumber, string shippingDocumentType);
        Task<string> GetShippingDocumentResult(string access_token, string orderId, string packageNumber, string shippingDocumentType);
        Task<string> DownloadShippingDocument(string access_token, string orderId, string packageNumber, string shippingDocumentType);
    }
}
