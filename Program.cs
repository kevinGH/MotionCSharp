using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Tester_CSharp
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length == 8)
            {
                Console.WriteLine(args[0].ToString().Trim()); // ip
                Console.WriteLine(args[1].ToString().Trim()); // port
                Console.WriteLine(args[2].ToString().Trim()); // username
                Console.WriteLine(args[3].ToString().Trim()); // password
                Console.WriteLine(args[4].ToString().Trim()); // CentralID
                Console.WriteLine(args[5].ToString().Trim()); // LocalID
                Console.WriteLine(args[6].ToString().Trim()); // StartTime
                Console.WriteLine(args[7].ToString().Trim()); // EndTime
            

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Tester_CSharp(args[0].ToString().Trim(), args[1].ToString().Trim(), args[2].ToString().Trim(), args[3].ToString().Trim(), args[4].ToString().Trim(), args[5].ToString().Trim(), DateTime.Parse(args[6].ToString().Trim()), DateTime.Parse(args[7].ToString().Trim())));
            }


            
        }
    }
}