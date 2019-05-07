using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using OkonkwoOandaV20.TradeLibrary.Account;
using OkonkwoOandaV20.TradeLibrary.Position;

using QuantConnect.Api;

using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;

using static OkonkwoOandaV20.TradeLibrary.REST.Rest20;

namespace QuantConnectTelegramBot
{

	/// <summary>
	/// Contains functionality for running a Telegram bot to communicate with the QuantConnect API.
	/// </summary>
	public class Bot
	{

		/// <summary>
		/// Gets or sets the Telegram Bot API token.
		/// </summary>
		private static string _telegramBotApiToken;

		/// <summary>
		/// Gets or sets the QuantConnect job user ID.
		/// </summary>
		private static int _qcJobUserId;

		/// <summary>
		/// Gets or sets the QuantConnect API access token.
		/// </summary>
		private static string _qcApiAccessToken;

		/// <summary>
		/// Gets or sets the QuantConnect project ID for the algorithm.
		/// </summary>
		private static int _qcProjectId;

		/// <summary>
		/// Gets or sets the QuantConnect deployment ID for the algorithm.
		/// </summary>
		private static string _qcDeploymentId;

		/// <summary>
		/// Gets or sets the Oanda API token.
		/// </summary>
		private static string _oandaApiToken;

		/// <summary>
		/// Gets or sets the Oanda account ID.
		/// </summary>
		private static string _oandaAccountId;

		/// <summary>
		/// Gets or sets the Oanda account mode (trade or practice).
		/// </summary>
		private static EEnvironment _oandaAccountMode;

		/// <summary>
		/// Gets or sets the list of authenticated Telegram usernames.
		/// </summary>
		private static List<string> _authedUsers;

		/// <summary>
		/// Gets or sets the QuantConnect API connection.
		/// </summary>
		private static Api _qcApi;

		/// <summary>
		/// Gets or sets the Telegram Bot client.
		/// </summary>
		private static ITelegramBotClient _botClient;

		/// <summary>
		/// Gets the culture info for the QuantConnect response.
		/// </summary>
		private static readonly CultureInfo _qcCI = new CultureInfo( "en-US" );

		/// <summary>
		/// Gets the culture info for the Oanda response.
		/// </summary>
		private static readonly CultureInfo _oandaCI = new CultureInfo( "en-GB" );

		/// <summary>
		/// Main erntry point.
		/// </summary>
		/// <param name="args"></param>
		public static void Main( string[] args )
		{

			// Load the configuration
			ReloadConfiguration();

			// Connect to the Telegram Bot API
			Log( "Connecting to the Telegram Bot API." );
			_botClient = new TelegramBotClient( _telegramBotApiToken );
			try {
				var me = _botClient.GetMeAsync().Result;
				Log( $"Telegram Bot API connection successful. Connected with user ID {me.Id} and name {me.FirstName}." );
			} catch ( Exception e ) {
				Log( $"Telegram Bot API connection unsuccessful. Exception: {e.Message}" );
				throw e;
			}

			// Connect to QuantConnect API
			Log( "Connecting to QuantConnect API." );
			_qcApi = new Api();
			_qcApi.Initialize( _qcJobUserId, _qcApiAccessToken, "notused" );

			// Check for successful connection
			if ( !_qcApi.Connected )
				throw new Exception( "QuantConnect API connection unsuccessful." );
			else
				Log( "QuantConnect API connection successful." );

			// Connect to Oanda API if token provided
			if ( _oandaApiToken != "" ) {
				try {
					Log( "Connecting to Oanda API." );
					Credentials.SetCredentials( _oandaAccountMode, _oandaApiToken, _oandaAccountId );
					Log( "Oanda API connection successful." );
				} catch ( Exception e ) {
					Log( $"Oanda API connection unsuccessful. Exception: {e.Message}" );
					throw e;
				}
			}

			// Bind message handler
			_botClient.OnMessage += MessageHandler;

			// Start receiving
			_botClient.StartReceiving();

			// Sleep forever
			Thread.Sleep( Timeout.Infinite );
		}

		/// <summary>
		/// Fires on new message received.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		static async void MessageHandler( object sender, MessageEventArgs e )
		{
			try {
				Log( $"Message received from {e.Message.From.Username}: {e.Message.Text}. Checking if authenticated..." );

				if ( e.Message.Text != null && _authedUsers.Contains( e.Message.From.Username ) ) {

					// Switch around the first command word
					switch ( e.Message.Text.Split( ' ' ).First().ToLower() ) {

						// Pings the bot
						case "/start":
							await _botClient.SendTextMessageAsync(
								chatId: e.Message.Chat,
								text: $"Welcome! Bot is connected to QuantConnect account `{_qcJobUserId}`" + ( _oandaApiToken != "" ? $" and Oanda {_oandaAccountMode.ToString().ToLower()} account `{_oandaAccountId}`." : "." ),
								parseMode: ParseMode.Markdown
							);
							break;

						// Reloads list of authenticated users
						case "/reloadusers":
							ReloadConfiguration();
							await _botClient.SendTextMessageAsync(
								chatId: e.Message.Chat,
								text: $"List of authenticated users has been reloaded."
							);
							break;

						// Get Oanda data
						case "/get_oanda":
							await _botClient.SendTextMessageAsync(
								chatId: e.Message.Chat,
								text: GetAccountData( "oanda-summary" ) + "\n_Current positions_\n" + GetAccountData( "oanda-positions" ),
								parseMode: ParseMode.Markdown
							);
							break;

						case "/get_qc":
							await _botClient.SendTextMessageAsync(
								chatId: e.Message.Chat,
								text: GetAccountData( "qc-nav" ),
								parseMode: ParseMode.Markdown
							);
							break;

						default:
							await _botClient.SendTextMessageAsync(
								chatId: e.Message.Chat,
								text: "No suitable command provided."
							);
							break;
					}

					Log( $"Message sent to {e.Message.From.Username}." );
				}
			} catch ( Exception ex ) {
				Log( $"Unable to reply to incoming message. Exception {ex.Message}" );
			}
		}

