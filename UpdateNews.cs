using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AuditSec
{
    class UpdateNews
    {
        public static string news = "NEWS:"
            /*+ "\n'SCCM Check' also available in the Computer scan. 27-mar-2012"
            + "\n'Check Administrators group' now also reports Missing ServiceDesk security groups. 28-mar-2012"
            + "\n'Check AD-mislocated' now also reports possible Wrong Owner. 30-mar-2012"
            + "\nReworked scan threading. 30-mar-2012"
            + "\nComputer reenabler. 10-apr-2012"
            + "\nOutdated antivirus (beta). 16-may-2012"
            + "\nExclude from non-auth: ishelpdesk and temp_admin. 16-may-2012"
            + "\nCustomizable authorised admins list. 23-may-2012"
            + "\nOptions panel now on the right with hiding mechanism. 23-may-2012"
            + "\nAD-unlocked account cleanup. 1-june-2012"
            + "\nTrigger Computer Management showing on Incoming Call detection (CUCILYNC). 1-june-2012"
            + "\ndesktop shortcuts are available now. 31-july-2012"
            + "\nv3.5.2 has improved stability. Thanks for your coop to correct issues. 10-sept-2012"
            + "\nPC Management standalone did not display passwords from the list. Corrected. 10-sept-2012"
            + "\n"
            + "\nThe SCCM Reporting module is more robust now. Makes nice Excel exports."
            + "\nI freshly introduced scheduling jobs, it is certainly still buggy. 10-sept-2012"
            + "\nDomain, Site, Department triple selection bug resolved. 13-sept-2012"
            + "\nWQL request attached in XLS export. 13-sept-2012"
            + "\nMore templates, ie. ARP tables. More syntaxes, ie. LIKE ALL, LIKE ANY. 13-sept-2012"
            + "\nThis update is essentially for SCCM Reporting module enhancement due to many new reports to address recently. 05-oct-2012"
            + "\nThe PC Management module also had a few improvements for search and application listing. 05-oct-2012"
            + "\nThe AuditSec module now checking if the Netbios name of a machine is correct, this avoid mismatch seen for Wifi connected laptops. 19-oct-2012"
            + "\nThe PC Management module has 2 new buttons, Remote Prompt C:\\>_ and Remote Folder \\\\C$. 12-nov-2012"
            + "\nComputer P/N replacement enhanced. Lenovo Models uptodate. 14-dec-2012"
            + "\nThe SCCM Reporting module had a lot of improvment, such as large group support and template coloring. 12-nov-2012"
            + "\nPUR template is now available. Scheduled job bugs being worked on. 22-nov-2012"
            + "\nUsers details directly through SCCM. 14-dec-2012"*/
         //   + "\nAuditSec now checks UAC off (still beta). 21-jan-2013"
            /*+ "\nAuditSec now checks WMI Win32_EncryptableVolume (Suspended Bitlocker). 25-mar-2013"
            + "\nSCCM Reporting recent query icon (new). 21-jan-2013"
            + "\nSCCM Reporting duplicate management went under a few changes needs to validate (beta). 21-jan-2013"
            + "\nSCCM Reporting - In arguments, a new TemplateLevel Combo helps changing easily the output level. 24-jan-2013"
            + "\nSCCM Reporting - Recent reports included. 26-mar-2013"
            + "\nSCCM Reporting - Multi-valued cells sorting fixed. 01-jul-2013"
            + "\nVarious bug fixes. 09-jul-2013"
            + "\nSCCM Reporting - List the ITOps Members / Management linked to the location of a workstation. 09-jul-2013"
            + "\nPC Management: The applications listing is copied to the clipboard when the Apps panel is shown. 17-sep-2013"
            + "\nPC Management: The bitlocker recovery code can be spoken out. 30-sep-2013"
            + "\nSCCM Collection Design: This is a new module for creating SCCM collections for site and dpts. 10-oct-2013"
            + "\nPC Management: replaced Bitlocker recovery key retrieval from AD with MBAM. 13-nov-2013"
            + "\nNew Tool! Registry Collector: multiple PC hive value collector. 29-nov-2013"
            + "\nNew Tool! Registry Collector: tool is maturing, multiple scan merging still to be done. 23-jan-2014"
            + "\nSCCM Reporting: new group expansion routine, below a manager. 21-feb-2014"
            + "\nGroup Editor: new tool available. 28-mar-2014"
            + "\nHotApps: new tool available. 20-jun-2014"
            + "\nPC Management: usb printers details available. 05-sep-2014"
            + "\nNew Tool! WMI Collection: Multiple PC Management values collector - testcase: usb printers. 16-sep-2014"
            + "\n2015. Auditsec news: PC Management module redesigned."
            + "\nAuditsec news: Multiple PC Registry Collector enhanced, IE Toolbars template added. 13-jan-2016"
            + "\n*SCCM Reporting news: new DirXML attributes available : employeeStatus and Type. 19-may-2016"*/
            + "\n*SCCM Reporting news: 2 servers (old/new env.) 30-mar-2017"
            ;
    }
}
