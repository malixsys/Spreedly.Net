using System;
using System.Net;
using System.Text;

namespace Spreedly.Net.Model
{
    public class UsernamePasswordKeys
    {
        public UsernamePasswordKeys(string username, string password)
        {
            Credentials = new NetworkCredential(username, password);
            Authorizationheader = Convert.ToBase64String(Encoding.UTF8.GetBytes(String.Format("{0}:{1}", username, password)));
        }
        internal NetworkCredential Credentials { get; private set; }

        internal string Authorizationheader { get; private set; }
    }
}