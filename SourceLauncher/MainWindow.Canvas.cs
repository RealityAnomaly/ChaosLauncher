using SourceLauncher.Controls;
using SourceLauncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SourceLauncher
{
    public partial class MainWindow : RibbonWindow
    {
        private bool isDragging = false;
        private bool isTargeting = false;
        private Point dragPoint;
        private UIElement targetSource;
        private ToolControl lastSelected;

        private List<Action> undoList = new List<Action>();
        private List<Action> redoList = new List<Action>();

        private void Undo()
        {
            Action undoAction = undoList.First();
            redoList.Add(undoAction);
            undoList.Remove(undoAction);
        }

        private void Redo()
        {
            Action lastAction = redoList.First();
            redoList.Remove(lastAction);
            undoList.Add(lastAction);
        }

        private void NewCmdletWidget()
        {
            CmdletTool newTool = CmdletTool.PickTool(chaosShell);
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
            ExternalTool newTool = ExternalTool.PickTool();
            if (newTool == null)
                return;

            AddToolWidget(newTool);
        }

        private void AddToolWidget(Tool tool)
        {
            ToolControl toolControl = new ToolControl(chaosShell, tool);
            AddWidgetToCanvas(toolControl);
        }

        private void StopTargeting()
        {
            isTargeting = false;
            targetSource = null;

            if (lastSelected != null)
                lastSelected.SetSelected(false);

            lastSelected = null;

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

            canvas.Children.Add(widget);
        }

        private void Widget_MouseOverChanged(object sender, MouseEventArgs e)
        {
            if (sender.GetType() != typeof(ToolControl))
                return;

            if (isTargeting && sender != targetSource)
            {
                ToolControl element = sender as ToolControl;

                if (element.IsMouseOver)
                {
                    element.SetSelected(true);
                    lastSelected = element;
                }
                else
                {
                    element.SetSelected(false);
                }
            }
        }

        private void Widget_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() != typeof(ToolControl))
                return;

            isDragging = false;
            ToolControl element = sender as ToolControl;
            element.ReleaseMouseCapture();

            element.UpdateConnections();

            IList<ToolControl> toUpdate = new List<ToolControl>();
            foreach (UIElement obj in canvas.Children)
            {
                if (obj is ToolControl tool)
                    toUpdate.Add(tool);
            }

            foreach (ToolControl control in toUpdate)
            {
                control.UpdateConnections();
            }
        }

        private void Widget_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender.GetType() != typeof(ToolControl))
                return;

            if (isTargeting && sender != targetSource)
            {
                StopTargeting();
            }

            if (!isTargeting && Keyboard.IsKeyDown(Key.LeftShift))
            {
                targetSource = sender as UIElement;
                Mouse.OverrideCursor = Cursors.Cross;
                isTargeting = true;
                return;
            }

            isDragging = true;
            ToolControl element = sender as ToolControl;
            dragPoint = e.GetPosition(element);
            element.CaptureMouse();
        }

        private void Widget_MouseMove(object sender, MouseEventArgs e)
        {
            UIElement element = sender as UIElement;

            if (isDragging && element != null)
            {
                Point newPoint = e.GetPosition(canvas);

                TranslateTransform transform = element.RenderTransform as TranslateTransform;
                if (transform == null)
                {
                    transform = new TranslateTransform();
                    element.RenderTransform = transform;
                }

                transform.X = newPoint.X - dragPoint.X;
                transform.Y = newPoint.Y - dragPoint.Y;
            }
        }
    }
}
