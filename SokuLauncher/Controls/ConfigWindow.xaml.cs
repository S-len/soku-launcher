﻿using SokuLauncher.Models;
using SokuLauncher.Utils;
using SokuLauncher.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Button = System.Windows.Controls.Button;
using Microsoft.Win32;
using Dsafa.WpfColorPicker;
using System.Threading;

namespace SokuLauncher.Controls
{
    public partial class ConfigWindow : Window
    {
        ConfigWindowViewModel ViewModel;
        public ConfigWindow(ConfigWindowViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new ConfigWindowViewModel();
            }
            DataContext = ViewModel;
            InitializeComponent();
            GetSokuFileIcon();
            ViewModel.Saveable = false;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ViewModel.Saveable)
            {
                if (MessageBox.Show("There are unsaved changes, do you want to discard changes?", "Unsaved changes", MessageBoxButton.YesNo, MessageBoxImage.Question)
                    == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            MainWindow mainWindow = new MainWindow
            {
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            mainWindow.Show();
        }

        private void SokuFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            string fileName = ConfigUtil.OpenExeFileDialog(ViewModel.SokuDirFullPath);

            if (fileName != null)
            {
                string selectedFileName = Path.GetFileName(fileName);
                string selectedDirPath = Path.GetDirectoryName(fileName);
                string relativePath = Static.GetRelativePath(selectedDirPath, Static.SelfFileDir);
                if (!relativePath.StartsWith("../../"))
                {
                    selectedDirPath = relativePath;
                }

                if (selectedDirPath != ViewModel.SokuDirPath || selectedFileName != ViewModel.SokuFileName)
                {
                    ViewModel.SokuDirPath = selectedDirPath;
                    ViewModel.SokuFileName = selectedFileName;
                    GetSokuFileIcon();

                    ViewModel.ModsManager.SokuDirFullPath = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));
                    ViewModel.ModsManager.SearchModulesDir();
                    ViewModel.ModsManager.LoadSWRSToysSetting();
                    ViewModel.UpdateModsPathInfo();
                }
            }
        }

        private void GetSokuFileIcon()
        {
            ViewModel.SokuFileIcon = Static.GetExtractAssociatedIcon(Path.GetFullPath(Path.Combine(ViewModel.SokuDirPath, ViewModel.SokuFileName ?? "")));
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Static.ConfigUtil.Config.SokuDirPath = ViewModel.SokuDirPath;
            Static.ConfigUtil.Config.SokuFileName = ViewModel.SokuFileName;
            Static.ConfigUtil.Config.SokuModSettingGroups = ViewModel.SokuModSettingGroups.ToList();
            Static.ConfigUtil.Config.SokuModAlias = ViewModel.SokuModAlias.ToList();
            Static.ConfigUtil.Config.AutoCheckForUpdates = ViewModel.AutoCheckForUpdates;
            Static.ConfigUtil.Config.AutoCheckForInstallable = ViewModel.AutoCheckForInstallable;
            Static.ConfigUtil.Config.VersionInfoUrl = ViewModel.VersionInfoUrl;
            Static.ConfigUtil.Config.Language = ViewModel.Language;

            Static.ConfigUtil.SaveConfig();

            if (Static.ModsManager.SokuDirFullPath != Static.ConfigUtil.SokuDirFullPath)
            {
                Static.ModsManager.SokuDirFullPath = Static.ConfigUtil.SokuDirFullPath;
                Static.ModsManager.SearchModulesDir();
                Static.ModsManager.LoadSWRSToysSetting();
            }
            ViewModel.Saveable = false;
        }

        private void SokuModSettingGroupsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.SelectedSokuModSettingGroup != null)
            {
                ModSettingGroupEditGrid.Opacity = 0;
                ModSettingGroupEditGrid.Visibility = Visibility.Visible;
                HideSokuModSettingGroupsGridAnimation((s, _) =>
                {
                    SokuModSettingGroupsGrid.Visibility = Visibility.Collapsed;
                });
                ShowModSettingGroupEditGridAnimation();
            }
        }

        private void HideSokuModSettingGroupsGridAnimation(EventHandler callback = null)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = 2,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            if (callback != null)
            {
                fadeAnimation.Completed += callback;
            }

            SokuModSettingGroupsGrid.Opacity = 1;
            SokuModSettingGroupsGrid.RenderTransform = new ScaleTransform(1, 1);
            SokuModSettingGroupsGrid.RenderTransformOrigin = new Point(.5, .5);

            SokuModSettingGroupsGrid.BeginAnimation(OpacityProperty, fadeAnimation);
            SokuModSettingGroupsGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            SokuModSettingGroupsGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }
        private void ShowSokuModSettingGroupsGridAnimation(EventHandler callback = null)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            if (callback != null)
            {
                fadeAnimation.Completed += callback;
            }

            SokuModSettingGroupsGrid.RenderTransform = new ScaleTransform(2, 2);
            SokuModSettingGroupsGrid.RenderTransformOrigin = new Point(.5, .5);

            SokuModSettingGroupsGrid.BeginAnimation(OpacityProperty, fadeAnimation);
            SokuModSettingGroupsGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            SokuModSettingGroupsGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }
        private void ShowModSettingGroupEditGridAnimation(EventHandler callback = null)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            if (callback != null)
            {
                fadeAnimation.Completed += callback;
            }

            ModSettingGroupEditGrid.RenderTransform = new ScaleTransform(.5, .5);
            ModSettingGroupEditGrid.RenderTransformOrigin = new Point(.5, .5);

            ModSettingGroupEditGrid.BeginAnimation(OpacityProperty, fadeAnimation);
            ModSettingGroupEditGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            ModSettingGroupEditGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }
        private void HidModSettingGroupEditGridAnimation(EventHandler callback = null)
        {
            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = .5,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            if (callback != null)
            {
                fadeAnimation.Completed += callback;
            }

            ModSettingGroupEditGrid.RenderTransform = new ScaleTransform(1, 1);
            ModSettingGroupEditGrid.RenderTransformOrigin = new Point(.5, .5);

            ModSettingGroupEditGrid.BeginAnimation(OpacityProperty, fadeAnimation);
            ModSettingGroupEditGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            ModSettingGroupEditGrid.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }

        private void BackToModSettingGroupsButton_Click(object sender, RoutedEventArgs e)
        {
            SokuModSettingGroupsGrid.Opacity = 0;
            SokuModSettingGroupsGrid.Visibility = Visibility.Visible;
            HidModSettingGroupEditGridAnimation((s, _) =>
            {
                ModSettingGroupEditGrid.Visibility = Visibility.Collapsed;
                ViewModel.SelectedSokuModSettingGroup = null;
            });
            ShowSokuModSettingGroupsGridAnimation();
        }

        private void SokuModSettingGroupsListView_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            e.Handled = true;
        }

        private void MoveUpModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ModSettingGroupViewModel selectedMember = (ModSettingGroupViewModel)button.DataContext;

            var listViewItem = SokuModSettingGroupsListView.ItemContainerGenerator.ContainerFromItem(selectedMember) as ListViewItem;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = -98,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            moveAnimation.Completed += (s, _) =>
            {
                int selectedIndex = ViewModel.SokuModSettingGroups.IndexOf(selectedMember);
                if (selectedIndex > 0)
                {
                    ModSettingGroupViewModel previousMember = ViewModel.SokuModSettingGroups[selectedIndex - 1];
                    ViewModel.SokuModSettingGroups[selectedIndex - 1] = selectedMember;
                    ViewModel.SokuModSettingGroups[selectedIndex] = previousMember;

                    ViewModel.Saveable = true;
                }
                listViewItem.RenderTransform = new TranslateTransform(0, 0);
            };

            listViewItem.RenderTransform = new TranslateTransform(0, 0);
            listViewItem.RenderTransformOrigin = new Point(.5, 0);

            listViewItem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }

        private void MoveDownModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ModSettingGroupViewModel selectedMember = (ModSettingGroupViewModel)button.DataContext;

            var listViewItem = SokuModSettingGroupsListView.ItemContainerGenerator.ContainerFromItem(selectedMember) as ListViewItem;

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = 98,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseInOut }
            };

            var originZindex = Panel.GetZIndex(listViewItem);
            Panel.SetZIndex(listViewItem, 100);

            moveAnimation.Completed += (s, _) =>
            {
                int selectedIndex = ViewModel.SokuModSettingGroups.IndexOf(selectedMember);
                if (selectedIndex < ViewModel.SokuModSettingGroups.Count - 1)
                {

                    ModSettingGroupViewModel nextMember = ViewModel.SokuModSettingGroups[selectedIndex + 1];
                    ViewModel.SokuModSettingGroups[selectedIndex + 1] = selectedMember;
                    ViewModel.SokuModSettingGroups[selectedIndex] = nextMember;

                    ViewModel.Saveable = true;
                }
                Panel.SetZIndex(listViewItem, originZindex);
                listViewItem.RenderTransform = new TranslateTransform(0, 0);
            };

            listViewItem.RenderTransform = new TranslateTransform(0, 0);
            listViewItem.RenderTransformOrigin = new Point(.5, 1);

            listViewItem.RenderTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);
        }

        private void DeleteModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ModSettingGroupViewModel selectedMember = (ModSettingGroupViewModel)button.DataContext;
            var listViewItem = SokuModSettingGroupsListView.ItemContainerGenerator.ContainerFromItem(selectedMember) as ListViewItem;

            DoubleAnimation fadeAnimation = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(300),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            DoubleAnimation moveAnimation = new DoubleAnimation
            {
                To = .5,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
            };

            fadeAnimation.Completed += (s, _) =>
            {
                ViewModel.SokuModSettingGroups.Remove(selectedMember);
                listViewItem.RenderTransform = new ScaleTransform(1, 1);
                listViewItem.Opacity = 1;
                ForceSokuModSettingGroupsListViewRefresh();
                ViewModel.Saveable = true;
            };

            listViewItem.RenderTransform = new ScaleTransform(1, 1);
            listViewItem.RenderTransformOrigin = new Point(.5, .5);

            listViewItem.BeginAnimation(OpacityProperty, fadeAnimation);
            listViewItem.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, moveAnimation, HandoffBehavior.Compose);
            listViewItem.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, moveAnimation, HandoffBehavior.Compose);
        }

        private void AddModSettingGroupButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SokuModSettingGroups.Add(new ModSettingGroupViewModel
            {
                Name = "Untitle",
                Desc = "Please enter description here...",
                Cover = "%resources%/gearbackground.png"
            });

            ForceSokuModSettingGroupsListViewRefresh();
            ViewModel.Saveable = true;
        }

        private void ForceSokuModSettingGroupsListViewRefresh()
        {
            ObservableCollection<ModSettingGroupViewModel> tempCollection = new ObservableCollection<ModSettingGroupViewModel>(ViewModel.SokuModSettingGroups);
            SokuModSettingGroupsListView.ItemsSource = tempCollection;
            SokuModSettingGroupsListView.ItemsSource = ViewModel.SokuModSettingGroups;
        }

        private void PickColorButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Color color;
            switch (button.Tag)
            {
                case "NameColor":
                    color = (Color)ColorConverter.ConvertFromString(ViewModel.SelectedSokuModSettingGroup.NameColor);
                    break;
                case "DescColor":
                    color = (Color)ColorConverter.ConvertFromString(ViewModel.SelectedSokuModSettingGroup.DescColor);
                    break;
                case "CoverOverlayColor":
                    color = (Color)ColorConverter.ConvertFromString(ViewModel.SelectedSokuModSettingGroup.CoverOverlayColor);
                    break;
                case "HoverColor":
                    color = (Color)ColorConverter.ConvertFromString(ViewModel.SelectedSokuModSettingGroup.HoverColor);
                    break;
                default:
                    return;
            }


            var dialog = new ColorPickerDialog(color);
            dialog.Owner = this;

            var res = dialog.ShowDialog();

            if (res == true)
            {
                string hexString = "#" + dialog.Color.A.ToString("X2") + dialog.Color.R.ToString("X2") + dialog.Color.G.ToString("X2") + dialog.Color.B.ToString("X2");

                switch (button.Tag)
                {
                    case "NameColor":
                        ViewModel.SelectedSokuModSettingGroup.NameColor = hexString;
                        break;
                    case "DescColor":
                        ViewModel.SelectedSokuModSettingGroup.DescColor = hexString;
                        break;
                    case "CoverOverlayColor":
                        ViewModel.SelectedSokuModSettingGroup.CoverOverlayColor = hexString;
                        break;
                    case "HoverColor":
                        ViewModel.SelectedSokuModSettingGroup.HoverColor = hexString;
                        break;
                }
                ViewModel.Saveable = true;
            }
        }

        private void SelectCoverBackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Image files (*.png,*.jpg,*.jpeg,*.gif,*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|Video files (*.mp4,*.avi,*.wmv)|*.mp4;*.avi;*.wmv";
            openFileDialog.InitialDirectory = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));
            openFileDialog.FileName = ViewModel.SokuFileName;

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string selectedFileName = openFileDialog.FileName;
                string relativePath = Static.GetRelativePath(openFileDialog.FileName, Static.SelfFileDir);
                if (!relativePath.StartsWith("../../"))
                {
                    selectedFileName = relativePath;
                }

                if (ViewModel.SelectedSokuModSettingGroup.Cover != selectedFileName)
                {
                    ViewModel.SelectedSokuModSettingGroup.Cover = selectedFileName;
                    ViewModel.Saveable = true;
                }
            }
        }

        private void TextBoxSetSaveableTrue_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            string oldValue = "";
            string newValue = "";

            foreach (var change in e.Changes)
            {
                int offset = change.Offset;
                int length = change.AddedLength;
                oldValue = textBox.Text.Substring(offset, length);
                newValue = textBox.Text;
                if (oldValue != newValue)
                {
                    ViewModel.Saveable = true;
                }
            }
        }

        private void ModSettingGroupSelectModsButton_Click(object sender, RoutedEventArgs e)
        {
            ModSettingGroupEditWindowViewModel msgewvm = new ModSettingGroupEditWindowViewModel();
            msgewvm.ModSettingInfoList = ViewModel.ModsManager.ModInfoList
                    .Select(x => new ModSettingInfoModel
                    {
                        Name = x.Name,
                        FullPath = x.FullPath,
                        Enabled = "null"
                    })
                    .ToList();

            if (ViewModel.SelectedSokuModSettingGroup.EnableMods?.Count > 0)
            {
                foreach (var modName in ViewModel.SelectedSokuModSettingGroup.EnableMods)
                {
                    var modInfo = msgewvm.ModSettingInfoList.FirstOrDefault(x => x.Name.ToLower() == modName.ToLower());
                    if (modInfo != null)
                    {
                        modInfo.Enabled = "true";
                    }
                    else
                    {
                        msgewvm.ModSettingInfoList.Add(new ModSettingInfoModel { Name = modName, Enabled = "true" });
                    }
                }
            }

            if (ViewModel.SelectedSokuModSettingGroup.DisableMods?.Count > 0)
            {
                foreach (var modName in ViewModel.SelectedSokuModSettingGroup.DisableMods)
                {
                    var modInfo = msgewvm.ModSettingInfoList.FirstOrDefault(x => x.Name.ToLower() == modName.ToLower());
                    if (modInfo != null)
                    {
                        modInfo.Enabled = "false";
                    }
                    else
                    {
                        msgewvm.ModSettingInfoList.Add(new ModSettingInfoModel { Name = modName, Enabled = "false" });
                    }
                }
            }

            ModSettingGroupEditWindow modSettingGroupEditWindow = new ModSettingGroupEditWindow(msgewvm);
            modSettingGroupEditWindow.ShowDialog();

            if (modSettingGroupEditWindow.DialogResult == true)
            {
                ViewModel.SelectedSokuModSettingGroup.EnableMods = msgewvm.EnableMods;
                ViewModel.SelectedSokuModSettingGroup.DisableMods = msgewvm.DisableMods;
                ViewModel.Saveable = true;
            }
        }

        private bool IsCheckingForUpdates = false;

        private async void CheckForUpdatesButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsCheckingForUpdates)
            {
                IsCheckingForUpdates = true;

                ConfigUtil configUtil = new ConfigUtil
                {
                    Config = new ConfigModel
                    {
                        SokuDirPath = ViewModel.SokuDirPath,
                        SokuFileName = ViewModel.SokuFileName,
                        SokuModSettingGroups = ViewModel.SokuModSettingGroups.ToList(),
                        SokuModAlias = ViewModel.SokuModAlias.ToList(),
                        AutoCheckForUpdates = ViewModel.AutoCheckForUpdates,
                        AutoCheckForInstallable = ViewModel.AutoCheckForInstallable,
                        VersionInfoUrl = ViewModel.VersionInfoUrl,
                        Language = ViewModel.Language
                    }
                };
                UpdatesManager updatesManager = new UpdatesManager(configUtil, ViewModel.ModsManager);
                Action<string> changeButtonStatus = (status) => ViewModel.CheckForUpdatesButtonText = status;
                updatesManager.StatusChanged += changeButtonStatus;
                await updatesManager.GetVersionInfoJson();
                updatesManager.StatusChanged -= changeButtonStatus;
                ViewModel.CheckForUpdatesButtonText = "Check for updates now";
                updatesManager.CheckForUpdates(false);
                IsCheckingForUpdates = false;
            }
        }
    }
}
