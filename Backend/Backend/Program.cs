using System;
using Nancy;
using Nancy.Hosting.Self;
using Backend.Models;
using System.IO;
using System.Linq;
using Backend.Services;
using Backend.Helpers;
using System.Threading;
using System.Threading.Tasks;

namespace Backend
{
    public class MainClass : NancyModule
    {
        static DBManager _manager;
        static ISensors _sensor;

        static int[] fanId = { 5, 6 };
        static int[] pumpId = { 14, 15 };

        public MainClass()
        {
            StaticConfiguration.DisableErrorTraces = false;

            #region paths

            Get["/"] = _ => "Hello IoT";

            Get["/info"] = _ =>
            {
                return Response.AsJson(
                    _manager.Query<ItemId>("SELECT * FROM ItemIds").Select(id =>
                    {
                        var item = _manager.Query<Item>("SELECT * FROM Items WHERE ItemId = @Id ORDER BY rowid DESC LIMIT 1", new { Id = id.Id }).FirstOrDefault();
                        return new
                        {
                            id.Name,
                            id.Id,
                            item.Weight,
                            item.Temperature,
                            item.Humidity,
                            item.Price,
                            id.MinWeight,
                            id.PriceThreshold
                        };
                    })
                );
            };

            Get["/info/{id}"] = _ =>
            {
                var id = (int)int.Parse(_.id);
                var item = _manager.Query<Item>("SELECT * FROM Items WHERE ItemId = @Id ORDER BY rowid DESC LIMIT 1", new { Id = id }).FirstOrDefault();
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
                Console.WriteLine("Starting new humidity thread...");
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
                return $"adjusting... humidity of {_.id} to {humidity}";
            };

            Get["/adjust/{id}/temperature"] = _ =>
            {
                var temperature = (double)double.Parse(Request.Query.t);
                var id = (int)int.Parse(_.id);
                Console.WriteLine("Starting new temperature thread...");
                Task.Factory.StartNew(() =>
                {
                    id -= 1;
                    var readouts = _sensor.GetSensorData()[id];
                    while (readouts.Temperature < temperature)
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
                return $"adjusting... temperature of {_.id} to {temperature}";
            };

            Get["/price/{id}"] = _ =>
            {
                var id = (int)int.Parse(_.id);
                return _manager.Query<Item>("SELECT * FROM Items WHERE ItemId = @Id ORDER BY rowid DESC LIMIT 1", new { Id = id }).FirstOrDefault().Price.ToString();
            };

            Get["/price/{id}/set"] = _ =>
            {
                var id = (int)int.Parse(_.id);
                var price = (double)double.Parse(Request.Query.p);

                return _manager.Execute("UPDATE Items SET Price = @P WHERE rowid = (SELECT MAX(rowid) from Items WHERE ItemId = @Id)", new { P = price, Id = id }).ToString();
            };

            Get["/plan"] = _ =>
            {
                // TODO: find a way to predict the stocking plan
                return "Some stocking plan...";
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
            _sensor = ServiceLocator.GetService<ISensors, Sensors>();

            _manager.Execute("DELETE FROM Items");
            foreach (var id in _manager.Query<ItemId>("SELECT * FROM ItemIds"))
            {
                _manager.Execute("UPDATE ItemIds SET DaysInStock = 0 WHERE Id = @Id", new { Id = id.Id });
            }

            #region initialize

            double refreshTime = 8000;

            Daemon.Create(refreshTime, (s, e) =>
            {
                Console.WriteLine("Getting sensor data...");
                var readouts = _sensor.GetSensorData();

                if (Math.Abs(readouts[0].Humidity) < 0.01
                    || Math.Abs(readouts[0].Temperature) < 0.01
                    || Math.Abs(readouts[1].Humidity) < 0.01
                    || Math.Abs(readouts[1].Temperature) < 0.01
                   || readouts[0].Weight > 1000
                   || readouts[1].Weight > 1000)
                    return;

                for (int i = 0; i < 2; i++)
                {
                    var itemid = _manager.Query<ItemId>("SELECT * FROM ItemIds WHERE Id = @Id", new { Id = i + 1 }).FirstOrDefault();
                    var weight = readouts[i].Weight;
                    var humidity = readouts[i].Humidity;
                    var temperature = readouts[i].Temperature;
                    var price = itemid.FullPrice - (itemid.FullPrice - itemid.PriceThreshold) / itemid.DaysAlive * itemid.DaysInStock;
                    _manager.Execute("INSERT INTO Items (ItemId, Weight, Humidity, Temperature, Price) VALUES (@Id, @W, @H, @T, @P)", new { Id = i + 1, W = weight, H = humidity, T = temperature, P = price });
                }
            }, true, false);

            Daemon.Create(3 * refreshTime, (s, e) =>
            {
                var itemIds = _manager.Query<ItemId>("SELECT * FROM ItemIds");
                foreach (var id in itemIds)
                {
                    _manager.Execute("UPDATE ItemIds SET DaysInStock = @D WHERE Id = @Id", new { D = id.DaysInStock + 1, Id = id.Id });
                }
            }, true, false);

            #endregion

            using (var host = new NancyHost(new Uri("http://localhost:80")))
            {
                host.Start();
                Console.WriteLine("Running... on port 80");
                Console.ReadLine();
            }
        }
    }
}
