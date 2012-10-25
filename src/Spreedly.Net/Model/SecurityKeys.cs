using System;
using System.Net;
using System.Text;

namespace Spreedly.Net.Model
{
    internal class SecurityKeys : UsernamePasswordKeys
    {

        internal SecurityKeys(string username_, string password_, string gatewayToken_, params string[] redactedTokens_):base(username_,password_)
        {
            LastGatewayToken = gatewayToken_;
            RedactedTokens = redactedTokens_;
        }

        internal string LastGatewayToken { get; set; }
        internal string[] RedactedTokens { get; set; }

    }
}