﻿using System.Threading.Tasks;
using Catrobat.IDE.Core.CatrobatObjects;
using Catrobat.IDE.Core.Services;
using Catrobat.IDE.Core.UI;
using Catrobat.IDE.Core.UI.PortableUI;
using Catrobat.IDE.Core.ViewModels;
using Catrobat.IDE.Core.ViewModels.Editor.Costumes;
using Catrobat.IDE.Core.Xml;
using Catrobat.IDE.Core.Xml.XmlObjects;
using Catrobat.IDE.Tests.Services;
using Catrobat.IDE.Tests.Services.Storage;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Catrobat.IDE.Tests.Tests.ViewModel.Editor.Costumes
{
    [TestClass]
    public class CostumeNameChooserViewModelTests
    {
        private PortableImage _imageToSave;

        [ClassInitialize]
        public static void TestClassInitialize(TestContext testContext)
        {
            ServiceLocator.NavigationService = new NavigationServiceTest();
            ServiceLocator.Register<StorageFactoryTest>(TypeCreationMode.Normal);
            ServiceLocator.Register<StorageTest>(TypeCreationMode.Normal);
            ServiceLocator.Register<ImageResizeServiceTest>(TypeCreationMode.Normal);
            ServiceLocator.Register<SensorServiceTest>(TypeCreationMode.Normal);
        }

        [TestMethod] // , TestCategory("GatedTests") // TODO: fix test takes very long time on server
        public async Task SaveActionTest()
        {
            _imageToSave = null;
            Messenger.Default.Register<GenericMessage<PortableImage>>(this,
                ViewModelMessagingToken.CostumeImageToSaveListener, CostumeImageReceivedMessageAction);
            
            var navigationService = (NavigationServiceTest)ServiceLocator.NavigationService;
            navigationService.PageStackCount = 2;
            navigationService.CurrentNavigationType = NavigationServiceTest.NavigationType.Initial;
            navigationService.CurrentView = typeof(CostumeNameChooserViewModel);

            var viewModel = new CostumeNameChooserViewModel();
            viewModel.SelectedSize = new ImageSizeEntry { NewHeight = 100, NewWidth = 100};
            viewModel.CostumeName = "TestCostume";

            var project = new XmlProject { ProjectHeader = new XmlProjectHeader(false) { ProgramName = "TestProject" } };
            var messageContext = new GenericMessage<XmlProject>(project);
            Messenger.Default.Send(messageContext, ViewModelMessagingToken.CurrentProjectChangedListener);

            var sprite = new XmlSprite();
            var messageContext2 = new GenericMessage<XmlSprite>(sprite);
            Messenger.Default.Send(messageContext2, ViewModelMessagingToken.CurrentSpriteChangedListener);
            
            var messageContext3 = new GenericMessage<PortableImage>(new PortableImage());
            Messenger.Default.Send(messageContext3, ViewModelMessagingToken.CostumeImageListener);

            await viewModel.SaveCommand.ExecuteAsync(null);

            Assert.IsNotNull(_imageToSave);
            Assert.AreEqual(1, sprite.Costumes.Costumes.Count);
            Assert.IsNotNull(sprite.Costumes.Costumes[0].Image);
            Assert.AreEqual(NavigationServiceTest.NavigationType.NavigateBack, navigationService.CurrentNavigationType);
            Assert.AreEqual(null, navigationService.CurrentView);
            Assert.AreEqual(0, navigationService.PageStackCount);
        }

        [TestMethod] // , TestCategory("GatedTests") // TODO: fix test takes very long time on server
        public void CancelActionTest()
        {
            var navigationService = (NavigationServiceTest)ServiceLocator.NavigationService;
            navigationService.PageStackCount = 2;
            navigationService.CurrentNavigationType = NavigationServiceTest.NavigationType.Initial;
            navigationService.CurrentView = typeof(CostumeNameChooserViewModel);

            var viewModel = new CostumeNameChooserViewModel();

            viewModel.CancelCommand.Execute(null);

            Assert.AreEqual(NavigationServiceTest.NavigationType.NavigateBack, navigationService.CurrentNavigationType);
            Assert.AreEqual(null, navigationService.CurrentView);
            Assert.AreEqual(0, navigationService.PageStackCount);
        }

        [TestMethod] // , TestCategory("GatedTests") // TODO: fix test takes very long time on server
        public void GoBackActionTest()
        {
            var navigationService = (NavigationServiceTest)ServiceLocator.NavigationService;
            navigationService.PageStackCount = 1;
            navigationService.CurrentNavigationType = NavigationServiceTest.NavigationType.Initial;
            navigationService.CurrentView = typeof(CostumeNameChooserViewModel);

            var viewModel = new CostumeNameChooserViewModel();

            viewModel.GoBackCommand.Execute(null);

            Assert.AreEqual(NavigationServiceTest.NavigationType.NavigateBack, navigationService.CurrentNavigationType);
            Assert.AreEqual(null, navigationService.CurrentView);
            Assert.AreEqual(0, navigationService.PageStackCount);
        }

        private void CostumeImageReceivedMessageAction(GenericMessage<PortableImage> message)
        {
            _imageToSave = message.Content;
        }
    }
}
