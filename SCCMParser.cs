using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Windows.Forms;
using System.Globalization;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.IO;

namespace AuditSec
{
    class SCCMParser
    {
        public class Context
        {
            public string PARSER_DEBUG_PATH = null, PARSER_TEMPL_PATH = null;
//            public StringBuilder PARSER_DEBUG = null, PARSER_TEMPL = null;
            public StreamWriter PARSER_DEBUG = null, PARSER_TEMPL = null;

            public string QUERY = null, QUERYNAME = null;
            public string[] COLNAME = null, COLTABLE = null, COLALIAS = null,
                ADSCOLNAME = null, DIRXMLCOLNAME = null, DIRXMLCOLALIAS = null,
                COLAGGREGFLAG = null, COLCOLOR = null;
            public int TABLECOUNT = 0;
            public bool RESTABLE = false;//SMS_R_System presence
            public Type[] COLTYPE = null, ADSCOLTYPE = null, DIRXMLCOLTYPE = null;
            public List<string>[] COLFUNC = null;
            public bool[] COLCOMB = null, COLSPLIT = null, COLNOAGGREG = null, COLHIDE = null, COLDEBUG = null;
            public List<string> SPLITCOLUMNS = null;

            public bool COMBINE, TRUNCATED;
            public Hashtable USERCACHE = null;
            public string LGROUPSTRING = null;
            public List<string> LGROUPSUBLISTS = null;
            public int LGROUPCOUNT = 0;
            public List<string> USERNOTFOUND = null;

            public int LARGE = 0;
            public bool unattended = false, ADDUSERNOTFOUND = true;
        }



        public static void createLogs(Context CX, SCCMReporting GUI)
        {
            string tstamp = DateTime.Now.ToString("yyMMddHHmmss");
            string logid = Guid.NewGuid().ToString().ToUpper().Substring(0, 6);

            CX.PARSER_DEBUG_PATH = Path.Combine(AuditSec.APPDATA, "sccmlast-WQL Logs-" + logid + ".txt");
            CX.PARSER_TEMPL_PATH = Path.Combine(AuditSec.APPDATA, "sccmlast-WQL Request-" + logid + ".txt");
            List<string> OldLogs = new List<string>();
            OldLogs.AddRange(Directory.GetFiles(AuditSec.APPDATA, "sccmlast-WQL Logs-*.txt"));
            OldLogs.AddRange(Directory.GetFiles(AuditSec.APPDATA, "sccmlast-WQL Request-*.txt"));
            foreach (string old in OldLogs) try
                {
                    File.Delete(old);
                    Console.WriteLine("Old Logs purge success: file://" + old);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Old Logs purge error: file://" + old + "\n" + e.Message);
                }
            try
            {
                CX.PARSER_DEBUG = new StreamWriter(CX.PARSER_DEBUG_PATH, false);
                CX.PARSER_DEBUG.WriteLine("#" + tstamp);
                CX.PARSER_DEBUG.Flush();
                Console.WriteLine("Logs file creation success: file://" + CX.PARSER_DEBUG_PATH);
            }
            catch (Exception e)
            {
                Console.WriteLine("Logs file creation error: file://" + CX.PARSER_DEBUG_PATH + "\n" + e.Message);
                throw new Exception("Logs file creation error.");
            }
            try
            {
                CX.PARSER_TEMPL = new StreamWriter(CX.PARSER_TEMPL_PATH, false);
                CX.PARSER_TEMPL.WriteLine("#" + tstamp + "\n¤");
                CX.PARSER_TEMPL.Flush();
                Console.WriteLine("Logs file creation success: file://" + CX.PARSER_TEMPL_PATH);
            }
            catch (Exception e)
            {
                Console.WriteLine("Logs file creation error: file://" + CX.PARSER_TEMPL_PATH + "\n" + e.Message);
                throw new Exception("Logs file creation error.");
            }
        }

        public static void DebugLine(Context CX, string text)
        {
            if (text == null) text = "";
            if (CX != null && CX.PARSER_DEBUG != null)
            {
                CX.PARSER_DEBUG.WriteLine(text);
                CX.PARSER_DEBUG.Flush();
            }
        }

        public static void TemplateLine(Context CX, string text)
        {
            if (text == null) text = "";
            if (CX != null && CX.PARSER_TEMPL != null)
            {
                CX.PARSER_TEMPL.WriteLine(text);
                CX.PARSER_TEMPL.Flush();
            }
        }

        public static void DebugClose(Context CX, SCCMReporting GUI)
        {
            if (CX.PARSER_DEBUG == null)
            {
                throw new Exception("PARSER_DEBUG ALREADY CLOSED!!!");
                return;
            }
            else try
            {
                CX.PARSER_DEBUG.Close();
                CX.PARSER_DEBUG = null;
                Console.WriteLine("Logs file write success: file://" + CX.PARSER_DEBUG_PATH);
            }
            catch (Exception e)
            {
                Console.WriteLine("Logs file write error: file://" + CX.PARSER_DEBUG_PATH + "\n" + e.Message);
            }
        }

        public static void TemplateClose(Context CX, SCCMReporting GUI)
        {
            if (CX.PARSER_TEMPL == null)
            {
                throw new Exception("PARSER_TEMPL ALREADY CLOSED!!!");
            }
            else try
            {
                CX.PARSER_TEMPL.Close();
                CX.PARSER_TEMPL = null;
                Console.WriteLine("Logs file write success: file://" + CX.PARSER_TEMPL_PATH);
            }
            catch (Exception e)
            {
                Console.WriteLine("Logs file write error: file://" + CX.PARSER_TEMPL_PATH + "\n" + e.Message);
            }
        }
        


        public static List<string> getSubQueries(Context CX, SCCMReporting GUI)
        {
            List<string> subQUERIES = new List<string>();
            if (CX.LGROUPSTRING != null)
            {
                GUI.setWQL("Information: Large Group subquerying. " + CX.LGROUPSUBLISTS.Count + " subqueries");
                //GUI.setWQL("\r\n==========SUBQUERIESPARSE=================================================================================\r\n");
                foreach (string sub in CX.LGROUPSUBLISTS)
                {
                    string q = CX.QUERY;
                    GUI.setWQL("Parsing subquery#" + (subQUERIES.Count+1) + "...");
                    expandInLargeGroup(CX, GUI, ref q, sub);
                    expandMultiops(CX, GUI, ref q, false);
                    subQUERIES.Add(q);
                    //Console.WriteLine("\n==========Subquery#" + subQUERIES.Count + "============================================================\n"
                    //    + q + "\n");
                }
            }
            else
            {
                if (CX.LARGE > 1)
                {
                    for (int z = 0; z < 100; z++)
                    {
                        StringBuilder q = new StringBuilder(CX.QUERY);
                        string previewClause = "ResourceID like \"%" + (z < 10 ? "0" + z : "" + z) + "\"";
                        SCCMParser.insertWhereClause(q, previewClause);
                        subQUERIES.Add(q.ToString());
                    }
                    GUI.setWQL("Information: ResourceID subquerying. 100 subqueries");
                }
                else if (CX.LARGE > 0)
                {
                    for (int z = 0; z < 10; z++)
                    {
                        StringBuilder q = new StringBuilder(CX.QUERY);
                        string previewClause = "ResourceID like \"%" + z + "\"";
                        SCCMParser.insertWhereClause(q, previewClause);
                        subQUERIES.Add(q.ToString());
                    }
                    GUI.setWQL("Information: ResourceID subquerying. 10 subqueries");
                }

                else
                {
                    subQUERIES.Add(CX.QUERY);
                    //GUI.setWQL("Information: No subquerying. 1 subquery");
                }
            }
            return subQUERIES;
        }




