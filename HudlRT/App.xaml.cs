﻿using Caliburn.Micro;
using HudlRT.ViewModels;
using HudlRT.Views;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

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
            container = new WinRTContainer();
            container.RegisterWinRTServices();
            container
                .PerRequest<LoginViewModel>()
                .PerRequest<ErrorViewModel>()
                .PerRequest<HubViewModel>()
                .PerRequest<SectionViewModel>()
                .PerRequest<VideoPlayerViewModel>()
                ;
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

        protected override void PrepareViewFirst(Frame rootFrame)
        {
            container.RegisterNavigationService(rootFrame);
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            DisplayRootView<LoginView>(args.SplashScreen);
        }
    }
}
