// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Windows.Controls;
using Microsoft.Templates.UI.Styles;
using Microsoft.Templates.UI.ViewModels.NewProject;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Microsoft.Templates.UI.Views.NewProject
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AvaloniaVersionPage : Page
    {
        public AvaloniaVersionPage()
        {
            Resources.MergedDictionaries.Add(AllStylesDictionary.GetMergeDictionary());

            DataContext = MainViewModel.Instance;
            this.InitializeComponent();
        }
    }
}
