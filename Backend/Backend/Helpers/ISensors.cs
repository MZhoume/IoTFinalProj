using System;
namespace Backend.Helpers
{
    public interface ISensors
    {
        SensorOutput[] GetSensorData();

        void FanOn(int id);

        void FanOff(int id);

        void PumpOn(int id);

        void PumpOff(int id);
    }
}
