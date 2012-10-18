﻿using System;
using System.Collections.Generic;
using HudlRT.ViewModels;
using Windows.ApplicationModel.Activation;
using Caliburn.Micro;
using Windows.UI.Xaml;
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
            base.OnLaunched(args);
            
            EnsurePage(args);
        }
    }
}
