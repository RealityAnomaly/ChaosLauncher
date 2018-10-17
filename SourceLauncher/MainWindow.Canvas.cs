using SourceLauncher.Controls;
using SourceLauncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SourceLauncher
{
    public partial class MainWindow
    {
        private bool _isDragging;
        private bool _isTargeting;
        private Point _dragPoint;
        private UIElement _targetSource;
        private ToolControl _lastSelected;

        private readonly IList<Action> _undoList = new List<Action>();
        private readonly IList<Action> _redoList = new List<Action>();

        private void Undo()
        {
            var undoAction = _undoList.First();
            _redoList.Add(undoAction);
            _undoList.Remove(undoAction);
        }

        private void Redo()
        {
            var lastAction = _redoList.First();
            _redoList.Remove(lastAction);
            _undoList.Add(lastAction);
        }

        private void NewCmdletWidget()
        {
            var newTool = CmdletTool.PickTool(ChaosShell);
            if (newTool == null)
                return;

            AddToolWidget(newTool);
        }

        private void NewScriptWidget()
        {
            /**
            ScriptTool scriptTool = ScriptTool.PickTool();
            if (scriptTool == null)
                return;

            AddToolWidget(scriptTool);*/
        }

        private void NewExternalWidget()
        {
            var newTool = ExternalTool.PickTool();
            if (newTool == null)
                return;

            AddToolWidget(newTool);
        }

        private void AddToolWidget(Tool tool)
        {
            var toolControl = new ToolControl(ChaosShell, tool);
            AddWidgetToCanvas(toolControl);
        }

        private void StopTargeting()
        {
            _isTargeting = false;
            _targetSource = null;

            _lastSelected?.SetSelected(false);
            _lastSelected = null;

            Mouse.OverrideCursor = null;
        }

        private void AddWidgetToCanvas(UIElement widget)
        {
            widget.MouseLeftButtonDown += Widget_MouseLeftButtonDown;
            widget.MouseMove += Widget_MouseMove;
            widget.MouseLeftButtonUp += Widget_MouseLeftButtonUp;
            widget.MouseEnter += Widget_MouseOverChanged;
            widget.MouseLeave += Widget_MouseOverChanged;

            Canvas.SetLeft(widget, 0);
            Canvas.SetTop(widget, 0);

            Canvas.Children.Add(widget);
        }

        private void Widget_MouseOverChanged(object sender, MouseEventArgs e)
        {
            if (sender.GetType() != typeof(ToolControl))
                return;

            if (!_isTargeting || sender.Equals(_targetSource)) return;

            var element = sender as ToolControl;

            if (element != null && element.IsMouseOver)
            {
                element.SetSelected(true);
                _lastSelected = element;
            }
            else
            {
                element?.SetSelected(false);
            }
        }

        private void Widget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() != typeof(ToolControl))
                return;

            _isDragging = false;
            var element = sender as ToolControl;
            element?.ReleaseMouseCapture();
            element?.UpdateConnections();

            IList<ToolControl> toUpdate = new List<ToolControl>();
            foreach (UIElement obj in Canvas.Children)
            {
                if (obj is ToolControl tool)
                    toUpdate.Add(tool);
            }

            foreach (var control in toUpdate)
            {
                control.UpdateConnections();
            }
        }

        private void Widget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() != typeof(ToolControl))
                return;

            if (_isTargeting && !sender.Equals(_targetSource))
            {
                StopTargeting();
            }

            if (!_isTargeting && Keyboard.IsKeyDown(Key.LeftShift))
            {
                _targetSource = sender as UIElement;
                Mouse.OverrideCursor = Cursors.Cross;
                _isTargeting = true;
                return;
            }

            _isDragging = true;
            var element = sender as ToolControl;
            _dragPoint = e.GetPosition(element);
            element?.CaptureMouse();
        }

        private void Widget_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDragging || !(sender is UIElement element)) return;

            var newPoint = e.GetPosition(Canvas);

            if (!(element.RenderTransform is TranslateTransform transform))
            {
                transform = new TranslateTransform();
                element.RenderTransform = transform;
            }

            transform.X = newPoint.X - _dragPoint.X;
            transform.Y = newPoint.Y - _dragPoint.Y;
        }
    }
}
