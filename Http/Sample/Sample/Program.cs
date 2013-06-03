namespace Sample
{
	using System;

	public class Program
	{
		static void Main(string[] args)
		{
			var buy = new JsonSwingTradeBuyDriver("http://localhost"); //XmlSwingTradeBuyDriver("http://localhost");

			Console.WriteLine("Signin in...");
			buy.Login();

			Console.WriteLine("Querying data...");
			buy.LookupAvaiableBalanceAndMargin();

			Console.WriteLine("Sending Order...");
			buy.SendOrder();

			Console.ReadKey();
		}
	}
}