        public static void prepareQuery(Context CX, SCCMReporting GUI, 
            string queryname, string querydef, string queryargs, bool unattended)
        {
            GUI.STOP = false;

            createLogs(CX, GUI);

            List<string> ADSChecked_ = null, DIRXMLChecked_ = null,
                freqChecked_ = null, domainChecked_ = null, OUChecked_ = null, dptChecked_ = null, WUChecked_ = null;
            int largeChecked_ = 0;
            string notifyRecipients_ = null;
            string cumulatedargs = "";
            CX.TRUNCATED = false;
            CX.COMBINE = false;
            CX.USERCACHE = new Hashtable();
            CX.LGROUPSTRING = null;
            CX.LGROUPSUBLISTS = null;
            CX.LGROUPCOUNT = 0;
            CX.USERNOTFOUND = null;
            CX.unattended = unattended;

            GUI.setWQL(null);
            GUI.setWQL("AuditSec - SCCM Reporting - " + GUI.progver + "\r\n");
            GUI.setWQL("Building up the WQL query...");
            GUI.setWQL("Query is " + (unattended ? "" : "not ") + "unattended.");

            if (GUI.mergePanicBox.Checked)
                GUI.setWQL("Warning: Merge Panic Mode activated.");
                       



            string BASEVAR = "";
            GUI.setWQL("Loading Base Variables...");
            foreach (string template in GUI.QUERIES.Keys) if (template.ToUpper().StartsWith("_TEMPLATE_ - BASE VARIABLES"))
            {
                string templargs = ((string[])GUI.QUERIES[template])[0];
                BASEVAR = templargs + (BASEVAR.Trim().Length > 0 ? ",\n\n" : "") + BASEVAR.Trim();
                TemplateLine(CX,template + "\n" + templargs + "\n" + "" + "\n¤\n\n");
                DebugLine(CX,"\r\n==========BASEVAR==========[" + template + "]========================================");
            }
            BASEVAR = removeArgDuplicates(BASEVAR);
            DebugLine(CX,"\r\n==========BASEVAR============================================================\r\n"
                + BASEVAR.Replace(",", "\n"));
            if (GUI.STOP) throw new Exception("User cancellation.");


            //GUI.setWQL("Removing /* */ comments...");
            CX.QUERY = Regex.Replace(querydef, @"/\*.*?\*/", "", IgnoreCaseSingleLine);
            CX.QUERYNAME = queryname + (unattended ? " (Scheduled Job)" : "");
            DebugLine(CX,"\r\n==========INITIAL==========[" + queryname + "]========================================\r\n"
                + CX.QUERY + "\r\nArgs: " + queryargs);
            TemplateLine(CX,queryname + "\n" + queryargs + "\n" + querydef + "\n¤\n\n");
            if (GUI.STOP) throw new Exception("User cancellation.");

            replaceDateTime(CX, GUI, ref CX.QUERY);

            CX.QUERY = Regex.Replace(CX.QUERY, @",\W*AS\W+\[", ", RES.ResourceID AS [", IgnoreCaseSingleLine);
            //DebugLine(CX,"==========INITIAL:NEWCOL===============================================================\r\n"
            //    + CX.QUERY);

            GUI.setWQL("Setting Multiple Columns...");
            CX.QUERY = !Regex.IsMatch(queryargs, "\\W{0,1}MultipleColumns=([^,]*)", IgnoreCaseSingleLine) ? CX.QUERY
                : CX.QUERY.Replace("!MultipleColumns!", Regex.Replace(queryargs, ".*\\W{0,1}MultipleColumns=([^,]*).*", "$1", IgnoreCaseSingleLine));
            //DebugLine(CX,"\r\n1==========COLEXP!MultipleColumns!============================================================\r\n"
            //    + CX.QUERY);
            string combdef = !Regex.IsMatch(CX.QUERY, combine0a, IgnoreCaseSingleLine) ? ""
                : combine0c.Replace("%combine%", Regex.Replace(CX.QUERY, combine0a, combine0b, IgnoreCaseSingleLine));
            //DebugLine(CX,"\r\n2==========COLEXP======================================================================\r\n"
            //    + "expand=" + combdef);
            if (combdef.Length > 0)
            {
                CX.COMBINE = true;
                for (int c = 0; c < combine1a.Length; c++)
                {
                    bool match; CX.QUERY = !(match = Regex.IsMatch(CX.QUERY, combine1a[c], IgnoreCaseSingleLine)) ? CX.QUERY
                        : Regex.Replace(CX.QUERY, combine1a[c], combine1b[c], IgnoreCaseSingleLine);
                    if (match)
                    {
                        // DebugLine(CX,"\r\n3==========COLEXP" + (10 - c) + "=====================================================================\r\n"
                        //    + "pattern=" + combine1a[c] + "\n      ==> " + combine1b[c]
                        //    + "\nquery=\n" + CX.QUERY);
                        break;
                    }
                }
                CX.QUERY = Regex.Replace(CX.QUERY, combine2a, combdef, IgnoreCaseSingleLine).Replace(" AS! ", " AS ");
                //DebugLine(CX,"\r\n4==========COLEXP======================================================================\r\n"
                //    + "pattern=" + combine2a + "\n      ==> " + combdef
                //    + "\nquery=\n" + CX.QUERY);
                for (int c = 0; c < combine3a.Length; c++)
                {
                    bool match; CX.QUERY = !(match = Regex.IsMatch(CX.QUERY, combine3a[c], IgnoreCaseSingleLine)) ? CX.QUERY
                        : Regex.Replace(CX.QUERY, combine3a[c], combine3b[c], IgnoreCaseSingleLine);
                    if (match)
                    {
                        // DebugLine(CX,"\r\n5==========COLEXP" + (10 - c) + "======================================================================\r\n"
                        //    + "pattern=" + combine3a[c] + "\n      ==> " + combine3b[c]
                        //    + "\nquery=\n" + CX.QUERY);
                        break;
                    }
                }
                queryargs += ", combine=true";
                DebugLine(CX,"\r\n==========COLEXP======================================================================\r\n"
                    + CX.QUERY + "\r\nArgs: " + queryargs);
                if (GUI.STOP) throw new Exception("User cancellation.");
            }


            Hashtable QUERIES_ = new Hashtable();
            foreach (string k in GUI.QUERIES.Keys) QUERIES_.Add(k, GUI.QUERIES[k]);

            {
                int tl = queryargs.ToUpper().IndexOf("TEMPLATELEVEL=");
                if (tl >= 0) tl += 14;
                int tlc = tl < 0 ? -1 : queryargs.IndexOf(',', tl);
                string TemplateLevel = tl < 0 ? null :
                    queryargs.Substring(tl, tlc < 0 ? queryargs.Length - tl : tlc - tl);
                string TemplateLevel_ = TemplateLevel == null ? null : PrefixLevel + TemplateLevel;
                if (TemplateLevel_ != null && GUI.QUERIES.Contains(TemplateLevel_))
                {
                    if (!QUERIES_.Contains(BaseTemplate))
                    {
                        QUERIES_.Add(BaseTemplate, GUI.QUERIES[TemplateLevel_]);
                        GUI.setWQL("Applying Template Level '" + TemplateLevel_ + "'.");
                    }
                }
                else if (TemplateLevel != null && GUI.QUERIES.Contains(TemplateLevel))
                {
                    if (!QUERIES_.Contains(BaseTemplate))
                    {
                        QUERIES_.Add(BaseTemplate, GUI.QUERIES[TemplateLevel]);
                        GUI.setWQL("Applying Template Level '" + TemplateLevel + "'.");
                    }
                }
                else
                {
                    if (TemplateLevel != null)
                        GUI.setWQL("Warning: Template Level '" + TemplateLevel + "' not found.");
                    else
                        GUI.setWQL("Template Level not defined.");
                    if (GUI.QUERIES[DefaultLevel] != null)
                    {
                        if (!QUERIES_.Contains(BaseTemplate))
                        {
                            QUERIES_.Add(BaseTemplate, GUI.QUERIES[DefaultLevel]);
                            GUI.setWQL("Set Default Level '" + DefaultLevel.Replace(PrefixLevel, "") + "'");
                        }
                    }
                    else
                    {
                        GUI.setWQL("Error: Default Template Level '" + DefaultLevel + "' not found.");
                    }
                }
            }
            if (GUI.STOP) throw new Exception("User cancellation.");


            GUI.setWQL("Replacing Template occurences...");
            bool replaced; do
            {
                CX.QUERY = Regex.Replace(CX.QUERY, @"SELECT\s+%([^%]+)%([^%]|)", "SELECT %%$1%%\n", IgnoreCaseSingleLine);
                replaced = false;

                foreach (string template in QUERIES_.Keys)
                {
                    string templargs = ((string[])QUERIES_[template])[0];
                    string templselect = "SELECT %%" + template + "%";
                    if (CX.QUERY.IndexOf(templselect) >= 0)
                    {
                        CX.COMBINE = true;
                        string templdef = ((string[])QUERIES_[template])[1];
                        //GUI.setWQL("Removing /* */ comments...");
                        templdef = Regex.Replace(templdef, @"/\*.*?\*/", "", IgnoreCaseSingleLine);

                        //DebugLine(CX,"\n-------------------------------------\n%%template%: " + templvar);
                        TemplateLine(CX,template + "\n" + templargs + "\n" + templdef + "\n¤\n\n");
                        DebugLine(CX,"\r\n==========TEMPLATE==========[" + template + "]========================================\r\n"
                            + templdef + "\r\nArgs: " + templargs);
                        cumulatedargs = templargs + (cumulatedargs.Trim().Length > 0 ? ",\n\n" : "") + cumulatedargs.Trim();
                        replaceDateTime(CX, GUI, ref templdef);

                        templdef = Regex.Replace(templdef, @",\W*AS\W+\[", ", RES.ResourceID AS [", IgnoreCaseSingleLine);
                        //DebugLine(CX,"==========TEMPLATE:NEWCOL===============================================================\r\n"
                        //    + templdef);


                        templdef = Regex.Replace(templdef, @",\W*AS\W+\[", ", RES.ResourceID AS [", IgnoreCaseSingleLine);
                        //DebugLine(CX,"==========TEMPLATE:NEWCOL==============================================================\r\n"
                        //    + CX.QUERY);

                        GUI.setWQL("Expand columns upon row combining...");
                        templdef = !Regex.IsMatch(templargs, "\\W{0,1}MultipleColumns=([^,])*", IgnoreCaseSingleLine) ? templdef
                            : templdef.Replace("!MultipleColumns!", Regex.Replace(templargs, ".*\\W{0,1}MultipleColumns=([^,])*.*", "$1", IgnoreCaseSingleLine));
                        //DebugLine(CX,"\r\n==========TEMPLATE:COLEXP!MultipleColumns!==================================================\r\n"
                        //    + templdef);
                        string combdef_ = !Regex.IsMatch(templdef, combine0a, IgnoreCaseSingleLine) ? ""
                            : combine0c.Replace("%combine%", Regex.Replace(templdef, combine0a, combine0b, IgnoreCaseSingleLine));
                        //DebugLine(CX,"\r\n==========TEMPLATE:COLEXP============================================================\r\n"
                        //    + "expand=" + combdef_);
                        if (combdef_.Length > 0)
                        {
                            for (int c = 0; c < combine1a.Length; c++)
                            {
                                bool match; templdef = !(match = Regex.IsMatch(templdef, combine1a[c], IgnoreCaseSingleLine)) ? templdef
                                    : "[" + Regex.Replace(templdef, combine1a[c], combine1b[c], IgnoreCaseSingleLine); if (match) break;
                                //DebugLine(CX,"\r\n==========TEMPLATE:COLEXP============================================================\r\n"
                                //    + "pattern=" + combine1a[c] + "\n      ==> " + combine1b[c]
                                //    + "\ntempldef=\n" + templdef);
                            }
                            templdef = Regex.Replace(templdef, combine2a, combdef_, IgnoreCaseSingleLine).Replace(" AS! ", " AS ");
                            //DebugLine(CX,"\r\n==========TEMPLATE:COLEXP============================================================\r\n"
                            //    + "pattern=" + combine2a + "\n      ==> " + combdef_
                            //    + "\ntempldef=\n" + templdef);

                            for (int c = 0; c < combine3a.Length; c++)
                            {
                                bool match; templdef = !(match = Regex.IsMatch(templdef, combine3a[c], IgnoreCaseSingleLine)) ? templdef
                                    : Regex.Replace(templdef, combine3a[c], combine3b[c], IgnoreCaseSingleLine); if (match) break;
                                //DebugLine(CX,"\r\n==========TEMPLATE:COLEXP============================================================\r\n"
                                //    + "pattern=" + combine3a[c] + "\n      ==> " + combine3b[c]
                                //    + "\ntempldef=\n" + templdef);
                            }
                            templargs += ", combine=true";
                            DebugLine(CX,"\r\n==========TEMPLATE:COLEXP============================================================\r\n"
                                + templdef + "\r\nArgs: " + templargs);
                        }

                        int colstart = CX.QUERY.IndexOf(templselect) + templselect.Length;
                        int colend = CX.QUERY.IndexOf('%', colstart); if (colend == -1) colend = CX.QUERY.Length;
                        string coladd = null; if (colend - colstart > 0)
                        {
                            coladd = CX.QUERY.Substring(colstart, colend - colstart);
                            CX.QUERY = CX.QUERY.Substring(0, colstart) + CX.QUERY.Substring(colend);
                            //DebugLine(CX,"query-coladd: " + CX.QUERY);
                        }
                        if (coladd != null)
                        {
                            //DebugLine(CX,"coladd: " + coladd);
                            if (templdef.IndexOf(',', 0) == -1)
                            {
                                //DebugLine(CX,"commacount: none");
                                int colins = -1;
                                colins = templdef.IndexOf('%', colins + 1);
                                //DebugLine(CX,"colins " + colins + ": " + replace.Substring(0, colins + 1));
                                colins = templdef.IndexOf('%', colins + 1);
                                //DebugLine(CX,"colins " + colins + ": " + replace.Substring(0, colins + 1));
                                colins = templdef.IndexOf('%', colins + 1);
                                //DebugLine(CX,"colins " + colins + ": " + replace.Substring(0, colins + 1));
                                if (colins >= 0)
                                {
                                    templdef = templdef.Substring(0, colins + 1) + coladd + templdef.Substring(colins + 1);
                                }
                            }
                            else
                            {
                                int commacount = 1, colins = -1; do
                                {
                                    //DebugLine(CX,"commacount: " + commacount);
                                    colins = templdef.IndexOf(',', colins + 1);
                                    //DebugLine(CX,"colins " + colins + ": " + replace.Substring(0, colins + 1));
                                    commacount--;
                                } while (commacount-- > 0 && colins >= 0);
                                if (colins >= 0)
                                {
                                    templdef = templdef.Substring(0, colins) + coladd + templdef.Substring(colins);
                                }
                            }
                        }
                        //DebugLine(CX,"replace+coladd: " + replace);


                        try
                        {
                            string templdef_ = !Regex.IsMatch(templdef, @"\Wwhere\W.*", IgnoreCaseSingleLine) ? templdef :
                                Regex.Replace(templdef, @"\Wwhere\W.*", "", IgnoreCaseSingleLine);
                            string templ_where = templdef_.Length == templdef.Length ? null : templdef.Substring(templdef_.Length + 1);
                            templdef = templdef_;
                            //DebugLine(CX,"\ntempldef_where: '" + templ_where + "'");

                            string QUERY_ = !Regex.IsMatch(CX.QUERY, @"\Wwhere\W.*", IgnoreCaseSingleLine) ? CX.QUERY :
                                Regex.Replace(CX.QUERY, @"\Wwhere\W.*", "", IgnoreCaseSingleLine);
                            string query_where = QUERY_.Length == CX.QUERY.Length ? null : CX.QUERY.Substring(QUERY_.Length + 1);
                            CX.QUERY = QUERY_;
                            //DebugLine(CX,"\nquery_where: '" + query_where + "'");

                            CX.QUERY += "\n"
                                + (templ_where == null ? "" : templ_where) + (query_where == null ? "" : " "
                                + (templ_where == null ? query_where : "\nAND (\n" + query_where.Substring(5) + "\n)"));
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine("where regex ==> " + ee.ToString());
                            GUI.setWQL("where regex ==> " + ee.Message);
                        }

                        if (CX.QUERY.IndexOf(templselect + "%") >= 0)
                        {
                            CX.QUERY = CX.QUERY.Replace(templselect + "%", templdef);
                            DebugLine(CX,"\r\n==========TEMPLATE APPLIED======================================================================\r\n"
                                + CX.QUERY + "\r\nArgs: " + cumulatedargs);
                            replaced = true;
                        }
                        else
                            throw new Exception("Parse Error: TEMPLATE closing % not found.");
                        if (GUI.STOP) throw new Exception("User cancellation.");
                    }
                }
            } while (replaced);

            GUI.setWQL("Replacing Condition-Template occurences...");
            do
            {
                CX.QUERY = Regex.Replace(CX.QUERY, @"\s(where|and)\s+%([^%]+)%\s", " $1 %%$2%%\n", IgnoreCaseSingleLine);
                CX.QUERY = Regex.Replace(CX.QUERY, @"\s(where|and)\s*\(\s*%([^%]+)%\s*\)\s*", " $1 ( %%$2%% )\n", IgnoreCaseSingleLine);

                replaced = false;
                foreach (string template in GUI.QUERIES.Keys) if (template.ToUpper().StartsWith("_CONDITION_"))
                {
                    string templdef = ((string[])GUI.QUERIES[template])[1];
                    //GUI.setWQL("Removing /* */ comments...");
                    templdef = Regex.Replace(templdef, @"/\*.*\*/", "", IgnoreCaseSingleLine);

                    bool match = Regex.IsMatch(CX.QUERY, @"\s(where|and)\s+%%" + template + @"%%\s", IgnoreCaseSingleLine);
                    bool match2 = match ? false
                               : Regex.IsMatch(CX.QUERY, @"\s(where|and)\s*\(\s*%%" + template + @"%%\s*\)\s*", IgnoreCaseSingleLine);
                    if (match || match2)
                    {
                        replaceDateTime(CX, GUI, ref templdef);
                        CX.QUERY = Regex.Replace(CX.QUERY,
                              @"\s(where|and)"
                            + (match2 ? @"\s*\(\s*" : @"\s+")
                            +  "%%" + template + "%%"
                            + (match2 ? @"\s*\)\s*" : @"\s"),
                            Regex.Replace(templdef, "WHERE", "$1", IgnoreCaseSingleLine), IgnoreCaseSingleLine);
                        replaced = true;
                        TemplateLine(CX,template + "\n" + "" + "\n" + templdef + "\n¤\n\n");
                        DebugLine(CX,"\r\n==========CONDITION==========[" + template + "]========================================\r\n"
                            + CX.QUERY);
                    }
                }
            } while (replaced);
            if (GUI.STOP) throw new Exception("User cancellation.");

            replaceCollection(CX, GUI, ref CX.QUERY, true);

            removeWhitespace(CX, GUI, ref CX.QUERY);
            CX.QUERY = Regex.Replace(CX.QUERY, @"^SELECT(\s+DISTINCT)?\s*,", "SELECT$1 ", RegexOptions.IgnoreCase);
            CX.QUERY = Regex.Replace(CX.QUERY, @"^(((?!(FROM|JOIN)).)*)(JOIN)", "$1 FROM", RegexOptions.IgnoreCase);
            CX.QUERY = Regex.Replace(CX.QUERY, @",\s*FROM", " FROM", RegexOptions.IgnoreCase);


            Hashtable TABLENAME = new Hashtable();
            parseTables(CX, GUI, ref CX.QUERY, TABLENAME);

            bool DO_TABLE_ALIAS = false;
            if (DO_TABLE_ALIAS)
                foreach (DictionaryEntry en in TABLENAME)
                {
                    string alias = en.Key.ToString();
                    string table = en.Value.ToString();

                    try
                    {
                        CX.QUERY = Regex.Replace(CX.QUERY, "(\\W)" + alias + "\\.", "$1" + table + ".",
                                 RegexOptions.IgnoreCase);
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine("replace " + alias + ". -> " + table + ".  ==> " + ee.ToString());
                        GUI.setWQL("replace " + alias + ". -> " + table + ".  ==> " + ee.Message);
                    }

                    try
                    {
                        CX.QUERY = Regex.Replace(CX.QUERY, "\\Was(\\W)" + alias + "(\\W)", "$1",
                                 RegexOptions.IgnoreCase);
                    }
                    catch (Exception ee)
                    {
                        Console.WriteLine("replace as " + alias + " ->  ==> " + ee.ToString());
                        GUI.setWQL("replace as " + alias + " ->  ==> " + ee.Message);
                    }
                }
            DebugLine(CX,"\r\n==========TABLE " + DO_TABLE_ALIAS + "======================================================================\r\n"
                + CX.QUERY);
            //Console.WriteLine("CX.QUERY after table alias replacement: " + CX.QUERY);
            if (GUI.STOP) throw new Exception("User cancellation.");


            GUI.setWQL("Parsing the arguments...");
            cumulatedargs += ",\n\n" + queryargs;
            cumulatedargs += ",\n\n" + BASEVAR;
            DebugLine(CX,"\r\n==========CUMULATED======================================================================\r\n"
                + "Args: " + cumulatedargs);
            cumulatedargs = removeArgDuplicates(cumulatedargs);
            DebugLine(CX,"\r\nArgs: " + cumulatedargs);
            //if (unattended)
            {
                ADSChecked_ = new List<string>(); DIRXMLChecked_ = new List<string>(); freqChecked_ = new List<string>();
                domainChecked_ = new List<string>(); OUChecked_ = new List<string>(); dptChecked_ = new List<string>(); WUChecked_ = new List<string>();
                GUI.applyArgsUnattended(cumulatedargs, ADSChecked_, DIRXMLChecked_,
                    ref largeChecked_, ref notifyRecipients_, freqChecked_,
                    domainChecked_, OUChecked_, dptChecked_, WUChecked_, true);
            }

            List<string> SPLITCOLUMNSfromargs = new List<string>();
            GUI.setWQL("Replacing arguments...");
            //Console.WriteLine("CX.QUERY: " + CX.QUERY);
            string[] gs = cumulatedargs.Split(new char[] { ',' });
            if (gs != null && gs.Length > 0) for (int i = 0; i < gs.Length; i++)
                {
                    int g = gs[i].IndexOf('='); if (g > 0)
                    {
                        string argname = gs[i].Substring(0, g).Trim(); if (argname.Length > 0)
                        {
                            string argvalue = gs[i].Substring(g + 1).Trim();
                            //Console.WriteLine(argname + "=" + argvalue);
                            if (argname.ToUpper().Equals("SPLIT"))
                            {
                                foreach (string splitColumn in argvalue.Split(new char[] { ';', '+' }, StringSplitOptions.RemoveEmptyEntries))
                                {
                                    SPLITCOLUMNSfromargs.Add(splitColumn);
                                    GUI.setWQL("Args Split=" + splitColumn);
                                }
                            }
                            else if (argname.ToUpper().Equals("NOTFOUND"))
                            {
                                if (argvalue.ToUpper().Equals("FALSE"))
                                {
                                    CX.ADDUSERNOTFOUND = false;
                                    GUI.setWQL("Args NOTFOUND=false");
                                }
                            }
                            else//===REPLACEARG
                            {
                                if (argname.ToLower().Contains("path"))
                                {
                                    argvalue = Regex.Replace(argvalue, @"([^\\]*)\\([^\\]*)", @"$1\\$2");
                                    string escaped = escapeFunc(argvalue, false);
                                    //string escaped = escapeFunc('"' + argvalue + '"', true);
                                    //escaped = escaped.Substring(1, escaped.Length - 2);

                                    CX.QUERY = CX.QUERY.Replace("!" + argname + "!", escaped);
                                    CX.QUERY = CX.QUERY.Replace("%" + argname + "%", argvalue);
                                }
                                else if (argname.ToLower().Contains("version") && Regex.IsMatch(argvalue, "^[0-9.,%; ]+$"))
                                {
                                    argvalue = argvalue + ";" + Regex.Replace(argvalue, @"\.", @",");
                                    string escaped = escapeFunc(argvalue, false);

                                    CX.QUERY = CX.QUERY.Replace("!" + argname + "!", argvalue);
                                    CX.QUERY = CX.QUERY.Replace("%" + argname + "%", argvalue);
                                }
                                else
                                {
                                    string escaped = escapeFunc(argvalue, true);

                                    CX.QUERY = CX.QUERY.Replace("!" + argname + "!", escaped);
                                    CX.QUERY = CX.QUERY.Replace("%" + argname + "%", escaped);
                                }
                            }
                            //Console.WriteLine("CX.QUERY: " + CX.QUERY);
                        }
                    }
                }
            DebugLine(CX,"\r\n==========ARGUMENT======================================================================\r\n"
                + CX.QUERY);
            if (GUI.STOP) throw new Exception("User cancellation.");


            expandInGroup(CX, GUI, ref CX.QUERY);
            expandMultiops(CX, GUI, ref CX.QUERY, true);


            GUI.setWQL("Replacing Column aliases...");
            CX.COLNAME = null; CX.COLTABLE = null; CX.TABLECOUNT = 0; CX.RESTABLE = false;
            CX.COLALIAS = null; CX.COLTYPE = null; CX.COLFUNC = null; CX.COLCOLOR = null;
            CX.COLCOMB = null; CX.COLSPLIT = null; CX.COLNOAGGREG = null;
            CX.COLHIDE = null; CX.COLDEBUG = null;
            CX.COLAGGREGFLAG = null; CX.SPLITCOLUMNS = null;
            if (!CX.QUERY.ToUpper().StartsWith("SELECT"))
            {
                throw new Exception("Parse Error: Query must start with word SELECT.");
                //GUI.setStatus("Query must start with word SELECT.");
                //GUI.setThreads("Parse Error");
                //return;
            }


            GUI.setWQL("Checking all templates are resolved...");
            if (Regex.IsMatch(CX.QUERY, @"SELECT\s+%{1,2}([^%]+)%{1,2}.*", IgnoreCaseSingleLine))
            {
                string unknown_templ = Regex.Replace(CX.QUERY, @"SELECT\s+%{1,2}([^%]+)%{1,2}.*", "$1", IgnoreCaseSingleLine);
                StringBuilder possible_matches = new StringBuilder();
                foreach (string templ in GUI.QUERIES.Keys)
                {
                    int dist = Levenshtein.Compute(unknown_templ, templ);
                    if (dist < 10)
                        possible_matches.AppendLine("'" + templ + "'");
                }
                throw new Exception("Parse Error: Unresolved Template: '" + unknown_templ + "'\n"
                    + (possible_matches.Length > 0 ? "Possible matches: " + possible_matches.ToString() : "")
                    + "\nInvalid Query.");
            }


            int s0 = 6;
            int s1 = CX.QUERY.ToUpper().IndexOf("FROM", s0);
            if (s1 < 0)
            {
                /*s1 = CX.QUERY.ToUpper().IndexOf("JOIN", s0);
                if (s1 < 0)
                {*/
                throw new Exception("Parse Error: FROM clause not found.");
                    //GUI.setStatus("FROM clause not found.");
                    //GUI.setThreads("Parse Error");
                    //return;
                /*}
                else
                {
                    CX.QUERY = CX.QUERY.Substring(0, s1) + "FROM" + CX.QUERY.Substring(s1 + 4);
                }*/
            }
            StringBuilder q = new StringBuilder();
            q.Append("SELECT ");
            //Console.WriteLine("SELECT " + CX.QUERY.Substring(s0, s1 - s0).Trim());
            string[] fs = CX.QUERY.Substring(s0, s1 - s0).Trim().Split(new char[] { ',' });
            CX.COLNAME = new string[fs.Length]; CX.COLTABLE = new string[fs.Length];
            CX.COLALIAS = new string[fs.Length]; CX.COLTYPE = new Type[fs.Length]; CX.COLAGGREGFLAG = new string[fs.Length];
            CX.COLFUNC = new List<string>[fs.Length]; CX.COLCOLOR = new string[fs.Length];
            CX.COLCOMB = new bool[fs.Length]; CX.COLSPLIT = new bool[fs.Length]; CX.COLNOAGGREG = new bool[fs.Length];
            CX.COLHIDE = new bool[fs.Length]; CX.COLDEBUG = new bool[fs.Length];
            CX.SPLITCOLUMNS = new List<string>();

            for (int i = 0; i < fs.Length; i++)
            {
                //Console.WriteLine("Select" + i + " " + fs[i]);
                CX.COLTYPE[i] = typeof(string);
                int a1 = fs[i].ToUpper().IndexOf(" AS ");
                string n = a1 < 0 ? fs[i].Trim() : fs[i].Substring(0, a1).Trim();
                string[] ns = n.Split(new char[] { ' ' });
                CX.COLNAME[i] =
                    ns == null || ns.Length == 0 ? ""
                    : ns.Length == 1 ? n
                    : ns[ns.Length - 1].Trim();
                if (CX.COLNAME[i].LastIndexOf('.') >= 0)
                {
                    CX.COLTABLE[i] = CX.COLNAME[i].Substring(0, CX.COLNAME[i].LastIndexOf('.'));
                    CX.COLNAME[i] = CX.COLNAME[i].Substring(CX.COLNAME[i].LastIndexOf('.') + 1);
                }
                else CX.COLTABLE[i] = null;
                CX.COLALIAS[i] = CX.COLNAME[i];
                CX.COLTYPE[i] = typeof(string);
                CX.COLFUNC[i] = null;
                CX.COLCOMB[i] = false; CX.COLSPLIT[i] = false;
                CX.COLHIDE[i] = false; CX.COLDEBUG[i] = false;
                CX.COLNOAGGREG[i] = false; CX.COLAGGREGFLAG[i] = null; CX.COLCOLOR[i] = null;
                q.Append(n);
                if (i < fs.Length - 1) q.Append(", ");
                if (a1 >= 0)
                {
                    string a = fs[i].Substring(a1 + 4);
                    List<string> ax = new List<string>(a.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        .Select(x => x.Trim()).ToList();
                    if (ax.Count >= 1)
                    {
                        string ax1 = ax.ElementAt(0);
                        ax.RemoveAt(0);
                        string ax2 = ax1.IndexOf("]") < 0 ? null : ax1.Substring(ax1.IndexOf("]") + 1);
                        if (ax2 != null)
                        {
                            ax1 = ax1.Substring(0, ax1.Length - ax2.Length);
                            if (ax2.Trim().Length > 0) ax.Insert(0, ax2);
                        }
                        CX.COLALIAS[i] = ax1.Replace("[", "").Replace("]", "");
                    }
                    if (ax.Count >= 2 && ax.ElementAt(0).ToUpper().Equals("TYPE"))
                    {
                        string TYPE = ax.ElementAt(1).ToUpper();
                        ax.RemoveRange(0, 2);
                        switch (TYPE)
                        {
                            case "CHAR": CX.COLTYPE[i] = typeof(char); break;
                            case "INT": CX.COLTYPE[i] = typeof(int); break;
                            case "FLOAT": CX.COLTYPE[i] = typeof(double); break;
                            case "BOOLEAN": CX.COLTYPE[i] = typeof(bool); break;
                            case "STRING": CX.COLTYPE[i] = typeof(string); break;
                            case "DATETIME": CX.COLTYPE[i] = typeof(DateTime); break;
                            case "LIST": CX.COLTYPE[i] = typeof(List<String>); break;
                            default: CX.COLTYPE[i] = typeof(string); break;
                        }
                    }
                    if (ax.Count > 0)
                    {
                        ax.RemoveAll(x => "NOP".Equals(x.ToUpper()));
                        CX.COLCOMB[i] = ax.RemoveAll(x => "COMBINE".Equals(x.ToUpper())) > 0;
                        CX.COLSPLIT[i] = ax.RemoveAll(x => "SPLIT".Equals(x.ToUpper())) > 0;
                        if (CX.COLSPLIT[i] && !CX.SPLITCOLUMNS.Contains(CX.COLALIAS[i]))
                        {
                            CX.SPLITCOLUMNS.Add(CX.COLALIAS[i]);
                            GUI.setWQL("Query Split += " + CX.COLALIAS[i]);
                        }
                        CX.COLNOAGGREG[i] = ax.RemoveAll(x => "NOMERGE".Equals(x.ToUpper())) > 0;
                        CX.COLHIDE[i] = ax.RemoveAll(x => "HIDE".Equals(x.ToUpper())) > 0;
                        CX.COLDEBUG[i] = ax.RemoveAll(x => "DEBUG".Equals(x.ToUpper())) > 0;

                        if (ax.RemoveAll(x => "COUNT".Equals(x.ToUpper())) > 0) CX.COLAGGREGFLAG[i] = "COUNT";
                        if (ax.RemoveAll(x => "MAXIMUM".Equals(x.ToUpper())) > 0) CX.COLAGGREGFLAG[i] = "MAXIMUM";
                        if (ax.RemoveAll(x => "MINIMUM".Equals(x.ToUpper())) > 0) CX.COLAGGREGFLAG[i] = "MINIMUM";
                        if (ax.RemoveAll(x => "SUM".Equals(x.ToUpper())) > 0) CX.COLAGGREGFLAG[i] = "SUM";
                        if (ax.RemoveAll(x => "LAST".Equals(x.ToUpper())) > 0) CX.COLAGGREGFLAG[i] = "LAST";
                        if (ax.RemoveAll(x => "FIRST".Equals(x.ToUpper())) > 0) CX.COLAGGREGFLAG[i] = "FIRST";
                        if (ax.RemoveAll(x => "SORT".Equals(x.ToUpper())) > 0) CX.COLAGGREGFLAG[i] = "SORT";

                        string colcolor = null;
                        if (ax.RemoveAll(x => {
                            bool ret = x.ToUpper().StartsWith("COLOR-");
                            if (ret) colcolor = x.Substring(6);
                            return ret;}) > 0) CX.COLCOLOR[i] = colcolor;

                        CX.COLFUNC[i] = ax.Select(x => unescapeFunc(x)).ToList();
                    }
                }
            }
            CX.TABLECOUNT = CX.COLTABLE.Where(table => table != null).Distinct().Count();
            CX.RESTABLE = CX.COLTABLE.Count(table => table != null && "SMS_R_SYSTEM".Equals(table.ToUpper())) > 0;
            q.Append(" " + CX.QUERY.Substring(s1));
            DebugLine(CX,"\r\n==========COLUMN======================================================================\r\n"
                + q + "\r\n"
                + "\r\n\r\n==========FUNC======================================================================\r\n");
            List<object> testrow = new List<object>();
            Boolean DELETEROW = false;
            for (int i = 0; i < fs.Length; i++)
            {
                DebugLine(CX, CX.COLTABLE[i] + "." + CX.COLNAME[i] + " AS [" + CX.COLALIAS[i] + "] TYPE " + CX.COLTYPE[i].Name + "\r\n"
                    + (CX.COLFUNC[i] == null || CX.COLFUNC[i].Count == 0 ? "" : CX.COLFUNC[i].Aggregate((x, y) => x + "\r\n" + y)) + "\r\n");

                testrow.Add(SCCMCalc.applyFunc(CX, error => { throw new Exception(error + "\n  at "
                    + CX.COLTABLE[i] + "." + CX.COLNAME[i] + " AS [" + CX.COLALIAS[i] + "] TYPE " + CX.COLTYPE[i].Name + "\n"); return true; },
                    i, "", testrow, ref DELETEROW, null, (x, y) => "0"));
            }

            if (GUI.STOP) throw new Exception("User cancellation.");


            GUI.setWQL("Setting the XLS Split columns...");
            DebugLine(CX,"\r\n==========SPLIT======================================================================\r\n"
                + "Query Split="
                + (CX.SPLITCOLUMNS == null || CX.SPLITCOLUMNS.Count == 0 ? "" : CX.SPLITCOLUMNS.Aggregate((x, y) => x + ", " + y))
                + "\r\nArgs  Split="
                + (SPLITCOLUMNSfromargs == null || SPLITCOLUMNSfromargs.Count == 0 ? "" : SPLITCOLUMNSfromargs.Aggregate((x, y) => x + ", " + y)));
            foreach (string splitColumn_ in SPLITCOLUMNSfromargs)
            {
                bool remove = splitColumn_.StartsWith("!");
                string splitColumn = remove ? splitColumn_.Substring(1) : splitColumn_;
                if (remove && CX.SPLITCOLUMNS.Contains(splitColumn)) CX.SPLITCOLUMNS.Remove(splitColumn);
                else if (!remove && !CX.SPLITCOLUMNS.Contains(splitColumn)) CX.SPLITCOLUMNS.Add(splitColumn);
            }
            DebugLine(CX,"\r\n==========SPLIT======================================================================\r\n"
                + "Cumulated Split="
                + (CX.SPLITCOLUMNS == null || CX.SPLITCOLUMNS.Count == 0 ? "" : CX.SPLITCOLUMNS.Aggregate((x, y) => x + ", " + y)));
            if (GUI.STOP) throw new Exception("User cancellation.");


            GUI.setWQL("Setting the Preview Clause...");
            int preview = unattended ? 0
                : (GUI.previewBox.CheckState.Equals(CheckState.Checked) ? 1 : 0)
                + (GUI.preview2Box.CheckState.Equals(CheckState.Checked) ? 1 : 0);
            string previewClause = "";
            if (preview > 0)
            {
                previewClause = "ResourceID like \"%" + (preview > 1 ? "00" : "0") + "\"";
                insertWhereClause(q, previewClause);
            }
            DebugLine(CX,"\r\n==========PREVIEW======================================================================\r\n"
                + q);
            //Console.WriteLine("Preview Clause:\n" + previewClause);
            if (GUI.STOP) throw new Exception("User cancellation.");
            CX.LARGE = GUI.getLarge(unattended, largeChecked_);

            GUI.setWQL("Setting the Organizational Unit Clause...");
            string OUClause = "";
            OUClause = GUI.getOUClause(unattended, domainChecked_, OUChecked_, dptChecked_, WUChecked_);
            if (OUClause.Length > 0)
                insertWhereClause(q, OUClause);
            DebugLine(CX,"\r\n==========OU======================================================================\r\n"
                + "Domain Checked: " + domainChecked_.Count + " - Site Checked: "  + OUChecked_.Count + " - Department Checked: " + dptChecked_
                + " - Object Checked: " + (WUChecked_.Count > 0 ? WUChecked_.Aggregate((x, y) => x + ", " + y) : "") + "\r\n" + q);
            if (GUI.STOP) throw new Exception("User cancellation.");


            CX.QUERY = q.ToString();
            removeWhitespace(CX, GUI, ref CX.QUERY);



            GUI.setWQL("Checking for incorrect use of uniops with multivalued...");
            string qtemp = q.ToString();
            StringBuilder incorrectuniopsfound = new StringBuilder();
            foreach (string uni in uniops.Distinct())
                if (Regex.IsMatch("(.*)(" + uni + @")(\s""[^""]+;[^""]+"")(.*)", qtemp, IgnoreCaseSingleLine))
                {
                    incorrectuniopsfound.Append(Regex.Replace("(.*)(" + uni + @"\s""[^""]+;[^""]+"")(.*)", qtemp, "$2$3", IgnoreCaseSingleLine) + "\r\n");
                    qtemp = Regex.Replace("(.*)(" + uni + @"\s""[^""]+;[^""]+"")(.*)", qtemp, "$1$2 *LIKE/ALL SUGGESTED*$3$4", IgnoreCaseSingleLine);
                }
            DebugLine(CX, "\r\n==========INCORRECT UNIOPS CHECK=================================================\r\n"
                + incorrectuniopsfound
                + "\r\n" + qtemp);
            if (incorrectuniopsfound.Length > 0)
                throw new Exception("Parse Error: Use of uniops with multivalued.");

            if (GUI.STOP) throw new Exception("User cancellation.");




            GUI.setWQL("Reintroduce <CR> and () indent...");
            CX.QUERY = indentWQL(CX.QUERY);
            DebugLine(CX,"\r\n==========READY======================================================================\r\n"
                + CX.QUERY);
            TemplateLine(CX,"WQL Request\n" + CX.QUERY + "\n¤\n\n");
            if (GUI.STOP) throw new Exception("User cancellation.");


            GUI.setWQL("Preparing ADS and DIRXML columns...");
            if (!unattended)
            {
                CX.ADSCOLNAME = new string[GUI.ADSBox.CheckedItems.Count];
                CX.ADSCOLTYPE = new Type[GUI.ADSBox.CheckedItems.Count];
                int ai = 0; foreach (string a in GUI.ADSBox.CheckedItems)
                {
                    CX.ADSCOLNAME[ai] = a;
                    CX.ADSCOLTYPE[ai] = MachineInfo.getPropertyType(a);
                    ai++;
                }
                CX.DIRXMLCOLNAME = new string[GUI.DIRXMLBox.CheckedItems.Count];
                CX.DIRXMLCOLALIAS = new string[GUI.DIRXMLBox.CheckedItems.Count];
                CX.DIRXMLCOLTYPE = new Type[GUI.DIRXMLBox.CheckedItems.Count];
                int di = 0; foreach (string alias in GUI.DIRXMLBox.CheckedItems)
                {
                    CX.DIRXMLCOLALIAS[di] = alias;
                    string attr = CX.DIRXMLCOLNAME[di] = UsersInfo.getDIRXMLattr(alias);
                    CX.DIRXMLCOLTYPE[di] = UsersInfo.getDIRXMLtype(attr);
                    di++;
                }
            }
            else
            {
                CX.ADSCOLNAME = new string[ADSChecked_.Count];
                CX.ADSCOLTYPE = new Type[ADSChecked_.Count];
                int ai = 0; foreach (string a in ADSChecked_)
                {
                    CX.ADSCOLNAME[ai] = a;
                    CX.ADSCOLTYPE[ai] = MachineInfo.getPropertyType(a);
                    ai++;
                }
                CX.DIRXMLCOLNAME = new string[DIRXMLChecked_.Count];
                CX.DIRXMLCOLALIAS = new string[DIRXMLChecked_.Count];
                CX.DIRXMLCOLTYPE = new Type[DIRXMLChecked_.Count];
                int di = 0; foreach (string alias in DIRXMLChecked_)
                {
                    CX.DIRXMLCOLALIAS[di] = alias;
                    string attr = CX.DIRXMLCOLNAME[di] = UsersInfo.getDIRXMLattr(alias);
                    CX.DIRXMLCOLTYPE[di] = UsersInfo.getDIRXMLtype(attr);
                    di++;
                }
            }
            if (GUI.STOP) throw new Exception("User cancellation.");


            GUI.setWQL("Checking at least one column is setup...");
            if (CX.COLNAME == null || CX.COLNAME.Length == 0)
            {
                throw new Exception("Parse Error: No column parsed. Invalid Query.");
            }


            GUI.setFilteredBoxes(true, true/*unattended*/, queryname, cumulatedargs/*queryargs*/,
                ADSChecked_, DIRXMLChecked_, CX.SPLITCOLUMNS,//splitColumns_,
                largeChecked_, notifyRecipients_, freqChecked_,
                domainChecked_, OUChecked_, dptChecked_, WUChecked_);
            GUI.setWQL("Query ready.");
            //Console.WriteLine("CX.QUERY: " + CX.QUERY);
        }


        public static void removeWhitespace(Context CX, SCCMReporting GUI, ref string QUERY)
        {
            if (GUI != null) GUI.setWQL("Remove extra White spaces...");
            QUERY = QUERY.Trim().Replace("\t", " ").Replace("\r", " ").Replace("\n", " ");
            int len = QUERY.Length; while ((QUERY = QUERY.Replace("  ", " ")).Length < len) len = QUERY.Length;
            if (CX != null) DebugLine(CX,"\r\n==========WHITE======================================================================\r\n" + QUERY);
            if (GUI != null) if (GUI.STOP) throw new Exception("User cancellation.");
        }

        public static void parseTables(string QUERY, Hashtable TABLENAME)
        {
            string q = QUERY;
            removeWhitespace(null, null, ref q);
            parseTables(null, null, ref q, TABLENAME);
        }

        public static void parseTables(Context CX, SCCMReporting GUI, ref string QUERY, Hashtable TABLENAME)
        {
            if (GUI != null) GUI.setWQL("Replacing Table aliases...");
            List<int> TABLEDEF = new List<int>();
            
            int ti = QUERY.ToUpper().IndexOf("FROM"); if (ti >= 0) TABLEDEF.Add(ti = ti + 4); else ti = 0;
            while ((ti = QUERY.ToUpper().IndexOf("JOIN", ti)) >= 0) TABLEDEF.Add(ti = ti + 4);
            foreach (int ti_ in TABLEDEF)
            {
                string sub = QUERY.Substring(ti_);
                string[] words = sub.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                //Console.WriteLine("table alias search sub: " + sub + "\nwords: " + words.Length);
                if (words.Length >= 3 && words[1].ToUpper().Equals("AS"))
                {
                    if (!TABLENAME.Contains(words[2]))
                        TABLENAME.Add(words[2], words[0]);
                    else if (CX != null)
                        DebugLine(CX,"\r\n==========TABLE==========\nAlias already defined: " + words[2] + "\r\n" + QUERY);
                    //Console.WriteLine("TABLE ALIAS: " + words[0] + " AS " + words[2]);
                }
            }
            try
            {
                QUERY = Regex.Replace(QUERY, "\\Was\\W", " AS ",
                         RegexOptions.IgnoreCase);
            }
            catch (Exception ee)
            {
                Console.WriteLine("replace as -> AS  ==> " + ee.ToString());
                if (GUI != null) GUI.setWQL("replace as -> AS  ==> " + ee.Message);
            }
        }

        public static void expandMultiops(Context CX, SCCMReporting GUI, ref string QUERY, bool show)
        {
            GUI.setWQL("Expanding Multiops...");
            for (int m = 0; m < multiops.Length; m++)
            {
                string
                    M0 = multista0.Replace("%multiops%", multiops[m]),
                    M = multista.Replace("%multiops%", multiops[m]),
                    M1 = multista1.Replace("%multiops%", multiops[m]),
                    U = unista.Replace("%uniops%", uniops[m])
                        .Replace("%logicops%", logicops[m]).Replace("%multiops%", multiops[m].Replace("\\W+", " ")),
                    U1 = unista1.Replace("%uniops%", uniops[m]);
                bool change; do
                {
                    string op = multiops[m].Replace("\\W+", " ");
                    /*DebugLine(CX,"\r\n==========MULTIOPS " + op + "============================================================\r\n"
                        + QUERY.Substring(QUERY.ToUpper().IndexOf("FROM") < 0 ? 0 : QUERY.ToUpper().IndexOf("FROM")));*/
                    bool c0, c1, c2, ce, c3;
                    if (c0 = Regex.IsMatch(QUERY, M0, IgnoreCaseSingleLine))
                        QUERY = Regex.Replace(QUERY, M0, "", IgnoreCaseSingleLine);
                    /*if (c0)
                        DebugLine(CX,"\r\n==========REGEX1 '" + M0 + "'" + " ==> ''\r\n"
                            + QUERY.Substring(QUERY.ToUpper().IndexOf("FROM") < 0 ? 0 : QUERY.ToUpper().IndexOf("FROM")));*/
                    if (c1 = Regex.IsMatch(QUERY, M, IgnoreCaseSingleLine))
                    {
                        bool member2null = Regex.Match(QUERY, M).Groups[3].Length == 0;
                        if (!member2null)
                            QUERY = Regex.Replace(QUERY, M, U, IgnoreCaseSingleLine);
                        else
                            QUERY = Regex.Replace(QUERY, M, U1, IgnoreCaseSingleLine);
                    }
                    /*if (c1)
                        DebugLine(CX,"\r\n==========REGEX2========== '" + M + "'" + " ==> '" + U + "'\r\n"
                            + QUERY.Substring(QUERY.ToUpper().IndexOf("FROM") < 0 ? 0 : QUERY.ToUpper().IndexOf("FROM")));*/
                    if (c2 = Regex.IsMatch(QUERY, M1, IgnoreCaseSingleLine))
                        QUERY = Regex.Replace(QUERY, M1, U1, IgnoreCaseSingleLine);
                    /*if (c2)
                        DebugLine(CX,"\r\n==========REGEX3 '" + M1 + "'" + " ==> '" + U1 + "'\r\n"
                            + QUERY.Substring(QUERY.ToUpper().IndexOf("FROM") < 0 ? 0 : QUERY.ToUpper().IndexOf("FROM")));*/

                    ce = false;
                    for (int n = 0; n < emptysta.Length; n++)
                    {
                        bool cn = Regex.IsMatch(QUERY, emptysta[n], IgnoreCaseSingleLine);
                        ce = ce || cn;
                        if (cn)
                            QUERY = Regex.Replace(QUERY, emptysta[n], "", IgnoreCaseSingleLine);
                    }

                    c3 = false;
                    if (Regex.IsMatch(QUERY, "\\s+(ANY|ALL)\\s+\"\"", IgnoreCaseSingleLine))
                    {
                        QUERY = Regex.Replace(QUERY, "\\s+(ANY|ALL)\\s+\"\"", " \"\"", IgnoreCaseSingleLine);
                        c3 = true;
                    }


                    change = c0 || c1 || c2 || ce || c3;
                    if (show)
                    {
                        if (change) DebugLine(CX,"\r\n==========MULTIOPS " + op + "============================================================\r\n"
                            + QUERY.Substring(QUERY.ToUpper().IndexOf("FROM") < 0 ? 0 : QUERY.ToUpper().IndexOf("FROM")));
                        else DebugLine(CX,"\r\n==========MULTIOPS " + op + "============================================================\r\n"
                            + M + " ==> " + U + "  -- not found.");
                    }
                } while (change);
            }
            if (GUI.STOP) throw new Exception("User cancellation.");
        }


        public static void replaceDateTime(Context CX, SCCMReporting GUI, ref string QUERY)
        {
            bool found1; int x = 1; do
            {
                GUI.setWQL("Expanding Datetime..." + (x++ > 1 ? "" + (x - 1) : ""));
                found1 = false;
                for (int z = 0; z < dtexp0.Length; z++)
                {
                    bool cdt1 = false;
                    int n = 1; string s = "", r = "", p = "", f = "";
                    if (cdt1 = Regex.IsMatch(QUERY, dtexp0[z], IgnoreCaseSingleLine))
                    {
                        s = dtexp0[z];
                        r = @"$1 ""%datetime%"" $5";
                        p = Regex.Replace(QUERY, dtexp0[z], "$2", IgnoreCaseSingleLine).ToLower();
                        f = Regex.Replace(QUERY, dtexp0[z], "$4", IgnoreCaseSingleLine);
                    }
                    if (!cdt1 && (cdt1 = Regex.IsMatch(QUERY, dtexp1[z], IgnoreCaseSingleLine)))
                    {
                        s = dtexp1[z];
                        r = @"$1 ""%datetime%"" $6";
                        string ns = Regex.Replace(QUERY, dtexp1[z], "$2", IgnoreCaseSingleLine);
                        try
                        {
                            n = int.Parse(ns);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Invalid Datetime variable multiplicator: " + ns);
                        }
                        p = Regex.Replace(QUERY, dtexp1[z], "$3", IgnoreCaseSingleLine).ToLower();
                        f = Regex.Replace(QUERY, dtexp1[z], "$5", IgnoreCaseSingleLine);
                    }
                    if (cdt1)
                    {
                        DebugLine(CX,"\r\n==========DATETIME======================================================================\r\n"
                            + "'" + s + "' ==> '" + r + "'\r\n"
                            + "n=" + n + " p=" + p + " f=" + f + "\r\n");
                        DateTime t = DateTime.Now;
                        if (p.StartsWith("yesterday"))
                        {
                            t = t.AddDays(-1);
                            DebugLine(CX,"days=-1 t=" + String.Format(new CultureInfo("en-US"), "{0:yyyy/MM/dd}", t));
                        }
                        else if (p.StartsWith("day"))
                        {
                            t = t.AddDays(-n);
                            DebugLine(CX,"days=-" + n + " t=" + String.Format(new CultureInfo("en-US"), "{0:yyyy/MM/dd}", t));
                        }
                        else if (p.StartsWith("week"))
                        {
                            t = t.AddDays(-7 * n);
                            DebugLine(CX,"days=-" + (7 * n) + " t=" + String.Format(new CultureInfo("en-US"), "{0:yyyy/MM/dd}", t));
                        }
                        else if (p.StartsWith("month"))
                        {
                            t = t.AddMonths(-n);
                            DebugLine(CX,"months=-" + n + " t=" + String.Format(new CultureInfo("en-US"), "{0:yyyy/MM/dd}", t));
                        }
                        string r2 = String.Format(new CultureInfo("en-US"), "{0:" + (f.Trim().Length == 0 ? "yyyy/MM/dd" : f) + "}", t);
                        QUERY = Regex.Replace(QUERY, s, r.Replace("%datetime%", r2), IgnoreCaseSingleLine);
                        DebugLine(CX,"\r\n==========DATETIME======================================================================\r\n"
                            + QUERY);
                        found1 = true;
                    }
                }
            } while (found1);
        }


        public static void replaceCollection(Context CX, SCCMReporting GUI, ref string QUERY, bool show)
        {
            GUI.setWQL("Replacing Collection Name by Collection ID...");
            bool change; do
            {
                if (change = Regex.IsMatch(QUERY, incollectionmatch, IgnoreCaseSingleLine))
                {
                    string before = Regex.Replace(QUERY, incollectionmatch, "$1", IgnoreCaseSingleLine);
                    string entity = Regex.Replace(QUERY, incollectionmatch, "$2", IgnoreCaseSingleLine);
                    string like = Regex.Replace(QUERY, incollectionmatch, "$3", IgnoreCaseSingleLine);
                    string after = Regex.Replace(QUERY, incollectionmatch, "$4", IgnoreCaseSingleLine);

                    List<SCCMCollectionDesigner.ColNode> CollectionItems = SCCMCollectionDesigner.getCollection(like);
                    if (CollectionItems.Count == 0)
                    {
                        GUI.setWQL("Error: Collection like \"" + like + "\" not found.");
                        throw new Exception("CollectionID not found. Invalid Query.");
                    }
                    StringBuilder subQ = new StringBuilder(); bool first = true; subQ.Append(" ( ");
                    int n = 1; foreach (SCCMCollectionDesigner.ColNode colItem in CollectionItems)
                    {
                        if (first)
                            first = false;
                        else
                            subQ.Append(" or ");
                        subQ.Append(entity + " in ( select ResourceID from SMS_CM_RES_COLL_" + colItem.CollectionID + " )");
                        GUI.setWQL("Info: Collection like \"" + like + "\" found. " + n++ + "/" + CollectionItems.Count);
                        GUI.setWQL("Info:     CollectionID=" + colItem.CollectionID);
                        GUI.setWQL("Info:         Name=" + colItem.Name);
                        GUI.setWQL("Info:         Comment=" + colItem.Comment);
                    }
                    subQ.Append(" ) ");
                    QUERY = Regex.Replace(QUERY, incollectionmatch, before + subQ + after, IgnoreCaseSingleLine);
                }
                if (show)
                {
                    if (change) DebugLine(CX,"\r\n==========IN COLLECTION============================================================\r\n"
                        + QUERY.Substring(QUERY.ToUpper().IndexOf("WHERE") < 0 ? 0 : QUERY.ToUpper().IndexOf("WHERE")));
                    else DebugLine(CX,"\r\n==========IN COLLECTION============================================================\r\n"
                        + "  -- not found.");
                }
            } while (change);

            
            do
            {
                if (change = Regex.IsMatch(QUERY, joincollectionmatch, IgnoreCaseSingleLine))
                {
                    string before = Regex.Replace(QUERY, joincollectionmatch, "$1", IgnoreCaseSingleLine);
                    string entity = Regex.Replace(QUERY, joincollectionmatch, "$2", IgnoreCaseSingleLine);
                    string like = Regex.Replace(QUERY, joincollectionmatch, "$3", IgnoreCaseSingleLine);
                    string after = Regex.Replace(QUERY, joincollectionmatch, "$4", IgnoreCaseSingleLine);

                    List<SCCMCollectionDesigner.ColNode> CollectionItems = SCCMCollectionDesigner.getCollection(like);
                    if (CollectionItems.Count == 0)
                    {
                        GUI.setWQL("Error: Collection like \"" + like + "\" not found.");
                        throw new Exception("CollectionID not found. Invalid Query.");
                    }
                    StringBuilder subQ = new StringBuilder();
                    int n = 1; foreach (SCCMCollectionDesigner.ColNode colItem in CollectionItems)
                    {
                        subQ.Append(" " + entity + " JOIN SMS_CM_RES_COLL_" + colItem.CollectionID + " ");
                        GUI.setWQL("Info: Collection like \"" + like + "\" found. " + n++ + "/" + CollectionItems.Count);
                        GUI.setWQL("Info:     CollectionID=" + colItem.CollectionID);
                        GUI.setWQL("Info:         Name=" + colItem.Name);
                        GUI.setWQL("Info:         Comment=" + colItem.Comment);
                    }
                    QUERY = Regex.Replace(QUERY, joincollectionmatch, before + subQ + after, IgnoreCaseSingleLine);
                }
                if (show)
                {
                    if (change) DebugLine(CX,"\r\n==========JOIN COLLECTION============================================================\r\n"
                        + QUERY.Substring(QUERY.ToUpper().IndexOf("WHERE") < 0 ? 0 : QUERY.ToUpper().IndexOf("WHERE")));
                    else DebugLine(CX,"\r\n==========JOIN COLLECTION============================================================\r\n"
                        + "  -- not found.");
                }
            } while (change);

            if (GUI.STOP) throw new Exception("User cancellation.");
        }



        public static void insertWhereClause(StringBuilder q, string whereClause)
        {
            int w = q.ToString().ToUpper().IndexOf("WHERE");
            string wc = w < 0 ? " WHERE " + whereClause : " " + whereClause + " AND ";
            w = w < 0 ? w = q.Length : w + 5;
            q.Insert(w, wc);
        }


        public static string indentWQL(string x)
        {
            StringBuilder ret = new StringBuilder();
            int level = 0, indent = 4;
            string newline = null;
            bool quote = false, where = false, space = true;
            char last = (char)0;
            for (int i = 0; i <= x.Length; i++)
            {
                if (newline != null)
                {
                    ret.Append('\n'); space = true;
                    for (int j = 0; j < indent * level; j++) ret.Append(' ');
                    ret.Append(newline);
                    if (newline.Trim().Length > 0) space = false;
                    if (newline.StartsWith(")")) newline = "";
                    newline = null;
                }
                if (i == x.Length) break;

                bool lasts = ("" + last).Replace('(', ' ').Replace(')', ' ').Trim().Length == 0;
                string sub = x.Substring(i);
                char c = x[i];
                if (!last.Equals('\\') && c == '\"') quote = !quote;

                if (c.Equals('\n')) newline = "";
                else if (lasts && Regex.IsMatch(sub, @"^from\W", IgnoreCaseSingleLine)) newline = "" + c;
                else if (lasts && Regex.IsMatch(sub, @"^(left\s+|right\s+|full\s+|inner\s+)?join\W", IgnoreCaseSingleLine)
                    && !Regex.IsMatch(ret.ToString(), @"\s(left|right|full|inner)\s$", IgnoreCaseSingleLine)) newline = "" + c;
                else if (!where && lasts && Regex.IsMatch(sub, @"^where\W", IgnoreCaseSingleLine)) { where = true; newline = "" + c; }
                else if (!quote && c.Equals('(')) { ret.Append("("); newline = ""; level++; }
                else if (!quote && c.Equals(')')) { newline = ")"; level--; if (level < 0) level = 0; }
                else if (c.Equals(' ')) { if (!space) ret.Append(c); }
                else { ret.Append(c); space = false; }
                last = c;
            }
            return Regex.Replace(ret.ToString(), @"\(\s*\)", "()", IgnoreCaseSingleLine);
        }


        public static string escapeRegexReservedChars(string x)
        {
            return x.Replace("[", "\\[").Replace("\\", "\\\\").Replace("^", "\\^").Replace("$", "\\$").Replace(".", "\\.")
                .Replace("|", "\\|").Replace("?", "\\?").Replace("*", "\\*").Replace("+", "\\+").Replace("(", "\\(").Replace(")", "\\)");
        }

        public static void expandInGroup(Context CX, SCCMReporting GUI, ref string ret)
        {
            bool found1; int x = 1; do
            {
                if (found1 = Regex.IsMatch(ret, ingroupmatch, IgnoreCaseSingleLine))
                {
                    GUI.setWQL("Expanding In Group..." + (x++ > 1 ? "" + (x - 1) : ""));

                    string varname = Regex.Replace(ret, ingroupmatch, "$2", IgnoreCaseSingleLine);
                    string list = Regex.Replace(ret, ingroupmatch, "$3", IgnoreCaseSingleLine);
                    string lucase = Regex.Replace(ret, ingroupmatch, "$4", IgnoreCaseSingleLine).Trim().ToUpper();
                    bool lower = lucase.Equals("LCASE"), upper = lucase.Equals("UCASE");
                    string admatch = Regex.Replace(ret, ingroupmatch, "$5", IgnoreCaseSingleLine).Trim();
                    if (admatch.StartsWith("\"")) admatch = admatch.Substring(1); if (admatch.EndsWith("\"")) admatch = admatch.Substring(0, admatch.Length - 1);
                    if (admatch.Length == 0) admatch = null;
                    string adreplace = Regex.Replace(ret, ingroupmatch, "$6", IgnoreCaseSingleLine).Trim();
                    if (adreplace.StartsWith("\"")) adreplace = adreplace.Substring(1); if (adreplace.EndsWith("\"")) adreplace = adreplace.Substring(0, adreplace.Length - 1);
                    string args = (upper ? " UCASE" : lower ? " LCASE" : "")
                        + (admatch == null ? "" : " \"" + admatch + "\"") + (adreplace == null ? "" : " \"" + adreplace + "\"");
                    string args_ = (upper ? @"\s+UCASE" : lower ? @"\s+LCASE" : "")
                        + (admatch == null ? "" : @"\s+""" + escapeRegexReservedChars(admatch) + @"""")
                        + (adreplace == null ? "" : @"\s+""" + escapeRegexReservedChars(adreplace) + @"""");
                    string match = ingroupexpr.Replace("%varname%", escapeRegexReservedChars(varname))
                        .Replace("%list%", escapeRegexReservedChars(list)).Replace("%args_%", args_);
                    string replace = ingrouplarge.Replace("%varname%", varname).Replace("%args%", args);
                    string lgroupindication = "Reference to a large group previously matched.";
                    if (!list.Equals(CX.LGROUPSTRING))
                    {
                        int listcount = list.Count(c => c == ';');
                        string expand = formatList(CX, GUI, list, true, null, null, false, false);
                        List<string> lgrouplist = new List<string>(expand.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Distinct());
                        CX.USERNOTFOUND = new List<string>(lgrouplist.Select(u => Regex.Replace(u, @".*\\", "")));
                        if (lgrouplist.Count > lgroupcountmax)
                        {
                            if (CX.LGROUPSTRING == null)
                            {
                                lgroupindication = "Reference to a large group.";
                                CX.LGROUPSTRING = list;
                                CX.LGROUPCOUNT = lgrouplist.Count;
                                CX.LGROUPSUBLISTS = new List<string>();
                                List<string> sub = new List<string>();
                                foreach (string i in lgrouplist)
                                {
                                    sub.Add(i.Trim());
                                    if (sub.Count == lgroupcountmax)
                                    {
                                        CX.LGROUPSUBLISTS.Add(sub.Aggregate((k, l) => k + "; " + l));
                                        sub.Clear();
                                    }
                                }
                                if (sub.Count > 0)
                                {
                                    CX.LGROUPSUBLISTS.Add(sub.Aggregate((k, l) => k + "; " + l));
                                    sub.Clear();
                                }
                                GUI.setWQL("Information: Large Group subquerying. "
                                    + listcount + " users submitted/"
                                    + CX.LGROUPCOUNT + " users validated"
                                    //+ "/" + CX.LGROUPSUBLISTS.Count + " subqueries"
                                    );
                            }
                            else
                            {
                                GUI.setWQL("Error: Multiple large groups subquerying not supported. Only one large group accepted.");
                                expand = formatList(CX, GUI, list, true, admatch, adreplace, lower, upper);
                                if (expand.Length == 0) expand = "%";
                                replace = ingroupany.Replace("%varname%", varname).Replace("%expand%", expand);
                            }
                        }
                        else
                        {
                            lgroupindication = "Reference to a simple group.";
                            expand = formatList(CX, GUI, list, true, admatch, adreplace, lower, upper);
                            if (expand.Length == 0) expand = "%";
                            replace = ingroupany.Replace("%varname%", varname).Replace("%expand%", expand);
                        }
                    }
                    ret = Regex.Replace(ret, match, replace, IgnoreCaseSingleLine);
                    DebugLine(CX,"\r\n==========INGROUP======================================================================\r\n"
                        + "match=" + match + "\r\nreplace=" + replace + "\r\nindication=" + lgroupindication + "\r\n" + ret);
                    if (GUI.STOP) throw new Exception("User cancellation.");
                }
                if (x > 20) throw new Exception("Group expansion: too many replacements");
            } while (found1);
        }

        public static void expandInLargeGroup(Context CX, SCCMReporting GUI, ref string ret, string sub)
        {
            bool found1; int x = 1; do
            {
                if (found1 = Regex.IsMatch(ret, ingrouplargematch, IgnoreCaseSingleLine))
                {
                    GUI.setWQL("Expanding In Large Group..." + (x++ > 1 ? "" + (x - 1) : ""));

                    string varname = Regex.Replace(ret, ingrouplargematch, "$2", IgnoreCaseSingleLine);
                    string lucase = Regex.Replace(ret, ingrouplargematch, "$3", IgnoreCaseSingleLine).Trim().ToUpper();
                    bool lower = lucase.Equals("LCASE"), upper = lucase.Equals("UCASE");
                    string admatch = Regex.Replace(ret, ingrouplargematch, "$4", IgnoreCaseSingleLine).Trim();
                    if (admatch.StartsWith("\"")) admatch = admatch.Substring(1); if (admatch.EndsWith("\"")) admatch = admatch.Substring(0, admatch.Length - 1);
                    if (admatch.Length == 0) admatch = null;
                    string adreplace = Regex.Replace(ret, ingrouplargematch, "$5", IgnoreCaseSingleLine).Trim();
                    if (adreplace.StartsWith("\"")) adreplace = adreplace.Substring(1); if (adreplace.EndsWith("\"")) adreplace = adreplace.Substring(0, adreplace.Length - 1);
                    string args = (upper ? " UCASE" : lower ? " LCASE" : "")
                        + (admatch == null ? "" : " \"" + admatch + "\"") + (adreplace == null ? "" : " \"" + adreplace + "\"");
                    string args_ = (upper ? @"\s+UCASE" : lower ? @"\s+LCASE" : "")
                        + (admatch == null ? "" : @"\s+""" + escapeRegexReservedChars(admatch) + @"""")
                        + (adreplace == null ? "" : @"\s+""" + escapeRegexReservedChars(adreplace) + @"""");
                    string match = ingrouplargeexpr.Replace("%varname%", escapeRegexReservedChars(varname))
                        .Replace("%args_%", args_);
                    string expand = formatList(CX, GUI, sub, false, admatch, adreplace, lower, upper);
                    if (expand.Length == 0) expand = "%";
                    string replace = ingroupany.Replace("%varname%", varname).Replace("%expand%", expand);

                    ret = Regex.Replace(ret, match, replace, IgnoreCaseSingleLine);
                    DebugLine(CX,"\r\n==========INLARGEGROUP======================================================================\r\n"
                        + "match=" + match + "\r\nreplace=" + replace + "\r\n" + ret);
                    if (GUI.STOP) throw new Exception("User cancellation.");
                }
                if (x > 20) throw new Exception("Large group expansion: too many replacements");
            } while (found1);
        }

        public static string formatList(Context CX, SCCMReporting GUI, string ADlist, bool resolve,
            string match, string replace, bool lower, bool upper)
        {
            if (resolve) return formatList_(CX, GUI, ADlist, match, replace, lower, upper);
            else
            {
                StringBuilder ret = new StringBuilder();
                foreach (string ADname in ADlist.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string user = formatUser(ADname.Trim(), match, replace, lower, upper);
                    ret.Append((ret.Length == 0 ? "" : "; ") + user);
                }
                return ret.ToString();
            }
        }

        public static string formatList_(Context CX, SCCMReporting GUI, string ADlist,
            string match, string replace, bool lower, bool upper)
        {
            string prevstatus = GUI.getStatus();
            GUI.setStatus("Expanding AD User/Group...");
            StringBuilder ret = new StringBuilder();
            List<string>[] ADnameQ = new List<string>[15];
            for (int i = 0; i < ADnameQ.Length; i++) ADnameQ[i] = new List<string>();
            int n = 0; foreach (string ADname in ADlist.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                ADnameQ[n++ % ADnameQ.Length].Add(ADname.Trim().ToUpper());
            ADnameQ = ADnameQ.Where(Q => Q.Count > 0).ToArray();
            int users = 0; ADnameQ.AsParallel().ForAll(Q =>
             {
                 DirectorySearcher USER_SEARCH = UsersInfo.getUserSearcher();
                 USER_SEARCH.PropertiesToLoad.Add("sAMAccountName");
                 foreach (string ADname_ in Q)
                 {                     
                     bool ALL = false, DIRECT = false;
                     string ADname = (ALL = ADname_.EndsWith("++")) ? ADname_.Substring(0, ADname_.Length - 2)
                         : (DIRECT = ADname_.EndsWith("+")) ? ADname_.Substring(0, ADname_.Length - 1)
                         : ADname_;
                     GUI.setStatus("Expanding AD User/Group '" + ADname + "'...", true);
                     string user = ALL || DIRECT ? null : formatUser(CX, USER_SEARCH, ADname, match, replace, lower, upper);
                     if (user != null) lock (ret)
                         {
                             users++;
                             ret.Append((ret.Length == 0 ? "" : "; ") + user);
                             GUI.setThreads(users + "/" + n);
                             //Console.WriteLine("Expanding AD User found: " + ADname);
                         }
                     if (user == null)
                     {

                         string group = ALL || DIRECT ?
                              formatReports(CX, GUI, ADname, ADname, match, replace, lower, upper, ALL)
                            : formatGroup(CX, GUI, ADname, ADname, match, replace, lower, upper);
                         lock (ret)
                         {
                             if (group != null)
                             {
                                 ret.Append((ret.Length == 0 ? "" : "; ") + group);
                                 //Console.WriteLine("Expanding AD Group found: " + ADname);
                             }
                             else
                             {
                                 GUI.setWQL("Error: Unknown AD User/Group '" + (ALL || DIRECT ? ADname_ : ADname) + "'");
                                 Console.WriteLine("Error: Unknown AD User/Group '" + (ALL || DIRECT ? ADname_ : ADname) + "'");
                             }
                         }
                     }
                     if (GUI.STOP) break;
                 }
             });
            GUI.setStatus(prevstatus, true);
            return ret.ToString();
        }

        public static string formatGroup(Context CX, SCCMReporting GUI, string ADname,
            string group, string match, string replace, bool lower, bool upper)
        {
            try
            {
                DomainCollection dc = Forest.GetCurrentForest().Domains;
                Domain[] domains = new Domain[dc.Count + 1]; dc.CopyTo(domains, 1);
                domains[0] = Forest.GetCurrentForest().RootDomain;
                SearchResult r = null; foreach (Domain d in domains)
                {
                    DirectorySearcher s = new DirectorySearcher();
                    s.SearchRoot = d.GetDirectoryEntry();
                    //Console.WriteLine("Search in '" + s.SearchRoot.Path + "'");
                    s.Filter = "(&(objectClass=group)(name=" + group + "))";
                    s.SearchScope = SearchScope.Subtree;
                    s.PropertiesToLoad.Add("name");
                    s.PropertiesToLoad.Add("member");
                    r = s.FindOne();
                    if (r != null) break;
                }
                if (r != null)
                {
                    //Console.WriteLine("expandGroup group '" + r.Path + "'");
                    List<DirectoryEntry> expand = new List<DirectoryEntry>();
                    expandGroup(GUI, ADname, r.GetDirectoryEntry(), expand);
                    return expand.Count == 0 ? null : expand.Select(x => formatUser(CX, x, match, replace, lower, upper))
                        .Aggregate((x, y) => x + "; " + y);
                }
                else throw new Exception("Group not found.");
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string formatReports(Context CX, SCCMReporting GUI, string ADname,
            string manager, string match, string replace, bool lower, bool upper, bool ALL)
        {
            try
            {
                DomainCollection dc = Forest.GetCurrentForest().Domains;
                Domain[] domains = new Domain[dc.Count + 1]; dc.CopyTo(domains, 1);
                domains[0] = Forest.GetCurrentForest().RootDomain;
                SearchResult r = null; foreach (Domain d in domains)
                {
                    DirectorySearcher s = new DirectorySearcher();
                    s.SearchRoot = d.GetDirectoryEntry();
                    //Console.WriteLine("Search in '" + s.SearchRoot.Path + "'");
                    s.Filter = "(&(objectClass=user)(sAMAccountName=" + manager + "))";
                    s.SearchScope = SearchScope.Subtree;
                    s.PropertiesToLoad.Add("directReports");
                    s.PropertiesToLoad.Add("employeeID");
                    r = s.FindOne();
                    if (r != null) break;
                }
                if (r != null)
                {
                    string mname = r.GetDirectoryEntry().Name.Replace("CN=", "").Replace("\\", "");
                    List<DirectoryEntry> expand = new List<DirectoryEntry>();
                    expandReports(GUI, ADname, r.GetDirectoryEntry(), expand, ALL);
                    GUI.setWQL("Information: Staff below '" + mname + "'"
                                    + expand.Count + " active employees found");

                    string ret = expand.Count == 0 ? null : expand.Select(x => formatUser(CX, x, match, replace, lower, upper))
                        .Aggregate((x, y) => x + "; " + y);
                    return ret;
                }
                else throw new Exception("Manager not found.");
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public static string formatUser(string user, string match, string replace, bool lower, bool upper)
        {
            string ret = user;
            if (lower) ret = ret.ToLower();
            if (upper) ret = ret.ToUpper();
            if (match != null) ret = Regex.Replace(ret, match, replace);
            return ret;
        }

        public static string formatUser(Context CX, DirectorySearcher USER_SEARCH,
            string user,        string match, string replace, bool lower, bool upper)
        {
            if (user.Length == 0) return null;
            string ret = CX.USERCACHE.Contains(user) ? (string)CX.USERCACHE[user]
                : UsersInfo.getDomainuser(user, USER_SEARCH);
            if (ret == null) return null;
            if (!CX.USERCACHE.Contains(user)) CX.USERCACHE.Add(user, ret);
            return formatUser(ret, match, replace, lower, upper);
        }

        public static string formatUser(Context CX,
            DirectoryEntry x,   string match, string replace, bool lower, bool upper)
        {
            string ret = CX.USERCACHE.Contains(x.Path) ? (string)CX.USERCACHE[x.Path]
                : UsersInfo.getDomainuser(x);
            if (ret == null) return null;
            if (!CX.USERCACHE.Contains(x.Path)) CX.USERCACHE.Add(x.Path, ret);
            return formatUser(ret, match, replace, lower, upper);
        }

        public static void expandGroup(SCCMReporting GUI, string ADname, DirectoryEntry group, List<DirectoryEntry> expand)
        {
            //Console.WriteLine("expandGroup group '" + group.Path + "'");
            object members = group.Invoke("Members", null);
            foreach (object member in (IEnumerable)members)
            {
                DirectoryEntry subgroup = new DirectoryEntry(member);
                string name = subgroup.Name.Replace("CN=", "").Replace("\\", "");
                if (subgroup.Properties["objectClass"].Contains("group"))
                {
                    GUI.setStatus("Expanding AD User/Group '" + ADname + "'... " + expand.Count + " ..." + name, true);
                    expandGroup(GUI, ADname, subgroup, expand);
                }
                else
                {
                    DirectoryEntry user = subgroup;
                    GUI.setStatus("Expanding AD User/Group '" + ADname + "'... " + expand.Count + " ..." + name, true);
                    //Console.WriteLine("expandGroup user '" + x.Name + "'");
                    expand.Add(user);
                }
            }
        }

        public static void expandReports(SCCMReporting GUI, string ADname, DirectoryEntry manager, List<DirectoryEntry> expand,
            bool ALL)
        {
            string mname = manager.Name.Replace("CN=", "").Replace("\\", "");
            Console.WriteLine("expandReports below '" + ADname + "' user '" + mname + "'");
            expand.Add(manager);
            GUI.setStatus("Expanding AD User/Group '" + ADname + "'... " + expand.Count + " ..." + mname, true);
            foreach (var u in manager.Properties["directReports"])
            {
                DirectoryEntry user = getDirectoryEntry(u.ToString());
                string name = user.Name.Replace("CN=", "").Replace("\\", "");
                bool isUser = user.Properties["employeeID"].Count > 0;
                bool isTerminated = (user.Properties["distinguishedName"].Count > 0 ?
                    user.Properties["distinguishedName"][0].ToString().ToLower() : "").Contains("terminated");

                if (isUser && !isTerminated)
                {
                    if (ALL) expandReports(GUI, ADname, user, expand, ALL);
                    else
                    {
                        Console.WriteLine("expandReports below '" + ADname + "' user    '" + name + "'"
                            //+ " manager '" + mname + "'"
                        );
                        expand.Add(user);
                        GUI.setStatus("Expanding AD User/Group '" + ADname + "'... " + expand.Count + " ..." + name, true);
                    }
                }/*
                else
                {
                    if (!isUser) Console.WriteLine("expandReports below '" + ADname + "' user '\t\t\t\t" + name + "'"
                        + " --- rejected: not an employee");
                    else if (isTerminated) Console.WriteLine("expandReports below '" + ADname + "' user '" + name + "'"
                        + " --- rejected: terminated");
                }*/
            }
        }

        public static DirectoryEntry getDirectoryEntry(string path)
        {
            string domain = path.Substring(path.ToUpper().IndexOf(",DC=") + 4).Replace(",DC=", ".");
            return new DirectoryEntry("LDAP://" + domain + "/" + path);
        }

        public static string containerW = "WORKSTATIONS%";
        public static string containerU = "USERS%";
        public static string getOUClause(List<string> domainChecked_, List<string> OUChecked_, List<string> dptChecked_, List<string> WUChecked_,
                           int domainBoxItemsCount, int OUBoxItemsCount, int dptBoxItemsCount,
                            List<string> ADSPATH)
        {
            /*Console.WriteLine("*****ouclause calc***** "
                + "Domain " + domainChecked_.Count + "/" + domainBoxItemsCount + " "
                + "Site" + OUChecked_.Count + "/" + OUBoxItemsCount + " "
                + "Department " + dptChecked_.Count + "/" + dptBoxItemsCount);
            foreach (string domain in domainChecked_) Console.WriteLine("Domain: " + domain);
            foreach (string OU in OUChecked_) Console.WriteLine("Site: " + OU);
            foreach (string dpt in dptChecked_) Console.WriteLine("Department: " + dpt);
            Console.WriteLine("*****ouclause calc***** ");*/


            bool checkW = WUChecked_.Contains("Computers");
            bool checkU1 = WUChecked_.Contains("TopUsers");
            bool checkU2 = WUChecked_.Contains("LastUsers");
            if (!checkW && !checkU1 && !checkU2) checkW = true;

            string W = "SMS_R_System.SystemOUName";
            string U1 = "U1.UserOUName";
            string U2 = "U2.UserOUName";

            if (    (domainChecked_.Count == domainBoxItemsCount || domainChecked_.Count == 0)
                &&  (OUChecked_.Count == OUBoxItemsCount || OUChecked_.Count == 0)
                &&  dptChecked_.Count == 0)
                return "";

            StringBuilder sb = new StringBuilder();
            if (checkW && !checkU1 && !checkU2)
                sb.AppendLine(
                      "ResourceID IN ( SELECT SMS_R_System.ResourceID FROM SMS_R_System"
                    + " WHERE ( ");
            else
                sb.AppendLine(
                      "ResourceID IN ( SELECT SMS_R_System.ResourceID FROM SMS_R_System"
                    + " LEFT JOIN SMS_G_System_SYSTEM_CONSOLE_USAGE ON SMS_R_System.ResourceID = SMS_G_System_SYSTEM_CONSOLE_USAGE.ResourceID"
                    + " LEFT JOIN SMS_R_User AS U1 ON SMS_G_System_SYSTEM_CONSOLE_USAGE.TopConsoleUser = U1.UniqueUserName"
                    + " LEFT JOIN SMS_R_User AS U2 ON SMS_R_System.LastLogonUserName = U2.UserName"
                    + " WHERE ( ");
            


            bool first = true;

            if (
                (domainChecked_.Count < domainBoxItemsCount)
                && (OUChecked_.Count < OUBoxItemsCount)
                && dptChecked_.Count > 0)
            {
                foreach (string domain in domainChecked_)
                    foreach (string OU in OUChecked_)
                        foreach (string dpt in dptChecked_)
                        {
                            string path1 = domain.ToUpper() + "/" + OU;
                            string path2 = domain.ToUpper() + "/" + OU + "/" + dpt;
                            if (ADSPATH.Contains(path2))
                            {
                                if (checkW)
                                {
                                    if (first) first = false;
                                    else sb.Append("OR ");
                                    sb.AppendLine(W + " like \"" + "%" + path2 + "[^A-Z]%\"");
                                }
                                if (checkU1)
                                {
                                    if (first) first = false;
                                    else sb.Append("OR ");
                                    sb.AppendLine(U1 + " like \"" + "%" + path2 + "[^A-Z]%\"");
                                }
                                if (checkU2)
                                {
                                    if (first) first = false;
                                    else sb.Append("OR ");
                                    sb.AppendLine(U2 + " like \"" + "%" + path2 + "[^A-Z]%\"");
                                }
                            }
                        }
            }
            else if (
              (domainChecked_.Count < domainBoxItemsCount)
              && (OUChecked_.Count < OUBoxItemsCount)
              && dptChecked_.Count == 0)
            {
                foreach (string domain in domainChecked_)
                    foreach (string OU in OUChecked_)
                    {
                        string path1 = domain.ToUpper() + "/" + OU;
                        if (ADSPATH.Contains(path1))
                        {
                            if( checkW) {
                                if (first) first = false;
                                else sb.Append("OR ");
                                sb.AppendLine(W + " like \"" + path1 + "[^A-Z]%\"");
                            }
                            if (checkU1)
                            {
                                if (first) first = false;
                                else sb.Append("OR ");
                                sb.AppendLine(U1 + " like \"" + path1 + "[^A-Z]%\"");
                            }
                            if (checkU2)
                            {
                                if (first) first = false;
                                else sb.Append("OR ");
                                sb.AppendLine(U2 + " like \"" + path1 + "[^A-Z]%\"");
                            }
                        }
                    }
            }
            else if (
              (domainChecked_.Count < domainBoxItemsCount)
              && (OUChecked_.Count == OUBoxItemsCount)
              && dptChecked_.Count > 0)
            {
                foreach (string domain in domainChecked_)
                    foreach (string dpt in dptChecked_)
                    {
                        if (checkW) {
                            if (first) first = false;
                            else sb.Append("OR ");
                            sb.AppendLine("( " + W + " like \"" + domain.ToUpper() + "/%\""
                                + " AND " + W + " like \"%/" + dpt + "/" + containerW + "\" )");
                        }
                        if (checkU1)
                        {
                            if (first) first = false;
                            else sb.Append("OR ");
                            sb.AppendLine("( " + U1 + " like \"" + domain.ToUpper() + "/%\""
                                + " AND " + U1 + " like \"%/" + dpt + "/" + containerU + "\" )");
                        }
                        if (checkU2)
                        {
                            if (first) first = false;
                            else sb.Append("OR ");
                            sb.AppendLine("( " + U2 + " like \"" + domain.ToUpper() + "/%\""
                                + " AND " + U2 + " like \"%/" + dpt + "/" + containerU + "\" )");
                        }
                    }
            }
            else if (
              (domainChecked_.Count < domainBoxItemsCount)
              && (OUChecked_.Count == OUBoxItemsCount)
              && dptChecked_.Count == 0)
            {
                foreach (string domain in domainChecked_)
                {
                    if (checkW) {
                        if (first) first = false;
                        else sb.Append("OR ");
                        sb.AppendLine(W + " like \"" + domain.ToUpper() + "/%\"");
                    }
                    if (checkU1)
                    {
                        if (first) first = false;
                        else sb.Append("OR ");
                        sb.AppendLine(U1 + " like \"" + domain.ToUpper() + "/%\"");
                    }
                    if (checkU2)
                    {
                        if (first) first = false;
                        else sb.Append("OR ");
                        sb.AppendLine(U2 + " like \"" + domain.ToUpper() + "/%\"");
                    }
                }
            }
            else if (
              (domainChecked_.Count == domainBoxItemsCount)
              && (OUChecked_.Count < OUBoxItemsCount)
              && dptChecked_.Count > 0)
            {
                foreach (string OU in OUChecked_)
                    foreach (string dpt in dptChecked_)
                    {
                        if (checkW) {
                            if (first) first = false;
                            else sb.Append("OR ");
                            sb.AppendLine(W + " like \"%/" + OU + "/" + dpt + "/" + containerW + "\"");
                        }
                        if (checkU1)
                        {
                            if (first) first = false;
                            else sb.Append("OR ");
                            sb.AppendLine(U1 + " like \"%/" + OU + "/" + dpt + "/" + containerU + "\"");
                        }
                        if (checkU2)
                        {
                            if (first) first = false;
                            else sb.Append("OR ");
                            sb.AppendLine(U2 + " like \"%/" + OU + "/" + dpt + "/" + containerU + "\"");
                        }
                    }
            }
            else if (
                (domainChecked_.Count == domainBoxItemsCount)
                && (OUChecked_.Count < OUBoxItemsCount)
                && dptChecked_.Count == 0)
            {
                foreach (string domain in domainChecked_)
                    foreach (string OU in OUChecked_)
                    {
                        string path1 = domain.ToUpper() + "/" + OU;
                        if (ADSPATH.Contains(path1))
                        {
                            if (checkW) {
                                if (first) first = false;
                                else sb.Append("OR ");
                                sb.AppendLine(W + " like \"" + path1 + "[^A-Z]%\"");
                            }
                            if (checkU1)
                            {
                                if (first) first = false;
                                else sb.Append("OR ");
                                sb.AppendLine(U1 + " like \"" + path1 + "[^A-Z]%\"");
                            }
                            if (checkU2)
                            {
                                if (first) first = false;
                                else sb.Append("OR ");
                                sb.AppendLine(U2 + " like \"" + path1 + "[^A-Z]%\"");
                            }
                        }
                    }
            }
            else if (
                (domainChecked_.Count == domainBoxItemsCount)
                && (OUChecked_.Count == OUBoxItemsCount)
                && dptChecked_.Count > 0)
            {
                foreach (string dpt in dptChecked_)
                {
                    if (checkW) {
                        if (first) first = false;
                        else sb.Append("OR ");
                        sb.AppendLine(W + " like \"%/" + dpt + "/" + containerW + "\"");
                    }
                    if (checkU1)
                    {
                        if (first) first = false;
                        else sb.Append("OR ");
                        sb.AppendLine(U1 + " like \"%/" + dpt + "/" + containerU + "\"");
                    }
                    if (checkU2)
                    {
                        if (first) first = false;
                        else sb.Append("OR ");
                        sb.AppendLine(U2 + " like \"%/" + dpt + "/" + containerU + "\"");
                    }
                }
            }


            sb.AppendLine(" ) )");
            return sb.ToString().Replace('\n', ' ');
        }


        public static string removeArgDuplicates(string args_)
        {
            Hashtable h = new Hashtable();
            List<string> l = new List<string>();
            string[] args = args_.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (args == null) return "";
            foreach (string arg in args)
            {
                string[] kv = arg.Split(new char[] { '=' }, 2, StringSplitOptions.None);
                string k = kv.Length > 0 ? kv[0].Trim() : "";
                string v = kv.Length > 1 ? kv[1].Trim() : "";
                if (h.Contains(k))
                {
                    h.Remove(k);
                    l.Remove(k);
                }
                h.Add(k, v);
                l.Add(k);
            }
            StringBuilder sb = new StringBuilder();
            foreach (string k in l) sb.Append((sb.Length > 0 ? ", " : "") + k + "=" + h[k]);
            return sb.ToString();
        }



        public static string getArg(StringBuilder args__, string argName)
        {
            return MachineInfo.getArg(args__, argName);
        }
       

        public static string unescapeFunc(string x)
        {
            return x.Replace("\\space", " ").Replace("\\comma", ",");
        }

        public static string escapeFunc(string x, bool insideQuotedOnly)//===REPLACEARG
        {
            if (!insideQuotedOnly)
                return x.Replace(" ", "\\space").Replace(",", "\\comma");
            else
            {
                int l0, l1;  do
                {
                    l0 = x.Length;
                    x = Regex.Replace(x, @"(""[^"" ]*) ([^"" ]+"")", @"$1\space$2", IgnoreCaseSingleLine);
                    x = Regex.Replace(x, @"(""[^"" ]+) ([^"" ]*"")", @"$1\space$2", IgnoreCaseSingleLine);
                    x = Regex.Replace(x, @"(""[^"" ]+) ", @"$1\space", IgnoreCaseSingleLine);
                    x = Regex.Replace(x, @"(""[^"",]*),([^"",]+"")", @"$1\comma$2", IgnoreCaseSingleLine);
                    x = Regex.Replace(x, @"(""[^"",]+),([^"",]*"")", @"$1\comma$2", IgnoreCaseSingleLine);
                    x = Regex.Replace(x, @"(""[^"" ]+),", @"$1\comma", IgnoreCaseSingleLine);
                    l1 = x.Length;
                } while (l1 != l0);
                return x;
            } 
        }











        static public RegexOptions IgnoreCaseSingleLine = RegexOptions.IgnoreCase | RegexOptions.Singleline;

        static string ingroupmatch = @"(.*\s)(\S+)\s+IN\s+GROUP\s+""([^""]*)""(\s+LCASE|\s+UCASE|)?(\s+""[^""]*"")?(\s+""[^""]*"")?(.*)";
        static string ingroupexpr = @"\s%varname%\s+IN\s+GROUP\s+""%list%""%args_%";
        //static string ingrouplarge = @"$1 %varname% IN LARGE GROUP %args% $7";
        //static string ingroupany = @"$1 ( %varname% EQUALS ANY ""%expand%"" ) $7";
        static string ingrouplarge = @" %varname% IN LARGE GROUP %args% ";
        static string ingroupany = @" ( %varname% EQUALS ANY ""%expand%"" ) ";
        static int lgroupcountmax = 150;

        static string incollectionmatch = @"(.*\s)(\S+)\s+IN\s+COLLECTION\s+""([^""]*)""(.*)";
        static string joincollectionmatch = @"(.*\s)(\S+)\s+JOIN\s+COLLECTION\s+""([^""]*)""(.*)";

        static string ingrouplargematch = @"(.*\s)(\S+)\s+IN\s+LARGE\s+GROUP(\s+LCASE|\s+UCASE|)?(\s+""[^""]*"")?(\s+""[^""]*"")?(.*)";
        static string ingrouplargeexpr = @"\s%varname%\s+IN\s+LARGE\s+GROUP%args_%";


        static string BaseTemplate = "_TEMPLATE_ - Base Template";
        static string DefaultLevel = "_TEMPLATE_ - Base Template - Level Medium";
        static string PrefixLevel = "_TEMPLATE_ - Base Template - Level ";

        static string[] dtexp0 = new string[] {
                @"(.*)%(now|yesterday)%(\s+DATETIMEFORMAT\s+""([^""]*)"")?(.*)",
                @"(.*)!(now|yesterday)!(\s+DATETIMEFORMAT\s+""([^""]*)"")?(.*)"
        };
        static string[] dtexp1 = new string[] {
                @"(.*)%(\d+)(days?ago|weeks?ago|months?ago)%(\s+DATETIMEFORMAT\s+""([^""]*)"")?(.*)",
                @"(.*)!(\d+)(days?ago|weeks?ago|months?ago)!(\s+DATETIMEFORMAT\s+""([^""]*)"")?(.*)"
        };

        static string[] multiops = new string[] {
                "NOT\\W+LIKE\\W+ANY",   "NOT\\W+EQUALS\\W+ANY",  "NOT\\W+LIKE\\W+ALL",   "NOT\\W+EQUALS\\W+ALL",
                "LIKE\\W+ANY",          "EQUALS\\W+ANY",         "LIKE\\W+ALL"
            };
        static string[] uniops = new string[] {
                "NOT LIKE",             "<>",                   "NOT LIKE",             "<>",
                "LIKE",                 "=",                    "LIKE"
            };
        static string[] logicops = new string[] {
                "OR",                   "OR",                   "AND",                  "AND",
                "OR",                   "OR",                   "AND"
            };
        static string
            multista0 = @"([\s\(])([\w\.]+)\s+%multiops%\s+""[\s%]+""",
            multista = @"([\s\(])([\w\.]+)\s+%multiops%\s+""\s*([^;""]+)\s*;([^""]*)""",
            multista1 = @"([\s\(])([\w\.]+)\s+%multiops%\s+""\s*([^;""]+)\s*""",
            unista = " $1$2 %uniops% \"$3\" %logicops% $2 %multiops% \"$4\"",
            unista1 = " $1$2 %uniops% \"$3\"";

        static string[] emptysta = new string[]{
            @"\sAND\s+\(\s+\)",
            @"\sOR\s+\(\s+\)",
            @"\(\s+\)\s+AND\s",
            @"\(\s+\)\s+OR\s"
        };

        static string combine0a = @".*\sAS\s+\[*(\w+)]*\s*\[([^\]]*)].*";
        static string combine0b = @"$2 ""$1""";
        static string combine0c = @"$1[%combine%]";
        static string[] combine1a = new string[] {
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",

            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s*]([^,%]*)",
        };
        static string[] combine1b_old = new string[] {
            (@",$1 AS! [$2$3]$13 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$13 KEEPIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$13 KEEPIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$13 KEEPIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$13 KEEPIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$13 KEEPIFMATCH ""$8"" COMBINE\n\t\t,$1 AS! [$2$9]$13 KEEPIFMATCH ""$9"" COMBINE\n\t\t,$1 AS! [$2$10]$13 KEEPIFMATCH ""$10"" COMBINE\n\t\t,$1 AS! [$2$11]$13 KEEPIFMATCH ""$11"" COMBINE\n\t\t,$1 AS! [$2$12]$13 KEEPIFMATCH ""$12"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 10 */
            (@",$1 AS! [$2$3]$12 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$12 KEEPIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$12 KEEPIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$12 KEEPIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$12 KEEPIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$12 KEEPIFMATCH ""$8"" COMBINE\n\t\t,$1 AS! [$2$9]$12 KEEPIFMATCH ""$9"" COMBINE\n\t\t,$1 AS! [$2$10]$12 KEEPIFMATCH ""$10"" COMBINE\n\t\t,$1 AS! [$2$11]$12 KEEPIFMATCH ""$11"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 09 */
            (@",$1 AS! [$2$3]$11 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$11 KEEPIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$11 KEEPIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$11 KEEPIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$11 KEEPIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$11 KEEPIFMATCH ""$8"" COMBINE\n\t\t,$1 AS! [$2$9]$11 KEEPIFMATCH ""$9"" COMBINE\n\t\t,$1 AS! [$2$10]$11 KEEPIFMATCH ""$10"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 08 */
            (@",$1 AS! [$2$3]$10 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$10 KEEPIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$10 KEEPIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$10 KEEPIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$10 KEEPIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$10 KEEPIFMATCH ""$8"" COMBINE\n\t\t,$1 AS! [$2$9]$10 KEEPIFMATCH ""$9"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 07 */
            (@",$1 AS! [$2$3]$9 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$9 KEEPIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$9 KEEPIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$9 KEEPIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$9 KEEPIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$9 KEEPIFMATCH ""$8"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 06 */

            @",$1 AS! [$2$3]$8 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$8 KEEPIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$8 KEEPIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$8 KEEPIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$8 KEEPIFMATCH ""$7"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 05 */
            @",$1 AS! [$2$3]$7 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$7 KEEPIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$7 KEEPIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$7 KEEPIFMATCH ""$6"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 04 */
            @",$1 AS! [$2$3]$6 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$6 KEEPIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$6 KEEPIFMATCH ""$5"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 03 */
            @",$1 AS! [$2$3]$5 KEEPIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$5 KEEPIFMATCH ""$4"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 02 */
            @",$1 AS! [$2$3]$4 KEEPIFMATCH ""$3"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 01 */
        };
        static string[] combine1b = new string[] {
            (@",$1 AS! [$2$3]$13 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$13 ONEIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$13 ONEIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$13 ONEIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$13 ONEIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$13 ONEIFMATCH ""$8"" COMBINE\n\t\t,$1 AS! [$2$9]$13 ONEIFMATCH ""$9"" COMBINE\n\t\t,$1 AS! [$2$10]$13 ONEIFMATCH ""$10"" COMBINE\n\t\t,$1 AS! [$2$11]$13 ONEIFMATCH ""$11"" COMBINE\n\t\t,$1 AS! [$2$12]$13 ONEIFMATCH ""$12"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 10 */
            (@",$1 AS! [$2$3]$12 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$12 ONEIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$12 ONEIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$12 ONEIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$12 ONEIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$12 ONEIFMATCH ""$8"" COMBINE\n\t\t,$1 AS! [$2$9]$12 ONEIFMATCH ""$9"" COMBINE\n\t\t,$1 AS! [$2$10]$12 ONEIFMATCH ""$10"" COMBINE\n\t\t,$1 AS! [$2$11]$12 ONEIFMATCH ""$11"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 09 */
            (@",$1 AS! [$2$3]$11 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$11 ONEIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$11 ONEIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$11 ONEIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$11 ONEIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$11 ONEIFMATCH ""$8"" COMBINE\n\t\t,$1 AS! [$2$9]$11 ONEIFMATCH ""$9"" COMBINE\n\t\t,$1 AS! [$2$10]$11 ONEIFMATCH ""$10"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 08 */
            (@",$1 AS! [$2$3]$10 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$10 ONEIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$10 ONEIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$10 ONEIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$10 ONEIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$10 ONEIFMATCH ""$8"" COMBINE\n\t\t,$1 AS! [$2$9]$10 ONEIFMATCH ""$9"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 07 */
            (@",$1 AS! [$2$3]$9 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$9 ONEIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$9 ONEIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$9 ONEIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$9 ONEIFMATCH ""$7"" COMBINE\n\t\t" + 
            @",$1 AS! [$2$8]$9 ONEIFMATCH ""$8"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 06 */

            @",$1 AS! [$2$3]$8 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$8 ONEIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$8 ONEIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$8 ONEIFMATCH ""$6"" COMBINE\n\t\t,$1 AS! [$2$7]$8 ONEIFMATCH ""$7"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 05 */
            @",$1 AS! [$2$3]$7 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$7 ONEIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$7 ONEIFMATCH ""$5"" COMBINE\n\t\t,$1 AS! [$2$6]$7 ONEIFMATCH ""$6"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 04 */
            @",$1 AS! [$2$3]$6 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$6 ONEIFMATCH ""$4"" COMBINE\n\t\t,$1 AS! [$2$5]$6 ONEIFMATCH ""$5"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 03 */
            @",$1 AS! [$2$3]$5 ONEIFMATCH ""$3"" COMBINE\n\t\t,$1 AS! [$2$4]$5 ONEIFMATCH ""$4"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 02 */
            @",$1 AS! [$2$3]$4 ONEIFMATCH ""$3"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 01 */
        };
        static string combine2a = @"(,\s*\S+\s+AS\s+\S+)";
        static string[] combine3a = new string[] {
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",

            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
            @",\s*(\S+)\s+AS\s+\[*(\w+)]*\s*\[\s*""([^""]+)""\s+""([^""]+)""\s*]([^,%]*)",
        };
        static string[] combine3b_old = new string[] {
            (@",$1 AS [$2$3]$14 KEEPIFCOLMATCH ""$13$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$14 KEEPIFCOLMATCH ""$13$4"" ""$4"" COMBINE\n\t\t,$1 AS [$2$5]$14 KEEPIFCOLMATCH ""$13$5"" ""$5"" COMBINE\n\t\t,$1 AS [$2$6]$14 KEEPIFCOLMATCH ""$13$6"" ""$6"" COMBINE\n\t\t,$1 AS [$2$7]$14 KEEPIFCOLMATCH ""$13$7"" ""$7"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$14 KEEPIFCOLMATCH ""$13$8"" ""$8"" COMBINE\n\t\t,$1 AS [$2$9]$14 KEEPIFCOLMATCH ""$13$9"" ""$9"" COMBINE\n\t\t,$1 AS [$2$10]$14 KEEPIFCOLMATCH ""$13$10"" ""$10"" COMBINE\n\t\t,$1 AS [$2$11]$14 KEEPIFCOLMATCH ""$13$11"" ""$11"" COMBINE\n\t\t,$1 AS [$2$12]$14 KEEPIFCOLMATCH ""$13$12"" ""$12"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 10 */
            (@",$1 AS [$2$3]$13 KEEPIFCOLMATCH ""$12$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$13 KEEPIFCOLMATCH ""$12$4"" ""$4"" COMBINE\n\t\t,$1 AS [$2$5]$13 KEEPIFCOLMATCH ""$12$5"" ""$5"" COMBINE\n\t\t,$1 AS [$2$6]$13 KEEPIFCOLMATCH ""$12$6"" ""$6"" COMBINE\n\t\t,$1 AS [$2$7]$13 KEEPIFCOLMATCH ""$12$7"" ""$7"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$13 KEEPIFCOLMATCH ""$12$8"" ""$8"" COMBINE\n\t\t,$1 AS [$2$9]$13 KEEPIFCOLMATCH ""$12$9"" ""$9"" COMBINE\n\t\t,$1 AS [$2$10]$13 KEEPIFCOLMATCH ""$12$10"" ""$10"" COMBINE\n\t\t,$1 AS [$2$11]$13 KEEPIFCOLMATCH ""$12$11"" ""$11"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 09 */
            (@",$1 AS [$2$3]$12 KEEPIFCOLMATCH ""$11$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$12 KEEPIFCOLMATCH ""$11$4"" ""$4"" COMBINE\n\t\t,$1 AS [$2$5]$12 KEEPIFCOLMATCH ""$11$5"" ""$5"" COMBINE\n\t\t,$1 AS [$2$6]$12 KEEPIFCOLMATCH ""$11$6"" ""$6"" COMBINE\n\t\t,$1 AS [$2$7]$12 KEEPIFCOLMATCH ""$11$7"" ""$7"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$12 KEEPIFCOLMATCH ""$11$8"" ""$8"" COMBINE\n\t\t,$1 AS [$2$9]$12 KEEPIFCOLMATCH ""$11$9"" ""$9"" COMBINE\n\t\t,$1 AS [$2$10]$12 KEEPIFCOLMATCH ""$11$10"" ""$10"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 08 */
            (@",$1 AS [$2$3]$11 KEEPIFCOLMATCH ""$10$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$11 KEEPIFCOLMATCH ""$10$4"" ""$4"" COMBINE\n\t\t,$1 AS [$2$5]$11 KEEPIFCOLMATCH ""$10$5"" ""$5"" COMBINE\n\t\t,$1 AS [$2$6]$11 KEEPIFCOLMATCH ""$10$6"" ""$6"" COMBINE\n\t\t,$1 AS [$2$7]$11 KEEPIFCOLMATCH ""$10$7"" ""$7"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$11 KEEPIFCOLMATCH ""$10$8"" ""$8"" COMBINE\n\t\t,$1 AS [$2$9]$11 KEEPIFCOLMATCH ""$10$9"" ""$9"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 07 */
            (@",$1 AS [$2$3]$10 KEEPIFCOLMATCH ""$9$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$10 KEEPIFCOLMATCH ""$9$4"" ""$4"" COMBINE\n\t\t,$1 AS [$2$5]$10 KEEPIFCOLMATCH ""$9$5"" ""$5"" COMBINE\n\t\t,$1 AS [$2$6]$10 KEEPIFCOLMATCH ""$9$6"" ""$6"" COMBINE\n\t\t,$1 AS [$2$7]$10 KEEPIFCOLMATCH ""$9$7"" ""$7"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$10 KEEPIFCOLMATCH ""$9$8"" ""$8"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 06 */

            @",$1 AS [$2$3]$9 KEEPIFCOLMATCH ""$8$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$9 KEEPIFCOLMATCH ""$8$4"" ""$4"" COMBINE\n\t\t,$1 AS [$2$5]$9 KEEPIFCOLMATCH ""$8$5"" ""$5"" COMBINE\n\t\t,$1 AS [$2$6]$9 KEEPIFCOLMATCH ""$8$6"" ""$6"" COMBINE\n\t\t,$1 AS [$2$7]$9 KEEPIFCOLMATCH ""$8$7"" ""$7"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 05 */
            @",$1 AS [$2$3]$8 KEEPIFCOLMATCH ""$7$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$8 KEEPIFCOLMATCH ""$7$4"" ""$4"" COMBINE\n\t\t,$1 AS [$2$5]$8 KEEPIFCOLMATCH ""$7$5"" ""$5"" COMBINE\n\t\t,$1 AS [$2$6]$8 KEEPIFCOLMATCH ""$7$6"" ""$6"" COMBINE\n\t\t\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 04 */
            @",$1 AS [$2$3]$7 KEEPIFCOLMATCH ""$6$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$7 KEEPIFCOLMATCH ""$6$4"" ""$4"" COMBINE\n\t\t,$1 AS [$2$5]$7 KEEPIFCOLMATCH ""$6$5"" ""$5"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 03 */
            @",$1 AS [$2$3]$6 KEEPIFCOLMATCH ""$5$3"" ""$3"" COMBINE\n\t\t,$1 AS [$2$4]$6 KEEPIFCOLMATCH ""$5$4"" ""$4"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 02 */
            @",$1 AS [$2$3]$5 KEEPIFCOLMATCH ""$4$3"" ""$3"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 01 */
        };
        static string[] combine3b = new string[] {
            (@",$1 AS [$2$3]$14 KEEPIFCOLMATCH ""$13$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$14 KEEPIFCOLMATCH ""$13$4"" ""1"" COMBINE\n\t\t,$1 AS [$2$5]$14 KEEPIFCOLMATCH ""$13$5"" ""1"" COMBINE\n\t\t,$1 AS [$2$6]$14 KEEPIFCOLMATCH ""$13$6"" ""1"" COMBINE\n\t\t,$1 AS [$2$7]$14 KEEPIFCOLMATCH ""$13$7"" ""1"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$14 KEEPIFCOLMATCH ""$13$8"" ""1"" COMBINE\n\t\t,$1 AS [$2$9]$14 KEEPIFCOLMATCH ""$13$9"" ""1"" COMBINE\n\t\t,$1 AS [$2$10]$14 KEEPIFCOLMATCH ""$13$10"" ""1"" COMBINE\n\t\t,$1 AS [$2$11]$14 KEEPIFCOLMATCH ""$13$11"" ""1"" COMBINE\n\t\t,$1 AS [$2$12]$14 KEEPIFCOLMATCH ""$13$12"" ""1"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 10 */
            (@",$1 AS [$2$3]$13 KEEPIFCOLMATCH ""$12$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$13 KEEPIFCOLMATCH ""$12$4"" ""1"" COMBINE\n\t\t,$1 AS [$2$5]$13 KEEPIFCOLMATCH ""$12$5"" ""1"" COMBINE\n\t\t,$1 AS [$2$6]$13 KEEPIFCOLMATCH ""$12$6"" ""1"" COMBINE\n\t\t,$1 AS [$2$7]$13 KEEPIFCOLMATCH ""$12$7"" ""1"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$13 KEEPIFCOLMATCH ""$12$8"" ""1"" COMBINE\n\t\t,$1 AS [$2$9]$13 KEEPIFCOLMATCH ""$12$9"" ""1"" COMBINE\n\t\t,$1 AS [$2$10]$13 KEEPIFCOLMATCH ""$12$10"" ""1"" COMBINE\n\t\t,$1 AS [$2$11]$13 KEEPIFCOLMATCH ""$12$11"" ""1"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 09 */
            (@",$1 AS [$2$3]$12 KEEPIFCOLMATCH ""$11$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$12 KEEPIFCOLMATCH ""$11$4"" ""1"" COMBINE\n\t\t,$1 AS [$2$5]$12 KEEPIFCOLMATCH ""$11$5"" ""1"" COMBINE\n\t\t,$1 AS [$2$6]$12 KEEPIFCOLMATCH ""$11$6"" ""1"" COMBINE\n\t\t,$1 AS [$2$7]$12 KEEPIFCOLMATCH ""$11$7"" ""1"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$12 KEEPIFCOLMATCH ""$11$8"" ""1"" COMBINE\n\t\t,$1 AS [$2$9]$12 KEEPIFCOLMATCH ""$11$9"" ""1"" COMBINE\n\t\t,$1 AS [$2$10]$12 KEEPIFCOLMATCH ""$11$10"" ""1"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 08 */
            (@",$1 AS [$2$3]$11 KEEPIFCOLMATCH ""$10$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$11 KEEPIFCOLMATCH ""$10$4"" ""1"" COMBINE\n\t\t,$1 AS [$2$5]$11 KEEPIFCOLMATCH ""$10$5"" ""1"" COMBINE\n\t\t,$1 AS [$2$6]$11 KEEPIFCOLMATCH ""$10$6"" ""1"" COMBINE\n\t\t,$1 AS [$2$7]$11 KEEPIFCOLMATCH ""$10$7"" ""1"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$11 KEEPIFCOLMATCH ""$10$8"" ""1"" COMBINE\n\t\t,$1 AS [$2$9]$11 KEEPIFCOLMATCH ""$10$9"" ""1"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 07 */
            (@",$1 AS [$2$3]$10 KEEPIFCOLMATCH ""$9$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$10 KEEPIFCOLMATCH ""$9$4"" ""1"" COMBINE\n\t\t,$1 AS [$2$5]$10 KEEPIFCOLMATCH ""$9$5"" ""1"" COMBINE\n\t\t,$1 AS [$2$6]$10 KEEPIFCOLMATCH ""$9$6"" ""1"" COMBINE\n\t\t,$1 AS [$2$7]$10 KEEPIFCOLMATCH ""$9$7"" ""1"" COMBINE\n\t\t" + 
            @",$1 AS [$2$8]$10 KEEPIFCOLMATCH ""$9$8"" ""1"" COMBINE\n\t\t"
            ).Replace("\\t", "\t").Replace("\\n", "\n"),/* 06 */

            @",$1 AS [$2$3]$9 KEEPIFCOLMATCH ""$8$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$9 KEEPIFCOLMATCH ""$8$4"" ""1"" COMBINE\n\t\t,$1 AS [$2$5]$9 KEEPIFCOLMATCH ""$8$5"" ""1"" COMBINE\n\t\t,$1 AS [$2$6]$9 KEEPIFCOLMATCH ""$8$6"" ""1"" COMBINE\n\t\t,$1 AS [$2$7]$9 KEEPIFCOLMATCH ""$8$7"" ""1"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 05 */
            @",$1 AS [$2$3]$8 KEEPIFCOLMATCH ""$7$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$8 KEEPIFCOLMATCH ""$7$4"" ""1"" COMBINE\n\t\t,$1 AS [$2$5]$8 KEEPIFCOLMATCH ""$7$5"" ""1"" COMBINE\n\t\t,$1 AS [$2$6]$8 KEEPIFCOLMATCH ""$7$6"" ""1"" COMBINE\n\t\t\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 04 */
            @",$1 AS [$2$3]$7 KEEPIFCOLMATCH ""$6$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$7 KEEPIFCOLMATCH ""$6$4"" ""1"" COMBINE\n\t\t,$1 AS [$2$5]$7 KEEPIFCOLMATCH ""$6$5"" ""1"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 03 */
            @",$1 AS [$2$3]$6 KEEPIFCOLMATCH ""$5$3"" ""1"" COMBINE\n\t\t,$1 AS [$2$4]$6 KEEPIFCOLMATCH ""$5$4"" ""1"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 02 */
            @",$1 AS [$2$3]$5 KEEPIFCOLMATCH ""$4$3"" ""1"" COMBINE\n\t\t"
            .Replace("\\t", "\t").Replace("\\n", "\n"),/* 01 */
        };



    }
}
