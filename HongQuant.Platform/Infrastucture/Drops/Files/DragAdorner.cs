using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace HongQuant.Platform.Infrastucture.Drops.Files
{
    /// <summary>
    /// Taken shamelessly from https://github.com/punker76/gong-wpf-dragdrop
    /// 
    /// Thanks
    /// </summary>
    public class DragAdorner : Adorner
    {

        private readonly AdornerLayer _adornerLayer;
        private readonly UIElement _adornment;
        private Point _mousePositon;

        public DragAdorner(UIElement adornedElement,
            UIElement adornment)
            : base(adornedElement)
        {
            _adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            _adornerLayer.Add(this);
            _adornment = adornment;
            IsHitTestVisible = false;
        }

        public Point MousePosition
        {
            get => _mousePositon;
            set
            {
                if (_mousePositon == value) return;
                _mousePositon = value;
                _adornerLayer.Update(AdornedElement);
            }
        }

        public void Detatch()
        {
            _adornerLayer.Remove(this);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _adornment.Arrange(new Rect(finalSize));
            return finalSize;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var result = new GeneralTransformGroup();
            result.Children.Add(base.GetDesiredTransform(transform));
            result.Children.Add(new TranslateTransform(MousePosition.X - 4, MousePosition.Y - 4));

            return result;
        }

        protected override Visual GetVisualChild(int index)
        {
            return _adornment;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _adornment.Measure(constraint);
            return _adornment.DesiredSize;
        }

        protected override int VisualChildrenCount => 1;

    }
}
