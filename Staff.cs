using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices;
using System.Threading;

namespace AuditSec
{
    class Staff
    {
        const int
            AD_Retry = 10,
            AD_Wait = 10;

        const string OUMaskBox = "^[A-Z]{3}$";

        public static Hashtable Counts_Users = null;
        public static Hashtable Counts_ITOps = null;
        public static Hashtable Members_ITOps = null;
        public static Hashtable Members_ITOpsMgmt = null;

        public static Hashtable getCounts_Users(CheckedListBox SVDBox)
        {
            return getCounts_Users(SVDBox, null);
        }

        public static Hashtable getCounts_ITOps(CheckedListBox SVDBox)
        {
            return getCounts_ITOps(SVDBox, null);
        }

        public static Hashtable getMembers_ITOps(CheckedListBox SVDBox)
        {
            return getMembers_ITOps(SVDBox, null);
        }

        public static Hashtable getMembers_ITOpsMgmt(CheckedListBox SVDBox)
        {
            return getMembers_ITOpsMgmt(SVDBox, null);
        }

        public static Hashtable getCounts_Users(CheckedListBox SVDBox, Func<string, bool> report)
        {
            return Counts_Users = getCounts(Counts_Users, null, "Users",
                SVDBox, report);
        }


        static string ITOps_Filter = "(department=IT OPERATIONS)(!title=*Manager)(!title=*Director)(!title=*VP)";
        static string ITOpsMgmt_Filter = "(department=IT OPERATIONS)(|(title=*Manager)(title=*Director))";

        public static Hashtable getCounts_ITOps(CheckedListBox SVDBox, Func<string, bool> report)
        {
            return Counts_ITOps = getCounts(Counts_ITOps, ITOps_Filter, "ITOps",
                SVDBox, report);
        }

        public static Hashtable getMembers_ITOps(CheckedListBox SVDBox, Func<string, bool> report)
        {
            return Members_ITOps = getMembers(Members_ITOps, ITOps_Filter, "ITOps Members",
                SVDBox, report);
        }

        public static Hashtable getMembers_ITOpsMgmt(CheckedListBox SVDBox, Func<string, bool> report)
        {
            return Members_ITOpsMgmt = getMembers(Members_ITOpsMgmt, ITOpsMgmt_Filter, "ITOps Management Members",
                SVDBox, report);
        }





        public static Hashtable getCounts(Hashtable counts, string filter_, string label,
            CheckedListBox SVDBox, Func<string, bool> report)
        {
            if (counts != null) return counts;
            counts = new Hashtable();
            new List<string>(SVDBox.Items.OfType<string>()).AsParallel().ForAll(SVD_ =>
            {
                Random rand = new Random(); int retry = AD_Retry; while (retry-- > 0) try
                    {
                        string SVD = SVD_;
                        if (SVD.IndexOf('-') >= 0) SVD = SVD.Substring(SVD.IndexOf('-') + 1);
                        string filter = "(&(ObjectClass=user)(!ObjectClass=computer)(employeeID=*)"
                            //+ "(!userAccountControl=514)(!userAccountControl=66050)"
                            + "(|(l=" + SVD + ")(st=" + SVD + "))"
                            + (filter_ == null ? "" : filter_) + ")";

                        DomainCollection dc = Forest.GetCurrentForest().Domains;
                        Domain[] domains = new Domain[dc.Count]; dc.CopyTo(domains, 0);
                        string current = Domain.GetCurrentDomain().Name;
                        domains.All(domain =>
                        {
                            Random rand2 = new Random(); int retry2 = AD_Retry; while (retry2-- > 0) try
                                {
                                    SearchResultCollection col = new DirectorySearcher(domain.GetDirectoryEntry(), filter,
                                        new string[] { "distinguishedName" }, SearchScope.Subtree).FindAll();
                                    //int DomainCount = col.Count;
                                    int DomainCount = 0;
                                    foreach (SearchResult r in col)
                                    {
                                        string dn = r.Properties["distinguishedName"].Count > 0 ? r.Properties["distinguishedName"][0].ToString().ToLower() : "";
                                        if (!dn.Contains("terminated")) DomainCount++;
                                    }
                                    lock (counts)
                                    {
                                        int SVDCount = counts.Contains(SVD_) ? ((int)counts[SVD_] == -1 ? -1 : (int)counts[SVD_] + DomainCount) : DomainCount;
//Console.WriteLine("\t" + SVD + " in " + domain.Name + ": " + DomainCount + ", in total:" + SVDCount);
                                        if (counts.Contains(SVD_)) counts.Remove(SVD_);
                                        counts.Add(SVD_, SVDCount);
                                    }
                                    retry2 = 0;
                                    return true;
                                }
                                catch (Exception ee)
                                {
                                    if (retry2 <= 0)
                                    {
                                        if (report != null) report(SVD_ + "-" + label + "=Error:" + ee.Message);
                                        lock (counts)
                                        {
                                            if (counts.Contains(SVD_)) counts.Remove(SVD_);
                                            counts.Add(SVD_, -1);
                                        }
                                    }
                                    else
                                    {
                                        if (report != null) report("Oups" + (AD_Retry - retry2) + "! I couldn't get " + label + " for " + SVD_);
                                        Thread.Sleep(1000 * rand2.Next(AD_Wait, AD_Wait * (AD_Retry - retry2)));
                                    }

                                }
                            return false;
                        });
                        if (report != null) report(SVD_ + "-" + label + "=" + counts[SVD_]);
                        retry = 0;
                    }
                    catch (Exception e)
                    {
                        if (retry == 0) 
                        {
                            if (report != null) report(SVD_ + "-" + label + "=Error:" + e.Message);
                            lock (counts)
                            {
                                if (counts.Contains(SVD_)) counts.Remove(SVD_);
                                counts.Add(SVD_, -1);
                            }
                        }
                        else
                        {
                            if (report != null) report("Oups" + (AD_Retry - retry) + "! I couldn't get " + label + " for " + SVD_);
                            Thread.Sleep(1000 * rand.Next(AD_Wait, AD_Wait * (AD_Retry - retry)));
                        }
                    }
            });
            return counts;
        }



