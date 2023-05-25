namespace Param_RootNamespace

open Avalonia
open Avalonia.Controls.ApplicationLifetimes
//^^
//{[{
open Avalonia.Data.Core.Plugins
//}]}
open Avalonia.Markup.Xaml
open Param_RootNamespace.ViewModels
open Param_RootNamespace.Views

type App() =
    inherit Application()

    override this.Initialize() =
            AvaloniaXamlLoader.Load(this)

    override this.OnFrameworkInitializationCompleted() =
        //^^
        //{[{
        BindingPlugins.DataValidators.RemoveAt(0)
        //}]}

        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktopLifetime ->
            desktopLifetime.MainWindow <- MainWindow(DataContext = MainViewModel())
        | :? ISingleViewApplicationLifetime as singleViewLifetime ->
            singleViewLifetime.MainView <- MainView(DataContext = MainViewModel())
        | _ -> ()

        base.OnFrameworkInitializationCompleted()
