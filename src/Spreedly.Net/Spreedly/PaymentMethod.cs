using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Spreedly.Net.Extensions;

namespace Spreedly.Net.BuiltIns
{
    public class PaymentMethod
    {
        public PaymentMethod(string raw)
        {
            Raw = raw;
        }

        public string Raw { get; set; }

        public string Token { get; set; }

        public string StorageState { get; set; }

        public string Number { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Zip { get; set; }

        public string Country { get; set; }

        public DateTime CreatedAt { get; set; }

        public int ExpirationMonth { get; set; }

        public int ExpirationYear { get; set; }

        public string CardType { get; set; }
        
        public static PaymentMethod FromXml(XDocument doc)
        {
            var element = doc.Element("payment_method");
            return FromXml(element);
        }

        public static PaymentMethod FromXml(XElement element)
        {
            if (element == null)
            {
                return null;
            }

            var ret = new PaymentMethod(element.ToString());
            ret.Token = element.GetStringChild("token");
            ret.Number = element.GetStringChild("number");
            ret.StorageState = element.GetStringChild("storage_state");
            ret.FirstName = element.GetStringChild("first_name");
            ret.LastName = element.GetStringChild("last_name");
            ret.CreatedAt = DateTime.Parse(element.GetStringChild("created_at"));

            ret.ExpirationMonth = int.Parse(element.GetStringChild("month"));
            ret.ExpirationYear = int.Parse(element.GetStringChild("year"));
            ret.CardType = element.GetStringChild("card_type");
            ret.Address1 = element.GetStringChild("address1");
            ret.Address2 = element.GetStringChild("address2");
            ret.City = element.GetStringChild("city");
            ret.State = element.GetStringChild("state");
            ret.Zip = element.GetStringChild("zip");
            ret.Country = element.GetStringChild("country");

            return ret;
        }

        public static List<PaymentMethod> ListFromXml(XDocument doc)
        {
            List<PaymentMethod> paymentMethods = new List<PaymentMethod>();

            var methods = doc.Element("payment_methods").Elements("payment_method");
            if (methods == null)
            {
                return paymentMethods;
            }

            foreach (var pm in methods)
            {
                var ret = FromXml(pm);
                paymentMethods.Add(ret);
            }

            return paymentMethods;
        }
    }
}
