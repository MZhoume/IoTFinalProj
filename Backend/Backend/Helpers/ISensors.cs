using System;
namespace Backend.Helpers
{
    public interface ISensors
    {
        SensorOutput[] GetSensorData();

        void RelayOn(int id);

        void RelayOff(int id);
    }
}
