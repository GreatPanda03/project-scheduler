using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace ProjectScheduler.Controls
{
    public sealed partial class PlaceholderView : UserControl
    {
        public PlaceholderView()
        {
            InitializeComponent();
        }

        // Symbol Icon
        public string IconGlyph
        {
            get => (string)GetValue(IconGlyphProperty);
            set => SetValue(IconGlyphProperty, value);
        }

        public static readonly DependencyProperty IconGlyphProperty =
            DependencyProperty.Register(
                nameof(IconGlyphProperty),
                typeof(string),
                typeof(PlaceholderView),
                new PropertyMetadata(null));

        // Header
        public string Header
        {
            get => (string)GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register(
                nameof(Header),
                typeof(string),
                typeof(PlaceholderView),
                new PropertyMetadata(null));

        // Body
        public string Body
        {
            get => (string)GetValue(BodyProperty);
            set => SetValue(BodyProperty, value);
        }

        public static readonly DependencyProperty BodyProperty =
            DependencyProperty.Register(
                nameof(Body),
                typeof(string),
                typeof(PlaceholderView),
                new PropertyMetadata(null));
    }
}
