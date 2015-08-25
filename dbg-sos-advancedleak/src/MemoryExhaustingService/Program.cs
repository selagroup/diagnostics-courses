using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;
using System.Xml;

namespace MemoryExhaustingService
{
    public class Product
    {
        public string Name { get; set; }
        public int Stock { get; set; }
        public byte[] Image { get; set; }
    }

    public static class InventoryDAL
    {
        public static Product[] GetAllProducts()
        {
            return
                Enumerable.Range(0, 10000)
                .Select(i => new Product { Name = "Product" + i, Stock = i, Image = new byte[1000] })
                .ToArray();
        }

        public static Product[] GetMyProducts()
        {
            Thread.Sleep(2000);

            return new Product[0];
        }
    }

    [ServiceContract]
    public interface IInventoryService
    {
        [OperationContract]
        Product[] GetAllProducts();

        [OperationContract]
        Product[] GetMyProducts();
    }

    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Single,
        InstanceContextMode = InstanceContextMode.Single)]
    public class InventoryService : IInventoryService
    {
        public Product[] GetAllProducts()
        {
            return InventoryDAL.GetAllProducts();
        }

        public Product[] GetMyProducts()
        {
            return InventoryDAL.GetMyProducts();
        }
    }

    public static class HeartbeatManager
    {
        private static Timer _timer;
        private static IInventoryService _inventoryService;

        public static void Initialize(IInventoryService inventoryService)
        {
            _timer = new Timer(OnTimer, null, 0, 1000);
            _inventoryService = inventoryService;
        }

        private static void OnTimer(object dummy)
        {
            Product[] allProducts = _inventoryService.GetAllProducts();
            Product[] myProducts = _inventoryService.GetMyProducts();
            foreach (Product product in allProducts)
            {
                if (myProducts.Contains(product) && product.Stock <= 0)
                {
                    Console.WriteLine("A product I need is not in stock: " + product.Name);
                }
            }
            Console.WriteLine("Heartbeat done.");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.ReaderQuotas = XmlDictionaryReaderQuotas.Max;

            ServiceHost host = new ServiceHost(typeof(InventoryService));
            host.AddServiceEndpoint(
                typeof(IInventoryService),
                binding,
                "net.tcp://localhost:9090/InventoryService");
            host.Open();

            IInventoryService proxy = ChannelFactory<IInventoryService>.CreateChannel(
                binding,
                new EndpointAddress("net.tcp://localhost:9090/InventoryService"));

            HeartbeatManager.Initialize(proxy);
            Console.ReadLine();
        }
    }
}
