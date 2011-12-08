﻿#region CodeMaid is Copyright 2007-2011 Steve Cadwallader.

// CodeMaid is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License version 3
// as published by the Free Software Foundation.
//
// CodeMaid is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details <http://www.gnu.org/licenses/>.

#endregion CodeMaid is Copyright 2007-2011 Steve Cadwallader.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SteveCadwallader.CodeMaid.CodeItems;
using SteveCadwallader.CodeMaid.Helpers;

namespace SteveCadwallader.CodeMaid.Spade
{
    /// <summary>
    /// The WPF based control/view for the <see cref="SpadeToolWindow"/>.
    /// </summary>
    public partial class SpadeView
    {
        #region Fields

        private Point _startPoint;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SpadeView"/> class.
        /// </summary>
        public SpadeView()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the view model.
        /// </summary>
        private SpadeViewModel ViewModel
        {
            get { return DataContext as SpadeViewModel; }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Called when a KeyDown event is raised by a TreeViewItem (not automatically handled by TreeView).
        /// Used to jump to a code item upon enter, or toggle the expansion state upon space.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.KeyEventArgs"/> instance containing the event data.</param>
        private void OnTreeViewItemKeyDown(object sender, KeyEventArgs e)
        {
            var treeViewItem = e.Source as TreeViewItem;
            if (treeViewItem == null) return;

            switch (e.Key)
            {
                case Key.Return:
                    JumpToCodeItem(treeViewItem.DataContext as BaseCodeItem);
                    break;

                case Key.Space:
                    treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                    break;
            }
        }

        /// <summary>
        /// Called when the header of a TreeViewItem receives a mouse down event.
        /// Used to jump to a code item, start detecting a drag and drop operation, or toggle the expansion state depending on conditions.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        private void OnTreeViewItemHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            var source = e.Source as DependencyObject;
            if (source == null) return;

            var treeViewItem = source.FindVisualAncestor<TreeViewItem>();
            if (treeViewItem == null) return;

            switch (e.ChangedButton)
            {
                case MouseButton.Left:
                    if (ViewModel.InteractionMode == SpadeInteractionMode.Reorder)
                    {
                        _startPoint = e.GetPosition(null);
                    }
                    else
                    {
                        JumpToCodeItem(treeViewItem.DataContext as BaseCodeItem);
                    }
                    break;

                case MouseButton.Middle:
                    treeViewItem.IsExpanded = !treeViewItem.IsExpanded;
                    break;
            }
        }

        /// <summary>
        /// Called when the header of a TreeViewItem receives a mouse move event.
        /// Used to conditionally initiate a drag and drop operation.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void OnTreeViewItemHeaderMouseMove(object sender, MouseEventArgs e)
        {
            if (ViewModel.InteractionMode != SpadeInteractionMode.Reorder ||
                e.LeftButton != MouseButtonState.Pressed) return;

            var delta = _startPoint - e.GetPosition(null);
            if (Math.Abs(delta.X) <= SystemParameters.MinimumHorizontalDragDistance &&
                Math.Abs(delta.Y) <= SystemParameters.MinimumVerticalDragDistance) return;

            var source = sender as DependencyObject;
            if (source == null) return;

            var treeViewItem = source.FindVisualAncestor<TreeViewItem>();
            if (treeViewItem == null) return;

            var codeItem = treeViewItem.DataContext as BaseCodeItemElement;
            if (codeItem == null) return;

            DragDrop.DoDragDrop(treeViewItem, new DataObject(typeof(BaseCodeItemElement), codeItem), DragDropEffects.Move);
        }

        /// <summary>
        /// Called when the header of a TreeViewItem receives a drop event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.DragEventArgs"/> instance containing the event data.</param>
        private void OnTreeViewItemHeaderDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(BaseCodeItemElement))) return;

            var source = sender as DependencyObject;
            if (source == null) return;

            var treeViewItem = source.FindVisualAncestor<TreeViewItem>();
            if (treeViewItem == null || e.Source == treeViewItem) return;

            var baseCodeItem = treeViewItem.DataContext as BaseCodeItemElement;
            if (baseCodeItem == null) return;

            var codeItemToMove = e.Data.GetData(typeof(BaseCodeItemElement)) as BaseCodeItemElement;
            if (codeItemToMove == null) return;

            CodeReorderHelper.MoveItemAboveBase(codeItemToMove, baseCodeItem);
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Jumps to the specified code item.
        /// </summary>
        /// <param name="codeItem">The code item.</param>
        private void JumpToCodeItem(BaseCodeItem codeItem)
        {
            var viewModel = ViewModel;
            if (codeItem == null || viewModel == null || codeItem.StartLine <= 0) return;

            Dispatcher.BeginInvoke(
                new Action(() => TextDocumentHelper.MoveToCodeItem(viewModel.Document, codeItem, viewModel.Package.Options.Spade.CenterOnWhole)));
        }

        #endregion Methods
    }
}