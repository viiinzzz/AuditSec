using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using System.Collections;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Windows.Forms;
using System.DirectoryServices.ActiveDirectory;
using System.Threading;

namespace AuditSec
{
    public class UsersInfo
    {
        Hashtable USERS = new Hashtable();
        Hashtable USRDU = new Hashtable();
        Hashtable USRDE = new Hashtable();
        Hashtable USRNT = new Hashtable();
        Hashtable USREN = new Hashtable();
        Hashtable USRCC = new Hashtable();
        Hashtable USRAD = new Hashtable();
        Hashtable USRSP = new Hashtable();
        Hashtable USRTL = new Hashtable();

        public int getUsersCount() { return USERS.Count; }

        public string getUsernameFromDisplayname(string desc)
        {
            string ret = null;
            try
            {
                ret = USERS.ContainsKey(desc) ? USERS[desc].ToString() : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid desc: " + desc); }
            return ret;
        }

        public string getSipFromDisplayname(string desc)
        {
            string ret = null;
            try
            {
                ret = USRSP.ContainsKey(desc) ? USRSP[desc].ToString() : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid desc: " + desc); }
            return ret;
        }

        public string getTelFromDisplayname(string desc)
        {
            string ret = null;
            try
            {
                ret = USRTL.ContainsKey(desc) ? USRTL[desc].ToString() : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid desc: " + desc); }
            return ret;
        }

        public string getOfficeTelFromDisplayname(string desc)
        {
            string tel = getTelFromDisplayname(desc);
            if (tel == null) return null;
            tel = tel.Split(new char[] { ';' })[0].Replace("extension:", "").Trim();
            return tel.Length == 0 ? null : tel;
        }

        public string getMobileTelFromDisplayname(string desc)
        {
            string tel = getTelFromDisplayname(desc);
            if (tel == null) return null;
            tel = tel.Split(new char[] { ';' })[2].Replace("mobile:", "").Trim();
            return tel.Length == 0 ? null : tel;
        }

        public string getDisplaynameFromUsername(string user)
        {
            string ret = null;
            try
            {
                ret = USRDE.ContainsKey(user) ? USRDE[user].ToString() : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid user: " + user); }
            return ret;
        }

        public string getDomainuserFromUsername(string user)
        {
            string ret = null;
            try
            {
                ret = USRDU.ContainsKey(user) ? USRDU[user].ToString() : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid user: " + user); }
            return ret;
        }

        public string getWinntFromUsername(string user)
        {
            string ret = null;
            try
            {
                ret = USRNT.ContainsKey(user) ? USRNT[user].ToString() : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid user: " + user); }
            return ret;
        }

        public DirectoryEntry getDirectoryentryFromUsername(string user)
        {
            DirectoryEntry ret = null;
            try
            {
                ret = USREN.ContainsKey(user) ? (DirectoryEntry)USREN[user] : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid user: " + user); }
            return ret;
        }

        public string getCostcenterFromUsername(string user)
        {
            string ret = null;
            try
            {
            ret = USRCC.ContainsKey(user) ? USRCC[user].ToString() : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid user: " + user); }
            return ret;
        }

        public string getAddressFromUsername(string user)
        {
            string ret = null;
            try
            {
            ret = USRAD.ContainsKey(user) ? USRAD[user].ToString() : null;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid user: " + user); }
            return ret;
        }

        public bool getDecentralizedFromUsername(string user)
        {
            bool ret = false;
            try
            {
                ret = USRAD.ContainsKey(user) ? USRAD[user].ToString().StartsWith("DECENTRALIZED-") : false;
            }
            catch (Exception e) { Console.WriteLine("UsersInfo: not a valid user: " + user); }
            return ret;
        }

        public string RemoveDelocalizedUsers(string domainuserslist)
        {
            if (domainuserslist == null || domainuserslist.Length == 0) return "";
            List<string> list = new List<string>(domainuserslist.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            foreach (string domainuser in list)
            {
                //string domain = domainuser.Split('\\')[0].ToUpper();
                string user = domainuser.Split('\\')[1].ToUpper();
                if (getDecentralizedFromUsername(user))
                {
                    Console.WriteLine("Delocalized users removed from list: " + user);
                    list.Remove(domainuser);
                }
            }
            return list.Count == 0 ? "" : list.Aggregate((x, y) => x + ", " + y);
        }


        public static DirectorySearcher getUserSearcher()
        {
            DirectorySearcher USER_SEARCH = new DirectorySearcher();
            USER_SEARCH.Filter = "(&(ObjectClass=user)(!ObjectClass=computer)(employeeID=*))";
            USER_SEARCH.SearchScope = SearchScope.Subtree;
            USER_SEARCH.PropertiesToLoad.Add("sAMAccountName");
            USER_SEARCH.PropertiesToLoad.Add("displayName");
            USER_SEARCH.PropertiesToLoad.Add("userPrincipalName");
            USER_SEARCH.PropertiesToLoad.Add("division");
            USER_SEARCH.PropertiesToLoad.Add("physicalDeliveryOfficeName");
            USER_SEARCH.PropertiesToLoad.Add("msRTCSIP-PrimaryUserAddress");
            USER_SEARCH.PropertiesToLoad.Add("otherTelephone");
            USER_SEARCH.PropertiesToLoad.Add("mobile");
            USER_SEARCH.PropertiesToLoad.Add("telephoneNumber");
            USER_SEARCH.PropertiesToLoad.Add("physicalDeliveryOfficeName");
            return USER_SEARCH;
        }



        public UsersInfo()
        {
        }

        public void Clear()
        {
            USERS.Clear();
            USRDU.Clear();
            USRDE.Clear();
            USRNT.Clear();
            USREN.Clear();
            USRCC.Clear();
            USRAD.Clear();
            USRSP.Clear();
            USRTL.Clear();
        }

        public bool FindOneDomainUser(String domainuser)
        {
            DirectorySearcher USER_SEARCH = getUserSearcher();
            string domain = domainuser.Split('\\')[0].ToUpper();
            string user = domainuser.Split('\\')[1].ToUpper();

            if (USRDU.ContainsKey(user)) return true;

            bool found = false;
            foreach (Domain d in Forest.GetCurrentForest().Domains)
                if (d.GetDirectoryEntry().Properties["name"].Value.ToString().ToUpper().Equals(domain))
                {
                    USER_SEARCH.SearchRoot = d.GetDirectoryEntry();
                    found = true;
                    break;
                }
            if (!found) return false;

            //string filter_save = USER_SEARCH.Filter;
            USER_SEARCH.Filter = "(&(ObjectClass=user)(!ObjectClass=computer)(employeeID=*)(sAMAccountName=" + user + "))";
            SearchResult r = USER_SEARCH.FindOne();
            //USER_SEARCH.Filter = filter_save;
            if (r == null) return false;
            AddUser(r);
            return true;
        }

        public bool FindOneUser(String user)
        {
            if (USRDU.ContainsKey(user)) return true;
            DirectorySearcher USER_SEARCH = getUserSearcher();
            //string filter_save = USER_SEARCH.Filter;
            foreach (Domain d in Forest.GetCurrentForest().Domains)
            {
                USER_SEARCH.SearchRoot = d.GetDirectoryEntry();
                USER_SEARCH.Filter = "(&(ObjectClass=user)(!ObjectClass=computer)(employeeID=*)(sAMAccountName=" + user + "))";
                SearchResult r = USER_SEARCH.FindOne();
                if (r != null) { AddUser(r); return true; }
            }
            //USER_SEARCH.Filter = filter_save;
            return false;
        }


        static public bool isValidDisplayName(string desc)
        {
            foreach(char c in desc)
                if (!(Char.IsLetter(c) || c == ',' || c == ' ' || c == '-' || c == '\'')) return false;
            return true;
        }

        static public string getDomainuser(DirectoryEntry r)
        {
            string user = r.Properties["sAMAccountName"].Count > 0 ?
                      r.Properties["sAMAccountName"][0].ToString().ToUpper() : "";
            string princ = r.Properties["userPrincipalName"].Count > 0 ?
                r.Properties["userPrincipalName"][0].ToString().Trim() : "";
            string domain = princ.Split(new[] { '@', '.' }, System.StringSplitOptions.None).Count() > 1 ?
                princ.Split(new[] { '@', '.' }, System.StringSplitOptions.None)[1].ToUpper() : "";
            string domainuser = string.Format("{0}\\{1}", domain, user);
            return domainuser;
        }



        static public string getDomainuser(String user)
        {
            DirectorySearcher USER_SEARCH = getUserSearcher();
            return getDomainuser(user, USER_SEARCH);
        }

        static public string getDomainuser(String user, DirectorySearcher USER_SEARCH)
        {
            //USER_SEARCH.PropertiesToLoad.Add("sAMAccountName");
            USER_SEARCH.Filter =
                //"(&(ObjectClass=user)(!ObjectClass=computer)(employeeID=*)(sAMAccountName=" + user + "))";
                "(&(ObjectClass=user)(!ObjectClass=computer)(|(sAMAccountName=" + user.Replace(' ', '.') + ")"
                + "(mail=" + (user.IndexOf('@') < 0 ? user : user.Substring(0, user.IndexOf('@'))).Replace(' ', '.') + "@*)"
                + "(name=" + user.Replace(" ", ", ") + ")))";
            if (user == null) return null;
            foreach (Domain d in Forest.GetCurrentForest().Domains)
            {
                USER_SEARCH.SearchRoot = d.GetDirectoryEntry();
                SearchResult r = USER_SEARCH.FindOne();
                if (r != null) return (d.GetDirectoryEntry().Properties["name"].Value.ToString().ToUpper())
                    + "\\" + (r.Properties["sAMAccountName"].Count > 0 ?
                      r.Properties["sAMAccountName"][0].ToString().ToUpper() : "");
            }
            //Console.WriteLine(USER_SEARCH.Filter);
            return null;
        }




        public bool FindOneDesc(String desc)
        {
            DirectorySearcher USER_SEARCH = getUserSearcher();
            if (!isValidDisplayName(desc))
            {
                Console.WriteLine("User name   of '" + desc + "': Invalid display name.");
                return false;
            }
            if (desc == null) return false;
            if (USERS.ContainsKey(desc)) return true;
            desc = desc.Trim();
            if (desc.Length == 0) return false;
            foreach (Domain d in Forest.GetCurrentForest().Domains)
            {
                string filter_save = USER_SEARCH.Filter;
                USER_SEARCH.SearchRoot = d.GetDirectoryEntry();
                USER_SEARCH.Filter = "(&(ObjectClass=user)(!ObjectClass=computer)(employeeID=*)(displayName=" + desc + "))";
                SearchResult r = USER_SEARCH.FindOne();
                USER_SEARCH.Filter = filter_save;
                if (r != null)
                {
                    string u = AddUser(r);
                    //Console.WriteLine("UsersInfo.FindOneDesc " + desc + " (" + u + ")");
                    
                    return true;
                }
            }
            Console.WriteLine("User name   of '" + desc + "': Not found.");
            return false;
        }

        public void FindAll(DirectoryEntry de)
        {
            DirectorySearcher USER_SEARCH = getUserSearcher();
            USER_SEARCH.SearchRoot = de;
            foreach (SearchResult r in USER_SEARCH.FindAll()) AddUser(r);
        }

        string AddUser(SearchResult r)
        {
            try
            {
                string user = r.Properties["sAMAccountName"].Count > 0 ?
                    r.Properties["sAMAccountName"][0].ToString().ToUpper() : "";
                string desc = r.Properties["displayName"].Count > 0 ?
                    r.Properties["displayName"][0].ToString().Trim() : "";
                string princ = r.Properties["userPrincipalName"].Count > 0 ?
                    r.Properties["userPrincipalName"][0].ToString().Trim() : "";
                string domain = princ.Split(new[] { '@', '.' }, System.StringSplitOptions.None).Count() > 1 ?
                    princ.Split(new[] { '@', '.' }, System.StringSplitOptions.None)[1].ToUpper() : "";
                string winnt = string.Format("WinNT://{0}/{1},user", domain, user);
                string domainuser = string.Format("{0}\\{1}", domain, user);
                string costcenter = r.Properties["division"].Count > 0 ?
                    r.Properties["division"][0].ToString().Trim() : "";
                string address = r.Properties["physicalDeliveryOfficeName"].Count > 0 ?
                    r.Properties["physicalDeliveryOfficeName"][0].ToString().Trim() : "";
                string sip = r.Properties["msRTCSIP-PrimaryUserAddress"].Count > 0 ?
                    r.Properties["msRTCSIP-PrimaryUserAddress"][0].ToString().Trim() : "";

                string ext = r.Properties["otherTelephone"].Count > 0 ?
                    r.Properties["otherTelephone"][0].ToString().Trim().Replace("#", "").Replace("-", "").Replace(" ", "") : "";
                string cel = r.Properties["mobile"].Count > 0 ?
                    r.Properties["mobile"][0].ToString().Trim().Replace("#", "").Replace("-", "").Replace(" ", "") : "";
                string tel = r.Properties["telephoneNumber"].Count > 0 ?
                    r.Properties["telephoneNumber"][0].ToString().Trim().Replace("#", "").Replace("-", "").Replace(" ", "") : "";


                if (user.Length > 0 && desc.Length > 0)
                lock (USERS) {
                    USERS.Remove(desc); USERS.Add(desc, user);
                    USRDU.Remove(user); USRDU.Add(user, domainuser);
                    USRDE.Remove(user); USRDE.Add(user, desc);
                    USRNT.Remove(user); USRNT.Add(user, winnt);
                    USREN.Remove(user); USREN.Add(user, r.GetDirectoryEntry());
                    USRCC.Remove(user); USRCC.Add(user, costcenter);
                    USRAD.Remove(user); USRAD.Add(user, address);
                    USRSP.Remove(desc); USRSP.Add(desc, sip);
                    USRTL.Remove(desc); USRTL.Add(desc, "extension:" + ext + "; office:" + tel + "; mobile:" + cel);
                }
                return user;
            }
            catch (Exception e)
            {
                Console.WriteLine("UserInfo.FindAll Error: " + e.ToString());
                return null;
            }
        }

        public string[] getDomainUsers()
        {
            string[] result = new string[USRDU.Count];
            USRDU.Values.CopyTo(result, 0);
            return result;
        }

        public string[] getUsers()
        {
            string[] result = new string[USRDU.Count];
            USRDE.Values.CopyTo(result, 0);
            return result;
        }

        public string[] getLockedUsers()
        {
            Console.WriteLine("Looking for locked accounts in a set of " + USERS.Count + "...");
            List<string> result = new List<string>();
            foreach (string user in USERS.Values)
                if (!isUnlockedAD(getDirectoryentryFromUsername(user)))
                {
                    Console.WriteLine(user + " locked!");
                    result.Add(user);
                }
                else
                {
                    //Console.WriteLine(user + " not locked.");
                }
            return result.ToArray();
        }

        public object[][] getExpiringUsers(int maxdays, int alarmdays)
        {
            Console.WriteLine("Looking for expiring accounts in a set of " + USERS.Count + "...");
            List<object[]> result = new List<object[]>();
            foreach (string user in USERS.Values)
            {
                /*int days = daysExpiringAD(getDirectoryentryFromUsername(user));
                if (days < 10)
                {
                    Console.WriteLine(user + " expiring in " + days + " day" + (days > 1 ? "s" : "") + "!");
                    result.Add(new object[] { user, days });
                }*/
                int days = daysToExpiration(getDirectoryentryFromUsername(user), maxdays);
                //Console.WriteLine(user + " expire days " + days);

                if (days < alarmdays && days >=0) result.Add(new object[] { user, days });
            }
            return result.ToArray();
        }

        public bool unlockAD(string user)
        {
            return unlockAD(getDirectoryentryFromUsername(user));
        }

        //const int UAC_SCRIPT = 0x0001;
        const int UAC_ACCOUNTDISABLE = 0x0002;
        const int UAC_ACCOUNTLOCKED = 0x0010;
        
        const int UAC_PASSWORDCANTCHANGE = 0x0040;
        const int UAC_DONTEXPIREPASSWORD = 0x10000;

        const int UAC_PASSWORDEXPIRED = 0x800000;
        const int UAC_PARTIALSECRETSACCOUNT = 0x4000000;
        const int UAC_USEAESKEYS = 0x8000000;


        //const int UAC_NORMAL_ACCOUNT = 0x0200;
        //const int UAC_HOMEDIR_REQUIRED = 0x0008;
        //const int UAC_PASSWD_NOTREQD = 0x0020;
        //const int UAC_TEMP_DUPLICATE_ACCOUNT = 0x0100;



        public static bool enableAD(DirectoryEntry de, bool enable)
        {
            try
            {
                if (isEnabledAD(de) != enable)
                {
                    int UAC = (int)de.Properties["userAccountControl"].Value;
                    int updated = UAC ^ UAC_ACCOUNTDISABLE;
                    de.Properties["userAccountControl"].Value = updated;
                    de.CommitChanges();
                    Console.WriteLine(de.Properties["cn"][0].ToString() + " change disabled>" + (!enable));
                    return true;
                }
                else return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(de.Properties["cn"][0].ToString() + " change disabled>" + (!enable) + ": error: " + e.Message);
                return false;
            }
        }


        public static bool enableUserAD(DirectoryEntry de, bool enable)
        {
            try
            {
                UserPrincipal u = UserPrincipal.FindByIdentity(new PrincipalContext(ContextType.Domain),
                    IdentityType.DistinguishedName, "" + de.Properties["distinguishedName"][0]);
                if (u.Enabled != enable)
                {
                    u.Enabled = enable;
                    u.Save();
                    Console.WriteLine(de.Properties["cn"][0].ToString() + " change disabled>" + (!enable));
                    return true;
                }
                else return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(de.Properties["cn"][0].ToString() + " change disabled>" + (!enable) + ": error: " + e.Message);
                return false;
            }
        }
 





        public static bool isEnabledAD(DirectoryEntry de)
        {
            if (de == null)
            {
                Console.WriteLine("isEnabledAD was passed null de");
                return false;
            }
            //try
            //{
                int UAC = (int)de.Properties["userAccountControl"].Value;
                bool disabled = (UAC & UAC_ACCOUNTDISABLE) > 0;
                //Console.WriteLine(de.Properties["cn"][0].ToString() + " userAccountControl=" + UAC + " disabled=" + disabled);
                return !disabled;
            /*}
            catch (Exception e)
            {
                Console.WriteLine("isEnabledAD was passed invalid de: " + e.Message);
                return false;
            }*/
        }


        public static bool unlockAD(DirectoryEntry de)
        {
            if (!isUnlockedAD(de))
            {
                de.Properties["LockOutTime"].Value = 0;
                de.CommitChanges();
                //Console.WriteLine(de.Properties["cn"][0].ToString() + " change lockouttime>0");
                return isUnlockedAD(de);
            }
            else return true;
        }

        const string UAC_COMPUTED = "msDS-User-Account-Control-Computed";
        const string UAC_ = "userAccountControl";

        public static bool isUnlockedAD(DirectoryEntry de)
        {
            if (de == null)
            {
                Console.WriteLine("isUnlockedAD was passed null de");
                return false;
            }
            //solution 1 - inaccurate for LDAP
            //int UAC = (int)de.Properties["userAccountControl"].Value;

            //solution 2
            try
            {
                de.RefreshCache(new string[] { UAC_COMPUTED });
                int UAC = (int)de.Properties[UAC_COMPUTED].Value;

                bool locked = (UAC & UAC_ACCOUNTLOCKED) > 0;

                //Console.WriteLine(de.Properties["cn"][0].ToString() + " userAccountControl=" + UAC + " locked=" + locked);
                return !locked;
            }
            catch (Exception e)
            {
                Console.WriteLine("isUnlockedAD?"+de.ToString() + "=error:" + e.ToString());
                return false;
            }
        }



        public static bool resetPassword(DirectoryEntry de)
        {
            /*
             * using (var context = new PrincipalContext(ContextType.Domain))
            {
                using (var user = UserPrincipal.FindByIdentity(context, IdentityType.SamAccountName, userName))
                {
                    user.SetPassword("newpassword");
                    // or
                    user.ChangePassword("oldPassword", "newpassword");
                }
            }
            */
            string newPassword = "Welcome1!";
            if (DialogResult.OK == AuditSec.pwInputBox("Reset user " + de.Name + " password", "New password", ref newPassword)
                && newPassword.Length > 0)
                try
                {
                    de.Invoke("SetPassword", new object[] { newPassword });
                    de.Properties["LockOutTime"].Value = 0; //unlock account
                    de.Close();
                    return true;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Reset user " + de.Name + " password: Exception: " + e.Message);
                    return false;
                }
            else return true;

        }


        public static bool isExpiringSoonAD(DirectoryEntry de, int maxdays)
        {
            int days = daysToExpiration(de, maxdays);
            return days < 10 && days >= 0;
        }

        public static bool isExpiredAD(DirectoryEntry de)
        {
            if (de == null)
            {
                Console.WriteLine("isExpiringAD was passed null de");
                return true;
            }
            try
            {
                de.RefreshCache(new string[] { UAC_COMPUTED });
                int UAC = (int)de.Properties[UAC_COMPUTED].Value;

                bool expired = (UAC & UAC_PASSWORDEXPIRED) > 0;

                //Console.WriteLine(de.Properties["cn"][0].ToString() + " userAccountControl=" + UAC + " expired=" + expired);
                return expired;
            }
            catch (Exception e)
            {
                Console.WriteLine("isExpiringAD?" + de.ToString() + "=error:" + e.ToString());
                return true;
            }
        }

        
        private static long ConvertLargeIntegerToLong(object largeInteger)
        {
            Type type = largeInteger.GetType();

            int highPart = (int)type.InvokeMember("HighPart", System.Reflection.BindingFlags.GetProperty, null, largeInteger, null);
            int lowPart = (int)type.InvokeMember("LowPart", System.Reflection.BindingFlags.GetProperty | System.Reflection.BindingFlags.Public, null, largeInteger, null);

            return (long)highPart <<32 | (uint)lowPart;
        }
        

        public static int daysExpiringAD__(DirectoryEntry de)
        {
            if (de == null)
            {
                Console.WriteLine("isExpiringAD was passed null de");
                return 0;
            }
            try
            {
                de.RefreshCache(new string[] { UAC_COMPUTED });
                int UAC = (int)de.Properties[UAC_COMPUTED].Value;
                bool expired = (UAC & UAC_PASSWORDEXPIRED) > 0;
                if (expired) return 0;

                string user = de.Properties["sAMAccountName"].Count > 0 ?
                    de.Properties["sAMAccountName"][0].ToString().ToUpper() : "";
                string princ = de.Properties["userPrincipalName"].Count > 0 ?
                    de.Properties["userPrincipalName"][0].ToString().Trim() : "";
                string domain = princ.Split(new[] { '@', '.' }, System.StringSplitOptions.None).Count() > 1 ?
                    princ.Split(new[] { '@', '.' }, System.StringSplitOptions.None)[1].ToUpper() : "";

                int ret = -1;
                PrincipalContext context = new PrincipalContext(ContextType.Domain);
                UserPrincipal p = UserPrincipal.FindByIdentity(context, domain + "\\" + user);
                if (p.AccountExpirationDate.HasValue) ret = DateTime.Now.Subtract(p.AccountExpirationDate.Value.ToLocalTime()).Days;
                Console.WriteLine(de.Properties["cn"][0].ToString() + " userAccountControl=" + UAC + " expiring days=" + ret);
                return ret;

                /*
                int ret = -1;
                de.RefreshCache(new string[] { "accountExpires" });
                System.DirectoryServices.SearchResult sr = new DirectorySearcher(
                    de, String.Format("({0}=*)", "accountExpires"), new string[] { "accountExpires" }, SearchScope.Base).FindOne();
                if (sr != null && sr.Properties.Contains("accountExpires"))
                {
                        long AE = (Int64)sr.Properties["accountExpires"][0];
                        if (AE == 0 || AE == 0x7FFFFFFFFFFFFFFF) return -1;
                        ret = DateTime.UtcNow.Subtract(DateTime.FromFileTime(AE)).Days;
                }
                Console.WriteLine(de.Properties["cn"][0].ToString() + " userAccountControl=" + UAC + " expiring days=" + ret);
                return ret;
                */


                //long accountExpires = ConvertLargeIntegerToLong(DirectoryEntryHelper.GetAdObjectProperty(de, "accountExpires"));
                //de.RefreshCache(new string[] { "accountExpires" });
                //long accountExpires = ConvertLargeIntegerToLong(de.Properties["accountExpires"].Value);
                //if (accountExpires == long.MaxValue || accountExpires <= 0 || DateTime.MaxValue.ToFileTime() <= accountExpires) return 0;
                //else return DateTime.UtcNow.Subtract(DateTime.FromFileTimeUtc(accountExpires)).Days;
            }
            catch (Exception e)
            {
                Console.WriteLine("daysExpiringAD?" + de.ToString() + "=error:" + e.ToString());
                return 0;
            }
        }


        public static int daysToExpiration(DirectoryEntry de, int maxdays)
        {
            if (de == null)
            {
                Console.WriteLine("isExpiringAD was passed null de");
                return 0;
            }
            try
            {
                de.RefreshCache(new string[] { UAC_ });
                int UAC = (int)de.Properties[UAC_].Value;
                bool disabled = (UAC & UAC_ACCOUNTDISABLE) > 0;
                bool expired = (UAC & UAC_PASSWORDEXPIRED) > 0;
                bool neverexpire = (UAC & UAC_DONTEXPIREPASSWORD) > 0;
                if (disabled || neverexpire) return -1;
                if (expired) return 0;


                int pwdays = 0;
                long pwdLastSet = ConvertLargeIntegerToLong(de.Properties["pwdLastSet"].Value);
                if (pwdLastSet != long.MaxValue && pwdLastSet > 0 && DateTime.MaxValue.ToFileTime() > pwdLastSet)
                    pwdays = DateTime.UtcNow.Subtract(DateTime.FromFileTimeUtc(pwdLastSet)).Days;
                if (pwdays < 0) return -1;
                int delta = maxdays - pwdays;
                if (delta < 0) delta = 0;
                return delta;
            }
            catch (Exception e)
            {
                Console.WriteLine("daysExpiringAD?" + de.ToString() + "=error:" + e.ToString());
                return 0;
            }
        }



        




        static public Image getUserPicture(String username)
        {
            Image i = null; bool again = true; string lasterror = null; while (again)
            {
                again = false;
                AuditSec.checkDIRXMLAccess(lasterror);
                if (!AuditSec.picdisabled && AuditSec.settings.picpw != null) try
                    {
                        Console.WriteLine("Retrieving picture of " + username + "...");
                        try
                        {
                            DirectorySearcher s = new DirectorySearcher(
                                new DirectoryEntry(AuditSec.defaultLdap,
                                    "cn=" + UserPrincipal.Current.SamAccountName + ",ou=USER,o=MYCOMPANY", AuditSec.settings.picpw, AuthenticationTypes.None),
                                "(&(objectClass=MYCOMPANYUser)(cn=" + username + "))",
                                new[] { "photo" }, SearchScope.OneLevel
                            );

                            i = Image.FromStream(new MemoryStream((byte[])s.FindOne().Properties["photo"][0]));
                            if (i != null) return Resize(i, 90, 120);
                            //Console.WriteLine("Picture of " + username + ": " + i.ToString());
                        }
                        catch (AccessViolationException ave) { throw new Exception(ave.Message); }
                    }
                    catch (Exception e)
                    {
                        lasterror = e.Message;
                        Console.WriteLine("Picture of " + username + ": Error: " + e.Message);
                        if (e.Message.StartsWith("Logon failure")
                            || e.Message.EndsWith("A constraint violation occurred.")
                            || e.Message.StartsWith("The server is unwilling to process the request"))
                        {
                            lasterror = "Invalid password.";
                            AuditSec.settings.picpw = null;
                            again = true;
                        } 
                    }
            } return i;
        }


        static public Image Resize(Image img, int resizedW, int resizedH)
        {
            if (img == null) return null;
            int originalW = img.Width;
            int originalH = img.Height;
            Bitmap bmp = new Bitmap(resizedW, resizedH);
            Graphics graphic = Graphics.FromImage((Image)bmp);
            graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphic.DrawImage(img, 0, 0, resizedW, resizedH);
            graphic.Dispose();
            return (Image)bmp;
        }

        static public object[][] DIRXMLmapping = new object[][] {
                          //attr                         alias               type
            new object[] {"fullName",                    "FullName",         typeof(string)},
            new object[] {"MYCOMPANYlastname",                 "LastName",         typeof(string)},
            new object[] {"MYCOMPANYfirstname",                "FirstName",        typeof(string)},
            new object[] {"MYCOMPANYKanjiName",                "KanjiName",        typeof(string)},
            new object[] {"MYCOMPANYPersonPhoneURI",           "PhoneNumber",      typeof(string)},
            new object[] {"MYCOMPANYEDMSTitle",                "Title",            typeof(string)},//title
            new object[] {"workforceID",                 "PersonnelID",      typeof(string)},
            new object[] {"MYCOMPANYManagerFullName",          "ManagerName",      typeof(string)},
            new object[] {"departmentnumber",            "DptNumber",        typeof(string)},
            new object[] {"MYCOMPANYcompanycode",              "CompanyCode",      typeof(string)},
            new object[] {"MYCOMPANYSBU",                      "SBU",              typeof(string)},
            new object[] {"MYCOMPANYoperatingdescription",     "OperatingUnit",    typeof(string)},
            new object[] {"MYCOMPANYregion",                   "Region",           typeof(string)},
            new object[] {"MYCOMPANYCountrySys",               "Country",          typeof(string)},     
            new object[] {"physicalDeliveryOfficeName",  "Office",           typeof(string)},
            new object[] {"MYCOMPANYaddraddressline1",         "Address",          typeof(string)},
            new object[] {"MYCOMPANYLMSLocation",              "Location",         typeof(string)},//MYCOMPANYaddrcity
            new object[] {"MYCOMPANYLMSLocation@2",            "Decentralized",    typeof(string)},
            new object[] {"mail",                        "Email",            typeof(string)},

            new object[] {"employeeStatus",              "EmployeeStatus",   typeof(string)},
            new object[] {"employeeType",                "EmployeeType",     typeof(string)}
        };

        public static string[] DIRXMLattr = new List<object[]>(DIRXMLmapping).Select(o => (string)o[0]).ToArray();
        public static string[] DIRXMLattr2 = new List<object[]>(DIRXMLmapping).Select(o =>
            ((string)o[0]).IndexOf('@') >= 0 ? ((string)o[0]).Substring(0, ((string)o[0]).IndexOf('@')) : (string)o[0]
        ).ToArray();
        public static string[] DIRXMLalias = new List<object[]>(DIRXMLmapping).Select(o => (string)o[1]).ToArray();

        static public Type getDIRXMLtype(string attr)
        {
            foreach (object[] o in DIRXMLmapping) if (attr.Equals(o[0])) return (Type)o[2];
            return null; //default
        }

        static public string getDIRXMLattr(string alias)
        {
            foreach (object[] o in DIRXMLmapping) if (alias.Equals(o[1])) return (string)o[0];
            return null; //default
        }

        static public string getDIRXMLalias(string attr)
        {
            foreach (object[] o in DIRXMLmapping) if (attr.Equals(o[0])) return (string)o[1];
            return null; //default
        }

        public static object getValue(Type type, string value)
        {
            if (type.Equals(typeof(string))) return value;
            if (type.Equals(typeof(sbyte))) {sbyte ret = 0; sbyte.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(byte))) {byte ret = 0; byte.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(short))) {short ret = 0; short.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(ushort))) {ushort ret = 0; ushort.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(int))) {int ret = 0; int.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(uint))) {uint ret = 0; uint.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(long))) {long ret = 0; long.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(ulong))) {ulong ret = 0; ulong.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(float))) {float ret = 0; float.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(double))) {double ret = 0; double.TryParse(value, out ret); return ret;}
            if (type.Equals(typeof(decimal))) {decimal ret = 0; decimal.TryParse(value, out ret); return ret; }
            if (type.Equals(typeof(DateTime))) {DateTime ret = DateTime.MinValue; DateTime.TryParse(value, out ret); return ret;}
            else return value == null ? "" : value;
        }



        static public Hashtable getDIRXMLAttributes(String username)
        {
            int maxtry = 10;
            int retrydelay = 500;
            Hashtable h = null; bool again = true; int trycount = 0; string lasterror = null; while (again && trycount <= maxtry)
            {
                trycount++;
                again = false;
                AuditSec.checkDIRXMLAccess(lasterror);
                if (!AuditSec.picdisabled && AuditSec.settings.picpw != null) try
                    {
                        //Console.WriteLine("Retrieving DirXML data of " + username + "...");
                        try
                        {
                            DirectorySearcher s = new DirectorySearcher(
                                new DirectoryEntry(AuditSec.defaultLdap,
                                    "cn=" + UserPrincipal.Current.SamAccountName + ",ou=USER,o=MYCOMPANY",
                                    AuditSec.settings.picpw, AuthenticationTypes.None),
                                "(&(objectClass=MYCOMPANYUser)(cn=" + username + "))",
                                DIRXMLattr2, SearchScope.OneLevel
                            );
                            SearchResult result = s.FindOne();
                            if (result == null)
                            {
                                Console.WriteLine("DirXML data of " + username + ": Error: Not found.");
                            }
                            ResultPropertyCollection p = result.Properties;
                            h = new Hashtable();
                            for (int i = 0; i < DIRXMLattr.Length; i++)
                            {
                                string attr = DIRXMLattr[i];
                                string attr2 = DIRXMLattr2[i];
                                Type type = getDIRXMLtype(attr);
                                //Console.WriteLine("Retrieving DirXML data of " + username + "/" + attribute + "...");
                                string value = p[attr2].Count > 0 ? p[attr2][0].ToString() : "";
                                if (getDIRXMLalias(attr).Equals("Decentralized"))
                                {
                                    value = value.ToLower().Contains("decentralized") ? "Home-based" : "Office-based";
                                }
                                h.Add(attr, getValue(type, value));
                            }
                            //Console.WriteLine("DirXML data of " + username + ": " + h.ToString());
                        }
                        catch (AccessViolationException ave) { throw new Exception(ave.Message); }
                    }
                    catch (Exception e)
                    {
                        lasterror = e.Message;
                        if (e.Message.StartsWith("Object reference not set to an instance of an object"))
                            ;//not found. ok
                        else if (e.Message.StartsWith("A device attached to the system is not functioning"))
                        {
                            lasterror = e.Message;
                            Thread.Sleep(retrydelay);
                            again = true;
                        }
                        else
                            Console.WriteLine("DirXML data of " + username + ": " + e.Message);
                        if (e.Message.StartsWith("Logon failure")
                            || e.Message.EndsWith("A constraint violation occurred.")
                            || e.Message.StartsWith("The server is unwilling to process the request"))
                            {
                            lasterror = "Invalid password.";
                            AuditSec.settings.picpw = null;
                            again = true;
                        } 
                    }
                if (again && trycount > maxtry)
                    Console.WriteLine("DirXML data of " + username + ": Error: " + lasterror + "\nMaximum retry reached.");
            } return h;
        }




    }//eosc
}
