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

    type ItemViewModel() as this = 
        let mutable _lineOne = ""
        let mutable _lineTwo = ""
        let mutable _lineThree = ""
        let changed = new Event<_,_>()
        let notifyPropertyChanged s = changed.Trigger(this, PropertyChangedEventArgs(s))
        /// Sample ViewModel property this property is used in the view to display its value open a Binding.
        member x.LineOne with get() = _lineOne and set v = _lineOne <- v; notifyPropertyChanged "LineOne"

        /// Sample ViewModel property this property is used in the view to display its value open a Binding.
        member x.LineTwo with get() = _lineTwo and set v = _lineTwo <- v; notifyPropertyChanged "LineTwo"

        /// Sample ViewModel property this property is used in the view to display its value open a Binding.
        member x.LineThree with get() = _lineThree and set v = _lineThree <- v; notifyPropertyChanged "LineThree"

        interface INotifyPropertyChanged with 
           [<CLIEvent>]
           member x.PropertyChanged = changed.Publish

