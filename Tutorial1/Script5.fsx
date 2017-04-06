//#I @"C:\Program Files\Reference Assemblies\Microsoft\Framework\v3.0"
//#r "PresentationCore.dll"
//#r "PresentationFramework.dll"
//#r "WindowsBase.dll"
//#r "WindowsFormsIntegration.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\WindowsFormsIntegration.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\WindowsBase.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\PresentationFramework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5.2\PresentationCore.dll"
 
open System.IO
open System.Windows.Forms
 
open System.Windows
open System.Windows.Controls
open System.Windows.Markup
open System.Windows.Media
 
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
    <Button Height='29' Margin='5,0,0,33' Name='CopyButton'
      VerticalAlignment='Bottom' IsEnabled='False'>Copy</Button>
    <Button Grid.ColumnSpan='2' Height='28' Margin='0,0,85,0'
      Name='BrowseButton' VerticalAlignment='Top'  Width='75'
      HorizontalAlignment='Right'>Browse...</Button>
    <ListView Grid.ColumnSpan='2' Margin='5,30,6,68'
      Name='FileList'/>
    <Button Height='28' HorizontalAlignment='Right' Margin='0,0,6,0'
      Name='ListButton' VerticalAlignment='Top' Width='73'
      IsEnabled='False' Grid.Column='1'>List</Button>
    <ProgressBar Grid.ColumnSpan='2' Height='28' Margin='5,0,6,2'
      Name='SortProgress' VerticalAlignment='Bottom' />
    </Grid>
  </Window>"
 
let (?) (this : Control) (prop: string) : 'T =
  this.FindName prop :?> 'T
let (+=) e f = Observable.add f e
 
let window = XamlReader.Parse xaml :?> Window
let label1: Label = window?Label1
let folderBox: TextBox = window?FolderBox
let copyButton: Button = window?CopyButton
let browseButton: Button = window?BrowseButton
let fileList: ListView = window?FileList
let listButton: Button = window?ListButton
let sortProgress: ProgressBar = window?SortProgress
let moveButton: Button = window?MoveButton
 
let getVersion fn =
 
  let (|StartsWith|_|) arg (s: string) =
    if s.StartsWith arg then Some() else None
 
  let data = Array.create 6 0uy
  use fs = File.OpenRead fn
  try
    if fs.Read(data, 0, 6) = 6 then
      match (new System.Text.ASCIIEncoding()).GetString data with
      | StartsWith "MC0.0" -> "R1.0"
      | StartsWith "AC1.2" -> "R1.2"
      | StartsWith "AC1.40" -> "R1.4"
      | StartsWith "AC1.50" -> "R2.05"
      | StartsWith "AC2.10" -> "R2.10"
      | StartsWith "AC2.21" -> "R2.21"
      | StartsWith "AC2.22" -> "R2.22"
      | StartsWith "AC1001" -> "R2.22"
      | StartsWith "AC1002" -> "R2.5"
      | StartsWith "AC1003" -> "R2.6"
      | StartsWith "AC1004" -> "R9"
      | StartsWith "AC1006" -> "R10"
      | StartsWith "AC1009" -> "R11"
      | StartsWith "AC1012" -> "R13"
      | StartsWith "AC1014" -> "R14"
      | StartsWith "AC1015" -> "2000"
      | StartsWith "AC1018" -> "2004"
      | StartsWith "AC1021" -> "2007"
      | StartsWith "AC1024" -> "2010"
      | _ -> "Unknown"
    else ""
  finally
    fs.Close()
 
let messageBox (s: string) =
  MessageBox.Show(s, "Sort On Drawing Version" ) |> ignore
 
let sortFiles move =
  let numSorted = ref 0
  let numSkipped = ref 0
  try
    if fileList.Items.Count = 0 then
      "Nothing to sort!" |> messageBox
    else
      sortProgress.Minimum <- 0.
      sortProgress.Maximum <- fileList.Items.Count - 1 |> float
      sortProgress.Value <- 0.
 
      for fn in Seq.cast fileList.Items do
        let ver = getVersion fn
        if not(System.String.IsNullOrEmpty ver) then
          let loc = Path.Combine(folderBox.Text, ver)
          if not(Directory.Exists loc) then
            Directory.CreateDirectory loc |> ignore
 
          let dest = Path.Combine(loc, Path.GetFileName fn)
          if not(File.Exists dest) then
            if move then
              File.Move(fn, dest)
            else
              File.Copy(fn, dest)
 
            incr numSorted
          else
            incr numSkipped
 
        sortProgress.Value <- sortProgress.Value + 1.
 
      System.String.Format(
        "{0} file{1} {2}, {3} (already existing) file{4} skipped.",
        !numSorted,
        (if !numSorted = 1 then "" else "s"),
        (if move then "moved" else "copied"),
        !numSkipped,
        (if !numSkipped = 1 then "" else "s" ) )
      |> messageBox
 
      sortProgress.Value <- 0.
      fileList.ItemsSource <- null
 
  with ex ->
    "A problem was found while sorting files: " + ex.Message
    |> messageBox
 
browseButton.Click +=
  fun _ ->
    let fbd =
      new FolderBrowserDialog(
        Description =
          "Select the root folder for the DWG version sort:" )
    if Directory.Exists folderBox.Text then
      fbd.SelectedPath <- folderBox.Text
    let dr = fbd.ShowDialog()
    if dr = DialogResult.OK then
      folderBox.Text <- fbd.SelectedPath
 
folderBox.TextChanged +=
  fun e ->
    let tb = e.Source :?> TextBox
    listButton.IsEnabled <- Directory.Exists tb.Text
 
listButton.Click +=
  fun _ ->
    try
      fileList.ItemsSource <-
        Directory.GetFiles(
          folderBox.Text, "*.xml", SearchOption.AllDirectories )
    with _ ->
      "A problem was found accessing sub-folders in this " +
      "location: will simply get the drawings in the root " +
      "folder."
      |> messageBox
 
      fileList.ItemsSource <-
        Directory.GetFiles(folderBox.Text, "*.xml")
 
  copyButton.IsEnabled <- true
  moveButton.IsEnabled <- true
 
moveButton.Click += fun _ -> sortFiles true
 
copyButton.Click += fun _ -> sortFiles false
 
#if COMPILED
[<System.STAThread>]
[<EntryPoint>]
let main _ = (new Application()).Run window
#else
(new Application()).Run window
#endif
