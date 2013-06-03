namespace Sample
{
	using System;
	using System.IO;
	using System.Net;
	using System.Text;

	public class JsonSwingTradeBuyDriver
	{
		const string DateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.fffK";

		private readonly Uri baseUrl;
		private string auth;

		public JsonSwingTradeBuyDriver(string baseUrl)
		{
			this.baseUrl = new Uri(baseUrl);
		}

		private HttpWebRequest CreateRequest(string relativeUri, string content = null, string method = "POST")
		{
			var request = WebRequest.CreateHttp(new Uri(baseUrl, relativeUri));

			request.Method = method;
			request.Accept = MediaTypes.JSon;
			request.ContentType = MediaTypes.JSon;

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
			var dob = new DateTime(1900, 1, 1).ToString(DateFormat);

			var json = @"{credentials: {cpf: 294, dob: '"+ dob + "', password:123456}}";

			var request = CreateRequest("signin/go", json);

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
			var dob = DateTime.Today.ToString(DateFormat);

			var json = "{" +
			           "msg:{ExchangeOrderType: 'Limit', ExpiresAt: '"+ dob +"', Price:10.00, Quantity: 100, Side: 'Buy', Symbol: 'PETR4'}, " +
			           "token:{Signature:'ABC1234'}" +
			           "}";

			var request = CreateRequest("standardbuysell/docket/sendneworder", json);

			using (var response = request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
					Console.WriteLine("Data: \r\n {0}", reader.ReadToEnd());
			}
		}
	}
}