        public static List<string> errorList = new List<string>(new string[] { "Error" });


        public static Hashtable getMembers(Hashtable members, string filter_, string label,
                                            CheckedListBox SVDBox, Func<string, bool> report)
        {
            if (members != null) return members;
            members = new Hashtable();
            new List<string>(SVDBox.Items.OfType<string>()).AsParallel().ForAll(SVD_ =>
            {
                Random rand = new Random(); int retry = AD_Retry; while (retry-- > 0) try
                    {
                        string SVD = SVD_;
                        if (SVD.IndexOf('-') >= 0) SVD = SVD.Substring(SVD.IndexOf('-') + 1);
                        string filter = "(&(ObjectClass=user)(!ObjectClass=computer)(employeeID=*)"
                            //+ "(!userAccountControl=514)(!userAccountControl=66050)"
                            //+ "(|(l=" + SVD + ")(st=" + SVD + "))"
                            + (filter_ == null ? "" : filter_) + ")";

                        DomainCollection dc = Forest.GetCurrentForest().Domains;
                        Domain[] domains = new Domain[dc.Count]; dc.CopyTo(domains, 0);
                        string current = Domain.GetCurrentDomain().Name;
                        domains.All(domain =>
                        {
                            Random rand2 = new Random(); int retry2 = AD_Retry; while (retry2-- > 0) try
                                {
                                    DirectoryEntry site = null;
                                    try
                                    {
                                        DirectoryEntry de = domain.GetDirectoryEntry();
                                        
                                        site = de.Children.Find("OU=" + SVD);
                                    }
                                    catch (Exception e)
                                    {
                                        ;
                                    }
                                    SearchResultCollection col = site == null ? null : new DirectorySearcher(site, filter,
                                        new string[] { "distinguishedName", "displayName", "department", "title" },
                                        SearchScope.Subtree).FindAll();
                                    //Console.WriteLine(domain + "/" + SVD + " -> " + (col == null ? 0 : col.Count));
                                    List<string> newmembers = new List<string>();
                                    if (col != null) foreach (SearchResult r in col)
                                    {
                                        string dn = r.Properties["distinguishedName"].Count > 0 ? r.Properties["distinguishedName"][0].ToString().ToLower() : "";
                                        string disp = r.Properties["displayName"].Count > 0 ? r.Properties["displayName"][0].ToString() : "";
                                        string dpt = r.Properties["department"].Count > 0 ? r.Properties["department"][0].ToString() : "";
                                        string title = r.Properties["title"].Count > 0 ? r.Properties["title"][0].ToString() : "";
                                        if (!dn.Contains("terminated")
                                            && !dn.Contains("system-accounts"))
                                        {
//Console.WriteLine(domain + "/" + SVD + "/" + dpt + "/" + title + " " + disp);
                                            newmembers.Add(disp);
                                        }
                                    }
                                    lock (members)
                                    {
                                        List<string> SVDMembers;
                                        if (members.Contains(SVD_))
                                        {
                                            if (members[SVD_] == errorList)
                                                SVDMembers = errorList;
                                            else
                                            {
                                                SVDMembers = (List<string>)members[SVD_];
                                                SVDMembers.AddRange(newmembers);
                                            }
                                        }
                                        else
                                            SVDMembers = newmembers;
                                        //Console.WriteLine("\t" + SVD + " in " + domain.Name + ": " + DomainCount + ", in total:" + SVDCount);
                                        if (members.Contains(SVD_)) members.Remove(SVD_);
                                        members.Add(SVD_, SVDMembers);
                                    }
                                    retry2 = 0;
                                    return true;
                                }
                                catch (Exception ee)
                                {
                                    if (retry2 <= 0)
                                    {
                                        if (report != null) report(label + " " + SVD_ + "... Error:" + ee.Message);
                                        lock (members)
                                        {
                                            if (members.Contains(SVD_)) members.Remove(SVD_);
                                            members.Add(SVD_, errorList);
                                        }
                                    }
                                    else
                                    {
                                        if (report != null) report("Oups" + (AD_Retry - retry2) + "! I couldn't get " + label + " for " + SVD_);
                                        Thread.Sleep(1000 * rand2.Next(AD_Wait, AD_Wait * (AD_Retry - retry2)));
                                    }

                                }
                            return false;
                        });
                        if (report != null
                            && ((List<string>)members[SVD_]).Count > 0
                            ) report(label + " " + SVD_ + "... "
                            + (((List<string>)members[SVD_]).Count == 0 ? "none" : ((List<string>)members[SVD_]).Aggregate((x, y) => x + "; " + y))
                            );
                        retry = 0;
                    }
                    catch (Exception e)
                    {
                        if (retry == 0)
                        {
                            if (report != null) report(label + " " + SVD_ + "... Error:" + e.Message);
                            lock (members)
                            {
                                if (members.Contains(SVD_)) members.Remove(SVD_);
                                members.Add(SVD_, errorList);
                            }
                        }
                        else
                        {
                            if (report != null) report("Oups" + (AD_Retry - retry) + "! I couldn't get " + label + " for " + SVD_);
                            Thread.Sleep(1000 * rand.Next(AD_Wait, AD_Wait * (AD_Retry - retry)));
                        }
                    }
            });
            return members;
        }


    }
}
