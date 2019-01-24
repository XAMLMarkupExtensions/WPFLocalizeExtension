﻿using Caliburn.Micro;
using WPFLocalizationExtensionDemoApplication.ViewModels.Examples;

namespace WPFLocalizationExtensionDemoApplication.ViewModels
{
    public class MainViewModel : Conductor<Screen>.Collection.OneActive
    {
        public MainViewModel()
        {
            Items.Add(new GapTextWpfExampleViewModel());
            Items.Add(new TextLocalizationExampleViewModel());
        }
    }
}