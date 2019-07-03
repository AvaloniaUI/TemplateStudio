﻿'{**
' This code block adds the method `GetImageGalleryDataAsync()` to the SampleDataService of your project.
'**}
Namespace Services
    Public Module SampleDataService
'{[{
        Private _gallerySampleData As ObservableCollection(Of SampleImage)
'}]}

'^^
'{[{

        ' TODO WTS: Remove this once your image gallery page is displaying real data.
        Public Async Function GetImageGalleryDataAsync(localResourcesPath As String) As Task(Of ObservableCollection(Of SampleImage))
            Await Task.CompletedTask
            If _gallerySampleData Is Nothing Then
                _gallerySampleData = New ObservableCollection(Of SampleImage)()
                For i As Integer = 1 To 10
                    _gallerySampleData.Add(New SampleImage() With {
                        .ID = $"{i}",
                        .Source = $"{localResourcesPath}/SampleData/SamplePhoto{i}.png",
                        .Name = $"Image sample {i}"
                    })
                Next
            End If
            Return _gallerySampleData
        End Function
'}]}
    End Module
End Namespace
