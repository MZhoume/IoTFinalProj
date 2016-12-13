using System;
using System.Collections;
using System.Linq;

namespace Test
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            var dbmanager = new DBManager();
            dbmanager.Query<ItemId>("SELECT * FROM ItemIds")
                     .ToList()
                     .ForEach(i => Console.WriteLine(i.Name));

            Console.ReadLine();
        }
    }
}
