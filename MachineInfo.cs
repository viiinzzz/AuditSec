using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

using System.ServiceProcess;
using System.Management;
using System.DirectoryServices;
using System.Collections;
using System.Windows.Forms;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using smsclictr.automation;
using System.Drawing;
using System.DirectoryServices.ActiveDirectory;
using System.Net.NetworkInformation;
using System.Net;

namespace AuditSec
{


    public class MachineInfo
    {
        static public DirectorySearcher getSearcher()
        {
            return getSearcher(null);
        }
        static public DirectorySearcher getSearcher(string filter)
        {
            return getSearcher(filter, null);
        }

        static public DirectorySearcher getSearcher(string filter, DirectoryEntry root)
        {
            DirectorySearcher COMP_SEARCH;
            COMP_SEARCH = new DirectorySearcher();
            if (root != null)
                COMP_SEARCH.SearchRoot = root;
            if (filter == null)
                COMP_SEARCH.Filter = "(&ObjectCategory=computer)";
            else
                COMP_SEARCH.Filter = "(&(ObjectCategory=computer)(" + filter + "))";
            COMP_SEARCH.SearchScope = SearchScope.Subtree;
            COMP_SEARCH.PropertiesToLoad.Add("name");
            COMP_SEARCH.PropertiesToLoad.Add("description");
            COMP_SEARCH.PropertiesToLoad.Add("dNSHostName");
            COMP_SEARCH.PropertiesToLoad.Add("operatingSystem");
            COMP_SEARCH.PropertiesToLoad.Add("operatingSystemVersion");
            COMP_SEARCH.PropertiesToLoad.Add("operatingSystemServicePack");
            COMP_SEARCH.PropertiesToLoad.Add("whenCreated");
            COMP_SEARCH.PropertiesToLoad.Add("whenChanged");
            return COMP_SEARCH;
        }

