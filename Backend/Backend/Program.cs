using System;
using Nancy;
using Nancy.Hosting.Self;
using Backend.Models;
using System.IO;
using System.Linq;
using Backend.Services;
using Backend.Helpers;
using DapperExtensions;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public class MainClass : NancyModule
    {
        static DBManager _manager;
        static ISensors _sensor;

        // TODO: get the pump and fan id for each items
        static int[] fanId = { 5, 6 };
        static int[] pumpId = { 3, 4 };

        public MainClass()
        {
            StaticConfiguration.DisableErrorTraces = false;

            #region paths

            Get["/"] = _ => "Hello IoT";

            Get["/info"] = _ =>
            {
                return Response.AsJson(
                    _manager.GetList<ItemId>(null)
                    .Select(itemId =>
                        {
                            var item = _manager.GetList<Item>(Predicates.Field<Item>(i => i.ItemId, Operator.Eq, itemId.Id))
                                               .OrderBy(i => i.RowId)
                                               .LastOrDefault();
                            return new
                            {
                                itemId.Name,
                                itemId.Id,
                                item.Weight,
                                item.Temperature,
                                item.Humidity,
                                item.Price,
                                itemId.MinWeight,
                                itemId.PriceThreshold
                            };
                        })
                );
            };

            Get["/info/{id}"] = _ =>
            {
                var id = (int)int.Parse(_.id);
                var item = _manager.GetList<Item>(Predicates.Field<Item>(i => i.ItemId, Operator.Eq, id))
                        .OrderBy(i => i.RowId)
                        .LastOrDefault();
                return Response.AsJson(
                    new
                    {
                        item.Weight,
                        item.Temperature,
                        item.Humidity,
                        item.Price
                    }
                );
            };

            Get["/info/{id}/add"] = _ =>
            {
                var id = (int)int.Parse(_.id);
                var weight = (double)double.Parse(Request.Query.w);
                var humidity = (double)double.Parse(Request.Query.h);
                var temperature = (double)double.Parse(Request.Query.t);

                return _manager.Execute("INSERT INTO Items (ItemId, Weight, Humidity, Temperature) VALUES (@Id, @W, @H, @T)", new { Id = id, W = weight, H = humidity, T = temperature }).ToString();
            };

            Get["/adjust/{id}/humidity"] = _ =>
            {
                var humidity = (double)double.Parse(Request.Query.h);
                var id = (int)int.Parse(_.id);
                Task.Factory.StartNew(() =>
                {
                    id -= 1;
                    var readouts = _sensor.GetSensorData()[id];
                    while (readouts.Humidity < humidity)
                    {
                        _sensor.RelayOn(fanId[id]);
                        _sensor.RelayOn(pumpId[id]);
                        Thread.Sleep(1000);
                        _sensor.RelayOff(fanId[id]);
                        _sensor.RelayOff(pumpId[id]);
                        Thread.Sleep(1000);
                        readouts = _sensor.GetSensorData()[id];
                    }
                });
                return $"adjusting... {_.id} to {humidity}";
            };

            Get["/adjust/{id}/temperature"] = _ =>
            {
                var temperature = (double)double.Parse(Request.Query.t);
                return $"adjusting... {_.id} to {temperature}";
            };

            Get["/price/{id}"] = _ =>
            {
                var id = (int)int.Parse(_.id);
                return _manager.GetList<Item>(Predicates.Field<Item>(i => i.ItemId, Operator.Eq, id))
                        .OrderBy(i => i.RowId)
                        .LastOrDefault().Price.ToString();
            };

            Get["/price/{id}/set"] = _ =>
            {
                var id = (int)int.Parse(_.id);
                var price = (double)double.Parse(Request.Query.p);

                id = _manager.GetList<Item>(Predicates.Field<Item>(i => i.ItemId, Operator.Eq, id))
                       .OrderBy(i => i.RowId)
                       .LastOrDefault().RowId;

                return _manager.Execute("UPDATE Items SET Price = @P WHERE rowid = @rowid", new { P = price, rowid = id }).ToString();
            };

            Get["/plan"] = _ =>
            {
                // TODO: find a way to predict the stocking plan
                return "this is a stocking plan...";
            };

            Get["/refill/{id}"] = _ =>
            {
                var id = (int)int.Parse(_.id);

                return _manager.Execute("UPDATE ItemIds SET DaysInStock = 0 WHERE Id = @Id", new { Id = id }).ToString();
            };

            #endregion
        }

        public static void Main(string[] args)
        {
            _manager = ServiceLocator.GetService<DBManager>();
            _sensor = ServiceLocator.GetService<ISensors, MockSensors>();

            _manager.Execute("DELETE FROM Items");
            foreach (var id in _manager.GetList<ItemId>(null))
            {
                _manager.Execute("UPDATE ItemIds SET DaysInStock = 0 WHERE Id = @Id", new { Id = id.Id });
            }

            #region initialize

            // it takes 6s to get the sensor's readouts
            double refreshTime = 10000;

            Daemon.Create(refreshTime, (s, e) =>
            {
                Console.WriteLine("Getting sensor data...");
                var readouts = _sensor.GetSensorData();

                if ((readouts[0].Humidity == 0
                   && readouts[0].Temperature == 0)
                   || (readouts[1].Humidity == 0
                   && readouts[1].Temperature == 0))
                    return;

                for (int i = 0; i < 2; i++)
                {
                    var itemid = _manager.GetById<ItemId>(i + 1);
                    var weight = readouts[i].Weight;
                    var humidity = readouts[i].Humidity;
                    var temperature = readouts[i].Temperature;
                    var price = itemid.FullPrice - (itemid.FullPrice - itemid.PriceThreshold) / itemid.DaysAlive * itemid.DaysInStock;
                    _manager.Execute("INSERT INTO Items (ItemId, Weight, Humidity, Temperature, Price) VALUES (@Id, @W, @H, @T, @P)", new { Id = i + 1, W = weight, H = humidity, T = temperature, P = price });
                }
            }, true, false);

            Daemon.Create(3 * refreshTime, (s, e) =>
            {
                var itemIds = _manager.GetList<ItemId>(null);
                foreach (var id in itemIds)
                {
                    _manager.Execute("UPDATE ItemIds SET DaysInStock = @D WHERE Id = @Id", new { D = id.DaysInStock + 1, Id = id.Id });
                }
            }, true, false);

            #endregion

            using (var host = new NancyHost(new Uri("http://localhost:5000")))
            {
                host.Start();
                Console.WriteLine("Running... on port 5000");
                Console.ReadLine();
            }
        }
    }
}
