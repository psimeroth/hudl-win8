using System;
using System.Collections.Generic;
using HudlRT.ViewModels;
using Windows.ApplicationModel.Activation;
using Caliburn.Micro;
using Windows.Storage;


namespace HudlRT
{
    public sealed partial class App
    {
        private WinRTContainer container;

        public App()
        {
            InitializeComponent();
        }

        protected override void Configure()
        {
            base.Configure();

            container = new WinRTContainer(RootFrame);
            container.RegisterWinRTServices();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override Type GetDefaultViewModel()
        {
            return typeof (LoginViewModel);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            if (args.PreviousExecutionState != ApplicationExecutionState.Running)
            {
                bool loadState = (args.PreviousExecutionState == ApplicationExecutionState.Terminated);

                ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-x"] = args.SplashScreen.ImageLocation.Left;
                ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-y"] = args.SplashScreen.ImageLocation.Top;
                ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-height"] = args.SplashScreen.ImageLocation.Height;
                ApplicationData.Current.RoamingSettings.Values["hudl-app-splash-width"] = args.SplashScreen.ImageLocation.Width;
            }

            base.OnLaunched(args);
        }
    }
}
