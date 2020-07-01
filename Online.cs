using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;
using System.Drawing;
using System.Net;

namespace AuditSec
{
    class Online
    {

        static Icon[] ICONS = {
            Icon.FromHandle(global::AuditSec.Properties.Resources.offline2.GetHicon()),
            Icon.FromHandle(global::AuditSec.Properties.Resources.offline.GetHicon()),
            Icon.FromHandle(global::AuditSec.Properties.Resources.online.GetHicon()),
            Icon.FromHandle(global::AuditSec.Properties.Resources.online2.GetHicon()),
        };

        static Icon getIcon(Status status)
        {
            return ICONS[(int)status.GetValue()];
        }

        public enum STATUS
        {
             UNKNOWN = 0,
             OFFLINE = 1,
             ONLINE = 2,
             ONLINE2 = 3
        };

        class Status
        {
            STATUS Value;
            DateTime Time;
            bool NA;
            public Status(STATUS S, bool NA)
            {
                Value = S;
                Time = DateTime.Now;
                this.NA = NA;
            }
            public bool isOutdated()
            {
                return NA || DateTime.Now.Subtract(Time).TotalMinutes > 5;
            }
            public STATUS GetValue()
            {
                if (!isOutdated()) return Value;
                else
                {
                    if (Value == STATUS.UNKNOWN) return STATUS.UNKNOWN;
                    if (Value == STATUS.OFFLINE) return STATUS.UNKNOWN;
                    if (Value == STATUS.ONLINE) return STATUS.ONLINE2;
                    if (Value == STATUS.ONLINE2) return STATUS.OFFLINE;
                    else return STATUS.UNKNOWN;
                }
            }
        }



        public Icon getIcon(string machine)
        {
            return ICONS[(int)GetStatus(machine)];
        }



        Hashtable Machines = new Hashtable();
        public bool STOP = false;
        Func<string, bool> Refresh;
        Func<bool> isActive;

        public Online(Func<string, bool> Refresh, Func<bool> isActive)
        {
            this.Refresh = Refresh;
            this.isActive = isActive;
        }

        public void Start()
        {
            while (!STOP)
            {
                Thread.Sleep(1000);
                if (isActive()) CheckStatus();
            }
        }

        public void Clear()
        {
            Machines.Clear();
        }

        public bool Add(string machine)
        {
            lock(Machines) {
                if (!Machines.Contains(machine))
                {
                    Machines.Add(machine, new Status(STATUS.UNKNOWN, true));
                    return true;
                }
                else return false;
            }
        }

        public STATUS GetStatus(string machine)
        {
            return Machines.Contains(machine) ? ((Status)Machines[machine]).GetValue() : STATUS.UNKNOWN;
            /*
            lock (Machines)
            {
                if (!Machines.Contains(machine)) Add(machine);
                return ((Status)Machines[machine]).GetValue();
            }
            */
        }

        void CheckStatus()
        {
            new ArrayList(Machines.Keys).ToArray().Cast<string>().AsParallel().ForAll(machine =>
            {
                if (isActive()
                    && Machines.Contains(machine)
                    && ((Status)Machines[machine]).isOutdated())
                    lock (Machines)
                    {
                        Machines.Remove(machine);
                        Console.WriteLine("Online Checking... " + machine + "...");
                        bool reach = MachineInfo.isReachableAndValidNetbios(machine);
                        Status status = new Status(reach ? STATUS.ONLINE : STATUS.OFFLINE, false);
                        Machines.Add(machine, status);
                        Console.WriteLine("Online Checking... " + machine + "   " + status.GetValue());
                        if (isActive()) Refresh(machine);
                    }
            });
        }
    }
}
