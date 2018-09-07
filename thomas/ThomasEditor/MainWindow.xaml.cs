using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Reflection;
using System.ComponentModel;

using System.IO;

using ThomasEngine;
using System.Threading;
using Xceed.Wpf.AvalonDock.Layout.Serialization;

namespace ThomasEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        double scrollRatio = 0;
        TimeSpan lastRender;
        public static MainWindow _instance;

        public MainWindow()
        {
            string enginePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            Environment.SetEnvironmentVariable("THOMAS_ENGINE", enginePath, EnvironmentVariableTarget.User);

            _instance = this;
            InitializeComponent();

            playPauseButton.DataContext = false;
            ThomasEngine.Component.editorAssembly = Assembly.GetAssembly(this.GetType());

            //Changeds decimals to . instead of ,
            System.Globalization.CultureInfo customCulture = (System.Globalization.CultureInfo)System.Threading.Thread.CurrentThread.CurrentCulture.Clone();
            customCulture.NumberFormat.NumberDecimalSeparator = ".";

            System.Threading.Thread.CurrentThread.CurrentCulture = customCulture;
            CompositionTarget.Rendering += DoUpdates;
            ThomasWrapper.OutputLog.CollectionChanged += OutputLog_CollectionChanged;

            if (Properties.Settings.Default.latestProjectPath != "")
                OpenProject(Properties.Settings.Default.latestProjectPath);


            LoadLayout();
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            SaveLayout();
        }

        private void SaveLayout()
        {
            string target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "thomas/layout.dock");
            using (var sw = new StreamWriter(target))
            {
                using (StringWriter fs = new StringWriter())
                {
                    XmlLayoutSerializer xmlLayout = new XmlLayoutSerializer(dockManager);
                    xmlLayout.Serialize(fs);
                    sw.Write(fs.ToString());
                }
            }

            if (WindowState == WindowState.Maximized)
            {
                // Use the RestoreBounds as the current values will be 0, 0 and the size of the screen
                Properties.Settings.Default.Top = RestoreBounds.Top;
                Properties.Settings.Default.Left = RestoreBounds.Left;
                Properties.Settings.Default.Height = RestoreBounds.Height;
                Properties.Settings.Default.Width = RestoreBounds.Width;
                Properties.Settings.Default.Maximized = true;
            }
            else
            {
                Properties.Settings.Default.Top = this.Top;
                Properties.Settings.Default.Left = this.Left;
                Properties.Settings.Default.Height = this.Height;
                Properties.Settings.Default.Width = this.Width;
                Properties.Settings.Default.Maximized = false;
            }

            Properties.Settings.Default.Save();
        }

        private void LoadLayout()
        {
            string target = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "thomas/layout.dock");
            if (System.IO.File.Exists(target))
            {
                using (var sr = new StreamReader(target))
                {
                    using (StringWriter fs = new StringWriter())
                    {
                        XmlLayoutSerializer xmlLayout = new XmlLayoutSerializer(dockManager);
                        xmlLayout.Deserialize(sr);
                    }
                }
            }
            this.Top = Properties.Settings.Default.Top;
            this.Left = Properties.Settings.Default.Left;
            this.Height = Properties.Settings.Default.Height;
            this.Width = Properties.Settings.Default.Width;
            // Very quick and dirty - but it does the job
            if (Properties.Settings.Default.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void OutputLog_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                if (e.NewItems != null)
                {
                    foreach (String output in e.NewItems)
                    {
                        TextBlock block = new TextBlock
                        {
                            Text = output,
                            TextWrapping = TextWrapping.Wrap
                        };
                        console.Items.Add(block);
                        console.Items.Add(new Separator());
                        if (console.Items.Count > 10)
                            console.Items.RemoveAt(0);
                    }
                } 
            }));
            
        }

        private void Node_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ContextMenu cm = this.FindResource("gameObjectContext") as ContextMenu;
            cm.PlacementTarget = sender as TreeViewItem;
            cm.IsOpen = true;
        }
                
        private void DoUpdates(object sender, EventArgs e)
        {                       
            RenderingEventArgs args = (RenderingEventArgs)e;
            
            if(this.lastRender != args.RenderingTime)
            {
                ThomasWrapper.Update();
                editorWindow.Title = ThomasWrapper.FrameRate.ToString();
                lastRender = args.RenderingTime;
                transformGizmo.UpdateTransformGizmo();
             }
            
        }

        private void NewEmptyObject_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void AddEmptyGameObject(object sender, RoutedEventArgs e)
        {
            var x = new GameObject("gameObject");
            ThomasWrapper.Selection.SelectGameObject(x);
        }

        private void OpenOptionsWindow_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenOptionsWindow(object sender, RoutedEventArgs e)
        {
            OptionsWindow oW = new OptionsWindow();
            oW.ShowDialog();
            oW.Focus();
        }

        private void Menu_RemoveGameObject(object sender, RoutedEventArgs e)
        {
            var x = sender as MenuItem;
            (x.DataContext as GameObject).Destroy();
        }

        private void RemoveSelectedGameObjects_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void RemoveSelectedGameObjects(object sender, RoutedEventArgs e)
        {
            for(int i=0; i < ThomasWrapper.Selection.Count; i++)
            {
                GameObject gObj = ThomasWrapper.Selection.op_Subscript(i);
                gObj.Destroy();
                //ThomasWrapper.SelectedGameObjects.RemoveAt(i);
               i--;
            }
        }


        private void Console_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {

            ScrollViewer scrollViewer = Extensions.GetDescendantByType<ScrollViewer>(sender as ListBox);

            if (e.ExtentWidthChange != 0 || e.ExtentHeightChange != 0)
            {
                //calculate and set accordingly
                double offset = scrollRatio * e.ExtentHeight - 0.5 * e.ViewportHeight;
                //see if it is negative because of initial values
                if (offset < 0)
                {
                    //center the content
                    //this can be set to 0 if center by default is not needed
                    offset = 0.5 * scrollViewer.ScrollableHeight;
                }
                scrollViewer.ScrollToVerticalOffset(offset);
            }
            else
            {
                //store the relative values if normal scroll
                if(e.ExtentHeight > 0)
                    scrollRatio = (e.VerticalOffset + 0.5 * e.ViewportHeight) / e.ExtentHeight;
            }


        }

        private void Recompile_Shader_Click(object sender, RoutedEventArgs e)
        {
            Shader.RecompileShaders();
        }

        private void Play_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void SaveScene_Click(object sender, RoutedEventArgs e)
        {

            if(Scene.CurrentScene.RelativeSavePath != null)
            {
                Scene sceneToSave = Scene.CurrentScene;
                Scene.SaveScene(sceneToSave);
            }
            else
            {
                SaveSceneAs_Click(sender, e);
            }
        }


        private void NewScene_Click(object sender, RoutedEventArgs e)
        {
            Scene.CurrentScene.UnLoad();
            Scene.CurrentScene = new Scene("Scene");
        }

        private void SaveSceneAs_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Thomas Dank Scene (*.tds) |*.tds";

            if (ThomasEngine.Application.currentProject == null)
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            else
                saveFileDialog.InitialDirectory = ThomasEngine.Application.currentProject.assetPath;
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = Scene.CurrentScene.Name;
            if (saveFileDialog.ShowDialog() == true)
            {
                Scene sceneToSave = Scene.CurrentScene;
                Scene.SaveSceneAs(sceneToSave, saveFileDialog.FileName);
                sceneToSave.Name = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
            }
        }

        private void LoadScene_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Thomas Dank Scene (*.tds) |*.tds";

            if (ThomasEngine.Application.currentProject == null)
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            else
                openFileDialog.InitialDirectory = ThomasEngine.Application.currentProject.assetPath;

            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                LoadScene(openFileDialog.FileName);
            }     
        }

        public void LoadScene(string path)
        {
            BackgroundWorker worker = new BackgroundWorker();
            showBusyIndicator("Loading scene...");
            worker.DoWork += (o, ea) =>
            {
                Scene newScene = Scene.LoadScene(path);
                Scene.CurrentScene.UnLoad();
                Scene.CurrentScene = newScene;
                Scene.CurrentScene.PostLoad();
            };
            worker.RunWorkerCompleted += (o, ea) =>
            {
                hideBusyIndicator();
            };
            worker.RunWorkerAsync();
        }

        private void PlayPauseButton_Click(object sender, ExecutedRoutedEventArgs e)
        {
            if (ThomasWrapper.IsPlaying())
                ThomasWrapper.Stop();
            else
                ThomasWrapper.Play();

            playPauseButton.DataContext = ThomasWrapper.IsPlaying();
        }

        #region primitives

        private void AddNewCubePrimitive(object sender, RoutedEventArgs e)
        {
            var x = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ThomasWrapper.Selection.SelectGameObject(x);
        }

        private void AddNewSpherePrimitive(object sender, RoutedEventArgs e)
        {
            var x = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ThomasWrapper.Selection.SelectGameObject(x);
        }

        private void AddNewQuadPrimitive(object sender, RoutedEventArgs e)
        {
            var x = GameObject.CreatePrimitive(PrimitiveType.Quad);
            ThomasWrapper.Selection.SelectGameObject(x);
        }

        private void AddNewPlanePrimitive(object sender, RoutedEventArgs e)
        {
            var x = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ThomasWrapper.Selection.SelectGameObject(x);
        }

        private void AddNewCylinderPrimitive(object sender, RoutedEventArgs e)
        {
            var x = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ThomasWrapper.Selection.SelectGameObject(x);
        }

        private void AddNewCapsulePrimitive(object sender, RoutedEventArgs e)
        {
            var x = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            ThomasWrapper.Selection.SelectGameObject(x);
        }

        #endregion

        private void NewProject_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.Filter = "Thomas Project (*.thomas) |*.thomas";


            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            saveFileDialog.RestoreDirectory = true;
            saveFileDialog.FileName = "New Project";

            showBusyIndicator("Creating new project...");
            if (saveFileDialog.ShowDialog() == true)
            {
                Thread worker = new Thread(new ThreadStart(() =>
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(saveFileDialog.FileName);
                    string dir = System.IO.Path.GetDirectoryName(saveFileDialog.FileName);

                    if (utils.ScriptAssemblyManager.CreateSolution(dir + "\\" + fileName, fileName))
                    {
                        Project proj = new Project(fileName, dir);
                        ThomasEngine.Application.currentProject = proj;
                    }
                    hideBusyIndicator();
                }));
                worker.SetApartmentState(ApartmentState.STA);
                worker.Start();

            }
                      
        }


       private void showBusyIndicator(string message)
       {
            busyCator.IsBusy = true;
            busyCator.BusyContent = message;
            editor.Visibility = Visibility.Hidden;
            game.Visibility = Visibility.Hidden;
        }
        private void hideBusyIndicator()
        {
            this.Dispatcher.Invoke((Action)(() =>
            {
                busyCator.IsBusy = false;
                editor.Visibility = Visibility.Visible;
                game.Visibility = Visibility.Visible;
            }));
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Thomas Project (*.thomas) |*.thomas";
            
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            openFileDialog.RestoreDirectory = true;

            if (openFileDialog.ShowDialog() == true)
            {
                OpenProject(openFileDialog.FileName);
                
            }
            
        }
        public void OpenProject(string projectPath)
        {
            showBusyIndicator("Opening project...");

            Thread worker = new Thread(new ThreadStart(() =>
            {
                string fileName = System.IO.Path.GetFileNameWithoutExtension(projectPath);
                string dir = System.IO.Path.GetDirectoryName(projectPath);
                if (utils.ScriptAssemblyManager.OpenSolution(dir + "/" + fileName + ".sln"))
                {
                    ThomasEngine.Application.currentProject = Project.LoadProject(projectPath);
                    Properties.Settings.Default.latestProjectPath = projectPath;
                    Properties.Settings.Default.Save();
                }
                hideBusyIndicator();
            }));
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();
        }

        private void ReloadAssembly(object sender, RoutedEventArgs e)
        {
            ScriptingManger.LoadAssembly();
        }

        private void __layoutRoot_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void SaveLayout_Click(object sender, RoutedEventArgs e)
        {
            SaveLayout();
        }
    }

    public static class Extensions
    {

        public static T GetDescendantByType<T>(this Visual element) where T : class
        {
            if (element == null)
            {
                return default(T);
            }
            if (element.GetType() == typeof(T))
            {
                return element as T;
            }
            T foundElement = null;
            if (element is FrameworkElement)
            {
                (element as FrameworkElement).ApplyTemplate();
            }
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var visual = VisualTreeHelper.GetChild(element, i) as Visual;
                foundElement = visual.GetDescendantByType<T>();
                if (foundElement != null)
                {
                    break;
                }
            }
            return foundElement;
        }
    }
}
