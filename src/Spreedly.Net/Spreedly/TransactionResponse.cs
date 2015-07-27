using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Spreedly.Net.Extensions;

namespace Spreedly.Net.BuiltIns
{
    public class TransactionResponse
    {
        public TransactionResponse(XElement node)
        {
            this.Success = string.Equals(node.GetStringChild("success"), "true", StringComparison.CurrentCultureIgnoreCase);
            this.Pending = string.Equals(node.GetStringChild("pending"), "true", StringComparison.CurrentCultureIgnoreCase);
            this.Cancelled = string.Equals(node.GetStringChild("cancelled"), "true", StringComparison.CurrentCultureIgnoreCase);
            this.FraudReview = string.Equals(node.GetStringChild("fraud_review"), "true", StringComparison.CurrentCultureIgnoreCase);

            this.Message = node.GetStringChild("message");
            this.AvsCode = node.GetStringChild("avs_code");
            this.AvsMessage = node.GetStringChild("avs_message");
            this.CvvCode = node.GetStringChild("cvv_code");
            this.CvvMessage = node.GetStringChild("cvv_message");
            this.ErrorCode = node.GetStringChild("error_code");
            this.ErrorDetail = node.GetStringChild("error_detail");

            this.CreatedAt = DateTime.Parse(node.GetStringChild("created_at"));

            var _updatedAt = node.GetStringChild("updated_at", null);
            this.UpdatedAt = _updatedAt == null ? (DateTime?)null : DateTime.Parse(_updatedAt);
        }

        public bool Success { get; private set; }

        public string Message { get; private set; }

        public string AvsCode { get; private set; }

        public string AvsMessage { get; private set; }

        public string CvvCode { get; private set; }

        public string CvvMessage { get; private set; }

        public bool Pending { get; private set; }

        public string ErrorCode { get; private set; }

        public string ErrorDetail { get; private set; }

        public bool Cancelled { get; private set; }

        public bool FraudReview { get; private set; }

        public DateTime CreatedAt { get; private set; }

        public DateTime? UpdatedAt { get; private set; }
    }
}