        public static bool matches(string machine, string mask)
        {
            try
            {
                return machine != null && machine.Length > 0
                && System.Text.RegularExpressions.Regex.IsMatch(machine, mask,
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public string getPrimaryUser()
        {
            return getProperty("PrimaryUser").ToString();
        }

        public string getPrimaryFullName()
        {
            return getProperty("PrimaryFullName").ToString();
        }

        public static MachineInfo getMachine(string machine)
        {
            return getMachine(machine, null);
        }
        public static MachineInfo getMachine(string machine, UsersInfo usersInfo)
        {
            try
            {
                foreach (Domain d in Forest.GetCurrentForest().Domains)
                {
                    SearchResult r = getSearcher("cn=" + machine, d.GetDirectoryEntry()).FindOne();
                    if (r != null)
                    {
                        MachineInfo mi = new MachineInfo(r, usersInfo, true);
                        string primaryFullName = mi.getPrimaryFullName();
                        if (primaryFullName.Length > 0 && usersInfo != null) usersInfo.FindOneDesc(primaryFullName);
                        mi.calc(true);
                        return mi;
                    }
                }
                Console.WriteLine("getMachine " + machine + " not found.");
                return null;

            }
            catch (Exception e)
            {
                Console.WriteLine("getMachine " + machine + " error: " + e.ToString());
                return null;
            }
        }

        public void updatemachinede()
        {
            try
            {
                machinede = getSearcher("cn=" + machine).FindOne().GetDirectoryEntry();
                calc(true);
            }
            catch (Exception e)
            {
                Console.WriteLine("updatemachinede " + machine + " error: " + e.ToString()); 
            }
        }

        public void calc(bool force)
        {
            bool recalc = force || !calculated;
            if (!recalc) return;

            System.DirectoryServices.PropertyCollection P = machinede.Properties;
            //memoryexception
            description = P["description"].Count > 0 ? P["description"][0].ToString() : "";

            //Console.WriteLine("Recalc: Machine= " + machine + " - Description= " + description);

            os = P["operatingSystem"].Count > 0 ? P["operatingSystem"][0].ToString() : "";
            osver = P["operatingSystemVersion"].Count > 0 ? P["operatingSystemVersion"][0].ToString() : "";
            ospak = P["operatingSystemServicePack"].Count > 0 ? P["operatingSystemServicePack"][0].ToString() : "";


            {//user
                string desc = description.Replace("(SCCM)", "").Trim();
                string user = usersInfo == null ? null : usersInfo.getUsernameFromDisplayname(desc);
                domainuser = user == null ? null
                    : usersInfo == null ? null : usersInfo.getDomainuserFromUsername(user);
                userde = user == null ? null
                    : usersInfo == null ? null : usersInfo.getDirectoryentryFromUsername(user);
            }

            Workstations = null; try
            {
                Workstations = userde == null ? null
                    : userde.Parent.Parent.Children.Find("OU=Workstations", "organizationalUnit");
            }
            catch (Exception e) { }

            misplaced = userde != null && Workstations != null && !machinede.Parent.Path.Equals(Workstations.Path);

            from = machinede.Path;
            to = Workstations == null ? "" : Workstations.Path;
            from = Regex.Replace(from, @".*CN=", @"");
            from = Regex.Replace(from, @",OU=", @"\");
            from = Regex.Replace(from, @",DC=.*", @"");
            to = Regex.Replace(to, @".*/OU=", @"");
            to = Regex.Replace(to, @",OU=", @"\");
            to = Regex.Replace(to, @",DC=.*", @"");

            site = Regex.Replace(from.ToUpper(), @".*\\(.*)$", "$1");
            department = Regex.Replace(from.ToUpper(), @".*\\(.*)\\.*$", "$1");
            department = Regex.Replace(department, @".*\\.*$", "?");

            reversepath = string.Join(".", from.Split(new char[] { '\\' }).Reverse());
            ou = from;
            ou = Regex.Replace(ou, @"^[^\\]*\\", @"");
            ou = Regex.Replace(ou, @"Workstations\\", @"");
            ou = Regex.Replace(ou, @".*\\", @"");
            
            //recovery key
            StringBuilder rkey = new StringBuilder();
            try
            {
                DirectorySearcher ds = new DirectorySearcher(machinede, "(objectClass=msFVE-RecoveryInformation)",
                    new String[] { "msFVE-RecoveryPassword" }, SearchScope.OneLevel);
                foreach (SearchResult r2 in ds.FindAll())
                    rkey.AppendLine(r2.Properties["msFVE-RecoveryPassword"].Count > 0 ?
                        r2.Properties["msFVE-RecoveryPassword"][0].ToString()
                        : "");
            }
            catch (Exception e)
            {
            }
            recovery = rkey.ToString();

            //volume ID
            StringBuilder vid = new StringBuilder();
            try
            {
                DirectorySearcher ds = new DirectorySearcher(machinede, "(objectClass=msFVE-RecoveryInformation)",
                    new String[] { "msFVE-VolumeGuid" }, SearchScope.OneLevel);
                foreach (SearchResult r2 in ds.FindAll())
                    vid.AppendLine(r2.Properties["msFVE-VolumeGuid"].Count > 0 ?
                        BitConverter.ToString((Byte[])r2.Properties["msFVE-VolumeGuid"][0]).Replace("-", string.Empty)
                        : "");
            }
            catch (Exception e)
            {
            }
            volumeid = vid.ToString();

            /*Console.WriteLine(this.ToString());*/

            calculated = true;
        }


        public string ToString()
        {
            return "\n--------------------------------------------------------------------------------"
                + "\nMachine:\t\t\t" + machine
                + "\nDescription:\t\t" + description
                + "\nOperating System:\t" + os
                + "\nO.S Version:\t\t" + osver
                + "\nO.S Service Pack:\t" + ospak
                + "\nInstallation Date:\t" + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy}", installed)
                + "\nWorkstation Path:\t\"" + machinede.Path + "\""
                + "\nOwner Path:\t\t\t\"" + (userde != null ? userde.Path : "") + "\""
                + "\nPrefered O.U Path:\t\"" + (Workstations != null ? Workstations.Path : "") + "\""
                + "\nMisplaced:\t\t\t" + misplaced
                + (misplaced ? "\nSuggested Move From:\t" + from : "")
                + (misplaced ? "\nSuggested Move To:\t" + to : "")
                + "\nAD Bitlocker Recov.Key:\t" + recovery
                + "\nVolume ID:\t\t\t" + volumeid
                + "\n--------------------------------------------------------------------------------";
        }



        public Object getProperty(string property)
        {
            switch (property)
            {
                case "PrimaryFullName":
                    {
                        string desc = description.IndexOf('#') < 0 ? description : description.Substring(0, description.IndexOf('#'));
                        desc = desc.Replace("(SCCM)", "").Trim();
                        return desc;
                    }
                case "PrimaryUser":
                    {
                        string desc = description.IndexOf('#') < 0 ? description : description.Substring(0, description.IndexOf('#'));
                        desc = desc.Replace("(SCCM)", "").Trim();
                        string user = usersInfo == null ? null : usersInfo.getUsernameFromDisplayname(desc);
                        return user == null ? "" : user;
                    }
                case "OperatingSystem": return OSReplace(os);
                case "OSServer": return ServerReplace(os);
                case "OSVersion": return osver;
                case "OSServicePack": return SPReplace(ospak);
                case "InstallationDate": return installed;
                case "RecentBoot":
                    double age0 = DateTime.UtcNow.Subtract(changed).TotalDays;
                    //Console.WriteLine(String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy}", changed) + "   boot age " + (int)age0);
                    return age0 < 7 ? "RecentBoot" : age0 > 60 ? "LongtimeBoot" : "";
                case "RecentInstall":
                    double age1 = DateTime.UtcNow.Subtract(installed).TotalDays;
                    return age1 < 15 ? "RecentInstall"
                    : age1 > 365 * 2 ? "LongtimeInstall" : "";
                case "ADSPath": return from.Replace(machine + @"\", "");
                case "ADSSite": return site;
                case "ADSDepartment": return department;
                case "ADStatus": return from.ToLower().Contains(@"\orphaned") ? "Orphaned"
                                        : from.ToLower().Contains(@"!unknown") ? "UnknownOU" : "";
                case "Misplaced": return misplaced;
                case "CorrectPath": return to;
                case "CorrectSite": return getCorrectSite();
                case "CorrectDepartment": return getCorrectDepartment();
                case "BitlockerRecovKey": return recovery;
                case "BitlockerVolumeID": return volumeid;
                case "Encrypted": return recovery != null && recovery.Length > 0;

                case "ITOpsMembers": return ITOps == null || !ITOps.Contains(site) || ((List<string>)ITOps[site]).Count == 0 ? ""
                    : ((List<string>)ITOps[site]).Aggregate((x, y) => x + "; " + y);
                case "ITOpsManagement": return ITOpsMgmt == null || !ITOpsMgmt.Contains(site) || ((List<string>)ITOpsMgmt[site]).Count == 0 ? ""
                    : ((List<string>)ITOpsMgmt[site]).Aggregate((x, y) => x + "; " + y);
                default: return null;
            }
        }


        static Hashtable ITOps = null, ITOpsMgmt = null;
        static public void setITOps(Hashtable ITOps_, Hashtable ITOpsMgmt_)
        {
            ITOps = ITOps_;
            ITOpsMgmt = ITOpsMgmt_;
        }

        static public Type getPropertyType(string property)
        {
            switch (property)
            {
                case "PrimaryFullName": return typeof(string);
                case "PrimaryUser": return typeof(string);
                case "OperatingSystem": return typeof(string);
                case "OSServer": return typeof(string);
                case "OSVersion": return typeof(string);
                case "OSServicePack": return typeof(string);
                case "InstallationDate": return typeof(DateTime);
                case "RecentInstall": return typeof(string);
                case "RecentBoot": return typeof(string);
                case "ADSPath": return typeof(string);
                case "ADSSite": return typeof(string);
                case "ADSDepartment": return typeof(string);
                case "ADStatus": return typeof(string);
                case "Misplaced": return typeof(bool);
                case "CorrectPath": return typeof(string);
                case "CorrectSite": return typeof(string);
                case "CorrectDepartment": return typeof(string);
                case "BitlockerRecovKey": return typeof(string);
                case "Encrypted": return typeof(bool);
                case "ITOpsMembers": return typeof(string);
                case "ITOpsManagement": return typeof(string);
                default: return null;
            }
        }

        static public String[] getPropertyNames()
        {
            return new string[]{
                "PrimaryUser",
                "PrimaryFullName",
                "OperatingSystem",
                "OSServer",
                "RecentInstall",
                "RecentBoot",
                "ADStatus",
                "InstallationDate",
                "OSVersion",
                "OSServicePack",
                "ADSPath",
                "ADSSite",
                "ADSDepartment",
                "Misplaced",
                "CorrectPath",
                "CorrectSite",
                "CorrectDepartment",
                
                //Bitlocker not available anymore from AD
                //"BitlockerRecovKey",
                //"Encrypted",
                
                "ITOpsMembers",
                "ITOpsManagement"
            };
        }


        void replace()
        {
            Console.WriteLine("re-Place invoked on " + machine);
            if (!misplaced) try
                {
                    machinede.MoveTo(Workstations);
                    Workstations.Close();
                    machinede.Close();
                    machinede = new DirectorySearcher(Workstations,
                        "(&(ObjectCategory=computer)(cn=" + machine + "))",
                        null, SearchScope.OneLevel).FindOne().GetDirectoryEntry();
                }
                catch (Exception e)
                {
                    throw new Exception("Cannot move: " + from + " to-> " + to);
                }
            calc(true);
        }

        SearchResult r;
        UsersInfo usersInfo;
        bool calculated = false;

        public MachineInfo(SearchResult r, UsersInfo usersInfo, bool calcNow)
        {
            this.r = r;
            this.usersInfo = usersInfo;
            task = "";
            task_t0 = DateTime.MinValue;
            machinede = r.GetDirectoryEntry();
            machine = r.Properties["name"].Count > 0 ? r.Properties["name"][0].ToString().ToUpper() : "";
            description = r.Properties["description"].Count > 0 ? r.Properties["description"][0].ToString() : "";
            dNSHostName = r.Properties["dNSHostName"].Count > 0 ? r.Properties["dNSHostName"][0].ToString() : "";
            domain = dNSHostName.Split('.').Length > 1 ? dNSHostName.Split('.')[1].ToUpper() : "";

            try
            {
                installed = r.Properties["whenCreated"].Count > 0 ?
                    DateTime.ParseExact(r.Properties["whenCreated"][0].ToString(),
                    "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"), DateTimeStyles.None)
                    : new DateTime();
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }

            try
            {
                changed = r.Properties["whenChanged"].Count > 0 ?
                    DateTime.ParseExact(r.Properties["whenChanged"][0].ToString(),
                    "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"), DateTimeStyles.None)
                    : new DateTime();
            }
            catch (Exception e) { Console.WriteLine(e.ToString()); }

            if (calcNow) calc(true);

        }

        public string machine { get; set; }
        public string domain { get; set; }
        public string dNSHostName { get; set; }
        public string description { get; set; }
        public string os { get; set; }
        public string osver { get; set; }
        public string ospak { get; set; }
        public DirectoryEntry machinede { get; set; }
        public DirectoryEntry userde { get; set; }
        public string domainuser { get; set; }
        public DirectoryEntry Workstations { get; set; }
        public Boolean misplaced { get; set; }
        public string from { get; set; }
        public string reversepath { get; set; }
        public string site { get; set; }
        public string department { get; set; }
        public string ou { get; set; }
        public string to { get; set; }
        public DateTime installed { get; set; }
        public DateTime changed { get; set; }
        public string recovery { get; set; }
        public string volumeid { get; set; }


        public string getCorrectDepartment()
        {
            if (string.IsNullOrWhiteSpace(to)) return "";
            string[] s = to.Split(new char[] { '\\' }, StringSplitOptions.None).Reverse().ToArray();
            return s.Length > 1 ? s[1] : "";
        }

        public string getCorrectSite()
        {
            if (string.IsNullOrWhiteSpace(to)) return "";
            string[] s = to.Split(new char[] { '\\' }, StringSplitOptions.None).Reverse().ToArray();
            return s.Length > 0 ? s[0] : "";
        }

        public string task { get; set; }
        public DateTime task_t0 { get; set; }

        public bool dotask(string task) {
            this.task = task;
            task_t0 = DateTime.Now;
            return true;
        }
        public int getRuntime()
        {
            return Convert.ToInt32((DateTime.Now - task_t0).TotalSeconds);
        }
        public string getTask()
        {
            return machine + ":" + task + "=" + getRuntime() + "\"";
        }




        public static bool disconnectWMI(ManagementScope scope)
        {
            if (scope == null) return false;
            else try
                {
                    scope.Path = new ManagementPath();
                    return true;
                }
                catch (Exception e)
                {
                    return false;
                }

        }

        public static ManagementScope connectWMI(string targethost)
        {
            return connectWMI(targethost, null, null, null);
        }
        public static ManagementScope connectWMI(string targethost, string subscope)
        {
            return connectWMI(targethost, subscope, null, null);
        }

        public static ManagementScope connectWMI(string targethost, string targetusername, string targetpassword)
        {
            return connectWMI(targethost, null, targetusername, targetpassword);
        }
        public static ManagementScope connectWMI(string targethost, string subscope, string targetusername, string targetpassword)
        {
            ConnectionOptions Conn = new ConnectionOptions();

            //test
            if (subscope != null)
            {
                Conn.Impersonation = ImpersonationLevel.Impersonate;
                Conn.Authentication = AuthenticationLevel.PacketPrivacy;
            }

            if (targethost != Environment.MachineName)
            {
                Conn.Username = targetusername; //can be null
                Conn.Password = targetpassword; //can be null
            }
            Conn.Timeout = TimeSpan.FromSeconds(10);
            ManagementScope scope = new ManagementScope(@"\\" + targethost + @"\root\cimv2"
                + (subscope != null ? @"\" + subscope : ""),
                Conn);
            scope.Connect();
            return scope;
        }

        public static ManagementObjectCollection queryWMI(ManagementScope scope, string query)
        {
            return new ManagementObjectSearcher(scope, new ObjectQuery(query)).Get();
        }


        public static bool clearSeclogs(string targethost, string targetusername, string targetpassword)
        {
            Console.Write("Trying to clear Security logs on " + targethost + "...");
            bool result = false;
            ManagementScope scope = null; try
            {
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_NTEventLogFile WHERE LogFileName='Security'"))
                {
                    ManagementBaseObject inParams = o.GetMethodParameters("ClearEventLog");
                    ManagementBaseObject outParams = o.InvokeMethod("ClearEventLog", inParams, null);
                    o.Dispose();
                    if (0 != (int)(uint)(outParams.Properties["ReturnValue"].Value)) throw new Exception("Method ClearEventLog returned failed.");
                }
                Console.WriteLine("Successfully cleared Security Logs on " + targethost);
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to clear Security Logs on " + targethost + "\n" + e.ToString());
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }

        public static string getGroupMembers(string targethost, string groupname, string targetusername, string targetpassword,
             bool hidesecugrp, bool onlysecugrp, string secugrplist)
        {

            StringBuilder result = new StringBuilder();
            ManagementScope scope = null; try
            {
                string machine = targethost.Split('.')[0].ToUpper();
                List<string> starts = new List<string>(), equals = new List<string>(), ends = new List<string>();
                ends.Add("\\DOMAIN ADMINS");
                equals.Add(machine + "\\ADMINISTRATOR");

                foreach (string s in secugrplist.Split(','))
                {
                    string u = s.Trim().ToUpper();
                    if (u.EndsWith(@"\")) starts.Add(u);
                    else if (u.StartsWith(@"\")) ends.Add(u);
                    else if (u.Length > 0)
                    {
                        if (u.IndexOf(@"\") < 0)
                            equals.Add(machine + @"\" + u);
                        else
                            equals.Add(u);
                    }
                }

                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT PartComponent FROM Win32_GroupUser "+
                    "WHERE GroupComponent = \"Win32_Group.Domain='" + targethost + "',Name='" + groupname + "'\""))
                {
                    ManagementPath path = new ManagementPath(o["PartComponent"].ToString());

                    string[] names = path.RelativePath.Split(',');
                    string user = names[0].Substring(names[0].IndexOf("=") + 1).Replace("\"", " ").Trim()
                                    + "\\" + names[1].Substring(names[1].IndexOf("=") + 1).Replace("\"", " ").Trim();
                    bool secugrp = false;
                    foreach (string s in starts) if (user.ToUpper().StartsWith(s)) secugrp = true;
                    foreach (string s in equals) if (user.ToUpper().Equals(s)) secugrp = true;
                    foreach (string s in ends) if (user.ToUpper().EndsWith(s)) secugrp = true;

                    /*
                    bool secugrp = user.ToUpper().StartsWith("MYCOMPANY\\")
                        || user.ToUpper().EndsWith("\\DOMAIN ADMINS")
                        || user.ToUpper().Equals(machine + "\\ADMINISTRATOR")
                        || user.ToUpper().Equals(machine + "\\USERADMIN")
                        || user.ToUpper().EndsWith("\\ISHELPDESK")
                        || user.ToUpper().EndsWith("\\TEMP_ADMIN");
                    */
                    //if (!(secugrp && hidesecugrp)) result.AppendLine(user);
                    if (onlysecugrp)
                    {
                        if (secugrp)
                        {
                            if (!hidesecugrp) result.AppendLine(user);
                        }
                        else
                        {
                            //onlysecugrp
                        }
                    }
                    else
                    {
                        if (secugrp) {
                            if (!hidesecugrp) result.AppendLine(user);
                        }
                        else
                        {
                            result.AppendLine(user);
                        }
                    }
                }
                scope.Path = new ManagementPath();
            }
            catch (Exception e)
            {
                //Console.WriteLine("getGroupMembers " + targethost + " error: " + e.Message);
                //Console.WriteLine("Not available.");
                result = null;
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result == null ? null : result.ToString().TrimEnd('\n');
        }


        public static Boolean AddMembers(string computerName, string groupName, string members)
        {
            return AddMembers(computerName, groupName, members, null);
        }

        public static Boolean AddMembers(string computerName, string groupName, string members, Func<string, bool> status)
        {
            foreach (string u in members.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string user = u.Trim();
                if (user.Length > 0) if (!AddMember(computerName, groupName, user, status)) return false;
            }
            return true;
        }

        public static Boolean AddMember(string computerName, string groupName, string userPath, Func<string, bool> status)
        {
            try
            {
                DirectoryEntry computer = new DirectoryEntry("WinNT://" + computerName);
                DirectoryEntry group = computer.Children.Find(groupName, "group");
                group.Invoke("Add", new[] { userPath });
                group.CommitChanges(); group.Dispose();
                Console.WriteLine("Successfully added '" + userPath + "' to " + computerName + "\\Administrators");
                status("Changes committed successfully onto '" + computerName + "'.");
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED to add '" + userPath + "' to " + computerName + "\\Administrators"
                    + "\nError: " + e.ToString());
                status("Changes could not be committed onto '" + computerName + "'."
                + "\nError: " + e.ToString());
                return false;
            }
        }

        public static Boolean RemoveMembers(string computerName, string groupName, string members)
        {
            return RemoveMembers(computerName, groupName, members, null);
        }

        public static Boolean RemoveMembers(string computerName, string groupName, string members, Func<string, bool> status)
        {
            foreach (string u in members.Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                string user = u.Trim();
                if (user.Length > 0) if (!RemoveMember(computerName, groupName, user, status)) return false;
            }
            return true;
        }

        public static Boolean RemoveMember(string computerName, string groupName, string userName, Func<string, bool> status)
        {
            try
            {
                if (status != null) status("Trying to remove user '" + userName
                    + "' from the group '" + groupName
                    + "' on computer '" + computerName + "'...");
                DirectoryEntry computer = new DirectoryEntry("WinNT://" + computerName);
                DirectoryEntry group = computer.Children.Find(groupName, "group");
                foreach (object m in (IEnumerable)group.Invoke("Members")) using (DirectoryEntry member = new DirectoryEntry(m))
                    {
                        String user = member.Path.ToUpper().Replace("WINNT://", "").Replace("/", "\\");
                        if (user.Equals(userName.ToUpper()))
                        {
                            group.Invoke("Remove", new[] { member.Path });
                            group.CommitChanges(); group.Dispose();
                            Console.WriteLine("Successfully removed '" + member.Path + "' from " + computerName + "\\Administrators");
                            if (status != null) status("Changes committed successfully onto '" + computerName + "'.");
                            return true;
                        }
                    }
                
                Console.WriteLine("FAILED to remove '" + userName + "' from " + computerName + "\\Administrators"
                    + "\nError: User not found.");
                if (status != null) status("Changes could not be committed onto '" + computerName + "'."
                + "\nError: User not found.");
                return false;
            }
            catch (Exception e)
            {
                Console.WriteLine("FAILED to remove '" + userName + "' from " + computerName + "\\Administrators"
                    + "\nError: " + e.ToString());
                if (status != null) status("Changes could not be committed onto '" + computerName + "'."
                + "\nError: " + e.ToString());
                return false;
            }
        }


        public static IPAddress ping(string machine, bool show)
        {
            Ping ping = new Ping(); PingReply reply;
            if (show) Console.WriteLine("ping " + machine + "...");
            try
            {
                if ((reply = ping.Send(machine)).Status == IPStatus.Success)
                {
                    if (show) Console.WriteLine("ping " + machine + " : Reply from " + reply.Address);
                    return reply.Address;
                }
                else
                {
                    if (show) Console.WriteLine("ping " + machine + " : " + reply.Status);
                    return null;
                }
            }
            catch (Exception e)
            {
                if (show) Console.WriteLine("ping " + machine + " : " + e.Message);
                return null;
            }
        }

        public static string ListServices(string machineName, string serviceNameFilter)
        {
            try
            {

                StringBuilder ret = new StringBuilder();
                ServiceController[] services = ServiceController.GetServices(machineName);
                foreach (ServiceController service in services)
                {
                    bool match = true;
                    if (serviceNameFilter != null && serviceNameFilter.Length > 0) try
                        {
                            match = System.Text.RegularExpressions.Regex.IsMatch(service.ServiceName, serviceNameFilter,
                                     System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                                || System.Text.RegularExpressions.Regex.IsMatch(service.DisplayName, serviceNameFilter,
                                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine("ServiceName=" + service.DisplayName + " IsMatch Regex=" + serviceNameFilter + " = " + ee.Message);
                        }
                    if (match) ret.AppendLine(service.ServiceName + " : " + service.Status);
                }
                return ret.ToString();
            }
            catch (Exception e)
            {
                Console.WriteLine("ListServices on " + machineName + " failed: " + e.Message);
                return null;
            }
        }

        public static bool isServicePresent(string machineName, string serviceNameFilter)
        {
            string list = ListServices(machineName, serviceNameFilter);
            if (list == null) return false;
            else return list.Split(new Char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries).Length > 0;
        }

        public static bool isServiceRunning(string machineName, string serviceNameFilter)
        {
            return isServiceRunning(machineName, serviceNameFilter, true);
        }

        public static bool isServiceRunning(string machineName, string serviceNameFilter, bool show)
        {
            StringBuilder ret = new StringBuilder();
            try
            {
                ServiceController[] services = ServiceController.GetServices(machineName);
                foreach (ServiceController service in services)
                {
                    bool match = true;
                    if (serviceNameFilter != null && serviceNameFilter.Length > 0) try
                        {
                            match = System.Text.RegularExpressions.Regex.IsMatch(service.ServiceName, serviceNameFilter,
                                     System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine("ServiceName=" + service.ServiceName
                                + " IsMatch Regex=" + serviceNameFilter + " = " + ee.Message);
                        }
                    if (match)
                    {
                        bool running = ServiceControllerStatus.Running == service.Status;
                        if (show) Console.WriteLine(machineName + " - Status " + service.ServiceName + " : " + service.Status);
                        return running;
                    }
                }
                if (show) Console.WriteLine(machineName + " - Status " + serviceNameFilter + " : Not found");
                return false;
            }
            catch (Exception e)
            {
                if (show) Console.WriteLine(machineName + " - Status " + serviceNameFilter + " : Not available");
                return false;
            }
        }

        public static bool StartService(string machineName, string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName, machineName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                //Console.WriteLine(machineName + " - Starting " + serviceName + "...");
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool StopService(string machineName, string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName, machineName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);
                //Console.WriteLine("attemping to Stop Service '" + serviceName + "' on " + machineName + "...");
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool RestartService(string machineName, string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName, machineName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                Console.WriteLine("attemping to Restart Service '" + serviceName + "' on " + machineName + "...");
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
                return true;
            }
            catch
            {
                return false;
            }
        }










        public enum ChassisTypes
        {
            Other = 1, Unknown, Desktop, LowProfileDesktop, PizzaBox, MiniTower, Tower,
            Portable, Laptop, Notebook, Handheld, DockingStation, AllInOne, SubNotebook, SpaceSaving, LunchBox,
            MainSystemChassis, ExpansionChassis, SubChassis, BusExpansionChassis, PeripheralChassis, StorageChassis, RackMountChassis,
            SealedCasePC
        }

        public static bool isLaptop(string targethost)
        {
            ChassisTypes v = getChassisType(targethost);
            return v >= ChassisTypes.Portable && v <= ChassisTypes.SubNotebook;
        }

        public static ChassisTypes getChassisType(string targethost)
        {
            //Console.Write("Trying to get Chassis Type of " + targethost + "...");
            ChassisTypes result = ChassisTypes.Unknown;
            ManagementScope scope = null; try
            {
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT ChassisTypes FROM Win32_SystemEnclosure"))
                {
                    foreach (ushort i in o.Properties["ChassisTypes"].Value as ushort[])
                    {
                        if (i > 0 && i < 25) return (ChassisTypes)i;
                    }
                    break;
                }
                result = ChassisTypes.Unknown;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Failed to get Chassis Type of " + targethost + "\n" + e.Message);
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }

        /*
         Win32_SystemEnclosure, ChassisTypes(1)=10
         Win32_Battery or Win32_PortableBattery
         Win32_PCMCIAController
         */

        public static string getMakerModel(string targethost)
        {
            //Console.Write("Trying to get Maker/Model of " + targethost + "...");
            string result = null;
            ManagementScope scope = null; try
            {
                string maker = "", model = "";
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_ComputerSystem"))
                {
                    maker = o["Manufacturer"].ToString();
                    model = o["Model"].ToString();
                    break;
                }
                maker = computerReplace("Model", model);
                model = computerReplace("Model", model);
               
                result = model.Length > 0 ? model : maker;
            }
            catch (Exception e)
            {
                //Console.WriteLine("Failed to get Chassis Type of " + targethost + "\n" + e.Message);
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }


        public static string getProtectionStatus(string targethost)
        {
            //Console.Write("Trying to get Protection Status of " + targethost + "...");
            string result = null;
            ManagementScope scope = null; try
            {
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost, @"Security\MicrosoftVolumeEncryption"),
                    "SELECT * FROM Win32_EncryptableVolume"))
                {
                    string letter = ("" + o["DriveLetter"]).ToUpper();
                    if (letter.Equals("C:"))
                    {
                        uint status = (uint)o["ProtectionStatus"];
                        switch (status)
                        {
                            case 0: result = "off"; break;
                            case 1: result = "on"; break;
                            case 2: result = "unknown"; break;
                            default: result = ""; break;
                        }
                        Console.WriteLine("Protection Status of " + targethost + " " + letter + " " + result + ".");
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get Protection Status of " + targethost + "\n" + e.Message);
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }


        public static string getMB(string targethost)
        {
            //Console.Write("Trying to get RAM of " + targethost + "...");
            string result = null;
            ManagementScope scope = null; try
            {
                ulong ram = 0;
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_ComputerSystem"))
                {
                    ram = ((ulong)o["TotalPhysicalMemory"]) / (1024 * 1204);
                    break;
                }
                //Console.WriteLine(targethost + ": " + ram);
                result = ram == 0 ? "?MB" : ram + "MB";
            }
            catch (Exception e)
            {
                //Console.WriteLine("Failed to get RAM of " + targethost + "\n" + e.Message);
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }

        public static string getMhz(string targethost)
        {
            //Console.Write("Trying to get CPU Speed of " + targethost + "...");
            string result = null;
            ManagementScope scope = null; try
            {
                uint cpu = 0;
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_Processor"))
                {
                    string id = (string)o["ProcessorId"];
                    cpu = (uint)o["MaxClockSpeed"];
                    //Console.WriteLine(targethost + ": cpu" + id + ": " + cpu + "Mhz");
                    break;
                }
                result = cpu == 0 ? "?Mhz" : cpu + "Mhz";
            }
            catch (Exception e)
            {
                //Console.WriteLine("Failed to get CPU Speed of " + targethost + "\n" + e.Message);
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }


        public enum DriveType {
            Unknown = 0,
            NoRootDirectory,
            RemovableDisk,
            LocalDisk,
            NetworkDrive,
            CompactDisc,
            RAMDisk
        }

        public static string getGB(string targethost)
        {
            //Console.Write("Trying to get Free Diskspace on " + targethost + "...");
            string result = null;
            ManagementScope scope = null; try
            {
                /*
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_DiskDrive"))
                {
                    ulong size = o["Size"] == null ? 0 : (ulong)o["Size"];
                    string sn = (string)o["SerialNumber"];
                }
                */
                StringBuilder res = new StringBuilder();
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_LogicalDisk"))
                {
                    string name = (string)o["Name"];
                    DriveType type = (DriveType)(o["Size"] == null ? 0 : (UInt32)o["DriveType"]);
                    decimal size = Math.Round((Convert.ToDecimal(o["Size"]) / 1073741824), 2);
                    decimal free = Math.Round((Convert.ToDecimal(o["FreeSpace"]) / 1073741824), 2);
                    decimal used = size - free;
                    int percent = size == 0 ? 100 : Convert.ToInt32((free / size) * 100);

//                    Int64 free = o["Size"] == null ? 0 : (Int64)((UInt64)o["FreeSpace"]) / (1024 * 1024 * 1024);
//                    Int64 size = o["Size"] == null ? 0 : (Int64)((UInt64)o["Size"]) / (1024*1024*1024);
                    if (type != DriveType.NetworkDrive
                        && type != DriveType.Unknown
                        && name.EndsWith(":"))
                    {
                        //Console.WriteLine(targethost + "\\" + name + " (" + type + ") has " + free + "GB Free / " + size + "GB Total");
                        res.AppendLine(name + " has " + free + "GB Free (" + percent + "%) / " + size + "GB");//Total
                    }
                }
                /*
                foreach (ManagementObject o in queryWMI(scope,
                    "SELECT * FROM Win32_PerfFormattedData_PerfDisk_LogicalDisk WHERE Name<>'_Total'"))
                {
                    uint f = (uint)o["FreeMegabytes"] / 1024;
                    string n = (string)o["Name"];
                }
                */
                result = res.ToString().TrimEnd('\n');
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get Diskspace on " + targethost + "\n" + e.ToString());
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }

        public static string getSerialNumber(string targethost)
        {
            //Console.Write("Trying to get Serial Number of " + targethost + "...");
            string result = null;
            ManagementScope scope = null; try
            {
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_SystemEnclosure"))
                {
                    result = (string)o["SerialNumber"];
                }
                //Console.WriteLine(targethost + " S/N = " + result);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Failed to get Serial Number of " + targethost + "\n" + e.ToString());
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }

        public static string getNetbiosName(string targethost)
        {
            //Console.Write("Trying to get Netbios Name of " + targethost + "...");
            string result = null;
            ManagementScope scope = null; try
            {
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_OperatingSystem"))
                {
                    result = (string)o["csname"];
                }
                //Console.WriteLine(targethost + " Netbios Name = " + result);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Failed to get Netbios Name of " + targethost + "\n" + e.ToString());
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }

        public static float getIEVersion(string targethost)
        {
            return getExeVersion(targethost, "iexplore", @"Program Files\Internet Explorer");
        }


        public static float getExeVersion(string targethost, string exe, string path_)
        {
            //Console.Write("Trying to get Exe " + exe + " Version of " + targethost + "...");
            string path = path_;
            if (!path.StartsWith(@"\") && !path.StartsWith(@"%")) path = @"\" + path;
            if (!path.EndsWith(@"\") && !path.EndsWith(@"%")) path = path + @"\";
            path = path.Replace(@"\", @"\\");

            string query = @"SELECT path,filename,extension,version FROM CIM_DataFile WHERE extension=""exe"""
                    + @" AND path" + (path.Contains("%") ? " LIKE " : "=") + @"""" + path + @""""
                    + @" AND filename" + (exe.Contains("%") ? " LIKE " : "=") + @"""" + exe + @"""";
            //Console.WriteLine("WMI Query: " + query);

            float result = 0;
            ManagementScope scope = null; try
            {
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost), query))
                {
                    string version = (string)o["version"];
                    version = Regex.Replace(version, @"([^.]*\.[^.]*)\..*", "$1");
                    try
                    {
                        float majorminor = float.Parse(version, CultureInfo.InvariantCulture);
                        if (majorminor > result) result = majorminor;
                    }
                    catch (Exception e)
                    {
                    }
                }
                //Console.WriteLine(targethost + ": Exe " + exe + " " + result);
            }
            catch (Exception e)
            {
                //Console.WriteLine("Failed to get Exe " + exe + " Version of " + targethost + "\n" + e.Message);
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }




        static Dictionary<ushort, string> BATT_Availability = new Dictionary<ushort, string> {
            {1, "Other"},
            {2, "Unknown"},
            {3, "Running or Full Power"},
            {4, "Warning"},
            {5, "In Test"},
            {6, "Not Applicable"},
            {7, "Power Off"},
            {8, "Off Line"},
            {9, "Off Duty"},
            {10, "Degraded"},
            {11, "Not Installed"},
            {12, "Install Error"},
            {13, "Power Save - Unknown - The device is known to be in a power save mode, but its exact status is unknown."},
            {14, "Power Save - Low Power Mode - The device is in a power save state but still functioning, and may exhibit degraded performance."},
            {15, "Power Save - Standby - The device is not functioning, but could be brought to full power quickly."},
            {16, "Power Cycle"},
            {17, "Power Save - Warning - The device is in a warning state, though also in a power save mode"}
        };

        static Dictionary<ushort, string> BATT_Status = new Dictionary<ushort, string> {
            {1, "The battery is discharging"},
            {2, "The system has access to AC so no battery is being discharged. However, the battery is not necessarily charging"},
            {3, "Fully Charged"},
            {4, "Low"},
            {5, "Critical"},
            {6, "Charging"},
            {7, "Charging and High"},
            {8, "Charging and Low"},
            {9, "Undefined"},
            {10,"Partially Charged"}
        };

        static Dictionary<ushort, string> BATT_Chemistry = new Dictionary<ushort, string> {
            {1, "Other"},
            {2, "Unknown"},
            {3, "Lead Acid"},
            {4, "Nickel Cadmium"},
            {5, "Nickel Metal Hydride"},
            {6, "Lithium-ion"},
            {7, "Zinc air"},
            {8, "Lithium Polymer"}
        };
        static Dictionary<uint, string> BATT_StatusInfo = new Dictionary<uint, string> {
            {1, "Other"},
            {2, "Unknown"},
            {3, "Enabled"},
            {4, "Disabled"},
            {5, "Not Applicable"},
        };

        static Dictionary<uint, string> BATT_ConfigManagerErrorCode = new Dictionary<uint, string> {
            {0, "Device is working properly."},
            {1, "Device is not configured correctly."},
            {2, "Windows cannot load the driver for this device."},
            {3, "Driver for this device might be corrupted, or the system may be low on memory or other resources."},
            {4, "Device is not working properly. One of its drivers or the registry might be corrupted."},
            {5, "Driver for the device requires a resource that Windows cannot manage."},
            {6, "Boot configuration for the device conflicts with other devices."},
            {7, "Cannot filter."},
            {8, "Driver loader for the device is missing."},
            {9, "Device is not working properly. The controlling firmware is incorrectly reporting the resources for the device."},
            {10, "Device cannot start."},
            {11, "Device failed."},
            {12, "Device cannot find enough free resources to use."},
            {13, "Windows cannot verify the device's resources."},
            {14, "Device cannot work properly until the computer is restarted."},
            {15, "Device is not working properly due to a possible re-enumeration problem."},
            {16, "Windows cannot identify all of the resources that the device uses."},
            {17, "Device is requesting an unknown resource type."},
            {18, "Device drivers must be reinstalled."},
            {19, "Failure using the VxD loader."},
            {20, "Registry might be corrupted."},
            {21, "System failure. If changing the device driver is ineffective, see the hardware documentation. Windows is removing the device."},
            {22, "Device is disabled."},
            {23, "System failure. If changing the device driver is ineffective, see the hardware documentation."},
            {24, "Device is not present, not working properly, or does not have all of its drivers installed."},
            {25, "Windows is still setting up the device."},
            {26, "Windows is still setting up the device."},
            {27, "Device does not have valid log configuration."},
            {28, "Device drivers are not installed."},
            {29, "Device is disabled. The device firmware did not provide the required resources."},
            {30, "Device is using an IRQ resource that another device is using."},
            {31, "Device is not working properly. Windows cannot load the required device drivers."}
        };
        
        
        static Dictionary<uint, string> BATT_PowerManagementCapabilities = new Dictionary<uint, string> {
            {0, "Unknown"},
            {1, "Not Supported"},
            {2, "Disabled"},
            {3, "Enabled - The power management features are currently enabled but the exact feature set is unknown or the information is unavailable."},
            {4, "Power Saving Modes Entered Automatically - The device can change its power state based on usage or other criteria."},
            {5, "Power State Settable - The SetPowerState method is supported. This method is found on the parent CIM_LogicalDevice class and can be implemented. For more information, see Designing Managed Object Format (MOF) Classes."},
            {6, "Power Cycling Supported - The SetPowerState method can be invoked with the PowerState parameter set to 5 (Power Cycle)."},
            {7, "Timed Power-On Supported - The SetPowerState method can be invoked with the PowerState parameter set to 5 (Power Cycle) and Time set to a specific date and time, or interval, for power-on."}
        };

        public static string getBatteryDetails(string targethost)
        {
            //Console.Write("Trying to get Netbios Name of " + targethost + "...");
            StringBuilder result = new StringBuilder();
            ManagementScope scope = null; try
            {
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_Battery"))
                {
                    //result.AppendLine(string.Format("Caption: {0}", (string)o["Caption"]));
                    result.AppendLine(string.Format("Description: {0}", (string)o["Description"]));
                    result.AppendLine(string.Format("P/N : {0}", (string)o["Name"]));
                    result.AppendLine(string.Format("S/N : {0}", (string)o["DeviceID"]));
                    result.AppendLine(string.Format("Status : {0}", (string)o["Status"]));
                    result.AppendLine(string.Format("BatteryStatus: {0}", BATT_Status[(ushort)o["BatteryStatus"]]));
                    /*
                     result.AppendLine(string.Format("Availability: {0}", BATT_Availability[(ushort)o["Availability"]]));
                    */
                    result.Append("\n" + string.Format("Chemistry: {0}", BATT_Chemistry[(ushort)o["Chemistry"]]));
                    DateTime InstallDate = Convert.ToDateTime(o["InstallDate"]);
                    result.Append(InstallDate == DateTime.MinValue ? "" : string.Format("  InstallDate: {0}", InstallDate.ToString()));
                    float DesignCapacity = 0, FullCapacity = 0, DesignVoltage = 0;
                    result.Append(/*"DesignVoltage"*/ o["FullChargeCapacity"] == null ? ""
                        : "  " + (DesignVoltage = (((ulong)o["DesignVoltage"]) / 1000f)) + " V");
                    //result.AppendLine(string.Format("FullChargeCapacity: {0}", o["FullChargeCapacity"] == null ? "N/A"
                    //    : (FullCapacity = (((uint)o["FullChargeCapacity"]) / 1000f)) + " Wh"));
                    result.Append(/*"DesignCapacity"*/ o["DesignCapacity"] == null ? ""
                        : "  " + (DesignCapacity = (((uint)o["DesignCapacity"]) / 1000f)) + " Wh");
                    float DesignAmpere = DesignVoltage == 0 ? 0 : DesignCapacity / DesignVoltage;
                    result.Append(/*"DesignCapacity"*/ DesignAmpere == 0 ? "" : "  " + DesignAmpere + " Ah");
                    result.AppendLine();

                    result.AppendLine("\n" + string.Format("EstimatedChargeRemaining: {0}", (ushort)o["EstimatedChargeRemaining"]) + " min");
                    result.AppendLine(string.Format("EstimatedRunTime : {0}", o["EstimatedRunTime"]).ToString() + " min");
                    /*result.AppendLine(string.Format("MaxRechargeTime: {0}", (string)o["MaxRechargeTime"]) + " min");
                    result.AppendLine(string.Format("ExpectedLife : {0}", (string)o["ExpectedLife"]) + " min");
                    result.AppendLine(string.Format("TimeToFullCharge : {0}", (string)o["TimeToFullCharge"]) + " min");
                    result.AppendLine(string.Format("TimeOnBattery: {0}", (string)o["TimeOnBattery"]) + " s");

                    result.AppendLine("\n" + string.Format("ConfigManagerUserConfig: {0}", (string)o["ConfigManagerUserConfig"]));
                    result.AppendLine(string.Format("ConfigManagerErrorCode: {0}", o["ConfigManagerErrorCode"] == null ? "none"
                        : BATT_ConfigManagerErrorCode[(uint)o["ConfigManagerErrorCode"]]));
                    result.AppendLine(string.Format("LastErrorCode : {0}", (string)o["LastErrorCode"]));
                    result.AppendLine(string.Format("ErrorCleared: {0}", (string)o["ErrorCleared"]));
                    result.AppendLine(string.Format("ErrorDescription : {0}", (string)o["ErrorDescription"]));

                    
                    //result.AppendLine(string.Format("ExpectedBatteryLife: {0}", (string)o["ExpectedBatteryLife"]));
                    result.AppendLine(string.Format("SmartBatteryVersion: {0}", (string)o["SmartBatteryVersion"]));
                    result.AppendLine(string.Format("StatusInfo : {0}", o["StatusInfo"] == null ? "none" : BATT_StatusInfo[(uint)o["StatusInfo"]]));


                    result.AppendLine(string.Format("PowerManagementSupported : {0}", o["PowerManagementSupported"]).ToString());
                    UInt16[] PowerManagement = (UInt16[])o["PowerManagementCapabilities"];
                    foreach (uint version in PowerManagement)
                    {
                        result.AppendLine(string.Format("PowerManagementCapabilities: {0}", BATT_PowerManagementCapabilities[version]));
                    }
                     */
                }
                //Console.WriteLine(targethost + " Netbios Name = " + result);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get Battery Details " + targethost + "\n" + e.ToString());
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result.Length == 0 ? null : result.ToString();
        }


        public static List<USBDeviceInfo> getUSBDevices(string targethost)
        {
            //Console.Write("Trying to get Netbios Name of " + targethost + "...");
            List<USBDeviceInfo> result = new List<USBDeviceInfo>();
            ManagementScope scope = null; try
            {
                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_USBHub"))
                {
                    USBDeviceInfo dev = new USBDeviceInfo((string)o["DeviceID"], (string)o["PNPDeviceID"], (string)o["Name"], (string)o["Description"]);
                    Console.WriteLine(targethost + " USB Device: " + dev);
                    result.Add(dev);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get USB Devices " + targethost + "\n" + e.ToString());
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }

        public class USBDeviceInfo
        {
            public USBDeviceInfo(string deviceID, string pnpDeviceID, string name, string description)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Name = name;
                this.Description = description;
            }
            public string DeviceID { get; private set; }
            public string PnpDeviceID { get; private set; }
            public string Name { get; private set; }
            public string Description { get; private set; }
            override public string ToString()
            {
                return DeviceID + "; " + PnpDeviceID + "; " + Name + "; " + Description;
            }
        }







        public static List<string> getUSBPrinters(string targethost)
        {
            return getUSBDevices(targethost, "USBPRINT");
        }

        public static List<string> getUSBStorage(string targethost)
        {
            return getUSBDevices(targethost, "USBSTOR");
        }


        public static List<string> getUSBDevices(string targethost, string DeviceTypeEquals)
        {
            //Console.Write("Trying to get Netbios Name of " + targethost + "...");
            List<string> result = new List<string>();
            ManagementScope scope = null; try
            {
                Hashtable DriveInfo = new Hashtable();
                foreach (ManagementObject ocd in queryWMI(scope = connectWMI(targethost), "SELECT * FROM Win32_CDRomDrive"))
                {
                    //string Model = (string)ocd["Model"];
                    string DeviceType = (string)ocd["MediaType"];
                    string DeviceID = (string)ocd["PNPDeviceID"];
                    //double GB = Math.Round((double)(UInt64)ocd["Size"] / (1024 * 1024 * 1024), 1);
                    Console.WriteLine(/*Model + "; " + */DeviceType + "; " + DeviceID /*+ "; " + GB*/);
                    DriveInfo[DeviceID] = DeviceType;
                }
                foreach (ManagementObject odd in queryWMI(scope = connectWMI(targethost), "SELECT * FROM Win32_DiskDrive"))
                {
                    string Model = (string)odd["Model"];
                    string DeviceType = (string)odd["MediaType"];
                    string DeviceID = (string)odd["PNPDeviceID"];
                    double GB = Math.Round((double)(UInt64)odd["Size"] / (1024 * 1024 * 1024), 1);
                    Console.WriteLine(Model + "; " + DeviceType + "; " + DeviceID + "; " + GB);
                    DriveInfo[DeviceID] = DeviceType + "/:" + Model + "/:" + GB;
                }                


                foreach (ManagementObject o in queryWMI(scope = connectWMI(targethost),
                    "SELECT * FROM Win32_USBControllerDevice"))
                {
                    string dev = (string)o["Dependent"];
                    string DeviceID = dev.Split('=')[1].Replace("\"", "");
                    string DeviceType = DeviceID.Split('\\')[0].Replace("\\", "");
                    string DeviceVendor = DeviceID.Split('\\')[2];
                    string Device = DeviceID.Split('\\')[4].Trim(new char[] { '{', '}' });

                    //Console.WriteLine(DeviceID);
                    if (DeviceType == DeviceTypeEquals) foreach (ManagementObject o2 in queryWMI(scope,
                    "SELECT * FROM Win32_PnPEntity WHERE DeviceID = '" + DeviceID + "'"))
                    {
                        string Description = (string)o2["Description"]; Description = Description.Replace("(", "").Replace(")", "").Trim();
                        string Manufacturer = (string)o2["Manufacturer"]; Manufacturer = Manufacturer.Replace("(", "").Replace(")", "").Trim();
                        string Service = (string)o2["Service"]; Manufacturer = Manufacturer.Replace("(", "").Replace(")", "").Trim();
                        Console.WriteLine(DeviceType + " \\ " + DeviceVendor + " \\ " + Device + "\n\t" + Manufacturer + "; " + Description);

                        string Info = DriveInfo.Contains(DeviceID) ? (string)DriveInfo[DeviceID] : "";

                        result.Add((Manufacturer.ToUpper().StartsWith("STANDARD") ? "" : Manufacturer + " ")
                            + Description.Replace(Manufacturer, "").Trim()
                            + Info);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to get USB Devices " + targethost + "\n" + e.ToString());
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }





        public static string GetTempFilePathWithExtension(string extension)
        {
            var path = Path.GetTempPath();
            var fileName = Guid.NewGuid().ToString() + extension;
            return Path.Combine(path, fileName);
        } 


        public static bool refreshManagementPoint(string machine, Func<string, bool> report, Func<bool, string, bool> check_MP)
        {
            try
            {
                string logfile = @"\\" + machine + @"\admin$\system32\CCM\Logs\LocationServices.log";
                report("Trying to refresh the MP...");
                if (!File.Exists(logfile))
                {
                    report("LocationServices.log not found!");
                    check_MP(false, null);
                    return false;
                }
                else
                {
                    DateTime last = new DateTime();
                    string tmp = null;
                    File.Copy(logfile, tmp = GetTempFilePathWithExtension(".tmp"));
                    string[] lines = File.ReadAllLines(tmp);
                    if (lines != null) foreach (string line in lines.Reverse<string>()) try
                            {
                                int i;
                                string time = line.Substring(i = line.IndexOf("<time=\"") + 7, line.IndexOf("\" date=") - i);
                                time = time.Substring(0, time.IndexOf("."));
                                string date = line.Substring(i = line.IndexOf(" date=\"") + 7, line.IndexOf("\" component=") - i);
                                //Console.WriteLine("=== LastDatetime = " + date + " " + time + " ===");
                                last = DateTime.ParseExact(date + " " + time, "MM-dd-yyyy HH:mm:ss",
                                    new CultureInfo("en-US"), DateTimeStyles.None);
                                Console.WriteLine("=== LastDatetime = " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm:ss}", last) + " ===");
                                break;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("invalid last line: " + e.Message + ": " + line);
                            }
                    MachineInfo.RestartService(machine, "CcmExec", 30000);
                    File.Delete(tmp);
                    Thread.Sleep(30000);
                    File.Copy(logfile, tmp);
                    lines = File.ReadAllLines(tmp);
                    bool LMP = false, PMP = false, DMP = false;
                    if (lines != null) foreach (string line in lines.Reverse<string>()) try
                            {
                                int i;
                                string log = line.Substring(i = line.IndexOf("<![LOG[") + 7, line.IndexOf("]LOG]!>") - i);
                                string time = line.Substring(i = line.IndexOf("<time=\"") + 7, line.IndexOf("\" date=") - i);
                                time = time.Substring(0, time.IndexOf("."));
                                string date = line.Substring(i = line.IndexOf(" date=\"") + 7, line.IndexOf("\" component=") - i);
                                DateTime t = DateTime.ParseExact(date + " " + time, "MM-dd-yyyy HH:mm:ss",
                                    new CultureInfo("en-US"), DateTimeStyles.None);
                                //Console.WriteLine("log: " + log + " datetime: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm:ss}", t));
                                if (t.CompareTo(last) >= 0)
                                {
                                    if (log.StartsWith("Retrieved local Management Point from AD: "))
                                    {
                                        string MP = log.Substring(42);
                                        report("LocalMP = " + MP);
                                        check_MP(true, MP);
                                        LMP = true;
                                    }
                                    else if (log.StartsWith("Retrieved Proxy Management Point from AD: "))
                                    {
                                        string MP = log.Substring(42);
                                        report("ProxyMP = " + MP);
                                        check_MP(true, MP);
                                        PMP = true;
                                    }
                                    else if (log.StartsWith("Retrieved Default Management Point from AD: "))
                                    {
                                        string MP = log.Substring(44);
                                        report("DefaultMP = " + MP);
                                        check_MP(true, MP);
                                        DMP = true;
                                    }
                                    if (LMP && PMP && DMP) break;
                                }
                                else break;
                            }
                            catch (Exception e)
                            {
                                //Console.WriteLine("invalid line: " + e.Message + ": " + line);
                            }
                    if (!LMP && !PMP && !DMP) check_MP(false, null);
                    return LMP || PMP || DMP;
                }
            }
            catch (Exception e)
            {
                report("refreshManagementPoint on " + machine + " failed: " + e.Message);
                check_MP(false, null);
                return false;
            }
        }


        public static bool enableSMSAutoAssignment(string targethost,
            Func<string, bool> report, Func<bool, string, bool> check_site)
        {
            report("Trying to enableSMSAutoAssignment on " + targethost + "...");
            bool result = false;
            ManagementScope scope = null; try
            {
                scope = connectWMI(targethost);

                StringBuilder free = new StringBuilder();
                ObjectQuery query = new ObjectQuery("SELECT * FROM SMS_Client");
                foreach (ManagementObject o in new ManagementObjectSearcher(scope, query).Get())
                {
                    o.SetPropertyValue("EnableAutoAssignment", 1);
                }
                report("enableSMSAutoAssignment on " + targethost + " has been set. Restarting CcmExec service...");
                MachineInfo.RestartService(targethost, "CcmExec", 30000);

                check_site(true, new SMSClient(targethost).SiteCode);
                result = true;
            }
            catch (Exception e)
            {
                report("enableSMSAutoAssignment on " + targethost + " failed: " + e.Message);
                check_site(false, null);
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }


        public static string getSMSComponents(string machine, Func<string, bool> report, Func<bool, bool> check)
        {
            try
            {
                SMSClient smscli = new SMSClient(machine);
                StringBuilder ret = new StringBuilder();
                foreach (var c in smscli.Components.CCM_InstalledComponent)
                    try
                    {
                        string mof = c.GetText(TextFormat.Mof);
                        //string wmi = c.GetText(TextFormat.WmiDtd20);
                        //string cim = c.GetText(TextFormat.CimDtd20);
                        int i, j;
                        string disp = mof.Substring(i = mof.IndexOf("\tDisplayName = \"") + 16, j = mof.IndexOf("\"", i) - i);
                        if (i == 15 || j < 0) throw new Exception("DisplayName not found.");
                        string name = mof.Substring(i = mof.IndexOf("\tName = \"") + 9, j = mof.IndexOf("\"", i) - i);
                        if (i == 8 || j < 0) throw new Exception("Name not found.");
                        report("Installed Component: " + disp + " (" + name + ")");
                        ret.AppendLine(disp + " (" + name + ")");
                    }
                    catch (Exception e)
                    {
                        report("Installed Component: *parsing issue* " + e.Message + ": " + c.GetText(TextFormat.Mof));
                    }
                check(ret.Length > 0);
                return ret.ToString();
            }
            catch (Exception e)
            {
                check(false);
                report("Installed Component: *client unavailable* " + e.Message);
                return "";
            }
        }


        public static DateTime getSMSClientHeartbeat(string SMS_server, string site_name, string machine,
            Func<string, bool> error)
        {
            Hashtable ret = new Hashtable();
            getSMSClientHeartbeat(SMS_server, site_name, "WHERE Name = '" + machine + "'", ref ret, 
                report => {Console.WriteLine(report); return true;}, error);
            return ret.ContainsKey(machine) ? (DateTime)ret[machine] : DateTime.MaxValue;
        }

        public static void getSMSClientHeartbeat(string SMS_server, string site_name, string Name_WHERE, ref Hashtable ret,
            Func<string, bool> report, Func<string, bool> error)
        {
            try
            {
                foreach (ManagementObject o in new ManagementObjectSearcher(
                    @"\\" + SMS_server + @"\root\sms\" + site_name, "SELECT * FROM SMS_R_System " + Name_WHERE).Get())
                {
                    string Computer = o["Name"] == null ? "" : ((string)o["Name"]).ToUpper();
                    if (Computer.Length > 0)
                    {
                        bool Client_Yes = o["Client"] == null ? false : ((uint)o["Client"]) == 1;
                        if (Client_Yes)
                        {
                            if (!report(Computer + " has a Client.")) return;
                            DateTime AgentTime = new DateTime();
                            DateTime[] AgentTimes = (o.Properties["AgentTime"].Value as string[])
                                .Select(x => DateTime.ParseExact(x.Substring(0, 14), "yyyyMMddHHmmss", new CultureInfo("en-US"), DateTimeStyles.None))
                                .ToArray();
                            //string[] AgentTimes = ((Array)o["AgentTime"]).Cast<string>().ToArray();
                            foreach (var agent in (o.Properties["AgentName"].Value as string[]).Select((x, i) => new { Name = x, Index = i }))
                                if (agent.Name == "Heartbeat Discovery")
                                {
                                    if (AgentTimes != null)
                                    {
                                        if (AgentTimes[agent.Index].CompareTo(AgentTime) > 0) AgentTime = AgentTimes[agent.Index];
                                        if (!report(Computer + " has Heartbeat on "
                                            + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", AgentTimes[agent.Index]))) return;
                                    }
                                }
                            if (ret.ContainsKey(Computer))
                            {
                                DateTime AgentTime0 = (DateTime)ret[Computer];
                                if (AgentTime0.CompareTo(AgentTime) > 0) AgentTime = AgentTime0;
                                ret.Remove(Computer);
                            }
                            ret.Add(Computer, AgentTime);
                        }
                        else
                        {
                            ret.Add(Computer, DateTime.MaxValue);
                            if (!report(Computer + " has NO Client.")) return;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                error("An exception occured while collecting heartbeat records: " + e.Message
                    + "\n" + e.StackTrace);
            }
        }

        /*
        const string WQL_HAVE_REPORT_IN_7DAYS = ""
            //query to display computers that have a heartbeat discovery in the past seven days
            + "select "
	        + "R.ResourceId, "
	        + "R.Name0, "
	        + "R.Resource_Domain_OR_Workgr0, "
	        + "AD.AgentTime, "
	        + "datediff(dd,AD.AgentTime,getdate() -7) "
            + "from "
	        + "dbo.v_R_System R "
	        + "join dbo.v_AgentDiscoveries AD on AD.ResourceId = R.ResourceId "
            + "Where "
	        + "AgentName = 'Heartbeat Discovery' "
	        + "and datediff(dd,AD.AgentTime,getdate() -7) > 0";


        const string WQL_NO_REPORT_IN_30DAYS = ""
            //query to list all the machines that haven't reported its discovery/inventory since past 30 days 
            + "select sys.Netbios_Name0, "
            + "sys.Client_Type0, "
            + "sys.Creation_date0 as 'Creation Date', "
            + "sys.Client_Version0, "
            + "sys.User_Domain0, "
            + "sys.User_Name0, "
            + "inst.sms_assigned_sites0 as 'sitecode', "
            + "disc.AgentTime as 'Last Discovery date', "
            + "hw.LastHWScan as 'Last HW Scan', "
            + "sw.LastScanDate as 'Last SW Scan' "
            + "from v_R_System sys"
            + "left join "
            + "("
            + "select ResourceId, MAX(AgentTime) as AgentTime"
            + "from v_AgentDiscoveries"
            + "group by ResourceId"
            + ") as disc on disc.ResourceId = sys.ResourceID"
            + "left join v_GS_WORKSTATION_STATUS hw on hw.ResourceID = sys.ResourceID"
            + "left join v_GS_LastSoftwareScan sw on sw.ResourceID = sys.ResourceID"
            + "left join v_RA_System_SMSAssignedSites inst on inst.resourceid=sys.resourceid"
            + "where "
            + "disc.AgentTime < getdate()-30"
            + "or hw.LastHWScan < getdate()-30"
            + "or sw.LastScanDate < getdate()-30"
            + "and (sys.Client0 = 1 and sys.Obsolete0=0)"
            + "and sys.Name0 in (SELECT Name0"
            + "FROM v_r_system"
            + "GROUP BY Name0"
            + "HAVING COUNT(*) < 2)"
            + "order by disc.AgentTime asc";
        */




        public static int getUACStatus(string machine, string username, string password, bool chkremreg)
        {
            object ret = GetRegistryValue(machine, RegHive.HKEY_LOCAL_MACHINE,
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System",
                "EnableLUA", username, password, chkremreg);
            Console.WriteLine("getUACStatus on " + machine + " : " + ret); 
            if (ret == null) return -1;
            else if (ret.ToString().Equals("0")) return 0;
            else return 1;
        }




        public enum RegHive : uint
        {
            HKEY_CLASSES_ROOT   = unchecked((uint)0x80000000),
            HKEY_CURRENT_USER   = unchecked((uint)0x80000001),
            HKEY_LOCAL_MACHINE  = unchecked((uint)0x80000002),
            HKEY_USERS          = unchecked((uint)0x80000003),
            HKEY_CURRENT_CONFIG = unchecked((uint)0x80000005)
        }


        static public RegHive getHive(string hiveString)
        {
            switch (hiveString)
            {
                case "HKEY_CLASSES_ROOT": return RegHive.HKEY_CLASSES_ROOT;
                case "HKEY_CURRENT_USER": return RegHive.HKEY_CURRENT_USER;
                case "HKEY_LOCAL_MACHINE": return RegHive.HKEY_LOCAL_MACHINE;
                case "HKEY_USERS": return RegHive.HKEY_USERS;
                case "HKEY_CURRENT_CONFIG": return RegHive.HKEY_CURRENT_CONFIG;
                default: return 0;
            }
        }

        static public string getHiveString(RegHive hive)
        {
            switch (hive)
            {
                case RegHive.HKEY_CLASSES_ROOT: return "HKEY_CLASSES_ROOT";
                case RegHive.HKEY_CURRENT_USER: return "HKEY_CURRENT_USER";
                case RegHive.HKEY_LOCAL_MACHINE: return "HKEY_LOCAL_MACHINE";
                case RegHive.HKEY_USERS: return "HKEY_USERS";
                case RegHive.HKEY_CURRENT_CONFIG: return "HKEY_CURRENT_CONFIG";
                default: return null;
            }
        }

        static public string getShortHiveString(string hiveString)
        {
            switch (hiveString)
            {
                case "HKEY_CLASSES_ROOT": return "HKCR";
                case "HKEY_CURRENT_USER": return "HKCU";
                case "HKEY_LOCAL_MACHINE": return "HKLM";
                case "HKEY_USERS": return "HKU";
                case "HKEY_CURRENT_CONFIG": return "HKCC";
                default: return hiveString;
            }
        }

        public enum RegType
        {
            REG_SZ = 1,
            REG_EXPAND_SZ,
            REG_BINARY,
            REG_DWORD,
            REG_MULTI_SZ = 7,
            REG_UNKNOWN = -1
        }

        public static WmiReg getWmiReg(string machine, string username, string password)
        {
            return new WmiReg(machine, username, password, true);
        }

        public static string NOTREACHABLE = "Not reachable.";
        public class WmiReg : ManagementClass
        {
            public string MACHINE = null;
            public string Error = "";
            public bool Connected = false;
            public bool remregrun = false;

            public WmiReg(string machine, string username, string password, bool chkremreg)
                : base("StdRegProv")
            {
                MACHINE = machine.ToUpper();
                try
                {
                    IPAddress IP = ping(MACHINE, false);
                    string netbios = IP == null ? "" : getNetbiosName(MACHINE);
                    if (netbios == null) netbios = "";
                    if (netbios.Length == 0 || !netbios.ToUpper().Equals(MACHINE))
                    {
                        throw new Exception(NOTREACHABLE);
                    }

                    if (chkremreg)
                    {
                        bool run = isServiceRunning(MACHINE, "RemoteRegistry", false);
                        if (!run)
                        {
                            StartService(MACHINE, "RemoteRegistry", 30000);
                            run = isServiceRunning(MACHINE, "RemoteRegistry", false);
                            remregrun = run;
                        }
                        if (!run)
                        {
                            throw new Exception("Remote Registry Service cannot be started.");
                        }
                    }

                    ManagementScope scope = new ManagementScope();
                    scope.Path.Server = MACHINE;
                    scope.Path.NamespacePath = @"root\default";
                    scope.Options.EnablePrivileges = true;
                    scope.Options.Impersonation = ImpersonationLevel.Impersonate;
                    scope.Options.Authentication = AuthenticationLevel.Default;
                    if (username != null && password != null)
                    {
                        scope.Options.Username = username;
                        scope.Options.Password = password;
                    }
                    scope.Connect();
                    Scope = scope;
                    Connected = true;
                }
                catch (Exception e)
                {
                    //Console.WriteLine("WMI connection failure: " + e);
                    Error = e.Message;
                    Scope = new ManagementScope();
                }
            }
            public void Disconnect()
            {
                if (Scope != null)
                {
                    disconnectWMI(Scope);
                    Scope = new ManagementScope();
                }
                Connected = false;
                if (remregrun)
                {
                    StopService(MACHINE, "RemoteRegistry", 30000);
                }
            }
        }

        public static object GetRegistryValue(string machine, RegHive hive, string key, string param,
            string username, string password, bool chkremreg)
        {
            WmiReg wmiReg = new WmiReg(machine, username, password, chkremreg);
            if (!wmiReg.Connected) return null;

            object ret = null;
            try
            {
                ret = GetRegistryValue(wmiReg, hive, key, param);
            }
            catch (Exception e)
            {
                Console.WriteLine("getRegistryValue on " + machine + "\nError: " + e.Message);
            }
            finally
            {
                wmiReg.Disconnect();
            }
            return ret;
        }


        public static List<string> GetRegistrySubKeys(ManagementClass mc, RegHive hive, string key)
        {
            try
            {
                List<string> list = new List<string>();
                object[] args = new object[] { hive, key, null };
                uint ret = (uint)mc.InvokeMethod("EnumKey", args);
                string[] subkeys = (string[])args[2];
                if (subkeys == null) return null;
                foreach (string k in subkeys) list.Add(k);
                return list;
            }
            catch (Exception e)
            {
                Console.WriteLine("GetRegistrySubKeys error: " + mc.Scope.Path.Server + "\\" + hive + "\\" + key + " : " + e.Message);
                return null;
            }
        }


        public static Dictionary<string, object> GetRegistryKeyNamesAndValues(ManagementClass mc, RegHive hive, string key,
            Func<string, bool> paramFilter, Func<string, string> valueReplacer, Func<string, bool> valueFilter)
        {
            ManagementBaseObject inParams = mc.GetMethodParameters("EnumValues");
            inParams["hDefKey"] = hive;
            inParams["sSubKeyName"] = key;
            ManagementBaseObject outParams = mc.InvokeMethod("EnumValues", inParams, null);

            if (Convert.ToUInt32(outParams["ReturnValue"]) != 0) return null;
            else {
                Dictionary<string, object> ret = new Dictionary<string, object>();
                string[] paramArray = outParams["sNames"] as String[];
                if (paramArray != null) foreach (string param in paramArray)
                {
                    var value = GetRegistryValue(mc, hive, key, param);
                    if (value == null) value = "";
                    if (value is string) value = valueReplacer((string)value);
                    if (paramFilter(param) && valueFilter(value.ToString()))
                        ret.Add(param, value);
                }
                return ret;
            }
        }

        public static string GetRegistrySubkeysValues(ManagementClass mc, RegHive hive, string key,
            Func<string, bool> paramFilter, Func<string, string> valueReplacer, Func<string, bool> valueFilter)
        {
            List<string> ret = new List<string>();
            var subkeys = MachineInfo.GetRegistrySubKeys(mc, hive, key);
            if (subkeys == null) return null;
            if (subkeys != null && subkeys.Count > 0)
            {
                List<string> values = new List<string>();
                foreach (var subkey in subkeys)
                    values.AddRange(GetRegistryKeyNamesAndValues(mc, hive, key + '\\' + subkey, paramFilter, valueReplacer, valueFilter)
                        .Values.Select(v => {
                            if (v == null) return "";
                            if (v.GetType().IsArray)
                            {
                                Type t = v.GetType().GetElementType();
                                if (t == typeof(byte)) return BitConverter.ToString((byte[])v);
                                else return t.ToString();
                            }
                            else return v.ToString();
                        }));
                ret.AddRange(values);
            }
            ret = ret.Distinct().ToList();
            return ret.Count > 0 ? ret.Aggregate((x, y) => x + ", \n" + y) : "";
        }


        public static object GetRegistryValue(ManagementClass mc, RegHive hive, string key, string param)
        {
            RegType rType = GetRegistryValueType(mc, hive, key, param);

            ManagementBaseObject inParams = mc.GetMethodParameters("GetStringValue");
            inParams["hDefKey"] = hive;
            inParams["sSubKeyName"] = key;
            inParams["sValueName"] = param;

            object oValue = null;

            switch (rType)
            {
                case RegType.REG_SZ:
                    ManagementBaseObject outParams = mc.InvokeMethod("GetStringValue", inParams, null);

                    if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                    {
                        oValue = outParams["sValue"];
                    }
                    else
                    {
                        // GetStringValue call failed
                    }
                    break;

                case RegType.REG_EXPAND_SZ:
                    outParams = mc.InvokeMethod("GetExpandedStringValue", inParams, null);

                    if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                    {
                        oValue = outParams["sValue"];
                    }
                    else
                    {
                        // GetExpandedStringValue call failed
                    }
                    break;

                case RegType.REG_MULTI_SZ:
                    outParams = mc.InvokeMethod("GetMultiStringValue", inParams, null);

                    if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                    {
                        oValue = outParams["sValue"];
                    }
                    else
                    {
                        // GetMultiStringValue call failed
                    }
                    break;

                case RegType.REG_DWORD:
                    outParams = mc.InvokeMethod("GetDWORDValue", inParams, null);

                    if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                    {
                        oValue = outParams["uValue"];
                    }
                    else
                    {
                        // GetDWORDValue call failed
                    }
                    break;

                case RegType.REG_BINARY:
                    outParams = mc.InvokeMethod("GetBinaryValue", inParams, null);

                    if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
                    {
                        oValue = outParams["uValue"] as byte[];
                    }
                    else
                    {
                        // GetBinaryValue call failed
                    }
                    break;
            }

            return oValue;
        }

        public static RegType GetRegistryValueType(ManagementClass mc, RegHive hive, string key, string param)
        {
            ManagementBaseObject inParams = mc.GetMethodParameters("EnumValues");
            inParams["hDefKey"] = hive;
            inParams["sSubKeyName"] = key;

            ManagementBaseObject outParams = mc.InvokeMethod("EnumValues", inParams, null);

            if (Convert.ToUInt32(outParams["ReturnValue"]) == 0)
            {
                string[] sNames = outParams["sNames"] as String[];
                int[] iTypes = outParams["Types"] as int[];

                for (int i = 0; i < sNames.Length; i++)
                {
                    if (sNames[i] == param)
                    {
                        return (RegType)iTypes[i];
                    }
                }
                // value not found
            }
            else
            {
                // EnumValues call failed
            }
            return RegType.REG_UNKNOWN;
        }




        public static List<Object[]> GetInstalledAppsWMI(string machine, Func<object[], int> update, Func<bool> stop,
            string username, string password, bool chkremreg, List<Object[]> desiredList)
        {
            Hashtable h = new Hashtable();
            List<Object[]>
    missingList = desiredList.Select(i => new Object[] { i[1], i[2], i[0], new DateTime(), -1 }).ToList(),
    foundList = new List<Object[]>();


            List<Object[]> rows = new List<Object[]>();
            Object[] HK = new[] {
                new Object[] {RegHive.HKEY_CURRENT_USER, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"},
                new Object[] {RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"},
                new Object[] {RegHive.HKEY_LOCAL_MACHINE, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"}
            };

            WmiReg wmiReg = new WmiReg(machine, username, password, chkremreg);
            if (!wmiReg.Connected)
            {
                Console.WriteLine("getInstalledApps on " + machine + " : Not available");
                return null;
            }

            try
            {
                foreach (Object[] o in HK)
                {
                    if (stop()) return null;

                    /*uint hive = (uint)o[0];
                    string hiveStr = hive == RegHive.HKEY_CURRENT_USER ? "HKEY_CURRENT_USER"
                        : hive == RegHive.HKEY_LOCAL_MACHINE ? "HKEY_LOCAL_MACHINE"
                        : "" + hive;
                    */
                    RegHive hive = (RegHive)o[0];
                    string path = (string)o[1];
                    object[] args = new object[] { hive, path, null };
                    uint ret = (uint)wmiReg.InvokeMethod("EnumKey", args);
                    Console.WriteLine("getInstalledApps on " + machine + ":\nInspecting... " + getHiveString(hive) + @"\" + path + " returns " + ret);
                    string[] subkeys = (string[])args[2];

                    ManagementBaseObject inParam = wmiReg.GetMethodParameters("GetStringValue");
                    inParam["hDefKey"] = hive;
                    if (subkeys != null) foreach (string k in subkeys)
                        {
                            if (stop()) return null;

                            inParam["sSubKeyName"] = path + @"\" + k;
                            ManagementBaseObject outParam;

                            string name = "";
                            inParam["sValueName"] = "DisplayName";
                            outParam = wmiReg.InvokeMethod("GetStringValue", inParam, null);
                            if ((uint)outParam["ReturnValue"] == 0) name = outParam["sValue"].ToString();

                            string version = "";
                            inParam["sValueName"] = "DisplayVersion";
                            outParam = wmiReg.InvokeMethod("GetStringValue", inParam, null);
                            if ((uint)outParam["ReturnValue"] == 0) version = outParam["sValue"].ToString();
                            int[] abcd = getVersion(version);

                            string vendor = "";
                            inParam["sValueName"] = "Publisher";
                            outParam = wmiReg.InvokeMethod("GetStringValue", inParam, null);
                            if ((uint)outParam["ReturnValue"] == 0) vendor = outParam["sValue"].ToString();

                            string install = "";
                            inParam["sValueName"] = "InstallDate";
                            outParam = wmiReg.InvokeMethod("GetStringValue", inParam, null);
                            if ((uint)outParam["ReturnValue"] == 0) install = outParam["sValue"].ToString();

                            DateTime date = install.Length == 8 ? DateTime.ParseExact(install, "yyyyMMdd",
                                new CultureInfo("en-US"), DateTimeStyles.None) : new DateTime();


                            int desired = 0;

                            foreach (Object[] i in missingList)
                            {
                                string desiredVendor = (string)i[2];
                                string desiredName = (string)i[0];
                                bool forbidden = false;
                                if (desiredName.StartsWith("!"))
                                {
                                    desiredName = desiredName.Substring(1);
                                    forbidden = true;
                                }
                                string desiredVersion = (string)i[1];
                                int[] dabcd = getVersion(desiredVersion);

                                if (vendor.ToLower().StartsWith(desiredVendor.ToLower())
                                    && name.ToLower().StartsWith(desiredName.ToLower()))
                                {
                                    //Console.WriteLine("***found*** vendor:" + vendor + ":desired:" + desiredVendor + ":");
                                    //Console.WriteLine("\tname:" + name + ":desired:" + desiredName + ":");
                                    foundList.Add(i);
                                    if (forbidden)
                                        desired = -2;
                                    else
                                    {
                                        desired = compareVersion(abcd, dabcd) == 0 ? 2 : 1;
                                        //Console.WriteLine("\t\tcompare:" + version + ":desired:" + desiredVersion+":" + desired);
                                    }
                                    break;
                                }
                            }


                            if (name.Length > 0)
                            {
                                string x = "[" + vendor + "] " + name + " (" + version
                                    + ") - Install Date: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy}", date);
                                if (!h.ContainsKey(x))
                                {
                                    Object[] row = new Object[] { name, version, vendor, date, desired };
                                    h.Add(x, row);
                                    rows.Add(row);
                                    Object[] row_ = new Object[] { name, version, vendor,
                                            date == DateTime.MinValue ? ""
                                            : String.Format(new CultureInfo("en-US"), "{0:yyyy-MM-dd}", date),
                                            desired };
                                    update(row_);

                                    //Console.WriteLine(x);
                                }
                            }
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("getInstalledApps on " + machine + "\nError: " + e.Message);
            }
            finally
            {
                wmiReg.Disconnect();
            }
            return rows;
        }


        public static List<Object[]> GetInstalledAppsREG(string machine, Func<object[], int> update, Func<bool> stop,
            bool console, List<Object[]> desiredList)
        {
            List<Object[]>
                missingList = desiredList.Select(i => new Object[] { i[1], i[2], i[0], new DateTime(), -1 }).ToList(),
                foundList = new List<Object[]>();

            List<Object[]> rows = new List<Object[]>();
            RegistryHive[] hives = new RegistryHive[] {
                RegistryHive.CurrentUser,
                RegistryHive.LocalMachine,
                RegistryHive.LocalMachine
            };
            string[] paths = new[] {
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall",
                @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
            };
            RegistryKey[] rs = new RegistryKey[paths.Length];
            for (int i = 0; i < rs.Length; i++)
            {
                try
                {
                    rs[i] = null;
                    rs[i] = RegistryKey.OpenRemoteBaseKey(hives[i], machine).OpenSubKey(paths[i]);
                    if (stop()) return null;
                }
                catch (Exception e)
                {
                    if (console) Console.WriteLine("getInstalledApps on " + machine + ":\nInspecting... " + hives[i] + "\\" + paths[i]
                        + "\nError: " + e.Message);
                }
            }
            Hashtable h = new Hashtable();
            foreach (RegistryKey r in rs) if (r != null)
                {
                    if (stop()) return null;
                    if (console) Console.WriteLine("getInstalledApps on " + machine + ":\nInspecting... " + r.ToString());
                    foreach (string n in r.GetSubKeyNames())
                    {
                        if (stop()) return null;
                        using (RegistryKey k = r.OpenSubKey(n))
                        {
                            try
                            {
                                string name = k.GetValue("DisplayName") != null ? k.GetValue("DisplayName").ToString().Trim() : "";
                                string version = k.GetValue("DisplayVersion") != null ? k.GetValue("DisplayVersion").ToString().Trim() : "";
                                int[] abcd = getVersion(version);
                                string vendor = k.GetValue("Publisher") != null ? k.GetValue("Publisher").ToString().Trim() : "";
                                string install = k.GetValue("InstallDate") != null ? k.GetValue("InstallDate").ToString().Trim() : "";
                                DateTime date = install.Length == 8 ? DateTime.ParseExact(install, "yyyyMMdd", new CultureInfo("en-US"), DateTimeStyles.None) : new DateTime();
                                int desired = 0;

                                foreach (Object[] i in missingList)
                                {
                                    string desiredVendor = (string)i[2];
                                    string desiredName = (string)i[0];
                                    bool forbidden = false;
                                    if (desiredName.StartsWith("!"))
                                    {
                                        desiredName = desiredName.Substring(1);
                                        forbidden = true;
                                    }
                                    string desiredVersion = (string)i[1];
                                    int[] dabcd = getVersion(desiredVersion);

                                    if (vendor.ToLower().StartsWith(desiredVendor.ToLower())
                                        && name.ToLower().StartsWith(desiredName.ToLower()))
                                    {
                                        //Console.WriteLine("***found*** vendor:" + vendor + ":desired:" + desiredVendor + ":");
                                        //Console.WriteLine("\tname:" + name + ":desired:" + desiredName + ":");
                                        foundList.Add(i);
                                        if (forbidden)
                                            desired = -2;
                                        else
                                        {
                                            desired = compareVersion(abcd, dabcd) == 0 ? 2 : 1;
                                            //Console.WriteLine("\t\tcompare:" + version + ":desired:" + desiredVersion+":" + desired);
                                        }
                                        break;
                                    }
                                }

                                if (name.Length > 0)
                                {
                                    string x = "[" + vendor + "] " + name + " (" + version
                                        + ") - Install Date: " + String.Format(new CultureInfo("en-US"), "{0:yyyy-MMM-dd}", date);
                                    if (!h.ContainsKey(x))
                                    {
                                        Object[] row = new Object[] { name, version, vendor, date, desired };
                                        h.Add(x, row);
                                        rows.Add(row);
                                        Object[] row_ = new Object[] { name, version, vendor,
                                            date == DateTime.MinValue ? ""
                                            : String.Format(new CultureInfo("en-US"), "{0:yyyy-MM-dd}", date),
                                            desired };
                                        update(row_);

                                        if (console) Console.WriteLine(x);
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                string kstring = k == null ? "null" : k.ToString();
                                Console.WriteLine("getInstalledApps on " + machine + ":\nInspecting... " + kstring + "\nError: " + e.ToString());
                                if (console) Console.WriteLine("getInstalledApps on " + machine + ":\nInspecting... " + kstring + "\nError: " + e.ToString());
                            }
                        }
                    }
                    r.Close();
                }

                //foreach (Object[] i in missingList) Console.WriteLine("missing---" + i[0]);
                //foreach (Object[] i in foundList) Console.WriteLine("found---" + i[0]);
                missingList.RemoveAll(i => foundList.Contains(i));//recherche erronnée
                //foreach (Object[] i in missingList) Console.WriteLine("diff---" + i[0]);
                foreach (Object[] i in missingList) if (!i[0].ToString().StartsWith("!"))
                        update(new Object[] { i[0], i[1], i[2], "***MISSING***", i[4] });

            return rows;
        }

        static int Parse(string str)
        {
            if (str == null || str.Length == 0) return 0;
            try
            {
                return int.Parse(str);
            }
            catch (Exception e)
            {
                return 0;
            }
        }

        static int[] getVersion(string str)
        {
            string[] abcd = str.Split(new char[] { '.' }, StringSplitOptions.None);
            int a = abcd.Length > 0 ? Parse(abcd[0]) : -1,
                b = abcd.Length > 1 ? Parse(abcd[1]) : -1,
                c = abcd.Length > 2 ? Parse(abcd[2]) : -1,
                d = abcd.Length > 3 ? Parse(abcd[3]) : -1;
            return new int[] { a, b, c, d };
        }

        static int compareVersion(int[] abcd, int[] dabcd)
        {
            if (dabcd[0] == -1) return 0;
            else if (abcd[0] > dabcd[0]) return 1;
            else if (abcd[0] < dabcd[0]) return -1;
            else
            {
                if (dabcd[1] == -1) return 0;
                else if (abcd[1] > dabcd[1]) return 1;
                else if (abcd[1] < dabcd[1]) return -1;
                else
                {
                    if (dabcd[2] == -1) return 0;
                    else if (abcd[2] > dabcd[2]) return 1;
                    else if (abcd[2] < dabcd[2]) return -1;
                    else
                    {
                        if (dabcd[3] == -1) return 0;
                        else if (abcd[3] > dabcd[3]) return 1;
                        else if (abcd[3] < dabcd[3]) return -1;
                        else return 0;
                    }
                }
            }
        }


        public static Bitmap getDesiredIcon(int desired)
        {
            switch (desired)
            {
                case -2: //forbidden app present
                    return global::AuditSec.Properties.Resources.FBD;
                case -1: //desired app not present
                    return global::AuditSec.Properties.Resources.MIS;
                case 0: //unknown app
                    return global::AuditSec.Properties.Resources.BLK;
                case 1: //desired app present, wrong version
                    return global::AuditSec.Properties.Resources.NG;
                case 2: //desired app present, right version
                    return global::AuditSec.Properties.Resources.OK;
                default:
                    return null;
            }
        }







        public static Boolean isValidCredential(string targethost, string username, string password)
        {
            bool result = false;
            ManagementScope scope = null; if (targethost != string.Empty) try
            {

                string bat = @"\\" + targethost + @"\admin$\process.bat";

                if (File.Exists(bat)) File.Delete(bat);
                StreamWriter sw = new StreamWriter(bat);
                string _cmd = "TASKLIST";
                sw.WriteLine(_cmd);
                sw.Close();

                scope = connectWMI(targethost, username, password);
                scope.Options.EnablePrivileges = true;


                ObjectGetOptions objectGetOptions = new ObjectGetOptions();
                ManagementPath managementPath = new ManagementPath("Win32_Process");
                ManagementClass processClass = new ManagementClass(scope, managementPath, objectGetOptions);
                ManagementBaseObject inParams = processClass.GetMethodParameters("Create");
                inParams["CommandLine"] = bat;
                ManagementBaseObject outParams = processClass.InvokeMethod("Create", inParams, null);
                //Console.WriteLine("Creation of the process returned: " + outParams["returnValue"]);
                //Console.WriteLine("Process ID: " + outParams["processId"]);
                result = true;
            }
            catch (Exception e)
            {
                if (e.ToString().StartsWith("The network path was not found.")) ;// Console.WriteLine("Not available.");
            }
            finally
            {
                disconnectWMI(scope);
            }
            return result;
        }





        public static Boolean ResetPassword(string computerName, string userName, string oldPassword, string newPassword, Func<string, bool> status)
        {

            const int ERROR_ACCESS_DENIED = 5;
            const int ERROR_INVALID_PASSWORD = 86;
            //const int NERR_InvalidComputer = ?;
            //const int NERR_NotPrimary = ?;
            //const int NERR_UserNotFound = 2221;
            //const int NERR_PasswordTooShort = ?;


            status("Trying to reset password of user '" + userName
                + "' on computer '" + computerName + "'...");
            uint ret = ChangeUserPassword(computerName, userName, oldPassword, newPassword);
            if (ret > 0)
            {
                status("Changes could not be committed onto '" + computerName + "'."
                + "\nError: " + (ret == 86 ? "The specified network password is not correct."
                : (ret == 5 ? "Access is denied." : "" + ret)));
                return false;
            }
            else
            {
                status("Changes committed successfully onto '" + computerName + "'.");
                return true;
            }

        }



        [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int NetUserAdd(
            [MarshalAs(UnmanagedType.LPWStr)] string servername,
            UInt32 level,
            ref USER_INFO_1 userinfo,
            out UInt32 parm_err);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct USER_INFO_1
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sUsername;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sPassword;
            public uint uiPasswordAge;
            public uint uiPriv;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sHome_Dir;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sComment;
            public uint uiFlags;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string sScript_Path;
        }

        [DllImport("netapi32.dll", CharSet = CharSet.Unicode,
            CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        static extern uint NetUserChangePassword(
            [MarshalAs(UnmanagedType.LPWStr)] string domainname,
            [MarshalAs(UnmanagedType.LPWStr)] string username,
            [MarshalAs(UnmanagedType.LPWStr)] string oldpassword,
            [MarshalAs(UnmanagedType.LPWStr)] string newpassword);

        // Method to change a Password of a user on a remote machine.
        public static uint ChangeUserPassword(string computer, string userName,
            string oldPassword, string newPassword)
        {
            return NetUserChangePassword(computer, userName,
                oldPassword, newPassword);
        }

        // Method used to create a new user on a Remote Machine
        private static uint CreateUser(string computer, string userName,
            string password)
        {
            const int UF_DONT_EXPIRE_PASSWD = 0x10000;
            const int UF_ACCOUNTDISABLE = 0x000002;

            const int USER_PRIV_GUEST = 0; // lmaccess.h:656
            const int USER_PRIV_USER = 1;   // lmaccess.h:657         
            const int USER_PRIV_ADMIN = 2;  // lmaccess.h:658  

            USER_INFO_1 userinfo = new USER_INFO_1()
            {
                sComment = "New User",
                sUsername = userName,
                sPassword = password,
                sHome_Dir = "",
                sScript_Path = "",
                uiPriv = USER_PRIV_USER,
                uiFlags = UF_DONT_EXPIRE_PASSWD
            };

            uint output;
            NetUserAdd(computer, 1, ref userinfo, out output);
            return output;
        }


        public static bool isReachableAndValidNetbios(string machine)
        {
            IPAddress IP = MachineInfo.ping(machine, false);
            string netbios = IP == null ? "" : getNetbiosName(machine);
            if (netbios == null) netbios = "";
            if (netbios.Length == 0 || !netbios.ToUpper().Equals(machine.ToUpper()))
                return false;
            else
                return true;
        }


        public static string getCurrentUsers(string targethost)
        {
            //Console.Write("Trying to get Current Users on " + targethost + "...");
            ManagementScope scope = null; try
            {
                scope = connectWMI(targethost);
                string currentUsers = Win32_LogonSession.getCurrentUsers(scope, true);
                scope.Path = new ManagementPath();
                return currentUsers;
            }
            catch (Exception e)
            {
                if (scope != null) scope.Path = new ManagementPath();
                //Console.WriteLine("Failed to get Current Users on " + targethost + "\n" + e.ToString());
                return null;
            }
        }



        public class Win32_LogonSession
        {
            public string AuthenticationPackage;
            public string LogonID;
            public LogonEventType LogonType;
            public string Name;
            public DateTime StartTime;

            public enum LogonEventType
            {
                System = 0,
                Interactive,
                Network,
                Batch,
                Service,
                Proxy,
                Unlock,
                NetworkClearText,
                NewCredentials,
                RemoteInteractive,
                CachedInteractive,
                CachedRemoteInteractive,
                CachedUnlock
            }

            public static string getCurrentUsers(ManagementScope scope, bool interactive_only)
            {
                StringBuilder users = new StringBuilder();
                string query = "Select * From Win32_LogonSession";
                if (interactive_only) query += " WHERE LogonType = 2";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));
                ManagementObjectCollection results = searcher.Get();
                List<Win32_LogonSession> list = new List<Win32_LogonSession>(results.Count);
                Dictionary<string, string> userTable = GetLoggedOnUsersTable(scope);
                foreach (ManagementObject logonCurrent in results)
                {
                    string LogonID = logonCurrent["LogonID"].ToString();
                    //Console.WriteLine("=== LogonID = " + LogonID + " ===");
                    Win32_LogonSession entry = new Win32_LogonSession();
                    entry.LogonID = LogonID;
                    if (userTable.ContainsKey(LogonID))
                    {
                        entry.Name = (string)userTable[entry.LogonID];
                        //Console.WriteLine(entry.Name);
                        if (users.ToString().IndexOf(entry.Name) < 0) users.AppendLine(entry.Name);
                    }
                }
                return users.ToString();
            }

            private static Dictionary<string, string> GetLoggedOnUsersTable(ManagementScope scope)
            {
                string query = "Select * From Win32_LoggedOnUser";
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, new ObjectQuery(query));
                ManagementObjectCollection results = searcher.Get();
                Dictionary<string, string> table = new Dictionary<string, string>(results.Count);
                foreach (ManagementObject currentObject in results)
                {
                    string account = GetUser((string)currentObject["Antecedent"]);
                    string session = GetLogonID((string)currentObject["Dependent"]);
                    //Console.WriteLine("=== LoggedOnUser.Account = " + account + " .Session = " + session + " ===");
                    table.Add(session, account);
                }
                return table;
            }

            private static string GetLogonID(string propertyText)
            {
                string pattern = "LogonId=\"(?<id>\\d+)\"";
                Match matchID = Regex.Match(propertyText, pattern);
                if (matchID.Success)
                {
                    return matchID.Groups["id"].Value;
                }
                else
                {
                    return "";
                }
            }

            private static string GetUser(string propertyText)
            {
                string pattern = "Domain=\"(?<domain>[A-Za-z\\d_-]+)\"|Name=\"(?<name>[A-Za-z\\d\\s_-]+)\"";
                string domain = "";
                string name = "";
                foreach (Match matchCurrent in Regex.Matches(propertyText, pattern))
                {
                    string fullText = matchCurrent.Groups[0].Value;
                    if (fullText.StartsWith("Domain"))
                    {
                        domain = matchCurrent.Groups["domain"].Value;
                    }
                    else
                    {
                        name = matchCurrent.Groups["name"].Value;
                    }
                }
                if (domain.Length == 0) return name;
                else return domain + "\\" + name;
            }

        }//eosc





        public static bool reassign(string machine, string owner, DirectoryEntry machinede, DirectoryEntry Workstations)
        {
            try
            {
                if (!UsersInfo.enableAD(machinede, true)) throw new Exception("Failed to enable AD account.");
                if (owner != null && owner.Length > 0)
                {
                    machinede.InvokeSet("Description", owner);
                    machinede.CommitChanges();
                }

                clearSeclogs(machine, null, null);
                
                machinede.MoveTo(Workstations);
                Workstations.Close();
                machinede.Close();

                return true;
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.ToString());
                return false;
            }
        }















        public static Object[] getAVInfo(string machine, bool console)
        {
            StringBuilder result = new StringBuilder();
            result.AppendLine(" Microsoft Forefront / Security Essentials Antivirus & Antispyware Details:");
            result.AppendLine("============================================================================");
            result.AppendLine("Machine Name: " + machine);
            try
            {
                bool remregrun = MachineInfo.isServiceRunning(machine, "RemoteRegistry");
                if (!remregrun) MachineInfo.StartService(machine, "RemoteRegistry", 30000);
                if (!MachineInfo.isServiceRunning(machine, "RemoteRegistry")) throw new Exception("Remote Registry unavailable.");
                RegistryKey k = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machine)
                    .OpenSubKey(@"SOFTWARE\Microsoft\Microsoft Antimalware\Signature Updates");

                string AVSignatureVersion = k.GetValue("AVSignatureVersion") != null ? k.GetValue("AVSignatureVersion").ToString().Trim() : "";
                string ASSignatureVersion = k.GetValue("ASSignatureVersion") != null ? k.GetValue("ASSignatureVersion").ToString().Trim() : "";
                byte[] AVSignatureApplied_bytes = k.GetValue("AVSignatureApplied") != null ? (byte[])k.GetValue("AVSignatureApplied") : new byte[] { };
                byte[] ASSignatureApplied_bytes = k.GetValue("ASSignatureApplied") != null ? (byte[])k.GetValue("ASSignatureApplied") : new byte[] { };
                string SignatureLocation = k.GetValue("SignatureLocation") != null ? k.GetValue("SignatureLocation").ToString().Trim() : "";

                k.Close();
                if (!remregrun) MachineInfo.StopService(machine, "RemoteRegistry", 30000);

                DateTime AVSignatureApplied = DateTime.FromBinary(BitConverter.ToInt64(AVSignatureApplied_bytes, 0)).AddYears(1600);
                DateTime ASSignatureApplied = DateTime.FromBinary(BitConverter.ToInt64(ASSignatureApplied_bytes, 0)).AddYears(1600);
                result.AppendLine("AV current version: " + AVSignatureVersion);
                result.AppendLine("AS current version: " + ASSignatureVersion);
                result.AppendLine("When AV Signature Applied: "
                    //+ BitConverter.ToString(AVSignatureApplied_bytes).Replace('-', ' ') + " "
                    + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", AVSignatureApplied));
                result.AppendLine("When AS Signature Applied: "
                    //+ BitConverter.ToString(ASSignatureApplied_bytes).Replace('-', ' ') + " "
                    + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", ASSignatureApplied));



                /*
                DateTime AVBase_createdon = getTimestamp(@"\\" + machine + @"\" + SignatureLocation.Replace(':', '$') + @"\" + "mpavbase.vdm");
                //DateTime AVDelta_createdon = getTimestamp(@"\\" + machine + @"\" + SignatureLocation.Replace(':', '$') + @"\" + "mpavdlta.vdm");
                //DateTime ASBase_createdon = getTimestamp(@"\\" + machine + @"\" + SignatureLocation.Replace(':', '$') + @"\" + "mpasbase.vdm");
                //DateTime ASDelta_createdon = getTimestamp(@"\\" + machine + @"\" + SignatureLocation.Replace(':', '$') + @"\" + "mpasdlta.vdm");
                */
                DateTime AVBase_lastchecked = File.GetLastWriteTime(@"\\" + machine + @"\" + SignatureLocation.Replace(':', '$') + @"\" + "mpavbase.vdm");
                DateTime AVDelta_lastchecked = File.GetLastWriteTime(@"\\" + machine + @"\" + SignatureLocation.Replace(':', '$') + @"\" + "mpavdlta.vdm");
                DateTime ASBase_lastchecked = File.GetLastWriteTime(@"\\" + machine + @"\" + SignatureLocation.Replace(':', '$') + @"\" + "mpasbase.vdm");
                DateTime ASDelta_lastchecked = File.GetLastWriteTime(@"\\" + machine + @"\" + SignatureLocation.Replace(':', '$') + @"\" + "mpasdlta.vdm");
                

                result.AppendLine("SignatureLocation: " + SignatureLocation);

                //result.AppendLine("AV Base created on: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", AVBase_createdon));
                //result.AppendLine("AV Delta created on: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", AVDelta_createdon));
                //result.AppendLine("AS Base created on: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", ASBase_createdon));
                //result.AppendLine("AS Delta created on: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", ASDelta_createdon));

                result.AppendLine("AV Base last checked: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", AVBase_lastchecked));
                result.AppendLine("AV Delta last checked: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", AVDelta_lastchecked));
                result.AppendLine("AS Base last checked: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", ASBase_lastchecked));
                result.AppendLine("AS Delta last checked: " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy hh:mm}", ASDelta_lastchecked));

                if (console) Console.WriteLine(result.ToString());
                return new Object[] {AVSignatureApplied, result.ToString()};
            }
            catch (Exception e)
            {
                result.AppendLine("Error: " + e.Message);
                result.AppendLine(e.ToString());
                if (console) Console.WriteLine(result.ToString());
                return new Object[] {DateTime.MinValue, result.ToString()};
            }
        }
    /*

        static DateTime getTimestamp(string f)
        {
            try
            {
                Console.WriteLine("getTimestamp of " + f);
                if (!File.Exists(f)) throw new Exception("File not found.");
                FileStream fs = new FileStream(f, FileMode.Open);
                byte[] b = new byte[300];
                fs.Seek(fs.Length - b.Length, SeekOrigin.Begin);
                int n = fs.Read(b, 0, b.Length);
                fs.Close();
                if (n < 300) throw new Exception("Not enough data.");
                string s = System.Text.Encoding.UTF8.GetString(b, 0, n);
                //Console.WriteLine(s);
                int i = s.IndexOf("Microsoft Time-Stamp PCA 20100");
                if (i < 0) throw new Exception("No timestamp found");
                if (i > 196) throw new Exception("Timestamp invalid");
                string s2 = s.Substring(i + 92, 12);
                Console.WriteLine("Timestamp="+s2);
                return DateTime.MinValue;
     
            }
            catch (Exception e)
            {
                Console.WriteLine("getTimestamp of " + f + ": Error: " + e.Message);
                return DateTime.MinValue;
            }
        }

*/






        static string queriestmp = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmqueries.wql.tmp");

        static Hashtable COMPREPLACE = new Hashtable();
        static bool COMPREPLACE_initialized = COMPREPLACE_initialize();
        static bool COMPREPLACE_initialize()
        {
            string lastline = "";
            try
            {
                if (!AuditSec.exportResource("sccmqueries", queriestmp, true))
                    throw new Exception("File installation failure");

                string queryname = null;
                StringBuilder queryargs = new StringBuilder();
                StringBuilder querydef = new StringBuilder();
                if (File.Exists(queriestmp))
                    foreach (string line in File.ReadAllLines(queriestmp))
                    {
                        lastline = line;
                        if (!line.StartsWith("#") && line.Trim().Length > 0)
                        {
                            if (line.StartsWith("¤"))
                            {
                                if (queryname != null && queryname.Length > 0
                                    && querydef != null && querydef.Length > 0)
                                {
                                    if (queryname.ToUpper().StartsWith("_TEMPLATE_ - BASE VARIABLES"))
                                    {
                                        string v = getArg(queryargs, "computerMakerReplace");
                                        if (v != null) COMPREPLACE["computerMakerReplace"] = v;
                                        v = getArg(queryargs, "computerModelReplace");
                                        if (v != null) COMPREPLACE["computerModelReplace"] = v;
                                    }
                                }
                                queryname = null;
                                queryargs.Clear();
                                querydef.Clear();
                            }
                            else
                            {
                                if (queryname == null)
                                    queryname = line;
                                else if (querydef.Length > 0)
                                    querydef.AppendLine(line);
                                else if ("SELECT".Equals(line.Split(new char[] { ' ', '\t' }, 2,
                                    StringSplitOptions.RemoveEmptyEntries)[0].ToUpper()))
                                    querydef.AppendLine(line);
                                else if ("WHERE".Equals(line.Split(new char[] { ' ', '\t' }, 2,
                                    StringSplitOptions.RemoveEmptyEntries)[0].ToUpper()))
                                    querydef.AppendLine(line);
                                else
                                    queryargs.Append(line + " ");
                            }
                        }
                    }
                if (queryname != null && queryname.Length > 0 && querydef != null && querydef.Length > 0)
                {
                    while (queryargs.Length > 0 && (
                        queryargs[0] == ' ' || queryargs[0] == '\n'
                        || queryargs[0] == '\t' || queryargs[0] == ','))
                        queryargs.Remove(0, 1);
                }
                Console.WriteLine("Replace Strings loaded from file: \"" + queriestmp + "\""
                //    + "\ncomputerMakerReplace=" + (COMPREPLACE.Contains("computerMakerReplace") ? COMPREPLACE["computerMakerReplace"] : "")
                //    + "\ncomputerModelReplace=" + (COMPREPLACE.Contains("computerModelReplace") ? COMPREPLACE["computerModelReplace"] : "")
                );
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot initialize Replace Strings, failed to load from file: \"" + queriestmp + "\""
                + e.Message + "\n" + "lastline=" + lastline + "\n"
                + e.StackTrace);
                return false;
            }
        }

        static Hashtable OSREPLACE = new Hashtable();
        static bool OSREPLACE_initialized = OSREPLACE_initialize();
        static bool OSREPLACE_initialize()
        {
            string lastline = "";
            try
            {
                if (!AuditSec.exportResource("sccmqueries", queriestmp, true))
                    throw new Exception("File installation failure");

                string queryname = null;
                StringBuilder queryargs = new StringBuilder();
                StringBuilder querydef = new StringBuilder();
                if (File.Exists(queriestmp))
                    foreach (string line in File.ReadAllLines(queriestmp))
                    {
                        lastline = line;
                        if (!line.StartsWith("#") && line.Trim().Length > 0)
                        {
                            if (line.StartsWith("¤"))
                            {
                                if (queryname != null && queryname.Length > 0
                                    && querydef != null && querydef.Length > 0)
                                {
                                    if (queryname.ToUpper().StartsWith("_TEMPLATE_ - BASE VARIABLES"))
                                    {
                                        string v = getArg(queryargs, "OSReplace");
                                        if (v != null) OSREPLACE["OSReplace"] = v;
                                        v = getArg(queryargs, "ServerReplace");
                                        if (v != null) OSREPLACE["ServerReplace"] = v;
                                        v = getArg(queryargs, "PlatformReplace");
                                        if (v != null) OSREPLACE["PlatformReplace"] = v;
                                        v = getArg(queryargs, "SPReplace");
                                        if (v != null) OSREPLACE["SPReplace"] = v;
                                    }
                                }
                                queryname = null;
                                queryargs.Clear();
                                querydef.Clear();
                            }
                            else
                            {
                                if (queryname == null)
                                    queryname = line;
                                else if (querydef.Length > 0)
                                    querydef.AppendLine(line);
                                else if ("SELECT".Equals(line.Split(new char[] { ' ', '\t' }, 2,
                                    StringSplitOptions.RemoveEmptyEntries)[0].ToUpper()))
                                    querydef.AppendLine(line);
                                else if ("WHERE".Equals(line.Split(new char[] { ' ', '\t' }, 2,
                                    StringSplitOptions.RemoveEmptyEntries)[0].ToUpper()))
                                    querydef.AppendLine(line);
                                else
                                    queryargs.Append(line + " ");
                            }
                        }
                    }
                if (queryname != null && queryname.Length > 0 && querydef != null && querydef.Length > 0)
                {
                    while (queryargs.Length > 0 && (
                        queryargs[0] == ' ' || queryargs[0] == '\n'
                        || queryargs[0] == '\t' || queryargs[0] == ','))
                        queryargs.Remove(0, 1);
                }
                Console.WriteLine("Replace Strings loaded from file: \"" + queriestmp + "\""
                    //    + "\ncomputerMakerReplace=" + (COMPREPLACE.Contains("computerMakerReplace") ? COMPREPLACE["computerMakerReplace"] : "")
                    //    + "\ncomputerModelReplace=" + (COMPREPLACE.Contains("computerModelReplace") ? COMPREPLACE["computerModelReplace"] : "")
                );
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot initialize Replace Strings, failed to load from file: \"" + queriestmp + "\""
                + e.Message + "\n" + "lastline=" + lastline + "\n"
                + e.StackTrace);
                return false;
            }
        }
       

        static string computerReplace(string argName_, string s_)
        {
            string argName = "computer" + argName_ + "Replace";
            string s = s_;
            try
            {
                if (!COMPREPLACE.Contains(argName)) return s;
                string r = (string)COMPREPLACE[argName];
                string[] w = r.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < w.Length; i++)
                {
                    string wi = w[i];
                    if (wi.ToUpper().Equals("UCASE")) s = s.ToUpper();
                    else if (wi.ToUpper().Equals("LCASE")) s = s.ToLower();
                    else if (wi.ToUpper().Equals("REPLACE"))
                    {
                        if (i + 2 >= w.Length) return s;
                        string arg1 = w[i + 1].Replace("\\space", " ").Replace("\\comma", ","),
                            arg2 = w[i + 2].Replace("\\space", " ").Replace("\\comma", ",");
                        i += 2;
                        if (arg1.StartsWith("\""))
                        {
                            arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                        }
                        if (arg2.StartsWith("\""))
                        {
                            arg2 = arg2.Substring(1);
                            if (arg2.EndsWith("\"")) arg2 = arg2.Substring(0, arg2.Length - 1);
                            arg2 = arg2.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
                        }
                        s = Regex.Replace(s, arg1, arg2);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("REPLACE " + argName + ": " + e.ToString());
            }
            return s;
        }

        static string OSReplace(string s_)
        {
            return OSReplace("OSReplace", s_);
        }
        static string ServerReplace(string s_)
        {
            return OSReplace("ServerReplace", s_);
        }
        static string PlatformReplace(string s_)
        {
            return OSReplace("PlatformReplace", s_);
        }
        static string SPReplace(string s_)
        {
            return OSReplace("SPReplace", s_);
        }
        static string OSReplace(string argName, string s_)
        {
            string s = s_;
            try
            {
                if (!OSREPLACE.Contains(argName)) return s;
                string r = (string)OSREPLACE[argName];
                string[] w = r.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < w.Length; i++)
                {
                    string wi = w[i];
                    if (wi.ToUpper().Equals("UCASE")) s = s.ToUpper();
                    else if (wi.ToUpper().Equals("LCASE")) s = s.ToLower();
                    else if (wi.ToUpper().Equals("REPLACE"))
                    {
                        if (i + 2 >= w.Length) return s;
                        string arg1 = w[i + 1].Replace("\\space", " ").Replace("\\comma", ","),
                            arg2 = w[i + 2].Replace("\\space", " ").Replace("\\comma", ",");
                        i += 2;
                        if (arg1.StartsWith("\""))
                        {
                            arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                        }
                        if (arg2.StartsWith("\""))
                        {
                            arg2 = arg2.Substring(1);
                            if (arg2.EndsWith("\"")) arg2 = arg2.Substring(0, arg2.Length - 1);
                            arg2 = arg2.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\t", "\t");
                        }
                        s = Regex.Replace(s, arg1, arg2);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("REPLACE " + argName + ": " + e.ToString());
            }
            return s;
        }

        public static string getArg(StringBuilder args__, string argName)
        {
            string args_ = args__.ToString();
            while (args_.Length > 0 && (args_[0] == ' ' || args_[0] == '\n' || args_[0] == '\t' || args_[0] == ','))
                args_.Remove(0, 1);
            string[] args = args_.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (args == null) return "";
            foreach (string arg in args)
            {
                string[] kv = arg.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                string k = kv.Length > 0 ? kv[0].Trim() : "";
                string v = kv.Length > 1 ? kv[1].Trim() : "";
                if (argName.ToUpper().Equals(k.ToUpper()))
                    return v;
            }
            return null;
        }




        static string filextype = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "filextype.txt");

        static Dictionary<string, string> FILEXTYPE = new Dictionary<string, string>();
        static public Dictionary<string, List<string>> TYPEFILEX = new Dictionary<string, List<string>>();

        static bool FILEXTYPE_initialized = FILEXTYPE_initialize();
        
        static bool FILEXTYPE_initialize()
        {
            TYPEFILEX.Clear();
            string lastline = "";
            try
            {
                if (!AuditSec.exportResource("filextype", filextype, true))
                    throw new Exception("File installation failure");

                if (File.Exists(filextype))
                    foreach (string line in File.ReadAllLines(filextype))
                        if (!(lastline = line).Contains("="))
                        {
                            if (line.Trim().Length > 0)
                                Console.WriteLine("File Extension/Type: Incorrect Line: " + line);
                        }
                        else
                        {
                            string invalidchars = string.Concat(line.ToCharArray().Where(c => !Char.IsLetterOrDigit(c) && !Char.IsWhiteSpace(c)
                                && c != '=' && c != ',' && c != '.' && c != '*' && c != '?'
                                && c != '_' && c != '-' && c != '+' && c != '!' ));
                            if (invalidchars.Length > 0)
                                Console.WriteLine("File Extension/Type: Incorrect Line: " + line + "\nInvalid characters: " + invalidchars);

                            string type = line.Substring(0, line.IndexOf('=')).Trim();
                            List<string> exts = line.Substring(line.IndexOf('=') + 1)
                                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(ext => ext.Trim().ToUpper()).Where(ext => ext.Length > 0).ToList();

                            List<string> extsdup = exts.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key).ToList();
                            if (extsdup.Count() > 0)
                                Console.WriteLine(extsdup.Select(ext => ext + " extension duplicates found in type: " + type).Aggregate((l1, l2) => l1 + "\n" + l2));

                            exts = exts.Distinct().ToList();

                            List<string> extstodel = new List<string>();
                            foreach (string ext in exts)
                                foreach (string type_ in TYPEFILEX.Keys) foreach (string ext_ in TYPEFILEX[type_])
                                        if (ext_ == ext)
                                        {
                                            Console.WriteLine(ext + " extension found in multiple types: " + type + ", " + type_);
                                            extstodel.Add(ext);
                                        }
                            exts.RemoveAll(ext => extstodel.Contains(ext));
                            TYPEFILEX[type] = exts;

                        }

                TYPEFILEX = TYPEFILEX.Where(kv => kv.Value.Count() > 0).ToDictionary(x => x.Key, x => x.Value);
                if (TYPEFILEX.Count() == 0) throw new Exception("Type to File Extension table: empty.");
                //Console.WriteLine("Type to File Extension table: " + TYPEFILEX.ToList().Select(kv => kv.Key + ": " + kv.Value.Aggregate((v1, v2) => v1 + ", " + v2))
                //    .Aggregate((l1, l2) => l1 + "\n" + l2) + "\n");

                FILEXTYPE = TYPEFILEX
                    .SelectMany(kv => kv.Value.Select(ext => new { Key = ext, Value = kv.Key }))
                    .ToDictionary(x => x.Key, x => x.Value);
                //Console.WriteLine("File Extension to Type table: " + FILEXTYPE.ToList().Select(kv => kv.Key + ": " + kv.Value)
                //    .Aggregate((l1, l2) => l1 + "\n" + l2) + "\n");

                Console.WriteLine("File Extension/Type loaded from file: \"" + filextype + "\"\n"
                                +"Types: " + TYPEFILEX.Count() + "\nExtensions: " + FILEXTYPE.Count());
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot initialize File Extension/Type, failed to load from file: \"" + filextype + "\""
                + e.Message + "\n" + "lastline=" + lastline + "\n"
                + e.StackTrace);
                return false;
            }
        }




        
        //static bool testGetProfilesSize = testGetProfilesSize_doit();
        static bool testGetProfilesSize_doing
            //= true;
            = false;
        static int testGetProfilesSize_modulo
            //= 0;
            = 100;
        static bool testGetProfilesSize_doit()
        {
            testGetProfilesSize_doing = true;
            //Console.WriteLine(GetProfilesSize("localhost", 100));
            //Console.WriteLine(GetProfilesSize("par001174", 100));
            //Console.WriteLine(GetProfilesSizeAsString("c:", 100));

            var ret = GetProfilesSize("par001054", new Files_Bytes());
            var arrays = GetProfilesSizeAsObjectArrayList(ret, "par001054");
            return true;
        }
        

        static public Dictionary<string, Tuple<int, long, DateTime, Dictionary<string, long>>> GetProfilesSize(string machine, Files_Bytes total_cb)
        {
            string users = (machine.EndsWith(":") ? "" : @"\\" + machine + @"\c$") + @"\Users";
            if (!Directory.Exists(users)) return new Dictionary<string, Tuple<int, long, DateTime, Dictionary<string, long>>>();
            total_cb.connected = true;

            var profiles = Directory.EnumerateDirectories(users)
                .Where(d => !new DirectoryInfo(d).Attributes.HasFlag(FileAttributes.ReparsePoint))
                .Select(p => new { path = p, user = new FileInfo(p).Name.ToUpper() })
                .Select(pu => new { path = pu.path, user = pu.user, cb = new Files_Bytes(pu.user) })
                .Where(pucb => pucb.user != "ADMINISTRATOR" && pucb.user != "ALL USERS" && pucb.user != "DEFAULT" && pucb.user != "DEFAULT USER" && pucb.user != "PUBLIC"
                && pucb.user != "USERADMIN"//MYCOMPANY SPECIFIC
                );

            Console.WriteLine("Examinating " + profiles.Count() + " profiles in " + machine + "...");
            total_cb.markers.AddRange(profiles.Select(pucb => pucb.user));

            return (testGetProfilesSize_doing ? profiles : profiles.AsParallel())
                .Select(pucb => new { user = pucb.user, cb = pucb.cb, byExt = GetDirSizeByExt(pucb.path, pucb.cb, total_cb) })
                .ToDictionary(ucbe => ucbe.user, ucbe => new Tuple<int, long, DateTime, Dictionary<string, long>>(ucbe.cb.files, ucbe.cb.bytes, ucbe.cb.last, ucbe.byExt));
        }


        static public List<object[]> GetProfilesSizeAsObjectArrayList(string machine, Files_Bytes total_cb)
        {
            return GetProfilesSizeAsObjectArrayList(GetProfilesSize(machine, total_cb), machine);
        }
        static public List<object[]> GetProfilesSizeAsObjectArrayList(Dictionary<string, Tuple<int, long, DateTime, Dictionary<string, long>>> ret, string machine)
        {
            return ret.Count() == 0 ? new List<object[]>()
                : ret.Select(kv => new object[] { machine, kv.Key, DateTime.Now.ToString("yyyy/MM/dd"),
                    kv.Value.Item1, kv.Value.Item2 / (1024 * 1024), kv.Value.Item3.ToString("yyyy/MM/dd") }
                .Concat(TYPEFILEX.Keys.OrderBy(type => type).Select(type => kv.Value.Item4.ContainsKey(type) ?
                    kv.Value.Item4[type] / (1024 * 1024) : 0).Select(x => x as object).ToArray())
                .ToArray())
                .Cast<object[]>().ToList();
        }
        
        static public string GetProfilesSizeAsString(string machine, int min_MB, Files_Bytes total_cb)
        {
            return GetProfilesSizeAsString(GetProfilesSize(machine, total_cb), min_MB);
        }

        static public string NOPROFILEFOUND = "No profile found.";
        static public string GetProfilesSizeAsString(Dictionary<string, Tuple<int, long, DateTime, Dictionary<string, long>>> ret, int min_MB)
        {
            return ret.Count() == 0 ? NOPROFILEFOUND : ret.ToList()
                .Select(kv => "User: " + kv.Key
                    + "\n\tTotal Files  : " + kv.Value.Item1.ToString("#,0")
                    + "\n\tTotal Size   : " + (kv.Value.Item2 / (1024 * 1024)).ToString("#,0") + "MB"
                    + "\n\tLast Modified: " + kv.Value.Item3.ToString("yyyy/MM/dd")
                    + (kv.Value.Item4.ToList().Where(kv_ => kv_.Value > (min_MB * 1024 * 1024)).Count() == 0
                    ? "" : "\n    Extensions:\n"
                    + kv.Value.Item4.ToList().Where(kv_ => kv_.Value > (min_MB * 1024 * 1024)) // display types that have at least min_MB size
                                            .OrderByDescending(kv_ => kv_.Value)
                                            .Select(kv_ => "\t" + kv_.Key + ": " + (kv_.Value / (1024 * 1024)).ToString("#,0") + "MB")
                                            .Aggregate((l1, l2) => l1 + "\n" + l2) + "\n"))
                .Aggregate((l1, l2) => l1 + "\n" + l2);
        }

        static public IEnumerable<string> GetFilesFromDir(string dir)
        {
            return Directory.EnumerateFiles(dir).Concat(
                   Directory.EnumerateDirectories(dir)
                            .SelectMany(subdir => GetFilesFromDir(subdir)));
        }

        /*
        static public long GetDirSize(string dir)
        {
            return
                Directory.EnumerateFiles(dir).AsParallel()//.WithDegreeOfParallelism(8)
                .Select(f => new FileInfo(f).Length).Aggregate((s1, s2) => s1 + s2)
            + Directory.EnumerateDirectories(dir).AsParallel()//.WithDegreeOfParallelism(4)
            .Select(d => GetDirSize(d)).Aggregate((s1, s2) => s1 + s2);
        }
        */

        public class Files_Bytes
        {
            public Files_Bytes() { }
            public Files_Bytes(string marker0)
            {
                markers.Add(marker0);
            }
            public int files = 0;
            public long bytes = 0;
            public DateTime scandate = DateTime.Now;
            public DateTime last = DateTime.MinValue;
            public bool aborted = false;
            public bool connected = false;
            public List<string> markers = new List<string>();
            public FileInfo GetFileInfo(string f, Files_Bytes total_cb)
            {
                FileInfo fi = new FileInfo(f);
                if (!fi.Name.ToUpper().StartsWith("NTUSER.DAT") && !fi.Name.ToUpper().StartsWith("USRCLASS.DAT"))
                {
                    files++; bytes += fi.Length;
                    total_cb.files++; total_cb.bytes += fi.Length;

                    if (fi.LastWriteTime > last) last = fi.LastWriteTime;
                    if (fi.LastWriteTime > total_cb.last) total_cb.last = fi.LastWriteTime;

                    if (testGetProfilesSize_doing && (testGetProfilesSize_modulo == 0 || files % testGetProfilesSize_modulo == 0))
                        Console.WriteLine((markers.Count > 0 ? markers.Aggregate((x, y) => x + "+" + y) : "")
                            + " Files:" + files + " Size:" + bytes + " " + last.ToString("yyyy/MM/dd") + "\n\t\t\t" + f
                            );
                    }
                return fi;
            }
        }

        static public Dictionary<string, long> GetDirSizeByExt(string dir, Files_Bytes cb, Files_Bytes total_cb)
        {
            if (total_cb.aborted) return new Dictionary<string, long>();
            try
            {
                List<Dictionary<string, long>> dicts = new List<Dictionary<string, long>>();
                dicts.Add(
                    Directory.EnumerateFiles(dir)
                        .Where(f => !new FileInfo(f).Attributes.HasFlag(FileAttributes.ReparsePoint))
                        .Select(f => cb.GetFileInfo(f, total_cb))
                        .GroupBy(fi => getFileType(fi), fi => fi.Length)
                        .ToDictionary(g => g.Key, g => g.Aggregate((s1, s2) => s1 + s2)));

                dicts.AddRange(
                    Directory.EnumerateDirectories(dir)//.AsParallel()//.WithDegreeOfParallelism(4)
                    .Where(d => !new DirectoryInfo(d).Attributes.HasFlag(FileAttributes.ReparsePoint))
                    .Select(d => GetDirSizeByExt(d, cb, total_cb)));

                return dicts.SelectMany(dict => dict.ToList()).GroupBy(x => x.Key, x => x.Value)
                    .ToDictionary(g => g.Key, g => g.Aggregate((s1, s2) => s1 + s2));
            }
            catch (Exception e)
            {
                //Console.WriteLine("GetDirSizeByExt " + dir + ": error: " + e.ToString());
                Console.WriteLine("GetDirSizeByExt error: " + e.Message);
                return new Dictionary<string, long>();
            }
        }



        static string getFileType(FileInfo fi)
        {

            foreach (string EXT in FILEXTYPE.Keys)
            {
                string type = FILEXTYPE[EXT];
                string NAME = fi.Name.ToUpper();
                if (EXT.Contains("*") || EXT.Contains("?"))
                {
                    string EXT_ = ("." + EXT).Replace(".", @"\.").Replace("-", @"\-").Replace("+", @"\+").Replace("!", @"\!").Replace("*", "[^.]*").Replace("?", "[^.]");
                    if (Regex.IsMatch(NAME, EXT_))
                        return type;
                }
                else
                {
                    string EXT_ = "." + EXT;
                    if (NAME.EndsWith(EXT_))
                        return type;
                }
            }
            string ext = fi.Extension.ToUpper();
            if (ext.StartsWith(".")) ext = ext.Substring(1);
            return "." + ext;
        }









    }//eoc
}


