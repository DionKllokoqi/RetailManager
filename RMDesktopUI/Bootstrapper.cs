﻿using AutoMapper;
using Caliburn.Micro;
using RMDesktopUI.Helpers;
using RMDesktopUI.Library.Api;
using RMDesktopUI.Library.Helpers;
using RMDesktopUI.Library.Models;
using RMDesktopUI.Models;
using RMDesktopUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RMDesktopUI
{
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer _container = new SimpleContainer();
        public Bootstrapper()
        {
            Initialize();

            ConventionManager.AddElementConvention<PasswordBox>(
            PasswordBoxHelper.BoundPasswordProperty,
            "Password",
            "PasswordChanged");
        }

        protected override void Configure()
        {
            // Put the mapper into the container (as a singleton in this case)
            _container.Instance(ConfigureAutoMapper());

            // Return an instace of this internal container when we ask the container for a simple container
            _container.Instance(_container)
                .PerRequest<IProductEndPoint, ProductEndPoint>()
                .PerRequest<IUserEndPoint, UserEndPoint>()
                .PerRequest<ISaleEndPoint, SaleEndPoint>();

            // Singleton -> We create one instance of the class for the life of the application (or lifetime scope of the container. If, e.g., ShellViewModel asks for an IEventAggregator, it's gonna get the first EventAggregator ever created -> One instance for the application. Don't use Singletons to much!
            _container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>()
                .Singleton<ILoggedInUserModel, LoggedInUserModel>()
                .Singleton<IConfigHelper, ConfigHelper>()
                .Singleton<IApiHelper, ApiHelper>();

            // Don't use reflection too much! Bad for performance
            GetType().Assembly.GetTypes()
                .Where(type => type.IsClass)
                .Where(type => type.Name.EndsWith("ViewModel"))
                .ToList()
                .ForEach(viewModelType => _container.RegisterPerRequest(
                    viewModelType, viewModelType.ToString(), viewModelType));
        }

        private IMapper ConfigureAutoMapper()
        {
            // Configure Automapper so that we have a relationship between the classes
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ProductModel, ProductDisplayModel>();
                cfg.CreateMap<CartItemModel, CartItemDisplayModel>();
            });

            var mapper = config.CreateMapper();

            return mapper;
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            // Launch ShellViewModel as our base view
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return _container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return _container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            _container.BuildUp(instance);
        }
    }
}
