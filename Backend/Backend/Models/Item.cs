﻿using System;
namespace Backend.Models
{
    public class Item
    {
        public int RowId
        {
            get;
            set;
        }

		public int ItemId
        {
            get;
            set;
        }

        public double Weight
        {
            get;
            set;
        }

        public double Humidity
        {
            get;
            set;
        }

        public double Temperature
        {
            get;
            set;
        }

        public double Price
        {
            get;
            set;
        }
    }
}
