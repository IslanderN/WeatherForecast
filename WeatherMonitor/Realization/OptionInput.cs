using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WeatherMonitor.Realization
{
    using Options;
    using Interfaces;
    public class OptionInput : IOptionInput
    {
        private readonly TimeoutOption timeoutOption;
        private readonly MonitorOption monitorOption;
        private readonly CityListReader cityListReader;
        public OptionInput(TimeoutOption timeoutOption, MonitorOption monitorOption, CityListReader cityListReader)
        {
            this.timeoutOption = timeoutOption;
            this.monitorOption = monitorOption;
            this.cityListReader = cityListReader;
            cityListReader.Read();
        }

        public void Input()
        {
            Console.WriteLine("Input period of check weather in muntes");
            this.timeoutOption.TimeoutInMinutes = AskForInput<double>(
                "You enter incorrect time period, try again",
                (input) => double.TryParse(input, out double temp) && temp >= 1,
                (input) => double.Parse(input)
                );

            Console.WriteLine("\nInput City");
            this.monitorOption.City = AskForInput<string>(
                "Cannot find this city, please try another",
                (input) => cityListReader.Cities.FirstOrDefault(c => string.Equals(input.ToLower(), c.Name.ToLower())) != null,
                (input) => cityListReader.Cities.FirstOrDefault(c => string.Equals(input.ToLower(), c.Name.ToLower())).Name
                );

            Console.WriteLine("\nInput Country");
            this.monitorOption.Country = AskForInput<string>(
                "Cannot find this country, please try another",
                (input) => cityListReader.Cities.Where(c=>c.Name == this.monitorOption.City)
                            .FirstOrDefault(c => string.Equals(input.ToLower(), c.Country.ToLower())) != null,
                (input) => cityListReader.Cities.Where(c => c.Name == this.monitorOption.City)
                            .FirstOrDefault(c => string.Equals(input.ToLower(), c.Country.ToLower())).Country
                );

            Console.WriteLine("Input unit of measure");
            this.monitorOption.Unit = AskForInput<Unit>(
                "You enter incorrect unit, please try again",
                (input) => Enum.TryParse<Unit>(input.ToLower(), out _),
                (input) => Enum.Parse<Unit>(input.ToLower())
                );

            Console.WriteLine("Input language");
            this.monitorOption.Language = AskForInput<Language>(
                    "You enter incorrect language, please try again",
                    (input) => Enum.TryParse<Language>(input.ToLower(), out _),
                    (input) => Enum.Parse<Language>(input.ToLower())
                    );

            Console.WriteLine("Input number of days");
            this.monitorOption.Days = AskForInput<int>(
                "You enter incorrect time period, try again",
                (input) => int.TryParse(input, out int temp) && temp >= 1 && temp <= 5,
                (input) => int.Parse(input)
                );
        }

        private T AskForInput<T>(string errorMessage, Func<string, bool> checker, Func<string, T> converter)
        {
            string input = string.Empty;
            bool isCorrect = false;
            while (!isCorrect)
            {
                input = Console.ReadLine();
                if (!(isCorrect = checker(input)))
                {
                    Console.WriteLine(errorMessage);
                }
            }

            return converter(input);
        }
    }
}
