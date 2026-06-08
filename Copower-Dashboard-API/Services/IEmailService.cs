using Azure.Core;
using Azure.Identity;
using Copower_API.Context;
using Copower_API.Helpers;
using Copower_API.Models.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;
using System.Text.RegularExpressions;

namespace Copower_API.Services
{
    /// <summary>
    /// Email service interface
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Prepares the necessary data and context for sending an email based on the specified request, user, topic,
        /// and language.
        /// </summary>
        /// <param name="reqid">The unique identifier of the request for which the email is being prepared. Cannot be null or empty.</param>
        /// <param name="userId">The identifier of the user who will receive the email. Cannot be null or empty.</param>
        /// <param name="topic">The topic or template name of the email to be prepared. Determines the content and structure of the email.
        /// Cannot be null or empty.</param>
        /// <param name="language">The language code (such as "en" or "fr") to use for the email content. Cannot be null or empty.</param>
        /// <param name="data">A dictionary containing key-value pairs of data to be merged into the email template. Keys represent
        /// template placeholders; values provide the corresponding content. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result is <see langword="true"/> if the email
        /// preparation was successful; otherwise, <see langword="false"/>.</returns>
        Task<bool> PrepareSendEmail(String reqid, Guid userId, String topic, String language, Dictionary<string, string> data);
    }

    /// <summary>
    /// Email service
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly AppSettings _appSettings;
        private readonly CommonContext _commonContext;
        private readonly EmailConfigurationModel _emailConfig;
        private readonly IGeneralService _generalService;
        private readonly IUtilsService _utilsService;

        private static readonly string[] scopes = ["https://graph.microsoft.com/.default"];
        private static readonly ClientSecretCredential? _clientSecretCredential;
        private static GraphServiceClient? _appClient;

        /// <summary>
        /// Get app-only token for Microsoft Graph
        /// </summary>
        /// <returns></returns>
        /// <exception cref="System.NullReferenceException"></exception>
        public static async Task<string> GetAppOnlyTokenAsync()
        {
            // Ensure credential isn't null
            _ = _clientSecretCredential ?? throw new System.NullReferenceException("Graph has not been initialized for app-only auth");

            // Request token with given scopes
            var context = new TokenRequestContext(["https://graph.microsoft.com/.default"]);
            var response = await _clientSecretCredential.GetTokenAsync(context);
            return response.Token;
        }

        /// <summary>
        /// Email service
        /// </summary>
        /// <param name="appSettings">App settings</param>
        /// <param name="config">Server configuration</param>
        /// <param name="commonContext">Common context</param>
        /// <param name="generalService">General service</param>
        /// <param name="utilsService">Utils service</param>
        public EmailService(IConfiguration config, CommonContext commonContext, IGeneralService generalService, IUtilsService utilsService)
        {
            _appSettings = config.GetSection("AppSettings").Get<AppSettings>() ?? throw new Exception("AppSettings failure");
            _commonContext = commonContext;
            _generalService = generalService;
            _utilsService = utilsService;

            var emailConfig = config.GetSection("Email").Get<EmailConfigurationModel>();
            if (emailConfig != null)
            {
                if (emailConfig.Active == true)
                {
                    _emailConfig = emailConfig;

                    var clientSecretCredential = new ClientSecretCredential(emailConfig.TenantId, emailConfig.ClientId, emailConfig.ClientSecret);

                    _appClient = new GraphServiceClient(clientSecretCredential, scopes);
                }
                else
                {
                    _emailConfig = new EmailConfigurationModel
                    {
                        Active = false,
                        ClientId = string.Empty,
                        ClientSecret = string.Empty,
                        FromEmail = string.Empty,
                        FromName = string.Empty,
                        TenantId = string.Empty
                    };
                    _appClient = null!; // Initialize to avoid CS8618
                    Console.WriteLine("Email service is not active, email sending is disabled, to enable it, set the Email:Active configuration to true");
                }
            }
            else
            {
                _emailConfig = new EmailConfigurationModel
                {
                    Active = false,
                    ClientId = string.Empty,
                    ClientSecret = string.Empty,
                    FromEmail = string.Empty,
                    FromName = string.Empty,
                    TenantId = string.Empty
                };
                _appClient = null!; // Initialize to avoid CS8618
                _generalService.StopServer("Email service initialization failed, email config not found");
            }
        }

        /// <inheritdoc/>
        public async Task<bool> PrepareSendEmail(String reqid, Guid userId, String topic, String language, Dictionary<string, string> data)
        {
            if (_emailConfig.Active == false) // Email service is not active, skip sending email
            {
                Console.WriteLine("Email service is not active, not sending email");
                return true;
            }

            var text = await _commonContext.Locale.FirstOrDefaultAsync(l => l.Key == topic && l.Language == language && l.Type == "email") ?? throw new Exception("528340");
            String textMessage = Regex.Unescape(text.Message);

            data["[appname]"] = _appSettings.AppName;
            foreach (var tobj in data)
            {
                if (textMessage.Contains(tobj.Key) == true)
                    textMessage = textMessage.Replace(tobj.Key, tobj.Value);
            }

            return await SendEmail(reqid, userId, text.Topic, textMessage);
        }

        /// <summary>
        /// Send an email
        /// </summary>
        /// <param name="reqid">Request id</param>
        /// <param name="userId">User id</param>
        /// <param name="topic">Topic of the email</param>
        /// <param name="message">Content of the email</param>
        /// <returns></returns>
        internal async Task<bool> SendEmail(string reqid, Guid userId, string topic, string message)
        {
            try
            {
                if (_utilsService.CheckUUID(userId) == false)
                    return false;

                var user = await _commonContext.User.FirstOrDefaultAsync(u => u.Id == userId && u.Deleted == null);
                if (user == null)
                    return false;

                _generalService.WriteLogMessage("email", reqid, "Email.SendEmail", "Sending email > " + topic + " to " + user.Id);

                var clientSecretCredential = new ClientSecretCredential(_emailConfig.TenantId, _emailConfig.ClientId, _emailConfig.ClientSecret);

                var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

                var body = new SendMailPostRequestBody
                {
                    Message = new Message
                    {
                        Subject = topic,
                        Body = new ItemBody
                        {
                            ContentType = BodyType.Html,
                            Content = $@"<html><body>{message}</body></html>"
                        },
                        ToRecipients =
                        [
                            new() {
                            EmailAddress = new EmailAddress
                            {
                                Address = user.Email
                            }
                        }
                        ]
                    }
                };

                if (_appClient != null)
                {
                    await _appClient.Users[_emailConfig.FromEmail].SendMail.PostAsync(body);
                    return true;
                }
                else { return false; }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
