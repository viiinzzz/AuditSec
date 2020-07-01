using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Lync.Controls;

namespace myWPF
{
    /// <summary>
    /// Interaction logic for LyncControl.xaml
    /// </summary>
    public partial class LyncControl : UserControl
    {
        public LyncControl()
        {
            InitializeComponent();
        }

        public void setRemoteSIP(string sip) {
            presence.Source = sip;
            startIM.Source = sip;
            sendFile.Source = sip;
        }

        public void openIMWindow()
        {
            //startIM.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Primitives.ButtonBase.ClickEvent));
            //startIM.AddHandler(new MouseEventHandler(startIM, null));
            /*
            System.Windows.Automation.Peers.ButtonAutomationPeer peer = new System.Windows.Automation.Peers.ButtonAutomationPeer(startIM);
            System.Windows.Automation.Provider.IInvokeProvider invokeProv = peer.GetPattern(System.Windows.Automation.Peers.PatternInterface.Invoke) as System.Windows.Automation.Provider.IInvokeProvider;
            invokeProv.Invoke();*/
        }

        public string getPresenceStatus()
        {
            return presence.AvailabilityState.ToString();
        }
    }
}
