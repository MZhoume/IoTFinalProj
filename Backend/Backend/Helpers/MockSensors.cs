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

        public void RelayOn(int id)
        {
            Console.WriteLine($"Relay on... {id}");
        }

        public void RelayOff(int id)
        {
            Console.WriteLine($"Relay off... {id}");
        }
    }
}
