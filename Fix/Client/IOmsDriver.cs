namespace Client
{
	using System.Collections;
	using QuickFix;

	public interface IOmsDriver
	{
		void Connect(IApplication application);

		void SendNewOrderSingle(string symbol, int quantity, decimal? price, decimal? stop, decimal? gain, string account, SessionID session);
		
		void SendOrderCancelRequest(SessionID session);

		void SendOrderReplaceCancelRequest(string symbol, int quantity, decimal? price, decimal? stop, decimal? gain, string account, SessionID session);
		
		void Disconnect();
		
		IEnumerable GetSessions();
	}
}