		/// <summary>
		/// Returns desired data from the account. Simplifies the access.
		/// </summary>
		/// <param name="desiredData"></param>
		/// <returns></returns>
		static string GetAccountData( string desiredData )
		{

			try {
				switch ( desiredData ) {

					// Summary of Oanda account
					case "oanda-summary":
						// Get the account summary
						AccountSummary accountSummary = GetAccountSummaryAsync( _oandaAccountId ).Result;

						return $"*Oanda account summary*\n" +
							$"NAV: {accountSummary.NAV.ToString( "C2", _oandaCI )}\n" +
							$"Unrealised P&L: {accountSummary.unrealizedPL.ToString( "C2", _oandaCI )}\n" +
							$"Realised P&L: {accountSummary.pl.ToString( "C2", _oandaCI )}\n" +
							$"Margin call %: {accountSummary.marginCloseoutPercent.ToString( "P", _oandaCI )}\n" +
							$"";

					case "oanda-positions":
						// Get the list of positions
						List<Position> positions = GetOpenPositionsAsync( _oandaAccountId ).Result;

						var positionsString = "";

						foreach ( var position in positions ) {

							string direction;
							long units;
							decimal unrealisedPnl;

							// Get direction
							if ( position.@long.units != 0 ) {
								direction = "Long";
								units = position.@long.units;
								unrealisedPnl = position.@long.unrealizedPL;
							} else {
								direction = "Short";
								units = position.@short.units;
								unrealisedPnl = position.@short.unrealizedPL;
							}
							positionsString += $"*{position.instrument}*\n" +
								$"Unrealised P&L: {unrealisedPnl.ToString( "C2", _oandaCI )}\n" +
								$"Direction: {direction}\n" +
								$"# Units: {units.ToString( "N0", _oandaCI )}\n\n";
						}

						return positionsString;

					// NAV/Equity in USD
					case "qc-nav":
						// Read the algorithm results
						var liveResults = _qcApi.ReadLiveAlgorithm( _qcProjectId, _qcDeploymentId ).LiveResults.Results;

						return $"*QuantConnect NAV*\n{liveResults.Charts["Strategy Equity"].Series["Equity"].Values.Last().y.ToString( "C2", _qcCI )}";

					default:
						return $"`{desiredData}` is not a recognised account data type.\n\nAvailable commands: `/get oanda-summary`, `/get oanda-positions`, `/get qc-nav`";
				}

			} catch ( Exception e ) {
				Log( $"Could not get account data '{desiredData}'. Exception: {e.Message}" );
			}

			return "";
		}

		/// <summary>
		/// Loads and overwrites our configuration parameters.
		/// </summary>
		public static void ReloadConfiguration()
		{

			Log( "Loading configuration." );

			string configFileContents;

			try {
				configFileContents = File.ReadAllText( "bot-config.json" );
			} catch ( Exception e ) {
				Log( $"Could not load the configuration file. Exception: {e.Message}" );
				throw e;
			}

			try {

				// Set configuration parameters
				var config = JsonConvert.DeserializeObject<JObject>( configFileContents );

				// Telegram
				_telegramBotApiToken = config["telegram-bot-api-token"].ToString();

				// QuantConnect
				_qcJobUserId = config["quantconnect-job-user-id"].ToObject<int>();
				_qcApiAccessToken = config["quantconnect-api-access-token"].ToString();
				_qcProjectId = config["quantconnect-project-id"].ToObject<int>();
				_qcDeploymentId = config["quantconnect-deployment-id"].ToString();

				// Oanda
				_oandaApiToken = config["oanda-api-token"].ToString();
				_oandaAccountId = config["oanda-account-id"].ToString();
				_oandaAccountMode = config["oanda-account-mode"].ToString() == "trade" ?
					EEnvironment.Trade :
						( config["oanda-account-mode"].ToString() == "practice" ?
							EEnvironment.Practice :
							throw new Exception( "Invalid Oanda account mode." )
					);

				_authedUsers = config["authed-users"].ToObject<List<string>>();

			} catch ( Exception e ) {
				Log( $"Could not extract required configuration parameters. Please check the config file is not malformed. Exception: {e.Message}" );
			}

			Log( "Configuration (re)loaded successfully." );
		}

		/// <summary>
		/// Logs text to the console.
		/// </summary>
		/// <param name="msg">Message to log.</param>
		public static void Log( string msg )
		{
			Console.WriteLine( $"[{DateTime.Now.ToString( "o" )}] {msg}" );
		}
	}
}