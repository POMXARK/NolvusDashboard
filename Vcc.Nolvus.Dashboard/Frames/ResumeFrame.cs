﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using System.Windows.Forms;
using Vcc.Nolvus.Core.Interfaces;
using Vcc.Nolvus.Core.Frames;
using Vcc.Nolvus.Core.Services;
using Vcc.Nolvus.Core.Enums;
using Vcc.Nolvus.Dashboard.Forms;
using Vcc.Nolvus.NexusApi;

namespace Vcc.Nolvus.Dashboard.Frames
{
    public partial class ResumeFrame : DashboardFrame
    {
        public ResumeFrame()
        {
            InitializeComponent();
        }

        public ResumeFrame(IDashboard Dashboard, FrameParameters Params)
            : base(Dashboard, Params)            
        {
            InitializeComponent();
        }

        private int DownloadLocationIndex(List<string> Locations)
        {
            int Index = 0;

            if (ServiceSingleton.Instances.WorkingInstance.Settings.CDN != string.Empty)
            {
                foreach (var Location in Locations)
                {
                    if (Location == ServiceSingleton.Instances.WorkingInstance.Settings.CDN)
                    {
                        break;
                    }

                    Index++;
                }
            }

            return Index;
        }

        private List<string> GetDownloadLocations()
        {
            List<string> Result = new List<string>();

            Result.Add("Paris");
            Result.Add("Nexus CDN");
            Result.Add("Amsterdam");
            Result.Add("Prague");
            Result.Add("Chicago");
            Result.Add("Los Angeles");
            Result.Add("Miami");
            Result.Add("Singapore");

            return Result;
        }

        protected override void OnLoad()
        {
            ServiceSingleton.Dashboard.Title("Nolvus Dashboard - [Instance Auto Installer]");

            DrpDwnLstInstances.DataSource = ServiceSingleton.Instances.InstancesToResume;
            DrpDwnLstInstances.DisplayMember = "Name";
            DrpDwnLstInstances.SelectedIndex = 0;

            if (!ApiManager.AccountInfo.IsPremium)
            {
                LblDownLoc.Visible = false;
                DrpDwnLstDownLoc.Visible = false;
            }

            if (ServiceSingleton.Settings.ErrorsThreshold == 1 || !NexusApi.ApiManager.AccountInfo.IsPremium)
            {
                RdBtnOneError.Checked = true;
                RdBtnThreshold.Text = string.Format("Stop the installation when {0} errors occured and display the error messages (max errors can be set up in the Nolvus Dashboard.ini file)", 50);
            }
            else if (ServiceSingleton.Settings.ErrorsThreshold == 0)
            {
                RdBtnNoThreshold.Checked = true;
                RdBtnThreshold.Text = string.Format("Stop the installation when {0} errors occured and display the error messages (max errors can be set up in the Nolvus Dashboard.ini file)", 50);
            }
            else
            {
                RdBtnThreshold.Checked = true;
                RdBtnThreshold.Text = string.Format("Stop the installation when {0} errors occured and display the error messages (max errors can be set up in the Nolvus Dashboard.ini file)", ServiceSingleton.Settings.ErrorsThreshold);
            }
        }       

        private void DrpDwnLstInstances_SelectedIndexChanged(object sender, EventArgs e)
        {
            var Instance = DrpDwnLstInstances.SelectedItem as INolvusInstance;

            ServiceSingleton.Instances.WorkingInstance = Instance;

            if (ApiManager.AccountInfo.IsPremium)
            {
                DrpDwnLstDownLoc.DataSource = GetDownloadLocations();
                DrpDwnLstDownLoc.SelectedIndex = DownloadLocationIndex(GetDownloadLocations());
            }
        }

        private void Resume_Click(object sender, EventArgs e)
        {
            if (RdBtnOneError.Checked)
            {
                ServiceSingleton.Settings.StoreIniValue("Process", "ErrorsThreshold", "1");
            }
            else if (RdBtnNoThreshold.Checked)
            {
                ServiceSingleton.Settings.StoreIniValue("Process", "ErrorsThreshold", "0");
            }
            else
            {
                if (ServiceSingleton.Settings.ErrorsThreshold == 1 || ServiceSingleton.Settings.ErrorsThreshold == 0)
                {
                    ServiceSingleton.Settings.StoreIniValue("Process", "ErrorsThreshold", "50");
                }
            }

            ServiceSingleton.Dashboard.LoadFrameAsync<PackageFrame>(); 
        }

        private void DrpDwnLstDownLoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            var Instance = DrpDwnLstInstances.SelectedItem as INolvusInstance;

            Instance.Settings.CDN = DrpDwnLstDownLoc.SelectedItem.ToString();

            ServiceSingleton.Instances.Save();            
        }               

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (NolvusMessageBox.ShowConfirmation("Cancel install?", "Are you sure you want to cancel installation? Your current installation will be deleted.") == DialogResult.Yes)
            {                
                ServiceSingleton.Dashboard.LoadFrame<DeleteFrame>(
                    new FrameParameters(
                        new FrameParameter()
                        {
                            Key ="Instance", Value= DrpDwnLstInstances.SelectedItem as INolvusInstance
                        }, 
                        new FrameParameter()
                        {
                            Key ="Action", Value=InstanceAction.Cancel
                        }
                        ));               
            }
        }
    }
}
