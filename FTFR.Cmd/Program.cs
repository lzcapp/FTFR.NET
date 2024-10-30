using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MiHome.Net.Dto;
using MiHome.Net.Middleware;
using MiHome.Net.Service;
using System.Configuration;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace FTFR.Cmd {
    internal class Program {
        static void Main(string[] args) {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var username = ConfigurationManager.AppSettings["Username"];
            var password = ConfigurationManager.AppSettings["Password"];
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) {
                return;
            }

            var hostBuilder = Host.CreateDefaultBuilder();
            hostBuilder.ConfigureServices(it => it.AddMiHomeDriver(x =>
            {
                x.UserName = username;
                x.Password = password;
            }));
            var host = hostBuilder.Build();

            var miHomeDriver = host.Services.GetService<IMiHomeDriver>();
            if (miHomeDriver == null) {
                return;
            }
            var deviceList = miHomeDriver.Cloud.GetDeviceListAsync().Result;
            
            // Temperature

            var temp = string.Empty;

            var temperatureDeviceInfo = deviceList.FirstOrDefault(it => string.Equals(it.Did, "blt.3.1jbhp2t80kg00", StringComparison.OrdinalIgnoreCase));
            if (temperatureDeviceInfo != null) {
                var tempSpec = miHomeDriver.Cloud.GetDeviceSpec(temperatureDeviceInfo.Model).Result;
                var unit = tempSpec.Services[2].Properties[0].Unit;
                var unitText = string.Empty;
                if (unit.Equals("celsius", StringComparison.OrdinalIgnoreCase)) {
                    unitText = "\u2103";
                } else if (unit.Equals("fahrenheit", StringComparison.OrdinalIgnoreCase)) {
                    unitText = "\u00b0F";
                }

                var tempResult = miHomeDriver.Cloud.GetPropertiesAsync([
                    new GetPropertyDto() {
                        Did = temperatureDeviceInfo.Did,
                        Siid = 3,
                        Piid = 1001
                    }
                ]).Result;
                temp = tempResult[0].Value + unitText;
            }

            // Power Strip

            var powerStatus = new List<bool>();

            var powerDeviceInfo = deviceList.FirstOrDefault(it => string.Equals(it.Did, "768966868", StringComparison.OrdinalIgnoreCase));
            if (powerDeviceInfo != null) {
                var tempSpec = miHomeDriver.Cloud.GetDeviceSpec(powerDeviceInfo.Model).Result;
                for (var i = 7; i >= 3; i--) {
                    var powerResult = miHomeDriver.Cloud.GetPropertiesAsync([
                        new GetPropertyDto() {
                            Did = powerDeviceInfo.Did,
                            Siid = i,
                            Piid = 1
                        }
                    ]).Result;
                    powerStatus.Add(bool.Parse(powerResult[0].Value.ToString() ?? "false"));
                }
            }

            Console.Clear();

            Console.WriteLine("温度：");
            Console.WriteLine("- 水温：\t" + temp);
            Console.WriteLine();
            Console.WriteLine("排插：");
            for (var i = 0; i < powerStatus.Count; i++) {
                var status = StatusBooleanToString(powerStatus[i]);
                if (i == 0) {
                    Console.WriteLine("- USB插座：\t" + status);
                } else {
                    Console.WriteLine("- 插座" + i + "：\t" + status);
                }
            }
        }

        private static string StatusBooleanToString(bool status) {
            return status ? "已开启" : "已关闭";
        }
    }
}
