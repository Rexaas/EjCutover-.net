using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;
using System.Xml;
using System.Xml.Linq;

namespace EJCutOver
{
    public partial class Service1 : ServiceBase
    {
        private Timer timer;
        public Service1()
        {
            InitializeComponent();
            this.ServiceName = "EJCutOver";

        }

        protected override void OnStart(string[] args)
        {
            timer = new Timer();
            timer.Interval = 60000; // 1 minute (adjust as needed)
            timer.Elapsed += TimerElapsed;
            timer.Start();
        }

        protected override void OnStop()
        {
            timer.Stop();
            timer.Dispose();
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            // Check the current time
            DateTime currentTime = DateTime.Now;

            if (currentTime.Hour == 00 && currentTime.Minute == 05)
            {
                PerformBackupAndLogActions();
            }
        }

        private void PerformBackupAndLogActions()
        {
            // Get today's date
            DateTime today = DateTime.Now;

            // Calculate the previous day
            DateTime previousDay = today.AddDays(-1);

            // Format the date as "yyyyMMdd"
            string formattedDate = previousDay.ToString("yyyyMMdd");

            string sourceFilePath = @"C:\Program Files (x86)\NCR APTRA\Advance NDC\Data\EJDATA.LOG";
            string secFilePath = @"C:\Program Files (x86)\NCR APTRA\Advance NDC\Data\EJBackups";
            string destinationFolder = @"E:\SecEJBackups";
            string logFilePath = @"C:\Program Files (x86)\NCR APTRA\Advance NDC\Data\EJDATA.LOG";

            string directoryPath = "E:\\SecEJBackups";

            // Construct the search pattern with the formatted date
            string searchPattern = $"*{formattedDate}.*";

            // Combine the directory path and search pattern
            string filePath = Path.Combine(directoryPath, searchPattern);

            // Get files that match the search pattern
            string[] matchingFiles = Directory.GetFiles(directoryPath, searchPattern);

            // Check if any matching files exist
            if (!(matchingFiles.Length > 0))
            {
                try
                {
                    // Read machineId from conf.xml
                    string confFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "conf.xml");
                    XDocument doc = XDocument.Load(confFilePath);
                    string machineId = doc.Element("Configuration").Element("MachineId").Value;
                    if (!Directory.Exists(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }

                    // Create a backup with today's date and machineId in the file name
                    string backupFileName = $"EJ{machineId}{formattedDate}.log";
                    string backupFilePath = Path.Combine(destinationFolder, backupFileName);
                    string backupFilePath2 = Path.Combine(secFilePath, backupFileName);

                    File.Copy(sourceFilePath, backupFilePath2);
                    File.Copy(sourceFilePath, backupFilePath);
                    File.WriteAllText(logFilePath, "EJ CutOver Successful at " + DateTime.Now.ToString("yyyyMMdd"));
                }
                catch (Exception ex)
                {
                    // Handle exceptions, e.g., log them
                    EventLog.WriteEntry("ClockMonitoringService", $"Error: {ex.Message}", EventLogEntryType.Error);
                }

            }
            


            /*try
            {
                // Create a backup with today's date
                string backupFileName = DateTime.Now.ToString("ddMMyyyy") + ".log";
                string backupFilePath = Path.Combine(destinationFolder, backupFileName);

                File.Copy(sourceFilePath, backupFilePath);
                File.WriteAllText(logFilePath, DateTime.Now.ToString("ddMMyyyy"));
            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., log them
                EventLog.WriteEntry("EJCutOver", $"Error: {ex.Message}", EventLogEntryType.Error);
            }*/
        }
    }
}
