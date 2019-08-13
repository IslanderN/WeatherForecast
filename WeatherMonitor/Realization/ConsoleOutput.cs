using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherMonitor.Realization
{
    using Interfaces;
    using Microsoft.Extensions.Logging;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class ConsoleOutput : IOutput
    {
        private readonly ILogger logger;
        private readonly StringBuilder stringBuilder;


        public ConsoleOutput(ILogger<ConsoleOutput> logger)
        {
            this.logger = logger;
            this.stringBuilder = new StringBuilder();
        }



        public void StartMessage()
        {
            stringBuilder.Clear();
            stringBuilder.Append(Environment.NewLine);
        }

        public void WriteMessage(string message)
        {
            //lock (locker)
            //{
            //    Console.BackgroundColor = ConsoleColor.Black;
            //    Console.WriteLine(message);
            //}
            this.stringBuilder.Append(message)
                .Append(Environment.NewLine);
        }
        public void EndMessage()
        {
            
            stringBuilder.Append(Environment.NewLine);
            //Console.BackgroundColor = ConsoleColor.Black;
            //Console.ForegroundColor = ConsoleColor.White;
            //Console.WriteLine(stringBuilder.ToString());
            this.logger.LogInformation(stringBuilder.ToString());
        }
    }
}
