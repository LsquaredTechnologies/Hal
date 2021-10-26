//using System.Collections.Generic;
//using System.IO;
//using System.Text.Json;
//using System.Xml.Serialization;
//using Lsquared.Foundation.CheckAssertions;
//using Lsquared.Foundation.Net.Hal.Tests.Samples;
//using Xunit;
//using Xunit.Abstractions;
//using Xunit.Sdk;

//namespace Lsquared.Foundation.Net.Hal.Tests
//{
//    public sealed class XmlSerializationTests
//    {
//        private readonly ITestOutputHelper _output;

//        public XmlSerializationTests(ITestOutputHelper output) =>
//            _output = output;

//        [Fact]
//        public void Serialize_Resource_As_Xml()
//        {
//            var expectedXml = @"<resource rel=""self"" href=""/orders""><link rel=""next"" href=""/orders?page=2""/><link rel=""find"" href=""/orders{/id}"" templated=""true""/><resource rel=""order"" href=""/orders/123""><link rel=""basket"" href=""/baskets/98712""/><link rel=""customer"" href=""/customers/7809""/><total>30</total><currency>USD</currency><status>Shipped</status></resource><resource rel=""order"" href=""/orders/124""><link rel=""basket"" href=""/baskets/97213""/><link rel=""customer"" href=""/customers/12369""/><total>22.99</total><currency>EUR</currency><status>Processing</status></resource><shippedToday>20</shippedToday></resource>";

//            var orders = new Orders { ShippedToday = 20 };
//            orders.Add(new()
//            {
//                Id = 123,
//                Total = 30,
//                Currency = "USD",
//                Status = "Shipped",
//                Basket = new() { Id = 98712, NumberOfItems = 3 },
//                Customer = new() { Id = 7809, Name = "Dupont" }
//            });
//            orders.Add(new()
//            {
//                Id = 124,
//                Total = 22.99m,
//                Currency = "EUR",
//                Status = "Processing",
//                Basket = new() { Id = 97213, NumberOfItems = 1 },
//                Customer = new() { Id = 12369, Name = "Durand" }
//            });

//            XmlSerializer serializer = new(typeof(HalResource), new[] { typeof(Orders) });
//            StringWriter writer = new();
//            serializer.Serialize(writer, HalResource.Create(orders));
//            var actualXml = writer.GetStringBuilder().ToString();

//            _output.WriteLine(expectedXml);
//            _output.WriteLine(actualXml);

//            Expect.That(actualXml).IsEqualTo(expectedXml);
//        }
//    }
//    public sealed class JsonSerializationTests
//    {
//        private readonly ITestOutputHelper _output;

//        public JsonSerializationTests(ITestOutputHelper output) =>
//            _output = output;

//        [Fact]
//        public void Test1()
//        {
//            var expectedJson = @"{""_links"":{""self"":{""href"":""/orders""},""next"":{""href"":""/orders?page=2""},""find"":{""href"":""/orders{/id}"",""templated"":true}},""_embedded"":{""orders"":[{""_links"":{""self"":{""href"":""/orders/123""},""basket"":{""href"":""/baskets/98712""},""find"":{""customer"":""/customers/7809""}},""total"":30,""currency"":""USD"",""status"":""Shipped""},{""_links"":{""self"":{""href"":""/orders/124""},""basket"":{""href"":""/baskets/97213""},""find"":{""customer"":""/customers/12369""}},""total"":22.99,""currency"":""EUR"",""status"":""Processing""}]},""shippedToday"":20}";

//            var orders = new Orders { ShippedToday = 20 };
//            orders.Add(new()
//            {
//                Id = 123,
//                Total = 30,
//                Currency = "USD",
//                Status = "Shipped",
//                Basket = new() { Id = 98712, NumberOfItems = 3 },
//                Customer = new() { Id = 7809, Name = "Dupont" }
//            });
//            orders.Add(new()
//            {
//                Id = 124,
//                Total = 22.99m,
//                Currency = "EUR",
//                Status = "Processing",
//                Basket = new() { Id = 97213, NumberOfItems = 1 },
//                Customer = new() { Id = 12369, Name = "Durand" }
//            });


//            var actualJson = JsonSerializer.Serialize(orders);

//            _output.WriteLine(expectedJson);
//            _output.WriteLine(actualJson);


//            Expect.That(actualJson).IsEqualTo(expectedJson);
//        }
//    }
//}
