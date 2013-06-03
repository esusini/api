namespace Client
{
	using System;

	public static class FixExtensions
	{
		public static string AsLocalMktDate(this DateTime date)
		{
			return date.ToString("yyyyMMdd");
		}
	}
}