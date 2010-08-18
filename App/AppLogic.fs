namespace WindowsPhoneListApp

open System
open System.Net
open System.Windows
open System.Windows.Controls
open System.Windows.Documents
open System.Windows.Ink
open System.Windows.Input
open System.Windows.Media
open System.Windows.Media.Animation
open System.Windows.Shapes
open System.Windows.Navigation
open Microsoft.Phone.Controls
open Microsoft.Phone.Shell

[<AutoOpen>]
module private Utilities = 
                                                                                      
    let utilityData = [ (1, "one"); (2, "two") ]

    let utilityFunction x y = 
        x + y * 2.0

    /// This is an implementation of the dynamic lookup operator for binding
    /// Xaml objects by name.
    let (?) (source:obj) (s:string) =
        match source with 
        | :? ResourceDictionary as r ->  r.[s] :?> 'T
        | :? Control as source -> 
            match source.FindName(s) with 
            | null -> invalidOp (sprintf "dynamic lookup of Xaml component %s failed" s)
            | :? 'T as x -> x
            | _ -> invalidOp (sprintf "dynamic lookup of Xaml component %s failed because the component found was of type %A instead of type %A"  s (s.GetType()) typeof<'T>)
        | _ -> invalidOp (sprintf "dynamic lookup of Xaml component %s failed because the source object was of type %A. It must be a control of a resource dictionary" s (source.GetType()))

[<AutoOpen>]
module Extensions = 

    type IEvent<'Del,'T when 'Del : delegate<'T,unit> and 'Del :> Delegate> with 
        member x.AddAsync (action) = 
            x.Add (fun args -> Async.StartImmediate(action args))
        member x.AddAsync (action) = 
            x.Add (fun args -> let p,cancellationToken = action args in Async.StartImmediate(p, cancellationToken=cancellationToken))

    type System.Net.WebClient with
        member this.AsyncDownloadString (address:Uri) : Async<string> =
            let downloadAsync =
                Async.FromContinuations (fun (cont, econt, ccont) ->
                            let userToken = new obj()
                            let rec handler = 
                                    System.Net.DownloadStringCompletedEventHandler (fun _ args ->
                                        if userToken = args.UserState then
                                            this.DownloadStringCompleted.RemoveHandler(handler)
                                            if args.Cancelled then
                                                ccont (new OperationCanceledException("canceled")) 
                                            elif args.Error <> null then
                                                econt args.Error
                                            else
                                                cont args.Result)
                            this.DownloadStringCompleted.AddHandler(handler)
                            this.DownloadStringAsync(address, userToken)
                        )

            async { 
                use! _holder = Async.OnCancel(fun _ -> this.CancelAsync())
                return! downloadAsync
              }

module ViewModel = 
    let viewModel = MainViewModel()


type DetailsPage() as this =
    inherit PhoneApplicationPage()

    // Load the Xaml for the page.
    do Application.LoadComponent(this, new System.Uri("/WindowsPhoneListApp;component/DetailsPage.xaml", System.UriKind.Relative))

    override this.OnNavigatedTo(e) =
        let ok, selectedIndex = this.NavigationContext.QueryString.TryGetValue("selectedItem")
        if ok then 
            let index = System.Int32.Parse(selectedIndex);
            let viewModelDetail = ViewModel.viewModel.Items.[index]
            this.DataContext <- viewModelDetail
            async { 
                viewModelDetail.LineTwo <- "loading..."
                let wc = new System.Net.WebClient()
                let url = viewModelDetail.LineOne
                let! html = wc.AsyncDownloadString(Uri("http://" + url))
                viewModelDetail.LineTwo <- "The HTML for " + url + " has " + string html.Length + " chars and " + string (html.Split([| ' '; '\r'; '\n'; '\t' |], StringSplitOptions.RemoveEmptyEntries).Length) + " words"
             } |> Async.StartImmediate 



type MainPage() as this =
    inherit PhoneApplicationPage()

    // Load the Xaml for the page.
    do Application.LoadComponent(this, new System.Uri("/WindowsPhoneListApp;component/MainPage.xaml", System.UriKind.Relative))

    // Bind named Xaml components relevant to this page.
    let mainListBox : ListBox = this?MainListBox

    // Handle selection changed on ListBox
    do mainListBox.SelectionChanged.Add(fun e -> 
            // If selected index is -1 (no selection) do nothing
            if (mainListBox.SelectedIndex <> -1) then 

                // Navigate to the new page and pass a parameter in the navigation context
                this.NavigationService.Navigate(new Uri("/WindowsPhoneListApp;component/DetailsPage.xaml?selectedItem=" + string mainListBox.SelectedIndex, UriKind.Relative)) |> ignore;

                // Reset selected index to -1 (no selection)
                mainListBox.SelectedIndex <- -1)

    override this.OnNavigatedTo(e) =
        if (this.DataContext = null) then
            this.DataContext <- ViewModel.viewModel



/// One instance of this type is created in the application host project.
type App(app:Application) = 

    // Global handler for uncaught exceptions. 
    // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
    do app.UnhandledException.Add(fun e -> 
            if (System.Diagnostics.Debugger.IsAttached) then
                // An unhandled exception has occurred, break into the debugger
                System.Diagnostics.Debugger.Break();
     )

    let rootFrame = new PhoneApplicationFrame();

    do app.RootVisual <- rootFrame;

    // Handle navigation failures
    do rootFrame.NavigationFailed.Add(fun _ -> 
        if (System.Diagnostics.Debugger.IsAttached) then
            // A navigation has failed; break into the debugger
            System.Diagnostics.Debugger.Break())

    // Navigate to the main page 
    do rootFrame.Navigate(new Uri("/WindowsPhoneListApp;component/MainPage.xaml", UriKind.Relative)) |> ignore

    // Required object that handles lifetime events for the application
    let service = PhoneApplicationService()
    // Code to execute when the application is launching (eg, from Start)
    // This code will not execute when the application is reactivated
    do service.Launching.Add(fun _ -> ())
    // Code to execute when the application is closing (eg, user hit Back)
    // This code will not execute when the application is deactivated
    do service.Closing.Add(fun _ -> ())
    // Code to execute when the application is activated (brought to foreground)
    // This code will not execute when the application is first launched
    do service.Activated.Add(fun _ -> ())
    // Code to execute when the application is deactivated (sent to background)
    // This code will not execute when the application is closing
    do service.Deactivated.Add(fun _ -> ())

    do app.ApplicationLifetimeObjects.Add(service) |> ignore
