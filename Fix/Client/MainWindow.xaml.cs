namespace Client
{
	using System;
	using System.Linq;
	using System.Windows;
	using System.ComponentModel;
	using QuickFix;
	using Message = QuickFix.Message;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IApplication
	{
		private readonly IOmsDriver driver;

		public MainWindow()
		{
			InitializeComponent();

			//driver = new SwingTradeDriver();
			driver = new DayTradeDriver();

			driver.Connect(this);

			foreach (var sessionId in driver.GetSessions())
				sessionsCombo.Items.Add(sessionId);

			sessionsCombo.SelectedItem = sessionsCombo.Items.Cast<SessionID>().First();
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			driver.Disconnect();
		}

		private short Account
		{
			get { return Convert.ToInt16(accountField.Text); }
		}

		private decimal? Stop
		{
			get { return string.IsNullOrEmpty(stopField.Text) ? (decimal?) null : Convert.ToDecimal(stopField.Text); }
		}

		private decimal Price
		{
			get { return Convert.ToDecimal(priceField.Text); }
		}

		private int Quantity
		{
			get { return Convert.ToInt32(quantityField.Text); }
		}

		private void SendNewOrderBtn_Click(object sender, RoutedEventArgs e)
		{
			Action disp = () => driver.SendNewOrderSingle(symbolField.Text, Quantity, Price, Stop, Account, (SessionID) sessionsCombo.SelectedItem);

			Dispatcher.BeginInvoke(disp);
		}

		private void OrderCancelBtn_Click(object sender, RoutedEventArgs e)
		{
			Action disp = () => driver.SendOrderCancelRequest((SessionID) sessionsCombo.SelectedItem);

			Dispatcher.BeginInvoke(disp);
		}

		private void SendReplaceBtn_Click(object sender, RoutedEventArgs e)
		{
			Action disp = () => driver.SendOrderReplaceCancelRequest(symbolField.Text, Quantity, Price, Stop, Account, (SessionID) sessionsCombo.SelectedItem);

			Dispatcher.BeginInvoke(disp);
		}

		public void ToAdmin(Message message, SessionID sessionID)
		{
			Trace(message);
		}

		public void FromAdmin(Message message, SessionID sessionID)
		{
			Trace(message);
		}

		public void ToApp(Message message, SessionID sessionId)
		{
			Trace(message);
		}

		public void FromApp(Message message, SessionID sessionID)
		{
			Trace(message);
		}

		private void Trace(Message message)
		{
			var text = "\r\n" + string.Join(" | ", message.Select(f => f.Key + "=" + f.Value));

			Log(text);
		}

		public void OnCreate(SessionID sessionID)
		{
			var text = "\r\n" + sessionID + " created";

			Log(text);
		}

		public void OnLogout(SessionID sessionID)
		{
			var text = "\r\n" + sessionID + " log out";

			Log(text);
		}

		public void OnLogon(SessionID sessionID)
		{
			var text = "\r\n" + sessionID + " log on";

			Log(text);
		}

		private void Log(string text)
		{
			Action writer = () =>
				{
					MessageLogTxt.Text += text;
					scroller.ScrollToEnd();
				};

			Dispatcher.BeginInvoke(writer);
		}
	}
}
