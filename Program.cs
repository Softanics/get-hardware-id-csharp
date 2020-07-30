using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using System.Security.Cryptography;

namespace GetHardwareId
{
    class Program
    {
        static void Main(string[] args)
        {
            // Display properties of HDD, CPU, and motherboard

            foreach (var managementClassName in new[] { "Win32_DiskDrive", "Win32_Processor", "Win32_BaseBoard" })
            { 
                var managementClass = new ManagementClass(managementClassName);
                var managementObject = managementClass.GetInstances().Cast<ManagementBaseObject>().First();

                Console.WriteLine(string.Format($"Management class name: {managementClassName}"));
                Console.WriteLine(string.Join(Environment.NewLine,
                    managementObject.Properties.Cast<PropertyData>().Select(x => string.Format($"Name: {x.Name}, value: {x.Value}"))));
            }

            // Hash the values

            var hash = (new SHA256Managed()).ComputeHash(Encoding.UTF8.GetBytes(string.Join("", GetHardwareProperties())));
            var hardwareId = string.Join("", hash.Select(x => x.ToString("X2")));

            Console.WriteLine($"Hardware id: {hardwareId}");
        }

        [ArmDot.Client.ObfuscateControlFlow]
        private static IEnumerable<string> GetHardwareProperties()
        {
            foreach (var properties in new Dictionary<string, string[]>
            {
                { "Win32_DiskDrive", new[] { "Model", "Manufacturer", "Signature", "TotalHeads" } },
                { "Win32_Processor", new[] { "UniqueId", "ProcessorId", "Name", "Manufacturer" } },
                { "Win32_BaseBoard", new[] { "Model", "Manufacturer", "Name", "SerialNumber" } }
            })
            {
                var managementClass = new ManagementClass(properties.Key);
                var managementObject = managementClass.GetInstances().Cast<ManagementBaseObject>().First();

                foreach (var prop in properties.Value)
                {
                    if (null != managementObject[prop])
                        yield return managementObject[prop].ToString();
                }
            }
        }
    }
}
