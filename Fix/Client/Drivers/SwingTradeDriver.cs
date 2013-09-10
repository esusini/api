namespace Client
{
	using System;
	using System.Collections;
	using QuickFix;
	using QuickFix.FIX44;
	using QuickFix.Fields;
	using QuickFix.Transport;

	public class SwingTradeDriver : IOmsDriver
	{
		protected virtual int TargetStrategy
		{
			get { return 1001; }
		}

		protected virtual char TIF
		{
			get { return TimeInForce.GOOD_TILL_DATE; }
		}

		private SocketInitiator initiator;

		private string clOrdId;
		private string origClOrdID;
		private string lastSymbol;
		private string lastAccount;

		public void Connect(IApplication application)
		{
			var settings = new SessionSettings("session.config");
			var myApp = application;
			var storeFactory = new FileStoreFactory(settings);
			var logFactory = new FileLogFactory(settings);

			initiator = new SocketInitiator(myApp, storeFactory, settings, logFactory);

			initiator.Start();
		}

		public void SendNewOrderSingle(string symbol, int quantity, decimal? price, decimal? stop, decimal? gain, string account, SessionID session)
		{
			clOrdId = DateTime.Now.Ticks.ToString();
			lastSymbol = symbol;
			lastAccount = account;

			var newOrderSingle = new NewOrderSingle(new ClOrdID(clOrdId),
			                                        new Symbol(symbol),
			                                        new Side(Side.BUY),
			                                        new TransactTime(DateTime.Now),
			                                        new OrdType(gain.HasValue ? 'X' : stop.HasValue ? OrdType.STOP : price.HasValue ? OrdType.LIMIT : OrdType.MARKET))
				{
					Account = new Account(account),
					OrderQty = new OrderQty(quantity),
					TargetStrategy = new TargetStrategy(TargetStrategy),
					TimeInForce = new TimeInForce(TIF),
				};

			if (TIF == TimeInForce.GOOD_TILL_DATE)
				newOrderSingle.ExpireDate = new ExpireDate(DateTime.Today.AddDays(1).AsLocalMktDate());

			if (stop.HasValue)
				newOrderSingle.StopPx = new StopPx(stop.Value);

			if (price.HasValue)
				newOrderSingle.Price = new Price(price.Value);

			if (gain.HasValue)
				newOrderSingle.SetField(new DecimalField(6001, gain.Value));

			Session.SendToTarget(newOrderSingle, session);
		}

		public void SendOrderCancelRequest(SessionID session)
		{
			origClOrdID = clOrdId;
			clOrdId = DateTime.Now.Ticks.ToString();

			var cancel = new OrderCancelRequest(new OrigClOrdID(origClOrdID),
			                                    new ClOrdID(clOrdId),
			                                    new Symbol(lastSymbol),
			                                    new Side(Side.BUY),
			                                    new TransactTime(DateTime.Now))
				{
					Account = new Account(lastAccount.ToString())
				};

			Session.SendToTarget(cancel, session);
		}

		public void SendOrderReplaceCancelRequest(string symbol, int quantity, decimal? price, decimal? stop, decimal? gain, string account, SessionID session)
		{
			origClOrdID = clOrdId;
			clOrdId = DateTime.Now.Ticks.ToString();

			var replace = new OrderCancelReplaceRequest(new OrigClOrdID(origClOrdID),
			                                            new ClOrdID(clOrdId),
			                                            new Symbol(lastSymbol),
			                                            new Side(Side.BUY),
			                                            new TransactTime(DateTime.Now),
														new OrdType(gain.HasValue ? 'X' : stop.HasValue ? OrdType.STOP : price.HasValue ? OrdType.LIMIT : OrdType.MARKET))
				{
					Account = new Account(lastAccount.ToString()),
					OrderQty = new OrderQty(quantity),
				};

			if (stop.HasValue)
				replace.StopPx = new StopPx(stop.Value);

			if (price.HasValue)
				replace.Price = new Price(price.Value);

			if (gain.HasValue)
				replace.SetField(new DecimalField(6001, gain.Value));

			Session.SendToTarget(replace, session);
		}

		public void Disconnect()
		{
			initiator.Stop(true);
		}

		public IEnumerable GetSessions()
		{
			return initiator.GetSessionIDs();
		}
	}
}