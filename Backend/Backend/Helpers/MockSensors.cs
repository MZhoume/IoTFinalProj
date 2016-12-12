using System;
namespace Backend.Helpers
{
    public class MockSensors : ISensors
    {
        private Random _random = new Random();

        public SensorOutput[] GetSensorData()
        {
            return new SensorOutput[]
            {
                new SensorOutput()
                {
                    Weight = _random.Next(0, 100),
                    Humidity = _random.Next(0, 100),
                    Temperature = _random.Next(0, 100)
                },
                new SensorOutput()
                {
                    Weight = _random.Next(0, 100),
                    Humidity = _random.Next(0, 100),
                    Temperature = _random.Next(0, 100)
                }
            };
        }

        public void FanOn(int id)
        {
            Console.WriteLine($"Fan on... {id}");
        }

        public void FanOff(int id)
        {
            Console.WriteLine($"Fan off... {id}");
        }

        public void PumpOn(int id)
        {
            Console.WriteLine($"Pump on... {id}");
        }

        public void PumpOff(int id)
        {
            Console.WriteLine($"Pump off... {id}");
        }
    }
}
