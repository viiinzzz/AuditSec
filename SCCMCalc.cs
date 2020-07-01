using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Management;
using System.IO;


namespace AuditSec
{
    class SCCMCalc
    {

        /*static string defaultDateFormats =
            "yyyyMMdd;yyyy/M/d;dd/MM/yyyy;M/d/yyyy;d/MMM/yyyy;dd/MM/yy;"
            + "dd-MM-yy;d-MMM-yyyy;d-M-yyyy;dd-MM-yyyy;yyyy-M-d;"
            + "d.M.yyyy;yyyy.MM.dd.";*/

        static string queriestmp = Path.Combine(Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.LocalApplicationData), "MYCOMPANY"), "sccmqueries.wql.tmp");

        static string defaultDateFormats = "";
        static bool defaultDateFormats_initialized = defaultDateFormats_initialize();
        static bool defaultDateFormats_initialize()
        {
            try
            {
                if (!AuditSec.exportResource("sccmqueries", queriestmp, true))
                    throw new Exception("File installation failure");

                string queryname = null;
                StringBuilder queryargs = new StringBuilder();
                StringBuilder querydef = new StringBuilder();
                if (File.Exists(queriestmp))
                    foreach (string line in File.ReadAllLines(queriestmp))
                        if (!line.StartsWith("#") && line.Trim().Length > 0)
                        {
                            if (line.StartsWith("¤"))
                            {
                                if (queryname != null && queryname.Length > 0
                                    && querydef != null && querydef.Length > 0)
                                {
                                    if (queryname.ToUpper().StartsWith("_TEMPLATE_ - BASE VARIABLES"))
                                    {
                                        string v = SCCMParser.getArg(queryargs, "defaultDateFormats");
                                        defaultDateFormats = v;
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
                if (queryname != null && queryname.Length > 0 && querydef != null && querydef.Length > 0)
                {
                    while (queryargs.Length > 0 && (
                        queryargs[0] == ' ' || queryargs[0] == '\n'
                        || queryargs[0] == '\t' || queryargs[0] == ','))
                        queryargs.Remove(0, 1);
                }
                Console.WriteLine("Replace Strings loaded from file: \"" + queriestmp + "\""
                        + "\ndefaultDateFormats=" + defaultDateFormats
                );
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot initialize Replace Strings, failed to load from file: \"" + queriestmp + "\""
                + e.Message + "\n"
                + e.StackTrace);
                return false;
            }
        }


        public static string getColAlias(SCCMParser.Context CX, int i)
        {
            if (i < CX.COLALIAS.Length) return CX.COLALIAS[i];
            i -= CX.COLALIAS.Length;
            if (i < CX.ADSCOLNAME.Length) return CX.ADSCOLNAME[i];
            i -= CX.ADSCOLNAME.Length;
            return CX.DIRXMLCOLNAME[i];
        }

        public static Type getColType(SCCMParser.Context CX, int i)
        {
            if (i < CX.COLTYPE.Length) return CX.COLTYPE[i];
            i -= CX.COLTYPE.Length;
            if (i < CX.ADSCOLTYPE.Length) return CX.ADSCOLTYPE[i];
            i -= CX.ADSCOLTYPE.Length;
            return CX.DIRXMLCOLTYPE[i];
        }

        public static bool getColNoAggreg(SCCMParser.Context CX, int i)
        {
            if (i < CX.COLNOAGGREG.Length) return CX.COLNOAGGREG[i];
            return false;
        }

        public static string getColAggregFlag(SCCMParser.Context CX, int i)
        {
            if (i < CX.COLAGGREGFLAG.Length) return CX.COLAGGREGFLAG[i] == null ? "" : CX.COLAGGREGFLAG[i];
            return "";
        }


        public static object processValue(SCCMParser.Context CX, Func<string, bool> setWQL,
            int i, object value, CimType type, List<object> row, ref bool DELETEROW, StringBuilder sb,
            Func<string, object, object> getWMI)
        {
            object v = null;
            if (value == null)
                //zero values
                v = getZeroValue(CX, i, type, sb);
            else //non-null values
                if (CX.COLTYPE[i].Equals(typeof(DateTime))
                || (type != CimType.None && type.Equals(CimType.DateTime)))
                {
                    //sb.AppendLine("Column [" + COLALIAS[i] + "] TYPE DateTime request:" + value.GetType().Name + " <" + value.ToString() + ">");
                    if (value.ToString().Length == 0) v = DateTime.MinValue;
                    else try //DMTF Date ie. 20020408141835.999999-420
                        {
                            if (!(value is DateTime))
                            {
                                v = ManagementDateTimeConverter.ToDateTime(value.ToString());
                                //sb.AppendLine("Column [" + COLALIAS[i] + "] TYPE DateTime request: DMTFCONV success:"  + v.GetType().Name + " <" + v.ToString() + ">");
                            }
                            //sb.AppendLine("================================datetime already: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception e)
                        {
                            v = value.ToString();
                            //sb.AppendLine("Column [" + COLALIAS[i] + "] TYPE DateTime request: DMTFCONV failed:" + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                }

             
                else if (CX.COLTYPE[i].Equals(typeof(int)) || (type != CimType.None &&
                    (type.Equals(CimType.SInt8) || type.Equals(CimType.UInt8)
                    || type.Equals(CimType.SInt16) || type.Equals(CimType.UInt16)
                    || type.Equals(CimType.SInt32) || type.Equals(CimType.UInt32)
                    || type.Equals(CimType.SInt64) || type.Equals(CimType.UInt64)
                    )))
                {
                    int vint = 0;
                    if (int.TryParse(value.ToString(), out vint)) v = vint;
                    else v = value.ToString();
                }

                else if (CX.COLTYPE[i].Equals(typeof(double)) || (type != CimType.None &&
                    (type.Equals(CimType.Real32) || type.Equals(CimType.Real64)
                    )))
                {
                    double vdouble = 0;
                    if (double.TryParse(value.ToString(), out vdouble)) v = vdouble;
                    else v = value.ToString();
                }

                else if (CX.COLTYPE[i].Equals(typeof(bool)) || (type != CimType.None &&
                    type.Equals(CimType.Boolean)
                    ))
                {
                    bool vbool = false;
                    if (bool.TryParse(value.ToString(), out vbool)) v = vbool;
                    else v = value.ToString();
                }

                else if (CX.COLTYPE[i].Equals(typeof(char)) || (type != CimType.None &&
                    type.Equals(CimType.Char16)
                    ))
                {
                    char vchar = (char)0;
                    if (char.TryParse(value.ToString(), out vchar)) v = vchar;
                    else v = value.ToString();
                }

                else if (CX.COLTYPE[i].Equals(typeof(List<string>))
                    || value.GetType().IsArray)
                {
                    List<string> values = value.GetType().IsArray ?
                        new List<object>((object[])value).Select(obj => obj.ToString()).ToList() :
                        new List<string>(value.ToString().Split(';'));
                    v = values.Aggregate((x, y) => x.Trim() + "; " + y.Trim());
                }

                else
                {
                    v = value.ToString(); //typeof(string), CIM_String, unknown type ...
                }

            v = applyFunc(CX, setWQL, i, v, row, ref DELETEROW, sb, getWMI);

            if (v == null) v = getZeroValue(CX, i, CimType.None, sb);

            /*if (COLTYPE[i].Equals(typeof(DateTime)))
            {
                Console.WriteLine("\n\n\n" + sb.ToString());
            }*/

            row.Add(v);
            return v;
        }



        
        public static object applyFunc(SCCMParser.Context CX, Func<string, bool> setWQL,
                                       int colindex, object v, List<object> row, ref bool DELETEROW, StringBuilder sb,
                                        Func<string, object, object> getWMI)
        {
            if (v == null) v = "";

            if (CX.COLAGGREGFLAG[colindex] != null && CX.COLAGGREGFLAG[colindex].Equals("COUNT"))
                return v = v.ToString().Length > 0 ? "1" : "";

            List<string> f_ = CX.COLFUNC[colindex];
            if (f_ == null) return v;
            if (getColType(CX, colindex) == typeof(List<String>))
            {
                List<string> vl = v is List<string> ? (List<string>)v : new List<string>(v.ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(m => (m.EndsWith(";") ? m.Substring(0, m.Length - 1) : m).Trim()).ToList();
                List<string> ret = new List<string>();
                foreach(string vli in vl)
                {
                    string vv = vli;
                    List<string> f = f_.ToList();
                    while (f.Count > 0)
                    {
                        string func = f.ElementAt(0).ToUpper(); f.RemoveAt(0);
                        if ("RETURN".Equals(func)) break;
                        else
                            vv = applyFunc(CX, setWQL, colindex, vv, row, ref DELETEROW, sb, func, f, getWMI).ToString();
                    }
                    ret.Add(vv);
                }
                return ret.Aggregate((x, y) => x + ";\n" + y);
            }
            else
            {
                List<string> f = f_.ToList();
                while (f.Count > 0)
                {
                    string func = f.ElementAt(0).ToUpper(); f.RemoveAt(0);
                    if ("RETURN".Equals(func)) break;
                    else
                        v = applyFunc(CX, setWQL, colindex, v, row, ref DELETEROW, sb, func, f, getWMI);
                }
                return v;
            }
        }
        


        public static object applyFunc(SCCMParser.Context CX, Func<string, bool> setWQL,
                        int colindex, object v, List<object> row, ref bool DELETEROW, StringBuilder sb,
                        string func, List<string> f,
                        Func<string, object, object> getWMI)
        {
            switch (func)
            {
                case "REPLACE":
                    if (sb != null) sb.AppendLine("Apply Function: REPLACE: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    string arg1 = null, arg2 = null;
                    if (f.Count >= 2) try
                        {
                            arg1 = f.ElementAt(0); arg2 = f.ElementAt(1);
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
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            if (sb != null) sb.AppendLine("\targ2: " + arg2);
                            v = Regex.Replace(v.ToString(), arg1, arg2);
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: REPLACE " + arg1 + " " + arg2
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 2);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: REPLACE "
                        + (f.Count > 0 ? f.ElementAt(0) : "") + (f.Count > 1 ? " " + f.ElementAt(1) : ""));
                    }
                    break;
                case "UCASE":
                    if (sb != null) sb.AppendLine("Apply Function: UCASE: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    v = v.ToString().ToUpper();
                    if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                    break;
                case "LCASE":
                    if (sb != null) sb.AppendLine("Apply Function: LCASE: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    v = v.ToString().ToLower();
                    if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                    break;
                case "TRIM":
                    if (sb != null) sb.AppendLine("Apply Function: TRIM: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    v = v.ToString().Trim();
                    if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                    break;

                case "SCHEDULETOKEN":
                    if (sb != null) sb.AppendLine("Apply Function: SCHEDULETOKEN: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    SMS_ScheduleToken st = parseScheduleToken(v.ToString());
                    v = st.IsFinished() ? "" : st.ToString();
                    if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                    break;

                case "DATETIMEFORMAT":
                    if (sb != null) sb.AppendLine("Apply Function: DATETIMEFORMAT: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (arg1.ToUpper().Equals("DEFAULT")) arg1 = defaultDateFormats;
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            if (!(v is DateTime))
                            {
                                v = v.ToString().Trim().Length == 0 ? DateTime.MinValue :
                                    DateTime.ParseExact(v.ToString(), arg1.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries),
                                    new CultureInfo("en-US"), DateTimeStyles.None);
                                if (((DateTime)v).Year > 2500) v = ((DateTime)v).Subtract(new DateTime(544, 0, 0));
                            }
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: DATETIMEFORMAT " + arg1
                                + " '" + v.ToString() + "'"
                                + (ee is FormatException ? "" : "\n" + ee.ToString())
                                );
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: DATETIMEFORMAT "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;
                case "KEEPIFMATCH":
                    if (sb != null) sb.AppendLine("Apply Function: KEEPIFMATCH: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;//colvalue
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            v = Regex.IsMatch(v.ToString(), arg1, RegexOptions.IgnoreCase) ? v
                                : getZeroValue(CX, colindex, CimType.None, sb);
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: KEEPIFMATCH " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: KEEPIFMATCH "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;
                case "ONEIFMATCH":
                    if (sb != null) sb.AppendLine("Apply Function: ONEIFMATCH: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;//colvalue
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            v = Regex.IsMatch(v.ToString(), arg1, RegexOptions.IgnoreCase) ? 1
                                : getZeroValue(CX, colindex, CimType.None, sb);
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: ONEIFMATCH " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: ONEIFMATCH "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;
                case "ONEIFAFTER":
                    if (sb != null) sb.AppendLine("Apply Function: ONEIFAFTER: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);

                            if (!(v is DateTime)) v = "";
                            else
                            {
                                v = ((DateTime)v).CompareTo(DateTime.ParseExact(arg1, "yyyy/MM/dd",
                                    new CultureInfo("en-US"), DateTimeStyles.None)) > 0 ? 1
                                    : getZeroValue(CX, colindex, CimType.None, sb);
                            }
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: ONEIFAFTER " + arg1
                                + "\n'" + v.ToString() + "'\n"
                                + (ee is FormatException ? ee.Message : ee.ToString()));
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: ONEIFAFTER "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "KEEPIFCOLMATCH":
                    if (sb != null) sb.AppendLine("Apply Function: KEEPIFCOLMATCH: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null; arg2 = null;//getColAlias(colindex) colvalue
                    if (f.Count >= 2) try
                        {
                            arg1 = f.ElementAt(0); arg2 = f.ElementAt(1);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (arg2.StartsWith("\"")) arg2 = arg2.Substring(1);
                            if (arg2.EndsWith("\"")) arg2 = arg2.Substring(0, arg2.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            if (sb != null) sb.AppendLine("\targ2: " + arg2);
                            bool found = false;
                            if (arg1.StartsWith("$")) arg1 = arg1.Substring(1);
                            for (int i = 0; i < CX.COLALIAS.Length; i++)
                                if (CX.COLALIAS[i].Equals(arg1))
                                {
                                    v = Regex.IsMatch(row[i].ToString(), arg2, RegexOptions.IgnoreCase) ? v
                                        : getZeroValue(CX, colindex, CimType.None, sb);
                                    found = true;
                                    break;
                                }
                            if (!found) throw new Exception("Column '" + arg1 + "' not found.");
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: KEEPIFCOLMATCH " + arg1 + " " + arg2
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 2);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: KEEPIFCOLMATCH "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "ONEIFCOLMATCH":
                    if (sb != null) sb.AppendLine("Apply Function: ONEIFCOLMATCH: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null; arg2 = null;//getColAlias(colindex) colvalue
                    if (f.Count >= 2) try
                        {
                            arg1 = f.ElementAt(0); arg2 = f.ElementAt(1);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (arg2.StartsWith("\"")) arg2 = arg2.Substring(1);
                            if (arg2.EndsWith("\"")) arg2 = arg2.Substring(0, arg2.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            if (sb != null) sb.AppendLine("\targ2: " + arg2);
                            if (arg1.StartsWith("$")) arg1 = arg1.Substring(1);
                            bool found = false;
                            for (int i = 0; i < CX.COLALIAS.Length; i++)
                                if (CX.COLALIAS[i].Equals(arg1))
                                {
                                    v = Regex.IsMatch(row[i].ToString(), arg2, RegexOptions.IgnoreCase) ? 1
                                        : getZeroValue(CX, colindex, CimType.None, sb);
                                    found = true;
                                    break;
                                }
                            if (!found) throw new Exception("Column '" + arg1 + "' not found.");
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: ONEIFCOLMATCH " + arg1 + " " + arg2
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 2);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: ONEIFCOLMATCH "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;
                case "ONEIFPOS":
                    if (sb != null) sb.AppendLine("Apply Function: ONEIFPOS: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    int vi = 0; try {vi = int.Parse(v.ToString());} catch(Exception e) {}
                    v = vi == 0 ? 0 : 1;
                    if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                    break;
                case "KEEPIFPRED":
                    if (sb != null) sb.AppendLine("Apply Function: KEEPIFPRED: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    if (row.Last().ToString().Length == 0) v = "";
                    if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                    break;
                case "KEEPROWIFONE":
                    if (sb != null) sb.AppendLine("Apply Function: KEEPROWIFONE: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    bool del = v == null || v.ToString().Trim().Length == 0;
                    DELETEROW = DELETEROW || del;
                    if (sb != null) sb.AppendLine("\tresult: DELETEROW " + del);
                    
                    //Console.WriteLine("\n" + row[0].ToString() + "\nApply Function: KEEPROWIFONE: [" + getColAlias(CX, colindex) + "] TYPE "
                    //    + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                    //    + v.GetType().Name + ")" + v.ToString() + "\nresult: DELETEROW " + del);
                    
                    break;

                case "DEDUPE":
                    if (sb != null) sb.AppendLine("Apply Function: DEDUPE: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    {
                        List<string> list;
                        if (v is List<string>)
                            list = (List<string>)v;
                        else
                            list = new List<string>(new List<string>(
                                v.ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                                .Select(x => x.Trim().EndsWith(";") ? x.Trim().Substring(0, x.Trim().Length - 1).Trim() : x.Trim())
                                .Distinct());
                        v = list.Count() > 0 ? list.Aggregate((x, y) => x + ";\n" + y) : "";
                    }
                    if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                    break;

                case "ROUND":
                    if (sb != null) sb.AppendLine("Apply Function: ROUND: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);

                            int digits;
                            try { digits = int.Parse(arg1); }
                            catch (Exception e) { digits = 0; }

                            double vd;
                            if (v == null) vd = 0;
                            else if (v is double) vd = (double)v;
                            else try { vd = double.Parse(v.ToString()); }
                                catch (Exception e) { throw e; }

                            v = Math.Round(vd, digits);

                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: ROUND " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: ROUND "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "?=":
                    if (sb != null) sb.AppendLine("Apply Function: EQUALIFMATCH(?=): [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null; arg2 = null;//getColAlias(colindex) colvalue
                    if (f.Count >= 2) try
                        {
                            arg1 = f.ElementAt(0); arg2 = f.ElementAt(1);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (arg2.StartsWith("\"")) arg2 = arg2.Substring(1);
                            if (arg2.EndsWith("\"")) arg2 = arg2.Substring(0, arg2.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            if (sb != null) sb.AppendLine("\targ2: " + arg2);

                            if (Regex.IsMatch(v.ToString(), arg1, RegexOptions.IgnoreCase))
                            {
                                bool colref = arg2.StartsWith("$");
                                if (!colref) v = arg2;
                                else
                                {
                                    string colname = arg2.Substring(1);
                                    if (colname.ToLower().StartsWith("col-"))
                                    {
                                        try
                                        {
                                            int before = int.Parse(colname.Substring(4));
                                            int i = colindex - before;
                                            if (i < 0) throw new Exception("Column Reference $" + colname + " refers to negative index.");
                                            v = row[i];//.ToString();
                                        }
                                        catch (Exception e)
                                        {
                                            throw new Exception("Column '" + colname + "' refers to incorrect index.");
                                        }
                                    }
                                    else
                                    {
                                        bool found = false;
                                        for (int i = 0; i < CX.COLALIAS.Length; i++)
                                            if (CX.COLALIAS[i].Equals(colname))
                                            {
                                                if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                                + " Reference $" + colname + "invalid.");
                                                v = row[i].ToString();
                                                found = true;
                                                break;
                                            }
                                        if (!found) throw new Exception("Column '" + colname + "' not found.");
                                    }
                                }

                            }

                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: EQUALIFMATCH(?=) " + arg1 + " " + arg2
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 2);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: EQUALIFMATCH(?=) "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "DURATION":
                    if (sb != null) sb.AppendLine("Apply Function: DURATION: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null; arg2 = null;
                    if (f.Count >= 2) try
                        {
                            arg1 = f.ElementAt(0); arg2 = f.ElementAt(1);
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
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            if (sb != null) sb.AppendLine("\targ2: " + arg2);

                            DateTime vd1 = DateTime.MinValue;
                            DateTime vd2 = DateTime.MinValue;
                            bool colref1 = arg1.StartsWith("$");
                            bool colref2 = arg2.StartsWith("$");
                            if (!colref1) vd1 = DateTime.ParseExact(arg1, "yyyy/MM/dd  hh:mm", null);
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < CX.COLALIAS.Length; i++)
                                    if (CX.COLALIAS[i].Equals(arg1.Substring(1)))
                                    {
                                        if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                        + " Reference " + arg1 + "invalid.");
                                        var value = row[i];
                                        if (value is string && ((string)value).Length == 0) value = DateTime.MinValue;
                                        if (value == null || !(value is DateTime))
                                            throw new Exception("Column '" + arg2.Substring(1) + "' must be a DATETIME.");
                                        vd1 = (DateTime)value;
                                        found = true;
                                        break;
                                    }
                                if (!found) throw new Exception("Column '" + arg1.Substring(1) + "' not found.");
                            }
                            if (!colref2) vd2 = DateTime.ParseExact(arg2, "yyyy/MM/dd  hh:mm", null);
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < CX.COLALIAS.Length; i++)
                                    if (CX.COLALIAS[i].Equals(arg2.Substring(1)))
                                    {
                                        if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                        + " Reference " + arg2 + "invalid.");
                                        var value = row[i];
                                        if (value is string && ((string)value).Length == 0) value = DateTime.MinValue;
                                        if (value == null || !(value is DateTime))
                                            throw new Exception("Column '" + arg2.Substring(1) + "' must be a DATETIME.");
                                        vd2 = (DateTime)value;
                                        found = true;
                                        break;
                                    }
                                if (!found) throw new Exception("Column '" + arg2.Substring(1) + "' not found.");
                            }

                            int min = (int)vd2.Subtract(vd1).TotalMinutes;
                            //Console.WriteLine("DURATION "
                            //    + vd2.ToString("yyyy/MM/dd hh:mm")
                            //    + vd1.ToString("yyyy/MM/dd hh:mm")
                            //    + " = " + Math.Round(min / 60f, 1) + "h");
                            v = min;
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: DURATION " + arg1 + " " + arg2
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 2);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: DURATION(=) "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "=":
                    if (sb != null) sb.AppendLine("Apply Function: EQUAL(=): [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            if (arg1.StartsWith("\"")) arg1 = arg1.Substring(1);
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            bool colref = arg1.StartsWith("$");
                            if (!colref) v = arg1;
                            else
                            {
                                string colname = arg1.Substring(1);
                                if (colname.ToLower().StartsWith("col-"))
                                {
                                    try
                                    {
                                        int before = int.Parse(colname.Substring(4));
                                        int i = colindex - before;
                                        if (i < 0) throw new Exception("Column Reference $" + colname + "' refers to negative index.");
                                        v = row[i];//.ToString();
                                    }
                                    catch (Exception e)
                                    {
                                        throw new Exception("Column '" + colname + "' refers to incorrect index.");
                                    }
                                }
                                else
                                {
                                    bool found = false;
                                    for (int i = 0; i < CX.COLALIAS.Length; i++)
                                        if (CX.COLALIAS[i].Equals(colname))
                                        {
                                            if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                            + " Reference $" + colname + "invalid.");
                                            v = row[i];//.ToString();
                                            found = true;
                                            break;
                                        }
                                    if (!found) throw new Exception("Column '" + colname + "' not found.");
                                }
                            }
                            //equal type issue fix try
                            transtypeValue(CX, colindex, ref v, sb);

                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: EQUAL(=) " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: EQUAL(=) "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "+":
                    if (sb != null) sb.AppendLine("Apply Function: PLUS(+): [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            bool argstr = false;
                            if (arg1.StartsWith("\"")) { arg1 = arg1.Substring(1); argstr = true; }
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            bool colref = arg1.StartsWith("$"), argnum = false;
                            int v2 = 0; if (!argstr) try
                                {
                                    v2 = int.Parse(arg1);
                                    argnum = true;
                                }
                                catch (Exception e) { }
                            if (!colref) v = sumfuncValue(CX, colindex, v, argnum ? (object)v2 : (object)arg1, sb);
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < CX.COLALIAS.Length; i++)
                                    if (CX.COLALIAS[i].Equals(arg1.Substring(1)))
                                    {
                                        if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                        + " Reference " + arg1 + "invalid.");
                                        v = sumfuncValue(CX, colindex, v, row[i], sb);
                                        found = true;
                                        break;
                                    }
                                if (!found) throw new Exception("Column '" + arg1.Substring(1) + "' not found.");
                            }
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: PLUS(+) " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: PLUS(+) "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case ";":
                    if (sb != null) sb.AppendLine("Apply Function: LIST(;): [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            bool argstr = false;
                            if (arg1.StartsWith("\"")) { arg1 = arg1.Substring(1); argstr = true; }
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            bool colref = arg1.StartsWith("$"), argnum = false;
                            int v2 = 0; if (!argstr) try
                                {
                                    v2 = int.Parse(arg1);
                                    argnum = true;
                                }
                                catch (Exception e) { }
                            if (!colref) v = sumValue(CX, colindex, v, argnum ? (object)v2 : (object)arg1, sb);
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < CX.COLALIAS.Length; i++)
                                    if (CX.COLALIAS[i].Equals(arg1.Substring(1)))
                                    {
                                        if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                        + " Reference " + arg1 + "invalid.");
                                        v = sumValue(CX, colindex, v, row[i], sb);
                                        found = true;
                                        break;
                                    }
                                if (!found) throw new Exception("Column '" + arg1.Substring(1) + "' not found.");
                            }
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: LIST(;) " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: LIST(;) "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "-":
                    if (sb != null) sb.AppendLine("Apply Function: MINUS(-): [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            bool argstr = false;
                            if (arg1.StartsWith("\"")) { arg1 = arg1.Substring(1); argstr = true; }
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            bool colref = arg1.StartsWith("$"), argnum = false;
                            int v2 = 0; if (!argstr) try
                                {
                                    v2 = int.Parse(arg1);
                                    argnum = true;
                                }
                                catch (Exception e) { }
                            if (!colref) v = subValue(CX, colindex, v, argnum ? (object)v2 : (object)arg1, sb);
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < CX.COLALIAS.Length; i++)
                                    if (CX.COLALIAS[i].Equals(arg1.Substring(1)))
                                    {
                                        if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                        + " Reference " + arg1 + "invalid.");
                                        v = subValue(CX, colindex, v, row[i], sb);
                                        found = true;
                                        break;
                                    }
                                if (!found) throw new Exception("Column '" + arg1.Substring(1) + "' not found.");
                            }
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: MINUS(-) " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: MINUS(-) "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "*":
                    if (sb != null) sb.AppendLine("Apply Function: MULTIPLY(*): [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            bool argstr = false;
                            if (arg1.StartsWith("\"")) { arg1 = arg1.Substring(1); argstr = true; }
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            bool colref = arg1.StartsWith("$"), argnum = false;
                            int v2 = 0; if (!argstr) try
                                {
                                    v2 = int.Parse(arg1);
                                    argnum = true;
                                }
                                catch (Exception e) { }
                            if (!colref) v = mulValue(CX, colindex, v, argnum ? (object)v2 : (object)arg1, sb);
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < CX.COLALIAS.Length; i++)
                                    if (CX.COLALIAS[i].Equals(arg1.Substring(1)))
                                    {
                                        if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                        + " Reference " + arg1 + "invalid.");
                                        v = mulValue(CX, colindex, v, row[i], sb);
                                        found = true;
                                        break;
                                    }
                                if (!found) throw new Exception("Column '" + arg1.Substring(1) + "' not found.");
                            }
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: MULTIPLY(*) " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: MULTIPLY(*) "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "/":
                    if (sb != null) sb.AppendLine("Apply Function: DIVIDE(/): [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            bool argstr = false;
                            if (arg1.StartsWith("\"")) { arg1 = arg1.Substring(1); argstr = true; }
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            bool colref = arg1.StartsWith("$"), argnum = false;
                            int v2 = 0; if (!argstr) try
                                {
                                    v2 = int.Parse(arg1);
                                    argnum = true;
                                }
                                catch (Exception e) { }
                            if (!colref) v = divValue(CX, colindex, v, argnum ? (object)v2 : (object)arg1, sb);
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < CX.COLALIAS.Length; i++)
                                    if (CX.COLALIAS[i].Equals(arg1.Substring(1)))
                                    {
                                        if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                        + " Reference " + arg1 + "invalid.");
                                        v = divValue(CX, colindex, v, row[i], sb);
                                        found = true;
                                        break;
                                    }
                                if (!found) throw new Exception("Column '" + arg1.Substring(1) + "' not found.");
                            }
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: DIVIDE(/) " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: DIVIDE(/) "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "REPLACEINFINITY":
                    if (sb != null) sb.AppendLine("Apply Function: REPLACEINFINITY: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            if (arg1.StartsWith("\"")) { arg1 = arg1.Substring(1); }
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            double v1 = 0, v2 = 0; try
                            {
                                v1 = double.Parse(v.ToString());
                                v2 = double.Parse(arg1);
                            }
                            catch (Exception e) { }
                            if (double.IsInfinity(v1)) v = v2;
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: REPLACEINFINITY " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: REPLACEINFINITY "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;


                case "XOR":
                    if (sb != null) sb.AppendLine("Apply Function: XOR: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    arg1 = null;
                    if (f.Count >= 1) try
                        {
                            arg1 = f.ElementAt(0);
                            bool argstr = false;
                            if (arg1.StartsWith("\"")) { arg1 = arg1.Substring(1); argstr = true; }
                            if (arg1.EndsWith("\"")) arg1 = arg1.Substring(0, arg1.Length - 1);
                            if (sb != null) sb.AppendLine("\targ1: " + arg1);
                            bool colref = arg1.StartsWith("$"), argnum = false;
                            int v2 = 0; if (!argstr) try
                                {
                                    v2 = int.Parse(arg1);
                                    argnum = true;
                                }
                                catch (Exception e) { }
                            if (!colref) v = xorValue(CX, colindex, v, argnum ? (object)v2 : (object)arg1, sb);
                            else
                            {
                                bool found = false;
                                for (int i = 0; i < CX.COLALIAS.Length; i++)
                                    if (CX.COLALIAS[i].Equals(arg1.Substring(1)))
                                    {
                                        if (row.Count <= i) throw new Exception("Column '" + CX.COLALIAS[i] + "' not evaluated yet."
                                        + " Reference " + arg1 + "invalid.");
                                        v = xorValue(CX, colindex, v, row[i], sb);
                                        found = true;
                                        break;
                                    }
                                if (!found) throw new Exception("Column '" + arg1.Substring(1) + "' not found.");
                            }
                            if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        }
                        catch (Exception ee)
                        {
                            if (sb != null) sb.AppendLine("\terror: " + ee.Message);
                            setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: XOR " + arg1
                                + "\n'" + v.ToString() + "'\n" + ee.ToString());
                        }
                        finally
                        {
                            f.RemoveRange(0, 1);
                        }
                    else
                    {
                        if (sb != null) sb.AppendLine("\terror: invalid arguments");
                        setWQL("[" + getColAlias(CX, colindex) + "] Invalid arguments in function: XOR "
                        + (f.Count > 0 ? f.ElementAt(0) : ""));
                    }
                    break;

                case "COLLCOUNT":
                    if (sb != null) sb.AppendLine("Apply Function: COLLCOUNT: [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    try
                    {
                        var count = getWMI("COLLCOUNT", v.ToString());
                        //v = "" + (int)count;
                        v = "" + count;
                        if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                    }
                    catch (Exception ee)
                    {
                        v = "";
                        if (sb != null) sb.AppendLine("\tresult: " + v.GetType().Name + " <" + v.ToString() + ">");
                        setWQL("[" + getColAlias(CX, colindex) + "] Exception while processing function: COLLCOUNT "// + arg1
                            + "\n'" + v.ToString() + "'\n" + ee.ToString());
                    }
                    break;


                default:
                    if (sb != null) sb.AppendLine("Apply Function: " + func + ": [" + getColAlias(CX, colindex) + "] TYPE "
                        + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                        + v.GetType().Name + ")" + v.ToString());
                    setWQL("[" + getColAlias(CX, colindex) + "] Invalid function: " + func);
                    break;
                
            }
            return v;
        }




        List<string> listValue(object v1, object v2)
        {
            List<string> ret = new List<string>();
            if (v1 != null)
            {
                if (v1 is List<string>) ret.AddRange((List<string>)v1);
                else ret.Add(v1.ToString());
            }
            if (v2 != null)
            {
                if (v2 is List<string>) ret.AddRange((List<string>)v2);
                else ret.Add(v2.ToString());
            }
            return ret;
        }
















        public static object getZeroValue(SCCMParser.Context CX, int colindex, CimType type, StringBuilder sb)
        {
            if (sb != null) sb.AppendLine("Apply Function: GETZERO: [" + getColAlias(CX, colindex) + "] TYPE "
                + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name));

            object v = null;
            if (getColType(CX, colindex).Equals(typeof(DateTime))
            || (type != CimType.None && type.Equals(CimType.DateTime)))
            {
                if (sb != null) sb.AppendLine("Zero: TYPE DateTime");
                v = DateTime.MinValue;
            }

            else if (getColType(CX, colindex).Equals(typeof(int)) || (type != CimType.None &&
                (type.Equals(CimType.SInt8) || type.Equals(CimType.UInt8)
                || type.Equals(CimType.SInt16) || type.Equals(CimType.UInt16)
                || type.Equals(CimType.SInt32) || type.Equals(CimType.UInt32)
                || type.Equals(CimType.SInt64) || type.Equals(CimType.UInt64)
                )))
            {
                if (sb != null) sb.AppendLine("Zero: TYPE int");
                v = (int)0;
            }

            else if (getColType(CX, colindex).Equals(typeof(double)) || (type != CimType.None &&
                (type.Equals(CimType.Real32) || type.Equals(CimType.Real64)
                )))
            {
                if (sb != null) sb.AppendLine("Zero: TYPE double");
                v = (double)0;
            }

            else if (getColType(CX, colindex).Equals(typeof(bool)) || (type != CimType.None &&
                type.Equals(CimType.Boolean)
                ))
            {
                if (sb != null) sb.AppendLine("Zero: TYPE bool");
                v = (bool)false;
            }

            else if (getColType(CX, colindex).Equals(typeof(char)) || (type != CimType.None &&
                type.Equals(CimType.Char16)
                ))
            {
                if (sb != null) sb.AppendLine("Zero: TYPE char");
                v = (char)0;
            }

            else if (getColType(CX, colindex).Equals(typeof(List<string>)))
            {
                if (sb != null) sb.AppendLine("Zero: TYPE List<string>");
                v = "";
            }

            else if (getColType(CX, colindex).Equals(typeof(string)))
            {
                if (sb != null) sb.AppendLine("Zero: TYPE string");
                v = "";
            }

            else
            {
                if (sb != null) sb.AppendLine("Zero: TYPE? " + (v == null ? "unknown-type" : v.GetType().Name));
                v = ""; //CIM_String, unknown type ...
            }
            return v;
        }
        

        public static bool isZeroValue(SCCMParser.Context CX, int colindex, object v, StringBuilder sb)
        {
            if (sb != null) sb.AppendLine("Test Value: ISZERO: [" + getColAlias(CX, colindex) + "] TYPE "
                + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name) + ": ("
                + v.GetType().Name + ")" + v.ToString());

            if (v.GetType().Equals(typeof(DateTime)) && DateTime.MinValue.Equals(v))
            {
                if (sb != null) sb.AppendLine("Zero: TYPE DateTime: true");
                return true;
            }

            else if (v.GetType().Equals(typeof(int)) && (int)v == 0)
            {
                if (sb != null) sb.AppendLine("Zero: TYPE int: true");
                return true;
            }

            else if (v.GetType().Equals(typeof(double)) && (double)v == 0)
            {
                if (sb != null) sb.AppendLine("Zero: TYPE double: true");
                return true;
            }

            else if (v.GetType().Equals(typeof(bool)) && (bool)v == false)
            {
                if (sb != null) sb.AppendLine("Zero: TYPE bool: true");
                return true;
            }

            else if (v.GetType().Equals(typeof(char)) && (char)v == 0)
            {
                if (sb != null) sb.AppendLine("Zero: TYPE char: true");
                return true;
            }

            else if (v.GetType().Equals(typeof(List<string>)) && ((List<string>)v).Count(x => x.Trim().Length > 0) == 0)
            {
                if (sb != null) sb.AppendLine("Zero: TYPE List<string>: true");
                return true;
            }

            else if (v.GetType().Equals(typeof(string)) && ((string)v).Trim().Length == 0)
            {
                if (sb != null) sb.AppendLine("Zero: TYPE string: true");
                return true;
            }

            else
            {
                if (sb != null) sb.AppendLine("Zero: TYPE " + (v == null ? "unknown-type" : v.GetType().Name) + ": false");
                return false;
                //CIM_String, unknown type ...
            }
        }

        public static void transtypeValue(SCCMParser.Context CX, int colindex, ref object v, StringBuilder sb)
        {
            if (v.GetType().Equals(getColType(CX, colindex))) return;
            if (getColType(CX, colindex).Equals(typeof(DateTime)))
            {
                bool failed = false; if (sb != null) sb.Append("[" + getColAlias(CX, colindex) + "] TYPE " + getColType(CX, colindex).Name
                     + " has to store value (" + v.GetType().Name + ")" + v.ToString() + ".");
                if (v == null) v = DateTime.MinValue;
                else try
                    {
                        v = DateTime.ParseExact(v.ToString(), "dd/MM/yyyy HH:mm:ss", new CultureInfo("en-US"), DateTimeStyles.None);
                    }
                    catch (Exception e)
                    {
                        v = DateTime.MinValue;
                        if (sb != null) sb.AppendLine(" Transtyping failure.");
                        failed = true;
                    }
                if (!failed && sb != null) sb.AppendLine(" Transtyped into value (" + v.GetType().Name + ")" + v.ToString());
            }

            else if (getColType(CX, colindex).Equals(typeof(int)))
            {
                bool failed = false; if (sb != null) sb.Append("[" + getColAlias(CX, colindex) + "] TYPE " + getColType(CX, colindex).Name
                    + " has to store value (" + v.GetType().Name + ")" + v.ToString() + ".");
                if (v == null) v = (int)0;
                else try
                    {
                        if (v.GetType().Equals(typeof(double))
                            || v.GetType().Equals(typeof(char)))
                            v = (int)v;
                        else if (v.GetType().Equals(typeof(bool)))
                            v = (bool)v ? (int)1 : (int)0;
                        else v = int.Parse(v.ToString());
                    }
                    catch (Exception e)
                    {
                        v = (int)0;
                        if (sb != null) sb.AppendLine(" Transtyping failure.");
                        failed = true;
                    }
                if (!failed && sb != null) sb.AppendLine(" Transtyped into value (" + v.GetType().Name + ")" + v.ToString());
            }

            else if (getColType(CX, colindex).Equals(typeof(double)))
            {
                bool failed = false; if (sb != null) sb.Append("[" + getColAlias(CX, colindex) + "] TYPE " + getColType(CX, colindex).Name
                    + " has to store value (" + v.GetType().Name + ")" + v.ToString() + ".");
                if (v == null) v = (double)0;
                else try
                    {
                        if (v.GetType().Equals(typeof(bool)))
                            v = (bool)v ? (double)1 : (double)0;
                        else v = double.Parse(v.ToString());
                    }
                    catch (Exception e)
                    {
                        v = (double)0;
                        if (sb != null) sb.AppendLine(" Transtyping failure.");
                        failed = true;
                    }
                if (!failed && sb != null) sb.AppendLine(" Transtyped into value (" + v.GetType().Name + ")" + v.ToString());
            }

            else if (getColType(CX, colindex).Equals(typeof(bool)))
            {
                bool failed = false; if (sb != null) sb.Append("[" + getColAlias(CX, colindex) + "] TYPE " + getColType(CX, colindex).Name
                    + " has to store value (" + v.GetType().Name + ")" + v.ToString() + ".");
                if (v == null) v = false;
                else try
                    {
                        if (v.GetType().Equals(typeof(double))
                            || v.GetType().Equals(typeof(int))
                            || v.GetType().Equals(typeof(char)))
                            v = (int)v > 0;
                        else v = bool.Parse(v.ToString());
                    }
                    catch (Exception e)
                    {
                        v = false;
                        if (sb != null) sb.AppendLine(" Transtyping failure.");
                        failed = true;
                    }
                if (!failed && sb != null) sb.AppendLine(" Transtyped into value (" + v.GetType().Name + ")" + v.ToString());
            }

            else if (getColType(CX, colindex).Equals(typeof(char)))
            {
                bool failed = false; if (sb != null) sb.Append("[" + getColAlias(CX, colindex) + "] TYPE " + getColType(CX, colindex).Name
                    + " has to store value (" + v.GetType().Name + ")" + v.ToString() + ".");
                if (v == null) v = (char)0;
                else try
                    {

                        if (v.GetType().Equals(typeof(double))
                            || v.GetType().Equals(typeof(int)))
                            v = (char)v;
                        else v = char.Parse(v.ToString());
                    }
                    catch (Exception e)
                    {
                        v = (char)0;
                        if (sb != null) sb.AppendLine(" Transtyping failure.");
                        failed = true;
                    }
                if (!failed && sb != null) sb.AppendLine(" Transtyped into value (" + v.GetType().Name + ")" + v.ToString());
            }

            else if (getColType(CX, colindex).Equals(typeof(List<string>)))
            {
                bool failed = false; if (sb != null) sb.Append("[" + getColAlias(CX, colindex) + "] TYPE " + getColType(CX, colindex).Name
                    + " has to store value (" + v.GetType().Name + ")" + v.ToString() + ".");
                List<String> l = new List<string>();
                if (v == null) v = l;
                else
                {
                    l.AddRange(v.ToString().Split(new char[] { ';', '\n' }, StringSplitOptions.RemoveEmptyEntries));
                    v = l;
                }
                if (!failed && sb != null) sb.AppendLine(" Transtyped into value (" + v.GetType().Name + ")" + v.ToString());
            }

            else if (getColType(CX, colindex).Equals(typeof(string)))
            {
                bool failed = false; if (sb != null) sb.Append("[" + getColAlias(CX, colindex) + "] TYPE " + getColType(CX, colindex).Name
                    + " has to store value (" + v.GetType().Name + ")" + v.ToString() + ".");
                if (v == null
                    || (v.GetType().Equals(typeof(DateTime)) && DateTime.MinValue.Equals(v))
                    || (v.GetType().Equals(typeof(int)) && (int)v == 0)
                    || (v.GetType().Equals(typeof(double)) && (double)v == 0)
                    || (v.GetType().Equals(typeof(bool)) && (bool)v == false)
                    || (v.GetType().Equals(typeof(char)) && (char)v == 0)
                    )
                    v = "";
                else v = v.ToString();
                if (!failed && sb != null) sb.AppendLine(" Transtyped into value (" + v.GetType().Name + ")" + v.ToString());
            }

            else
            {
                //CIM_String, unknown type ...
                bool failed = false; if (sb != null) sb.Append("[" + getColAlias(CX, colindex) + "] TYPE " + getColType(CX, colindex).Name
                    + " has to store value (" + v.GetType().Name + ")" + v.ToString() + ".");
                if (v == null) v = "";
                else v = v.ToString();
                if (!failed && sb != null) sb.AppendLine(" Transtyped into value (" + v.GetType().Name + ")" + v.ToString());
            }
        }


        static int AGGREGFLAG_SUMFUNC = -2;
        static int AGGREGFLAG_SUM = -3;
        static int AGGREGFLAG_SUB = -4;
        static int AGGREGFLAG_MUL = -5;
        static int AGGREGFLAG_DIV = -6;
        static int AGGREGFLAG_SORT = -7;
        static int AGGREGFLAG_XOR = -8;

        public static object sumfuncValue(SCCMParser.Context CX, int colindex, object v1, object v2, StringBuilder sb)
        {
            object ret = aggregateValue(CX, colindex, AGGREGFLAG_SUMFUNC, v1, v2, sb);
            //Console.WriteLine(v1 + " ; " + v2 + " = " + ret);
            return ret;
        }
        public static object sumValue(SCCMParser.Context CX, int colindex, object v1, object v2, StringBuilder sb)
        {
            object ret = aggregateValue(CX, colindex, AGGREGFLAG_SUM, v1, v2, sb);
            //Console.WriteLine(v1 + " + " + v2 + " = " + ret);
            return ret;
        }
        public static object subValue(SCCMParser.Context CX, int colindex, object v1, object v2, StringBuilder sb)
        {
            return aggregateValue(CX, colindex, AGGREGFLAG_SUB, v1, v2, sb);
        }
        public static object mulValue(SCCMParser.Context CX, int colindex, object v1, object v2, StringBuilder sb)
        {
            return aggregateValue(CX, colindex, AGGREGFLAG_MUL, v1, v2, sb);
        }
        public static object divValue(SCCMParser.Context CX, int colindex, object v1, object v2, StringBuilder sb)
        {
            return aggregateValue(CX, colindex, AGGREGFLAG_DIV, v1, v2, sb);
        }
        public static object xorValue(SCCMParser.Context CX, int colindex, object v1, object v2, StringBuilder sb)
        {
            return aggregateValue(CX, colindex, AGGREGFLAG_XOR, v1, v2, sb);
        }


        public static object aggregateValue(SCCMParser.Context CX, int colindex, object v1, object v2, StringBuilder sb)
        {
            return aggregateValue(CX, colindex, -1, v1, v2, sb);
        }
        public static object aggregateValue(SCCMParser.Context CX, int colindex, int aggregindex, object v1, object v2, StringBuilder sb)
        {
            string AGGREGFLAG = "";//default CONCATENATE (; list style)
            if (aggregindex == -1)
                AGGREGFLAG = colindex >= CX.COLAGGREGFLAG.Length ? ""
                    : CX.COLAGGREGFLAG[colindex] == null ? "" : CX.COLAGGREGFLAG[colindex];

            else if (aggregindex == AGGREGFLAG_SUMFUNC) AGGREGFLAG = "SUMFUNC";
            else if (aggregindex == AGGREGFLAG_SUM) AGGREGFLAG = "SUM";
            else if (aggregindex == AGGREGFLAG_SUB) AGGREGFLAG = "SUB";
            else if (aggregindex == AGGREGFLAG_MUL) AGGREGFLAG = "MUL";
            else if (aggregindex == AGGREGFLAG_DIV) AGGREGFLAG = "DIV";
            else if (aggregindex == AGGREGFLAG_SORT) AGGREGFLAG = "SORT";
            else if (aggregindex == AGGREGFLAG_XOR) AGGREGFLAG = "XOR";

            string debug = "Apply Function: AGGREGATE: [" + getColAlias(CX, colindex) + "] TYPE "
                + (getColType(CX, colindex) == null ? "unknown-type" : getColType(CX, colindex).Name)
                + ": (" + v1.GetType().Name + ")" + v1.ToString()
                + " *" + AGGREGFLAG + "*"
                + " (" + v2.GetType().Name + ")" + v2.ToString();
            if (sb != null) sb.AppendLine(debug);

            if (getColType(CX, colindex).Equals(typeof(DateTime)))
            {
                DateTime v1d = (DateTime)v1;
                DateTime v2d = (DateTime)v2;
                if (AGGREGFLAG.Equals("MAXIMUM"))
                {
                    return v2d >= v1d ? v2d : v1d;
                }
                else if (AGGREGFLAG.Equals("MINIMUM"))
                {
                    return v2d >= v1d ? v1d : v2d;
                }
                else if (AGGREGFLAG.Equals("LAST"))
                {
                    return v2d;
                }
                else if (AGGREGFLAG.Equals("FIRST"))
                {
                    return v1d;
                }
                else if (AGGREGFLAG.Equals("SUB"))
                {
                    //trying to fix datetime diff
                    int vdiff = (int)v1d.Subtract(v2d).TotalMinutes;
                    return DateTime.MinValue.AddSeconds(v1d.Subtract(v2d).TotalSeconds);
                }
                else
                {//invalid operators: "" SUM MUL DIV XOR; return MAX
                    return v2d >= v1d ? v2d : v1d;
                }
            }

            else if (getColType(CX, colindex).Equals(typeof(int)))
            {
                int v1i, v2i;
                if (v1 == null || v1.ToString().Length == 0) v1i = 0;
                else if (v1 is int) v1i = (int)v1;
                else if (v1.ToString().ToUpper().Equals("NAN"))
                    return "NaN";
                else try { v1i = int.Parse(v1.ToString()); }
                    catch (Exception e) { throw new Exception("'" + v1.ToString() + "' " + e.Message); }
                if (v2 == null || v2.ToString().Length == 0) v2i = 0;
                else if (v2 is int) v2i = (int)v2;
                else if (v2.ToString().ToUpper().Equals("NAN"))
                    return "NaN";
                else try { v2i = int.Parse(v2.ToString()); }
                    catch (Exception e) { throw new Exception("'" + v2.ToString() + "' " + e.Message); }
                if (AGGREGFLAG.Equals("COUNT"))
                {
                    int count = v1i;
                    count++;
                    return count;
                }
                else if (AGGREGFLAG.Equals("MINIMUM"))
                {
                    return v2i >= v1i ? v1i : v2i;
                }
                else if (AGGREGFLAG.Equals("LAST"))
                {
                    return v2i;
                }
                else if (AGGREGFLAG.Equals("FIRST"))
                {
                    return v1i;
                }
                else if (AGGREGFLAG.Equals("SUM"))
                {
                    return v1i + v2i;
                }
                else if (AGGREGFLAG.Equals("SUB"))
                {
                    return v1i - v2i;
                }
                else if (AGGREGFLAG.Equals("MUL"))
                {
                    return v1i * v2i;
                }
                else if (AGGREGFLAG.Equals("DIV"))
                {
                    if (v2i == 0) return "NaN";
                    else return v1i / v2i;
                }
                else if (AGGREGFLAG.Equals("XOR"))
                {
                    return v1i & v2i;
                }
                else
                {//default MAXIMUM
                    return v2i >= v1i ? v2i : v1i;
                }
            }

            else if (getColType(CX, colindex).Equals(typeof(double)))
            {
                double v1d, v2d;
                if (v1 == null || v1.ToString().Length == 0) v1d = 0;
                else if (v1 is double) v1d = (double)v1;
                else try { v1d = double.Parse(v1.ToString()); }
                    catch (Exception e) { throw new Exception("'" + v1.ToString() + "' " + e.Message); }
                if (v2 == null || v2.ToString().Length == 0) v2d = 0;
                else if (v2 is double) v2d = (double)v2;
                else try { v2d = double.Parse(v2.ToString()); }
                    catch (Exception e) { throw new Exception("'" + v2.ToString() + "' " + e.Message); }
                if (AGGREGFLAG.Equals("COUNT"))
                {
                    int count = (int)v1d;
                    count++;
                    return (double)count;
                }
                else if (AGGREGFLAG.Equals("MINIMUM"))
                {
                    return v2d >= v1d ? v1d : v2d;
                }
                else if (AGGREGFLAG.Equals("LAST"))
                {
                    return v2d;
                }
                else if (AGGREGFLAG.Equals("FIRST"))
                {
                    return v1d;
                }
                else if (AGGREGFLAG.Equals("SUM"))
                {
                    return v1d + v2d;
                }
                else if (AGGREGFLAG.Equals("SUB"))
                {
                    return v1d - v2d;
                }
                else if (AGGREGFLAG.Equals("MUL"))
                {
                    return v1d * v2d;
                }
                else if (AGGREGFLAG.Equals("DIV"))
                {
                    return v1d / v2d;
                }
                else
                {//default MAXIMUM
                    return v2d >= v1d ? v2d : v1d;
                }
            }

            else if (getColType(CX, colindex).Equals(typeof(bool)))
            {
                bool v1b, v2b;
                if (v1 == null) v1b = false;
                else if (v1 is bool) v1b = (bool)v1;
                else try { v1b = bool.Parse(v1.ToString()); }
                    catch (Exception e) { throw e; }
                if (v2 == null) v2b = false;
                else if (v2 is bool) v2b = (bool)v2;
                else try { v2b = bool.Parse(v2.ToString()); }
                    catch (Exception e) { throw e; }

                if (AGGREGFLAG.Equals("MINIMUM"))
                {
                    return v1b && v2b;
                }
                else if (AGGREGFLAG.Equals("LAST"))
                {
                    return v2b;
                }
                else if (AGGREGFLAG.Equals("FIRST"))
                {
                    return v1b;
                }
                else if (AGGREGFLAG.Equals("SUM"))
                {
                    return v1b || v2b;
                }
                else if (AGGREGFLAG.Equals("SUB"))
                {
                    return v1b ^ v2b;//XOR
                }
                else if (AGGREGFLAG.Equals("MUL"))
                {
                    return v1b && v2b;
                }
                else if (AGGREGFLAG.Equals("DIV"))
                {
                    return v1b && v2b;
                }
                else
                {//default MAXIMUM
                    return v1b || v2b;
                }
            }

            else //if (getColType(CX, colindex).Equals(typeof(List<string>)))
            //if (getColType(colindex).Equals(typeof(string)) || getColType(colindex).Equals(typeof(char)))
            {
                //if (CX.COLALIAS[colindex].Equals("TopUser%"))
                //    Console.WriteLine("TopUser% " + v1.ToString() + " *" + AGGREGFLAG + "* " + v2.ToString());
                List<string> ret = new List<string>();
                List<string> v1l = v1 is List<string> ? (List<string>)v1 : new List<string>(v1.ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(m => (m.EndsWith(";") ? m.Substring(0, m.Length - 1) : m).Trim()).ToList();
                if (v1l.Count == 0) v1l.Add("");
                List<string> v2l = v2 is List<string> ? (List<string>)v2 : new List<string>(v2.ToString().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                    .Select(m => (m.EndsWith(";") ? m.Substring(0, m.Length - 1) : m).Trim()).ToList();
                if (v2l.Count == 0) v2l.Add("");

                var v2i = v2l.GetEnumerator(); bool exhausted = false;
                foreach (string v1s in v1l)
                {
                    
                    string v2s = !exhausted && !(exhausted = !v2i.MoveNext()) ? v2i.Current : "";
                    bool NUM = true;
                    float v1f = 0; try { v1f = float.Parse(v1s); }
                    catch (Exception e) { NUM = false; }
                    float v2f = 0; try { v2f = float.Parse(v2s); }
                    catch (Exception e) { NUM = false; }

                    if (AGGREGFLAG.Equals("COUNT"))
                    {
                        int count = 1; try { count = int.Parse(v1s); }
                        catch (Exception e) { }
                        if (v2f == 1) count++;
                        ret.Add("" + count);
                    }
                    else if (AGGREGFLAG.Equals("MAXIMUM"))
                    {
                        ret.Add((NUM ? v2f >= v1f : v2s.CompareTo(v1s) >= 0) ? v2s : v1s);
                    }
                    else if (AGGREGFLAG.Equals("MINIMUM"))
                    {
                        ret.Add((NUM ? v2f >= v1f : v2s.CompareTo(v1s) >= 0) ? v1s : v2s);
                    }
                    else if (AGGREGFLAG.Equals("LAST"))
                    {
                        ret.Add(v2s);
                    }
                    else if (AGGREGFLAG.Equals("FIRST"))
                    {
                        ret.Add(v1s);
                    }
                    else if (AGGREGFLAG.Equals("SUB"))
                    {
                        ret.Add(NUM ? "" + (v1f - v2f) : v1s.Replace(v2s, ""));
                    }
                    else if (AGGREGFLAG.Equals("DIV"))
                    {
                        ret.Add(NUM ? "" + (v1f == 0 ? v1f : v1f / v2f) : v1s.Length == 0 ? v2s : v2s.Replace(v1s, ""));
                    }
                    else if (AGGREGFLAG.Equals("MUL"))
                    {
                        ret.Add(NUM ? "" + (v1f * v2f)
                            : v2s.Trim() + (v2s.Trim().Length > 0 && v1s.Trim().Length > 0 ? ";\n" : "") + v1s.Trim());
                    }
                    else if (AGGREGFLAG.Equals("SUM"))
                    {
                        ret.Add(NUM ? "" + (v1f + v2f)
                            : v1s.Trim() + (v1s.Trim().Length > 0 && v2s.Trim().Length > 0 ? ";\n" : "") + v2s.Trim());
                    }
                    else if (AGGREGFLAG.Equals("SUMFUNC"))
                    {
                        ret.Add(NUM ? "" + (v1f + v2f) : v1s + v2s);
                    }
                    else //default "" = CONCATENATE
                    {
                        ret.Add(v1s.Trim() + (v1s.Trim().Length > 0 && v2s.Trim().Length > 0 ? ";\n" : "") + v2s.Trim());
                    }

                }

                if (AGGREGFLAG.Equals("SORT"))
                {
                    List<string> sort = new List<string>();
                    foreach(string y in ret) foreach(string x in new List<string>(y.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
                        .Select(m => (m.EndsWith(";") ? m.Substring(0, m.Length - 1) : m).Trim()).ToList()) sort.Add(x);

                    sort.Sort(new Comparison<string>((x1s, x2s) => {

                        bool NUM = true;
                        float x1f = 0; try { x1f = float.Parse(x1s); }
                        catch (Exception e) { NUM = false; }
                        float x2f = 0; try { x2f = float.Parse(x2s); }
                        catch (Exception e) { NUM = false; }

                        bool VER = true;
                        string x1vs = x1s.ToLower(); if (x1vs.StartsWith("v")) x1vs = x1vs.Substring(1);
                        int p1 = 3 - x1vs.Count(c => c == '.'); while (p1-- > 0) x1vs += ".0";
                        string x2vs = x2s.ToLower(); if (x2vs.StartsWith("v")) x2vs = x2vs.Substring(1);
                        int p2 = 3 - x2vs.Count(c => c == '.'); while (p2-- > 0) x2vs += ".0";
                        Version x1v = new Version(); try { x1v = new Version(x1vs); }
                        catch (Exception e) { NUM = false; }
                        Version x2v = new Version(); try { x2v = new Version(x2vs); }
                        catch (Exception e) { NUM = false; }

                        if (VER)
                            return x1v.CompareTo(x2v);
                        if (NUM)
                            return x1f.CompareTo(x2f);
                        else
                            return x1s.CompareTo(x2s);
                    })); ret = sort;
                }

                return ret.Count == 0 ? "" : ret.Aggregate((x, y) => x + ";\n" + y);
            }
        }






        //This class emulates the SMS_ScheduleMethods WMI object available on the SMS site server
        public class SMS_ScheduleToken
        {
            public string Class = null;

            public DateTime Start = DateTime.MinValue;
            public bool IsGMT = false;

            public TimeSpan Duration = new TimeSpan(0);
            public TimeSpan Span = new TimeSpan(0);

            public int Day = 1;
            public int ForNumberOfWeeks = 1;
            public int ForNumberOfMonths = 1;
            public int WeekOrder = 0;
            public int MonthDay = 0;

            public bool IsFinished()
            {
                return Start.Add(Duration).Subtract(IsGMT ? DateTime.UtcNow : DateTime.Now).Ticks > 0;
            }

            override public string ToString()
            {
                string ret = String.Format(new CultureInfo("en-US"),
                    (IsGMT ? DateTime.UtcNow : DateTime.Now).Subtract(Start).Ticks > 0
                    && (!"SMS_ST_NonRecurring".Equals(Class) || Duration.Ticks == 0) ?
                    ("SMS_ST_NonRecurring".Equals(Class) ? "" : "{0:HH:mm}") :
                    "{0:d-MMM-yyyy HH:mm}", Start) + (IsGMT ? " GMT" : "");
                ret += Duration.Ticks == 0 ? "" : " - " + String.Format(new CultureInfo("en-US"), "{0:d-MMM-yyyy HH:mm}", Start.Add(Duration));
                switch (Class)
                {
                    case "SMS_ST_NonRecurring":

                        ret += (ret.Length > 0 ? ", " :"") + "Once"
                            ; break;

                    case "SMS_ST_RecurInterval":
                        if (Span.TotalDays >=1)
                            ret += ", Every" + (Span.TotalDays == 1 ? "day" : Span.TotalDays == 7 ? " Week" :
                            " " + Span.TotalDays + " Day" + (Span.TotalDays >= 2 ? "s" : ""))
                        ; else
                            ret += ", Every " + (Span.TotalHours == 1 ? "" : Span.TotalHours + " ") + "Hour"
                            +  (Span.TotalHours >= 2 ? "s" : "")
                        ; break;

                    case "SMS_ST_RecurWeekly":

                        ret += ", Every " + (ForNumberOfWeeks == 1 ? "" :
                            ForNumberOfWeeks + " " + "Week" + (ForNumberOfWeeks > 1 ? "s" : "") + " on ")
                            
                            + (Day == 1 ? "Sunday" : Day == 2 ? "Monday" : Day == 3 ? "Tuesday" : Day == 4 ? "Wednesday" :
                           Day == 5 ? "Thursday" : Day == 6 ? "Friday" : Day == 7 ? "Saturday" : "?Day")
                           ; break;

                    case "SMS_ST_RecurMonthlyByWeekday":

                        ret += ", Every " + (ForNumberOfMonths == 1 ? "" :
                            ForNumberOfMonths + " " + "Month" + (ForNumberOfMonths > 1 ? "s" : "") + " on the ")
                            
                            + (WeekOrder == 0 ? "Last" : WeekOrder == 1 ? "First" : WeekOrder == 2 ? "Second" :
                            WeekOrder == 3 ? "Third" : WeekOrder == 4 ? "Fourth" : "?WeekOrder")

                            + (Day == 1 ? "Sunday" : Day == 2 ? "Monday" : Day == 3 ? "Tuesday" : Day == 4 ? "Wednesday" :
                            Day == 5 ? "Thursday" : Day == 6 ? "Friday" : Day == 7 ? "Saturday" : "?Day")
                            ; break;

                    case "SMS_ST_RecurMonthlyByDate":

                        ret += ", Every " + (ForNumberOfMonths == 1 ? "" :
                            ForNumberOfMonths + " ") + "Month" + (ForNumberOfMonths > 1 ? "s" : "")
                            
                            + " on the " + MonthDay + (MonthDay == 1 ? "st" : MonthDay == 2 ? "nd" : MonthDay == 3 ? "rd" : "th")
                            ; break;

                    default:
                        return null;
                }
                return ret;
            }
        };

        public static SMS_ScheduleToken parseScheduleToken(ManagementBaseObject mo)
        {
//            foreach (var p in mo.Properties) Console.WriteLine(p.Name + "=" + p.Value.ToString());
            SMS_ScheduleToken st = new SMS_ScheduleToken();
            st.Class = mo.ClassPath.ClassName;


            switch (st.Class)
            {
                case "SMS_ST_NonRecurring":
                    break;

                case "SMS_ST_RecurInterval":
                    try
                    {
                        st.Span = new TimeSpan((int)(uint)mo["DaySpan"], (int)(uint)mo["HourSpan"], (int)(uint)mo["MinuteSpan"], 0);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case "SMS_ST_RecurWeekly":
                    try
                    {
                        st.Span = new TimeSpan((int)(uint)mo["DaySpan"], (int)(uint)mo["HourSpan"], (int)(uint)mo["MinuteSpan"], 0);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    try
                    {
                        st.Day = (int)(uint)mo["Day"];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    try
                    {
                        st.ForNumberOfWeeks = (int)(uint)mo["ForNumberOfWeeks"];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case "SMS_ST_RecurMonthlyByWeekday":
                    try
                    {
                        st.Day = (int)(uint)mo["Day"];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    try
                    {
                        st.ForNumberOfMonths = (int)(uint)mo["ForNumberOfMonths"];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    try
                    {
                        st.WeekOrder = (int)(uint)mo["WeekOrder"];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    break;

                case "SMS_ST_RecurMonthlyByDate":
                    try
                    {
                        st.MonthDay = (int)(uint)mo["MonthDay"];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    try
                    {
                        st.ForNumberOfMonths = (int)(uint)mo["ForNumberOfMonths"];
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                    break;
            }
                        
            try
            {
                st.Start = ManagementDateTimeConverter.ToDateTime(mo["StartTime"].ToString());
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                st.IsGMT = (bool)mo["IsGMT"];
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            try
            {
                st.Duration = new TimeSpan((int)(uint)mo["DayDuration"], (int)(uint)mo["HourDuration"], (int)(uint)mo["MinuteDuration"], 0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            return st;
        }

        public static SMS_ScheduleToken parseScheduleToken(string scheduleToken)
        {
            SMS_ScheduleToken st = new SMS_ScheduleToken();
            switch (stReadBits(scheduleToken, 19, 21))
            {
                case 1:
                    st.Class = "SMS_ST_NonRecurring";
                    break;
                case 2:
                    st.Class = "SMS_ST_RecurInterval";
                    st.Span = new TimeSpan(
                        stReadBits(scheduleToken, 3, 7),
                        stReadBits(scheduleToken, 8, 12),
                        stReadBits(scheduleToken, 13, 18),
                        0);
                    break;
                case 3:
                    st.Class = "SMS_ST_RecurWeekly";
                    st.Day = stReadBits(scheduleToken, 16, 18);
                    st.ForNumberOfWeeks = stReadBits(scheduleToken, 13, 15);
                    break;
                case 4:
                    st.Class = "SMS_ST_RecurMonthlyByWeekday";
                    st.Day = stReadBits(scheduleToken, 16, 18);
                    st.ForNumberOfMonths = stReadBits(scheduleToken, 12, 15);
                    st.WeekOrder = stReadBits(scheduleToken, 9, 11);
                    break;
                case 5:
                    st.Class = "SMS_ST_RecurMonthlyByDate";
                    st.MonthDay = stReadBits(scheduleToken, 14, 18);
                    st.ForNumberOfMonths = stReadBits(scheduleToken, 10, 13);
                    break;
            }

            if (stReadBits(scheduleToken, 0, 0) == 1)
                st.IsGMT = true;

            st.Duration = new TimeSpan(
                stReadBits(scheduleToken, 22, 26),
                stReadBits(scheduleToken, 27, 31),
                stReadBits(scheduleToken, 32, 37),
                0);

            st.Start = new DateTime(
                1970 + stReadBits(scheduleToken, 38, 43),
                stReadBits(scheduleToken, 44, 47),
                stReadBits(scheduleToken, 48, 52),
                stReadBits(scheduleToken, 53, 57),
                stReadBits(scheduleToken, 58, 63),
                0);
            //Console.WriteLine(scheduleToken + " scheduleToken: " + st.ToString() + ", " + (st.IsFinished() ? "" : "Not ") + "Finished");
            return st;
        }
        

        public static int stReadBits(string scheduleToken, int i, int j)
        {
            try
            {
                char[] arr = scheduleToken.ToCharArray();
                Array.Reverse(arr);
                string x = new string(arr);

                x = x.Substring(i / 4);
                int n = (j - i + 1 + (i % 4)) / 4;
                x.Substring(x.Length - n, n);

                arr = x.ToCharArray();
                Array.Reverse(arr);
                x = new string(arr);

                long nRaw = long.Parse(x, System.Globalization.NumberStyles.HexNumber);
                long nMask = (long)Math.Pow(2, (j - i + 1)) - 1;
                long nShift = (long)Math.Pow(2, (i % 4));
                long res = (nRaw & (nMask * nShift)) / nShift;

                //Console.WriteLine("stReadBits " + scheduleToken + " " + i + " " + j + ": " + x + ": " + res);
                return (int)res;
            }
            catch (Exception e)
            {
                Console.WriteLine("stReadBits " + scheduleToken + " " + i + " " + j + ": error: " + e.ToString());
                return 0;
            }
        }

      















    }
}
