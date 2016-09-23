﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DereTore.Applications.StarlightDirector.Components;
using DereTore.Applications.StarlightDirector.Entities;
using DereTore.Applications.StarlightDirector.Extensions;
using DereTore.Applications.StarlightDirector.Interop;
using NAudio.Wave;
using AudioOut = NAudio.Wave.WasapiOut;

namespace DereTore.Applications.StarlightDirector.UI.Windows {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    partial class MainWindow {

        public MainWindow() {
            InitializeComponent();
            CommandHelper.InitializeCommandBindings(this);
        }

        public Project Project {
            get { return (Project)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public Brush AccentColorBrush {
            get { return (Brush)GetValue(AccentColorBrushProperty); }
            private set { SetValue(AccentColorBrushProperty, value); }
        }

        public static readonly DependencyProperty ProjectProperty = DependencyProperty.Register(nameof(Project), typeof(Project), typeof(MainWindow),
            new PropertyMetadata(null, OnProjectChanged));

        public static readonly DependencyProperty AccentColorBrushProperty = DependencyProperty.Register(nameof(AccentColorBrush), typeof(Brush), typeof(MainWindow),
            new PropertyMetadata(ColorizationHelper.GetWindowColorizationBrush()));

        private static void OnProjectChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            var window = obj as MainWindow;
            var newProject = e.NewValue as Project;
            var oldProject = e.OldValue as Project;
            Debug.Assert(window != null);
            window.Editor.Project = newProject;
            window.Editor.Score = newProject?.GetScore(newProject.Difficulty);

            if (oldProject != null) {
                oldProject.DifficultyChanged -= window.Project_DifficultyChanged;
                //BindingOperations.ClearBinding(window.DifficultySelector, ComboBox.SelectedIndexProperty);
            }
            if (newProject != null) {
                newProject.DifficultyChanged += window.Project_DifficultyChanged;
                //var binding = new Binding();
                //binding.Converter = new DifficultyToIndexConverter();
                //binding.Path = new PropertyPath($"{nameof(Project)}.{nameof(Entities.Project.Difficulty)}");
                //binding.Mode = BindingMode.TwoWay;
                //binding.Source = window;
                //BindingOperations.SetBinding(window.DifficultySelector, ComboBox.SelectedIndexProperty, binding);
            }
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e) {
            Project = Project.Current;
        }

        private void Project_DifficultyChanged(object sender, EventArgs e) {
            var project = Project;
            Editor.Score = project?.GetScore(project.Difficulty);
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e) {
            if (Project == null || (!Project.IsChanged && Project.IsSaved)) {
                return;
            }
            var result = MessageBox.Show(Application.Current.FindResource<string>(App.ResourceKeys.ExitPrompt), Title, MessageBoxButton.YesNoCancel, MessageBoxImage.Exclamation);
            switch (result) {
                case MessageBoxResult.Yes:
                    if (CmdFileSaveProject.CanExecute(null)) {
                        CmdFileSaveProject.Execute(null);
                    }
                    if (!Project.IsSaved) {
                        e.Cancel = true;
                    }
                    break;
                case MessageBoxResult.No:
                    break;
                case MessageBoxResult.Cancel:
                    e.Cancel = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result), result, null);
            }
        }

        private void MainWindow_OnSourceInitialized(object sender, EventArgs e) {
            this.RegisterWndProc(WndProc);
        }
        
        private void OnDwmColorizationColorChanged(object sender, EventArgs e) {
            AccentColorBrush = ColorizationHelper.GetWindowColorizationBrush();
        }

        private IntPtr WndProc(IntPtr hWnd, int uMsg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            switch (uMsg) {
                case NativeConstants.WM_DWMCOLORIZATIONCOLORCHANGED:
                    OnDwmColorizationColorChanged(this, EventArgs.Empty);
                    return IntPtr.Zero;
                default:
                    return IntPtr.Zero;
            }
        }

        private void SelectedWaveOut_PlaybackStopped(object sender, EventArgs e) {
            var waveOut = _selectedWaveOut;
            if (waveOut != null) {
                waveOut.PlaybackStopped -= SelectedWaveOut_PlaybackStopped;
                waveOut.Dispose();
                _selectedWaveOut = null;
            }
            _waveReader?.Dispose();
            _waveReader = null;
        }

        private AudioOut _selectedWaveOut;
        private WaveFileReader _waveReader;

    }
}
