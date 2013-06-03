namespace Client
{
	using QuickFix.Fields;

	public class DayTradeDriver : SwingTradeDriver
	{
		protected override int TargetStrategy
		{
			get { return 1002; }
		}

		protected override char TIF
		{
			get { return TimeInForce.DAY; }
		}
	}
}