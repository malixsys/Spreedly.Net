using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Spreedly.Net.BuiltIns
{
    public class CreditCard
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }

        //http://en.wikipedia.org/wiki/ISO_3166-1_alpha-2
        public string Country { get; set; }

        public string Number { get; set; }
        public string CVV { get; set; }

        public int ExpirationMonth { get; set; }

        public int ExpirationYear { get; set; }

        public string ToXml()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<credit_card>");

            if (!string.IsNullOrEmpty(this.FirstName))
                sb.Append(string.Format("<first_name>{0}</first_name>", XmlEscape(this.FirstName)));
            if (!string.IsNullOrEmpty(this.LastName))
                sb.Append(string.Format("<last_name>{0}</last_name>", XmlEscape(this.LastName)));
            
            if (!string.IsNullOrEmpty(this.Address1))
                sb.Append(string.Format("<address1>{0}</address1>", XmlEscape(this.Address1)));

            if (!string.IsNullOrEmpty(this.Address2))
                sb.Append(string.Format("<address2>{0}</address2>", XmlEscape(this.Address2)));

            sb.Append(string.Format("<city>{0}</city>", XmlEscape(this.City)));
            sb.Append(string.Format("<state>{0}</state>", XmlEscape(this.State)));
            sb.Append(string.Format("<zip>{0}</zip>", XmlEscape(this.PostalCode)));

            if (!string.IsNullOrEmpty(this.Country))
                sb.Append(string.Format("<country>{0}</country>", XmlEscape(this.Country)));

            sb.Append(string.Format("<number>{0}</number>", XmlEscape(this.Number)));
            sb.Append(string.Format("<verification_value>{0}</verification_value>", XmlEscape(this.CVV)));
            sb.Append(string.Format("<month>{0}</month>", this.ExpirationMonth));
            sb.Append(string.Format("<year>{0}</year>", this.ExpirationYear));

            sb.Append("</credit_card>");

            return sb.ToString();
        }

        public static string XmlEscape(string unescaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        public static string XmlUnescape(string escaped)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode node = doc.CreateElement("root");
            node.InnerXml = escaped;
            return node.InnerText;
        }
    }
}
