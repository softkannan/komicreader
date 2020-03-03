using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace ComicViewer
{
    public class ApplicationCommandsDefinition
    {
        static RoutedUICommand effectNone;

        public static RoutedUICommand EffectNone
        {
            get { return ApplicationCommandsDefinition.effectNone; }
        }
        static RoutedUICommand effectGrey;

        public static RoutedUICommand EffectGrey
        {
            get { return ApplicationCommandsDefinition.effectGrey; }
        }
        static RoutedUICommand effectBW;

        public static RoutedUICommand EffectBW
        {
            get { return ApplicationCommandsDefinition.effectBW; }
        }
        static RoutedUICommand openFolder;

        public static RoutedUICommand OpenFolder
        {
            get { return ApplicationCommandsDefinition.openFolder; }
        }

        static RoutedUICommand minimise;

        public static RoutedUICommand Minimise
        {
            get { return ApplicationCommandsDefinition.minimise; }
        }

        static RoutedUICommand maximise;

        public static RoutedUICommand Maximise
        {
            get { return ApplicationCommandsDefinition.maximise; }
        }

        static RoutedUICommand exit;

        public static RoutedUICommand Exit
        {
            get { return exit; }
        }
        static RoutedUICommand previousPage;

        public static RoutedUICommand PreviousPage
        {
            get { return previousPage; }
        }
        static RoutedUICommand nextPage;

        public static RoutedUICommand NextPage
        {
            get { return nextPage; }
        }
        static RoutedUICommand firstPage;

        public static RoutedUICommand FirstPage
        {
            get { return ApplicationCommandsDefinition.firstPage; }
        }
        static RoutedUICommand lastPage;

        public static RoutedUICommand LastPage
        {
            get { return ApplicationCommandsDefinition.lastPage; }
        }
        static RoutedUICommand gotoPage;

        public static RoutedUICommand GotoPage
        {
            get { return ApplicationCommandsDefinition.gotoPage; }
        }

        static RoutedUICommand zoomFitWidth;

        public static RoutedUICommand ZoomFitWidth
        {
            get { return ApplicationCommandsDefinition.zoomFitWidth; }
        }
        static RoutedUICommand zoomOriginal;

        public static RoutedUICommand ZoomOriginal
        {
            get { return ApplicationCommandsDefinition.zoomOriginal; }
        }
        static RoutedUICommand zoomFit;

        public static RoutedUICommand ZoomFit
        {
            get { return ApplicationCommandsDefinition.zoomFit; }
        }
        static RoutedUICommand pageBufferLength;

        public static RoutedUICommand PageBufferLength
        {
            get { return ApplicationCommandsDefinition.pageBufferLength; }
        }
        static RoutedUICommand rotateNormal;

        public static RoutedUICommand RotateNormal
        {
            get { return ApplicationCommandsDefinition.rotateNormal; }
        }
        static RoutedUICommand rotate90;

        public static RoutedUICommand Rotate90
        {
            get { return ApplicationCommandsDefinition.rotate90; }
        }
        static RoutedUICommand rotate180;

        public static RoutedUICommand Rotate180
        {
            get { return ApplicationCommandsDefinition.rotate180; }
        }
        static RoutedUICommand rotate90CounterClock;

        public static RoutedUICommand Rotate90CounterClock
        {
            get { return ApplicationCommandsDefinition.rotate90CounterClock; }
        }

        static RoutedUICommand directionLeftToRight;

        public static RoutedUICommand DirectionLeftToRight
        {
            get { return ApplicationCommandsDefinition.directionLeftToRight; }
        }
        static RoutedUICommand directionRightToLeft;

        public static RoutedUICommand DirectionRightToLeft
        {
            get { return ApplicationCommandsDefinition.directionRightToLeft; }
        }

        static RoutedUICommand modeSinglePage;

        public static RoutedUICommand ModeSinglePage
        {
            get { return ApplicationCommandsDefinition.modeSinglePage; }
        }

        static RoutedUICommand modeContinuousPage;

        public static RoutedUICommand ModeContinuousPage
        {
            get { return ApplicationCommandsDefinition.modeContinuousPage; }
        }

        static RoutedUICommand modeDoublePage;

        public static RoutedUICommand ModeDoublePage
        {
            get { return ApplicationCommandsDefinition.modeDoublePage; }
        }

        static RoutedUICommand modeDoubleContinuousPage;

        public static RoutedUICommand ModeDoubleContinuousPage
        {
            get { return ApplicationCommandsDefinition.modeDoubleContinuousPage; }
        }

        static RoutedUICommand fullscreen;

        public static RoutedUICommand Fullscreen
        {
            get { return ApplicationCommandsDefinition.fullscreen; }
        }

        static RoutedUICommand about;

        public static RoutedUICommand About
        {
            get { return ApplicationCommandsDefinition.about; }
        }

        static RoutedUICommand thumbnailViewer;

        public static RoutedUICommand ThumbnailViewer
        {
            get { return ApplicationCommandsDefinition.thumbnailViewer; }
        }
        static RoutedUICommand toolbar;

        public static RoutedUICommand Toolbar
        {
            get { return ApplicationCommandsDefinition.toolbar; }
        }


        static ApplicationCommandsDefinition()
        {
            InputGestureCollection inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.Q, ModifierKeys.Control, "Ctrl+Q"));
            exit = new RoutedUICommand("Exit", "Exit", typeof(ApplicationCommandsDefinition), inputs);

            openFolder = new RoutedUICommand("OpenFolder", "OpenFolder", typeof(ApplicationCommandsDefinition));

            maximise = new RoutedUICommand("Maximise", "Maximise", typeof(ApplicationCommandsDefinition));

            minimise = new RoutedUICommand("Minimise", "Minimise", typeof(ApplicationCommandsDefinition));

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.PageUp, ModifierKeys.None, "PageUp"));
            previousPage = new RoutedUICommand("Previous Page", "PreviousPage", typeof(ApplicationCommandsDefinition), inputs);
            
            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.PageDown, ModifierKeys.None, "PageDown"));
            nextPage = new RoutedUICommand("Next Page", "NextPage", typeof(ApplicationCommandsDefinition), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.Home, ModifierKeys.None, "Home"));
            firstPage = new RoutedUICommand("First Page", "FirstPage", typeof(ApplicationCommandsDefinition), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.End, ModifierKeys.None, "End"));
            lastPage = new RoutedUICommand("Last Page", "LastPage", typeof(ApplicationCommandsDefinition), inputs);

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.G, ModifierKeys.Control, "Ctrl+G"));
            gotoPage = new RoutedUICommand("Goto Page", "GotoPage", typeof(ApplicationCommandsDefinition), inputs);
            
            zoomFit = new RoutedUICommand("Fit", "ZoomFit", typeof(ApplicationCommandsDefinition));
            zoomOriginal = new RoutedUICommand("Original", "ZoomOriginal", typeof(ApplicationCommandsDefinition));
            zoomFitWidth = new RoutedUICommand("FitWidth", "ZoomFitWidth", typeof(ApplicationCommandsDefinition));
            pageBufferLength = new RoutedUICommand("Page Buffer Length", "PageBufferLength", typeof(ApplicationCommandsDefinition));

            rotateNormal = new RoutedUICommand("Normal","RotateNormal",typeof(ApplicationCommandsDefinition));
            rotate180 = new RoutedUICommand("180", "Rotate180", typeof(ApplicationCommandsDefinition));
            rotate90 = new RoutedUICommand("90", "Rotate90", typeof(ApplicationCommandsDefinition));
            rotate90CounterClock = new RoutedUICommand("90 Counter Clock Wise", "Rotate90CounterClock", typeof(ApplicationCommandsDefinition));

            directionLeftToRight = new RoutedUICommand("Left To Right", "DirectionLeftToRight", typeof(ApplicationCommandsDefinition));
            directionRightToLeft = new RoutedUICommand("Right to Left", "DirectionRightToLeft", typeof(ApplicationCommandsDefinition));

            modeDoublePage = new RoutedUICommand("Double Page", "DoublePage", typeof(ApplicationCommandsDefinition));
            modeSinglePage = new RoutedUICommand("Single Page", "SinglePage", typeof(ApplicationCommandsDefinition));
            modeContinuousPage = new RoutedUICommand("Continuous Page", "ContinuousPage", typeof(ApplicationCommandsDefinition));
            modeDoubleContinuousPage = new RoutedUICommand("DoubleContinuous Page", "DoubleContinuousPage", typeof(ApplicationCommandsDefinition));

            effectNone = new RoutedUICommand("None", "EffectNone", typeof(ApplicationCommandsDefinition));
            effectGrey = new RoutedUICommand("Grey", "EffectGrey", typeof(ApplicationCommandsDefinition));
            effectBW = new RoutedUICommand("BW", "EffectBW", typeof(ApplicationCommandsDefinition));

            inputs = new InputGestureCollection();
            inputs.Add(new KeyGesture(Key.Enter, ModifierKeys.Alt, "Alt+Enter"));
            fullscreen = new RoutedUICommand("Full Screen", "FullScreen", typeof(ApplicationCommandsDefinition), inputs);

            about = new RoutedUICommand("About", "About", typeof(ApplicationCommandsDefinition));

            thumbnailViewer = new RoutedUICommand("Thumbnail Viewer", "ThumbnailViewer", typeof(ApplicationCommandsDefinition));
            toolbar = new RoutedUICommand("Toolbar", "Toolbar", typeof(ApplicationCommandsDefinition));
            
        }

       
        
    }
}
