// Example of writing a model in F#
namespace WindowsPhoneListApp

    open System
    open System.ComponentModel
    open System.Diagnostics
    open System.Net
    open System.Windows
    open System.Windows.Controls
    open System.Windows.Documents
    open System.Windows.Ink
    open System.Windows.Input
    open System.Windows.Media
    open System.Windows.Media.Animation
    open System.Windows.Shapes
    open System.Collections.ObjectModel


    type MainViewModel() as this = 
        let mutable items = new ObservableCollection<ItemViewModel>()
        do items.Add(ItemViewModel(LineOne="www.google.com", LineTwo = "F# is fun!", LineThree = "I am a huge fan of F# and have been using it since 2005 for all my applied research projects"))
        do items.Add(ItemViewModel(LineOne="www.bing.com", LineTwo = "Learn F#", LineThree = "Banking Firm Uses Functional Language to Speed Development by 50 Percent. A large financial services firm in Europe sought new development tools that could cut costs, boost productivity, and improve the quality of its mathematical models"))
        do items.Add(ItemViewModel(LineOne="www.yahoo.com", LineTwo = "Love F#", LineThree = "Come to this session to understand how F#, the new programming language from Microsoft, can be used to achieve huge performance gains in applications managing multiple, asynchronous tasks."))
        do items.Add(ItemViewModel(LineOne="www.apple.com", LineTwo = "Live F#", LineThree = "With F#... we have written a complete genome resequencing pipeline with interface, algs, reporting in ~5K lines and it has been incredibly reliable, fast  and easy to maintain."))
        let mutable sampleProperty = "Sample"

        let changed = new Event<_,_>()
        let notifyPropertyChanged s = changed.Trigger(this, PropertyChangedEventArgs(s))

        /// Sample MainModel collection property. This property is used in the view to display its value open a Binding.
        // Note: observable collections have their own change notification
        member x.Items = items

        /// Sample MainModel property. This property is used in the view to display its value open a Binding.
        member x.SampleProperty  with get() = sampleProperty and  set v = (sampleProperty <- v; notifyPropertyChanged "SampleProperty")

        interface INotifyPropertyChanged with 
           [<CLIEvent>]
           member x.PropertyChanged = changed.Publish
