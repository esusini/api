namespace Sample
{
	using System;
	using System.IO;
	using System.Net;
	using System.Text;

	public class XmlSwingTradeBuyDriver
	{
		private readonly Uri baseUrl;
		private string auth;

		public XmlSwingTradeBuyDriver(string baseUrl)
		{
			this.baseUrl = new Uri(baseUrl);
		}

		private HttpWebRequest CreateRequest(string relativeUri, string content = null, string method = "POST")
		{
			var request = WebRequest.CreateHttp(new Uri(baseUrl, relativeUri));

			request.Method = method;
			request.Accept = MediaTypes.Xml;
			request.ContentType = MediaTypes.Xml;

			if (!string.IsNullOrEmpty(auth))
				request.Headers.Add(HttpRequestHeader.Authorization, auth);

			if (!string.IsNullOrEmpty(content))
			{
				var body = request.GetRequestStream();

				var data = Encoding.UTF8.GetBytes(content);

				body.Write(data, 0, data.Length);
			}

			Console.WriteLine(request.RequestUri);

			return request;
		}

		public void Login()
		{
			var xml = @"<?xml version='1.0' encoding='utf-8' ?>
						<credentials>
							<cpf>289</cpf>
							<dob>2012-01-01</dob>
							<password>123456</password>
						</credentials>
						";

			var request = CreateRequest("signin/go", xml);

			using (var response = request.GetResponse())
			{
				auth = response.Headers[HttpResponseHeader.WwwAuthenticate];
			}

			Console.WriteLine("Authorization: " + auth);
		}

		public void LookupAvaiableBalanceAndMargin()
		{
			var request = CreateRequest("standardbuysell/Docket/BuyData/PETR4", method: "GET");

			using (var response = request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
					Console.WriteLine("Data: \r\n {0}", reader.ReadToEnd());
			}
		}

		public void SendOrder()
		{
			var xml = @"<?xml version='1.0' encoding='utf-8' ?>
						<envelope>
							<msg>
								<ExchangeOrderType>Limit</ExchangeOrderType>
								<ExpiresAt>2013-05-16</ExpiresAt>
								<Price>10.00</Price>
								<Quantity>100</Quantity>
								<Side>Buy</Side>
								<Symbol>PETR4</Symbol>
							</msg>
							<token>
								<Signature>ABC1234</Signature>
							</token>
						</envelope>";

			var request = CreateRequest("standardbuysell/docket/sendneworder", xml);

			using (var response = request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
					Console.WriteLine("Data: \r\n {0}", reader.ReadToEnd());
			}
		}

		public void CancelOrder()
		{
			
		}
	}
}