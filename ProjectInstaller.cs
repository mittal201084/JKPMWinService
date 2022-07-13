using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace JKPMMLService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            var config = ConfigurationManager.OpenExeConfiguration(this.GetType().Assembly.Location);

            string instanceName = config.AppSettings.Settings["Installer_NamedInstanceName"].Value;
            string instanceID = config.AppSettings.Settings["Installer_NamedInstanceID"].Value;
            bool usesNamedInstance = !string.IsNullOrWhiteSpace(instanceName) && !string.IsNullOrWhiteSpace(instanceID);

            if (usesNamedInstance)
            {
                foreach (var installer in this.Installers)
                {
                    if (installer is ServiceInstaller)
                    {
                        var ins = (ServiceInstaller)installer;
                        ins.ServiceName =  instanceID; //ins.ServiceName + "-" +instanceID
                        // Want the service to be named SVC (Instance Name) Audit Log Blah Blah Service
                        ins.DisplayName = ins.DisplayName.Replace("JKPMService ", "JKPMService (" + instanceName + ") ");
                    }
                }
            }
        }
    }
}
