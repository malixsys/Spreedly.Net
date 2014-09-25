using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Spreedly.Net.Extensions;

namespace Spreedly.Net.BuiltIns
{
    public class Gateway
    {
        private string _token;
        private bool _enabled;
        private string _type;

        public static IEnumerable<Gateway> FromXml(XDocument doc)
        {
            return doc.Descendants("gateway").Select(node => new Gateway(node)).ToList();
        }

        public Gateway(XElement node)
        {
            _token = node.GetStringChild("token");
            
            _type = node.GetStringChild("gateway_type");

            var redacted = node.GetStringChild("redacted");
            _enabled = redacted != null && string.Equals(redacted, "false", StringComparison.InvariantCultureIgnoreCase);
        }


        public string Token
        {
            get {
                return _token;
            }
        }

        public bool Enabled
        {
            get {
                return _enabled;
            }
        }

        public string Type
        {
            get {
                return _type;
            }
        }
    }
}
