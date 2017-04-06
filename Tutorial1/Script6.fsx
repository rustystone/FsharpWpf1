
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\WindowsFormsIntegration.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\WindowsBase.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\PresentationFramework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\PresentationCore.dll"
#r @"C:\MyCode1\FSharp1\Tut1\Tutorial1\packages\Microsoft.Xaml.4.0.0.1\lib\System.Xaml.dll"

open System 
open System.IO
open System.Windows.Forms
 
open System.Windows
open System.Windows.Controls
open System.Windows.Markup
open System.Windows.Media
open System.Xaml
open System.Xml
 
let xaml =
  "<Window
    xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation'
    xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'
    Title='Sort On Drawing Version' Height='327' Width='511'
    WindowStartupLocation='CenterScreen'>
    <Grid Height='Auto'>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width='270*' />
            <ColumnDefinition Width='270*' />
        </Grid.ColumnDefinitions>
        <Label Grid.ColumnSpan='2' Height='25' HorizontalAlignment='Left'
               Name='Label1' VerticalAlignment='Top'
               Width='77'>Root folder:</Label>
        <TextBox Grid.ColumnSpan='2' Height='26' Margin='73,-1,166,0'
                 Name='FolderBox' VerticalAlignment='Top' />
        <Button Height='29' Margin='5,0,0,33' Name='CalcButton'
                VerticalAlignment='Bottom' IsEnabled='False'>Calculate Size</Button>
        <Button Grid.ColumnSpan='2' Height='28' Margin='0,0,85,0'
                Name='BrowseButton' VerticalAlignment='Top'  Width='75'
                HorizontalAlignment='Right'>Browse...</Button>
        <ListView Grid.ColumnSpan='2' Margin='5,30,6,68'
                  Name='FileList'/>
        <Button Height='28' HorizontalAlignment='Right' Margin='0,0,6,0'
                Name='ListButton' VerticalAlignment='Top' Width='73'
                IsEnabled='False' Grid.Column='1'>List</Button>
        <Label Grid.Column='1' Height='29' Margin='0,0,6,33'
                Name='CalcLabel' VerticalAlignment='Bottom'
                IsEnabled='False'>Folder size</Label>
        <ComboBox Grid.Column='1' Name='SelectBox' Margin='64,268,0,0' VerticalAlignment='Bottom'>
            <ComboBoxItem IsSelected='True' Name='cbi1'>*.xml</ComboBoxItem>
            <ComboBoxItem Name='cbi2'>*.cs</ComboBoxItem>
            <ComboBoxItem Name='cbi3'>*.js</ComboBoxItem>
        </ComboBox>
    </Grid>
</Window>"
 
let (?) (this : Control) (prop: string) : 'T =
  this.FindName prop :?> 'T
let (+=) e f = Observable.add f e

// List all Controls on Window 
let window = XamlReader.Parse xaml :?> Window
let label1: Label = window?Label1
let folderBox: TextBox = window?FolderBox
let calcButton: Button = window?CalcButton
let browseButton: Button = window?BrowseButton
let fileList: ListView = window?FileList
let listButton: Button = window?ListButton
let sortProgress: ProgressBar = window?SortProgress
let calcLabel: Label = window?CalcLabel
let selectBox: ComboBox = window?SelectBox
let ccbi1: ComboBoxItem = window?cbi1
let ccbi2: ComboBoxItem = window?cbi2
let ccbi3: ComboBoxItem = window?cbi3

// Function to load Xaml from Xaml-file
let loadXamlWindow (filename:string) =
  let reader = XmlReader.Create(filename)
  XamlReader.Load(reader) :?> Window


let w = loadXamlWindow(@"C:\MyCode1\Csharp1\Wpf1\Wpf1\Wpf1\Window1.xaml")
w.Show()
 

// General message box 
let messageBox (s: string) =
  MessageBox.Show(s, "A message box" ) |> ignore
 

// Click event for folder browse button 
browseButton.Click +=
  fun _ ->
    let fbd =
      new FolderBrowserDialog(
        Description =
          "Select the folder with subfolders to search inside:" )
    if Directory.Exists folderBox.Text then
      fbd.SelectedPath <- folderBox.Text
    let dr = fbd.ShowDialog()
    if dr = DialogResult.OK then
      folderBox.Text <- fbd.SelectedPath

//  Event when textbox with chosen folder is changed
folderBox.TextChanged +=
  fun e ->
    let tb = e.Source :?> TextBox
    listButton.IsEnabled <- Directory.Exists tb.Text

// Click event for List button 
listButton.Click +=
  fun _ ->
    try
      fileList.ItemsSource <-
        Directory.GetFiles(
          folderBox.Text, selectBox.Text, SearchOption.AllDirectories )
    with _ ->
      "An error occured when retrieving the files."
      |> messageBox
 
      fileList.ItemsSource <-
        Directory.GetFiles(folderBox.Text, selectBox.Text)
 
  calcButton.IsEnabled <- true


// Gets file info from filename
let fileInfo filename = new FileInfo(filename)

// Gets the file size from a FileInfo object
let fileSize (fileinfo : FileInfo) = fileinfo.Length

// Converts a byte count to MB
let bytesToMB (bytes : Int64) = bytes / (1024L * 1024L)

// Get all files in subfolders
let rec getAllFiles dir pattern =
    seq { yield! Directory.EnumerateFiles(dir, pattern)
          for d in Directory.EnumerateDirectories(dir) do
              yield! getAllFiles d pattern }

// Get total size of files in subfolders
let getFolderSize4 folder filter =
    getAllFiles folder filter
    |> Seq.map fileInfo
    |> Seq.map fileSize
    |> Seq.fold (+) 0L
    |> bytesToMB


// Event for Combobox when selection is changed
selectBox.SelectionChanged += 
    fun _ -> 
        let listItems = [|"*.xml";"*.cs";"*.js"|]
        calcLabel.Content <- listItems.[selectBox.SelectedIndex]

 
// Click event to compute the total size of files in subfolders
calcButton.Click += (fun _ -> calcLabel.Content <- (getFolderSize4 folderBox.Text selectBox.Text).ToString())
 
#if COMPILED
[<System.STAThread>]
[<EntryPoint>]
let main _ = (new Application()).Run window
#else
(new Application()).Run window
#endif

