﻿using Atlassian;
using CF.RESTClientDotNet;
using groupkt;
using System;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using restclientdotnet = CF.RESTClientDotNet;

namespace CF.RESTClient.NET.Sample
{

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage
    {
        #region Fields
        private restclientdotnet.RESTClient _BitbucketClient;
        #endregion

        #region Constructror
        public MainPage()
        {
            InitializeComponent();
            ChangeDescriptionButton.Clicked += ChangeDescriptionButton_Clicked;
            CurrentPageChanged += MainPage_CurrentPageChanged;
            GetReposButton.Clicked += GetReposButton_Clicked;
            ReposBox.ItemSelected += ReposBox_ItemSelected;
        }

        private void GetReposButton_Clicked(object sender, EventArgs e)
        {
            GetReposClick();
        }

        private void MainPage_CurrentPageChanged(object sender, EventArgs e)
        {
            if (CurrentPage == CountryCodesPage)
            {
                CountryCodeGridLoaded();
            }
        }
        #endregion

        #region Event Handlers

#if (!SILVERLIGHT)
        private async void CountryCodeGridLoaded()
        {
            ToggleBusy(true);
            var countryCodeClient = new restclientdotnet.RESTClient(new NewtonsoftSerializationAdapter(), new Uri("http://services.groupkt.com/country/get/all"));

            var countryData = await countryCodeClient.GetAsync<groupktResult<CountriesResult>>();
            CountryCodeList.ItemsSource = countryData.RestResponse.result;
            ToggleBusy(false);
        }
#endif

        private async void GetReposClick()
        {
            try
            {
                ToggleBusy(true);

                //Ensure the client is ready to go
                GetBitBucketClient();

                //Download the repository data
#if (SILVERLIGHT)
                var repos = (await _BitbucketClient.GetAsync<RepositoryList>());
#else
                var repos = (await _BitbucketClient.GetAsync<RepositoryList>());
#endif

                //Put it in the List Box
                ReposBox.ItemsSource = repos.values;
            }
            catch
            {

            }

            ToggleBusy(false);

        }

#if (!SILVERLIGHT)
        private void ReposBox_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var selectedRepo = ReposBox.SelectedItem as Repository;
            if (selectedRepo == null)
            {
                return;
            }
            DescriptionBox.Text = selectedRepo.description;
        }

        private async void ChangeDescriptionButton_Clicked(object sender, EventArgs e)
        {
            ToggleBusy(true);

            try
            {
                var selectedRepo = ReposBox.SelectedItem as Repository;
                if (selectedRepo == null)
                {
                    return;
                }

                //Ensure the client is ready to go
                GetBitBucketClient();

                selectedRepo.description = DescriptionBox.Text;

                var repoSlug = selectedRepo.full_name.Split('/')[1];

                //Post the change
                var retVal = await _BitbucketClient.PutAsync<Repository, Repository, string>(selectedRepo, repoSlug);
            }
            catch (Exception ex)
            {
                ErrorModel errorModel = null;
                var rex = ex as RESTException;

                if (rex != null)
                {
                    errorModel = rex.Error as ErrorModel;
                }

                string message = "An error occurred trying to change the repository description.";

                if (errorModel != null)
                {
                    message += "\r\n" + errorModel.error.message;
                }

                await DisplayAlert(message, "Error", "OK");
            }

            ToggleBusy(false);

        }
#endif

        #endregion

        #region Private Methods
        private void GetBitBucketClient()
        {
#if (SILVERLIGHT)
            string url = "http://localhost:49902/api/BitBucketRepository/" + UsernameBox.Text + "-" + ThePasswordBox.Password;
#else
            var url = "https://api.bitbucket.org/2.0/repositories/" + UsernameBox.Text;
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(UsernameBox.Text + ":" + ThePasswordBox.Text));
#endif
            _BitbucketClient = new restclientdotnet.RESTClient(new NewtonsoftSerializationAdapter(), new Uri(url));
#if (!SILVERLIGHT)
            _BitbucketClient.Headers.Add("Authorization", "Basic " + credentials);
#endif
            _BitbucketClient.ErrorType = typeof(ErrorModel);
        }

        private void ToggleBusy(bool isBusy)
        {
#if (!SILVERLIGHT)
            Ring2.IsVisible = isBusy;
#else
            Ring2.IsIndeterminate = isBusy;
#endif

        }
        #endregion

    }
}