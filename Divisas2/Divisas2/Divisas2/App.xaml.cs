using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Divisas2.Views;

using Xamarin.Forms;

namespace Divisas2
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            //se borro el MainPage.xaml que se crea en la raiz de la carpeta compartida
            //new ForeingExchangePage() hace referencia alnombre de la pagina
            //se borro el MainPage.xaml
            //Esta en la actividad con la cual se inicia el proyecto
            MainPage = new NavigationPage(new ForeingExchangePage());
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
