using log4net;
using log4net.Config;
using System;
using System.IO;

namespace ExampleApp
{
    /// <summary>
    /// 111
    /// </summary>
    public class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));

        /// <summary>Defines the entry point of the application.</summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            // Add a custom property to test it logs properly
            LogicalThreadContext.Properties["CustomProperty"] = "Custom value";

            // Read in text and log it out immediately
            Console.WriteLine("Anything you type will be logged!  Type 'Exit' to exit!");
            var inputText = string.Empty;
            do
            {
                inputText = Console.ReadLine();
                if (inputText == "Throw")
                {
                    try
                    {
                        throw new InvalidOperationException("Doh!");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Log.Error(inputText, ex);
                    }
                }
                Log.DebugFormat("You Wrote: \"{0}\"", inputText);
            }
            while (inputText != "Exit");
        }
    }
}
