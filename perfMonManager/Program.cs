using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

using ETM.WCCOA;
using System.Diagnostics;

namespace GettingStarted
{
    // This C# program provides the same functionality as the C++ template manager.
    // it establishes connection to a project
    // reads necessary information from the config File if available
    // Connects on DPE1 and forwards value changes to DPE2
    class Program
    {
        static void Main(string[] args)
        {
            // Create Manager object
            OaManager myManager = OaSdk.CreateManager();

            // Initialize Manager Configuration
            myManager.Init(ManagerSettings.DefaultApiSettings, args);

            // Start the Manager and Connect to the OA project with the given configuration
            myManager.Start();

            // Read from config File Section myCsTest. 
            // ReadString(section, key, defaultval)
            OaConfigurationFile file = new OaConfigurationFile();
            string dpNameSet = file.ReadString("myCsTest", "dpNameSet", "ExampleDP_Arg2.");
            string dpNameConnect = file.ReadString("myCsTest", "dpNameConnect", "ExampleDP_Arg1.");

            //Alex code
            PerformanceCounter cpuCounter;
            PerformanceCounter ramCounter;
            PerformanceCounter networkCounter;
            PerformanceCounter diskCounter;

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            //networkCounter = new PerformanceCounter("Network Adapter", "Bytes Received/sec");

            PerformanceCounterCategory category = new PerformanceCounterCategory("Network Adapter");
            String[] instancename = category.GetInstanceNames();

            foreach (string name in instancename)
            {
                Console.WriteLine(name);
            }
            PerformanceCounter[] counterName = category.GetCounters("Intel[R] Dual Band Wireless-AC 8260");

            foreach (PerformanceCounter name in counterName)
            {
                Console.WriteLine(name.CounterName);
            }

            Process[] processlist = Process.GetProcesses();

            foreach (Process theprocess in processlist)
            {
                if (theprocess.ProcessName.StartsWith("WCC"))
                    Console.WriteLine("Process: {0} ID: {1}", theprocess.ProcessName, theprocess.Id);
            }

            // Get Access to the ProcessValues
            var valueAccess = myManager.ProcessValues;

            // Create Subscription object
            var mySubscription = valueAccess.CreateDpValueSubscription();

            // Append Datapoints to subcribe on
            mySubscription.AddDp(dpNameConnect);

            // Define Lambda function for value changed event. Can be done as shon here as Lambda function or as seperate function
            mySubscription.SingleValueChanged += (vcsender, vce) =>
            {
                // vce.Value can be null in error case 
                if(vce.Value == null)
                  return;
                
                Console.WriteLine("Received value: " + vce.Value.DpValue.ToString() + " for DPE: " + vce.Value.DpName.ToString());
                
                //Set received value on DPE dpNameSet
                valueAccess.SetDpValue(dpNameSet, vce.Value.DpValue.ToDouble());
                Console.WriteLine("Set value: " + vce.Value.DpValue.ToString() + " also on DPE: "+dpNameSet);
            };

            // If FireChangedEventForAnswer is set to true, the ValueChanged Event is alsed fired for the first answer
            mySubscription.FireChangedEventForAnswer = true;

            // Start the subscription and as an additional option wait for the first anwer as result value
            mySubscription.StartAsync();


            //getCategories();
            while (true)
            {
                Console.WriteLine(cpuCounter.NextValue() + "%");

                Console.WriteLine(ramCounter.NextValue() + "MB");
                //Console.WriteLine(networkCounter.NextValue() + "MB/s");
                Thread.Sleep(5000);
            }

        }

        public static void getCategories()
        {
            string machineName = "";
            PerformanceCounterCategory[] categories;


            // Generate a list of categories registered on the specified computer.
            try
            {
                if (machineName.Length > 0)
                {
                    categories = PerformanceCounterCategory.GetCategories(machineName);
                }
                else
                {
                    categories = PerformanceCounterCategory.GetCategories();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to get categories on " +
                    (machineName.Length > 0 ? "computer \"{0}\":" : "this computer:"), machineName);
                Console.WriteLine(ex.Message);
                return;
            }

            Console.WriteLine("These categories are registered on " +
                (machineName.Length > 0 ? "computer \"{0}\":" : "this computer:"), machineName);

            // Create and sort an array of category names.
            string[] categoryNames = new string[categories.Length];
            int objX;
            for (objX = 0; objX < categories.Length; objX++)
            {
                categoryNames[objX] = categories[objX].CategoryName;
            }
            Array.Sort(categoryNames);

            for (objX = 0; objX < categories.Length; objX++)
            {
                Console.WriteLine("{0,4} - {1}", objX + 1, categoryNames[objX]);
            }
        }
    }
}