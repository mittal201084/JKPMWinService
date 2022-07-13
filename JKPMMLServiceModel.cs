using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
namespace JKPMMLService
{
    public partial class JKPMMLServiceModel : ServiceBase
    {
        System.Timers.Timer timer = new System.Timers.Timer(); // name space(using System.Timers;)  
        public JKPMMLServiceModel()
        {
            InitializeComponent();
            Int32 intTimerMS = Convert.ToInt32(ConfigurationSettings.AppSettings["intTimerMS"]);
            timer.Interval = intTimerMS * 60 * 1000;
            timer.Enabled = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(OnElapsedTime);
        }


        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at  : " + DateTime.Now);
            
            timer.Enabled = true;
            timer.Start();
        }
        protected override void OnStop()
        {
            WriteToFile("Service is started at " + DateTime.Now);
            timer.Enabled = false;
            timer.Stop();
        }

        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            string strFileName = string.Empty;
            WriteToFile("Service is OnElapsedTime at " + DateTime.Now);

            DataSet objDataSet = new DataSet();
            strFileName = AppDomain.CurrentDomain.BaseDirectory + "\\MLModelConfiguration.xml";
            objDataSet = new DataSet();
            objDataSet.ReadXml(strFileName);
            timer.Enabled = false;
           
            CallingMLModels(objDataSet);
            timer.Enabled = true;
           
        }


        private bool CallingMLModels(DataSet dsResult)
        {
            if (dsResult != null && dsResult.Tables != null && dsResult.Tables.Count > 0 && dsResult.Tables[0] != null && dsResult.Tables[0].Rows.Count > 0)
            {

                DataTable Dt = new DataTable();

               //DataRow[] rows = dsResult.Tables[0].Select("chrStatus='chrStatus'");


                Dt = dsResult.Tables[0].AsEnumerable().Where(x => x.Field<string>("chrStatus") == "Y").CopyToDataTable();

                long total = 0;

                // Use type parameter to make subtotal a long, not an int
                Parallel.For<long>(0, Dt.Rows.Count, () => 0, (i, loop, subtotal) =>
                      {
                          subtotal += i;

                          try
                          {
                              ProcessStartInfo sInfo = new ProcessStartInfo();
                              sInfo.FileName = Dt.Rows[i]["Path"].ToString(); ;
                              WriteToFile("batchfile is started at " + Dt.Rows[i]["Name"].ToString() + DateTime.Now);
                              //sInfo.Arguments = GetArguments();
                              sInfo.UseShellExecute = true;
                              sInfo.CreateNoWindow = true;
                              sInfo.ErrorDialog = false;
                              sInfo.WindowStyle = ProcessWindowStyle.Hidden;

                              //sInfo.RedirectStandardError = true;  //didn't work
                              //sInfo.RedirectStandardInput = true;  //didn't work
                              //sInfo.RedirectStandardOutput = true;  //didn't work


                              using (Process exeProcess = Process.Start(sInfo))
                              {
                                  //StreamWriter inputWriter = exeProcess.StandardInput;
                                  //StreamReader outputReader = exeProcess.StandardOutput;
                                  //StreamReader errorReader = exeProcess.StandardError;

                                  WriteToFile("batchfile is End at " + Dt.Rows[i]["Name"].ToString() + DateTime.Now);
                                  exeProcess.WaitForExit();
                              }

                              return 1;

                          }
                          catch (Exception ex)
                          {
                              
                              timer.Enabled = true;
                              //timer.Start();
                              return 0;

                          }

                      },
                       (x) => Interlocked.Add(ref total, x)
                      );
            }
            return true;
        }



        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}
