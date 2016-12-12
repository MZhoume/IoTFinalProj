using System;
using System.Diagnostics;
using Accord.Neuro;

namespace Backend.Helpers
{
    public class Sensors : ISensors
    {
        Network _network = Network.Load("network.net");

        public SensorOutput[] GetSensorData()
        {
            var output = new SensorOutput[]
            {
                new SensorOutput(),
                new SensorOutput()
            };

            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = "HW/temphum.py",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.WaitForExit();

            for (int i = 0; i < 2; i++)
            {
                var info = proc.StandardOutput.ReadLine().Split('|');
                output[i].Temperature = _network.Compute(new double[] { double.Parse(info[0]) / 150.0 })[0] * 150.0;
                output[i].Humidity = _network.Compute(new double[] { double.Parse(info[1]) / 150.0 })[0] * 150.0;
            }

            return output;
        }

        public void FanOn(int id)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"HW/relay_on.py ${id}"
                }
            }.Start();
        }

        public void FanOff(int id)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"HW/relay_off.py ${id}"
                }
            }.Start();
        }

        public void PumpOn(int id)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"HW/pump_on.py ${id}"
                }
            }.Start();
        }

        public void PumpOff(int id)
        {
            new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "python",
                    Arguments = $"HW/pump_off.py ${id}"
                }
            }.Start();
        }
    }
}
