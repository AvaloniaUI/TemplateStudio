﻿using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Animation;

using Param_ItemNamespace.Helpers;
using Param_ItemNamespace.Models;
using Param_ItemNamespace.Services;

using Prism.Windows.Mvvm;

namespace Param_ItemNamespace.ViewModels
{
    public class ImageGalleryViewDetailViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private static UIElement _image;
        private object _selectedImage;
        private ObservableCollection<SampleImage> _source;
        private readonly ISampleDataService sampleDataService;


        public object SelectedImage
        {
            get => _selectedImage;
            set
            {
                SetProperty(ref _selectedImage, value);
                ApplicationData.Current.LocalSettings.SaveString(ImageGalleryViewViewModel.ImageGalleryViewSelectedImageId, ((SampleImage)SelectedImage).ID);
            }
        }

        public ObservableCollection<SampleImage> Source
        {
            get => _source;
            set => SetProperty(ref _source, value);
        }

        public ImageGalleryViewDetailViewModel(ISampleDataService sampleDataService)
        {
            // TODO WTS: Replace this with your actual data
            this.sampleDataService = sampleDataService;
            Source = sampleDataService.GetGallerySampleData();
        }

        public void SetImage(UIElement image) => _image = image;

        public void Initialize(SampleImage sampleImage)
        {
            SelectedImage = Source.FirstOrDefault(i => i.ID == sampleImage.ID);
            var animation = ConnectedAnimationService.GetForCurrentView().GetAnimation(ImageGalleryViewViewModel.ImageGalleryViewAnimationOpen);
            animation?.TryStart(_image);
        }

        public void SetAnimation()
        {
            ConnectedAnimationService.GetForCurrentView()?.PrepareToAnimate(ImageGalleryViewViewModel.ImageGalleryViewAnimationClose, _image);
        }
    }
}
