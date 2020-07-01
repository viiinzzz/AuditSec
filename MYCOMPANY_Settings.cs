using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuditSec
{
    class MYCOMPANY_Settings
    {
        public static string[][] SMS_SERVER_SITE_DESC = new string[][] {
            new string[] {"servername1", "sitename1", "New SCCM Environment"},
            new string[] {"servername2", "sitename2", "Old SCCM Environment"},
        };

        public static string SMS_DEFAULT_SERVER = SMS_SERVER_SITE_DESC[0][0];
        public static string SMS_DEFAULT_SITE = SMS_SERVER_SITE_DESC[0][1];

    }
}
    
