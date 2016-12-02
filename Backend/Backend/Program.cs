using System;
using Nancy;
using Nancy.Hosting.Self;
using Backend.Models;
using System.IO;
using System.Linq;

namespace Backend
{
    public class MainClass : NancyModule
    {
        private DBManager _manager = new DBManager();

        public MainClass()
        {
            StaticConfiguration.DisableErrorTraces = false;

            #region paths

            Get["/"] = _ => "Hello IoT";

            Get["/info"] = _ =>
            {
                return Response.AsJson(_manager.Query<Item>(@"SELECT * FROM Items"));
            };

            Get["/info/{id}"] = _ =>
            {
                return Response.AsJson(_manager.Query<Item>(@"SELECT * FROM Items WHERE ItemId = @Id ORDER BY rowid DESC LIMIT 1", new { Id = _.id }));
            };

            Get["/info/{id}/add"] = _ =>
            {
                var weight = double.Parse(Request.Query.w);
                var humidity = double.Parse(Request.Query.h);
                var temperature = double.Parse(Request.Query.t);

                return _manager.Execute(@"INSERT INTO Items (ItemId, Weight, Humidity, Temperature) VALUES (@Id, @W, @H, @T)",
                                        new { Id = _.id, W = weight, H = humidity, T = temperature }).ToString();
            };

            Get["/adjust/{id}/humidity"] = _ =>
            {
                // TODO: adjust humidity of id
                var humidity = Request.Query.h;
                return $"adjusting... {_.id} to {humidity}";
            };

            // CANT DO NOW
            //Get["/adjust/{id}/temperature"] = _ =>
            //{
            //    // TODO: adjust temperature of id
            //};

            Get["/price/{id}"] = _ =>
            {
                var price = _manager.Query<Item>(@"SELECT Price FROM Items WHERE ItemId = @Id ORDER BY rowid DESC LIMIT 1",
                                            new { Id = _.id }).First().Price;
                return price == null ? "null" : price.ToString();
            };

            Get["/price/{id}/set"] = _ =>
            {
                var price = Request.Query.p;
                return _manager.Execute(@"UPDATE Items SET Price = @P WHERE rowid = (SELECT MAX(rowid) FROM Items WHERE ItemId = @Id)",
                                        new { P = price, Id = _.id }).ToString();
            };

            Get["/plan"] = _ =>
            {
                return "this is a stocking plan...";
            };

            #endregion
        }

        public static void Main(string[] args)
        {
            using (var host = new NancyHost(new Uri("http://0.0.0.0:5000")))
            {
                host.Start();
                Console.WriteLine("Running on http://0.0.0.0:5000");
                Console.ReadLine();
            }
        }
    }
}
