using System;
namespace Backend.Models
{
    public class ItemId
    {
        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public double PriceThreshold
        {
            get;
            set;
        }

        public double FullPrice
        {
            get;
            set;
        }

        public int DaysInStock
        {
            get;
            set;
        }

        public int DaysAlive
        {
            get;
            set;
        }
    }
}
