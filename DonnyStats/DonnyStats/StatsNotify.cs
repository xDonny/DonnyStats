/*
 * Donny Bridgen
 * February 2015
 * Application to run on startup
 * 
 * Purpose:
 * To tell me when the internet is actually down, or just being slow.
 * 
 * Red:     Internet not working
 * Blue:    Internet Activity
 * Black:   Idle
 * 
 * Modify, sell, distirbute or do whatever you like, but this software is provided without
 * any warranty. I'm not responsible for any issues that happen while using this program.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DonnyStats
{

    public partial class StatsNotify : Form
    {
        #region GlobalVariables
        //Create objects for icons.
        NotifyIcon statusIcon;
        Icon runningIcon;
        Icon activityIcon;
        Icon errorIcon;
        Thread workerThread;
        #endregion

        public StatsNotify()
        {
            InitializeComponent();

            //Hide the form, and remove it from the taskbar.
            #region HideBar
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            #endregion

            //Initlize icons and add to tray
            #region addToTray
            //Initilize the statusIcon
            runningIcon = DonnyStats.Properties.Resources.donnyRunning;
            activityIcon = DonnyStats.Properties.Resources.donnyActivity;
            errorIcon = DonnyStats.Properties.Resources.donnyError;

            //Add the NotifyIcon to the Notification Area.
            statusIcon = new NotifyIcon();
            statusIcon.Icon = runningIcon;
            statusIcon.Visible = true;
            #endregion

            //add menu options and click functionality for exit
            #region Click for exit
            //Add a menu when right clicking
            ContextMenu contextMenu = new ContextMenu();
            MenuItem programNameMenuItem = new MenuItem("Donny Bridgen's Stats v0.1 Beta");
            MenuItem exitMenuItem = new MenuItem("Exit");

            exitMenuItem.Click += exitMenuItem_Click;

            contextMenu.MenuItems.Add(programNameMenuItem);
            contextMenu.MenuItems.Add(exitMenuItem);

            statusIcon.ContextMenu = contextMenu;
            #endregion

           //create and run thread

            workerThread = new Thread(new ThreadStart(workingThread));
            workerThread.Start();
        }
        
        /// <summary>
        /// Clean up and exit application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void exitMenuItem_Click(object sender, EventArgs e)
        {
            workerThread.Abort();
            statusIcon.Dispose();
            this.Close();
        }

        /// <summary>
        /// Thread that pings google every 10 seconds, and checks if there is data being recived.
        /// </summary>
        void workingThread()
        {
            //add network monitor wmi
            ManagementClass driverManagement = new ManagementClass("Win32_PerfRawData_Tcpip_NetworkAdapter");

            UInt64 old = 0;

            int counter = 0;
            Ping pingSender = new Ping ();
            PingOptions options = new PingOptions ();

            options.DontFragment = true;

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes (data);
            int timeout = 120;
            bool networkDown = false;
            while (true)
            {
                if (!networkDown)
                {
                    ManagementObjectCollection networkCollection = driverManagement.GetInstances();
                    foreach (ManagementObject obj in networkCollection)
                    {
                        if (obj["name"].ToString().Contains("Wireless"))
                        {
                            UInt64 newInt = Convert.ToUInt64(obj["BytesReceivedPersec"]);

                            if ((Convert.ToUInt64(obj["BytesReceivedPersec"]) - old) != 0)
                            {
                                statusIcon.Icon = activityIcon;
                            }
                            else
                            {
                                statusIcon.Icon = runningIcon;
                            }

                            old = newInt;
                            break;
                        }
                    }
                }
                else
                {
                    try
                    {
                        PingReply reply = pingSender.Send("www.google.com", timeout, buffer, options);
                        networkDown = false;
                    }
                    catch (Exception)
                    {
                        statusIcon.Icon = errorIcon;
                        networkDown = true;
                    }
                }

                if (counter == 100)
                {
                    try
                    {
                        PingReply reply = pingSender.Send("www.google.com", timeout, buffer, options);
                        networkDown = false;
                    }
                    catch (Exception)
                    {
                        statusIcon.Icon = errorIcon;
                        networkDown = true;
                    }
                    counter = 0;
                }

                counter++;
                Thread.Sleep(100);
            }
        }

    }
}
