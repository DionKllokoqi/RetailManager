using RMDesktopUI.Library.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RMDesktopUI.Library.Api
{
    public class ApiHelper : IApiHelper
    {
        private HttpClient _apiClient;
        private ILoggedInUserModel _loggedInUser;

        public HttpClient ApiClient
        {
            get
            {
                return _apiClient;
            }
        }

        public ApiHelper(ILoggedInUserModel loggedInUser)
        {
            InitializeClient();
            _loggedInUser = loggedInUser;
        }

        /// <summary>
        /// Method is used to initialize a new HTTP Client, which is used for the lifetime of the application.
        /// </summary>
        private void InitializeClient()
        {
            string api = ConfigurationManager.AppSettings["api"];

            _apiClient = new HttpClient();
            _apiClient.BaseAddress = new Uri(api); // Base address is read from config file. This address can be changed during runtime
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Method is used to create the FormURLEncoded Content and post to the Token.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns>Authenticated user with a valid token.</returns>
        public async Task<AuthenticatedUser> Authenticate(string username, string password)
        {
            // Data that we will send to the API Endpoint
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
            });

            using (HttpResponseMessage response = await _apiClient.PostAsync("/Token", data)) // Use only relative path + address in config file
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<AuthenticatedUser>();
                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public async Task GetLoggedInUserInfo(string token)
        {
            _apiClient.DefaultRequestHeaders.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Clear();
            _apiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _apiClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}"); // for every request we make via this method, we want to add the authorization and bearer token in the http header

            using (HttpResponseMessage response = await _apiClient.GetAsync("/api/User"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<LoggedInUserModel>();
                    _loggedInUser.CreatedDate = result.CreatedDate;
                    _loggedInUser.EmailAddress = result.EmailAddress;
                    _loggedInUser.FirstName = result.FirstName;
                    _loggedInUser.LastName = result.LastName;
                    _loggedInUser.Id = result.Id;
                    _loggedInUser.Token = token;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}
