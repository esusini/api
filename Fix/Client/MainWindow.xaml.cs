namespace Client
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Windows;
	using System.ComponentModel;
	using System.Windows.Controls;
	using QuickFix;
	using Message = QuickFix.Message;

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, IApplication
	{
		private readonly Dictionary<string, IOmsDriver> drivers = new Dictionary<string, IOmsDriver>();

		private IOmsDriver currentDriver;

		public MainWindow()
		{
			InitializeComponent();

			drivers.Add("SwingTrade", new SwingTradeDriver());
			drivers.Add("DayTrade", new DayTradeDriver());

			SetDriver(strategyCombo.SelectedItem);
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			currentDriver.Disconnect();
		}

		private short Account
		{
			get { return Convert.ToInt16(accountField.Text); }
		}

		private decimal? Stop
		{
			get { return string.IsNullOrEmpty(stopField.Text) ? (decimal?) null : Convert.ToDecimal(stopField.Text); }
		}

		private decimal? Price
		{
			get { return string.IsNullOrEmpty(priceField.Text) ? (decimal?) null : Convert.ToDecimal(priceField.Text); }
		}

		private decimal? Gain
		{
			get { return string.IsNullOrEmpty(gainField.Text) ? (decimal?) null : Convert.ToDecimal(gainField.Text); }
		}

		private int Quantity
		{
			get { return Convert.ToInt32(quantityField.Text); }
		}

		private void SendNewOrderBtn_Click(object sender, RoutedEventArgs e)
		{
			Action disp = () => currentDriver.SendNewOrderSingle(symbolField.Text, Quantity, Price, Stop, Gain, Account, (SessionID) sessionsCombo.SelectedItem);

			Dispatcher.BeginInvoke(disp);
		}

		private void OrderCancelBtn_Click(object sender, RoutedEventArgs e)
		{
			Action disp = () => currentDriver.SendOrderCancelRequest((SessionID) sessionsCombo.SelectedItem);

			Dispatcher.BeginInvoke(disp);
		}

		private void SendReplaceBtn_Click(object sender, RoutedEventArgs e)
		{
			Action disp = () => currentDriver.SendOrderReplaceCancelRequest(symbolField.Text, Quantity, Price, Stop, Gain, Account, (SessionID) sessionsCombo.SelectedItem);

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

		private void strategyCombo_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			SetDriver(e.AddedItems[0]);
		}

		private void SetDriver(object selectedItem)
		{
			var item = selectedItem as ComboBoxItem;

			if (item == null || item.Content == null) return;

			if (currentDriver != null)
				currentDriver.Disconnect();

			currentDriver = drivers[item.Content.ToString()];

			currentDriver.Connect(this);

			sessionsCombo.Items.Clear();

			foreach (var sessionId in currentDriver.GetSessions())
				sessionsCombo.Items.Add(sessionId);

			sessionsCombo.SelectedItem = sessionsCombo.Items.Cast<SessionID>().First();
		}
	}
}
